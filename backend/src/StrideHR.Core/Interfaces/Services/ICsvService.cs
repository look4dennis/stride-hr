using StrideHR.Core.Models.DataImportExport;

namespace StrideHR.Core.Interfaces.Services;

public interface ICsvService
{
    // CSV Reading Operations
    Task<List<Dictionary<string, object>>> ReadCsvFileAsync(Stream fileStream, string fileName, string delimiter = ",");
    Task<List<T>> ReadCsvFileAsync<T>(Stream fileStream, string fileName, Dictionary<string, string>? fieldMappings = null, string delimiter = ",") where T : class, new();
    
    // CSV Writing Operations
    Task<byte[]> WriteToCsvAsync<T>(IEnumerable<T> data, List<string>? selectedFields = null, string delimiter = ",");
    Task<byte[]> WriteToCsvAsync(List<Dictionary<string, object>> data, List<string>? selectedFields = null, string delimiter = ",");
    
    // Template Generation
    Task<byte[]> GenerateTemplateAsync(Type entityType, string delimiter = ",");
    Task<byte[]> GenerateTemplateAsync(Dictionary<string, Type> fieldTypes, string delimiter = ",");
    
    // Validation
    ValidationResultDto ValidateCsvData<T>(List<Dictionary<string, object>> data, Dictionary<string, string>? fieldMappings = null) where T : class, new();
    
    // Utility Methods
    List<string> GetColumnNames(Stream fileStream, string fileName, string delimiter = ",");
    bool IsValidCsvFile(string fileName);
}