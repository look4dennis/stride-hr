using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IAssessmentRepository : IRepository<Assessment>
{
    Task<IEnumerable<Assessment>> GetAssessmentsByModuleAsync(int moduleId);
    Task<Assessment?> GetAssessmentWithQuestionsAsync(int assessmentId);
    Task<IEnumerable<Assessment>> GetActiveAssessmentsAsync();
    Task<Assessment?> GetAssessmentWithAttemptsAsync(int assessmentId, int employeeId);
    Task<bool> CanEmployeeRetakeAssessmentAsync(int assessmentId, int employeeId);
    Task<DateTime?> GetNextRetakeTimeAsync(int assessmentId, int employeeId);
}

public interface IAssessmentAttemptRepository : IRepository<AssessmentAttempt>
{
    Task<IEnumerable<AssessmentAttempt>> GetAttemptsByEmployeeAsync(int employeeId);
    Task<IEnumerable<AssessmentAttempt>> GetAttemptsByAssessmentAsync(int assessmentId);
    Task<AssessmentAttempt?> GetAttemptWithAnswersAsync(int attemptId);
    Task<AssessmentAttempt?> GetLatestAttemptAsync(int assessmentId, int employeeId);
    Task<int> GetAttemptCountAsync(int assessmentId, int employeeId);
    Task<IEnumerable<AssessmentAttempt>> GetInProgressAttemptsAsync();
}

public interface ICertificationRepository : IRepository<Certification>
{
    Task<IEnumerable<Certification>> GetCertificationsByEmployeeAsync(int employeeId);
    Task<IEnumerable<Certification>> GetCertificationsByModuleAsync(int moduleId);
    Task<IEnumerable<Certification>> GetExpiringSoonCertificationsAsync(int daysAhead = 30);
    Task<IEnumerable<Certification>> GetExpiredCertificationsAsync();
    Task<IEnumerable<Certification>> GetActiveCertificationsAsync();
    Task<bool> HasValidCertificationAsync(int employeeId, int moduleId);
    Task<string> GenerateCertificationNumberAsync();
}