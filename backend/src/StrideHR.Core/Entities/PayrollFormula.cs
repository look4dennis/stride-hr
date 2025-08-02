using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class PayrollFormula : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PayrollFormulaType Type { get; set; }
    public string Formula { get; set; } = string.Empty; // Mathematical expression
    public string Variables { get; set; } = "[]"; // JSON array of variable names
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0; // Execution order
    public string? Conditions { get; set; } // JSON string for conditional logic
    
    // Scope
    public int? OrganizationId { get; set; }
    public int? BranchId { get; set; }
    public string? Department { get; set; }
    public string? Designation { get; set; }
    
    // Navigation Properties
    public virtual Organization? Organization { get; set; }
    public virtual Branch? Branch { get; set; }
}