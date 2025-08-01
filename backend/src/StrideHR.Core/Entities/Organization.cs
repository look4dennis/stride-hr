namespace StrideHR.Core.Entities;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public TimeSpan NormalWorkingHours { get; set; } = TimeSpan.FromHours(8);
    public decimal OvertimeRate { get; set; } = 1.5m;
    public int ProductiveHoursThreshold { get; set; } = 6;
    public bool BranchIsolationEnabled { get; set; } = true;
    
    // Navigation Properties
    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
}