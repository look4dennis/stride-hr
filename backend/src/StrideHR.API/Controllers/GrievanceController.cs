using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Attributes;
using StrideHR.API.Models;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Grievance;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GrievanceController : BaseController
{
    private readonly IGrievanceService _grievanceService;

    public GrievanceController(IGrievanceService grievanceService)
    {
        _grievanceService = grievanceService;
    }

    /// <summary>
    /// Create a new grievance
    /// </summary>
    [HttpPost]
    [RequirePermission("Grievance.Create")]
    public async Task<IActionResult> CreateGrievance([FromBody] CreateGrievanceDto dto)
    {
        var grievance = await _grievanceService.CreateGrievanceAsync(dto, GetCurrentEmployeeId());
        return Success(grievance, "Grievance submitted successfully");
    }

    /// <summary>
    /// Get grievance by ID
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("Grievance.Read")]
    public async Task<IActionResult> GetGrievance(int id)
    {
        var grievance = await _grievanceService.GetGrievanceByIdAsync(id);
        return Success(grievance);
    }

    /// <summary>
    /// Get grievance by grievance number
    /// </summary>
    [HttpGet("by-number/{grievanceNumber}")]
    [RequirePermission("Grievance.Read")]
    public async Task<IActionResult> GetGrievanceByNumber(string grievanceNumber)
    {
        var grievance = await _grievanceService.GetGrievanceByNumberAsync(grievanceNumber);
        if (grievance == null)
            return NotFound($"Grievance with number {grievanceNumber} not found");
        
        return Success(grievance);
    }

    /// <summary>
    /// Search grievances with filters and pagination
    /// </summary>
    [HttpPost("search")]
    [RequirePermission("Grievance.Read")]
    public async Task<IActionResult> SearchGrievances([FromBody] GrievanceSearchCriteria criteria)
    {
        var (grievances, totalCount) = await _grievanceService.SearchGrievancesAsync(criteria);
        
        var response = new
        {
            Grievances = grievances,
            TotalCount = totalCount,
            PageNumber = criteria.PageNumber,
            PageSize = criteria.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / criteria.PageSize)
        };
        
        return Success(response);
    }

    /// <summary>
    /// Update grievance details
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission("Grievance.Update")]
    public async Task<IActionResult> UpdateGrievance(int id, [FromBody] UpdateGrievanceDto dto)
    {
        var grievance = await _grievanceService.UpdateGrievanceAsync(id, dto, GetCurrentEmployeeId());
        return Success(grievance, "Grievance updated successfully");
    }

    /// <summary>
    /// Assign grievance to an employee
    /// </summary>
    [HttpPost("{id}/assign")]
    [RequirePermission("Grievance.Assign")]
    public async Task<IActionResult> AssignGrievance(int id, [FromBody] AssignGrievanceRequest request)
    {
        var grievance = await _grievanceService.AssignGrievanceAsync(id, request.AssignedToId, GetCurrentEmployeeId());
        return Success(grievance, "Grievance assigned successfully");
    }

    /// <summary>
    /// Update grievance status
    /// </summary>
    [HttpPost("{id}/status")]
    [RequirePermission("Grievance.UpdateStatus")]
    public async Task<IActionResult> UpdateGrievanceStatus(int id, [FromBody] UpdateGrievanceStatusRequest request)
    {
        var grievance = await _grievanceService.UpdateStatusAsync(id, request.Status, GetCurrentEmployeeId(), request.Reason);
        return Success(grievance, "Grievance status updated successfully");
    }

    /// <summary>
    /// Resolve a grievance
    /// </summary>
    [HttpPost("{id}/resolve")]
    [RequirePermission("Grievance.Resolve")]
    public async Task<IActionResult> ResolveGrievance(int id, [FromBody] ResolveGrievanceRequest request)
    {
        var grievance = await _grievanceService.ResolveGrievanceAsync(id, request.Resolution, request.ResolutionNotes, GetCurrentEmployeeId());
        return Success(grievance, "Grievance resolved successfully");
    }

    /// <summary>
    /// Close a grievance
    /// </summary>
    [HttpPost("{id}/close")]
    [RequirePermission("Grievance.Close")]
    public async Task<IActionResult> CloseGrievance(int id, [FromBody] CloseGrievanceRequest request)
    {
        var grievance = await _grievanceService.CloseGrievanceAsync(id, GetCurrentEmployeeId(), request.SatisfactionRating, request.FeedbackComments);
        return Success(grievance, "Grievance closed successfully");
    }

    /// <summary>
    /// Escalate a grievance
    /// </summary>
    [HttpPost("{id}/escalate")]
    [RequirePermission("Grievance.Escalate")]
    public async Task<IActionResult> EscalateGrievance(int id, [FromBody] EscalateGrievanceRequest request)
    {
        var grievance = await _grievanceService.EscalateGrievanceAsync(id, request.ToLevel, request.Reason, GetCurrentEmployeeId(), request.EscalatedToId);
        return Success(grievance, "Grievance escalated successfully");
    }

    /// <summary>
    /// Withdraw a grievance
    /// </summary>
    [HttpPost("{id}/withdraw")]
    [RequirePermission("Grievance.Withdraw")]
    public async Task<IActionResult> WithdrawGrievance(int id, [FromBody] WithdrawGrievanceRequest request)
    {
        var grievance = await _grievanceService.WithdrawGrievanceAsync(id, request.Reason, GetCurrentEmployeeId());
        return Success(grievance, "Grievance withdrawn successfully");
    }

    /// <summary>
    /// Get my submitted grievances
    /// </summary>
    [HttpGet("my-grievances")]
    [RequirePermission("Grievance.ReadOwn")]
    public async Task<IActionResult> GetMyGrievances()
    {
        var grievances = await _grievanceService.GetMyGrievancesAsync(GetCurrentEmployeeId());
        return Success(grievances);
    }

    /// <summary>
    /// Get grievances assigned to me
    /// </summary>
    [HttpGet("assigned-to-me")]
    [RequirePermission("Grievance.ReadAssigned")]
    public async Task<IActionResult> GetAssignedGrievances()
    {
        var grievances = await _grievanceService.GetAssignedGrievancesAsync(GetCurrentEmployeeId());
        return Success(grievances);
    }

    /// <summary>
    /// Get overdue grievances
    /// </summary>
    [HttpGet("overdue")]
    [RequirePermission("Grievance.ReadOverdue")]
    public async Task<IActionResult> GetOverdueGrievances()
    {
        var grievances = await _grievanceService.GetOverdueGrievancesAsync();
        return Success(grievances);
    }

    /// <summary>
    /// Get escalated grievances
    /// </summary>
    [HttpGet("escalated")]
    [RequirePermission("Grievance.ReadEscalated")]
    public async Task<IActionResult> GetEscalatedGrievances()
    {
        var grievances = await _grievanceService.GetEscalatedGrievancesAsync();
        return Success(grievances);
    }

    /// <summary>
    /// Get anonymous grievances
    /// </summary>
    [HttpGet("anonymous")]
    [RequirePermission("Grievance.ReadAnonymous")]
    public async Task<IActionResult> GetAnonymousGrievances()
    {
        var grievances = await _grievanceService.GetAnonymousGrievancesAsync();
        return Success(grievances);
    }

    /// <summary>
    /// Add comment to grievance
    /// </summary>
    [HttpPost("{id}/comments")]
    [RequirePermission("Grievance.AddComment")]
    public async Task<IActionResult> AddComment(int id, [FromBody] CreateGrievanceCommentDto dto)
    {
        var comment = await _grievanceService.AddCommentAsync(id, dto, GetCurrentEmployeeId());
        return Success(comment, "Comment added successfully");
    }

    /// <summary>
    /// Get grievance comments
    /// </summary>
    [HttpGet("{id}/comments")]
    [RequirePermission("Grievance.ReadComments")]
    public async Task<IActionResult> GetGrievanceComments(int id, [FromQuery] bool includeInternal = false)
    {
        var comments = await _grievanceService.GetGrievanceCommentsAsync(id, includeInternal);
        return Success(comments);
    }

    /// <summary>
    /// Schedule follow-up for grievance
    /// </summary>
    [HttpPost("{id}/follow-ups")]
    [RequirePermission("Grievance.ScheduleFollowUp")]
    public async Task<IActionResult> ScheduleFollowUp(int id, [FromBody] CreateGrievanceFollowUpDto dto)
    {
        var followUp = await _grievanceService.ScheduleFollowUpAsync(id, dto, GetCurrentEmployeeId());
        return Success(followUp, "Follow-up scheduled successfully");
    }

    /// <summary>
    /// Complete follow-up
    /// </summary>
    [HttpPost("follow-ups/{followUpId}/complete")]
    [RequirePermission("Grievance.CompleteFollowUp")]
    public async Task<IActionResult> CompleteFollowUp(int followUpId, [FromBody] CompleteGrievanceFollowUpDto dto)
    {
        var followUp = await _grievanceService.CompleteFollowUpAsync(followUpId, dto, GetCurrentEmployeeId());
        return Success(followUp, "Follow-up completed successfully");
    }

    /// <summary>
    /// Get grievance follow-ups
    /// </summary>
    [HttpGet("{id}/follow-ups")]
    [RequirePermission("Grievance.ReadFollowUps")]
    public async Task<IActionResult> GetGrievanceFollowUps(int id)
    {
        var followUps = await _grievanceService.GetGrievanceFollowUpsAsync(id);
        return Success(followUps);
    }

    /// <summary>
    /// Get pending follow-ups
    /// </summary>
    [HttpGet("follow-ups/pending")]
    [RequirePermission("Grievance.ReadPendingFollowUps")]
    public async Task<IActionResult> GetPendingFollowUps()
    {
        var followUps = await _grievanceService.GetPendingFollowUpsAsync();
        return Success(followUps);
    }

    /// <summary>
    /// Get grievance analytics
    /// </summary>
    [HttpGet("analytics")]
    [RequirePermission("Grievance.ReadAnalytics")]
    public async Task<IActionResult> GetGrievanceAnalytics([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var analytics = await _grievanceService.GetAnalyticsAsync(fromDate, toDate);
        return Success(analytics);
    }

    /// <summary>
    /// Delete grievance
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("Grievance.Delete")]
    public async Task<IActionResult> DeleteGrievance(int id)
    {
        await _grievanceService.DeleteGrievanceAsync(id);
        return Success("Grievance deleted successfully");
    }
}

// Request models for the controller
public class AssignGrievanceRequest
{
    public int AssignedToId { get; set; }
}

public class UpdateGrievanceStatusRequest
{
    public GrievanceStatus Status { get; set; }
    public string? Reason { get; set; }
}

public class ResolveGrievanceRequest
{
    public string Resolution { get; set; } = string.Empty;
    public string? ResolutionNotes { get; set; }
}

public class CloseGrievanceRequest
{
    public int? SatisfactionRating { get; set; }
    public string? FeedbackComments { get; set; }
}

public class EscalateGrievanceRequest
{
    public EscalationLevel ToLevel { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int? EscalatedToId { get; set; }
}

public class WithdrawGrievanceRequest
{
    public string Reason { get; set; } = string.Empty;
}