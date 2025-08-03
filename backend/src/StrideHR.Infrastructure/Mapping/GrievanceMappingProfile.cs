using AutoMapper;
using StrideHR.Core.Entities;
using StrideHR.Core.Models.Grievance;

namespace StrideHR.Infrastructure.Mapping;

public class GrievanceMappingProfile : Profile
{
    public GrievanceMappingProfile()
    {
        // Grievance mappings
        CreateMap<Grievance, GrievanceDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.ToString()))
            .ForMember(dest => dest.PriorityName, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.EscalationLevelName, opt => opt.MapFrom(src => src.CurrentEscalationLevel.ToString()))
            .ForMember(dest => dest.SubmitterName, opt => opt.MapFrom(src => src.IsAnonymous ? "Anonymous" : src.SubmittedBy.FullName))
            .ForMember(dest => dest.SubmitterEmail, opt => opt.MapFrom(src => src.IsAnonymous ? "anonymous@company.com" : src.SubmittedBy.Email))
            .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => src.AssignedTo != null ? src.AssignedTo.FullName : null))
            .ForMember(dest => dest.ResolvedByName, opt => opt.MapFrom(src => src.ResolvedBy != null ? src.ResolvedBy.FullName : null))
            .ForMember(dest => dest.EscalatedByName, opt => opt.MapFrom(src => src.EscalatedBy != null ? src.EscalatedBy.FullName : null))
            .ForMember(dest => dest.CommentsCount, opt => opt.Ignore())
            .ForMember(dest => dest.EscalationsCount, opt => opt.Ignore())
            .ForMember(dest => dest.FollowUpsCount, opt => opt.Ignore())
            .ForMember(dest => dest.IsOverdue, opt => opt.Ignore())
            .ForMember(dest => dest.ResolutionTime, opt => opt.Ignore());

        CreateMap<CreateGrievanceDto, Grievance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.GrievanceNumber, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.SubmittedById, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentEscalationLevel, opt => opt.Ignore())
            .ForMember(dest => dest.DueDate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

        // Grievance Comment mappings
        CreateMap<GrievanceComment, GrievanceCommentDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.FullName));

        CreateMap<CreateGrievanceCommentDto, GrievanceComment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.GrievanceId, opt => opt.Ignore())
            .ForMember(dest => dest.AuthorId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

        // Grievance Follow-up mappings
        CreateMap<GrievanceFollowUp, GrievanceFollowUpDto>()
            .ForMember(dest => dest.ScheduledByName, opt => opt.MapFrom(src => src.ScheduledBy.FullName))
            .ForMember(dest => dest.CompletedByName, opt => opt.MapFrom(src => src.CompletedBy != null ? src.CompletedBy.FullName : null));

        CreateMap<CreateGrievanceFollowUpDto, GrievanceFollowUp>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.GrievanceId, opt => opt.Ignore())
            .ForMember(dest => dest.ScheduledById, opt => opt.Ignore())
            .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

        // Grievance Status History mappings
        CreateMap<GrievanceStatusHistory, GrievanceStatusHistoryDto>()
            .ForMember(dest => dest.ChangedByName, opt => opt.MapFrom(src => src.ChangedBy.FullName))
            .ForMember(dest => dest.FromStatusName, opt => opt.MapFrom(src => src.FromStatus.ToString()))
            .ForMember(dest => dest.ToStatusName, opt => opt.MapFrom(src => src.ToStatus.ToString()));

        // Grievance Escalation mappings
        CreateMap<GrievanceEscalation, GrievanceEscalationDto>()
            .ForMember(dest => dest.EscalatedByName, opt => opt.MapFrom(src => src.EscalatedBy.FullName))
            .ForMember(dest => dest.EscalatedToName, opt => opt.MapFrom(src => src.EscalatedTo != null ? src.EscalatedTo.FullName : null))
            .ForMember(dest => dest.FromLevelName, opt => opt.MapFrom(src => src.FromLevel.ToString()))
            .ForMember(dest => dest.ToLevelName, opt => opt.MapFrom(src => src.ToLevel.ToString()));
    }
}

// Additional DTOs for status history and escalation
public class GrievanceStatusHistoryDto
{
    public int Id { get; set; }
    public int GrievanceId { get; set; }
    public string FromStatusName { get; set; } = string.Empty;
    public string ToStatusName { get; set; } = string.Empty;
    public int ChangedById { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GrievanceEscalationDto
{
    public int Id { get; set; }
    public int GrievanceId { get; set; }
    public string FromLevelName { get; set; } = string.Empty;
    public string ToLevelName { get; set; } = string.Empty;
    public int EscalatedById { get; set; }
    public string EscalatedByName { get; set; } = string.Empty;
    public int? EscalatedToId { get; set; }
    public string? EscalatedToName { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime EscalatedAt { get; set; }
    public bool IsAutoEscalation { get; set; }
}