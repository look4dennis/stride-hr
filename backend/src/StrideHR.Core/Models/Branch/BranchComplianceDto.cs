namespace StrideHR.Core.Models.Branch;

public class BranchComplianceDto
{
    public Dictionary<string, object> LaborLaws { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, object> TaxSettings { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, object> StatutoryRequirements { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, object> PayrollSettings { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, object> LeaveSettings { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, object> AdditionalSettings { get; set; } = new Dictionary<string, object>();
}