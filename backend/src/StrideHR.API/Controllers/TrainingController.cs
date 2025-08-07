using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[Authorize]
public class TrainingController : BaseController
{
    private readonly ITrainingService _trainingService;
    private readonly ILogger<TrainingController> _logger;

    public TrainingController(ITrainingService trainingService, ILogger<TrainingController> logger)
    {
        _trainingService = trainingService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    #region Training Module Management

    /// <summary>
    /// Create a new training module
    /// </summary>
    [HttpPost("modules")]
    [Authorize(Policy = "CanManageTraining")]
    public async Task<IActionResult> CreateTrainingModule([FromBody] CreateTrainingModuleDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var module = await _trainingService.CreateTrainingModuleAsync(dto, currentUserId);
            return Success(module, "Training module created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating training module");
            return Error("Failed to create training module");
        }
    }

    /// <summary>
    /// Update an existing training module
    /// </summary>
    [HttpPut("modules/{moduleId}")]
    [Authorize(Policy = "CanManageTraining")]
    public async Task<IActionResult> UpdateTrainingModule(int moduleId, [FromBody] UpdateTrainingModuleDto dto)
    {
        try
        {
            var module = await _trainingService.UpdateTrainingModuleAsync(moduleId, dto);
            return Success(module, "Training module updated successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating training module {ModuleId}", moduleId);
            return Error("Failed to update training module");
        }
    }

    /// <summary>
    /// Delete a training module
    /// </summary>
    [HttpDelete("modules/{moduleId}")]
    [Authorize(Policy = "CanManageTraining")]
    public async Task<IActionResult> DeleteTrainingModule(int moduleId)
    {
        try
        {
            var result = await _trainingService.DeleteTrainingModuleAsync(moduleId);
            if (!result)
                return Error("Training module not found");

            return Success("Training module deleted successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting training module {ModuleId}", moduleId);
            return Error("Failed to delete training module");
        }
    }

    /// <summary>
    /// Get a specific training module
    /// </summary>
    [HttpGet("modules/{moduleId}")]
    public async Task<IActionResult> GetTrainingModule(int moduleId)
    {
        try
        {
            var module = await _trainingService.GetTrainingModuleAsync(moduleId);
            if (module == null)
                return Error("Training module not found");

            return Success(module);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving training module {ModuleId}", moduleId);
            return Error("Failed to retrieve training module");
        }
    }

    /// <summary>
    /// Get all training modules
    /// </summary>
    [HttpGet("modules")]
    public async Task<IActionResult> GetAllTrainingModules([FromQuery] bool activeOnly = false)
    {
        try
        {
            var modules = activeOnly 
                ? await _trainingService.GetActiveTrainingModulesAsync()
                : await _trainingService.GetAllTrainingModulesAsync();

            return Success(modules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving training modules");
            return Error("Failed to retrieve training modules");
        }
    }

    /// <summary>
    /// Search training modules
    /// </summary>
    [HttpGet("modules/search")]
    public async Task<IActionResult> SearchTrainingModules([FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Error("Search term is required");

            var modules = await _trainingService.SearchTrainingModulesAsync(searchTerm);
            return Success(modules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching training modules with term: {SearchTerm}", searchTerm);
            return Error("Failed to search training modules");
        }
    }

    /// <summary>
    /// Upload content for a training module
    /// </summary>
    [HttpPost("modules/{moduleId}/content")]
    [Authorize(Policy = "CanManageTraining")]
    [ApiExplorerSettings(IgnoreApi = true)] // Temporarily exclude from Swagger due to IFormFile issue
    public async Task<IActionResult> UploadTrainingContent(int moduleId, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return Error("File is required");

            using var stream = file.OpenReadStream();
            var filePath = await _trainingService.UploadTrainingContentAsync(moduleId, stream, file.FileName);
            
            return Success(new { FilePath = filePath }, "Content uploaded successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading content for module {ModuleId}", moduleId);
            return Error("Failed to upload content");
        }
    }

    /// <summary>
    /// Delete content from a training module
    /// </summary>
    [HttpDelete("modules/{moduleId}/content")]
    [Authorize(Policy = "CanManageTraining")]
    public async Task<IActionResult> DeleteTrainingContent(int moduleId, [FromQuery] string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return Error("File name is required");

            var result = await _trainingService.DeleteTrainingContentAsync(moduleId, fileName);
            if (!result)
                return Error("Content not found");

            return Success("Content deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting content from module {ModuleId}", moduleId);
            return Error("Failed to delete content");
        }
    }

    #endregion

    #region Training Assignment Management

    /// <summary>
    /// Assign training to employees
    /// </summary>
    [HttpPost("assignments")]
    [Authorize(Policy = "CanAssignTraining")]
    public async Task<IActionResult> AssignTraining([FromBody] CreateTrainingAssignmentDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var assignments = await _trainingService.AssignTrainingToEmployeesAsync(dto, currentUserId);
            return Success(assignments, "Training assigned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning training");
            return Error("Failed to assign training");
        }
    }

    /// <summary>
    /// Bulk assign training to multiple groups
    /// </summary>
    [HttpPost("assignments/bulk")]
    [Authorize(Policy = "CanAssignTraining")]
    public async Task<IActionResult> BulkAssignTraining([FromBody] BulkTrainingAssignmentDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var assignments = await _trainingService.BulkAssignTrainingAsync(dto, currentUserId);
            return Success(assignments, "Training bulk assigned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk assigning training");
            return Error("Failed to bulk assign training");
        }
    }

    /// <summary>
    /// Cancel a training assignment
    /// </summary>
    [HttpPut("assignments/{assignmentId}/cancel")]
    [Authorize(Policy = "CanAssignTraining")]
    public async Task<IActionResult> CancelTrainingAssignment(int assignmentId)
    {
        try
        {
            var result = await _trainingService.CancelTrainingAssignmentAsync(assignmentId);
            if (!result)
                return Error("Training assignment not found");

            return Success("Training assignment cancelled successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling training assignment {AssignmentId}", assignmentId);
            return Error("Failed to cancel training assignment");
        }
    }

    /// <summary>
    /// Get training assignments for an employee
    /// </summary>
    [HttpGet("assignments/employee/{employeeId}")]
    public async Task<IActionResult> GetEmployeeTrainingAssignments(int employeeId)
    {
        try
        {
            var assignments = await _trainingService.GetEmployeeTrainingAssignmentsAsync(employeeId);
            return Success(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving training assignments for employee {EmployeeId}", employeeId);
            return Error("Failed to retrieve training assignments");
        }
    }

    /// <summary>
    /// Get assignments for a training module
    /// </summary>
    [HttpGet("assignments/module/{moduleId}")]
    [Authorize(Policy = "CanViewTrainingReports")]
    public async Task<IActionResult> GetModuleAssignments(int moduleId)
    {
        try
        {
            var assignments = await _trainingService.GetModuleAssignmentsAsync(moduleId);
            return Success(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignments for module {ModuleId}", moduleId);
            return Error("Failed to retrieve module assignments");
        }
    }

    /// <summary>
    /// Get overdue training assignments
    /// </summary>
    [HttpGet("assignments/overdue")]
    [Authorize(Policy = "CanViewTrainingReports")]
    public async Task<IActionResult> GetOverdueAssignments()
    {
        try
        {
            var assignments = await _trainingService.GetOverdueAssignmentsAsync();
            return Success(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue assignments");
            return Error("Failed to retrieve overdue assignments");
        }
    }

    /// <summary>
    /// Get assignments due soon
    /// </summary>
    [HttpGet("assignments/due-soon")]
    [Authorize(Policy = "CanViewTrainingReports")]
    public async Task<IActionResult> GetAssignmentsDueSoon([FromQuery] int daysAhead = 7)
    {
        try
        {
            var assignments = await _trainingService.GetAssignmentsDueSoonAsync(daysAhead);
            return Success(assignments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignments due soon");
            return Error("Failed to retrieve assignments due soon");
        }
    }

    #endregion

    #region Training Progress Management

    /// <summary>
    /// Start a training assignment
    /// </summary>
    [HttpPost("assignments/{assignmentId}/start")]
    public async Task<IActionResult> StartTraining(int assignmentId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _trainingService.StartTrainingAsync(assignmentId, currentUserId);
            
            if (!result)
                return Error("Unable to start training. Check assignment status and prerequisites.");

            return Success("Training started successfully");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting training assignment {AssignmentId}", assignmentId);
            return Error("Failed to start training");
        }
    }

    /// <summary>
    /// Update training progress
    /// </summary>
    [HttpPut("assignments/{assignmentId}/progress")]
    public async Task<IActionResult> UpdateTrainingProgress(int assignmentId, [FromBody] decimal progressPercentage)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _trainingService.UpdateTrainingProgressAsync(assignmentId, currentUserId, progressPercentage);
            
            if (!result)
                return Error("Unable to update training progress");

            return Success("Training progress updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating training progress for assignment {AssignmentId}", assignmentId);
            return Error("Failed to update training progress");
        }
    }

    /// <summary>
    /// Complete a training assignment
    /// </summary>
    [HttpPost("assignments/{assignmentId}/complete")]
    public async Task<IActionResult> CompleteTraining(int assignmentId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _trainingService.CompleteTrainingAsync(assignmentId, currentUserId);
            
            if (!result)
                return Error("Unable to complete training");

            return Success("Training completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing training assignment {AssignmentId}", assignmentId);
            return Error("Failed to complete training");
        }
    }

    /// <summary>
    /// Get training progress for an assignment
    /// </summary>
    [HttpGet("assignments/{assignmentId}/progress")]
    public async Task<IActionResult> GetTrainingProgress(int assignmentId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var progress = await _trainingService.GetTrainingProgressAsync(assignmentId, currentUserId);
            
            if (progress == null)
                return Error("Training progress not found");

            return Success(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving training progress for assignment {AssignmentId}", assignmentId);
            return Error("Failed to retrieve training progress");
        }
    }

    #endregion

    #region Assessment Management

    /// <summary>
    /// Create an assessment for a training module
    /// </summary>
    [HttpPost("assessments")]
    [Authorize(Policy = "CanManageTraining")]
    public async Task<IActionResult> CreateAssessment([FromBody] CreateAssessmentDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var assessment = await _trainingService.CreateAssessmentAsync(dto, currentUserId);
            return Success(assessment, "Assessment created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating assessment");
            return Error("Failed to create assessment");
        }
    }

    /// <summary>
    /// Update an assessment
    /// </summary>
    [HttpPut("assessments/{assessmentId}")]
    [Authorize(Policy = "CanManageTraining")]
    public async Task<IActionResult> UpdateAssessment(int assessmentId, [FromBody] CreateAssessmentDto dto)
    {
        try
        {
            var assessment = await _trainingService.UpdateAssessmentAsync(assessmentId, dto);
            return Success(assessment, "Assessment updated successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating assessment {AssessmentId}", assessmentId);
            return Error("Failed to update assessment");
        }
    }

    /// <summary>
    /// Delete an assessment
    /// </summary>
    [HttpDelete("assessments/{assessmentId}")]
    [Authorize(Policy = "CanManageTraining")]
    public async Task<IActionResult> DeleteAssessment(int assessmentId)
    {
        try
        {
            var result = await _trainingService.DeleteAssessmentAsync(assessmentId);
            if (!result)
                return Error("Assessment not found");

            return Success("Assessment deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting assessment {AssessmentId}", assessmentId);
            return Error("Failed to delete assessment");
        }
    }

    /// <summary>
    /// Get a specific assessment
    /// </summary>
    [HttpGet("assessments/{assessmentId}")]
    public async Task<IActionResult> GetAssessment(int assessmentId)
    {
        try
        {
            var assessment = await _trainingService.GetAssessmentAsync(assessmentId);
            if (assessment == null)
                return Error("Assessment not found");

            return Success(assessment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assessment {AssessmentId}", assessmentId);
            return Error("Failed to retrieve assessment");
        }
    }

    /// <summary>
    /// Get assessments for a training module
    /// </summary>
    [HttpGet("modules/{moduleId}/assessments")]
    public async Task<IActionResult> GetModuleAssessments(int moduleId)
    {
        try
        {
            var assessments = await _trainingService.GetModuleAssessmentsAsync(moduleId);
            return Success(assessments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assessments for module {ModuleId}", moduleId);
            return Error("Failed to retrieve module assessments");
        }
    }

    #endregion

    #region Assessment Attempt Management

    /// <summary>
    /// Start an assessment attempt
    /// </summary>
    [HttpPost("assessments/{assessmentId}/attempts")]
    public async Task<IActionResult> StartAssessmentAttempt(int assessmentId, [FromBody] int trainingProgressId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Check if employee can retake assessment
            var canRetake = await _trainingService.CanEmployeeRetakeAssessmentAsync(assessmentId, currentUserId);
            if (!canRetake)
                return Error("Maximum attempts reached or waiting period not elapsed");

            var attemptId = await _trainingService.StartAssessmentAttemptAsync(assessmentId, currentUserId, trainingProgressId);
            return Success(new { AttemptId = attemptId }, "Assessment attempt started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting assessment attempt for assessment {AssessmentId}", assessmentId);
            return Error("Failed to start assessment attempt");
        }
    }

    /// <summary>
    /// Submit an answer for an assessment question
    /// </summary>
    [HttpPost("attempts/{attemptId}/answers")]
    public async Task<IActionResult> SubmitAssessmentAnswer(int attemptId, [FromBody] SubmitAnswerDto dto)
    {
        try
        {
            var result = await _trainingService.SubmitAssessmentAnswerAsync(attemptId, dto.QuestionId, dto.Answers);
            if (!result)
                return Error("Failed to submit answer");

            return Success("Answer submitted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting answer for attempt {AttemptId}", attemptId);
            return Error("Failed to submit answer");
        }
    }

    /// <summary>
    /// Complete an assessment attempt
    /// </summary>
    [HttpPost("attempts/{attemptId}/complete")]
    public async Task<IActionResult> CompleteAssessmentAttempt(int attemptId)
    {
        try
        {
            var attempt = await _trainingService.CompleteAssessmentAttemptAsync(attemptId);
            return Success(attempt, "Assessment attempt completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing assessment attempt {AttemptId}", attemptId);
            return Error("Failed to complete assessment attempt");
        }
    }

    /// <summary>
    /// Get employee's assessment attempts
    /// </summary>
    [HttpGet("employees/{employeeId}/attempts")]
    public async Task<IActionResult> GetEmployeeAssessmentAttempts(int employeeId)
    {
        try
        {
            var attempts = await _trainingService.GetEmployeeAssessmentAttemptsAsync(employeeId);
            return Success(attempts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assessment attempts for employee {EmployeeId}", employeeId);
            return Error("Failed to retrieve assessment attempts");
        }
    }

    #endregion

    #region Certification Management

    /// <summary>
    /// Issue a certification
    /// </summary>
    [HttpPost("certifications")]
    [Authorize(Policy = "CanIssueCertifications")]
    public async Task<IActionResult> IssueCertification([FromBody] CreateCertificationDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var certification = await _trainingService.IssueCertificationAsync(dto, currentUserId);
            return Success(certification, "Certification issued successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error issuing certification");
            return Error("Failed to issue certification");
        }
    }

    /// <summary>
    /// Revoke a certification
    /// </summary>
    [HttpPut("certifications/{certificationId}/revoke")]
    [Authorize(Policy = "CanIssueCertifications")]
    public async Task<IActionResult> RevokeCertification(int certificationId, [FromBody] string reason)
    {
        try
        {
            var result = await _trainingService.RevokeCertificationAsync(certificationId, reason);
            if (!result)
                return Error("Certification not found");

            return Success("Certification revoked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking certification {CertificationId}", certificationId);
            return Error("Failed to revoke certification");
        }
    }

    /// <summary>
    /// Get employee certifications
    /// </summary>
    [HttpGet("employees/{employeeId}/certifications")]
    public async Task<IActionResult> GetEmployeeCertifications(int employeeId)
    {
        try
        {
            var certifications = await _trainingService.GetEmployeeCertificationsAsync(employeeId);
            return Success(certifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving certifications for employee {EmployeeId}", employeeId);
            return Error("Failed to retrieve employee certifications");
        }
    }

    /// <summary>
    /// Get certifications expiring soon
    /// </summary>
    [HttpGet("certifications/expiring-soon")]
    [Authorize(Policy = "CanViewTrainingReports")]
    public async Task<IActionResult> GetExpiringSoonCertifications([FromQuery] int daysAhead = 30)
    {
        try
        {
            var certifications = await _trainingService.GetExpiringSoonCertificationsAsync(daysAhead);
            return Success(certifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expiring certifications");
            return Error("Failed to retrieve expiring certifications");
        }
    }

    /// <summary>
    /// Renew a certification
    /// </summary>
    [HttpPut("certifications/{certificationId}/renew")]
    [Authorize(Policy = "CanIssueCertifications")]
    public async Task<IActionResult> RenewCertification(int certificationId, [FromBody] DateTime newExpiryDate)
    {
        try
        {
            var result = await _trainingService.RenewCertificationAsync(certificationId, newExpiryDate);
            if (!result)
                return Error("Certification not found");

            return Success("Certification renewed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing certification {CertificationId}", certificationId);
            return Error("Failed to renew certification");
        }
    }

    /// <summary>
    /// Generate certificate file
    /// </summary>
    [HttpPost("certifications/{certificationId}/generate-certificate")]
    [Authorize(Policy = "CanIssueCertifications")]
    public async Task<IActionResult> GenerateCertificate(int certificationId)
    {
        try
        {
            var certificatePath = await _trainingService.GenerateCertificateAsync(certificationId);
            return Success(new { CertificatePath = certificatePath }, "Certificate generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating certificate for certification {CertificationId}", certificationId);
            return Error("Failed to generate certificate");
        }
    }

    #endregion

    #region Reporting and Analytics

    /// <summary>
    /// Get comprehensive training report
    /// </summary>
    [HttpGet("reports/training")]
    [Authorize(Policy = "CanViewTrainingReports")]
    public async Task<IActionResult> GetTrainingReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var report = await _trainingService.GetTrainingReportAsync(startDate, endDate);
            return Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating training report");
            return Error("Failed to generate training report");
        }
    }

    /// <summary>
    /// Get training module statistics
    /// </summary>
    [HttpGet("reports/module-statistics")]
    [Authorize(Policy = "CanViewTrainingReports")]
    public async Task<IActionResult> GetModuleStatistics()
    {
        try
        {
            var stats = await _trainingService.GetModuleStatisticsAsync();
            return Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module statistics");
            return Error("Failed to retrieve module statistics");
        }
    }

    /// <summary>
    /// Get employee training statistics
    /// </summary>
    [HttpGet("reports/employee-statistics")]
    [Authorize(Policy = "CanViewTrainingReports")]
    public async Task<IActionResult> GetEmployeeTrainingStatistics()
    {
        try
        {
            var stats = await _trainingService.GetEmployeeTrainingStatisticsAsync();
            return Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee training statistics");
            return Error("Failed to retrieve employee training statistics");
        }
    }

    /// <summary>
    /// Export training report
    /// </summary>
    [HttpGet("reports/export")]
    [Authorize(Policy = "CanViewTrainingReports")]
    public async Task<IActionResult> ExportTrainingReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var reportData = await _trainingService.ExportTrainingReportAsync(startDate, endDate);
            return File(reportData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                       $"training-report-{DateTime.Now:yyyy-MM-dd}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting training report");
            return Error("Failed to export training report");
        }
    }

    #endregion

    #region Notifications and Reminders

    /// <summary>
    /// Send training reminder
    /// </summary>
    [HttpPost("assignments/{assignmentId}/remind")]
    [Authorize(Policy = "CanAssignTraining")]
    public async Task<IActionResult> SendTrainingReminder(int assignmentId)
    {
        try
        {
            await _trainingService.SendTrainingReminderAsync(assignmentId);
            return Success("Training reminder sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending training reminder for assignment {AssignmentId}", assignmentId);
            return Error("Failed to send training reminder");
        }
    }

    /// <summary>
    /// Send certification expiry reminder
    /// </summary>
    [HttpPost("certifications/{certificationId}/remind")]
    [Authorize(Policy = "CanIssueCertifications")]
    public async Task<IActionResult> SendCertificationExpiryReminder(int certificationId)
    {
        try
        {
            await _trainingService.SendCertificationExpiryReminderAsync(certificationId);
            return Success("Certification expiry reminder sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending certification expiry reminder for certification {CertificationId}", certificationId);
            return Error("Failed to send certification expiry reminder");
        }
    }

    #endregion
}

/// <summary>
/// DTO for submitting assessment answers
/// </summary>
public class SubmitAnswerDto
{
    public int QuestionId { get; set; }
    public List<string> Answers { get; set; } = new();
}