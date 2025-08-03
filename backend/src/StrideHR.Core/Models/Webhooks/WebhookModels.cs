namespace StrideHR.Core.Models.Webhooks;

public class WebhookSubscription
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public List<string> Events { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int CreatedBy { get; set; }
    
    // Navigation properties
    public virtual List<WebhookDelivery> Deliveries { get; set; } = new();
}

public class WebhookDelivery
{
    public int Id { get; set; }
    public int WebhookSubscriptionId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int HttpStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? ErrorMessage { get; set; }
    public int AttemptCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public WebhookDeliveryStatus Status { get; set; }
    
    // Navigation properties
    public virtual WebhookSubscription WebhookSubscription { get; set; } = null!;
}

public class CreateWebhookSubscriptionDto
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public List<string> Events { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public class UpdateWebhookSubscriptionDto
{
    public string? Name { get; set; }
    public string? Url { get; set; }
    public string? Secret { get; set; }
    public List<string>? Events { get; set; }
    public bool? IsActive { get; set; }
}

public class WebhookTestResult
{
    public bool Success { get; set; }
    public int HttpStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan ResponseTime { get; set; }
}

public enum WebhookDeliveryStatus
{
    Pending,
    Delivered,
    Failed,
    Retrying
}

public static class WebhookEvents
{
    public const string EmployeeCreated = "employee.created";
    public const string EmployeeUpdated = "employee.updated";
    public const string EmployeeDeleted = "employee.deleted";
    
    public const string AttendanceCheckedIn = "attendance.checked_in";
    public const string AttendanceCheckedOut = "attendance.checked_out";
    
    public const string LeaveRequested = "leave.requested";
    public const string LeaveApproved = "leave.approved";
    public const string LeaveRejected = "leave.rejected";
    
    public const string PayrollProcessed = "payroll.processed";
    public const string PayrollApproved = "payroll.approved";
    
    public const string ProjectCreated = "project.created";
    public const string ProjectCompleted = "project.completed";
    
    public const string TaskAssigned = "task.assigned";
    public const string TaskCompleted = "task.completed";
    
    public static List<string> GetAllEvents()
    {
        return new List<string>
        {
            EmployeeCreated, EmployeeUpdated, EmployeeDeleted,
            AttendanceCheckedIn, AttendanceCheckedOut,
            LeaveRequested, LeaveApproved, LeaveRejected,
            PayrollProcessed, PayrollApproved,
            ProjectCreated, ProjectCompleted,
            TaskAssigned, TaskCompleted
        };
    }
}