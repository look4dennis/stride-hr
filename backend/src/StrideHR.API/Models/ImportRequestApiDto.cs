using Microsoft.AspNetCore.Http;

namespace StrideHR.API.Models;

public class ImportRequestApiDto
{
    public string EntityType { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
    public int? BranchId { get; set; }
    public bool ValidateOnly { get; set; } = false;
    public bool UpdateExisting { get; set; } = false;
    public Dictionary<string, string> FieldMappings { get; set; } = new();
    public Dictionary<string, object> DefaultValues { get; set; } = new();
}