namespace StrideHR.Infrastructure.DTOs.Setup;

public class SetupCompletionDto
{
    public int OrganizationId { get; set; }
    public int AdminUserId { get; set; }
    public int BranchId { get; set; }
    public DateTime SetupCompletedAt { get; set; }
    public string CompletedBy { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}

public class SetupCompletionRequestDto
{
    public int OrganizationId { get; set; }
    public int AdminUserId { get; set; }
    public int BranchId { get; set; }
}