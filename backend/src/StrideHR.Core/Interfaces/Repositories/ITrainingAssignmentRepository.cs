using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface ITrainingAssignmentRepository : IRepository<TrainingAssignment>
{
    Task<IEnumerable<TrainingAssignment>> GetAssignmentsByEmployeeAsync(int employeeId);
    Task<IEnumerable<TrainingAssignment>> GetAssignmentsByModuleAsync(int moduleId);
    Task<IEnumerable<TrainingAssignment>> GetOverdueAssignmentsAsync();
    Task<IEnumerable<TrainingAssignment>> GetAssignmentsByStatusAsync(TrainingAssignmentStatus status);
    Task<TrainingAssignment?> GetAssignmentWithProgressAsync(int assignmentId);
    Task<bool> IsEmployeeAssignedToModuleAsync(int employeeId, int moduleId);
    Task<IEnumerable<TrainingAssignment>> GetAssignmentsDueSoonAsync(int daysAhead = 7);
    Task<IEnumerable<TrainingAssignment>> GetAssignmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
}