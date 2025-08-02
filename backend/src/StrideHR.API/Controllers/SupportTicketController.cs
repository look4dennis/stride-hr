using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Attributes;
using StrideHR.API.Models;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.SupportTicket;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public partial class SupportTicketController : BaseController
{
    private readonly ISupportTicketService _supportTicketService;

    public SupportTicketController(ISupportTicketService supportTicketService)
    {
        _supportTicketService = supportTicketService;
    }

    /// <summary>
    /// Create a new support ticket
    /// </summary>
    [HttpPost]
    [RequirePermission("SupportTicket.Create")]
    public async Task<IActionResult> CreateTicket([FromBody] CreateSupportTicketDto dto)
    {
        var ticket = await _supportTicketService.CreateTicketAsync(dto, GetCurrentEmployeeId());
        return Success(ticket, "Support ticket created successfully");
    }

    /// <summary>
    /// Get support ticket by ID
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission("SupportTicket.Read")]
    public async Task<IActionResult> GetTicket(int id)
    {
        var ticket = await _supportTicketService.GetTicketByIdAsync(id);
        return Success(ticket);
    }

    /// <summary>
    /// Get support ticket by ticket number
    /// </summary>
    [HttpGet("by-number/{ticketNumber}")]
    [RequirePermission("SupportTicket.Read")]
    public async Task<IActionResult> GetTicketByNumber(string ticketNumber)
    {
        var ticket = await _supportTicketService.GetTicketByNumberAsync(ticketNumber);
        if (ticket == null)
            return NotFound(Error("Support ticket not found"));

        return Success(ticket);
    }

    /// <summary>
    /// Search support tickets with filters and pagination
    /// </summary>
    [HttpPost("search")]
    [RequirePermission("SupportTicket.Read")]
    public async Task<IActionResult> SearchTickets([FromBody] SupportTicketSearchCriteria criteria)
    {
        var (tickets, totalCount) = await _supportTicketService.SearchTicketsAsync(criteria);
        
        var response = new
        {
            Tickets = tickets,
            TotalCount = totalCount,
            PageNumber = criteria.PageNumber,
            PageSize = criteria.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / criteria.PageSize)
        };

        return Success(response);
    }

    /// <summary>
    /// Update support ticket
    /// </summary>
    [HttpPut("{id}")]
    [RequirePermission("SupportTicket.Update")]
    public async Task<IActionResult> UpdateTicket(int id, [FromBody] UpdateSupportTicketDto dto)
    {
        var ticket = await _supportTicketService.UpdateTicketAsync(id, dto, GetCurrentEmployeeId());
        return Success(ticket, "Support ticket updated successfully");
    }

    /// <summary>
    /// Assign support ticket to an agent
    /// </summary>
    [HttpPost("{id}/assign")]
    [RequirePermission("SupportTicket.Assign")]
    public async Task<IActionResult> AssignTicket(int id, [FromBody] AssignTicketRequest request)
    {
        var ticket = await _supportTicketService.AssignTicketAsync(id, request.AssignedToId, GetCurrentEmployeeId());
        return Success(ticket, "Support ticket assigned successfully");
    }

    /// <summary>
    /// Update support ticket status
    /// </summary>
    [HttpPost("{id}/status")]
    [RequirePermission("SupportTicket.UpdateStatus")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var ticket = await _supportTicketService.UpdateStatusAsync(id, request.Status, GetCurrentEmployeeId(), request.Reason);
        return Success(ticket, "Support ticket status updated successfully");
    }

    /// <summary>
    /// Resolve support ticket
    /// </summary>
    [HttpPost("{id}/resolve")]
    [RequirePermission("SupportTicket.Resolve")]
    public async Task<IActionResult> ResolveTicket(int id, [FromBody] ResolveTicketRequest request)
    {
        var ticket = await _supportTicketService.ResolveTicketAsync(id, request.Resolution, GetCurrentEmployeeId());
        return Success(ticket, "Support ticket resolved successfully");
    }

    /// <summary>
    /// Close support ticket
    /// </summary>
    [HttpPost("{id}/close")]
    [RequirePermission("SupportTicket.Close")]
    public async Task<IActionResult> CloseTicket(int id, [FromBody] CloseTicketRequest request)
    {
        var ticket = await _supportTicketService.CloseTicketAsync(id, GetCurrentEmployeeId(), request.SatisfactionRating, request.FeedbackComments);
        return Success(ticket, "Support ticket closed successfully");
    }

    /// <summary>
    /// Reopen support ticket
    /// </summary>
    [HttpPost("{id}/reopen")]
    [RequirePermission("SupportTicket.Reopen")]
    public async Task<IActionResult> ReopenTicket(int id, [FromBody] ReopenTicketRequest request)
    {
        var ticket = await _supportTicketService.ReopenTicketAsync(id, request.Reason, GetCurrentEmployeeId());
        return Success(ticket, "Support ticket reopened successfully");
    }

    /// <summary>
    /// Get my support tickets (tickets I created)
    /// </summary>
    [HttpGet("my-tickets")]
    public async Task<IActionResult> GetMyTickets()
    {
        var tickets = await _supportTicketService.GetMyTicketsAsync(GetCurrentEmployeeId());
        return Success(tickets);
    }

    /// <summary>
    /// Get assigned support tickets (tickets assigned to me)
    /// </summary>
    [HttpGet("assigned-tickets")]
    [RequirePermission("SupportTicket.ViewAssigned")]
    public async Task<IActionResult> GetAssignedTickets()
    {
        var tickets = await _supportTicketService.GetAssignedTicketsAsync(GetCurrentEmployeeId());
        return Success(tickets);
    }

    /// <summary>
    /// Get overdue support tickets
    /// </summary>
    [HttpGet("overdue")]
    [RequirePermission("SupportTicket.ViewOverdue")]
    public async Task<IActionResult> GetOverdueTickets()
    {
        var tickets = await _supportTicketService.GetOverdueTicketsAsync();
        return Success(tickets);
    }

    /// <summary>
    /// Add comment to support ticket
    /// </summary>
    [HttpPost("{id}/comments")]
    [RequirePermission("SupportTicket.Comment")]
    public async Task<IActionResult> AddComment(int id, [FromBody] CreateSupportTicketCommentDto dto)
    {
        var comment = await _supportTicketService.AddCommentAsync(id, dto, GetCurrentEmployeeId());
        return Success(comment, "Comment added successfully");
    }

    /// <summary>
    /// Get support ticket comments
    /// </summary>
    [HttpGet("{id}/comments")]
    [RequirePermission("SupportTicket.Read")]
    public async Task<IActionResult> GetComments(int id, [FromQuery] bool includeInternal = false)
    {
        var comments = await _supportTicketService.GetTicketCommentsAsync(id, includeInternal);
        return Success(comments);
    }

    /// <summary>
    /// Get support ticket analytics
    /// </summary>
    [HttpGet("analytics")]
    [RequirePermission("SupportTicket.ViewAnalytics")]
    public async Task<IActionResult> GetAnalytics([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var analytics = await _supportTicketService.GetAnalyticsAsync(fromDate, toDate);
        return Success(analytics);
    }

    /// <summary>
    /// Delete support ticket
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission("SupportTicket.Delete")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        await _supportTicketService.DeleteTicketAsync(id);
        return Success("Support ticket deleted successfully");
    }
}

// Request DTOs
public class AssignTicketRequest
{
    public int AssignedToId { get; set; }
}

public class UpdateStatusRequest
{
    public SupportTicketStatus Status { get; set; }
    public string? Reason { get; set; }
}

public class ResolveTicketRequest
{
    public string Resolution { get; set; } = string.Empty;
}

public class CloseTicketRequest
{
    public int? SatisfactionRating { get; set; }
    public string? FeedbackComments { get; set; }
}

public class ReopenTicketRequest
{
    public string Reason { get; set; } = string.Empty;
}

// Helper method
public partial class SupportTicketController
{
    private int GetCurrentEmployeeId()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        return int.TryParse(employeeIdClaim, out var employeeId) ? employeeId : 0;
    }
}