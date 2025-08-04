using StrideHR.Core.Models.Project;

namespace StrideHR.Core.Interfaces.Services;

public interface IProjectCollaborationService
{
    // Team Collaboration Features
    Task<ProjectCollaborationDto> GetProjectCollaborationAsync(int projectId);
    Task<List<ProjectCommentDto>> GetProjectCommentsAsync(int projectId);
    Task<List<ProjectCommentDto>> GetTaskCommentsAsync(int taskId);
    Task<ProjectCommentDto> AddProjectCommentAsync(CreateProjectCommentDto dto, int employeeId);
    Task<ProjectCommentReplyDto> AddCommentReplyAsync(CreateCommentReplyDto dto, int employeeId);
    Task<bool> DeleteCommentAsync(int commentId, int employeeId);
    Task<bool> DeleteCommentReplyAsync(int replyId, int employeeId);
    
    // Project Activities
    Task<List<ProjectActivityDto>> GetProjectActivitiesAsync(int projectId, int limit = 50);
    Task<ProjectActivityDto> LogProjectActivityAsync(int projectId, int employeeId, string activityType, string description, string details = "");
    Task<List<ProjectActivityDto>> GetTeamActivitiesAsync(int teamLeaderId, int limit = 100);
    
    // Communication Stats
    Task<ProjectCommunicationStatsDto> GetProjectCommunicationStatsAsync(int projectId);
    Task<List<TeamMemberActivityDto>> GetTeamMemberActivitiesAsync(int projectId);
    
    // Real-time Collaboration
    Task NotifyTeamMembersAsync(int projectId, string message, string notificationType);
    Task NotifyTaskAssigneesAsync(int taskId, string message, string notificationType);
    Task BroadcastProjectUpdateAsync(int projectId, string updateType, object updateData);
}