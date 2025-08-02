using AutoMapper;
using StrideHR.Core.Entities;
using StrideHR.Core.Models.Email;

namespace StrideHR.Infrastructure.Mapping;

public class EmailMappingProfile : Profile
{
    public EmailMappingProfile()
    {
        // EmailTemplate mappings
        CreateMap<EmailTemplate, EmailTemplateDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null))
            .ForMember(dest => dest.UsageCount, opt => opt.Ignore()); // Will be populated separately

        CreateMap<CreateEmailTemplateDto, EmailTemplate>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Branch, opt => opt.Ignore())
            .ForMember(dest => dest.EmailLogs, opt => opt.Ignore());

        CreateMap<UpdateEmailTemplateDto, EmailTemplate>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.BranchId, opt => opt.Ignore())
            .ForMember(dest => dest.Branch, opt => opt.Ignore())
            .ForMember(dest => dest.EmailLogs, opt => opt.Ignore());

        // EmailLog mappings
        CreateMap<EmailLog, EmailLogDto>()
            .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.EmailTemplate != null ? src.EmailTemplate.Name : null))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null && src.User.Employee != null ? $"{src.User.Employee.FirstName} {src.User.Employee.LastName}" : null))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null));

        CreateMap<SendEmailDto, EmailLog>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmailTemplateId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.SentAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeliveredAt, opt => opt.Ignore())
            .ForMember(dest => dest.OpenedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ClickedAt, opt => opt.Ignore())
            .ForMember(dest => dest.BouncedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ErrorMessage, opt => opt.Ignore())
            .ForMember(dest => dest.ExternalId, opt => opt.Ignore())
            .ForMember(dest => dest.RetryCount, opt => opt.Ignore())
            .ForMember(dest => dest.NextRetryAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.EmailTemplate, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Branch, opt => opt.Ignore());

        // EmailCampaign mappings
        CreateMap<EmailCampaign, EmailCampaignDto>()
            .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.EmailTemplate.Name))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedByUser.Employee != null ? $"{src.CreatedByUser.Employee.FirstName} {src.CreatedByUser.Employee.LastName}" : src.CreatedByUser.Username));

        CreateMap<CreateEmailCampaignDto, EmailCampaign>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TotalRecipients, opt => opt.Ignore())
            .ForMember(dest => dest.SentCount, opt => opt.Ignore())
            .ForMember(dest => dest.DeliveredCount, opt => opt.Ignore())
            .ForMember(dest => dest.OpenedCount, opt => opt.Ignore())
            .ForMember(dest => dest.ClickedCount, opt => opt.Ignore())
            .ForMember(dest => dest.BouncedCount, opt => opt.Ignore())
            .ForMember(dest => dest.FailedCount, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.EmailTemplate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.EmailLogs, opt => opt.Ignore());

        CreateMap<UpdateEmailCampaignDto, EmailCampaign>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TotalRecipients, opt => opt.Ignore())
            .ForMember(dest => dest.SentCount, opt => opt.Ignore())
            .ForMember(dest => dest.DeliveredCount, opt => opt.Ignore())
            .ForMember(dest => dest.OpenedCount, opt => opt.Ignore())
            .ForMember(dest => dest.ClickedCount, opt => opt.Ignore())
            .ForMember(dest => dest.BouncedCount, opt => opt.Ignore())
            .ForMember(dest => dest.FailedCount, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.EmailTemplate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUser, opt => opt.Ignore())
            .ForMember(dest => dest.EmailLogs, opt => opt.Ignore());

        CreateMap<EmailCampaign, EmailCampaignStatsDto>()
            .ForMember(dest => dest.CampaignId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CampaignName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.DeliveryRate, opt => opt.MapFrom(src => 
                src.TotalRecipients > 0 ? (decimal)src.DeliveredCount / src.TotalRecipients * 100 : 0))
            .ForMember(dest => dest.OpenRate, opt => opt.MapFrom(src => 
                src.DeliveredCount > 0 ? (decimal)src.OpenedCount / src.DeliveredCount * 100 : 0))
            .ForMember(dest => dest.ClickRate, opt => opt.MapFrom(src => 
                src.OpenedCount > 0 ? (decimal)src.ClickedCount / src.OpenedCount * 100 : 0))
            .ForMember(dest => dest.BounceRate, opt => opt.MapFrom(src => 
                src.TotalRecipients > 0 ? (decimal)src.BouncedCount / src.TotalRecipients * 100 : 0))
            .ForMember(dest => dest.FailureRate, opt => opt.MapFrom(src => 
                src.TotalRecipients > 0 ? (decimal)src.FailedCount / src.TotalRecipients * 100 : 0))
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => 
                src.StartedAt.HasValue && src.CompletedAt.HasValue 
                    ? src.CompletedAt.Value - src.StartedAt.Value 
                    : (TimeSpan?)null))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedByUser.Employee != null ? $"{src.CreatedByUser.Employee.FirstName} {src.CreatedByUser.Employee.LastName}" : src.CreatedByUser.Username));
    }
}