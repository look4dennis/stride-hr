namespace StrideHR.Core.Enums;

public enum NotificationDeliveryState
{
    Pending = 1,
    Queued = 2,
    Delivering = 3,
    Delivered = 4,
    Read = 5,
    Confirmed = 6,
    Failed = 7,
    Expired = 8,
    Retrying = 9
}