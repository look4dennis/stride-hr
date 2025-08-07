namespace StrideHR.Core.Enums;

public enum AttendanceAlertType
{
    LateArrival = 1,
    EarlyDeparture = 2,
    MissedCheckIn = 3,
    MissedCheckOut = 4,
    LongBreak = 5,
    Overtime = 6,
    Absence = 7,
    LocationViolation = 8,
    DuplicateEntry = 9,
    SystemError = 10,
    ExcessiveBreakTime = 11,
    ConsecutiveAbsences = 12,
    LowAttendancePercentage = 13,
    OvertimeThreshold = 14,
    UnusualWorkingHours = 15
}