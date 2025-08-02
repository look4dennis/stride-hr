using StrideHR.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models;

public class CertificationDto
{
    public int Id { get; set; }
    public int TrainingModuleId { get; set; }
    public string TrainingModuleTitle { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public string CertificationName { get; set; } = string.Empty;
    public string CertificationNumber { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public CertificationStatus Status { get; set; }
    public string? CertificateFilePath { get; set; }
    public int IssuedBy { get; set; }
    public string IssuedByName { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public string? Notes { get; set; }
    public bool IsExternalCertification { get; set; }
    public string? ExternalProvider { get; set; }
    public string? VerificationUrl { get; set; }
    public bool IsExpiringSoon { get; set; }
    public int DaysUntilExpiry { get; set; }
}

public class CreateCertificationDto
{
    [Required]
    public int TrainingModuleId { get; set; }
    
    [Required]
    public int EmployeeId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string CertificationName { get; set; } = string.Empty;
    
    public DateTime? ExpiryDate { get; set; }
    
    public decimal? Score { get; set; }
    
    public string? Notes { get; set; }
    
    public bool IsExternalCertification { get; set; }
    
    public string? ExternalProvider { get; set; }
    
    public string? VerificationUrl { get; set; }
}

public class TrainingReportDto
{
    public int TotalModules { get; set; }
    public int ActiveModules { get; set; }
    public int TotalAssignments { get; set; }
    public int CompletedAssignments { get; set; }
    public int OverdueAssignments { get; set; }
    public decimal OverallCompletionRate { get; set; }
    public int TotalCertifications { get; set; }
    public int ExpiringSoonCertifications { get; set; }
    public List<TrainingModuleStatsDto> ModuleStats { get; set; } = new();
    public List<EmployeeTrainingStatsDto> EmployeeStats { get; set; } = new();
}

public class TrainingModuleStatsDto
{
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; } = string.Empty;
    public int AssignedCount { get; set; }
    public int CompletedCount { get; set; }
    public int InProgressCount { get; set; }
    public int OverdueCount { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal AverageScore { get; set; }
    public int AverageCompletionTimeMinutes { get; set; }
}

public class EmployeeTrainingStatsDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int AssignedTrainings { get; set; }
    public int CompletedTrainings { get; set; }
    public int OverdueTrainings { get; set; }
    public decimal CompletionRate { get; set; }
    public int CertificationsEarned { get; set; }
    public int ExpiringSoonCertifications { get; set; }
}