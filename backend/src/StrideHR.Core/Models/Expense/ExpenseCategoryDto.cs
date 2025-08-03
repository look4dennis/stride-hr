namespace StrideHR.Core.Models.Expense;

public class ExpenseCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool RequiresReceipt { get; set; }
    public decimal? MaxAmount { get; set; }
    public decimal? DailyLimit { get; set; }
    public decimal? MonthlyLimit { get; set; }
    public bool RequiresApproval { get; set; }
    public int? DefaultApprovalLevel { get; set; }
    public bool IsMileageBased { get; set; }
    public decimal? MileageRate { get; set; }
    public string? PolicyDescription { get; set; }
}

public class CreateExpenseCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool RequiresReceipt { get; set; } = true;
    public decimal? MaxAmount { get; set; }
    public decimal? DailyLimit { get; set; }
    public decimal? MonthlyLimit { get; set; }
    public bool RequiresApproval { get; set; } = true;
    public int? DefaultApprovalLevel { get; set; }
    public bool IsMileageBased { get; set; }
    public decimal? MileageRate { get; set; }
    public string? PolicyDescription { get; set; }
}