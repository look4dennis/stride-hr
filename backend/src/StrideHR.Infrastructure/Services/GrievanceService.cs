using AutoMapper;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Grievance;
using StrideHR.Core.Models.Notification;

namespace StrideHR.Infrastructure.Services;

public class GrievanceService : IGrievanceService
{
    private readonly IGrievanceRepository _grievanceRepository;
    private readonly IGrievanceCommentRepository _commentRepository;
    private readonly IGrievanceFollowUpRepository _followUpRepository;
    private readonly IRepository<GrievanceStatusHistory> _statusHistoryRepository;
    private readonly IRepository<GrievanceEscalation> _escalationRepository;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly ILogger<GrievanceService> _logger;

    public GrievanceService(
        IGrievanceRepository grievanceRepository,
        IGrievanceCommentRepository commentRepository,
        IGrievanceFollowUpRepository followUpRepository,
        IRepository<GrievanceStatusHistory> statusHistoryRepository,
        IRepository<GrievanceEscalation> escalationRepository,
        INotificationService notificationService,
        IMapper mapper,
        ILogger<GrievanceService> logger)
    {
        _grievanceRepository = grievanceRepository;
        _commentRepository = commentRepository;
        _followUpRepository = followUpRepository;
        _statusHistoryRepository = statusHistoryRepository;
        _escalationRepository = escalationRepository;
        _notificationService = notificationService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<GrievanceDto> CreateGrievanceAsync(CreateGrievanceDto dto, int submitterId)
    {
        var grievance = new Grievance
        {
            GrievanceNumber = await _grievanceRepository.GenerateGrievanceNumberAsync(),
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Priority = dto.Priority,
            IsAnonymous = dto.IsAnonymous,
            SubmittedById = submitterId,
            RequiresInvestigation = dto.RequiresInvestigation,
            AttachmentPath = dto.AttachmentPath,
            Status = GrievanceStatus.Submitted,
            CurrentEscalationLevel = dto.PreferredEscalationLevel ?? EscalationLevel.Level1_DirectManager,
            DueDate = CalculateDueDate(dto.Priority),
            CreatedBy = submitterId.ToString()
        };

        await _grievanceRepository.AddAsync(grievance);
        await _grievanceRepository.SaveChangesAsync();

        // Create initial status history
        var statusHistory = new GrievanceStatusHistory
        {
            GrievanceId = grievance.Id,
            FromStatus = GrievanceStatus.Submitted,
            ToStatus = GrievanceStatus.Submitted,
            ChangedById = submitterId,
            Reason = "Grievance submitted",
            CreatedBy = submitterId.ToString()
        };

        await _statusHistoryRepository.AddAsync(statusHistory);
        await _statusHistoryRepository.SaveChangesAsync();

        // Send notification to appropriate personnel
        await NotifyGrievanceSubmission(grievance);

        _logger.LogInformation("Grievance {GrievanceNumber} created by employee {SubmitterId}", 
            grievance.GrievanceNumber, submitterId);

        var createdGrievance = await _grievanceRepository.GetWithDetailsAsync(grievance.Id);
        return _mapper.Map<GrievanceDto>(createdGrievance);
    }

    public async Task<GrievanceDto> GetGrievanceByIdAsync(int id)
    {
        var grievance = await _grievanceRepository.GetWithDetailsAsync(id);
        if (grievance == null)
            throw new ArgumentException($"Grievance with ID {id} not found");

        var dto = _mapper.Map<GrievanceDto>(grievance);
        
        // Calculate additional properties
        dto.CommentsCount = await _commentRepository.GetCommentsCountAsync(id);
        dto.EscalationsCount = grievance.Escalations.Count;
        dto.FollowUpsCount = await _followUpRepository.GetFollowUpsCountAsync(id);
        dto.IsOverdue = grievance.DueDate.HasValue && grievance.DueDate.Value < DateTime.UtcNow && 
                       grievance.Status != GrievanceStatus.Resolved && grievance.Status != GrievanceStatus.Closed;
        
        if (grievance.ResolvedAt.HasValue)
        {
            dto.ResolutionTime = grievance.ResolvedAt.Value - grievance.CreatedAt;
        }

        return dto;
    }

    public async Task<GrievanceDto?> GetGrievanceByNumberAsync(string grievanceNumber)
    {
        var grievance = await _grievanceRepository.GetByGrievanceNumberAsync(grievanceNumber);
        if (grievance == null)
            return null;

        return await GetGrievanceByIdAsync(grievance.Id);
    }

    public async Task<(List<GrievanceDto> Grievances, int TotalCount)> SearchGrievancesAsync(GrievanceSearchCriteria criteria)
    {
        var (grievances, totalCount) = await _grievanceRepository.SearchAsync(criteria);
        
        var grievanceDtos = new List<GrievanceDto>();
        foreach (var grievance in grievances)
        {
            var dto = _mapper.Map<GrievanceDto>(grievance);
            dto.CommentsCount = await _commentRepository.GetCommentsCountAsync(grievance.Id);
            dto.EscalationsCount = grievance.Escalations.Count;
            dto.FollowUpsCount = await _followUpRepository.GetFollowUpsCountAsync(grievance.Id);
            dto.IsOverdue = grievance.DueDate.HasValue && grievance.DueDate.Value < DateTime.UtcNow && 
                           grievance.Status != GrievanceStatus.Resolved && grievance.Status != GrievanceStatus.Closed;
            
            if (grievance.ResolvedAt.HasValue)
            {
                dto.ResolutionTime = grievance.ResolvedAt.Value - grievance.CreatedAt;
            }
            
            grievanceDtos.Add(dto);
        }

        return (grievanceDtos, totalCount);
    }

    public async Task<GrievanceDto> UpdateGrievanceAsync(int id, UpdateGrievanceDto dto, int updatedById)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(id);
        if (grievance == null)
            throw new ArgumentException($"Grievance with ID {id} not found");

        // Update fields if provided
        if (!string.IsNullOrEmpty(dto.Title))
            grievance.Title = dto.Title;
        
        if (!string.IsNullOrEmpty(dto.Description))
            grievance.Description = dto.Description;
        
        if (dto.Category.HasValue)
            grievance.Category = dto.Category.Value;
        
        if (dto.Priority.HasValue)
            grievance.Priority = dto.Priority.Value;
        
        if (dto.RequiresInvestigation.HasValue)
            grievance.RequiresInvestigation = dto.RequiresInvestigation.Value;
        
        if (!string.IsNullOrEmpty(dto.InvestigationNotes))
            grievance.InvestigationNotes = dto.InvestigationNotes;
        
        if (!string.IsNullOrEmpty(dto.AttachmentPath))
            grievance.AttachmentPath = dto.AttachmentPath;

        grievance.UpdatedAt = DateTime.UtcNow;
        grievance.UpdatedBy = updatedById.ToString();

        await _grievanceRepository.UpdateAsync(grievance);
        await _grievanceRepository.SaveChangesAsync();

        _logger.LogInformation("Grievance {GrievanceNumber} updated by employee {UpdatedById}", 
            grievance.GrievanceNumber, updatedById);

        return await GetGrievanceByIdAsync(id);
    }

    public async Task<GrievanceDto> AssignGrievanceAsync(int id, int assignedToId, int assignedById)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(id);
        if (grievance == null)
            throw new ArgumentException($"Grievance with ID {id} not found");

        grievance.AssignedToId = assignedToId;
        grievance.UpdatedAt = DateTime.UtcNow;
        grievance.UpdatedBy = assignedById.ToString();

        // Update status to UnderReview if it's still Submitted
        if (grievance.Status == GrievanceStatus.Submitted)
        {
            await UpdateStatusAsync(id, GrievanceStatus.UnderReview, assignedById, "Grievance assigned for review");
        }

        await _grievanceRepository.UpdateAsync(grievance);
        await _grievanceRepository.SaveChangesAsync();

        // Send notification to assigned person
        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = assignedToId,
            Title = "Grievance Assigned",
            Message = $"Grievance {grievance.GrievanceNumber} has been assigned to you for review.",
            Type = NotificationType.GrievanceAssigned,
            Priority = NotificationPriority.Normal,
            Channel = NotificationChannel.InApp
        });

        _logger.LogInformation("Grievance {GrievanceNumber} assigned to employee {AssignedToId} by {AssignedById}", 
            grievance.GrievanceNumber, assignedToId, assignedById);

        return await GetGrievanceByIdAsync(id);
    }

    public async Task<GrievanceDto> UpdateStatusAsync(int id, GrievanceStatus status, int updatedById, string? reason = null)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(id);
        if (grievance == null)
            throw new ArgumentException($"Grievance with ID {id} not found");

        var oldStatus = grievance.Status;
        grievance.Status = status;
        grievance.UpdatedAt = DateTime.UtcNow;
        grievance.UpdatedBy = updatedById.ToString();

        await _grievanceRepository.UpdateAsync(grievance);

        // Create status history
        var statusHistory = new GrievanceStatusHistory
        {
            GrievanceId = id,
            FromStatus = oldStatus,
            ToStatus = status,
            ChangedById = updatedById,
            Reason = reason,
            CreatedBy = updatedById.ToString()
        };

        await _statusHistoryRepository.AddAsync(statusHistory);
        await _grievanceRepository.SaveChangesAsync();

        // Send notification to submitter
        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = grievance.SubmittedById,
            Title = "Grievance Status Updated",
            Message = $"Your grievance {grievance.GrievanceNumber} status has been updated to {status}.",
            Type = NotificationType.GrievanceStatusChanged,
            Priority = NotificationPriority.Normal,
            Channel = NotificationChannel.InApp
        });

        _logger.LogInformation("Grievance {GrievanceNumber} status updated from {OldStatus} to {NewStatus} by {UpdatedById}", 
            grievance.GrievanceNumber, oldStatus, status, updatedById);

        return await GetGrievanceByIdAsync(id);
    }

    public async Task<GrievanceDto> ResolveGrievanceAsync(int id, string resolution, string? resolutionNotes, int resolvedById)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(id);
        if (grievance == null)
            throw new ArgumentException($"Grievance with ID {id} not found");

        grievance.Status = GrievanceStatus.Resolved;
        grievance.Resolution = resolution;
        grievance.ResolutionNotes = resolutionNotes;
        grievance.ResolvedById = resolvedById;
        grievance.ResolvedAt = DateTime.UtcNow;
        grievance.UpdatedAt = DateTime.UtcNow;
        grievance.UpdatedBy = resolvedById.ToString();

        await _grievanceRepository.UpdateAsync(grievance);

        // Create status history
        var statusHistory = new GrievanceStatusHistory
        {
            GrievanceId = id,
            FromStatus = grievance.Status,
            ToStatus = GrievanceStatus.Resolved,
            ChangedById = resolvedById,
            Reason = "Grievance resolved",
            Notes = resolution,
            CreatedBy = resolvedById.ToString()
        };

        await _statusHistoryRepository.AddAsync(statusHistory);
        await _grievanceRepository.SaveChangesAsync();

        // Send notification to submitter
        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = grievance.SubmittedById,
            Title = "Grievance Resolved",
            Message = $"Your grievance {grievance.GrievanceNumber} has been resolved. Resolution: {resolution}",
            Type = NotificationType.GrievanceResolved,
            Priority = NotificationPriority.High,
            Channel = NotificationChannel.InApp
        });

        _logger.LogInformation("Grievance {GrievanceNumber} resolved by employee {ResolvedById}", 
            grievance.GrievanceNumber, resolvedById);

        return await GetGrievanceByIdAsync(id);
    }

    public async Task<GrievanceDto> CloseGrievanceAsync(int id, int closedById, int? satisfactionRating = null, string? feedbackComments = null)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(id);
        if (grievance == null)
            throw new ArgumentException($"Grievance with ID {id} not found");

        if (grievance.Status != GrievanceStatus.Resolved)
            throw new InvalidOperationException("Only resolved grievances can be closed");

        grievance.Status = GrievanceStatus.Closed;
        grievance.ClosedAt = DateTime.UtcNow;
        grievance.SatisfactionRating = satisfactionRating;
        grievance.FeedbackComments = feedbackComments;
        grievance.UpdatedAt = DateTime.UtcNow;
        grievance.UpdatedBy = closedById.ToString();

        await _grievanceRepository.UpdateAsync(grievance);

        // Create status history
        var statusHistory = new GrievanceStatusHistory
        {
            GrievanceId = id,
            FromStatus = GrievanceStatus.Resolved,
            ToStatus = GrievanceStatus.Closed,
            ChangedById = closedById,
            Reason = "Grievance closed",
            Notes = feedbackComments,
            CreatedBy = closedById.ToString()
        };

        await _statusHistoryRepository.AddAsync(statusHistory);
        await _grievanceRepository.SaveChangesAsync();

        _logger.LogInformation("Grievance {GrievanceNumber} closed by employee {ClosedById} with satisfaction rating {Rating}", 
            grievance.GrievanceNumber, closedById, satisfactionRating);

        return await GetGrievanceByIdAsync(id);
    }

    public async Task<GrievanceDto> EscalateGrievanceAsync(int id, EscalationLevel toLevel, string reason, int escalatedById, int? escalatedToId = null)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(id);
        if (grievance == null)
            throw new ArgumentException($"Grievance with ID {id} not found");

        var fromLevel = grievance.CurrentEscalationLevel;
        grievance.CurrentEscalationLevel = toLevel;
        grievance.IsEscalated = true;
        grievance.EscalatedAt = DateTime.UtcNow;
        grievance.EscalatedById = escalatedById;
        grievance.EscalationReason = reason;
        grievance.AssignedToId = escalatedToId;
        grievance.UpdatedAt = DateTime.UtcNow;
        grievance.UpdatedBy = escalatedById.ToString();

        await _grievanceRepository.UpdateAsync(grievance);

        // Create escalation record
        var escalation = new GrievanceEscalation
        {
            GrievanceId = id,
            FromLevel = fromLevel,
            ToLevel = toLevel,
            EscalatedById = escalatedById,
            EscalatedToId = escalatedToId,
            Reason = reason,
            EscalatedAt = DateTime.UtcNow,
            CreatedBy = escalatedById.ToString()
        };

        await _escalationRepository.AddAsync(escalation);
        await _grievanceRepository.SaveChangesAsync();

        // Send notifications
        if (escalatedToId.HasValue)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = escalatedToId.Value,
                Title = "Grievance Escalated",
                Message = $"Grievance {grievance.GrievanceNumber} has been escalated to you. Reason: {reason}",
                Type = NotificationType.GrievanceEscalated,
                Priority = NotificationPriority.High,
                Channel = NotificationChannel.InApp
            });
        }

        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
        {
            UserId = grievance.SubmittedById,
            Title = "Grievance Escalated",
            Message = $"Your grievance {grievance.GrievanceNumber} has been escalated to {toLevel}.",
            Type = NotificationType.GrievanceEscalated,
            Priority = NotificationPriority.High,
            Channel = NotificationChannel.InApp
        });

        _logger.LogInformation("Grievance {GrievanceNumber} escalated from {FromLevel} to {ToLevel} by employee {EscalatedById}", 
            grievance.GrievanceNumber, fromLevel, toLevel, escalatedById);

        return await GetGrievanceByIdAsync(id);
    }

    public async Task<GrievanceDto> WithdrawGrievanceAsync(int id, string reason, int withdrawnById)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(id);
        if (grievance == null)
            throw new ArgumentException($"Grievance with ID {id} not found");

        if (grievance.SubmittedById != withdrawnById)
            throw new UnauthorizedAccessException("Only the submitter can withdraw a grievance");

        if (grievance.Status == GrievanceStatus.Closed || grievance.Status == GrievanceStatus.Withdrawn)
            throw new InvalidOperationException("Cannot withdraw a closed or already withdrawn grievance");

        await UpdateStatusAsync(id, GrievanceStatus.Withdrawn, withdrawnById, reason);

        _logger.LogInformation("Grievance {GrievanceNumber} withdrawn by employee {WithdrawnById}. Reason: {Reason}", 
            grievance.GrievanceNumber, withdrawnById, reason);

        return await GetGrievanceByIdAsync(id);
    }

    public async Task<List<GrievanceDto>> GetMyGrievancesAsync(int employeeId)
    {
        var grievances = await _grievanceRepository.GetBySubmitterIdAsync(employeeId);
        var grievanceDtos = new List<GrievanceDto>();

        foreach (var grievance in grievances)
        {
            var dto = _mapper.Map<GrievanceDto>(grievance);
            dto.CommentsCount = await _commentRepository.GetCommentsCountAsync(grievance.Id);
            dto.EscalationsCount = grievance.Escalations.Count;
            dto.FollowUpsCount = await _followUpRepository.GetFollowUpsCountAsync(grievance.Id);
            dto.IsOverdue = grievance.DueDate.HasValue && grievance.DueDate.Value < DateTime.UtcNow && 
                           grievance.Status != GrievanceStatus.Resolved && grievance.Status != GrievanceStatus.Closed;
            
            if (grievance.ResolvedAt.HasValue)
            {
                dto.ResolutionTime = grievance.ResolvedAt.Value - grievance.CreatedAt;
            }
            
            grievanceDtos.Add(dto);
        }

        return grievanceDtos;
    }

    public async Task<List<GrievanceDto>> GetAssignedGrievancesAsync(int employeeId)
    {
        var grievances = await _grievanceRepository.GetByAssignedToIdAsync(employeeId);
        var grievanceDtos = new List<GrievanceDto>();

        foreach (var grievance in grievances)
        {
            var dto = _mapper.Map<GrievanceDto>(grievance);
            dto.CommentsCount = await _commentRepository.GetCommentsCountAsync(grievance.Id);
            dto.EscalationsCount = grievance.Escalations.Count;
            dto.FollowUpsCount = await _followUpRepository.GetFollowUpsCountAsync(grievance.Id);
            dto.IsOverdue = grievance.DueDate.HasValue && grievance.DueDate.Value < DateTime.UtcNow && 
                           grievance.Status != GrievanceStatus.Resolved && grievance.Status != GrievanceStatus.Closed;
            
            if (grievance.ResolvedAt.HasValue)
            {
                dto.ResolutionTime = grievance.ResolvedAt.Value - grievance.CreatedAt;
            }
            
            grievanceDtos.Add(dto);
        }

        return grievanceDtos;
    }

    public async Task<List<GrievanceDto>> GetOverdueGrievancesAsync()
    {
        var grievances = await _grievanceRepository.GetOverdueGrievancesAsync();
        return _mapper.Map<List<GrievanceDto>>(grievances);
    }

    public async Task<List<GrievanceDto>> GetEscalatedGrievancesAsync()
    {
        var grievances = await _grievanceRepository.GetEscalatedGrievancesAsync();
        return _mapper.Map<List<GrievanceDto>>(grievances);
    }

    public async Task<List<GrievanceDto>> GetAnonymousGrievancesAsync()
    {
        var grievances = await _grievanceRepository.GetAnonymousGrievancesAsync();
        return _mapper.Map<List<GrievanceDto>>(grievances);
    }

    public async Task<GrievanceCommentDto> AddCommentAsync(int grievanceId, CreateGrievanceCommentDto dto, int authorId)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(grievanceId);
        if (grievance == null)
            throw new ArgumentException($"Grievance with ID {grievanceId} not found");

        var comment = new GrievanceComment
        {
            GrievanceId = grievanceId,
            Comment = dto.Comment,
            AuthorId = authorId,
            IsInternal = dto.IsInternal,
            AttachmentPath = dto.AttachmentPath,
            CreatedBy = authorId.ToString()
        };

        await _commentRepository.AddAsync(comment);
        await _commentRepository.SaveChangesAsync();

        // Send notification to relevant parties
        if (!dto.IsInternal)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = grievance.SubmittedById,
                Title = "New Comment on Grievance",
                Message = $"A new comment has been added to your grievance {grievance.GrievanceNumber}.",
                Type = NotificationType.GrievanceCommentAdded,
                Priority = NotificationPriority.Normal,
                Channel = NotificationChannel.InApp
            });
        }

        _logger.LogInformation("Comment added to grievance {GrievanceNumber} by employee {AuthorId}", 
            grievance.GrievanceNumber, authorId);

        var createdComment = await _commentRepository.GetByIdAsync(comment.Id);
        return _mapper.Map<GrievanceCommentDto>(createdComment);
    }

    public async Task<List<GrievanceCommentDto>> GetGrievanceCommentsAsync(int grievanceId, bool includeInternal = false)
    {
        var comments = await _commentRepository.GetByGrievanceIdAsync(grievanceId, includeInternal);
        return _mapper.Map<List<GrievanceCommentDto>>(comments);
    }

    public async Task<GrievanceFollowUpDto> ScheduleFollowUpAsync(int grievanceId, CreateGrievanceFollowUpDto dto, int scheduledById)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(grievanceId);
        if (grievance == null)
            throw new ArgumentException($"Grievance with ID {grievanceId} not found");

        var followUp = new GrievanceFollowUp
        {
            GrievanceId = grievanceId,
            Title = dto.Title,
            Description = dto.Description,
            ScheduledDate = dto.ScheduledDate,
            ScheduledById = scheduledById,
            CreatedBy = scheduledById.ToString()
        };

        await _followUpRepository.AddAsync(followUp);
        await _followUpRepository.SaveChangesAsync();

        _logger.LogInformation("Follow-up scheduled for grievance {GrievanceNumber} by employee {ScheduledById}", 
            grievance.GrievanceNumber, scheduledById);

        var createdFollowUp = await _followUpRepository.GetByIdAsync(followUp.Id);
        return _mapper.Map<GrievanceFollowUpDto>(createdFollowUp);
    }

    public async Task<GrievanceFollowUpDto> CompleteFollowUpAsync(int followUpId, CompleteGrievanceFollowUpDto dto, int completedById)
    {
        var followUp = await _followUpRepository.GetByIdAsync(followUpId);
        if (followUp == null)
            throw new ArgumentException($"Follow-up with ID {followUpId} not found");

        followUp.IsCompleted = true;
        followUp.CompletedAt = DateTime.UtcNow;
        followUp.CompletedById = completedById;
        followUp.CompletionNotes = dto.CompletionNotes;
        followUp.UpdatedAt = DateTime.UtcNow;
        followUp.UpdatedBy = completedById.ToString();

        await _followUpRepository.UpdateAsync(followUp);
        await _followUpRepository.SaveChangesAsync();

        _logger.LogInformation("Follow-up {FollowUpId} completed by employee {CompletedById}", 
            followUpId, completedById);

        var updatedFollowUp = await _followUpRepository.GetByIdAsync(followUpId);
        return _mapper.Map<GrievanceFollowUpDto>(updatedFollowUp);
    }

    public async Task<List<GrievanceFollowUpDto>> GetGrievanceFollowUpsAsync(int grievanceId)
    {
        var followUps = await _followUpRepository.GetByGrievanceIdAsync(grievanceId);
        return _mapper.Map<List<GrievanceFollowUpDto>>(followUps);
    }

    public async Task<List<GrievanceFollowUpDto>> GetPendingFollowUpsAsync()
    {
        var followUps = await _followUpRepository.GetPendingFollowUpsAsync();
        return _mapper.Map<List<GrievanceFollowUpDto>>(followUps);
    }

    public async Task<GrievanceAnalyticsDto> GetAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        return await _grievanceRepository.GetAnalyticsAsync(fromDate, toDate);
    }

    public async Task DeleteGrievanceAsync(int id)
    {
        var grievance = await _grievanceRepository.GetByIdAsync(id);
        if (grievance == null)
            throw new ArgumentException($"Grievance with ID {id} not found");

        await _grievanceRepository.DeleteAsync(grievance);
        await _grievanceRepository.SaveChangesAsync();

        _logger.LogInformation("Grievance {GrievanceNumber} deleted", grievance.GrievanceNumber);
    }

    private DateTime CalculateDueDate(GrievancePriority priority)
    {
        var businessDays = priority switch
        {
            GrievancePriority.Critical => 1,
            GrievancePriority.Urgent => 2,
            GrievancePriority.High => 5,
            GrievancePriority.Medium => 10,
            GrievancePriority.Low => 15,
            _ => 10
        };

        var dueDate = DateTime.UtcNow;
        var addedDays = 0;

        while (addedDays < businessDays)
        {
            dueDate = dueDate.AddDays(1);
            if (dueDate.DayOfWeek != DayOfWeek.Saturday && dueDate.DayOfWeek != DayOfWeek.Sunday)
            {
                addedDays++;
            }
        }

        return dueDate;
    }

    private async Task NotifyGrievanceSubmission(Grievance grievance)
    {
        // Determine who to notify based on escalation level
        var notificationMessage = $"New grievance {grievance.GrievanceNumber} has been submitted";
        
        if (grievance.IsAnonymous)
        {
            notificationMessage += " (Anonymous)";
        }

        // This would typically involve finding the appropriate HR personnel or managers
        // based on the escalation level and sending notifications
        // For now, we'll just log it
        _logger.LogInformation("Grievance submission notification: {Message}", notificationMessage);
    }
}