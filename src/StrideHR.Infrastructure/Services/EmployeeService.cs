using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;

namespace StrideHR.Infrastructure.Services;

/// <summary>
/// Service implementation for employee management operations
/// </summary>
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IRepository<Branch> _branchRepository;
    private readonly ILogger<EmployeeService> _logger;
    private readonly IAuditService _auditService;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IRepository<Branch> branchRepository,
        ILogger<EmployeeService> logger,
        IAuditService auditService)
    {
        _employeeRepository = employeeRepository;
        _branchRepository = branchRepository;
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<Employee> CreateEmployeeAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new employee: {Email}", request.Email);

        // Validate branch exists
        var branch = await _branchRepository.GetByIdAsync(request.BranchId, cancellationToken);
        if (branch == null)
        {
            throw new ArgumentException($"Branch with ID {request.BranchId} not found");
        }

        // Validate email uniqueness
        if (!await IsEmailUniqueAsync(request.Email, null, cancellationToken))
        {
            throw new InvalidOperationException($"Email {request.Email} is already in use");
        }

        // Validate reporting manager if specified
        if (request.ReportingManagerId.HasValue)
        {
            var manager = await _employeeRepository.GetByIdAsync(request.ReportingManagerId.Value, cancellationToken);
            if (manager == null)
            {
                throw new ArgumentException($"Reporting manager with ID {request.ReportingManagerId} not found");
            }
        }

        // Generate unique employee ID
        var employeeId = await GenerateEmployeeIdAsync(request.BranchId, cancellationToken);

        var employee = new Employee
        {
            BranchId = request.BranchId,
            EmployeeId = employeeId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            Email = request.Email,
            Phone = request.Phone,
            AlternatePhone = request.AlternatePhone,
            DateOfBirth = request.DateOfBirth,
            JoiningDate = request.JoiningDate,
            Designation = request.Designation,
            Department = request.Department,
            BasicSalary = request.BasicSalary,
            Status = EmployeeStatus.Active,
            ReportingManagerId = request.ReportingManagerId,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            EmergencyContact = request.EmergencyContact,
            NationalId = request.NationalId,
            TaxId = request.TaxId,
            BankDetails = request.BankDetails,
            VisaStatus = request.VisaStatus,
            VisaExpiryDate = request.VisaExpiryDate,
            CreatedAt = DateTime.UtcNow
        };

        var createdEmployee = await _employeeRepository.AddAsync(employee, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Employee", createdEmployee.Id, "CREATE", 
            $"Employee {employeeId} created", cancellationToken);

        _logger.LogInformation("Employee created successfully: {EmployeeId}", employeeId);
        return createdEmployee;
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _employeeRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Employee?> GetEmployeeByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default)
    {
        return await _employeeRepository.GetByEmployeeIdAsync(employeeId, cancellationToken);
    }

    public async Task<Employee> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating employee: {Id}", id);

        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        if (employee == null)
        {
            throw new ArgumentException($"Employee with ID {id} not found");
        }

        // Validate email uniqueness if email is being changed
        if (!string.IsNullOrEmpty(request.Email) && request.Email != employee.Email)
        {
            if (!await IsEmailUniqueAsync(request.Email, id, cancellationToken))
            {
                throw new InvalidOperationException($"Email {request.Email} is already in use");
            }
        }

        // Validate reporting manager if being changed
        if (request.ReportingManagerId.HasValue && request.ReportingManagerId != employee.ReportingManagerId)
        {
            if (!await ValidateReportingStructureAsync(id, request.ReportingManagerId, cancellationToken))
            {
                throw new InvalidOperationException("Invalid reporting structure - would create circular reference");
            }
        }

        // Update fields that are provided
        if (!string.IsNullOrEmpty(request.FirstName)) employee.FirstName = request.FirstName;
        if (!string.IsNullOrEmpty(request.LastName)) employee.LastName = request.LastName;
        if (request.MiddleName != null) employee.MiddleName = request.MiddleName;
        if (!string.IsNullOrEmpty(request.Email)) employee.Email = request.Email;
        if (request.Phone != null) employee.Phone = request.Phone;
        if (request.AlternatePhone != null) employee.AlternatePhone = request.AlternatePhone;
        if (request.DateOfBirth.HasValue) employee.DateOfBirth = request.DateOfBirth.Value;
        if (request.Designation != null) employee.Designation = request.Designation;
        if (request.Department != null) employee.Department = request.Department;
        if (request.BasicSalary.HasValue) employee.BasicSalary = request.BasicSalary.Value;
        if (request.Status.HasValue) employee.Status = request.Status.Value;
        if (request.ReportingManagerId != null) employee.ReportingManagerId = request.ReportingManagerId;
        if (request.Address != null) employee.Address = request.Address;
        if (request.City != null) employee.City = request.City;
        if (request.State != null) employee.State = request.State;
        if (request.PostalCode != null) employee.PostalCode = request.PostalCode;
        if (request.Country != null) employee.Country = request.Country;
        if (request.EmergencyContact != null) employee.EmergencyContact = request.EmergencyContact;
        if (request.NationalId != null) employee.NationalId = request.NationalId;
        if (request.TaxId != null) employee.TaxId = request.TaxId;
        if (request.BankDetails != null) employee.BankDetails = request.BankDetails;
        if (request.VisaStatus != null) employee.VisaStatus = request.VisaStatus;
        if (request.VisaExpiryDate != null) employee.VisaExpiryDate = request.VisaExpiryDate;

        employee.UpdatedAt = DateTime.UtcNow;

        var updatedEmployee = await _employeeRepository.UpdateAsync(employee, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Employee", id, "UPDATE", 
            $"Employee {employee.EmployeeId} updated", cancellationToken);

        _logger.LogInformation("Employee updated successfully: {EmployeeId}", employee.EmployeeId);
        return updatedEmployee;
    }

    public async Task<bool> DeleteEmployeeAsync(int id, string? deletedBy = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting employee: {Id}", id);

        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        if (employee == null)
        {
            return false;
        }

        await _employeeRepository.SoftDeleteAsync(id, deletedBy, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Employee", id, "DELETE", 
            $"Employee {employee.EmployeeId} deleted", cancellationToken);

        _logger.LogInformation("Employee deleted successfully: {EmployeeId}", employee.EmployeeId);
        return true;
    }

    public async Task<(IEnumerable<Employee> Employees, int TotalCount)> SearchEmployeesAsync(
        EmployeeSearchCriteria criteria, 
        CancellationToken cancellationToken = default)
    {
        return await _employeeRepository.SearchAsync(criteria, cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByBranchAsync(int branchId, CancellationToken cancellationToken = default)
    {
        return await _employeeRepository.GetByBranchIdAsync(branchId, cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentAsync(string department, CancellationToken cancellationToken = default)
    {
        return await _employeeRepository.GetByDepartmentAsync(department, cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByManagerAsync(int managerId, CancellationToken cancellationToken = default)
    {
        return await _employeeRepository.GetByManagerIdAsync(managerId, cancellationToken);
    }

    public async Task<string> UploadProfilePhotoAsync(int employeeId, Stream photoStream, string fileName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading profile photo for employee: {EmployeeId}", employeeId);

        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee == null)
        {
            throw new ArgumentException($"Employee with ID {employeeId} not found");
        }

        // Create uploads directory if it doesn't exist
        var uploadsPath = Path.Combine("uploads", "profile-photos");
        Directory.CreateDirectory(uploadsPath);

        // Generate unique filename
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"{employee.EmployeeId}_{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsPath, uniqueFileName);

        // Save file
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await photoStream.CopyToAsync(fileStream, cancellationToken);
        }

        // Delete old photo if exists
        if (!string.IsNullOrEmpty(employee.ProfilePhotoPath) && File.Exists(employee.ProfilePhotoPath))
        {
            File.Delete(employee.ProfilePhotoPath);
        }

        // Update employee record
        employee.ProfilePhotoPath = filePath;
        employee.UpdatedAt = DateTime.UtcNow;

        await _employeeRepository.UpdateAsync(employee, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Employee", employeeId, "UPDATE", 
            $"Profile photo uploaded for employee {employee.EmployeeId}", cancellationToken);

        _logger.LogInformation("Profile photo uploaded successfully for employee: {EmployeeId}", employee.EmployeeId);
        return filePath;
    }

    public async Task<bool> DeleteProfilePhotoAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee == null || string.IsNullOrEmpty(employee.ProfilePhotoPath))
        {
            return false;
        }

        // Delete file if exists
        if (File.Exists(employee.ProfilePhotoPath))
        {
            File.Delete(employee.ProfilePhotoPath);
        }

        // Update employee record
        employee.ProfilePhotoPath = null;
        employee.UpdatedAt = DateTime.UtcNow;

        await _employeeRepository.UpdateAsync(employee, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Employee", employeeId, "UPDATE", 
            $"Profile photo deleted for employee {employee.EmployeeId}", cancellationToken);

        return true;
    }

    public async Task<Employee> OnboardEmployeeAsync(int employeeId, OnboardingRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting onboarding process for employee: {EmployeeId}", employeeId);

        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee == null)
        {
            throw new ArgumentException($"Employee with ID {employeeId} not found");
        }

        // Update employee status if currently in probation
        if (employee.Status == EmployeeStatus.Probation)
        {
            employee.Status = EmployeeStatus.Active;
        }

        // Store onboarding information in settings (as JSON)
        var onboardingData = new
        {
            CompletedDocuments = request.CompletedDocuments,
            PendingDocuments = request.PendingDocuments,
            OnboardingNotes = request.OnboardingNotes,
            OrientationDate = request.OrientationDate,
            BuddyEmployeeId = request.BuddyEmployeeId,
            OnboardingCompletedAt = DateTime.UtcNow
        };

        employee.Settings = System.Text.Json.JsonSerializer.Serialize(onboardingData);
        employee.UpdatedAt = DateTime.UtcNow;

        var updatedEmployee = await _employeeRepository.UpdateAsync(employee, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Employee", employeeId, "ONBOARD", 
            $"Onboarding completed for employee {employee.EmployeeId}", cancellationToken);

        _logger.LogInformation("Onboarding completed for employee: {EmployeeId}", employee.EmployeeId);
        return updatedEmployee;
    }

    public async Task<Employee> InitiateExitProcessAsync(int employeeId, ExitProcessRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initiating exit process for employee: {EmployeeId}", employeeId);

        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee == null)
        {
            throw new ArgumentException($"Employee with ID {employeeId} not found");
        }

        // Update employee status and termination details
        employee.Status = EmployeeStatus.Terminated;
        employee.TerminationDate = request.LastWorkingDay;
        employee.TerminationReason = request.ExitReason;

        // Store exit process information in settings
        var exitData = new
        {
            ExitReason = request.ExitReason,
            ExitNotes = request.ExitNotes,
            IsVoluntary = request.IsVoluntary,
            AssetsToReturn = request.AssetsToReturn,
            HandoverNotes = request.HandoverNotes,
            ExitInitiatedAt = DateTime.UtcNow,
            LastWorkingDay = request.LastWorkingDay
        };

        employee.Settings = System.Text.Json.JsonSerializer.Serialize(exitData);
        employee.UpdatedAt = DateTime.UtcNow;

        var updatedEmployee = await _employeeRepository.UpdateAsync(employee, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Employee", employeeId, "EXIT_INITIATED", 
            $"Exit process initiated for employee {employee.EmployeeId}", cancellationToken);

        _logger.LogInformation("Exit process initiated for employee: {EmployeeId}", employee.EmployeeId);
        return updatedEmployee;
    }

    public async Task<Employee> CompleteExitProcessAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Completing exit process for employee: {EmployeeId}", employeeId);

        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee == null)
        {
            throw new ArgumentException($"Employee with ID {employeeId} not found");
        }

        // Mark as inactive instead of deleting
        employee.Status = EmployeeStatus.Inactive;
        employee.UpdatedAt = DateTime.UtcNow;

        var updatedEmployee = await _employeeRepository.UpdateAsync(employee, cancellationToken);
        await _employeeRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Employee", employeeId, "EXIT_COMPLETED", 
            $"Exit process completed for employee {employee.EmployeeId}", cancellationToken);

        _logger.LogInformation("Exit process completed for employee: {EmployeeId}", employee.EmployeeId);
        return updatedEmployee;
    }

    public async Task<bool> IsEmployeeIdUniqueAsync(string employeeId, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _employeeRepository.IsEmployeeIdUniqueAsync(employeeId, excludeId, cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await _employeeRepository.IsEmailUniqueAsync(email, excludeId, cancellationToken);
    }

    public async Task<string> GenerateEmployeeIdAsync(int branchId, CancellationToken cancellationToken = default)
    {
        var branch = await _branchRepository.GetByIdAsync(branchId, cancellationToken);
        if (branch == null)
        {
            throw new ArgumentException($"Branch with ID {branchId} not found");
        }

        var sequence = await _employeeRepository.GetNextEmployeeSequenceAsync(branchId, cancellationToken);
        var year = DateTime.Now.Year.ToString().Substring(2); // Last 2 digits of year
        
        // Format: [BranchCode]-EMP-[YY]-[Sequence]
        // Example: NYC-EMP-25-001
        var branchCode = branch.Name.Length >= 3 ? branch.Name.Substring(0, 3).ToUpper() : branch.Name.ToUpper();
        return $"{branchCode}-EMP-{year}-{sequence:D3}";
    }

    public async Task<IEnumerable<Employee>> GetOrganizationalHierarchyAsync(int? rootEmployeeId = null, CancellationToken cancellationToken = default)
    {
        return await _employeeRepository.GetHierarchyAsync(rootEmployeeId, cancellationToken);
    }

    public async Task<bool> ValidateReportingStructureAsync(int employeeId, int? newManagerId, CancellationToken cancellationToken = default)
    {
        if (!newManagerId.HasValue)
        {
            return true; // No manager is valid
        }

        // Check if the new manager exists
        var manager = await _employeeRepository.GetByIdAsync(newManagerId.Value, cancellationToken);
        if (manager == null)
        {
            return false;
        }

        // Check for circular reference
        return !await _employeeRepository.IsCircularReferenceAsync(employeeId, newManagerId, cancellationToken);
    }
}