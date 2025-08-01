using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Infrastructure.Services;
using System.Linq.Expressions;
using Xunit;

namespace StrideHR.Tests.Services;

public class AttendanceServiceTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<AttendanceService>> _mockLogger;
    private Mock<IAuditLogService> _mockAuditLogService;
    private Mock<IRepository<AttendanceRecord>> _mockAttendanceRepository;
    private Mock<IRepository<BreakRecord>> _mockBreakRepository;
    private Mock<IRepository<Employee>> _mockEmployeeRepository;
    private AttendanceService _attendanceService;

    public AttendanceServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<AttendanceService>>();
        _mockAuditLogService = new Mock<IAuditLogService>();
        _mockAttendanceRepository = new Mock<IRepository<AttendanceRecord>>();
        _mockBreakRepository = new Mock<IRepository<BreakRecord>>();
        _mockEmployeeRepository = new Mock<IRepository<Employee>>();

        _mockUnitOfWork.Setup(u => u.AttendanceRecords).Returns(_mockAttendanceRepository.Object);
        _mockUnitOfWork.Setup(u => u.BreakRecords).Returns(_mockBreakRepository.Object);
        _mockUnitOfWork.Setup(u => u.Employees).Returns(_mockEmployeeRepository.Object);

        _attendanceService = new AttendanceService(
            _mockUnitOfWork.Object,
            _mockLogger.Object,
            _mockAuditLogService.Object);
    }

    [Fact]
    public async Task GetTodayAttendanceAsync_ValidEmployeeId_ReturnsAttendanceRecord()
    {
        // Arrange
        var employeeId = 1;
        var today = DateTime.Today;
        var expectedRecord = new AttendanceRecord
        {
            Id = 1,
            EmployeeId = employeeId,
            Date = today,
            CheckInTime = DateTime.Now,
            Status = AttendanceStatus.Present
        };

        _mockAttendanceRepository
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AttendanceRecord, bool>>>(),
                It.IsAny<Expression<Func<AttendanceRecord, object>>[]>()))
            .ReturnsAsync(expectedRecord);

        // Act
        var result = await _attendanceService.GetTodayAttendanceAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(today, result.Date);
    }

    [Fact]
    public async Task CheckInAsync_ValidEmployee_ReturnsAttendanceRecord()
    {
        // Arrange
        var employeeId = 1;
        var location = "Office";
        var latitude = 40.7128;
        var longitude = -74.0060;
        var deviceInfo = "iPhone 12";
        var ipAddress = "192.168.1.1";

        _mockAttendanceRepository
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AttendanceRecord, bool>>>(),
                It.IsAny<Expression<Func<AttendanceRecord, object>>[]>()))
            .ReturnsAsync((AttendanceRecord?)null);

        _mockEmployeeRepository
            .Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync(new Employee
            {
                Id = employeeId,
                BranchId = 1,
                Branch = new Branch
                {
                    Id = 1,
                    Organization = new Organization
                    {
                        NormalWorkingHours = TimeSpan.FromHours(23) // 11 PM start time (future time to avoid late status)
                    }
                }
            });

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _attendanceService.CheckInAsync(employeeId, location, latitude, longitude, deviceInfo, ipAddress);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(location, result.CheckInLocation);
        Assert.Equal(latitude, result.CheckInLatitude);
        Assert.Equal(longitude, result.CheckInLongitude);
        Assert.Equal(deviceInfo, result.DeviceInfo);
        Assert.Equal(ipAddress, result.IpAddress);
        Assert.Equal(AttendanceStatus.Present, result.Status);
        Assert.True(result.CheckInTime.HasValue);

        _mockAttendanceRepository.Verify(r => r.AddAsync(It.IsAny<AttendanceRecord>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CheckInAsync_AlreadyCheckedIn_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = 1;
        var existingRecord = new AttendanceRecord
        {
            Id = 1,
            EmployeeId = employeeId,
            Date = DateTime.Today,
            CheckInTime = DateTime.Now.AddHours(-1),
            Status = AttendanceStatus.Present
        };

        _mockAttendanceRepository
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AttendanceRecord, bool>>>(),
                It.IsAny<Expression<Func<AttendanceRecord, object>>[]>()))
            .ReturnsAsync(existingRecord);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _attendanceService.CheckInAsync(employeeId));
    }

    [Fact]
    public async Task CheckOutAsync_ValidEmployee_ReturnsAttendanceRecord()
    {
        // Arrange
        var employeeId = 1;
        var location = "Office";
        var latitude = 40.7128;
        var longitude = -74.0060;
        var checkInTime = DateTime.Now.AddHours(-8);

        var existingRecord = new AttendanceRecord
        {
            Id = 1,
            EmployeeId = employeeId,
            Date = DateTime.Today,
            CheckInTime = checkInTime,
            Status = AttendanceStatus.Present,
            BreakRecords = new List<BreakRecord>
            {
                new BreakRecord
                {
                    Id = 1,
                    Type = BreakType.Lunch,
                    StartTime = checkInTime.AddHours(4),
                    EndTime = checkInTime.AddHours(4.5),
                    Duration = TimeSpan.FromMinutes(30)
                }
            }
        };

        _mockAttendanceRepository
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AttendanceRecord, bool>>>(),
                It.IsAny<Expression<Func<AttendanceRecord, object>>[]>()))
            .ReturnsAsync(existingRecord);

        _mockEmployeeRepository
            .Setup(r => r.GetByIdAsync(employeeId))
            .ReturnsAsync(new Employee
            {
                Id = employeeId,
                BranchId = 1,
                Branch = new Branch
                {
                    Id = 1,
                    Organization = new Organization
                    {
                        NormalWorkingHours = TimeSpan.FromHours(9) // 9 AM start time
                    }
                }
            });

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _attendanceService.CheckOutAsync(employeeId, location, latitude, longitude);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(location, result.CheckOutLocation);
        Assert.Equal(latitude, result.CheckOutLatitude);
        Assert.Equal(longitude, result.CheckOutLongitude);
        Assert.True(result.CheckOutTime.HasValue);
        Assert.True(result.TotalWorkingHours.HasValue);
        Assert.True(result.BreakDuration.HasValue);

        _mockAttendanceRepository.Verify(r => r.UpdateAsync(It.IsAny<AttendanceRecord>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CheckOutAsync_NotCheckedIn_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = 1;

        _mockAttendanceRepository
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AttendanceRecord, bool>>>(),
                It.IsAny<Expression<Func<AttendanceRecord, object>>[]>()))
            .ReturnsAsync((AttendanceRecord?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _attendanceService.CheckOutAsync(employeeId));
    }

    [Fact]
    public async Task StartBreakAsync_ValidEmployee_ReturnsBreakRecord()
    {
        // Arrange
        var employeeId = 1;
        var breakType = BreakType.Lunch;
        var location = "Cafeteria";
        var latitude = 40.7128;
        var longitude = -74.0060;

        var attendanceRecord = new AttendanceRecord
        {
            Id = 1,
            EmployeeId = employeeId,
            Date = DateTime.Today,
            CheckInTime = DateTime.Now.AddHours(-4),
            Status = AttendanceStatus.Present,
            BreakRecords = new List<BreakRecord>()
        };

        _mockAttendanceRepository
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AttendanceRecord, bool>>>(),
                It.IsAny<Expression<Func<AttendanceRecord, object>>[]>()))
            .ReturnsAsync(attendanceRecord);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _attendanceService.StartBreakAsync(employeeId, breakType, location, latitude, longitude);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(attendanceRecord.Id, result.AttendanceRecordId);
        Assert.Equal(breakType, result.Type);
        Assert.Equal(location, result.Location);
        Assert.Equal(latitude, result.Latitude);
        Assert.Equal(longitude, result.Longitude);
        Assert.True(result.StartTime > DateTime.MinValue);
        Assert.Null(result.EndTime);

        _mockBreakRepository.Verify(r => r.AddAsync(It.IsAny<BreakRecord>()), Times.Once);
        _mockAttendanceRepository.Verify(r => r.UpdateAsync(It.IsAny<AttendanceRecord>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task StartBreakAsync_AlreadyOnBreak_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = 1;
        var breakType = BreakType.Tea;

        var attendanceRecord = new AttendanceRecord
        {
            Id = 1,
            EmployeeId = employeeId,
            Date = DateTime.Today,
            CheckInTime = DateTime.Now.AddHours(-4),
            Status = AttendanceStatus.OnBreak,
            BreakRecords = new List<BreakRecord>
            {
                new BreakRecord
                {
                    Id = 1,
                    Type = BreakType.Lunch,
                    StartTime = DateTime.Now.AddMinutes(-15),
                    EndTime = null // Active break
                }
            }
        };

        _mockAttendanceRepository
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AttendanceRecord, bool>>>(),
                It.IsAny<Expression<Func<AttendanceRecord, object>>[]>()))
            .ReturnsAsync(attendanceRecord);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _attendanceService.StartBreakAsync(employeeId, breakType));
    }

    [Fact]
    public async Task EndBreakAsync_ValidEmployee_ReturnsBreakRecord()
    {
        // Arrange
        var employeeId = 1;
        var startTime = DateTime.Now.AddMinutes(-30);

        var activeBreak = new BreakRecord
        {
            Id = 1,
            Type = BreakType.Lunch,
            StartTime = startTime,
            EndTime = null
        };

        var attendanceRecord = new AttendanceRecord
        {
            Id = 1,
            EmployeeId = employeeId,
            Date = DateTime.Today,
            CheckInTime = DateTime.Now.AddHours(-4),
            Status = AttendanceStatus.OnBreak,
            BreakRecords = new List<BreakRecord> { activeBreak }
        };

        _mockAttendanceRepository
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AttendanceRecord, bool>>>(),
                It.IsAny<Expression<Func<AttendanceRecord, object>>[]>()))
            .ReturnsAsync(attendanceRecord);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _attendanceService.EndBreakAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(activeBreak.Id, result.Id);
        Assert.True(result.EndTime.HasValue);
        Assert.True(result.Duration.HasValue);
        Assert.True(result.Duration.Value.TotalMinutes > 0);

        _mockBreakRepository.Verify(r => r.UpdateAsync(It.IsAny<BreakRecord>()), Times.Once);
        _mockAttendanceRepository.Verify(r => r.UpdateAsync(It.IsAny<AttendanceRecord>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task EndBreakAsync_NotOnBreak_ThrowsInvalidOperationException()
    {
        // Arrange
        var employeeId = 1;

        var attendanceRecord = new AttendanceRecord
        {
            Id = 1,
            EmployeeId = employeeId,
            Date = DateTime.Today,
            CheckInTime = DateTime.Now.AddHours(-4),
            Status = AttendanceStatus.Present,
            BreakRecords = new List<BreakRecord>()
        };

        _mockAttendanceRepository
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AttendanceRecord, bool>>>(),
                It.IsAny<Expression<Func<AttendanceRecord, object>>[]>()))
            .ReturnsAsync(attendanceRecord);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _attendanceService.EndBreakAsync(employeeId));
    }

    [Fact]
    public async Task IsEmployeeCheckedInAsync_CheckedIn_ReturnsTrue()
    {
        // Arrange
        var employeeId = 1;
        var attendanceRecord = new AttendanceRecord
        {
            Id = 1,
            EmployeeId = employeeId,
            Date = DateTime.Today,
            CheckInTime = DateTime.Now.AddHours(-4),
            CheckOutTime = null,
            Status = AttendanceStatus.Present
        };

        _mockAttendanceRepository
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AttendanceRecord, bool>>>(),
                It.IsAny<Expression<Func<AttendanceRecord, object>>[]>()))
            .ReturnsAsync(attendanceRecord);

        // Act
        var result = await _attendanceService.IsEmployeeCheckedInAsync(employeeId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsEmployeeCheckedInAsync_NotCheckedIn_ReturnsFalse()
    {
        // Arrange
        var employeeId = 1;

        _mockAttendanceRepository
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AttendanceRecord, bool>>>(),
                It.IsAny<Expression<Func<AttendanceRecord, object>>[]>()))
            .ReturnsAsync((AttendanceRecord?)null);

        // Act
        var result = await _attendanceService.IsEmployeeCheckedInAsync(employeeId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsEmployeeOnBreakAsync_OnBreak_ReturnsTrue()
    {
        // Arrange
        var employeeId = 1;
        var attendanceRecord = new AttendanceRecord
        {
            Id = 1,
            EmployeeId = employeeId,
            Date = DateTime.Today,
            Status = AttendanceStatus.OnBreak
        };

        _mockAttendanceRepository
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<AttendanceRecord, bool>>>(),
                It.IsAny<Expression<Func<AttendanceRecord, object>>[]>()))
            .ReturnsAsync(attendanceRecord);

        // Act
        var result = await _attendanceService.IsEmployeeOnBreakAsync(employeeId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CorrectAttendanceAsync_ValidCorrection_ReturnsUpdatedRecord()
    {
        // Arrange
        var attendanceRecordId = 1;
        var correctedBy = 2;
        var newCheckInTime = DateTime.Today.AddHours(9);
        var newCheckOutTime = DateTime.Today.AddHours(17);
        var reason = "Time correction requested by employee";

        var existingRecord = new AttendanceRecord
        {
            Id = attendanceRecordId,
            EmployeeId = 1,
            Date = DateTime.Today,
            CheckInTime = DateTime.Today.AddHours(10),
            CheckOutTime = DateTime.Today.AddHours(18),
            BreakRecords = new List<BreakRecord>
            {
                new BreakRecord
                {
                    Duration = TimeSpan.FromMinutes(30)
                }
            }
        };

        _mockAttendanceRepository
            .Setup(r => r.GetByIdAsync(attendanceRecordId))
            .ReturnsAsync(existingRecord);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _attendanceService.CorrectAttendanceAsync(
            attendanceRecordId, correctedBy, newCheckInTime, newCheckOutTime, reason);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newCheckInTime, result.CheckInTime);
        Assert.Equal(newCheckOutTime, result.CheckOutTime);
        Assert.Equal(reason, result.CorrectionReason);
        Assert.Equal(correctedBy, result.CorrectedBy);
        Assert.True(result.CorrectedAt.HasValue);
        Assert.True(result.TotalWorkingHours.HasValue);

        _mockAttendanceRepository.Verify(r => r.UpdateAsync(It.IsAny<AttendanceRecord>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockAuditLogService.Verify(a => a.LogEventAsync(
            "AttendanceCorrection",
            It.IsAny<string>(),
            correctedBy,
            It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task AddMissingAttendanceAsync_ValidData_ReturnsNewRecord()
    {
        // Arrange
        var employeeId = 1;
        var date = DateTime.Today.AddDays(-1);
        var checkInTime = date.AddHours(9);
        var checkOutTime = date.AddHours(17);
        var addedBy = 2;
        var reason = "Employee forgot to mark attendance";

        _mockAttendanceRepository
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<AttendanceRecord, bool>>>()))
            .ReturnsAsync((AttendanceRecord?)null);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _attendanceService.AddMissingAttendanceAsync(
            employeeId, date, checkInTime, checkOutTime, addedBy, reason);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(date.Date, result.Date);
        Assert.Equal(checkInTime, result.CheckInTime);
        Assert.Equal(checkOutTime, result.CheckOutTime);
        Assert.Equal(reason, result.CorrectionReason);
        Assert.Equal(addedBy, result.CorrectedBy);
        Assert.Equal(AttendanceStatus.Present, result.Status);

        _mockAttendanceRepository.Verify(r => r.AddAsync(It.IsAny<AttendanceRecord>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockAuditLogService.Verify(a => a.LogEventAsync(
            "AttendanceAdded",
            It.IsAny<string>(),
            addedBy,
            It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAttendanceRecordAsync_ValidRecord_ReturnsTrue()
    {
        // Arrange
        var attendanceRecordId = 1;
        var deletedBy = 2;
        var reason = "Duplicate entry";

        var existingRecord = new AttendanceRecord
        {
            Id = attendanceRecordId,
            EmployeeId = 1,
            Date = DateTime.Today
        };

        _mockAttendanceRepository
            .Setup(r => r.GetByIdAsync(attendanceRecordId))
            .ReturnsAsync(existingRecord);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _attendanceService.DeleteAttendanceRecordAsync(attendanceRecordId, deletedBy, reason);

        // Assert
        Assert.True(result);

        _mockAttendanceRepository.Verify(r => r.DeleteAsync(It.IsAny<AttendanceRecord>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockAuditLogService.Verify(a => a.LogEventAsync(
            "AttendanceDeleted",
            It.IsAny<string>(),
            deletedBy,
            It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task GetAverageWorkingHoursAsync_ValidData_ReturnsAverageHours()
    {
        // Arrange
        var employeeId = 1;
        var startDate = DateTime.Today.AddDays(-7);
        var endDate = DateTime.Today;

        var attendanceRecords = new List<AttendanceRecord>
        {
            new AttendanceRecord { TotalWorkingHours = TimeSpan.FromHours(8) },
            new AttendanceRecord { TotalWorkingHours = TimeSpan.FromHours(7.5) },
            new AttendanceRecord { TotalWorkingHours = TimeSpan.FromHours(8.5) }
        };

        _mockAttendanceRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<AttendanceRecord, bool>>>()))
            .ReturnsAsync(attendanceRecords);

        // Act
        var result = await _attendanceService.GetAverageWorkingHoursAsync(employeeId, startDate, endDate);

        // Assert
        Assert.Equal(TimeSpan.FromHours(8), result); // (8 + 7.5 + 8.5) / 3 = 8
    }

    [Fact]
    public async Task GetLateCountAsync_ValidData_ReturnsLateCount()
    {
        // Arrange
        var employeeId = 1;
        var startDate = DateTime.Today.AddDays(-7);
        var endDate = DateTime.Today;

        var lateRecords = new List<AttendanceRecord>
        {
            new AttendanceRecord { IsLate = true },
            new AttendanceRecord { IsLate = true }
        };

        _mockAttendanceRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<AttendanceRecord, bool>>>()))
            .ReturnsAsync(lateRecords);

        // Act
        var result = await _attendanceService.GetLateCountAsync(employeeId, startDate, endDate);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task GetTotalOvertimeAsync_ValidData_ReturnsTotalOvertime()
    {
        // Arrange
        var employeeId = 1;
        var startDate = DateTime.Today.AddDays(-7);
        var endDate = DateTime.Today;

        var overtimeRecords = new List<AttendanceRecord>
        {
            new AttendanceRecord { OvertimeHours = TimeSpan.FromHours(1) },
            new AttendanceRecord { OvertimeHours = TimeSpan.FromHours(0.5) },
            new AttendanceRecord { OvertimeHours = TimeSpan.FromHours(2) }
        };

        _mockAttendanceRepository
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<AttendanceRecord, bool>>>()))
            .ReturnsAsync(overtimeRecords);

        // Act
        var result = await _attendanceService.GetTotalOvertimeAsync(employeeId, startDate, endDate);

        // Assert
        Assert.Equal(TimeSpan.FromHours(3.5), result); // 1 + 0.5 + 2 = 3.5
    }
}