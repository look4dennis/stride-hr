using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Project;

namespace StrideHR.Infrastructure.Services;

public class ProjectCollaborationService : IProjectCollaborationService
{
    private readonly IProjectCommentRepository _commentRepository;
    private readonly IProjectCommentReplyRepository _replyRepository;
    private readonly IProjectActivityRepository _activityRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAssignmentRepository _assignmentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProjectCollaborationService> _logger;
    private readonly IHubContext<Hub> _hubContext;

    public ProjectCollaborationService(
        IProjectCommentRepository commentRepository,
        IProjectCommentReplyRepository replyRepository,
        IProjectActivityRepository activityRepository,
        IProjectRepository projectRepository,
        IProjectAssignmentRepository assignmentRepository,
        IMapper mapper,
        ILogger<ProjectCollaborationService> logger,
        IHubContext<Hub> hubContext)
    {
        _commentRepository = commentRepository;
        _replyRepository = replyRepository;
        _activityRepository = activityRepository;
        _projectRepository = projectRepository;
        _assignmentRepository = assignmentRepository;
        _mapper = mapper;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task<ProjectCollaborationDto> GetProjectCollaborationAsync(int projectId)
    {
        try
        {
            var project = await _projectRepository.GetProjectWithDetailsAsync(projectId);
            if (project == null)
                throw new ArgumentException("Project not found");

            var comments = await GetProjectCommentsAsync(projectId);
            var activities = await GetProjectActivitiesAsync(projectId, 50);
            var teamMembers = await _assignmentRepository.GetProjectTeamMembersAsync(projectId);
            var communicationStats = await GetProjectCommunicationStatsAsync(projectId);

            return new ProjectCollaborationDto
            {
                ProjectId = projectId,
                ProjectName = project.Name,
                Comments = comments,
                Activities = activities,
                TeamMembers = _mapper.Map<List<ProjectTeamMemberDto>>(teamMembers),
                CommunicationStats = communicationStats
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project collaboration for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<List<ProjectCommentDto>> GetProjectCommentsAsync(int projectId)
    {
        try
        {
            var comments = await _commentRepository.GetProjectCommentsAsync(projectId);
            return _mapper.Map<List<ProjectCommentDto>>(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project comments for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<List<ProjectCommentDto>> GetTaskCommentsAsync(int taskId)
    {
        try
        {
            var comments = await _commentRepository.GetTaskCommentsAsync(taskId);
            return _mapper.Map<List<ProjectCommentDto>>(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task comments for task {TaskId}", taskId);
            throw;
        }
    }

    public async Task<ProjectCommentDto> AddProjectCommentAsync(CreateProjectCommentDto dto, int employeeId)
    {
        try
        {
            var comment = new ProjectComment
            {
                ProjectId = dto.ProjectId,
                TaskId = dto.TaskId,
                EmployeeId = employeeId,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepository.AddAsync(comment);
            await _commentRepository.SaveChangesAsync();

            // Log activity
            await LogProjectActivityAsync(dto.ProjectId, employeeId, "Comment", 
                dto.TaskId.HasValue ? "Added comment to task" : "Added comment to project");

            // Get the comment with employee details
            var savedComment = await _commentRepository.GetCommentWithRepliesAsync(comment.Id);
            var commentDto = _mapper.Map<ProjectCommentDto>(savedComment);

            // Notify team members
            await NotifyTeamMembersAsync(dto.ProjectId, 
                $"New comment added by {savedComment?.Employee.FirstName} {savedComment?.Employee.LastName}", 
                "comment_added");

            _logger.LogInformation("Comment added to project {ProjectId} by employee {EmployeeId}", dto.ProjectId, employeeId);

            return commentDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding project comment for project {ProjectId}", dto.ProjectId);
            throw;
        }
    }

    public async Task<ProjectCommentReplyDto> AddCommentReplyAsync(CreateCommentReplyDto dto, int employeeId)
    {
        try
        {
            var reply = new ProjectCommentReply
            {
                CommentId = dto.CommentId,
                EmployeeId = employeeId,
                Reply = dto.Reply,
                CreatedAt = DateTime.UtcNow
            };

            await _replyRepository.AddAsync(reply);
            await _replyRepository.SaveChangesAsync();

            // Get the original comment to find project ID
            var originalComment = await _commentRepository.GetByIdAsync(dto.CommentId);
            if (originalComment != null)
            {
                await LogProjectActivityAsync(originalComment.ProjectId, employeeId, "Reply", "Replied to comment");
            }

            // Get the reply with employee details
            var savedReply = await _replyRepository.GetByIdAsync(reply.Id);
            var replyDto = _mapper.Map<ProjectCommentReplyDto>(savedReply);

            _logger.LogInformation("Reply added to comment {CommentId} by employee {EmployeeId}", dto.CommentId, employeeId);

            return replyDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment reply for comment {CommentId}", dto.CommentId);
            throw;
        }
    }

    public async Task<bool> DeleteCommentAsync(int commentId, int employeeId)
    {
        try
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null || comment.EmployeeId != employeeId)
                return false;

            await _commentRepository.DeleteAsync(comment);
            await _commentRepository.SaveChangesAsync();

            await LogProjectActivityAsync(comment.ProjectId, employeeId, "Delete", "Deleted comment");

            _logger.LogInformation("Comment {CommentId} deleted by employee {EmployeeId}", commentId, employeeId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
            throw;
        }
    }

    public async Task<bool> DeleteCommentReplyAsync(int replyId, int employeeId)
    {
        try
        {
            var reply = await _replyRepository.GetByIdAsync(replyId);
            if (reply == null || reply.EmployeeId != employeeId)
                return false;

            await _replyRepository.DeleteAsync(reply);
            await _replyRepository.SaveChangesAsync();

            _logger.LogInformation("Comment reply {ReplyId} deleted by employee {EmployeeId}", replyId, employeeId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment reply {ReplyId}", replyId);
            throw;
        }
    }

    public async Task<List<ProjectActivityDto>> GetProjectActivitiesAsync(int projectId, int limit = 50)
    {
        try
        {
            var activities = await _activityRepository.GetProjectActivitiesAsync(projectId, limit);
            return _mapper.Map<List<ProjectActivityDto>>(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project activities for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<ProjectActivityDto> LogProjectActivityAsync(int projectId, int employeeId, string activityType, string description, string details = "")
    {
        try
        {
            var activity = new ProjectActivity
            {
                ProjectId = projectId,
                EmployeeId = employeeId,
                ActivityType = activityType,
                Description = description,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await _activityRepository.AddAsync(activity);
            await _activityRepository.SaveChangesAsync();

            var savedActivity = await _activityRepository.GetByIdAsync(activity.Id);
            var activityDto = _mapper.Map<ProjectActivityDto>(savedActivity);

            // Broadcast activity to team members
            await BroadcastProjectUpdateAsync(projectId, "activity_logged", activityDto);

            return activityDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging project activity for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<List<ProjectActivityDto>> GetTeamActivitiesAsync(int teamLeaderId, int limit = 100)
    {
        try
        {
            var projects = await _projectRepository.GetProjectsByTeamLeadAsync(teamLeaderId);
            var projectIds = projects.Select(p => p.Id).ToList();
            var activities = await _activityRepository.GetTeamActivitiesAsync(projectIds, limit);
            return _mapper.Map<List<ProjectActivityDto>>(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team activities for team lead {TeamLeaderId}", teamLeaderId);
            throw;
        }
    }

    public async Task<ProjectCommunicationStatsDto> GetProjectCommunicationStatsAsync(int projectId)
    {
        try
        {
            var comments = await _commentRepository.GetProjectCommentsAsync(projectId);
            var activities = await _activityRepository.GetProjectActivitiesAsync(projectId, 1000);
            var teamMembers = await _assignmentRepository.GetProjectTeamMembersAsync(projectId);

            var memberActivities = teamMembers.Select(tm => new TeamMemberActivityDto
            {
                EmployeeId = tm.Id, // Use Employee.Id instead of EmployeeId
                EmployeeName = tm.FullName ?? "Unknown",
                CommentsCount = comments.Count(c => c.EmployeeId == tm.Id),
                ActivitiesCount = activities.Count(a => a.EmployeeId == tm.Id),
                LastActivity = activities.Where(a => a.EmployeeId == tm.Id)
                                      .OrderByDescending(a => a.CreatedAt)
                                      .FirstOrDefault()?.CreatedAt ?? DateTime.MinValue
            }).ToList();

            return new ProjectCommunicationStatsDto
            {
                TotalComments = comments.Count,
                TotalActivities = activities.Count,
                ActiveTeamMembers = memberActivities.Count(ma => ma.LastActivity > DateTime.Today.AddDays(-7)),
                LastActivity = activities.OrderByDescending(a => a.CreatedAt).FirstOrDefault()?.CreatedAt ?? DateTime.MinValue,
                MemberActivities = memberActivities
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project communication stats for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task<List<TeamMemberActivityDto>> GetTeamMemberActivitiesAsync(int projectId)
    {
        try
        {
            var stats = await GetProjectCommunicationStatsAsync(projectId);
            return stats.MemberActivities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team member activities for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task NotifyTeamMembersAsync(int projectId, string message, string notificationType)
    {
        try
        {
            var teamMembers = await _assignmentRepository.GetProjectTeamMembersAsync(projectId);
            var groupName = $"Project_{projectId}";

            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", new
            {
                ProjectId = projectId,
                Message = message,
                Type = notificationType,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Notification sent to {MemberCount} team members for project {ProjectId}", 
                teamMembers.Count(), projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying team members for project {ProjectId}", projectId);
            throw;
        }
    }

    public async Task NotifyTaskAssigneesAsync(int taskId, string message, string notificationType)
    {
        try
        {
            // This would require task assignment repository to get assignees
            // For now, we'll use a simplified approach
            var groupName = $"Task_{taskId}";

            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", new
            {
                TaskId = taskId,
                Message = message,
                Type = notificationType,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Notification sent to task assignees for task {TaskId}", taskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying task assignees for task {TaskId}", taskId);
            throw;
        }
    }

    public async Task BroadcastProjectUpdateAsync(int projectId, string updateType, object updateData)
    {
        try
        {
            var groupName = $"Project_{projectId}";

            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveProjectUpdate", new
            {
                ProjectId = projectId,
                UpdateType = updateType,
                Data = updateData,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Project update broadcasted for project {ProjectId}: {UpdateType}", 
                projectId, updateType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting project update for project {ProjectId}", projectId);
            throw;
        }
    }
}

// SignalR Hub for real-time project collaboration
public class ProjectHub : Hub
{
    public async Task JoinProjectGroup(int projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Project_{projectId}");
    }

    public async Task LeaveProjectGroup(int projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Project_{projectId}");
    }

    public async Task JoinTaskGroup(int taskId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Task_{taskId}");
    }

    public async Task LeaveTaskGroup(int taskId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Task_{taskId}");
    }
}