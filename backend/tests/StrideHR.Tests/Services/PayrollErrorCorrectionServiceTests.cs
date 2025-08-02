using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Payroll;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class PayrollErrorCorrectionServiceTests
{
    private readonly Mock<IPayrollErrorCorrectionRepository> _mockErrorCorrectionRepository;
    private readonly Mock<IPayrollRepository> _mockPayrollRepository;
    private readonly Mock<IPayrollService> _mockPayrollService;
    private readonly Mock<IPayrollAuditTrailRepository> _mockAuditTrailRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<PayrollErrorCorrectionService>> _mockLogger;
    private readonly PayrollErrorCorrectionService _service;

    public PayrollErrorCorrectionServiceTests()
    {
        _mockErrorCorrectionRepository = new Mock<IPayrollErrorCorrectionRepository>();
        _mockPayrollRepository = new Mock<IPayrollRepository>();
        _mockPayrollService = new Mock<IPayrollService>();
        _mockAuditTrailRepository = new Mock<IPayrollAuditTrailRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<PayrollErrorCorrectionService>>();

        _service = new PayrollErrorCorrectionService(
            _mockErrorCorrectionRepository.Object,
            _mockPayrollRepository.Object,
            _mockPayrollService.Object,
            _mockAuditTrailRepository.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateErrorCorrectionAsync_WithValidRequest_ReturnsErrorCorrectionResult()
    {
        // Arrange
        var request = new PayrollErrorCorrectionRequest
        {
            PayrollRecordId = 1,
            ErrorType = PayrollErrorType.CalculationError,
            ErrorDescription = "Incorrect basic salary calculation",
            CorrectionData = new Dictionary<string, object> { { "BasicSalary", 5500 } },
            Reason = "Salary increase not applied",
            RequestedBy = 1
        };

        var payrollRecord = new PayrollRecord
        {
            Id = 1,
            EmployeeId = 1,
            BasicSalary = 5000,
            GrossSalary = 6000,
            NetSalary = 4500,
            Employee = new Employee { FirstName = "John", LastName = "Doe" }
        };

        _mockPayrollRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(payrollRecord);

        _mockErrorCorrectionRepository.Setup(x => x.AddAsync(It.IsAny<PayrollErrorCorrection>()))
            .ReturnsAsync(new PayrollErrorCorrection { Id = 1 });

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockAuditTrailRepository.Setup(x => x.AddAsync(It.IsAny<PayrollAuditTrail>()))
            .ReturnsAsync(new PayrollAuditTrail { Id = 1 });

        // Act
        var result = await _service.CreateErrorCorrectionAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.PayrollRecordId);
        Assert.Equal(PayrollErrorType.CalculationError, result.ErrorType);
        Assert.Equal("Pending", result.Status);
        Assert.NotNull(result.OriginalCalculation);
        Assert.NotNull(result.CorrectedCalculation);
        Assert.NotEmpty(result.Changes);

        _mockErrorCorrectionRepository.Verify(x => x.AddAsync(It.IsAny<PayrollErrorCorrection>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        _mockAuditTrailRepository.Verify(x => x.AddAsync(It.IsAny<PayrollAuditTrail>()), Times.Once);
    }

    [Fact]
    public async Task CreateErrorCorrectionAsync_WithInvalidPayrollRecord_ThrowsArgumentException()
    {
        // Arrange
        var request = new PayrollErrorCorrectionRequest
        {
            PayrollRecordId = 999,
            ErrorType = PayrollErrorType.CalculationError,
            ErrorDescription = "Test error",
            CorrectionData = new Dictionary<string, object> { { "BasicSalary", 5500 } },
            RequestedBy = 1
        };

        _mockPayrollRepository.Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((PayrollRecord?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateErrorCorrectionAsync(request));
    }

    [Fact]
    public async Task ApproveErrorCorrectionAsync_WithValidCorrectionId_ReturnsTrue()
    {
        // Arrange
        var correctionId = 1;
        var approvedBy = 2;
        var notes = "Approved for processing";

        var errorCorrection = new PayrollErrorCorrection
        {
            Id = correctionId,
            PayrollRecordId = 1,
            Status = PayrollCorrectionStatus.Pending,
            PayrollRecord = new PayrollRecord { EmployeeId = 1 }
        };

        _mockErrorCorrectionRepository.Setup(x => x.GetByIdAsync(correctionId))
            .ReturnsAsync(errorCorrection);

        _mockErrorCorrectionRepository.Setup(x => x.UpdateAsync(It.IsAny<PayrollErrorCorrection>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockAuditTrailRepository.Setup(x => x.AddAsync(It.IsAny<PayrollAuditTrail>()))
            .ReturnsAsync(new PayrollAuditTrail { Id = 1 });

        // Act
        var result = await _service.ApproveErrorCorrectionAsync(correctionId, approvedBy, notes);

        // Assert
        Assert.True(result);
        Assert.Equal(PayrollCorrectionStatus.Approved, errorCorrection.Status);
        Assert.Equal(approvedBy, errorCorrection.ApprovedBy);
        Assert.Equal(notes, errorCorrection.ApprovalNotes);
        Assert.NotNull(errorCorrection.ApprovedAt);

        _mockErrorCorrectionRepository.Verify(x => x.UpdateAsync(errorCorrection), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ApproveErrorCorrectionAsync_WithInvalidCorrectionId_ReturnsFalse()
    {
        // Arrange
        var correctionId = 999;
        var approvedBy = 2;

        _mockErrorCorrectionRepository.Setup(x => x.GetByIdAsync(correctionId))
            .ReturnsAsync((PayrollErrorCorrection?)null);

        // Act
        var result = await _service.ApproveErrorCorrectionAsync(correctionId, approvedBy);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RejectErrorCorrectionAsync_WithValidCorrectionId_ReturnsTrue()
    {
        // Arrange
        var correctionId = 1;
        var rejectedBy = 2;
        var reason = "Insufficient documentation";

        var errorCorrection = new PayrollErrorCorrection
        {
            Id = correctionId,
            PayrollRecordId = 1,
            Status = PayrollCorrectionStatus.Pending,
            PayrollRecord = new PayrollRecord { EmployeeId = 1 }
        };

        _mockErrorCorrectionRepository.Setup(x => x.GetByIdAsync(correctionId))
            .ReturnsAsync(errorCorrection);

        _mockErrorCorrectionRepository.Setup(x => x.UpdateAsync(It.IsAny<PayrollErrorCorrection>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockAuditTrailRepository.Setup(x => x.AddAsync(It.IsAny<PayrollAuditTrail>()))
            .ReturnsAsync(new PayrollAuditTrail { Id = 1 });

        // Act
        var result = await _service.RejectErrorCorrectionAsync(correctionId, rejectedBy, reason);

        // Assert
        Assert.True(result);
        Assert.Equal(PayrollCorrectionStatus.Rejected, errorCorrection.Status);
        Assert.Equal(rejectedBy, errorCorrection.ApprovedBy);
        Assert.Equal(reason, errorCorrection.ApprovalNotes);
        Assert.NotNull(errorCorrection.ApprovedAt);
    }

    [Fact]
    public async Task ProcessErrorCorrectionAsync_WithApprovedCorrection_ReturnsCalculationResult()
    {
        // Arrange
        var correctionId = 1;
        var processedBy = 3;

        var errorCorrection = new PayrollErrorCorrection
        {
            Id = correctionId,
            PayrollRecordId = 1,
            Status = PayrollCorrectionStatus.Approved,
            CorrectionData = "{\"BasicSalary\": 5500}"
        };

        var payrollRecord = new PayrollRecord
        {
            Id = 1,
            EmployeeId = 1,
            BasicSalary = 5000,
            GrossSalary = 6000,
            NetSalary = 4500,
            Employee = new Employee { FirstName = "John", LastName = "Doe" }
        };

        _mockErrorCorrectionRepository.Setup(x => x.GetByIdAsync(correctionId))
            .ReturnsAsync(errorCorrection);

        _mockPayrollRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(payrollRecord);

        _mockPayrollRepository.Setup(x => x.UpdateAsync(It.IsAny<PayrollRecord>()))
            .Returns(Task.CompletedTask);

        _mockErrorCorrectionRepository.Setup(x => x.UpdateAsync(It.IsAny<PayrollErrorCorrection>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockAuditTrailRepository.Setup(x => x.AddAsync(It.IsAny<PayrollAuditTrail>()))
            .ReturnsAsync(new PayrollAuditTrail { Id = 1 });

        // Act
        var result = await _service.ProcessErrorCorrectionAsync(correctionId, processedBy);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.EmployeeId);
        Assert.Equal("John Doe", result.EmployeeName);
        Assert.Equal(PayrollCorrectionStatus.Processed, errorCorrection.Status);
        Assert.Equal(processedBy, errorCorrection.ProcessedBy);
        Assert.NotNull(errorCorrection.ProcessedAt);

        _mockPayrollRepository.Verify(x => x.UpdateAsync(payrollRecord), Times.Once);
        _mockErrorCorrectionRepository.Verify(x => x.UpdateAsync(errorCorrection), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ProcessErrorCorrectionAsync_WithUnapprovedCorrection_ThrowsInvalidOperationException()
    {
        // Arrange
        var correctionId = 1;
        var processedBy = 3;

        var errorCorrection = new PayrollErrorCorrection
        {
            Id = correctionId,
            Status = PayrollCorrectionStatus.Pending
        };

        _mockErrorCorrectionRepository.Setup(x => x.GetByIdAsync(correctionId))
            .ReturnsAsync(errorCorrection);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.ProcessErrorCorrectionAsync(correctionId, processedBy));
    }

    [Fact]
    public async Task GetErrorCorrectionAsync_WithValidId_ReturnsErrorCorrection()
    {
        // Arrange
        var correctionId = 1;
        var errorCorrection = new PayrollErrorCorrection
        {
            Id = correctionId,
            PayrollRecordId = 1,
            ErrorType = PayrollErrorType.CalculationError
        };

        _mockErrorCorrectionRepository.Setup(x => x.GetByIdAsync(correctionId))
            .ReturnsAsync(errorCorrection);

        // Act
        var result = await _service.GetErrorCorrectionAsync(correctionId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(correctionId, result.Id);
        Assert.Equal(PayrollErrorType.CalculationError, result.ErrorType);
    }

    [Fact]
    public async Task GetPayrollErrorCorrectionsAsync_WithValidPayrollRecordId_ReturnsErrorCorrections()
    {
        // Arrange
        var payrollRecordId = 1;
        var errorCorrections = new List<PayrollErrorCorrection>
        {
            new PayrollErrorCorrection { Id = 1, PayrollRecordId = payrollRecordId },
            new PayrollErrorCorrection { Id = 2, PayrollRecordId = payrollRecordId }
        };

        _mockErrorCorrectionRepository.Setup(x => x.GetByPayrollRecordIdAsync(payrollRecordId))
            .ReturnsAsync(errorCorrections);

        // Act
        var result = await _service.GetPayrollErrorCorrectionsAsync(payrollRecordId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, ec => Assert.Equal(payrollRecordId, ec.PayrollRecordId));
    }

    [Fact]
    public async Task GetPendingErrorCorrectionsAsync_WithBranchId_ReturnsPendingCorrections()
    {
        // Arrange
        var branchId = 1;
        var pendingCorrections = new List<PayrollErrorCorrection>
        {
            new PayrollErrorCorrection { Id = 1, Status = PayrollCorrectionStatus.Pending },
            new PayrollErrorCorrection { Id = 2, Status = PayrollCorrectionStatus.UnderReview }
        };

        _mockErrorCorrectionRepository.Setup(x => x.GetPendingCorrectionsAsync(branchId))
            .ReturnsAsync(pendingCorrections);

        // Act
        var result = await _service.GetPendingErrorCorrectionsAsync(branchId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, ec => Assert.True(
            ec.Status == PayrollCorrectionStatus.Pending || 
            ec.Status == PayrollCorrectionStatus.UnderReview));
    }

    [Fact]
    public async Task ValidateErrorCorrectionAsync_WithValidRequest_ReturnsNoErrors()
    {
        // Arrange
        var request = new PayrollErrorCorrectionRequest
        {
            PayrollRecordId = 1,
            ErrorType = PayrollErrorType.CalculationError,
            CorrectionData = new Dictionary<string, object> { { "BasicSalary", 5500 } }
        };

        var payrollRecord = new PayrollRecord { Id = 1, EmployeeId = 1 };

        _mockPayrollRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(payrollRecord);

        // Act
        var result = await _service.ValidateErrorCorrectionAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ValidateErrorCorrectionAsync_WithInvalidPayrollRecord_ReturnsErrors()
    {
        // Arrange
        var request = new PayrollErrorCorrectionRequest
        {
            PayrollRecordId = 999,
            ErrorType = PayrollErrorType.CalculationError,
            CorrectionData = new Dictionary<string, object> { { "BasicSalary", 5500 } }
        };

        _mockPayrollRepository.Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((PayrollRecord?)null);

        // Act
        var result = await _service.ValidateErrorCorrectionAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Payroll record not found", result);
    }

    [Fact]
    public async Task CancelErrorCorrectionAsync_WithValidCorrectionId_ReturnsTrue()
    {
        // Arrange
        var correctionId = 1;
        var cancelledBy = 2;
        var reason = "No longer needed";

        var errorCorrection = new PayrollErrorCorrection
        {
            Id = correctionId,
            PayrollRecordId = 1,
            Status = PayrollCorrectionStatus.Pending,
            PayrollRecord = new PayrollRecord { EmployeeId = 1 }
        };

        _mockErrorCorrectionRepository.Setup(x => x.GetByIdAsync(correctionId))
            .ReturnsAsync(errorCorrection);

        _mockErrorCorrectionRepository.Setup(x => x.UpdateAsync(It.IsAny<PayrollErrorCorrection>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _mockAuditTrailRepository.Setup(x => x.AddAsync(It.IsAny<PayrollAuditTrail>()))
            .ReturnsAsync(new PayrollAuditTrail { Id = 1 });

        // Act
        var result = await _service.CancelErrorCorrectionAsync(correctionId, cancelledBy, reason);

        // Assert
        Assert.True(result);
        Assert.Equal(PayrollCorrectionStatus.Cancelled, errorCorrection.Status);
        Assert.Equal(cancelledBy, errorCorrection.ProcessedBy);
        Assert.Equal(reason, errorCorrection.ProcessingNotes);
        Assert.NotNull(errorCorrection.ProcessedAt);
    }
}