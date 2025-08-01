namespace StrideHR.Core.Entities;

public class ProjectAssignment : BaseEntity
{
    public int ProjectId { get; set; }
    public int EmployeeId { get; set; }
    public bool IsTeamLead { get; set; } = false;
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UnassignedDate { get; set; }
    public string? Role { get; set; }
    public decimal? HourlyRate { get; set; }
    
    // Navigation Properties
    public virtual Project Project { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
}