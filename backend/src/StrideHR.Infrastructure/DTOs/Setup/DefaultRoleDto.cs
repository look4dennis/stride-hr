namespace StrideHR.Infrastructure.DTOs.Setup;

public class DefaultRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int HierarchyLevel { get; set; }
    public bool IsRequired { get; set; }
    public string[] Permissions { get; set; } = Array.Empty<string>();
}