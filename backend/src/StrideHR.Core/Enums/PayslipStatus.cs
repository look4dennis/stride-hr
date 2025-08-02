namespace StrideHR.Core.Enums;

public enum PayslipStatus
{
    Generated = 0,
    PendingHRApproval = 1,
    HRApproved = 2,
    PendingFinanceApproval = 3,
    FinanceApproved = 4,
    Released = 5,
    HRRejected = 6,
    FinanceRejected = 7,
    Cancelled = 8
}