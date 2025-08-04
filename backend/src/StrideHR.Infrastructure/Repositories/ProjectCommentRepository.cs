using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class ProjectCommentRepository : Repository<ProjectComment>, IProjectCommentRepository
{
    public ProjectCommentRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<ProjectComment>> GetProjectCommentsAsync(int projectId)
    {
        return await _context.ProjectComments
            .Where(c => c.ProjectId == projectId)
            .Include(c => c.Employee)
            .Include(c => c.Replies)
                .ThenInclude(r => r.Employee)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ProjectComment>> GetTaskCommentsAsync(int taskId)
    {
        return await _context.ProjectComments
            .Where(c => c.TaskId == taskId)
            .Include(c => c.Employee)
            .Include(c => c.Replies)
                .ThenInclude(r => r.Employee)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<ProjectComment?> GetCommentWithRepliesAsync(int commentId)
    {
        return await _context.ProjectComments
            .Include(c => c.Employee)
            .Include(c => c.Replies)
                .ThenInclude(r => r.Employee)
            .FirstOrDefaultAsync(c => c.Id == commentId);
    }

    public async Task<int> GetProjectCommentsCountAsync(int projectId)
    {
        return await _context.ProjectComments
            .CountAsync(c => c.ProjectId == projectId);
    }
}

public class ProjectCommentReplyRepository : Repository<ProjectCommentReply>, IProjectCommentReplyRepository
{
    public ProjectCommentReplyRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<ProjectCommentReply>> GetCommentRepliesAsync(int commentId)
    {
        return await _context.ProjectCommentReplies
            .Where(r => r.CommentId == commentId)
            .Include(r => r.Employee)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }
}

public class ProjectActivityRepository : Repository<ProjectActivity>, IProjectActivityRepository
{
    public ProjectActivityRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<ProjectActivity>> GetProjectActivitiesAsync(int projectId, int limit = 50)
    {
        return await _context.ProjectActivities
            .Where(a => a.ProjectId == projectId)
            .Include(a => a.Employee)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<ProjectActivity>> GetTeamActivitiesAsync(List<int> projectIds, int limit = 100)
    {
        return await _context.ProjectActivities
            .Where(a => projectIds.Contains(a.ProjectId))
            .Include(a => a.Employee)
            .Include(a => a.Project)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<ProjectActivity>> GetEmployeeActivitiesAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.ProjectActivities
            .Where(a => a.EmployeeId == employeeId);

        if (startDate.HasValue)
            query = query.Where(a => a.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.CreatedAt <= endDate.Value);

        return await query
            .Include(a => a.Project)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }
}

public class ProjectRiskRepository : Repository<ProjectRisk>, IProjectRiskRepository
{
    public ProjectRiskRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<List<ProjectRisk>> GetProjectRisksAsync(int projectId)
    {
        return await _context.ProjectRisks
            .Where(r => r.ProjectId == projectId)
            .Include(r => r.AssignedToEmployee)
            .OrderByDescending(r => r.IdentifiedAt)
            .ToListAsync();
    }

    public async Task<List<ProjectRisk>> GetHighRisksAsync(List<int> projectIds)
    {
        return await _context.ProjectRisks
            .Where(r => projectIds.Contains(r.ProjectId) && 
                       (r.Severity == "High" || r.Severity == "Critical") &&
                       r.Status != "Resolved")
            .Include(r => r.Project)
            .Include(r => r.AssignedToEmployee)
            .OrderByDescending(r => r.Impact * r.Probability)
            .ToListAsync();
    }

    public async Task<List<ProjectRisk>> GetActiveRisksAsync(int projectId)
    {
        return await _context.ProjectRisks
            .Where(r => r.ProjectId == projectId && r.Status != "Resolved")
            .Include(r => r.AssignedToEmployee)
            .OrderByDescending(r => r.Impact * r.Probability)
            .ToListAsync();
    }
}