using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ITrainingModuleRepository : IRepository<TrainingModule>
{
    Task<IEnumerable<TrainingModule>> GetActiveModulesAsync();
    Task<IEnumerable<TrainingModule>> GetMandatoryModulesAsync();
    Task<IEnumerable<TrainingModule>> GetModulesByTypeAsync(TrainingType type);
    Task<IEnumerable<TrainingModule>> GetModulesByLevelAsync(TrainingLevel level);
    Task<TrainingModule?> GetModuleWithAssignmentsAsync(int moduleId);
    Task<TrainingModule?> GetModuleWithAssessmentsAsync(int moduleId);
    Task<IEnumerable<TrainingModule>> GetPrerequisiteModulesAsync(int moduleId);
    Task<bool> HasPrerequisitesCompletedAsync(int moduleId, int employeeId);
    Task<IEnumerable<TrainingModule>> SearchModulesAsync(string searchTerm);
}