using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Employee;

namespace StrideHR.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IUnitOfWork unitOfWork, 
        IFileStorageService fileStorageService,
        ILogger<EmployeeService> logger)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    #region Basic CRUD Operations

    public async Task<Employee?> GetByIdAsync(int id)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id, e => e.Branch, e => e.ReportingManager);
        return employee;
    }

    public async Task<Employee?> GetByEmployeeIdAsync(string employeeId)
    {
        var employee = await _unitOfWork.Employees.FirstOrDefaultAsync(
            e => e.EmployeeId == employeeId,
            e => e.Branch,
            e => e.ReportingManager
        );
        return employee;
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        var employees = await _unitOfWork.Employees.GetAllAsync(e => e.Branch, e => e.ReportingManager);
        return employees ?? Enumerable.Empty<Employee>();
    }

    public async Task<IEnumerable<Employee>> GetByBranchAsync(int branchId)
    {
        var employees = await _unitOfWork.Employees.FindAsync(
            e => e.BranchId == branchId,
            e => e.Branch,
            e => e.ReportingManager
        );
        return employees ?? Enumerable.Empty<Employee>();
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        await _unitOfWork.Employees.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee> CreateAsync(CreateEmployeeDto dto)
    {
        // Validate the data
        if (!await ValidateEmployeeDataAsync(dto))
        {
            throw new ArgumentException("Invalid employee data");
        }

        // Generate employee ID
        var employeeId = await GenerateEmployeeIdAsync(dto.BranchId);

        var employee = new Employee
        {
            EmployeeId = employeeId,
            BranchId = dto.BranchId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            AlternatePhone = dto.AlternatePhone,
            DateOfBirth = dto.DateOfBirth,
            JoiningDate = dto.JoiningDate,
            Designation = dto.Designation,
            Department = dto.Department,
            Address = dto.Address,
            EmergencyContactName = dto.EmergencyContactName,
            EmergencyContactPhone = dto.EmergencyContactPhone,
            BloodGroup = dto.BloodGroup,
            NationalId = dto.NationalId,
            PassportNumber = dto.PassportNumber,
            VisaStatus = dto.VisaStatus,
            BasicSalary = dto.BasicSalary,
            ReportingManagerId = dto.ReportingManagerId,
            Notes = dto.Notes,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Employees.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Employee created successfully: {EmployeeId}", employee.EmployeeId);
        return employee;
    }

    public async Task UpdateAsync(Employee employee)
    {
        employee.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Employees.UpdateAsync(employee);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, UpdateEmployeeDto dto)
    {
        if (!await ValidateEmployeeUpdateAsync(id, dto))
        {
            throw new ArgumentException("Invalid employee update data");
        }

        var employee = await GetByIdAsync(id);
        if (employee == null)
        {
            throw new ArgumentException("Employee not found");
        }

        // Update properties
        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.Phone = dto.Phone;
        employee.AlternatePhone = dto.AlternatePhone;
        employee.DateOfBirth = dto.DateOfBirth;
        employee.Designation = dto.Designation;
        employee.Department = dto.Department;
        employee.Address = dto.Address;
        employee.EmergencyContactName = dto.EmergencyContactName;
        employee.EmergencyContactPhone = dto.EmergencyContactPhone;
        employee.BloodGroup = dto.BloodGroup;
        employee.NationalId = dto.NationalId;
        employee.PassportNumber = dto.PassportNumber;
        employee.VisaStatus = dto.VisaStatus;
        employee.BasicSalary = dto.BasicSalary;
        employee.ReportingManagerId = dto.ReportingManagerId;
        employee.Notes = dto.Notes;
        employee.Status = dto.Status;
        employee.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Employees.UpdateAsync(employee);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Employee updated successfully: {EmployeeId}", employee.EmployeeId);
    }

    public async Task DeleteAsync(int id)
    {
        await _unitOfWork.Employees.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _unitOfWork.Employees.AnyAsync(e => e.Id == id);
    }

    public async Task<bool> ExistsByEmployeeIdAsync(string employeeId)
    {
        return await _unitOfWork.Employees.AnyAsync(e => e.EmployeeId == employeeId);
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        return await GetByIdAsync(id);
    }

    #endregion

    #region Enhanced Functionality

    public async Task<PagedResult<EmployeeDto>> SearchEmployeesAsync(EmployeeSearchCriteria criteria)
    {
        var query = _unitOfWork.Employees.GetQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(criteria.SearchTerm))
        {
            var searchTerm = criteria.SearchTerm.ToLower();
            query = query.Where(e => 
                e.FirstName.ToLower().Contains(searchTerm) ||
                e.LastName.ToLower().Contains(searchTerm) ||
                e.Email.ToLower().Contains(searchTerm) ||
                e.EmployeeId.ToLower().Contains(searchTerm));
        }

        if (criteria.BranchId.HasValue)
        {
            query = query.Where(e => e.BranchId == criteria.BranchId.Value);
        }

        if (!string.IsNullOrEmpty(criteria.Department))
        {
            query = query.Where(e => e.Department == criteria.Department);
        }

        if (!string.IsNullOrEmpty(criteria.Designation))
        {
            query = query.Where(e => e.Designation == criteria.Designation);
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(e => e.Status == criteria.Status.Value);
        }

        if (criteria.ReportingManagerId.HasValue)
        {
            query = query.Where(e => e.ReportingManagerId == criteria.ReportingManagerId.Value);
        }

        if (criteria.JoiningDateFrom.HasValue)
        {
            query = query.Where(e => e.JoiningDate >= criteria.JoiningDateFrom.Value);
        }

        if (criteria.JoiningDateTo.HasValue)
        {
            query = query.Where(e => e.JoiningDate <= criteria.JoiningDateTo.Value);
        }

        // Apply sorting
        query = ApplySorting(query, criteria.SortBy, criteria.SortDescending);

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var employees = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .Include(e => e.Branch)
            .Include(e => e.ReportingManager)
            .ToListAsync();

        var employeeDtos = employees.Select(MapToEmployeeDto).ToList();

        return new PagedResult<EmployeeDto>
        {
            Items = employeeDtos,
            TotalCount = totalCount,
            PageNumber = criteria.PageNumber,
            PageSize = criteria.PageSize
        };
    }

    public async Task<EmployeeDto?> GetEmployeeDtoAsync(int id)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(id, e => e.Branch, e => e.ReportingManager);
        return employee != null ? MapToEmployeeDto(employee) : null;
    }

    public async Task<IEnumerable<EmployeeDto>> GetEmployeeDtosAsync()
    {
        var employees = await _unitOfWork.Employees.GetAllAsync(e => e.Branch, e => e.ReportingManager);
        return employees?.Select(MapToEmployeeDto) ?? Enumerable.Empty<EmployeeDto>();
    }

    public async Task<IEnumerable<EmployeeDto>> GetEmployeeDtosByBranchAsync(int branchId)
    {
        var employees = await _unitOfWork.Employees.FindAsync(
            e => e.BranchId == branchId,
            e => e.Branch,
            e => e.ReportingManager
        );
        return employees?.Select(MapToEmployeeDto) ?? Enumerable.Empty<EmployeeDto>();
    }

    #endregion

    #region Profile Photo Management

    public async Task<string> UploadProfilePhotoAsync(ProfilePhotoUploadDto dto)
    {
        var employee = await GetByIdAsync(dto.EmployeeId);
        if (employee == null)
        {
            throw new ArgumentException("Employee not found");
        }

        // Delete existing photo if exists
        if (!string.IsNullOrEmpty(employee.ProfilePhoto))
        {
            await _fileStorageService.DeleteFileAsync(employee.ProfilePhoto);
        }

        // Save new photo
        var filePath = await _fileStorageService.SaveFileAsync(
            dto.PhotoData, 
            dto.FileName, 
            "profile-photos");

        // Update employee record
        employee.ProfilePhoto = filePath;
        employee.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Employees.UpdateAsync(employee);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Profile photo uploaded for employee: {EmployeeId}", employee.EmployeeId);
        return filePath;
    }

    public async Task<byte[]?> GetProfilePhotoAsync(int employeeId)
    {
        var employee = await GetByIdAsync(employeeId);
        if (employee?.ProfilePhoto == null)
        {
            return null;
        }

        return await _fileStorageService.GetFileAsync(employee.ProfilePhoto);
    }

    public async Task DeleteProfilePhotoAsync(int employeeId)
    {
        var employee = await GetByIdAsync(employeeId);
        if (employee?.ProfilePhoto == null)
        {
            return;
        }

        await _fileStorageService.DeleteFileAsync(employee.ProfilePhoto);
        
        employee.ProfilePhoto = null;
        employee.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Employees.UpdateAsync(employee);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Profile photo deleted for employee: {EmployeeId}", employee.EmployeeId);
    }

    #endregion

    #region Employee ID Generation

    public async Task<string> GenerateEmployeeIdAsync(int branchId)
    {
        var branch = await _unitOfWork.Branches.GetByIdAsync(branchId);
        if (branch == null)
        {
            throw new ArgumentException("Branch not found");
        }

        // Get branch code (first 3 letters of branch name)
        var branchCode = branch.Name.Length >= 3 
            ? branch.Name.Substring(0, 3).ToUpper()
            : branch.Name.ToUpper().PadRight(3, 'X');

        // Get current year (last 2 digits)
        var year = DateTime.Now.Year.ToString().Substring(2);

        // Get next sequence number for this branch and year
        var existingEmployees = await _unitOfWork.Employees.FindAsync(
            e => e.BranchId == branchId && e.EmployeeId.Contains($"{branchCode}-{year}"));

        var maxSequence = 0;
        if (existingEmployees?.Any() == true)
        {
            foreach (var emp in existingEmployees)
            {
                var parts = emp.EmployeeId.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out var sequence))
                {
                    maxSequence = Math.Max(maxSequence, sequence);
                }
            }
        }

        var nextSequence = (maxSequence + 1).ToString("D3");
        return $"{branchCode}-{year}-{nextSequence}";
    }

    #endregion

    #region Onboarding Workflow

    public async Task<bool> StartOnboardingAsync(EmployeeOnboardingDto dto)
    {
        var employee = await GetByIdAsync(dto.EmployeeId);
        if (employee == null)
        {
            return false;
        }

        var onboarding = new EmployeeOnboarding
        {
            EmployeeId = dto.EmployeeId,
            OnboardingDate = dto.OnboardingDate,
            OnboardingManager = dto.OnboardingManager,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.EmployeeOnboardings.AddAsync(onboarding);
        await _unitOfWork.SaveChangesAsync();

        // Add tasks
        foreach (var task in dto.Tasks)
        {
            var onboardingTask = new EmployeeOnboardingTask
            {
                EmployeeOnboardingId = onboarding.Id,
                TaskName = task.TaskName,
                Description = task.Description,
                DueDate = task.DueDate,
                AssignedTo = task.AssignedTo,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.EmployeeOnboardingTasks.AddAsync(onboardingTask);
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Onboarding started for employee: {EmployeeId}", dto.EmployeeId);
        return true;
    }

    public async Task<bool> CompleteOnboardingTaskAsync(int employeeId, string taskName)
    {
        var onboarding = await _unitOfWork.EmployeeOnboardings.FirstOrDefaultAsync(
            o => o.EmployeeId == employeeId && !o.IsCompleted);

        if (onboarding == null)
        {
            return false;
        }

        var task = await _unitOfWork.EmployeeOnboardingTasks.FirstOrDefaultAsync(
            t => t.EmployeeOnboardingId == onboarding.Id && t.TaskName == taskName && !t.IsCompleted);

        if (task == null)
        {
            return false;
        }

        task.IsCompleted = true;
        task.CompletedDate = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.EmployeeOnboardingTasks.UpdateAsync(task);

        // Check if all tasks are completed
        var allTasks = await _unitOfWork.EmployeeOnboardingTasks.FindAsync(
            t => t.EmployeeOnboardingId == onboarding.Id);

        if (allTasks?.All(t => t.IsCompleted) == true)
        {
            onboarding.IsCompleted = true;
            onboarding.CompletedDate = DateTime.UtcNow;
            onboarding.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.EmployeeOnboardings.UpdateAsync(onboarding);
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Onboarding task completed: {TaskName} for employee: {EmployeeId}", taskName, employeeId);
        return true;
    }

    public async Task<EmployeeOnboardingDto?> GetOnboardingStatusAsync(int employeeId)
    {
        var onboarding = await _unitOfWork.EmployeeOnboardings.FirstOrDefaultAsync(
            o => o.EmployeeId == employeeId,
            o => o.Tasks);

        if (onboarding == null)
        {
            return null;
        }

        return new EmployeeOnboardingDto
        {
            EmployeeId = onboarding.EmployeeId,
            OnboardingDate = onboarding.OnboardingDate,
            OnboardingManager = onboarding.OnboardingManager,
            Notes = onboarding.Notes,
            Tasks = onboarding.Tasks.Select(t => new OnboardingTask
            {
                TaskName = t.TaskName,
                Description = t.Description,
                DueDate = t.DueDate,
                IsCompleted = t.IsCompleted,
                CompletedDate = t.CompletedDate,
                AssignedTo = t.AssignedTo,
                CompletionNotes = t.CompletionNotes
            }).ToList()
        };
    }

    #endregion

    #region Exit Workflow

    public async Task<bool> InitiateExitProcessAsync(EmployeeExitDto dto)
    {
        var employee = await GetByIdAsync(dto.EmployeeId);
        if (employee == null)
        {
            return false;
        }

        var exit = new EmployeeExit
        {
            EmployeeId = dto.EmployeeId,
            ExitDate = dto.ExitDate,
            ExitReason = dto.ExitReason,
            ExitNotes = dto.ExitNotes,
            ProcessedBy = dto.ProcessedBy,
            IsExitInterviewCompleted = dto.IsExitInterviewCompleted,
            ExitInterviewDate = dto.ExitInterviewDate,
            ExitInterviewNotes = dto.ExitInterviewNotes,
            AreAssetsReturned = dto.AreAssetsReturned,
            AssetReturnNotes = dto.AssetReturnNotes,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.EmployeeExits.AddAsync(exit);
        await _unitOfWork.SaveChangesAsync();

        // Add tasks
        foreach (var task in dto.Tasks)
        {
            var exitTask = new EmployeeExitTask
            {
                EmployeeExitId = exit.Id,
                TaskName = task.TaskName,
                Description = task.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.EmployeeExitTasks.AddAsync(exitTask);
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Exit process initiated for employee: {EmployeeId}", dto.EmployeeId);
        return true;
    }

    public async Task<bool> CompleteExitTaskAsync(int employeeId, string taskName)
    {
        var exit = await _unitOfWork.EmployeeExits.FirstOrDefaultAsync(
            e => e.EmployeeId == employeeId && !e.IsCompleted);

        if (exit == null)
        {
            return false;
        }

        var task = await _unitOfWork.EmployeeExitTasks.FirstOrDefaultAsync(
            t => t.EmployeeExitId == exit.Id && t.TaskName == taskName && !t.IsCompleted);

        if (task == null)
        {
            return false;
        }

        task.IsCompleted = true;
        task.CompletedDate = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.EmployeeExitTasks.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Exit task completed: {TaskName} for employee: {EmployeeId}", taskName, employeeId);
        return true;
    }

    public async Task<EmployeeExitDto?> GetExitStatusAsync(int employeeId)
    {
        var exit = await _unitOfWork.EmployeeExits.FirstOrDefaultAsync(
            e => e.EmployeeId == employeeId,
            e => e.Tasks);

        if (exit == null)
        {
            return null;
        }

        return new EmployeeExitDto
        {
            EmployeeId = exit.EmployeeId,
            ExitDate = exit.ExitDate,
            ExitReason = exit.ExitReason,
            ExitNotes = exit.ExitNotes,
            ProcessedBy = exit.ProcessedBy,
            IsExitInterviewCompleted = exit.IsExitInterviewCompleted,
            ExitInterviewDate = exit.ExitInterviewDate,
            ExitInterviewNotes = exit.ExitInterviewNotes,
            AreAssetsReturned = exit.AreAssetsReturned,
            AssetReturnNotes = exit.AssetReturnNotes,
            Tasks = exit.Tasks.Select(t => new ExitTask
            {
                TaskName = t.TaskName,
                Description = t.Description,
                IsCompleted = t.IsCompleted,
                CompletedDate = t.CompletedDate,
                CompletedBy = t.CompletedBy,
                CompletionNotes = t.CompletionNotes
            }).ToList()
        };
    }

    public async Task<bool> FinalizeExitAsync(int employeeId)
    {
        var exit = await _unitOfWork.EmployeeExits.FirstOrDefaultAsync(
            e => e.EmployeeId == employeeId && !e.IsCompleted);

        if (exit == null)
        {
            return false;
        }

        var employee = await GetByIdAsync(employeeId);
        if (employee == null)
        {
            return false;
        }

        // Update employee status and exit date
        employee.Status = EmployeeStatus.Terminated;
        employee.ExitDate = exit.ExitDate;
        employee.UpdatedAt = DateTime.UtcNow;

        // Mark exit as completed
        exit.IsCompleted = true;
        exit.CompletedDate = DateTime.UtcNow;
        exit.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Employees.UpdateAsync(employee);
        await _unitOfWork.EmployeeExits.UpdateAsync(exit);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Exit finalized for employee: {EmployeeId}", employeeId);
        return true;
    }

    #endregion

    #region Validation

    public async Task<bool> ValidateEmployeeDataAsync(CreateEmployeeDto dto)
    {
        // Check if email already exists
        var existingEmployee = await _unitOfWork.Employees.FirstOrDefaultAsync(e => e.Email == dto.Email);
        if (existingEmployee != null)
        {
            return false;
        }

        // Check if branch exists
        var branch = await _unitOfWork.Branches.GetByIdAsync(dto.BranchId);
        if (branch == null)
        {
            return false;
        }

        // Check if reporting manager exists (if provided)
        if (dto.ReportingManagerId.HasValue)
        {
            var manager = await _unitOfWork.Employees.GetByIdAsync(dto.ReportingManagerId.Value);
            if (manager == null)
            {
                return false;
            }
        }

        return true;
    }

    public async Task<bool> ValidateEmployeeUpdateAsync(int id, UpdateEmployeeDto dto)
    {
        // Check if email already exists for another employee
        var existingEmployee = await _unitOfWork.Employees.FirstOrDefaultAsync(
            e => e.Email == dto.Email && e.Id != id);
        if (existingEmployee != null)
        {
            return false;
        }

        // Check if reporting manager exists (if provided)
        if (dto.ReportingManagerId.HasValue)
        {
            var manager = await _unitOfWork.Employees.GetByIdAsync(dto.ReportingManagerId.Value);
            if (manager == null)
            {
                return false;
            }

            // Prevent circular reporting
            if (dto.ReportingManagerId.Value == id)
            {
                return false;
            }
        }

        return true;
    }

    #endregion

    #region Private Helper Methods

    private static IQueryable<Employee> ApplySorting(IQueryable<Employee> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return query.OrderBy(e => e.FirstName);
        }

        Expression<Func<Employee, object>> keySelector = sortBy.ToLower() switch
        {
            "firstname" => e => e.FirstName,
            "lastname" => e => e.LastName,
            "email" => e => e.Email,
            "employeeid" => e => e.EmployeeId,
            "designation" => e => e.Designation,
            "department" => e => e.Department,
            "joiningdate" => e => e.JoiningDate,
            "status" => e => e.Status,
            _ => e => e.FirstName
        };

        return sortDescending 
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }

    private static EmployeeDto MapToEmployeeDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            EmployeeId = employee.EmployeeId,
            BranchId = employee.BranchId,
            BranchName = employee.Branch?.Name ?? string.Empty,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            FullName = employee.FullName,
            Email = employee.Email,
            Phone = employee.Phone,
            AlternatePhone = employee.AlternatePhone,
            ProfilePhoto = employee.ProfilePhoto,
            DateOfBirth = employee.DateOfBirth,
            JoiningDate = employee.JoiningDate,
            ExitDate = employee.ExitDate,
            Designation = employee.Designation,
            Department = employee.Department,
            Address = employee.Address,
            EmergencyContactName = employee.EmergencyContactName,
            EmergencyContactPhone = employee.EmergencyContactPhone,
            BloodGroup = employee.BloodGroup,
            NationalId = employee.NationalId,
            PassportNumber = employee.PassportNumber,
            VisaStatus = employee.VisaStatus,
            BasicSalary = employee.BasicSalary,
            Status = employee.Status,
            ReportingManagerId = employee.ReportingManagerId,
            ReportingManagerName = employee.ReportingManager?.FullName,
            Notes = employee.Notes,
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt
        };
    }

    #endregion
}