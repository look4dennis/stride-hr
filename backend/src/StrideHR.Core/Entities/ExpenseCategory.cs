using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class ExpenseCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool RequiresReceipt { get; set; } = true;
    public decimal? MaxAmount { get; set; }
    public decimal? DailyLimit { get; set; }
    public decimal? MonthlyLimit { get; set; }
    public bool RequiresApproval { get; set; } = true;
    public int? DefaultApprovalLevel { get; set; }
    public bool IsMileageBased { get; set; }
    public decimal? MileageRate { get; set; }
    public string? PolicyDescription { get; set; }
    public int OrganizationId { get; set; }

    // Navigation Properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<ExpenseItem> ExpenseItems { get; set; } = new List<ExpenseItem>();
    public virtual ICollection<ExpensePolicyRule> PolicyRules { get; set; } = new List<ExpensePolicyRule>();
}