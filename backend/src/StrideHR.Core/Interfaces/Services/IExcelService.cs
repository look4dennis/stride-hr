using StrideHR.Core.Models.DataImportExport;

namespace StrideHR.Core.Interfaces.Services;

public interface IExcelService
{
    // Excel Reading Operations
    Task<List<Dictionary<string, object>>> ReadExcelFileAsync(Stream fileStream, string fileName);
    Task<List<T>> ReadExcelFileAsync<T>(Stream fileStream, string fileName, Dictionary<string, string>? fieldMappings = null) where T : class, new();
    
    // Excel Writing Operations
    Task<byte[]> WriteToExcelAsync<T>(IEnumerable<T> data, string sheetName = "Sheet1", List<string>? selectedFields = null);
    Task<byte[]> WriteToExcelAsync(List<Dictionary<string, object>> data, string sheetName = "Sheet1", List<string>? selectedFields = null);
    
    // Template Generation
    Task<byte[]> GenerateTemplateAsync(Type entityType, string sheetName = "Template");
    Task<byte[]> GenerateTemplateAsync(Dictionary<string, Type> fieldTypes, string sheetName = "Template");
    
    // Validation
    ValidationResultDto ValidateExcelData<T>(List<Dictionary<string, object>> data, Dictionary<string, string>? fieldMappings = null) where T : class, new();
    
    // Utility Methods
    List<string> GetColumnNames(Stream fileStream, string fileName);
    bool IsValidExcelFile(string fileName);
}