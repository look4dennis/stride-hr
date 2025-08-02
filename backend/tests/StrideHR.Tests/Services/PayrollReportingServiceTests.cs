using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Payroll;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class PayrollReportingServiceTests
{
    private readonly Mock<IPayrollRepository> _mockPayrollRepository;
    private readonly Mock<ICurrencyService> _mockCurrencyService;
    private readonly Mock<IPayrollAuditTrailRepository> _mockAuditTrailRepository;
    private readonly Mock<ILogger<PayrollReportingService>> _mockLogger;
    private readonly PayrollReportingService _service;

    public PayrollReportingServiceTests()
    {
        _mockPayrollRepository = new Mock<IPayrollRepository>();
        _mockCurrencyService = new Mock<ICurrencyService>();
        _mockAuditTrailRepository = new Mock<IPayrollAuditTrailRepository>();
        _mockLogger = new Mock<ILogger<PayrollReportingService>>();

        _service = new PayrollReportingService(
            _mockPayrollRepository.Object,
            _mockCurrencyService.Object,
            _mockAuditTrailRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GeneratePayrollReportAsync_WithValidRequest_ReturnsReport()
    {
        // Arrange
        var request = new PayrollReportRequest
        {
            BranchId = 1,
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 1, 31),
            ReportType = PayrollReportType.Summary,
            Currency = "USD"
        };

        var payrollRecords = new List<PayrollRecord>
        {
            new PayrollRecord
            {
                Id = 1,
                EmployeeId = 1,
                BasicSalary = 5000,
                GrossSalary = 6000,
                NetSalary = 4500,
                TotalAllowances = 1000,
                TotalDeductions = 1500,
                Currency = "USD",
                Employee = new Employee { FirstName = "John", LastName = "Doe" }
            },
            new PayrollRecord
            {
                Id = 2,
                EmployeeId = 2,
                BasicSalary = 4000,
                GrossSalary = 5000,
                NetSalary = 3800,
                TotalAllowances = 1000,
                TotalDeductions = 1200,
                Currency = "USD",
                Employee = new Employee { FirstName = "Jane", LastName = "Smith" }
            }
        };

        _mockPayrollRepository.Setup(x => x.GetByBranchAndPeriodAsync(1, 2024, 1))
            .ReturnsAsync(payrollRecords);

        // Act
        var result = await _service.GeneratePayrollReportAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PayrollReportType.Summary, result.ReportType);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(2, result.Summary.TotalEmployees);
        Assert.Equal(11000, result.Summary.TotalGrossSalary);
        Assert.Equal(8300, result.Summary.TotalNetSalary);
        Assert.Equal(2000, result.Summary.TotalAllowances);
        Assert.Equal(2700, result.Summary.TotalDeductions);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GeneratePayrollReportAsync_WithCurrencyConversion_AppliesConversion()
    {
        // Arrange
        var request = new PayrollReportRequest
        {
            BranchId = 1,
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 1, 31),
            ReportType = PayrollReportType.Summary,
            Currency = "EUR",
            IncludeCurrencyConversion = true
        };

        var payrollRecords = new List<PayrollRecord>
        {
            new PayrollRecord
            {
                Id = 1,
                EmployeeId = 1,
                BasicSalary = 5000,
                GrossSalary = 6000,
                NetSalary = 4500,
                Currency = "USD",
                Employee = new Employee { FirstName = "John", LastName = "Doe" }
            }
        };

        _mockPayrollRepository.Setup(x => x.GetByBranchAndPeriodAsync(1, 2024, 1))
            .ReturnsAsync(payrollRecords);

        _mockCurrencyService.Setup(x => x.GetExchangeRateAsync("USD", "EUR"))
            .ReturnsAsync(0.85m);

        // Act
        var result = await _service.GeneratePayrollReportAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("EUR", result.Currency);
        _mockCurrencyService.Verify(x => x.GetExchangeRateAsync("USD", "EUR"), Times.Once);
    }

    [Fact]
    public async Task GenerateComplianceReportAsync_WithValidRequest_ReturnsComplianceReport()
    {
        // Arrange
        var request = new ComplianceReportRequest
        {
            BranchId = 1,
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 1, 31),
            ReportType = ComplianceReportType.TaxDeduction,
            Country = "US"
        };

        var payrollRecords = new List<PayrollRecord>
        {
            new PayrollRecord
            {
                Id = 1,
                EmployeeId = 1,
                GrossSalary = 6000,
                TaxDeduction = 1200,
                ProvidentFund = 600,
                EmployeeStateInsurance = 180,
                ProfessionalTax = 200,
                Employee = new Employee { FirstName = "John", LastName = "Doe" }
            }
        };

        _mockPayrollRepository.Setup(x => x.GetByBranchAndPeriodAsync(1, 2024, 1))
            .ReturnsAsync(payrollRecords);

        // Act
        var result = await _service.GenerateComplianceReportAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ComplianceReportType.TaxDeduction, result.ReportType);
        Assert.Equal("US", result.Country);
        Assert.Equal(1, result.Summary.TotalEmployees);
        Assert.Equal(1200, result.Summary.TotalTaxDeducted);
        Assert.Equal(600, result.Summary.TotalProvidentFund);
        Assert.Equal(180, result.Summary.TotalESI);
        Assert.Equal(200, result.Summary.TotalProfessionalTax);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GenerateAnalyticsReportAsync_WithValidRequest_ReturnsAnalyticsReport()
    {
        // Arrange
        var request = new PayrollAnalyticsRequest
        {
            BranchId = 1,
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 3, 31),
            AnalyticsType = PayrollAnalyticsType.CostAnalysis,
            Currency = "USD"
        };

        // Act
        var result = await _service.GenerateAnalyticsReportAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PayrollAnalyticsType.CostAnalysis, result.AnalyticsType);
        Assert.Equal("USD", result.Currency);
        Assert.NotNull(result.Summary);
        Assert.NotNull(result.Metrics);
        Assert.NotNull(result.TrendData);
    }

    [Fact]
    public async Task GenerateBudgetVarianceReportAsync_WithValidRequest_ReturnsBudgetVarianceReport()
    {
        // Arrange
        var request = new BudgetVarianceRequest
        {
            BranchId = 1,
            BudgetYear = 2024,
            BudgetMonth = 1,
            Currency = "USD",
            VarianceType = BudgetVarianceType.Monthly
        };

        // Act
        var result = await _service.GenerateBudgetVarianceReportAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2024, result.BudgetYear);
        Assert.Equal(1, result.BudgetMonth);
        Assert.Equal(BudgetVarianceType.Monthly, result.VarianceType);
        Assert.Equal("USD", result.Currency);
        Assert.NotNull(result.Summary);
        Assert.NotNull(result.Items);
        Assert.NotNull(result.Alerts);
    }

    [Fact]
    public async Task GetPayrollAuditTrailAsync_WithValidRequest_ReturnsAuditTrail()
    {
        // Arrange
        var request = new PayrollAuditTrailRequest
        {
            PayrollRecordId = 1,
            PageNumber = 1,
            PageSize = 10
        };

        var auditTrailItems = new List<PayrollAuditTrail>
        {
            new PayrollAuditTrail
            {
                Id = 1,
                PayrollRecordId = 1,
                EmployeeId = 1,
                Action = PayrollAuditAction.Created,
                ActionDescription = "Payroll record created",
                UserId = 1,
                Timestamp = DateTime.UtcNow,
                Employee = new Employee { FirstName = "John", LastName = "Doe" },
                User = new User { Username = "admin" }
            }
        };

        _mockAuditTrailRepository.Setup(x => x.GetPagedAsync(request))
            .ReturnsAsync((auditTrailItems, 1));

        // Act
        var result = await _service.GetPayrollAuditTrailAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(1, result.TotalPages);
        
        var firstItem = result.Items.First();
        Assert.Equal(1, firstItem.PayrollRecordId);
        Assert.Equal(PayrollAuditAction.Created, firstItem.Action);
        Assert.Equal("Payroll record created", firstItem.ActionDescription);
        Assert.Equal("John Doe", firstItem.EmployeeName);
        Assert.Equal("admin", firstItem.UserName);
    }

    [Fact]
    public async Task ValidateComplianceAsync_WithValidParameters_ReturnsViolations()
    {
        // Arrange
        var branchId = 1;
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);

        // Act
        var result = await _service.ValidateComplianceAsync(branchId, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<ComplianceViolation>>(result);
    }

    [Fact]
    public async Task ExportReportAsync_WithValidParameters_ReturnsFileContent()
    {
        // Arrange
        var reportResult = new PayrollReportResult();
        var format = "pdf";

        // Act
        var result = await _service.ExportReportAsync(reportResult, format);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<byte[]>(result);
    }
}