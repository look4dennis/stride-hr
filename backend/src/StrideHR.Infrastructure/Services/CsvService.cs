using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.DataImportExport;

namespace StrideHR.Infrastructure.Services;

public class CsvService : ICsvService
{
    private readonly ILogger<CsvService> _logger;

    public CsvService(ILogger<CsvService> logger)
    {
        _logger = logger;
    }

    public async Task<List<Dictionary<string, object>>> ReadCsvFileAsync(Stream fileStream, string fileName, string delimiter = ",")
    {
        try
        {
            var data = new List<Dictionary<string, object>>();
            
            using var reader = new StreamReader(fileStream);
            var content = await reader.ReadToEndAsync();
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length == 0)
                return data;

            // Parse headers
            var headers = ParseCsvLine(lines[0], delimiter);

            // Parse data rows
            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseCsvLine(lines[i], delimiter);
                if (values.Count == 0) continue;

                var rowData = new Dictionary<string, object>();
                for (int j = 0; j < Math.Min(headers.Count, values.Count); j++)
                {
                    rowData[headers[j]] = values[j];
                }

                data.Add(rowData);
            }

            _logger.LogInformation("Successfully read {Count} rows from CSV file {FileName}", data.Count, fileName);
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading CSV file {FileName}", fileName);
            throw new InvalidOperationException($"Failed to read CSV file: {ex.Message}", ex);
        }
    }

    public async Task<List<T>> ReadCsvFileAsync<T>(Stream fileStream, string fileName, Dictionary<string, string>? fieldMappings = null, string delimiter = ",") where T : class, new()
    {
        var rawData = await ReadCsvFileAsync(fileStream, fileName, delimiter);
        return MapDataToObjects<T>(rawData, fieldMappings);
    }

    public async Task<byte[]> WriteToCsvAsync<T>(IEnumerable<T> data, List<string>? selectedFields = null, string delimiter = ",")
    {
        try
        {
            var sb = new StringBuilder();
            var properties = typeof(T).GetProperties();
            
            if (selectedFields != null && selectedFields.Any())
            {
                properties = properties.Where(p => selectedFields.Contains(p.Name)).ToArray();
            }

            // Write headers
            var headers = properties.Select(p => 
                p.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? p.Name);
            sb.AppendLine(string.Join(delimiter, headers.Select(EscapeCsvValue)));

            // Write data
            foreach (var item in data)
            {
                var values = properties.Select(p => p.GetValue(item)?.ToString() ?? "");
                sb.AppendLine(string.Join(delimiter, values.Select(EscapeCsvValue)));
            }

            var result = Encoding.UTF8.GetBytes(sb.ToString());
            _logger.LogInformation("Successfully created CSV file with {Count} rows", data.Count());
            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating CSV file");
            throw new InvalidOperationException($"Failed to create CSV file: {ex.Message}", ex);
        }
    }

    public async Task<byte[]> WriteToCsvAsync(List<Dictionary<string, object>> data, List<string>? selectedFields = null, string delimiter = ",")
    {
        try
        {
            var sb = new StringBuilder();

            if (!data.Any())
                return await Task.FromResult(Encoding.UTF8.GetBytes(""));

            var headers = selectedFields ?? data.First().Keys.ToList();

            // Write headers
            sb.AppendLine(string.Join(delimiter, headers.Select(EscapeCsvValue)));

            // Write data
            foreach (var row in data)
            {
                var values = headers.Select(h => row.ContainsKey(h) ? row[h]?.ToString() ?? "" : "");
                sb.AppendLine(string.Join(delimiter, values.Select(EscapeCsvValue)));
            }

            var result = Encoding.UTF8.GetBytes(sb.ToString());
            _logger.LogInformation("Successfully created CSV file with {Count} rows", data.Count);
            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating CSV file");
            throw new InvalidOperationException($"Failed to create CSV file: {ex.Message}", ex);
        }
    }

    public async Task<byte[]> GenerateTemplateAsync(Type entityType, string delimiter = ",")
    {
        try
        {
            var sb = new StringBuilder();
            var properties = entityType.GetProperties()
                .Where(p => p.CanWrite && (!p.PropertyType.IsClass || p.PropertyType == typeof(string)))
                .ToArray();

            // Write headers with type information
            var headers = properties.Select(p => 
                $"{p.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? p.Name} ({p.PropertyType.Name})");
            sb.AppendLine(string.Join(delimiter, headers.Select(EscapeCsvValue)));

            var result = Encoding.UTF8.GetBytes(sb.ToString());
            _logger.LogInformation("Successfully generated CSV template for {EntityType}", entityType.Name);
            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CSV template for {EntityType}", entityType.Name);
            throw new InvalidOperationException($"Failed to generate CSV template: {ex.Message}", ex);
        }
    }

    public async Task<byte[]> GenerateTemplateAsync(Dictionary<string, Type> fieldTypes, string delimiter = ",")
    {
        try
        {
            var sb = new StringBuilder();
            var headers = fieldTypes.Select(kvp => $"{kvp.Key} ({kvp.Value.Name})");
            sb.AppendLine(string.Join(delimiter, headers.Select(EscapeCsvValue)));

            var result = Encoding.UTF8.GetBytes(sb.ToString());
            _logger.LogInformation("Successfully generated CSV template with {Count} fields", fieldTypes.Count);
            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CSV template");
            throw new InvalidOperationException($"Failed to generate CSV template: {ex.Message}", ex);
        }
    }

    public ValidationResultDto ValidateCsvData<T>(List<Dictionary<string, object>> data, Dictionary<string, string>? fieldMappings = null) where T : class, new()
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
            var rowNumber = i + 2; // CSV rows start from 1, and first row is header

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

    public List<string> GetColumnNames(Stream fileStream, string fileName, string delimiter = ",")
    {
        try
        {
            using var reader = new StreamReader(fileStream);
            var firstLine = reader.ReadLine();
            
            if (string.IsNullOrEmpty(firstLine))
                return new List<string>();

            return ParseCsvLine(firstLine, delimiter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading column names from CSV file {FileName}", fileName);
            throw new InvalidOperationException($"Failed to read column names: {ex.Message}", ex);
        }
    }

    public bool IsValidCsvFile(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension == ".csv" || extension == ".txt";
    }

    private List<string> ParseCsvLine(string line, string delimiter)
    {
        var result = new List<string>();
        var inQuotes = false;
        var currentField = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    // Escaped quote
                    currentField.Append('"');
                    i++; // Skip next quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c.ToString() == delimiter && !inQuotes)
            {
                result.Add(currentField.ToString().Trim());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        result.Add(currentField.ToString().Trim());
        return result;
    }

    private string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
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
                        var convertedValue = Convert.ChangeType(value, property.PropertyType, CultureInfo.InvariantCulture);
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
            Convert.ChangeType(value, property.PropertyType, CultureInfo.InvariantCulture);
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