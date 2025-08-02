using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class TrainingModuleRepository : Repository<TrainingModule>, ITrainingModuleRepository
{
    public TrainingModuleRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TrainingModule>> GetActiveModulesAsync()
    {
        return await _context.TrainingModules
            .Where(tm => tm.IsActive)
            .Include(tm => tm.CreatedByEmployee)
            .OrderBy(tm => tm.Title)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrainingModule>> GetMandatoryModulesAsync()
    {
        return await _context.TrainingModules
            .Where(tm => tm.IsActive && tm.IsMandatory)
            .Include(tm => tm.CreatedByEmployee)
            .OrderBy(tm => tm.Title)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrainingModule>> GetModulesByTypeAsync(TrainingType type)
    {
        return await _context.TrainingModules
            .Where(tm => tm.IsActive && tm.Type == type)
            .Include(tm => tm.CreatedByEmployee)
            .OrderBy(tm => tm.Title)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrainingModule>> GetModulesByLevelAsync(TrainingLevel level)
    {
        return await _context.TrainingModules
            .Where(tm => tm.IsActive && tm.Level == level)
            .Include(tm => tm.CreatedByEmployee)
            .OrderBy(tm => tm.Title)
            .ToListAsync();
    }

    public async Task<TrainingModule?> GetModuleWithAssignmentsAsync(int moduleId)
    {
        return await _context.TrainingModules
            .Include(tm => tm.TrainingAssignments)
                .ThenInclude(ta => ta.Employee)
            .Include(tm => tm.CreatedByEmployee)
            .FirstOrDefaultAsync(tm => tm.Id == moduleId);
    }

    public async Task<TrainingModule?> GetModuleWithAssessmentsAsync(int moduleId)
    {
        return await _context.TrainingModules
            .Include(tm => tm.Assessments)
                .ThenInclude(a => a.Questions)
            .Include(tm => tm.CreatedByEmployee)
            .FirstOrDefaultAsync(tm => tm.Id == moduleId);
    }

    public async Task<IEnumerable<TrainingModule>> GetPrerequisiteModulesAsync(int moduleId)
    {
        var module = await _context.TrainingModules
            .FirstOrDefaultAsync(tm => tm.Id == moduleId);

        if (module?.PrerequisiteModuleIds?.Any() != true)
            return Enumerable.Empty<TrainingModule>();

        return await _context.TrainingModules
            .Where(tm => module.PrerequisiteModuleIds.Contains(tm.Id))
            .ToListAsync();
    }

    public async Task<bool> HasPrerequisitesCompletedAsync(int moduleId, int employeeId)
    {
        var module = await _context.TrainingModules
            .FirstOrDefaultAsync(tm => tm.Id == moduleId);

        if (module?.PrerequisiteModuleIds?.Any() != true)
            return true;

        var completedPrerequisites = await _context.TrainingAssignments
            .Where(ta => ta.EmployeeId == employeeId && 
                        module.PrerequisiteModuleIds.Contains(ta.TrainingModuleId) &&
                        ta.Status == TrainingAssignmentStatus.Completed)
            .CountAsync();

        return completedPrerequisites == module.PrerequisiteModuleIds.Count;
    }

    public async Task<IEnumerable<TrainingModule>> SearchModulesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetActiveModulesAsync();

        return await _context.TrainingModules
            .Where(tm => tm.IsActive && 
                        (tm.Title.Contains(searchTerm) || 
                         tm.Description.Contains(searchTerm)))
            .Include(tm => tm.CreatedByEmployee)
            .OrderBy(tm => tm.Title)
            .ToListAsync();
    }
}