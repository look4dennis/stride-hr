using StrideHR.Core.Entities;
using StrideHR.Core.Models;

namespace StrideHR.Core.Interfaces.Services;

public interface ITrainingService
{
    // Training Module Management
    Task<TrainingModuleDto> CreateTrainingModuleAsync(CreateTrainingModuleDto dto, int createdBy);
    Task<TrainingModuleDto> UpdateTrainingModuleAsync(int moduleId, UpdateTrainingModuleDto dto);
    Task<bool> DeleteTrainingModuleAsync(int moduleId);
    Task<TrainingModuleDto?> GetTrainingModuleAsync(int moduleId);
    Task<IEnumerable<TrainingModuleDto>> GetAllTrainingModulesAsync();
    Task<IEnumerable<TrainingModuleDto>> GetActiveTrainingModulesAsync();
    Task<IEnumerable<TrainingModuleDto>> SearchTrainingModulesAsync(string searchTerm);
    Task<string> UploadTrainingContentAsync(int moduleId, Stream fileStream, string fileName);
    Task<bool> DeleteTrainingContentAsync(int moduleId, string fileName);
    
    // Training Assignment Management
    Task<IEnumerable<TrainingAssignmentDto>> AssignTrainingToEmployeesAsync(CreateTrainingAssignmentDto dto, int assignedBy);
    Task<IEnumerable<TrainingAssignmentDto>> BulkAssignTrainingAsync(BulkTrainingAssignmentDto dto, int assignedBy);
    Task<bool> CancelTrainingAssignmentAsync(int assignmentId);
    Task<IEnumerable<TrainingAssignmentDto>> GetEmployeeTrainingAssignmentsAsync(int employeeId);
    Task<IEnumerable<TrainingAssignmentDto>> GetModuleAssignmentsAsync(int moduleId);
    Task<IEnumerable<TrainingAssignmentDto>> GetOverdueAssignmentsAsync();
    
    // Training Progress Management
    Task<bool> StartTrainingAsync(int assignmentId, int employeeId);
    Task<bool> UpdateTrainingProgressAsync(int assignmentId, int employeeId, decimal progressPercentage);
    Task<bool> CompleteTrainingAsync(int assignmentId, int employeeId);
    Task<TrainingProgress?> GetTrainingProgressAsync(int assignmentId, int employeeId);
    
    // Assessment Management
    Task<AssessmentDto> CreateAssessmentAsync(CreateAssessmentDto dto, int createdBy);
    Task<AssessmentDto> UpdateAssessmentAsync(int assessmentId, CreateAssessmentDto dto);
    Task<bool> DeleteAssessmentAsync(int assessmentId);
    Task<AssessmentDto?> GetAssessmentAsync(int assessmentId);
    Task<IEnumerable<AssessmentDto>> GetModuleAssessmentsAsync(int moduleId);
    
    // Assessment Attempt Management
    Task<int> StartAssessmentAttemptAsync(int assessmentId, int employeeId, int trainingProgressId);
    Task<bool> SubmitAssessmentAnswerAsync(int attemptId, int questionId, List<string> answers);
    Task<AssessmentAttempt> CompleteAssessmentAttemptAsync(int attemptId);
    Task<bool> CanEmployeeRetakeAssessmentAsync(int assessmentId, int employeeId);
    Task<IEnumerable<AssessmentAttempt>> GetEmployeeAssessmentAttemptsAsync(int employeeId);
    
    // Certification Management
    Task<CertificationDto> IssueCertificationAsync(CreateCertificationDto dto, int issuedBy);
    Task<bool> RevokeCertificationAsync(int certificationId, string reason);
    Task<IEnumerable<CertificationDto>> GetEmployeeCertificationsAsync(int employeeId);
    Task<IEnumerable<CertificationDto>> GetExpiringSoonCertificationsAsync(int daysAhead = 30);
    Task<bool> RenewCertificationAsync(int certificationId, DateTime newExpiryDate);
    Task<string> GenerateCertificateAsync(int certificationId);
    
    // Reporting and Analytics
    Task<TrainingReportDto> GetTrainingReportAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<TrainingModuleStatsDto>> GetModuleStatisticsAsync();
    Task<IEnumerable<EmployeeTrainingStatsDto>> GetEmployeeTrainingStatisticsAsync();
    Task<byte[]> ExportTrainingReportAsync(DateTime? startDate = null, DateTime? endDate = null);
    
    // Notification and Reminders
    Task SendTrainingReminderAsync(int assignmentId);
    Task SendCertificationExpiryReminderAsync(int certificationId);
    Task<IEnumerable<TrainingAssignmentDto>> GetAssignmentsDueSoonAsync(int daysAhead = 7);
}