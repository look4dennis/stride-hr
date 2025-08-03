using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Models;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.DataImportExport;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DataImportExportController : BaseController
{
    private readonly IDataImportExportService _dataImportExportService;
    private readonly ILogger<DataImportExportController> _logger;

    public DataImportExportController(
        IDataImportExportService dataImportExportService,
        ILogger<DataImportExportController> logger)
    {
        _dataImportExportService = dataImportExportService;
        _logger = logger;
    }

    #region Import Operations

    /// <summary>
    /// Validate import data without actually importing
    /// </summary>
    [HttpPost("validate")]
    public async Task<ActionResult<ApiResponse<ValidationResultDto>>> ValidateImportData([FromForm] ImportRequestApiDto apiRequest)
    {
        try
        {
            if (apiRequest.File == null || apiRequest.File.Length == 0)
            {
                return BadRequest(ApiResponse<ValidationResultDto>.CreateFailure("No file provided"));
            }

            var request = new ImportRequestDto
            {
                EntityType = apiRequest.EntityType,
                FileStream = apiRequest.File.OpenReadStream(),
                FileName = apiRequest.File.FileName,
                BranchId = apiRequest.BranchId,
                ValidateOnly = apiRequest.ValidateOnly,
                UpdateExisting = apiRequest.UpdateExisting,
                FieldMappings = apiRequest.FieldMappings,
                DefaultValues = apiRequest.DefaultValues
            };

            var result = await _dataImportExportService.ValidateImportDataAsync(request);
            return Ok(ApiResponse<ValidationResultDto>.CreateSuccess(result, "Data validation completed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating import data for entity type {EntityType}", apiRequest.EntityType);
            return StatusCode(500, ApiResponse<ValidationResultDto>.CreateFailure($"Validation failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Import data from Excel or CSV file
    /// </summary>
    [HttpPost("import")]
    public async Task<ActionResult<ApiResponse<ImportResultDto>>> ImportData([FromForm] ImportRequestApiDto apiRequest)
    {
        try
        {
            if (apiRequest.File == null || apiRequest.File.Length == 0)
            {
                return BadRequest(ApiResponse<ImportResultDto>.CreateFailure("No file provided"));
            }

            var request = new ImportRequestDto
            {
                EntityType = apiRequest.EntityType,
                FileStream = apiRequest.File.OpenReadStream(),
                FileName = apiRequest.File.FileName,
                BranchId = apiRequest.BranchId,
                ValidateOnly = apiRequest.ValidateOnly,
                UpdateExisting = apiRequest.UpdateExisting,
                FieldMappings = apiRequest.FieldMappings,
                DefaultValues = apiRequest.DefaultValues
            };

            var result = await _dataImportExportService.ImportDataAsync(request);
            
            if (result.Success)
            {
                return Ok(ApiResponse<ImportResultDto>.CreateSuccess(result, result.Message ?? "Import completed successfully"));
            }
            else
            {
                return BadRequest(ApiResponse<ImportResultDto>.CreateFailure(result.Message ?? "Import failed"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing data for entity type {EntityType}", apiRequest.EntityType);
            return StatusCode(500, ApiResponse<ImportResultDto>.CreateFailure($"Import failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Import employees from Excel or CSV file
    /// </summary>
    [HttpPost("import/employees")]
    public async Task<ActionResult<ApiResponse<ImportResultDto>>> ImportEmployees([FromForm] ImportRequestApiDto apiRequest)
    {
        try
        {
            if (apiRequest.File == null || apiRequest.File.Length == 0)
            {
                return BadRequest(ApiResponse<ImportResultDto>.CreateFailure("No file provided"));
            }

            var request = new ImportRequestDto
            {
                EntityType = "Employee",
                FileStream = apiRequest.File.OpenReadStream(),
                FileName = apiRequest.File.FileName,
                BranchId = apiRequest.BranchId,
                ValidateOnly = apiRequest.ValidateOnly,
                UpdateExisting = apiRequest.UpdateExisting,
                FieldMappings = apiRequest.FieldMappings,
                DefaultValues = apiRequest.DefaultValues
            };

            var result = await _dataImportExportService.ImportEmployeesAsync(request);
            
            if (result.Success)
            {
                return Ok(ApiResponse<ImportResultDto>.CreateSuccess(result, result.Message ?? "Employee import completed successfully"));
            }
            else
            {
                return BadRequest(ApiResponse<ImportResultDto>.CreateFailure(result.Message ?? "Employee import failed"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing employees");
            return StatusCode(500, ApiResponse<ImportResultDto>.CreateFailure($"Employee import failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Import attendance records from Excel or CSV file
    /// </summary>
    [HttpPost("import/attendance")]
    public async Task<ActionResult<ApiResponse<ImportResultDto>>> ImportAttendance([FromForm] ImportRequestApiDto apiRequest)
    {
        try
        {
            if (apiRequest.File == null || apiRequest.File.Length == 0)
            {
                return BadRequest(ApiResponse<ImportResultDto>.CreateFailure("No file provided"));
            }

            var request = new ImportRequestDto
            {
                EntityType = "AttendanceRecord",
                FileStream = apiRequest.File.OpenReadStream(),
                FileName = apiRequest.File.FileName,
                BranchId = apiRequest.BranchId,
                ValidateOnly = apiRequest.ValidateOnly,
                UpdateExisting = apiRequest.UpdateExisting,
                FieldMappings = apiRequest.FieldMappings,
                DefaultValues = apiRequest.DefaultValues
            };

            var result = await _dataImportExportService.ImportAttendanceAsync(request);
            
            if (result.Success)
            {
                return Ok(ApiResponse<ImportResultDto>.CreateSuccess(result, result.Message ?? "Attendance import completed successfully"));
            }
            else
            {
                return BadRequest(ApiResponse<ImportResultDto>.CreateFailure(result.Message ?? "Attendance import failed"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing attendance records");
            return StatusCode(500, ApiResponse<ImportResultDto>.CreateFailure($"Attendance import failed: {ex.Message}"));
        }
    }

    #endregion

    #region Export Operations

    /// <summary>
    /// Export data to Excel or CSV format
    /// </summary>
    [HttpPost("export")]
    public async Task<ActionResult> ExportData([FromBody] ExportRequestDto request)
    {
        try
        {
            var result = await _dataImportExportService.ExportDataAsync(request);
            
            if (result.Success && result.FileContent != null)
            {
                return File(result.FileContent, result.ContentType, result.FileName);
            }
            else
            {
                return BadRequest(ApiResponse<object>.CreateFailure(result.Message ?? "Export failed"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting data for entity type {EntityType}", request.EntityType);
            return StatusCode(500, ApiResponse<object>.CreateFailure($"Export failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Export employees to Excel or CSV format
    /// </summary>
    [HttpPost("export/employees")]
    public async Task<ActionResult> ExportEmployees([FromBody] ExportRequestDto request)
    {
        try
        {
            request.EntityType = "Employee";
            var result = await _dataImportExportService.ExportEmployeesAsync(request);
            
            if (result.Success && result.FileContent != null)
            {
                return File(result.FileContent, result.ContentType, result.FileName);
            }
            else
            {
                return BadRequest(ApiResponse<object>.CreateFailure(result.Message ?? "Employee export failed"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting employees");
            return StatusCode(500, ApiResponse<object>.CreateFailure($"Employee export failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Export attendance records to Excel or CSV format
    /// </summary>
    [HttpPost("export/attendance")]
    public async Task<ActionResult> ExportAttendance([FromBody] ExportRequestDto request)
    {
        try
        {
            request.EntityType = "AttendanceRecord";
            var result = await _dataImportExportService.ExportAttendanceAsync(request);
            
            if (result.Success && result.FileContent != null)
            {
                return File(result.FileContent, result.ContentType, result.FileName);
            }
            else
            {
                return BadRequest(ApiResponse<object>.CreateFailure(result.Message ?? "Attendance export failed"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting attendance records");
            return StatusCode(500, ApiResponse<object>.CreateFailure($"Attendance export failed: {ex.Message}"));
        }
    }

    #endregion

    #region Data Migration Operations

    /// <summary>
    /// Migrate data between different entity types or systems
    /// </summary>
    [HttpPost("migrate")]
    public async Task<ActionResult<ApiResponse<ImportResultDto>>> MigrateData([FromBody] DataMigrationRequestDto request)
    {
        try
        {
            var result = await _dataImportExportService.MigrateDataAsync(request);
            
            if (result.Success)
            {
                return Ok(ApiResponse<ImportResultDto>.CreateSuccess(result, result.Message ?? "Data migration completed successfully"));
            }
            else
            {
                return BadRequest(ApiResponse<ImportResultDto>.CreateFailure(result.Message ?? "Data migration failed"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error migrating data from {SourceType} to {TargetType}", 
                request.SourceEntityType, request.TargetEntityType);
            return StatusCode(500, ApiResponse<ImportResultDto>.CreateFailure($"Data migration failed: {ex.Message}"));
        }
    }

    #endregion

    #region Utility Operations

    /// <summary>
    /// Generate import template for a specific entity type
    /// </summary>
    [HttpGet("template/{entityType}")]
    public async Task<ActionResult> GenerateImportTemplate(string entityType)
    {
        try
        {
            var templateContent = await _dataImportExportService.GenerateImportTemplateAsync(entityType);
            var fileName = $"{entityType}_Import_Template.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            
            return File(templateContent, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating import template for entity type {EntityType}", entityType);
            return StatusCode(500, ApiResponse<object>.CreateFailure($"Template generation failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get list of supported entity types for import/export
    /// </summary>
    [HttpGet("supported-entities")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetSupportedEntityTypes()
    {
        try
        {
            var entityTypes = await _dataImportExportService.GetSupportedEntityTypesAsync();
            return Ok(ApiResponse<List<string>>.CreateSuccess(entityTypes, "Supported entity types retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supported entity types");
            return StatusCode(500, ApiResponse<List<string>>.CreateFailure($"Failed to get supported entity types: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get field mappings for a specific entity type
    /// </summary>
    [HttpGet("field-mappings/{entityType}")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, string>>>> GetEntityFieldMappings(string entityType)
    {
        try
        {
            var fieldMappings = await _dataImportExportService.GetEntityFieldMappingsAsync(entityType);
            return Ok(ApiResponse<Dictionary<string, string>>.CreateSuccess(fieldMappings, 
                $"Field mappings for {entityType} retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting field mappings for entity type {EntityType}", entityType);
            return StatusCode(500, ApiResponse<Dictionary<string, string>>.CreateFailure(
                $"Failed to get field mappings: {ex.Message}"));
        }
    }

    #endregion
}