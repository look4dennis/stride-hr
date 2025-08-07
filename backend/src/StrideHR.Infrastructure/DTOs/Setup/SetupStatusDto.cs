namespace StrideHR.Infrastructure.DTOs.Setup;

public class SetupStatusDto
{
    public bool IsSetupComplete { get; set; }
    public bool HasOrganization { get; set; }
    public bool HasAdminUser { get; set; }
    public bool HasBranches { get; set; }
    public bool HasRoles { get; set; }
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
}