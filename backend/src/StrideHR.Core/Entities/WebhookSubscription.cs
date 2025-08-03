using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class WebhookSubscription : BaseEntity
{
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public string Events { get; set; } = string.Empty; // JSON array of event types
    public bool IsActive { get; set; }
    public int CreatedBy { get; set; }
    
    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual Employee CreatedByEmployee { get; set; } = null!;
    public virtual ICollection<WebhookDelivery> Deliveries { get; set; } = new List<WebhookDelivery>();
}

public class WebhookDelivery : BaseEntity
{
    public int WebhookSubscriptionId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int HttpStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? ErrorMessage { get; set; }
    public int AttemptCount { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public WebhookDeliveryStatus Status { get; set; }
    
    // Navigation properties
    public virtual WebhookSubscription WebhookSubscription { get; set; } = null!;
}