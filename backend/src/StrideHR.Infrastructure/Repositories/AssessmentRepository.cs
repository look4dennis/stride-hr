using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class AssessmentRepository : Repository<Assessment>, IAssessmentRepository
{
    public AssessmentRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Assessment>> GetAssessmentsByModuleAsync(int moduleId)
    {
        return await _context.Assessments
            .Where(a => a.TrainingModuleId == moduleId)
            .Include(a => a.CreatedByEmployee)
            .OrderBy(a => a.Title)
            .ToListAsync();
    }

    public async Task<Assessment?> GetAssessmentWithQuestionsAsync(int assessmentId)
    {
        return await _context.Assessments
            .Include(a => a.Questions.Where(q => q.IsActive))
            .Include(a => a.TrainingModule)
            .Include(a => a.CreatedByEmployee)
            .FirstOrDefaultAsync(a => a.Id == assessmentId);
    }

    public async Task<IEnumerable<Assessment>> GetActiveAssessmentsAsync()
    {
        return await _context.Assessments
            .Where(a => a.IsActive)
            .Include(a => a.TrainingModule)
            .Include(a => a.CreatedByEmployee)
            .OrderBy(a => a.Title)
            .ToListAsync();
    }

    public async Task<Assessment?> GetAssessmentWithAttemptsAsync(int assessmentId, int employeeId)
    {
        return await _context.Assessments
            .Include(a => a.Attempts.Where(att => att.EmployeeId == employeeId))
            .Include(a => a.Questions.Where(q => q.IsActive))
            .Include(a => a.TrainingModule)
            .FirstOrDefaultAsync(a => a.Id == assessmentId);
    }

    public async Task<bool> CanEmployeeRetakeAssessmentAsync(int assessmentId, int employeeId)
    {
        var assessment = await _context.Assessments
            .FirstOrDefaultAsync(a => a.Id == assessmentId);

        if (assessment == null) return false;

        var attemptCount = await _context.AssessmentAttempts
            .CountAsync(att => att.AssessmentId == assessmentId && att.EmployeeId == employeeId);

        if (attemptCount >= assessment.MaxAttempts) return false;

        var lastAttempt = await _context.AssessmentAttempts
            .Where(att => att.AssessmentId == assessmentId && att.EmployeeId == employeeId)
            .OrderByDescending(att => att.CompletedAt)
            .FirstOrDefaultAsync();

        if (lastAttempt?.CompletedAt == null) return true;

        var waitingPeriodEnd = lastAttempt.CompletedAt.Value.AddHours(assessment.RetakeWaitingPeriodHours);
        return DateTime.UtcNow >= waitingPeriodEnd;
    }

    public async Task<DateTime?> GetNextRetakeTimeAsync(int assessmentId, int employeeId)
    {
        var assessment = await _context.Assessments
            .FirstOrDefaultAsync(a => a.Id == assessmentId);

        if (assessment == null) return null;

        var lastAttempt = await _context.AssessmentAttempts
            .Where(att => att.AssessmentId == assessmentId && att.EmployeeId == employeeId)
            .OrderByDescending(att => att.CompletedAt)
            .FirstOrDefaultAsync();

        if (lastAttempt?.CompletedAt == null) return null;

        return lastAttempt.CompletedAt.Value.AddHours(assessment.RetakeWaitingPeriodHours);
    }
}

public class AssessmentAttemptRepository : Repository<AssessmentAttempt>, IAssessmentAttemptRepository
{
    public AssessmentAttemptRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AssessmentAttempt>> GetAttemptsByEmployeeAsync(int employeeId)
    {
        return await _context.AssessmentAttempts
            .Where(att => att.EmployeeId == employeeId)
            .Include(att => att.Assessment)
                .ThenInclude(a => a.TrainingModule)
            .Include(att => att.Employee)
            .OrderByDescending(att => att.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<AssessmentAttempt>> GetAttemptsByAssessmentAsync(int assessmentId)
    {
        return await _context.AssessmentAttempts
            .Where(att => att.AssessmentId == assessmentId)
            .Include(att => att.Employee)
            .OrderByDescending(att => att.StartedAt)
            .ToListAsync();
    }

    public async Task<AssessmentAttempt?> GetAttemptWithAnswersAsync(int attemptId)
    {
        return await _context.AssessmentAttempts
            .Include(att => att.Answers)
                .ThenInclude(ans => ans.AssessmentQuestion)
            .Include(att => att.Assessment)
                .ThenInclude(a => a.Questions)
            .Include(att => att.Employee)
            .FirstOrDefaultAsync(att => att.Id == attemptId);
    }

    public async Task<AssessmentAttempt?> GetLatestAttemptAsync(int assessmentId, int employeeId)
    {
        return await _context.AssessmentAttempts
            .Where(att => att.AssessmentId == assessmentId && att.EmployeeId == employeeId)
            .Include(att => att.Assessment)
            .OrderByDescending(att => att.StartedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetAttemptCountAsync(int assessmentId, int employeeId)
    {
        return await _context.AssessmentAttempts
            .CountAsync(att => att.AssessmentId == assessmentId && att.EmployeeId == employeeId);
    }

    public async Task<IEnumerable<AssessmentAttempt>> GetInProgressAttemptsAsync()
    {
        return await _context.AssessmentAttempts
            .Where(att => att.Status == AssessmentAttemptStatus.InProgress)
            .Include(att => att.Assessment)
            .Include(att => att.Employee)
            .ToListAsync();
    }
}

public class CertificationRepository : Repository<Certification>, ICertificationRepository
{
    public CertificationRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Certification>> GetCertificationsByEmployeeAsync(int employeeId)
    {
        return await _context.Certifications
            .Where(c => c.EmployeeId == employeeId)
            .Include(c => c.TrainingModule)
            .Include(c => c.IssuedByEmployee)
            .OrderByDescending(c => c.IssuedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Certification>> GetCertificationsByModuleAsync(int moduleId)
    {
        return await _context.Certifications
            .Where(c => c.TrainingModuleId == moduleId)
            .Include(c => c.Employee)
            .Include(c => c.IssuedByEmployee)
            .OrderByDescending(c => c.IssuedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Certification>> GetExpiringSoonCertificationsAsync(int daysAhead = 30)
    {
        var targetDate = DateTime.Today.AddDays(daysAhead);
        return await _context.Certifications
            .Where(c => c.ExpiryDate.HasValue && 
                       c.ExpiryDate.Value <= targetDate && 
                       c.ExpiryDate.Value >= DateTime.Today &&
                       c.Status == CertificationStatus.Active)
            .Include(c => c.Employee)
            .Include(c => c.TrainingModule)
            .OrderBy(c => c.ExpiryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Certification>> GetExpiredCertificationsAsync()
    {
        return await _context.Certifications
            .Where(c => c.ExpiryDate.HasValue && 
                       c.ExpiryDate.Value < DateTime.Today &&
                       c.Status == CertificationStatus.Active)
            .Include(c => c.Employee)
            .Include(c => c.TrainingModule)
            .ToListAsync();
    }

    public async Task<IEnumerable<Certification>> GetActiveCertificationsAsync()
    {
        return await _context.Certifications
            .Where(c => c.Status == CertificationStatus.Active)
            .Include(c => c.Employee)
            .Include(c => c.TrainingModule)
            .Include(c => c.IssuedByEmployee)
            .OrderByDescending(c => c.IssuedDate)
            .ToListAsync();
    }

    public async Task<bool> HasValidCertificationAsync(int employeeId, int moduleId)
    {
        return await _context.Certifications
            .AnyAsync(c => c.EmployeeId == employeeId && 
                          c.TrainingModuleId == moduleId &&
                          c.Status == CertificationStatus.Active &&
                          (!c.ExpiryDate.HasValue || c.ExpiryDate.Value >= DateTime.Today));
    }

    public async Task<string> GenerateCertificationNumberAsync()
    {
        var year = DateTime.Now.Year;
        var count = await _context.Certifications
            .CountAsync(c => c.IssuedDate.Year == year);
        
        return $"CERT-{year}-{(count + 1):D6}";
    }
}