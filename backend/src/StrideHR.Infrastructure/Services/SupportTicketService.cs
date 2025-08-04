using AutoMapper;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.SupportTicket;

namespace StrideHR.Infrastructure.Services;

public class SupportTicketService : ISupportTicketService
{
    private readonly ISupportTicketRepository _ticketRepository;
    private readonly ISupportTicketCommentRepository _commentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SupportTicketService(
        ISupportTicketRepository ticketRepository,
        ISupportTicketCommentRepository commentRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _ticketRepository = ticketRepository;
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SupportTicketDto> CreateTicketAsync(CreateSupportTicketDto dto, int requesterId)
    {
        var ticketNumber = await _ticketRepository.GenerateTicketNumberAsync();
        
        var ticket = new SupportTicket
        {
            TicketNumber = ticketNumber,
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Priority = dto.Priority,
            Status = SupportTicketStatus.Open,
            RequesterId = requesterId,
            RequiresRemoteAccess = dto.RequiresRemoteAccess,
            RemoteAccessDetails = dto.RemoteAccessDetails,
            AssetId = dto.AssetId,
            AttachmentPath = dto.AttachmentPath
        };

        await _ticketRepository.AddAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        // Try to load the ticket with details, but fallback to basic mapping if not found
        var createdTicket = await _ticketRepository.GetWithDetailsAsync(ticket.Id);
        if (createdTicket != null)
        {
            return MapToDto(createdTicket);
        }
        
        // Fallback: return DTO from the created ticket without navigation properties
        return MapToDtoBasic(ticket);
    }

    public async Task<SupportTicketDto> GetTicketByIdAsync(int id)
    {
        var ticket = await _ticketRepository.GetWithDetailsAsync(id);
        if (ticket == null)
        {
            // Fallback: try to get the basic ticket without navigation properties
            ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null)
                throw new ArgumentException($"Support ticket with ID {id} not found.");
            
            return MapToDtoBasic(ticket);
        }

        return MapToDto(ticket);
    }

    public async Task<SupportTicketDto?> GetTicketByNumberAsync(string ticketNumber)
    {
        var ticket = await _ticketRepository.GetByTicketNumberAsync(ticketNumber);
        return ticket != null ? MapToDto(ticket) : null;
    }

    public async Task<(List<SupportTicketDto> Tickets, int TotalCount)> SearchTicketsAsync(SupportTicketSearchCriteria criteria)
    {
        var (tickets, totalCount) = await _ticketRepository.SearchAsync(criteria);
        var ticketDtos = tickets.Select(MapToDto).ToList();
        return (ticketDtos, totalCount);
    }

    public async Task<SupportTicketDto> UpdateTicketAsync(int id, UpdateSupportTicketDto dto, int updatedById)
    {
        var ticket = await _ticketRepository.GetWithDetailsAsync(id);
        if (ticket == null)
            throw new ArgumentException($"Support ticket with ID {id} not found.");

        var originalStatus = ticket.Status;

        // Update fields if provided
        if (!string.IsNullOrEmpty(dto.Title))
            ticket.Title = dto.Title;
        
        if (!string.IsNullOrEmpty(dto.Description))
            ticket.Description = dto.Description;
        
        if (dto.Category.HasValue)
            ticket.Category = dto.Category.Value;
        
        if (dto.Priority.HasValue)
            ticket.Priority = dto.Priority.Value;
        
        if (dto.Status.HasValue && dto.Status.Value != originalStatus)
        {
            await UpdateStatusInternalAsync(ticket, dto.Status.Value, updatedById, "Status updated");
        }
        
        if (dto.AssignedToId.HasValue)
        {
            ticket.AssignedToId = dto.AssignedToId.Value;
            ticket.AssignedAt = DateTime.UtcNow;
        }
        
        if (!string.IsNullOrEmpty(dto.Resolution))
            ticket.Resolution = dto.Resolution;
        
        if (dto.RequiresRemoteAccess.HasValue)
            ticket.RequiresRemoteAccess = dto.RequiresRemoteAccess.Value;
        
        if (!string.IsNullOrEmpty(dto.RemoteAccessDetails))
            ticket.RemoteAccessDetails = dto.RemoteAccessDetails;
        
        if (dto.AssetId.HasValue)
            ticket.AssetId = dto.AssetId.Value;

        await _ticketRepository.UpdateAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(ticket);
    }

    public async Task<SupportTicketDto> AssignTicketAsync(int id, int assignedToId, int assignedById)
    {
        var ticket = await _ticketRepository.GetWithDetailsAsync(id);
        if (ticket == null)
            throw new ArgumentException($"Support ticket with ID {id} not found.");

        ticket.AssignedToId = assignedToId;
        ticket.AssignedAt = DateTime.UtcNow;

        if (ticket.Status == SupportTicketStatus.Open)
        {
            await UpdateStatusInternalAsync(ticket, SupportTicketStatus.InProgress, assignedById, "Ticket assigned");
        }

        await _ticketRepository.UpdateAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(ticket);
    }

    public async Task<SupportTicketDto> UpdateStatusAsync(int id, SupportTicketStatus status, int updatedById, string? reason = null)
    {
        var ticket = await _ticketRepository.GetWithDetailsAsync(id);
        if (ticket == null)
            throw new ArgumentException($"Support ticket with ID {id} not found.");

        await UpdateStatusInternalAsync(ticket, status, updatedById, reason);

        await _ticketRepository.UpdateAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(ticket);
    }

    public async Task<SupportTicketDto> ResolveTicketAsync(int id, string resolution, int resolvedById)
    {
        var ticket = await _ticketRepository.GetWithDetailsAsync(id);
        if (ticket == null)
            throw new ArgumentException($"Support ticket with ID {id} not found.");

        ticket.Resolution = resolution;
        ticket.ResolvedAt = DateTime.UtcNow;
        ticket.ResolutionTime = ticket.ResolvedAt - ticket.CreatedAt;

        await UpdateStatusInternalAsync(ticket, SupportTicketStatus.Resolved, resolvedById, "Ticket resolved");

        await _ticketRepository.UpdateAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(ticket);
    }

    public async Task<SupportTicketDto> CloseTicketAsync(int id, int closedById, int? satisfactionRating = null, string? feedbackComments = null)
    {
        var ticket = await _ticketRepository.GetWithDetailsAsync(id);
        if (ticket == null)
            throw new ArgumentException($"Support ticket with ID {id} not found.");

        ticket.ClosedAt = DateTime.UtcNow;
        ticket.SatisfactionRating = satisfactionRating;
        ticket.FeedbackComments = feedbackComments;

        if (ticket.ResolvedAt == null)
        {
            ticket.ResolvedAt = ticket.ClosedAt;
            ticket.ResolutionTime = ticket.ResolvedAt - ticket.CreatedAt;
        }

        await UpdateStatusInternalAsync(ticket, SupportTicketStatus.Closed, closedById, "Ticket closed");

        await _ticketRepository.UpdateAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(ticket);
    }

    public async Task<SupportTicketDto> ReopenTicketAsync(int id, string reason, int reopenedById)
    {
        var ticket = await _ticketRepository.GetWithDetailsAsync(id);
        if (ticket == null)
            throw new ArgumentException($"Support ticket with ID {id} not found.");

        // Reset resolution fields
        ticket.ResolvedAt = null;
        ticket.ClosedAt = null;
        ticket.ResolutionTime = null;
        ticket.Resolution = null;

        await UpdateStatusInternalAsync(ticket, SupportTicketStatus.Reopened, reopenedById, reason);

        await _ticketRepository.UpdateAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(ticket);
    }

    public async Task<List<SupportTicketDto>> GetMyTicketsAsync(int employeeId)
    {
        var tickets = await _ticketRepository.GetByRequesterIdAsync(employeeId);
        return tickets.Select(MapToDto).ToList();
    }

    public async Task<List<SupportTicketDto>> GetAssignedTicketsAsync(int employeeId)
    {
        var tickets = await _ticketRepository.GetByAssignedToIdAsync(employeeId);
        return tickets.Select(MapToDto).ToList();
    }

    public async Task<List<SupportTicketDto>> GetOverdueTicketsAsync()
    {
        var tickets = await _ticketRepository.GetOverdueTicketsAsync();
        return tickets.Select(MapToDto).ToList();
    }

    public async Task<SupportTicketCommentDto> AddCommentAsync(int ticketId, CreateSupportTicketCommentDto dto, int authorId)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId);
        if (ticket == null)
            throw new ArgumentException($"Support ticket with ID {ticketId} not found.");

        var comment = new SupportTicketComment
        {
            SupportTicketId = ticketId,
            AuthorId = authorId,
            Comment = dto.Comment,
            IsInternal = dto.IsInternal,
            AttachmentPath = dto.AttachmentPath
        };

        await _commentRepository.AddAsync(comment);
        await _unitOfWork.SaveChangesAsync();

        // Load comment with author details
        var createdComment = await _commentRepository.GetByIdAsync(comment.Id);
        return MapCommentToDto(createdComment!);
    }

    public async Task<List<SupportTicketCommentDto>> GetTicketCommentsAsync(int ticketId, bool includeInternal = false)
    {
        var comments = includeInternal 
            ? await _commentRepository.GetByTicketIdAsync(ticketId)
            : await _commentRepository.GetPublicCommentsByTicketIdAsync(ticketId);

        return comments.Select(MapCommentToDto).ToList();
    }

    public async Task<SupportTicketAnalyticsDto> GetAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        return await _ticketRepository.GetAnalyticsAsync(fromDate, toDate);
    }

    public async Task DeleteTicketAsync(int id)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);
        if (ticket == null)
            throw new ArgumentException($"Support ticket with ID {id} not found.");

        await _ticketRepository.DeleteAsync(ticket);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateStatusInternalAsync(SupportTicket ticket, SupportTicketStatus newStatus, int changedById, string? reason)
    {
        var oldStatus = ticket.Status;
        ticket.Status = newStatus;

        var statusHistory = new SupportTicketStatusHistory
        {
            SupportTicketId = ticket.Id,
            FromStatus = oldStatus,
            ToStatus = newStatus,
            ChangedById = changedById,
            ChangedAt = DateTime.UtcNow,
            Reason = reason
        };

        ticket.StatusHistory.Add(statusHistory);
    }

    private SupportTicketDto MapToDto(SupportTicket ticket)
    {
        if (ticket == null)
            throw new ArgumentNullException(nameof(ticket));

        return new SupportTicketDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber ?? string.Empty,
            Title = ticket.Title ?? string.Empty,
            Description = ticket.Description ?? string.Empty,
            Category = ticket.Category,
            CategoryName = ticket.Category.ToString(),
            Priority = ticket.Priority,
            PriorityName = ticket.Priority.ToString(),
            Status = ticket.Status,
            StatusName = ticket.Status.ToString(),
            RequesterId = ticket.RequesterId,
            RequesterName = ticket.Requester != null ? $"{ticket.Requester.FirstName} {ticket.Requester.LastName}".Trim() : "Unknown",
            RequesterEmail = ticket.Requester?.Email ?? string.Empty,
            AssignedToId = ticket.AssignedToId,
            AssignedToName = ticket.AssignedTo != null ? $"{ticket.AssignedTo.FirstName} {ticket.AssignedTo.LastName}".Trim() : null,
            CreatedAt = ticket.CreatedAt,
            AssignedAt = ticket.AssignedAt,
            ResolvedAt = ticket.ResolvedAt,
            ClosedAt = ticket.ClosedAt,
            Resolution = ticket.Resolution,
            RequiresRemoteAccess = ticket.RequiresRemoteAccess,
            RemoteAccessDetails = ticket.RemoteAccessDetails,
            AssetId = ticket.AssetId,
            AssetName = ticket.Asset?.Name,
            AttachmentPath = ticket.AttachmentPath,
            ResolutionTime = ticket.ResolutionTime,
            SatisfactionRating = ticket.SatisfactionRating,
            FeedbackComments = ticket.FeedbackComments,
            CommentsCount = ticket.Comments?.Count ?? 0
        };
    }

    private SupportTicketDto MapToDtoBasic(SupportTicket ticket)
    {
        if (ticket == null)
            throw new ArgumentNullException(nameof(ticket));

        return new SupportTicketDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber ?? string.Empty,
            Title = ticket.Title ?? string.Empty,
            Description = ticket.Description ?? string.Empty,
            Category = ticket.Category,
            CategoryName = ticket.Category.ToString(),
            Priority = ticket.Priority,
            PriorityName = ticket.Priority.ToString(),
            Status = ticket.Status,
            StatusName = ticket.Status.ToString(),
            RequesterId = ticket.RequesterId,
            RequesterName = "Unknown", // Will be populated when navigation properties are available
            RequesterEmail = string.Empty,
            AssignedToId = ticket.AssignedToId,
            AssignedToName = null,
            CreatedAt = ticket.CreatedAt,
            AssignedAt = ticket.AssignedAt,
            ResolvedAt = ticket.ResolvedAt,
            ClosedAt = ticket.ClosedAt,
            Resolution = ticket.Resolution,
            RequiresRemoteAccess = ticket.RequiresRemoteAccess,
            RemoteAccessDetails = ticket.RemoteAccessDetails,
            AssetId = ticket.AssetId,
            AssetName = null,
            AttachmentPath = ticket.AttachmentPath,
            ResolutionTime = ticket.ResolutionTime,
            SatisfactionRating = ticket.SatisfactionRating,
            FeedbackComments = ticket.FeedbackComments,
            CommentsCount = 0
        };
    }

    private SupportTicketCommentDto MapCommentToDto(SupportTicketComment comment)
    {
        return new SupportTicketCommentDto
        {
            Id = comment.Id,
            SupportTicketId = comment.SupportTicketId,
            AuthorId = comment.AuthorId,
            AuthorName = $"{comment.Author?.FirstName} {comment.Author?.LastName}".Trim(),
            Comment = comment.Comment,
            CreatedAt = comment.CreatedAt,
            IsInternal = comment.IsInternal,
            AttachmentPath = comment.AttachmentPath
        };
    }
}