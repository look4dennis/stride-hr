namespace StrideHR.Core.Enums;

[Flags]
public enum NotificationChannel
{
    None = 0,
    InApp = 1,
    Email = 2,
    SMS = 4,
    Push = 8,
    WhatsApp = 16,
    All = InApp | Email | SMS | Push | WhatsApp
}