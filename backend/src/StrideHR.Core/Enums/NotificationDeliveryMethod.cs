namespace StrideHR.Core.Enums;

public enum NotificationDeliveryMethod
{
    SignalR = 1,
    WebSocket = 2,
    ServerSentEvents = 3,
    Queue = 4,
    Retry = 5
}