using AutoMapper;
using StrideHR.Core.Entities;
using StrideHR.Core.Models.SupportTicket;

namespace StrideHR.Infrastructure.Mapping;

public class SupportTicketMappingProfile : Profile
{
    public SupportTicketMappingProfile()
    {
        CreateMap<SupportTicket, SupportTicketDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.ToString()))
            .ForMember(dest => dest.PriorityName, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.RequesterName, opt => opt.MapFrom(src => $"{src.Requester.FirstName} {src.Requester.LastName}".Trim()))
            .ForMember(dest => dest.RequesterEmail, opt => opt.MapFrom(src => src.Requester.Email))
            .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => src.AssignedTo != null ? $"{src.AssignedTo.FirstName} {src.AssignedTo.LastName}".Trim() : null))
            .ForMember(dest => dest.AssetName, opt => opt.MapFrom(src => src.Asset != null ? src.Asset.Name : null))
            .ForMember(dest => dest.CommentsCount, opt => opt.MapFrom(src => src.Comments.Count));

        CreateMap<CreateSupportTicketDto, SupportTicket>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TicketNumber, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.RequesterId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        CreateMap<SupportTicketComment, SupportTicketCommentDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => $"{src.Author.FirstName} {src.Author.LastName}".Trim()));

        CreateMap<CreateSupportTicketCommentDto, SupportTicketComment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupportTicketId, opt => opt.Ignore())
            .ForMember(dest => dest.AuthorId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
    }
}