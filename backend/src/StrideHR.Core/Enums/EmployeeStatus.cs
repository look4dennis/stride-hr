namespace StrideHR.Core.Enums;

public enum EmployeeStatus
{
    Active = 1,
    Inactive = 2,
    Terminated = 3,
    OnLeave = 4,
    Probation = 5
}

public enum AttendanceStatus
{
    Present = 1,
    Absent = 2,
    Late = 3,
    OnBreak = 4,
    HalfDay = 5,
    OnLeave = 6
}

public enum BreakType
{
    Tea = 1,
    Lunch = 2,
    Personal = 3,
    Meeting = 4
}

public enum HolidayType
{
    National = 1,
    Regional = 2,
    Religious = 3,
    Company = 4,
    Optional = 5
}