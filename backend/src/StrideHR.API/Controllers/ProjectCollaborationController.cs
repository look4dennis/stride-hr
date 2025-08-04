using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Project;

namespace StrideHR.API.Controllers;

[Authorize]
public class ProjectCollaborationController : BaseController
{
    private readonly IProjectCollaborationService _collaborationService;
    private readonly ILogger<ProjectCollaborationController> _logger;

    public ProjectCollaborationController(
        IProjectCollaborationService collaborationService,
        ILogger<ProjectCollaborationController> logger)
    {
        _collaborationService = collaborationService;
        _logger = logger;
    }

    private int GetCurrentEmployeeId()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        return int.TryParse(employeeIdClaim, out var employeeId) ? employeeId : 0;
    }

    // Team Collaboration Features
    [HttpGet("projects/{projectId}/collaboration")]
    public async Task<IActionResult> GetProjectCollaboration(int projectId)
    {
        try
        {
            var collaboration = await _collaborationService.GetProjectCollaborationAsync(projectId);
            return Success(collaboration);
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project collaboration for project {ProjectId}", projectId);
            return Error("Failed to get project collaboration");
        }
    }

    [HttpGet("projects/{projectId}/comments")]
    public async Task<IActionResult> GetProjectComments(int projectId)
    {
        try
        {
            var comments = await _collaborationService.GetProjectCommentsAsync(projectId);
            return Success(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project comments for project {ProjectId}", projectId);
            return Error("Failed to get project comments");
        }
    }

    [HttpGet("tasks/{taskId}/comments")]
    public async Task<IActionResult> GetTaskComments(int taskId)
    {
        try
        {
            var comments = await _collaborationService.GetTaskCommentsAsync(taskId);
            return Success(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task comments for task {TaskId}", taskId);
            return Error("Failed to get task comments");
        }
    }

    [HttpPost("comments")]
    public async Task<IActionResult> AddProjectComment([FromBody] CreateProjectCommentDto dto)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var comment = await _collaborationService.AddProjectCommentAsync(dto, currentEmployeeId);
            return Success(comment, "Comment added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding project comment");
            return Error("Failed to add comment");
        }
    }

    [HttpPost("comments/replies")]
    public async Task<IActionResult> AddCommentReply([FromBody] CreateCommentReplyDto dto)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var reply = await _collaborationService.AddCommentReplyAsync(dto, currentEmployeeId);
            return Success(reply, "Reply added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment reply");
            return Error("Failed to add reply");
        }
    }

    [HttpDelete("comments/{commentId}")]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var result = await _collaborationService.DeleteCommentAsync(commentId, currentEmployeeId);
            if (!result)
                return Error("Comment not found or you don't have permission to delete it");

            return Success("Comment deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
            return Error("Failed to delete comment");
        }
    }

    [HttpDelete("comments/replies/{replyId}")]
    public async Task<IActionResult> DeleteCommentReply(int replyId)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var result = await _collaborationService.DeleteCommentReplyAsync(replyId, currentEmployeeId);
            if (!result)
                return Error("Reply not found or you don't have permission to delete it");

            return Success("Reply deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment reply {ReplyId}", replyId);
            return Error("Failed to delete reply");
        }
    }

    // Project Activities
    [HttpGet("projects/{projectId}/activities")]
    public async Task<IActionResult> GetProjectActivities(int projectId, [FromQuery] int limit = 50)
    {
        try
        {
            var activities = await _collaborationService.GetProjectActivitiesAsync(projectId, limit);
            return Success(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project activities for project {ProjectId}", projectId);
            return Error("Failed to get project activities");
        }
    }

    [HttpPost("projects/{projectId}/activities")]
    public async Task<IActionResult> LogProjectActivity(int projectId, [FromBody] LogProjectActivityRequest request)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var activity = await _collaborationService.LogProjectActivityAsync(projectId, currentEmployeeId, request.ActivityType, request.Description, request.Details);
            return Success(activity, "Activity logged successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging project activity for project {ProjectId}", projectId);
            return Error("Failed to log activity");
        }
    }

    [HttpGet("team-activities")]
    public async Task<IActionResult> GetTeamActivities([FromQuery] int limit = 100)
    {
        try
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId == 0)
                return Error("Unable to identify current employee");

            var activities = await _collaborationService.GetTeamActivitiesAsync(currentEmployeeId, limit);
            return Success(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team activities");
            return Error("Failed to get team activities");
        }
    }

    // Communication Stats
    [HttpGet("projects/{projectId}/communication-stats")]
    public async Task<IActionResult> GetProjectCommunicationStats(int projectId)
    {
        try
        {
            var stats = await _collaborationService.GetProjectCommunicationStatsAsync(projectId);
            return Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project communication stats for project {ProjectId}", projectId);
            return Error("Failed to get communication stats");
        }
    }

    [HttpGet("projects/{projectId}/team-member-activities")]
    public async Task<IActionResult> GetTeamMemberActivities(int projectId)
    {
        try
        {
            var activities = await _collaborationService.GetTeamMemberActivitiesAsync(projectId);
            return Success(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team member activities for project {ProjectId}", projectId);
            return Error("Failed to get team member activities");
        }
    }

    // Real-time Collaboration
    [HttpPost("projects/{projectId}/notify-team")]
    public async Task<IActionResult> NotifyTeamMembers(int projectId, [FromBody] NotifyTeamRequest request)
    {
        try
        {
            await _collaborationService.NotifyTeamMembersAsync(projectId, request.Message, request.NotificationType);
            return Success("Team members notified successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying team members for project {ProjectId}", projectId);
            return Error("Failed to notify team members");
        }
    }

    [HttpPost("tasks/{taskId}/notify-assignees")]
    public async Task<IActionResult> NotifyTaskAssignees(int taskId, [FromBody] NotifyTeamRequest request)
    {
        try
        {
            await _collaborationService.NotifyTaskAssigneesAsync(taskId, request.Message, request.NotificationType);
            return Success("Task assignees notified successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying task assignees for task {TaskId}", taskId);
            return Error("Failed to notify task assignees");
        }
    }

    [HttpPost("projects/{projectId}/broadcast-update")]
    public async Task<IActionResult> BroadcastProjectUpdate(int projectId, [FromBody] BroadcastUpdateRequest request)
    {
        try
        {
            await _collaborationService.BroadcastProjectUpdateAsync(projectId, request.UpdateType, request.UpdateData);
            return Success("Project update broadcasted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting project update for project {ProjectId}", projectId);
            return Error("Failed to broadcast project update");
        }
    }
}

// Request DTOs
public class LogProjectActivityRequest
{
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}

public class NotifyTeamRequest
{
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
}

public class BroadcastUpdateRequest
{
    public string UpdateType { get; set; } = string.Empty;
    public object UpdateData { get; set; } = new();
}