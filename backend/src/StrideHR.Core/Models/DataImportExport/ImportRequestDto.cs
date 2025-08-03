namespace StrideHR.Core.Models.DataImportExport;

public class ImportRequestDto
{
    public string EntityType { get; set; } = string.Empty;
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public bool ValidateOnly { get; set; } = false;
    public bool UpdateExisting { get; set; } = false;
    public Dictionary<string, string> FieldMappings { get; set; } = new();
    public Dictionary<string, object> DefaultValues { get; set; } = new();
}

public class DataMigrationRequestDto
{
    public string SourceEntityType { get; set; } = string.Empty;
    public string TargetEntityType { get; set; } = string.Empty;
    public Dictionary<string, string> FieldMappings { get; set; } = new();
    public Dictionary<string, object> TransformationRules { get; set; } = new();
    public int? SourceBranchId { get; set; }
    public int? TargetBranchId { get; set; }
    public bool ValidateOnly { get; set; } = false;
}