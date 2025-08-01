using System.Linq;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Services;

namespace StrideHR.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

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

    public async Task UpdateAsync(Employee employee)
    {
        await _unitOfWork.Employees.UpdateAsync(employee);
        await _unitOfWork.SaveChangesAsync();
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
}