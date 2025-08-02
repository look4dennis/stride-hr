using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class TrainingAssignmentRepository : Repository<TrainingAssignment>, ITrainingAssignmentRepository
{
    public TrainingAssignmentRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TrainingAssignment>> GetAssignmentsByEmployeeAsync(int employeeId)
    {
        return await _context.TrainingAssignments
            .Where(ta => ta.EmployeeId == employeeId)
            .Include(ta => ta.TrainingModule)
            .Include(ta => ta.AssignedByEmployee)
            .Include(ta => ta.TrainingProgress)
            .OrderByDescending(ta => ta.AssignedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrainingAssignment>> GetAssignmentsByModuleAsync(int moduleId)
    {
        return await _context.TrainingAssignments
            .Where(ta => ta.TrainingModuleId == moduleId)
            .Include(ta => ta.Employee)
            .Include(ta => ta.AssignedByEmployee)
            .Include(ta => ta.TrainingProgress)
            .OrderByDescending(ta => ta.AssignedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrainingAssignment>> GetOverdueAssignmentsAsync()
    {
        var today = DateTime.Today;
        return await _context.TrainingAssignments
            .Where(ta => ta.DueDate.HasValue && 
                        ta.DueDate.Value < today && 
                        ta.Status != TrainingAssignmentStatus.Completed &&
                        ta.Status != TrainingAssignmentStatus.Cancelled)
            .Include(ta => ta.TrainingModule)
            .Include(ta => ta.Employee)
            .Include(ta => ta.AssignedByEmployee)
            .OrderBy(ta => ta.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrainingAssignment>> GetAssignmentsByStatusAsync(TrainingAssignmentStatus status)
    {
        return await _context.TrainingAssignments
            .Where(ta => ta.Status == status)
            .Include(ta => ta.TrainingModule)
            .Include(ta => ta.Employee)
            .Include(ta => ta.AssignedByEmployee)
            .Include(ta => ta.TrainingProgress)
            .OrderByDescending(ta => ta.AssignedAt)
            .ToListAsync();
    }

    public async Task<TrainingAssignment?> GetAssignmentWithProgressAsync(int assignmentId)
    {
        return await _context.TrainingAssignments
            .Include(ta => ta.TrainingModule)
            .Include(ta => ta.Employee)
            .Include(ta => ta.AssignedByEmployee)
            .Include(ta => ta.TrainingProgress)
                .ThenInclude(tp => tp.AssessmentAttempts)
            .FirstOrDefaultAsync(ta => ta.Id == assignmentId);
    }

    public async Task<bool> IsEmployeeAssignedToModuleAsync(int employeeId, int moduleId)
    {
        return await _context.TrainingAssignments
            .AnyAsync(ta => ta.EmployeeId == employeeId && 
                           ta.TrainingModuleId == moduleId &&
                           ta.Status != TrainingAssignmentStatus.Cancelled);
    }

    public async Task<IEnumerable<TrainingAssignment>> GetAssignmentsDueSoonAsync(int daysAhead = 7)
    {
        var targetDate = DateTime.Today.AddDays(daysAhead);
        return await _context.TrainingAssignments
            .Where(ta => ta.DueDate.HasValue && 
                        ta.DueDate.Value <= targetDate && 
                        ta.DueDate.Value >= DateTime.Today &&
                        ta.Status != TrainingAssignmentStatus.Completed &&
                        ta.Status != TrainingAssignmentStatus.Cancelled)
            .Include(ta => ta.TrainingModule)
            .Include(ta => ta.Employee)
            .Include(ta => ta.AssignedByEmployee)
            .OrderBy(ta => ta.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrainingAssignment>> GetAssignmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.TrainingAssignments
            .Where(ta => ta.AssignedAt >= startDate && ta.AssignedAt <= endDate)
            .Include(ta => ta.TrainingModule)
            .Include(ta => ta.Employee)
            .Include(ta => ta.AssignedByEmployee)
            .Include(ta => ta.TrainingProgress)
            .OrderByDescending(ta => ta.AssignedAt)
            .ToListAsync();
    }
}