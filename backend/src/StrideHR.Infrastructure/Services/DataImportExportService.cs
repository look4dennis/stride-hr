using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.DataImportExport;

namespace StrideHR.Infrastructure.Services;

public class DataImportExportService : IDataImportExportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExcelService _excelService;
    private readonly ICsvService _csvService;
    private readonly ILogger<DataImportExportService> _logger;

    private readonly Dictionary<string, Type> _supportedEntities = new()
    {
        { "Employee", typeof(Employee) },
        { "AttendanceRecord", typeof(AttendanceRecord) },
        { "LeaveRequest", typeof(LeaveRequest) },
        { "Project", typeof(Project) },
        { "Branch", typeof(Branch) },
        { "Organization", typeof(Organization) }
    };

    public DataImportExportService(
        IUnitOfWork unitOfWork,
        IExcelService excelService,
        ICsvService csvService,
        ILogger<DataImportExportService> logger)
    {
        _unitOfWork = unitOfWork;
        _excelService = excelService;
        _csvService = csvService;
        _logger = logger;
    }

    #region Import Operations

    public async Task<ValidationResultDto> ValidateImportDataAsync(ImportRequestDto request)
    {
        try
        {
            if (!_supportedEntities.ContainsKey(request.EntityType))
            {
                throw new ArgumentException($"Entity type '{request.EntityType}' is not supported");
            }

            var entityType = _supportedEntities[request.EntityType];
            List<Dictionary<string, object>> data;

            if (IsExcelFile(request.FileName))
            {
                data = await _excelService.ReadExcelFileAsync(request.FileStream, request.FileName);
                return ValidateDataByEntityType(entityType, data, request.FieldMappings, true);
            }
            else if (IsCsvFile(request.FileName))
            {
                data = await _csvService.ReadCsvFileAsync(request.FileStream, request.FileName);
                return ValidateDataByEntityType(entityType, data, request.FieldMappings, false);
            }
            else
            {
                throw new ArgumentException("Unsupported file format. Only Excel (.xlsx, .xls) and CSV (.csv) files are supported.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating import data for entity type {EntityType}", request.EntityType);
            throw;
        }
    }

    public async Task<ImportResultDto> ImportDataAsync(ImportRequestDto request)
    {
        try
        {
            // First validate the data
            var validationResult = await ValidateImportDataAsync(request);
            
            if (!validationResult.IsValid && !request.ValidateOnly)
            {
                return new ImportResultDto
                {
                    Success = false,
                    Message = "Data validation failed. Please fix the errors and try again.",
                    Errors = validationResult.Errors.Select(e => new ImportErrorDto
                    {
                        RowNumber = e.RowNumber,
                        Field = e.Field,
                        Value = e.Value,
                        ErrorMessage = e.ErrorMessage,
                        ErrorType = e.ErrorCode
                    }).ToList()
                };
            }

            if (request.ValidateOnly)
            {
                return new ImportResultDto
                {
                    Success = validationResult.IsValid,
                    TotalRecords = validationResult.TotalRecords,
                    SuccessfulRecords = validationResult.ValidRecords,
                    FailedRecords = validationResult.InvalidRecords,
                    Message = validationResult.IsValid ? "Validation successful" : "Validation failed",
                    Errors = validationResult.Errors.Select(e => new ImportErrorDto
                    {
                        RowNumber = e.RowNumber,
                        Field = e.Field,
                        Value = e.Value,
                        ErrorMessage = e.ErrorMessage,
                        ErrorType = e.ErrorCode
                    }).ToList()
                };
            }

            // Route to specific import method based on entity type
            return request.EntityType switch
            {
                "Employee" => await ImportEmployeesAsync(request),
                "AttendanceRecord" => await ImportAttendanceAsync(request),
                "LeaveRequest" => await ImportLeaveRequestsAsync(request),
                "Project" => await ImportProjectsAsync(request),
                _ => throw new ArgumentException($"Import not implemented for entity type '{request.EntityType}'")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing data for entity type {EntityType}", request.EntityType);
            return new ImportResultDto
            {
                Success = false,
                Message = $"Import failed: {ex.Message}"
            };
        }
    }

    public async Task<ImportResultDto> ImportEmployeesAsync(ImportRequestDto request)
    {
        var result = new ImportResultDto();
        
        try
        {
            List<Dictionary<string, object>> data;
            
            if (IsExcelFile(request.FileName))
            {
                data = await _excelService.ReadExcelFileAsync(request.FileStream, request.FileName);
            }
            else
            {
                data = await _csvService.ReadCsvFileAsync(request.FileStream, request.FileName);
            }

            result.TotalRecords = data.Count;
            var successCount = 0;
            var errors = new List<ImportErrorDto>();

            await _unitOfWork.BeginTransactionAsync();

            for (int i = 0; i < data.Count; i++)
            {
                var row = data[i];
                var rowNumber = i + 2; // Account for header row

                try
                {
                    var employee = MapToEmployee(row, request.FieldMappings, request.DefaultValues);
                    
                    if (request.BranchId.HasValue)
                    {
                        employee.BranchId = request.BranchId.Value;
                    }

                    // Check if employee already exists
                    var existingEmployee = await _unitOfWork.Employees.FirstOrDefaultAsync(e => e.Email == employee.Email);
                    
                    if (existingEmployee != null)
                    {
                        if (request.UpdateExisting)
                        {
                            // Update existing employee
                            UpdateEmployeeFromImport(existingEmployee, employee);
                            await _unitOfWork.Employees.UpdateAsync(existingEmployee);
                        }
                        else
                        {
                            errors.Add(new ImportErrorDto
                            {
                                RowNumber = rowNumber,
                                Field = "Email",
                                Value = employee.Email,
                                ErrorMessage = "Employee with this email already exists",
                                ErrorType = "DUPLICATE_RECORD"
                            });
                            continue;
                        }
                    }
                    else
                    {
                        // Generate employee ID if not provided
                        if (string.IsNullOrEmpty(employee.EmployeeId))
                        {
                            employee.EmployeeId = await GenerateEmployeeIdAsync(employee.BranchId);
                        }

                        await _unitOfWork.Employees.AddAsync(employee);
                    }

                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing employee at row {RowNumber}", rowNumber);
                    errors.Add(new ImportErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "General",
                        Value = "",
                        ErrorMessage = ex.Message,
                        ErrorType = "IMPORT_ERROR"
                    });
                }
            }

            if (errors.Count == 0)
            {
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                
                result.Success = true;
                result.SuccessfulRecords = successCount;
                result.Message = $"Successfully imported {successCount} employees";
            }
            else
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Success = false;
                result.SuccessfulRecords = successCount;
                result.FailedRecords = errors.Count;
                result.Errors = errors;
                result.Message = $"Import completed with {errors.Count} errors";
            }

            _logger.LogInformation("Employee import completed. Success: {Success}, Total: {Total}, Successful: {Successful}, Failed: {Failed}", 
                result.Success, result.TotalRecords, result.SuccessfulRecords, result.FailedRecords);

            return result;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error during employee import");
            
            result.Success = false;
            result.Message = $"Import failed: {ex.Message}";
            return result;
        }
    }

    public async Task<ImportResultDto> ImportAttendanceAsync(ImportRequestDto request)
    {
        var result = new ImportResultDto();
        
        try
        {
            List<Dictionary<string, object>> data;
            
            if (IsExcelFile(request.FileName))
            {
                data = await _excelService.ReadExcelFileAsync(request.FileStream, request.FileName);
            }
            else
            {
                data = await _csvService.ReadCsvFileAsync(request.FileStream, request.FileName);
            }

            result.TotalRecords = data.Count;
            var successCount = 0;
            var errors = new List<ImportErrorDto>();

            await _unitOfWork.BeginTransactionAsync();

            for (int i = 0; i < data.Count; i++)
            {
                var row = data[i];
                var rowNumber = i + 2;

                try
                {
                    var attendance = MapToAttendanceRecord(row, request.FieldMappings, request.DefaultValues);
                    
                    // Validate employee exists
                    var employeeId = attendance.Employee?.EmployeeId;
                    if (string.IsNullOrEmpty(employeeId))
                    {
                        errors.Add(new ImportErrorDto
                        {
                            RowNumber = rowNumber,
                            Field = "EmployeeId",
                            Value = "",
                            ErrorMessage = "Employee ID is required",
                            ErrorType = "MISSING_FIELD"
                        });
                        continue;
                    }
                    
                    var employee = await _unitOfWork.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
                    if (employee == null)
                    {
                        errors.Add(new ImportErrorDto
                        {
                            RowNumber = rowNumber,
                            Field = "EmployeeId",
                            Value = attendance.Employee?.EmployeeId ?? "",
                            ErrorMessage = "Employee not found",
                            ErrorType = "INVALID_REFERENCE"
                        });
                        continue;
                    }

                    attendance.EmployeeId = employee.Id;
                    attendance.Employee = null!; // Clear navigation property

                    await _unitOfWork.AttendanceRecords.AddAsync(attendance);
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error importing attendance record at row {RowNumber}", rowNumber);
                    errors.Add(new ImportErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = "General",
                        Value = "",
                        ErrorMessage = ex.Message,
                        ErrorType = "IMPORT_ERROR"
                    });
                }
            }

            if (errors.Count == 0)
            {
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                
                result.Success = true;
                result.SuccessfulRecords = successCount;
                result.Message = $"Successfully imported {successCount} attendance records";
            }
            else
            {
                await _unitOfWork.RollbackTransactionAsync();
                result.Success = false;
                result.SuccessfulRecords = successCount;
                result.FailedRecords = errors.Count;
                result.Errors = errors;
                result.Message = $"Import completed with {errors.Count} errors";
            }

            return result;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error during attendance import");
            
            result.Success = false;
            result.Message = $"Import failed: {ex.Message}";
            return result;
        }
    }

    public async Task<ImportResultDto> ImportLeaveRequestsAsync(ImportRequestDto request)
    {
        // Implementation similar to other import methods
        // This would map CSV/Excel data to LeaveRequest entities
        throw new NotImplementedException("Leave requests import will be implemented in a future iteration");
    }

    public async Task<ImportResultDto> ImportProjectsAsync(ImportRequestDto request)
    {
        // Implementation similar to other import methods
        // This would map CSV/Excel data to Project entities
        throw new NotImplementedException("Projects import will be implemented in a future iteration");
    }

    #endregion

    #region Export Operations

    public async Task<ExportResultDto> ExportDataAsync(ExportRequestDto request)
    {
        try
        {
            return request.EntityType switch
            {
                "Employee" => await ExportEmployeesAsync(request),
                "AttendanceRecord" => await ExportAttendanceAsync(request),
                "LeaveRequest" => await ExportLeaveRequestsAsync(request),
                "PayrollRecord" => await ExportPayrollAsync(request),
                "Project" => await ExportProjectsAsync(request),
                _ => throw new ArgumentException($"Export not implemented for entity type '{request.EntityType}'")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting data for entity type {EntityType}", request.EntityType);
            return new ExportResultDto
            {
                Success = false,
                Message = $"Export failed: {ex.Message}"
            };
        }
    }

    public async Task<ExportResultDto> ExportEmployeesAsync(ExportRequestDto request)
    {
        try
        {
            var query = _unitOfWork.Employees.GetQueryable();

            // Apply filters
            if (request.BranchId.HasValue)
            {
                query = query.Where(e => e.BranchId == request.BranchId.Value);
            }

            if (!request.IncludeDeleted)
            {
                query = query.Where(e => !e.IsDeleted);
            }

            var employees = await Task.FromResult(query.ToList());
            
            byte[] fileContent;
            string contentType;
            string fileName;

            if (request.Format == ReportExportFormat.Excel)
            {
                fileContent = await _excelService.WriteToExcelAsync(employees, "Employees", request.SelectedFields);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileName = $"Employees_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            }
            else
            {
                fileContent = await _csvService.WriteToCsvAsync(employees, request.SelectedFields);
                contentType = "text/csv";
                fileName = $"Employees_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            }

            return new ExportResultDto
            {
                Success = true,
                FileName = fileName,
                FileContent = fileContent,
                ContentType = contentType,
                RecordCount = employees.Count,
                Message = $"Successfully exported {employees.Count} employees"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting employees");
            return new ExportResultDto
            {
                Success = false,
                Message = $"Export failed: {ex.Message}"
            };
        }
    }

    public async Task<ExportResultDto> ExportAttendanceAsync(ExportRequestDto request)
    {
        try
        {
            var query = _unitOfWork.AttendanceRecords.GetQueryable();

            // Apply filters
            if (request.BranchId.HasValue)
            {
                query = query.Where(a => a.Employee.BranchId == request.BranchId.Value);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(a => a.Date >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(a => a.Date <= request.EndDate.Value);
            }

            var attendanceRecords = await Task.FromResult(query.ToList());
            
            byte[] fileContent;
            string contentType;
            string fileName;

            if (request.Format == ReportExportFormat.Excel)
            {
                fileContent = await _excelService.WriteToExcelAsync(attendanceRecords, "Attendance", request.SelectedFields);
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                fileName = $"Attendance_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            }
            else
            {
                fileContent = await _csvService.WriteToCsvAsync(attendanceRecords, request.SelectedFields);
                contentType = "text/csv";
                fileName = $"Attendance_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            }

            return new ExportResultDto
            {
                Success = true,
                FileName = fileName,
                FileContent = fileContent,
                ContentType = contentType,
                RecordCount = attendanceRecords.Count,
                Message = $"Successfully exported {attendanceRecords.Count} attendance records"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting attendance records");
            return new ExportResultDto
            {
                Success = false,
                Message = $"Export failed: {ex.Message}"
            };
        }
    }

    public async Task<ExportResultDto> ExportLeaveRequestsAsync(ExportRequestDto request)
    {
        // Implementation similar to other export methods
        throw new NotImplementedException("Leave requests export will be implemented in a future iteration");
    }

    public async Task<ExportResultDto> ExportPayrollAsync(ExportRequestDto request)
    {
        // Implementation similar to other export methods
        throw new NotImplementedException("Payroll export will be implemented in a future iteration");
    }

    public async Task<ExportResultDto> ExportProjectsAsync(ExportRequestDto request)
    {
        // Implementation similar to other export methods
        throw new NotImplementedException("Projects export will be implemented in a future iteration");
    }

    #endregion

    #region Data Migration Operations

    public async Task<ImportResultDto> MigrateDataAsync(DataMigrationRequestDto request)
    {
        // This would implement data migration between different entity types or systems
        throw new NotImplementedException("Data migration will be implemented in a future iteration");
    }

    #endregion

    #region Utility Operations

    public async Task<byte[]> GenerateImportTemplateAsync(string entityType)
    {
        try
        {
            if (!_supportedEntities.ContainsKey(entityType))
            {
                throw new ArgumentException($"Entity type '{entityType}' is not supported");
            }

            var type = _supportedEntities[entityType];
            return await _excelService.GenerateTemplateAsync(type, $"{entityType}_Template");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating template for entity type {EntityType}", entityType);
            throw;
        }
    }

    public async Task<List<string>> GetSupportedEntityTypesAsync()
    {
        return await Task.FromResult(_supportedEntities.Keys.ToList());
    }

    public async Task<Dictionary<string, string>> GetEntityFieldMappingsAsync(string entityType)
    {
        try
        {
            if (!_supportedEntities.ContainsKey(entityType))
            {
                throw new ArgumentException($"Entity type '{entityType}' is not supported");
            }

            var type = _supportedEntities[entityType];
            var properties = type.GetProperties()
                .Where(p => p.CanWrite && (!p.PropertyType.IsClass || p.PropertyType == typeof(string)))
                .ToDictionary(p => p.Name, p => p.Name);

            return await Task.FromResult(properties);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting field mappings for entity type {EntityType}", entityType);
            throw;
        }
    }

    #endregion

    #region Private Helper Methods

    private ValidationResultDto ValidateDataByEntityType(Type entityType, List<Dictionary<string, object>> data, Dictionary<string, string> fieldMappings, bool isExcel = true)
    {
        if (isExcel)
        {
            // Use reflection to call the generic validation method on the Excel service
            var method = _excelService.GetType().GetMethod(nameof(IExcelService.ValidateExcelData));
            var genericMethod = method?.MakeGenericMethod(entityType);
            
            if (genericMethod != null)
            {
                return (ValidationResultDto)genericMethod.Invoke(_excelService, new object[] { data, fieldMappings })!;
            }
        }
        else
        {
            // Use reflection to call the generic validation method on the CSV service
            var method = _csvService.GetType().GetMethod(nameof(ICsvService.ValidateCsvData));
            var genericMethod = method?.MakeGenericMethod(entityType);
            
            if (genericMethod != null)
            {
                return (ValidationResultDto)genericMethod.Invoke(_csvService, new object[] { data, fieldMappings })!;
            }
        }

        throw new InvalidOperationException($"Could not validate data for entity type {entityType.Name}");
    }

    private Employee MapToEmployee(Dictionary<string, object> row, Dictionary<string, string> fieldMappings, Dictionary<string, object> defaultValues)
    {
        var employee = new Employee();
        
        // Apply default values first
        foreach (var defaultValue in defaultValues)
        {
            var property = typeof(Employee).GetProperty(defaultValue.Key);
            if (property != null && property.CanWrite)
            {
                try
                {
                    var convertedValue = Convert.ChangeType(defaultValue.Value, property.PropertyType);
                    property.SetValue(employee, convertedValue);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to set default value for property {Property}", defaultValue.Key);
                }
            }
        }

        // Map fields from row data
        var mappings = fieldMappings.Any() ? fieldMappings : GetDefaultEmployeeFieldMappings();
        
        foreach (var mapping in mappings)
        {
            if (!row.ContainsKey(mapping.Value)) continue;

            var property = typeof(Employee).GetProperty(mapping.Key);
            if (property == null || !property.CanWrite) continue;

            var value = row[mapping.Value];
            if (value == null) continue;

            try
            {
                var convertedValue = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(employee, convertedValue);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to convert value {Value} for property {Property}", value, mapping.Key);
            }
        }

        return employee;
    }

    private AttendanceRecord MapToAttendanceRecord(Dictionary<string, object> row, Dictionary<string, string> fieldMappings, Dictionary<string, object> defaultValues)
    {
        var attendance = new AttendanceRecord();
        
        // Apply default values first
        foreach (var defaultValue in defaultValues)
        {
            var property = typeof(AttendanceRecord).GetProperty(defaultValue.Key);
            if (property != null && property.CanWrite)
            {
                try
                {
                    var convertedValue = Convert.ChangeType(defaultValue.Value, property.PropertyType);
                    property.SetValue(attendance, convertedValue);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to set default value for property {Property}", defaultValue.Key);
                }
            }
        }

        // Map fields from row data
        var mappings = fieldMappings.Any() ? fieldMappings : GetDefaultAttendanceFieldMappings();
        
        foreach (var mapping in mappings)
        {
            if (!row.ContainsKey(mapping.Value)) continue;

            var property = typeof(AttendanceRecord).GetProperty(mapping.Key);
            if (property == null || !property.CanWrite) continue;

            var value = row[mapping.Value];
            if (value == null) continue;

            try
            {
                // Special handling for EmployeeId - create a temporary Employee object
                if (mapping.Key == "EmployeeId" && property.PropertyType == typeof(Employee))
                {
                    attendance.Employee = new Employee { EmployeeId = value.ToString()! };
                    continue;
                }

                var convertedValue = Convert.ChangeType(value, property.PropertyType);
                property.SetValue(attendance, convertedValue);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to convert value {Value} for property {Property}", value, mapping.Key);
            }
        }

        return attendance;
    }

    private void UpdateEmployeeFromImport(Employee existingEmployee, Employee importedEmployee)
    {
        // Update only non-null values from imported employee
        var properties = typeof(Employee).GetProperties()
            .Where(p => p.CanWrite && p.Name != "Id" && p.Name != "CreatedAt" && p.Name != "CreatedBy");

        foreach (var property in properties)
        {
            var importedValue = property.GetValue(importedEmployee);
            if (importedValue != null)
            {
                property.SetValue(existingEmployee, importedValue);
            }
        }

        existingEmployee.UpdatedAt = DateTime.UtcNow;
    }

    private async Task<string> GenerateEmployeeIdAsync(int branchId)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(branchId);
        var branchCode = branch?.Name?.Substring(0, Math.Min(3, branch.Name.Length)).ToUpper() ?? "EMP";
        var year = DateTime.Now.Year.ToString().Substring(2);
        
        var lastEmployee = await _unitOfWork.Employees
            .GetQueryable()
            .Where(e => e.BranchId == branchId && e.EmployeeId.StartsWith($"{branchCode}-{year}"))
            .OrderByDescending(e => e.EmployeeId)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastEmployee != null)
        {
            var lastIdParts = lastEmployee.EmployeeId.Split('-');
            if (lastIdParts.Length > 2 && int.TryParse(lastIdParts[2], out var lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{branchCode}-{year}-{nextNumber:D3}";
    }

    private Dictionary<string, string> GetDefaultEmployeeFieldMappings()
    {
        return new Dictionary<string, string>
        {
            { "EmployeeId", "Employee ID" },
            { "FirstName", "First Name" },
            { "LastName", "Last Name" },
            { "Email", "Email" },
            { "Phone", "Phone" },
            { "DateOfBirth", "Date of Birth" },
            { "JoiningDate", "Joining Date" },
            { "Designation", "Designation" },
            { "Department", "Department" },
            { "BasicSalary", "Basic Salary" }
        };
    }

    private Dictionary<string, string> GetDefaultAttendanceFieldMappings()
    {
        return new Dictionary<string, string>
        {
            { "EmployeeId", "Employee ID" },
            { "Date", "Date" },
            { "CheckInTime", "Check In Time" },
            { "CheckOutTime", "Check Out Time" },
            { "TotalWorkingHours", "Total Working Hours" },
            { "BreakDuration", "Break Duration" },
            { "OvertimeHours", "Overtime Hours" },
            { "Location", "Location" }
        };
    }

    private bool IsExcelFile(string fileName)
    {
        return _excelService.IsValidExcelFile(fileName);
    }

    private bool IsCsvFile(string fileName)
    {
        return _csvService.IsValidCsvFile(fileName);
    }

    #endregion
}