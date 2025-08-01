using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

/// <summary>
/// Unit tests for AttendanceService focusing on core functionality
/// </summary>
public class AttendanceServiceUnitTests
{
    private readonly Mock<IAttendanceRepository> _mockAttendanceRepository;
    private readonly Mock<IBreakRecordRepository> _mockBreakRecordRepository;
    private readonly Mock<IAttendanceCorrectionRepository> _mockCorrectionRepository;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
    private readonly Mock<IBranchRepository> _mockBranchRepository;
    private readonly Mock<ILogger<AttendanceService>> _mockLogger;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly AttendanceService _attendanceService;

    public AttendanceServiceUnitTests()
    {
        _mockAttendanceRepository = new Mock<IAttendanceRepository>();
        _mockBreakRecordRepository = new Mock<IBreakRecordRepository>();
        _mockCorrectionRepository = new Mock<IAttendanceCorrectionRepository>();
        _mockEmployeeRepository = new Mock<IEmployeeRepository>();
        _mockBranchRepository = new Mock<IBranchRepository>();
        _mockLogger = new Mock<ILogger<AttendanceService>>();
        _mockAuditService = new Mock<IAuditService>();

        _attendanceService = new AttendanceService(
            _mockAttendanceRepository.Object,
            _mockBreakRecordRepository.Object,
            _mockCorrectionRepository.Object,
            _mockEmployeeRepository.Object,
            _mockBranchRepository.Object,
            _mockLogger.Object,
            _mockAuditService.Object);
    }

    [Fact]
    public async Task CheckInAsync_ValidRequest_ReturnsAttendanceRecord()
    {
        // Arrange
        var employeeId = 1;
        var employee = new Employee
        {
            Id = employeeId,
            EmployeeId = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            Branch = new Branch
            {
                Id = 1,
                Name = "Main Branch",
                TimeZone = "UTC",
                Organization = new Organization
                {
                    Id = 1,
                    Name = "Test Org",
                    NormalWorkingHours = 8.0m
                }
            }
        };

        var request = new CheckInRequest
        {
            Location = "Office",
            IpAddress = "192.168.1.1",
            DeviceInfo = "Mobile App"
        };

        var expectedRecord = new AttendanceRecord
        {
            Id = 1,
            EmployeeId = employeeId,
            Date = DateTime.Today,
            CheckInTime = DateTime.UtcNow,
            Status = AttendanceStatus.Present
        };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _mockAttendanceRepository.Setup(x => x.GetTodayRecordAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttendanceRecord?)null);
        _mockAttendanceRepository.Setup(x => x.AddAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRecord);
        _mockAttendanceRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockAuditService.Setup(x => x.LogDataModificationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _attendanceService.CheckInAsync(employeeId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        _mockAttendanceRepository.Verify(x => x.AddAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockAttendanceRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockAuditService.Verify(x => x.LogDataModificationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task StartBreakAsync_ValidRequest_ReturnsBreakRecord()
    {
        // Arrange
        var employeeId = 1;
        var employee = new Employee
        {
            Id = employeeId,
            Branch = new Branch { TimeZone = "UTC" }
        };
        var attendanceRecord = new AttendanceRecord
        {
            Id = 1,
            EmployeeId = employeeId,
            CheckInTime = DateTime.UtcNow.AddHours(-2)
        };
        var request = new StartBreakRequest
        {
            Type = BreakType.Tea,
            Location = "Office"
        };
        var expectedBreak = new BreakRecord
        {
            Id = 1,
            AttendanceRecordId = 1,
            Type = BreakType.Tea
        };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _mockAttendanceRepository.Setup(x => x.GetTodayRecordAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attendanceRecord);
        _mockBreakRecordRepository.Setup(x => x.GetActiveBreakAsync(attendanceRecord.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BreakRecord?)null);
        _mockBreakRecordRepository.Setup(x => x.AddAsync(It.IsAny<BreakRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBreak);
        _mockBreakRecordRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockAttendanceRepository.Setup(x => x.UpdateAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(attendanceRecord);
        _mockAttendanceRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockAuditService.Setup(x => x.LogDataModificationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _attendanceService.StartBreakAsync(employeeId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(BreakType.Tea, result.Type);
        _mockBreakRecordRepository.Verify(x => x.AddAsync(It.IsAny<BreakRecord>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockBreakRecordRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockAuditService.Verify(x => x.LogDataModificationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrentStatusAsync_ExistingRecord_ReturnsStatus()
    {
        // Arrange
        var employeeId = 1;
        var record = new AttendanceRecord
        {
            Id = 1,
            EmployeeId = employeeId,
            Status = AttendanceStatus.Present
        };

        _mockAttendanceRepository.Setup(x => x.GetTodayRecordAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        // Act
        var result = await _attendanceService.GetCurrentStatusAsync(employeeId);

        // Assert
        Assert.Equal(AttendanceStatus.Present, result);
    }

    [Fact]
    public async Task GetCurrentStatusAsync_NoRecord_ReturnsAbsent()
    {
        // Arrange
        var employeeId = 1;
        _mockAttendanceRepository.Setup(x => x.GetTodayRecordAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttendanceRecord?)null);

        // Act
        var result = await _attendanceService.GetCurrentStatusAsync(employeeId);

        // Assert
        Assert.Equal(AttendanceStatus.Absent, result);
    }

    [Fact]
    public async Task RequestCorrectionAsync_ValidRequest_ReturnsCorrection()
    {
        // Arrange
        var attendanceId = 1;
        var attendanceRecord = new AttendanceRecord
        {
            Id = attendanceId,
            EmployeeId = 1
        };
        var request = new CorrectionRequest
        {
            RequestedBy = 1,
            Type = CorrectionType.CheckInTime,
            OriginalValue = "09:00",
            CorrectedValue = "08:30",
            Reason = "Traffic delay"
        };
        var expectedCorrection = new AttendanceCorrection
        {
            Id = 1,
            AttendanceRecordId = attendanceId,
            Type = CorrectionType.CheckInTime,
            Status = CorrectionStatus.Pending
        };

        _mockAttendanceRepository.Setup(x => x.GetByIdAsync(attendanceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attendanceRecord);
        _mockCorrectionRepository.Setup(x => x.AddAsync(It.IsAny<AttendanceCorrection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCorrection);
        _mockCorrectionRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mockAuditService.Setup(x => x.LogDataModificationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _attendanceService.RequestCorrectionAsync(attendanceId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(CorrectionStatus.Pending, result.Status);
        _mockCorrectionRepository.Verify(x => x.AddAsync(It.IsAny<AttendanceCorrection>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockCorrectionRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockAuditService.Verify(x => x.LogDataModificationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()), Times.Once);
    }
}