using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class AttendanceServiceTests
{
    private readonly Mock<IAttendanceRepository> _mockAttendanceRepository;
    private readonly Mock<IBreakRecordRepository> _mockBreakRecordRepository;
    private readonly Mock<IAttendanceCorrectionRepository> _mockCorrectionRepository;
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
    private readonly Mock<IBranchRepository> _mockBranchRepository;
    private readonly Mock<ILogger<AttendanceService>> _mockLogger;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly AttendanceService _attendanceService;

    public AttendanceServiceTests()
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

        // Act
        var result = await _attendanceService.CheckInAsync(employeeId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        _mockAttendanceRepository.Verify(x => x.AddAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockAttendanceRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckInAsync_EmployeeNotFound_ThrowsArgumentException()
    {
        // Arrange
        var employeeId = 999;
        var request = new CheckInRequest { Location = "Office" };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _attendanceService.CheckInAsync(employeeId, request));
        Assert.Contains("Employee with ID 999 not found", exception.Message);
    }

    [Fact]
    public async Task CheckInAsync_AlreadyCheckedIn_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = 1;
        var employee = new Employee
        {
            Id = employeeId,
            Branch = new Branch { TimeZone = "UTC", Organization = new Organization() }
        };
        var existingRecord = new AttendanceRecord
        {
            Id = 1,
            EmployeeId = employeeId,
            CheckInTime = DateTime.UtcNow.AddHours(-1)
        };
        var request = new CheckInRequest { Location = "Office" };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _mockAttendanceRepository.Setup(x => x.GetTodayRecordAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRecord);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _attendanceService.CheckInAsync(employeeId, request));
        Assert.Contains("Employee has already checked in today", exception.Message);
    }

    [Fact]
    public async Task CheckOutAsync_ValidRequest_ReturnsAttendanceRecord()
    {
        // Arrange
        var employeeId = 1;
        var employee = new Employee
        {
            Id = employeeId,
            EmployeeId = "EMP001",
            Branch = new Branch
            {
                TimeZone = "UTC",
                Organization = new Organization { NormalWorkingHours = 8.0m }
            }
        };
        var existingRecord = new AttendanceRecord
        {
            Id = 1,
            EmployeeId = employeeId,
            CheckInTime = DateTime.UtcNow.AddHours(-8),
            BreakRecords = new List<BreakRecord>()
        };
        var request = new CheckOutRequest { Location = "Office" };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _mockAttendanceRepository.Setup(x => x.GetTodayRecordAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRecord);
        _mockBreakRecordRepository.Setup(x => x.GetActiveBreaksByEmployeeAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BreakRecord>());
        _mockAttendanceRepository.Setup(x => x.UpdateAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRecord);
        _mockAttendanceRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _attendanceService.CheckOutAsync(employeeId, request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.CheckOutTime);
        _mockAttendanceRepository.Verify(x => x.UpdateAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockAttendanceRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckOutAsync_NotCheckedIn_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = 1;
        var employee = new Employee { Id = employeeId };
        var request = new CheckOutRequest { Location = "Office" };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _mockAttendanceRepository.Setup(x => x.GetTodayRecordAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttendanceRecord?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _attendanceService.CheckOutAsync(employeeId, request));
        Assert.Contains("Employee has not checked in today", exception.Message);
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

        // Act
        var result = await _attendanceService.StartBreakAsync(employeeId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(BreakType.Tea, result.Type);
        _mockBreakRecordRepository.Verify(x => x.AddAsync(It.IsAny<BreakRecord>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockBreakRecordRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartBreakAsync_AlreadyOnBreak_ThrowsInvalidOperationException()
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
        var activeBreak = new BreakRecord
        {
            Id = 1,
            AttendanceRecordId = 1,
            Type = BreakType.Lunch,
            StartTime = DateTime.UtcNow.AddMinutes(-30)
        };
        var request = new StartBreakRequest { Type = BreakType.Tea };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _mockAttendanceRepository.Setup(x => x.GetTodayRecordAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attendanceRecord);
        _mockBreakRecordRepository.Setup(x => x.GetActiveBreakAsync(attendanceRecord.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeBreak);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _attendanceService.StartBreakAsync(employeeId, request));
        Assert.Contains("Employee is already on a break", exception.Message);
    }

    [Fact]
    public async Task EndBreakAsync_ValidRequest_ReturnsBreakRecord()
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
        var activeBreak = new BreakRecord
        {
            Id = 1,
            AttendanceRecordId = 1,
            Type = BreakType.Tea,
            StartTime = DateTime.UtcNow.AddMinutes(-15),
            AttendanceRecord = attendanceRecord
        };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _mockAttendanceRepository.Setup(x => x.GetTodayRecordAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(attendanceRecord);
        _mockBreakRecordRepository.Setup(x => x.GetActiveBreakAsync(attendanceRecord.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeBreak);
        _mockBreakRecordRepository.Setup(x => x.UpdateAsync(It.IsAny<BreakRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeBreak);
        _mockAttendanceRepository.Setup(x => x.UpdateAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(attendanceRecord);
        _mockAttendanceRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _attendanceService.EndBreakAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.EndTime);
        _mockBreakRecordRepository.Verify(x => x.UpdateAsync(It.IsAny<BreakRecord>(), It.IsAny<CancellationToken>()), Times.Once);
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

        // Act
        var result = await _attendanceService.RequestCorrectionAsync(attendanceId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(CorrectionStatus.Pending, result.Status);
        _mockCorrectionRepository.Verify(x => x.AddAsync(It.IsAny<AttendanceCorrection>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockCorrectionRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateManualEntryAsync_ValidRequest_ReturnsAttendanceRecord()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 1,
            EmployeeId = "EMP001",
            Branch = new Branch { TimeZone = "UTC" }
        };
        var request = new ManualAttendanceRequest
        {
            EmployeeId = 1,
            Date = DateTime.Today,
            CheckInTime = DateTime.Today.AddHours(9),
            CheckOutTime = DateTime.Today.AddHours(17),
            Status = AttendanceStatus.Present,
            Reason = "System was down",
            EnteredBy = 2
        };
        var expectedRecord = new AttendanceRecord
        {
            Id = 1,
            EmployeeId = 1,
            IsManualEntry = true
        };

        _mockEmployeeRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _mockAttendanceRepository.Setup(x => x.GetEmployeeAttendanceByDateAsync(1, DateTime.Today, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AttendanceRecord?)null);
        _mockAttendanceRepository.Setup(x => x.AddAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRecord);
        _mockAttendanceRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _attendanceService.CreateManualEntryAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsManualEntry);
        _mockAttendanceRepository.Verify(x => x.AddAsync(It.IsAny<AttendanceRecord>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockAttendanceRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}