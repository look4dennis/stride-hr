namespace StrideHR.Core.Models.Employee;

public class EmployeeRoleDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string RoleDescription { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public DateTime? RevokedDate { get; set; }
    public int AssignedBy { get; set; }
    public string AssignedByName { get; set; } = string.Empty;
    public int? RevokedBy { get; set; }
    public string? RevokedByName { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class AssignRoleDto
{
    public int EmployeeId { get; set; }
    public int RoleId { get; set; }
    public string? Notes { get; set; }
}

public class RevokeRoleDto
{
    public int EmployeeId { get; set; }
    public int RoleId { get; set; }
    public string? Notes { get; set; }
}