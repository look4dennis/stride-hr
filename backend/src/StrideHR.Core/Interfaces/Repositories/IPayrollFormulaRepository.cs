using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IPayrollFormulaRepository : IRepository<PayrollFormula>
{
    Task<List<PayrollFormula>> GetActiveFormulasAsync();
    Task<List<PayrollFormula>> GetFormulasByTypeAsync(PayrollFormulaType type);
    Task<List<PayrollFormula>> GetFormulasForEmployeeAsync(int employeeId);
    Task<List<PayrollFormula>> GetFormulasByBranchAsync(int branchId);
    Task<List<PayrollFormula>> GetFormulasByOrganizationAsync(int organizationId);
}