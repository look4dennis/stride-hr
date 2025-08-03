using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.DataImportExport;

namespace StrideHR.Infrastructure.Services;

public class ExcelService : IExcelService
{
    private readonly ILogger<ExcelService> _logger;

    public ExcelService(ILogger<ExcelService> logger)
    {
        _logger = logger;
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
    }

    public async Task<List<Dictionary<string, object>>> ReadExcelFileAsync(Stream fileStream, string fileName)
    {
        try
        {
            var data = new List<Dictionary<string, object>>();
            
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];
            
            if (worksheet.Dimension == null)
                return data;

            var rowCount = worksheet.Dimension.Rows;
            var colCount = worksheet.Dimension.Columns;

            // Get headers from first row
            var headers = new List<string>();
            for (int col = 1; col <= colCount; col++)
            {
                var header = worksheet.Cells[1, col].Value?.ToString() ?? $"Column{col}";
                headers.Add(header);
            }

            // Read data rows
            for (int row = 2; row <= rowCount; row++)
            {
                var rowData = new Dictionary<string, object>();
                bool hasData = false;

                for (int col = 1; col <= colCount; col++)
                {
                    var cellValue = worksheet.Cells[row, col].Value;
                    if (cellValue != null)
                    {
                        hasData = true;
                        rowData[headers[col - 1]] = cellValue;
                    }
                    else
                    {
                        rowData[headers[col - 1]] = string.Empty;
                    }
                }

                if (hasData)
                    data.Add(rowData);
            }

            _logger.LogInformation("Successfully read {Count} rows from Excel file {FileName}", data.Count, fileName);
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading Excel file {FileName}", fileName);
            throw new InvalidOperationException($"Failed to read Excel file: {ex.Message}", ex);
        }
    }

    public async Task<List<T>> ReadExcelFileAsync<T>(Stream fileStream, string fileName, Dictionary<string, string>? fieldMappings = null) where T : class, new()
    {
        var rawData = await ReadExcelFileAsync(fileStream, fileName);
        return MapDataToObjects<T>(rawData, fieldMappings);
    }

    public async Task<byte[]> WriteToExcelAsync<T>(IEnumerable<T> data, string sheetName = "Sheet1", List<string>? selectedFields = null)
    {
        try
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            var properties = typeof(T).GetProperties();
            if (selectedFields != null && selectedFields.Any())
            {
                properties = properties.Where(p => selectedFields.Contains(p.Name)).ToArray();
            }

            // Write headers
            for (int i = 0; i < properties.Length; i++)
            {
                var displayName = properties[i].GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? properties[i].Name;
                worksheet.Cells[1, i + 1].Value = displayName;
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            // Write data
            var dataList = data.ToList();
            for (int row = 0; row < dataList.Count; row++)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    var value = properties[col].GetValue(dataList[row]);
                    worksheet.Cells[row + 2, col + 1].Value = value;
                }
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            _logger.LogInformation("Successfully created Excel file with {Count} rows", dataList.Count);
            return await Task.FromResult(package.GetAsByteArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Excel file");
            throw new InvalidOperationException($"Failed to create Excel file: {ex.Message}", ex);
        }
    }

    public async Task<byte[]> WriteToExcelAsync(List<Dictionary<string, object>> data, string sheetName = "Sheet1", List<string>? selectedFields = null)
    {
        try
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            if (!data.Any())
                return await Task.FromResult(package.GetAsByteArray());

            var headers = selectedFields ?? data.First().Keys.ToList();

            // Write headers
            for (int i = 0; i < headers.Count; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            // Write data
            for (int row = 0; row < data.Count; row++)
            {
                for (int col = 0; col < headers.Count; col++)
                {
                    var key = headers[col];
                    var value = data[row].ContainsKey(key) ? data[row][key] : null;
                    worksheet.Cells[row + 2, col + 1].Value = value;
                }
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            _logger.LogInformation("Successfully created Excel file with {Count} rows", data.Count);
            return await Task.FromResult(package.GetAsByteArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Excel file");
            throw new InvalidOperationException($"Failed to create Excel file: {ex.Message}", ex);
        }
    }

    public async Task<byte[]> GenerateTemplateAsync(Type entityType, string sheetName = "Template")
    {
        try
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            var properties = entityType.GetProperties()
                .Where(p => p.CanWrite && !p.PropertyType.IsClass || p.PropertyType == typeof(string))
                .ToArray();

            // Write headers
            for (int i = 0; i < properties.Length; i++)
            {
                var displayName = properties[i].GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? properties[i].Name;
                worksheet.Cells[1, i + 1].Value = displayName;
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                
                // Add data validation comments
                var comment = worksheet.Cells[1, i + 1].AddComment($"Type: {properties[i].PropertyType.Name}", "System");
                comment.AutoFit = true;
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            _logger.LogInformation("Successfully generated template for {EntityType}", entityType.Name);
            return await Task.FromResult(package.GetAsByteArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating template for {EntityType}", entityType.Name);
            throw new InvalidOperationException($"Failed to generate template: {ex.Message}", ex);
        }
    }

    public async Task<byte[]> GenerateTemplateAsync(Dictionary<string, Type> fieldTypes, string sheetName = "Template")
    {
        try
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            var fields = fieldTypes.Keys.ToList();

            // Write headers
            for (int i = 0; i < fields.Count; i++)
            {
                worksheet.Cells[1, i + 1].Value = fields[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                
                // Add data validation comments
                var fieldType = fieldTypes[fields[i]];
                var comment = worksheet.Cells[1, i + 1].AddComment($"Type: {fieldType.Name}", "System");
                comment.AutoFit = true;
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            _logger.LogInformation("Successfully generated template with {Count} fields", fields.Count);
            return await Task.FromResult(package.GetAsByteArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating template");
            throw new InvalidOperationException($"Failed to generate template: {ex.Message}", ex);
        }
    }

    public ValidationResultDto ValidateExcelData<T>(List<Dictionary<string, object>> data, Dictionary<string, string>? fieldMappings = null) where T : class, new()
    {
        var result = new ValidationResultDto
        {
            TotalRecords = data.Count
        };

        var properties = typeof(T).GetProperties();
        var mappings = fieldMappings ?? properties.ToDictionary(p => p.Name, p => p.Name);

        for (int i = 0; i < data.Count; i++)
        {
            var row = data[i];
            var rowNumber = i + 2; // Excel rows start from 1, and first row is header

            foreach (var property in properties)
            {
                if (!mappings.ContainsKey(property.Name))
                    continue;

                var columnName = mappings[property.Name];
                if (!row.ContainsKey(columnName))
                {
                    result.Errors.Add(new ValidationErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = property.Name,
                        Value = "",
                        ErrorMessage = $"Required field '{columnName}' is missing",
                        ErrorCode = "MISSING_FIELD"
                    });
                    continue;
                }

                var value = row[columnName];
                var validationError = ValidateFieldValue(property, value, rowNumber);
                if (validationError != null)
                {
                    result.Errors.Add(validationError);
                }
            }
        }

        result.ValidRecords = result.TotalRecords - result.Errors.Count;
        result.InvalidRecords = result.Errors.Count;
        result.IsValid = result.Errors.Count == 0;

        return result;
    }

    public List<string> GetColumnNames(Stream fileStream, string fileName)
    {
        try
        {
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets[0];
            
            if (worksheet.Dimension == null)
                return new List<string>();

            var colCount = worksheet.Dimension.Columns;
            var headers = new List<string>();

            for (int col = 1; col <= colCount; col++)
            {
                var header = worksheet.Cells[1, col].Value?.ToString() ?? $"Column{col}";
                headers.Add(header);
            }

            return headers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading column names from Excel file {FileName}", fileName);
            throw new InvalidOperationException($"Failed to read column names: {ex.Message}", ex);
        }
    }

    public bool IsValidExcelFile(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension == ".xlsx" || extension == ".xls";
    }

    private List<T> MapDataToObjects<T>(List<Dictionary<string, object>> data, Dictionary<string, string>? fieldMappings) where T : class, new()
    {
        var result = new List<T>();
        var properties = typeof(T).GetProperties();
        var mappings = fieldMappings ?? properties.ToDictionary(p => p.Name, p => p.Name);

        foreach (var row in data)
        {
            var obj = new T();
            
            foreach (var property in properties)
            {
                if (!mappings.ContainsKey(property.Name) || !row.ContainsKey(mappings[property.Name]))
                    continue;

                var value = row[mappings[property.Name]];
                if (value != null)
                {
                    try
                    {
                        var convertedValue = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(obj, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to convert value {Value} to type {Type} for property {Property}", 
                            value, property.PropertyType.Name, property.Name);
                    }
                }
            }
            
            result.Add(obj);
        }

        return result;
    }

    private ValidationErrorDto? ValidateFieldValue(PropertyInfo property, object? value, int rowNumber)
    {
        try
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                if (!IsNullable(property.PropertyType))
                {
                    return new ValidationErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = property.Name,
                        Value = value?.ToString() ?? "",
                        ErrorMessage = $"Field '{property.Name}' is required",
                        ErrorCode = "REQUIRED_FIELD"
                    };
                }
                return null;
            }

            // Try to convert the value to the property type
            Convert.ChangeType(value, property.PropertyType);
            return null;
        }
        catch (Exception)
        {
            return new ValidationErrorDto
            {
                RowNumber = rowNumber,
                Field = property.Name,
                Value = value?.ToString() ?? "",
                ErrorMessage = $"Invalid value for field '{property.Name}'. Expected type: {property.PropertyType.Name}",
                ErrorCode = "INVALID_TYPE"
            };
        }
    }

    private bool IsNullable(Type type)
    {
        return Nullable.GetUnderlyingType(type) != null || !type.IsValueType;
    }
}