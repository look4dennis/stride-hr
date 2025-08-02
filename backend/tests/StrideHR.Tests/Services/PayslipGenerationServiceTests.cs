using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Payroll;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class PayslipGenerationServiceTests
{
    private readonly Mock<IPayslipGenerationRepository> _mockPayslipGenerationRepository;
    private readonly Mock<IPayslipTemplateService> _mockPayslipTemplateService;
    private readonly Mock<IPayslipDesignerService> _mockPayslipDesignerService;
    private readonly Mock<IPayrollService> _mockPayrollService;
    private readonly Mock<IFileStorageService> _mockFileStorageService;
    private readonly Mock<ILogger<PayslipGenerationService>> _mockLogger;
    private readonly PayslipGenerationService _service;

    public PayslipGenerationServiceTests()
    {
        _mockPayslipGenerationRepository = new Mock<IPayslipGenerationRepository>();
        _mockPayslipTemplateService = new Mock<IPayslipTemplateService>();
        _mockPayslipDesignerService = new Mock<IPayslipDesignerService>();
        _mockPayrollService = new Mock<IPayrollService>();
        _mockFileStorageService = new Mock<IFileStorageService>();
        _mockLogger = new Mock<ILogger<PayslipGenerationService>>();

        _service = new PayslipGenerationService(
            _mockPayslipGenerationRepository.Object,
            _mockPayslipTemplateService.Object,
            _mockPayslipDesignerService.Object,
            _mockPayrollService.Object,
            _mockFileStorageService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GeneratePayslipAsync_ValidRequest_ReturnsPayslipGenerationDto()
    {
        // Arrange
        var request = new CreatePayslipGenerationRequest
        {
            PayrollRecordId = 1,
            PayslipTemplateId = 1,
            AutoSubmitForApproval = true
        };

        var payrollRecord = new PayrollRecord
        {
            Id = 1,
            EmployeeId = 1,
            PayrollMonth = 12,
            PayrollYear = 2024,
            PayrollPeriodStart = new DateTime(2024, 12, 1),
            PayrollPeriodEnd = new DateTime(2024, 12, 31),
            NetSalary = 5000m,
            Currency = "USD"
        };

        var template = new PayslipTemplateDto
        {
            Id = 1,
            Name = "Default Template",
            OrganizationId = 1
        };

        var payrollData = new PayrollCalculationResult
        {
            EmployeeId = 1,
            EmployeeName = "John Doe",
            BasicSalary = 4000m,
            GrossSalary = 5500m,
            NetSalary = 5000m,
            Currency = "USD",
            PayrollMonth = 12,
            PayrollYear = 2024
        };

        var pdfContent = new byte[] { 1, 2, 3, 4, 5 };
        var fileName = "Payslip_John_Doe_202412.pdf";

        _mockPayrollService.Setup(x => x.GetPayrollRecordAsync(1))
            .ReturnsAsync(payrollRecord);

        _mockPayslipGenerationRepository.Setup(x => x.GetByPayrollRecordAsync(1))
            .ReturnsAsync((PayslipGeneration?)null);

        _mockPayslipTemplateService.Setup(x => x.GetTemplateAsync(1))
            .ReturnsAsync(template);

        _mockPayrollService.Setup(x => x.CalculatePayrollAsync(It.IsAny<PayrollCalculationRequest>()))
            .ReturnsAsync(payrollData);

        _mockPayslipDesignerService.Setup(x => x.GeneratePdfPayslipAsync(template, payrollData))
            .ReturnsAsync((pdfContent, fileName));

        _mockFileStorageService.Setup(x => x.SaveFileAsync(pdfContent, fileName, "payslips/2024/12"))
            .ReturnsAsync("/payslips/2024/12/payslip_john_doe_202412.pdf");

        _mockPayslipGenerationRepository.Setup(x => x.AddAsync(It.IsAny<PayslipGeneration>()))
            .Returns(Task.FromResult(new PayslipGeneration()));

        _mockPayslipGenerationRepository.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(true));

        _mockPayslipGenerationRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new PayslipGeneration
            {
                Id = 1,
                PayrollRecordId = 1,
                PayslipTemplateId = 1,
                Status = PayslipStatus.PendingHRApproval,
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = 1,
                Version = 1,
                PayrollRecord = payrollRecord,
                GeneratedByEmployee = new Employee { FirstName = "Admin", LastName = "User" }
            });

        // Act
        var result = await _service.GeneratePayslipAsync(request, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PayrollRecordId);
        Assert.Equal(1, result.PayslipTemplateId);
        Assert.Equal(PayslipStatus.PendingHRApproval, result.Status);
        Assert.Equal(1, result.Version);

        _mockPayslipGenerationRepository.Verify(x => x.AddAsync(It.IsAny<PayslipGeneration>()), Times.Once);
        _mockPayslipGenerationRepository.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GeneratePayslipAsync_PayrollRecordNotFound_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreatePayslipGenerationRequest
        {
            PayrollRecordId = 999,
            PayslipTemplateId = 1
        };

        _mockPayrollService.Setup(x => x.GetPayrollRecordAsync(999))
            .ReturnsAsync((PayrollRecord?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GeneratePayslipAsync(request, 1));

        Assert.Contains("Payroll record with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task GeneratePayslipAsync_PayslipAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreatePayslipGenerationRequest
        {
            PayrollRecordId = 1,
            PayslipTemplateId = 1
        };

        var payrollRecord = new PayrollRecord { Id = 1, EmployeeId = 1 };
        var existingPayslip = new PayslipGeneration { Id = 1, PayrollRecordId = 1 };

        _mockPayrollService.Setup(x => x.GetPayrollRecordAsync(1))
            .ReturnsAsync(payrollRecord);

        _mockPayslipGenerationRepository.Setup(x => x.GetByPayrollRecordAsync(1))
            .ReturnsAsync(existingPayslip);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GeneratePayslipAsync(request, 1));

        Assert.Contains("Payslip already exists for payroll record 1", exception.Message);
    }

    [Fact]
    public async Task ProcessApprovalAsync_HRApproval_UpdatesStatusCorrectly()
    {
        // Arrange
        var request = new PayslipApprovalRequest
        {
            PayslipGenerationId = 1,
            ApprovalLevel = PayslipApprovalLevel.HR,
            Action = PayslipApprovalAction.Approved,
            Notes = "Approved by HR"
        };

        var payslipGeneration = new PayslipGeneration
        {
            Id = 1,
            Status = PayslipStatus.PendingHRApproval,
            ApprovalHistory = new List<PayslipApprovalHistory>()
        };

        _mockPayslipGenerationRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(payslipGeneration);

        _mockPayslipGenerationRepository.Setup(x => x.UpdateAsync(It.IsAny<PayslipGeneration>()))
            .Returns(Task.CompletedTask);

        _mockPayslipGenerationRepository.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(true));

        // Act
        var result = await _service.ProcessApprovalAsync(request, 1);

        // Assert
        Assert.True(result);
        Assert.Equal(PayslipStatus.PendingFinanceApproval, payslipGeneration.Status);
        Assert.Equal(1, payslipGeneration.HRApprovedBy);
        Assert.NotNull(payslipGeneration.HRApprovedAt);
        Assert.Equal("Approved by HR", payslipGeneration.HRApprovalNotes);
        Assert.Single(payslipGeneration.ApprovalHistory);

        var historyEntry = payslipGeneration.ApprovalHistory.First();
        Assert.Equal(PayslipApprovalLevel.HR, historyEntry.ApprovalLevel);
        Assert.Equal(PayslipApprovalAction.Approved, historyEntry.Action);
        Assert.Equal(PayslipStatus.PendingHRApproval, historyEntry.PreviousStatus);
        Assert.Equal(PayslipStatus.PendingFinanceApproval, historyEntry.NewStatus);
    }

    [Fact]
    public async Task ProcessApprovalAsync_FinanceApproval_UpdatesStatusCorrectly()
    {
        // Arrange
        var request = new PayslipApprovalRequest
        {
            PayslipGenerationId = 1,
            ApprovalLevel = PayslipApprovalLevel.Finance,
            Action = PayslipApprovalAction.Approved,
            Notes = "Approved by Finance"
        };

        var payslipGeneration = new PayslipGeneration
        {
            Id = 1,
            Status = PayslipStatus.PendingFinanceApproval,
            ApprovalHistory = new List<PayslipApprovalHistory>()
        };

        _mockPayslipGenerationRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(payslipGeneration);

        _mockPayslipGenerationRepository.Setup(x => x.UpdateAsync(It.IsAny<PayslipGeneration>()))
            .Returns(Task.CompletedTask);

        _mockPayslipGenerationRepository.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(true));

        // Act
        var result = await _service.ProcessApprovalAsync(request, 1);

        // Assert
        Assert.True(result);
        Assert.Equal(PayslipStatus.FinanceApproved, payslipGeneration.Status);
        Assert.Equal(1, payslipGeneration.FinanceApprovedBy);
        Assert.NotNull(payslipGeneration.FinanceApprovedAt);
        Assert.Equal("Approved by Finance", payslipGeneration.FinanceApprovalNotes);
    }

    [Fact]
    public async Task ProcessApprovalAsync_HRRejection_UpdatesStatusCorrectly()
    {
        // Arrange
        var request = new PayslipApprovalRequest
        {
            PayslipGenerationId = 1,
            ApprovalLevel = PayslipApprovalLevel.HR,
            Action = PayslipApprovalAction.Rejected,
            RejectionReason = "Incorrect calculations"
        };

        var payslipGeneration = new PayslipGeneration
        {
            Id = 1,
            Status = PayslipStatus.PendingHRApproval,
            ApprovalHistory = new List<PayslipApprovalHistory>()
        };

        _mockPayslipGenerationRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(payslipGeneration);

        _mockPayslipGenerationRepository.Setup(x => x.UpdateAsync(It.IsAny<PayslipGeneration>()))
            .Returns(Task.CompletedTask);

        _mockPayslipGenerationRepository.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(true));

        // Act
        var result = await _service.ProcessApprovalAsync(request, 1);

        // Assert
        Assert.True(result);
        Assert.Equal(PayslipStatus.HRRejected, payslipGeneration.Status);
        Assert.Single(payslipGeneration.ApprovalHistory);

        var historyEntry = payslipGeneration.ApprovalHistory.First();
        Assert.Equal("Incorrect calculations", historyEntry.RejectionReason);
    }

    [Fact]
    public async Task ReleasePayslipsAsync_ValidRequest_ReleasesPayslips()
    {
        // Arrange
        var request = new PayslipReleaseRequest
        {
            PayslipGenerationIds = new List<int> { 1, 2 },
            SendNotifications = true,
            ReleaseNotes = "Monthly payroll release"
        };

        var payslip1 = new PayslipGeneration
        {
            Id = 1,
            Status = PayslipStatus.FinanceApproved,
            ApprovalHistory = new List<PayslipApprovalHistory>()
        };

        var payslip2 = new PayslipGeneration
        {
            Id = 2,
            Status = PayslipStatus.FinanceApproved,
            ApprovalHistory = new List<PayslipApprovalHistory>()
        };

        _mockPayslipGenerationRepository.SetupSequence(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(payslip1)
            .ReturnsAsync(payslip2);

        _mockPayslipGenerationRepository.Setup(x => x.UpdateAsync(It.IsAny<PayslipGeneration>()))
            .Returns(Task.CompletedTask);

        _mockPayslipGenerationRepository.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(true));

        // Act
        var result = await _service.ReleasePayslipsAsync(request, 1);

        // Assert
        Assert.True(result);
        Assert.Equal(PayslipStatus.Released, payslip1.Status);
        Assert.Equal(PayslipStatus.Released, payslip2.Status);
        Assert.Equal(1, payslip1.ReleasedBy);
        Assert.Equal(1, payslip2.ReleasedBy);
        Assert.NotNull(payslip1.ReleasedAt);
        Assert.NotNull(payslip2.ReleasedAt);
        Assert.True(payslip1.IsNotificationSent);
        Assert.True(payslip2.IsNotificationSent);

        _mockPayslipGenerationRepository.Verify(x => x.UpdateAsync(It.IsAny<PayslipGeneration>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetApprovalSummaryAsync_ValidRequest_ReturnsCorrectSummary()
    {
        // Arrange
        var payslips = new List<PayslipGeneration>
        {
            new PayslipGeneration
            {
                Status = PayslipStatus.PendingHRApproval,
                PayrollRecord = new PayrollRecord { NetSalary = 5000m, Currency = "USD" }
            },
            new PayslipGeneration
            {
                Status = PayslipStatus.PendingFinanceApproval,
                PayrollRecord = new PayrollRecord { NetSalary = 6000m, Currency = "USD" }
            },
            new PayslipGeneration
            {
                Status = PayslipStatus.Released,
                PayrollRecord = new PayrollRecord { NetSalary = 7000m, Currency = "USD" }
            },
            new PayslipGeneration
            {
                Status = PayslipStatus.HRRejected,
                PayrollRecord = new PayrollRecord { NetSalary = 4000m, Currency = "USD" }
            }
        };

        _mockPayslipGenerationRepository.Setup(x => x.GetByBranchAndPeriodAsync(1, 2024, 12))
            .ReturnsAsync(payslips);

        // Act
        var result = await _service.GetApprovalSummaryAsync(1, 2024, 12);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.TotalPayslips);
        Assert.Equal(1, result.PendingHRApproval);
        Assert.Equal(1, result.PendingFinanceApproval);
        Assert.Equal(0, result.Approved); // FinanceApproved status
        Assert.Equal(1, result.Released);
        Assert.Equal(1, result.Rejected);
        Assert.Equal(22000m, result.TotalPayrollAmount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public async Task RegeneratePayslipAsync_ValidRequest_CreatesNewVersion()
    {
        // Arrange
        var existingPayslip = new PayslipGeneration
        {
            Id = 1,
            PayrollRecordId = 1,
            PayslipTemplateId = 1,
            Version = 1,
            Status = PayslipStatus.Released
        };

        _mockPayslipGenerationRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(existingPayslip);

        _mockPayslipGenerationRepository.Setup(x => x.UpdateAsync(It.IsAny<PayslipGeneration>()))
            .Returns(Task.CompletedTask);

        // Setup for new payslip generation
        var payrollRecord = new PayrollRecord
        {
            Id = 1,
            EmployeeId = 1,
            PayrollMonth = 12,
            PayrollYear = 2024
        };

        var template = new PayslipTemplateDto { Id = 1, Name = "Default Template" };
        var payrollData = new PayrollCalculationResult { EmployeeId = 1, EmployeeName = "John Doe" };

        _mockPayrollService.Setup(x => x.GetPayrollRecordAsync(1))
            .ReturnsAsync(payrollRecord);

        _mockPayslipGenerationRepository.Setup(x => x.GetByPayrollRecordAsync(1))
            .ReturnsAsync((PayslipGeneration?)null); // No existing payslip for new generation

        _mockPayslipTemplateService.Setup(x => x.GetTemplateAsync(1))
            .ReturnsAsync(template);

        _mockPayrollService.Setup(x => x.CalculatePayrollAsync(It.IsAny<PayrollCalculationRequest>()))
            .ReturnsAsync(payrollData);

        _mockPayslipDesignerService.Setup(x => x.GeneratePdfPayslipAsync(template, payrollData))
            .ReturnsAsync((new byte[] { 1, 2, 3 }, "test.pdf"));

        _mockFileStorageService.Setup(x => x.SaveFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("/path/to/file.pdf");

        var newPayslip = new PayslipGeneration
        {
            Id = 2,
            PayrollRecordId = 1,
            Version = 2,
            RegenerationReason = "Correction needed",
            PayrollRecord = payrollRecord,
            GeneratedByEmployee = new Employee { FirstName = "Admin", LastName = "User" }
        };

        _mockPayslipGenerationRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(existingPayslip); // First call for existing payslip
        
        _mockPayslipGenerationRepository.Setup(x => x.GetByIdAsync(2))
            .ReturnsAsync(newPayslip); // Second call for new payslip

        _mockPayslipGenerationRepository.Setup(x => x.AddAsync(It.IsAny<PayslipGeneration>()))
            .Returns(Task.FromResult(new PayslipGeneration { Id = 2 }));

        _mockPayslipGenerationRepository.Setup(x => x.SaveChangesAsync())
            .Returns(Task.FromResult(true));

        // Act
        var result = await _service.RegeneratePayslipAsync(1, "Correction needed", 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(PayslipStatus.Cancelled, existingPayslip.Status);
        
        _mockPayslipGenerationRepository.Verify(x => x.UpdateAsync(existingPayslip), Times.Once);
        _mockPayslipGenerationRepository.Verify(x => x.AddAsync(It.IsAny<PayslipGeneration>()), Times.Once);
    }
}