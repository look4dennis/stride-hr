using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IProjectCommentRepository : IRepository<ProjectComment>
{
    Task<List<ProjectComment>> GetProjectCommentsAsync(int projectId);
    Task<List<ProjectComment>> GetTaskCommentsAsync(int taskId);
    Task<ProjectComment?> GetCommentWithRepliesAsync(int commentId);
    Task<int> GetProjectCommentsCountAsync(int projectId);
}

public interface IProjectCommentReplyRepository : IRepository<ProjectCommentReply>
{
    Task<List<ProjectCommentReply>> GetCommentRepliesAsync(int commentId);
}

public interface IProjectActivityRepository : IRepository<ProjectActivity>
{
    Task<List<ProjectActivity>> GetProjectActivitiesAsync(int projectId, int limit = 50);
    Task<List<ProjectActivity>> GetTeamActivitiesAsync(List<int> projectIds, int limit = 100);
    Task<List<ProjectActivity>> GetEmployeeActivitiesAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null);
}

public interface IProjectRiskRepository : IRepository<ProjectRisk>
{
    Task<List<ProjectRisk>> GetProjectRisksAsync(int projectId);
    Task<List<ProjectRisk>> GetHighRisksAsync(List<int> projectIds);
    Task<List<ProjectRisk>> GetActiveRisksAsync(int projectId);
}