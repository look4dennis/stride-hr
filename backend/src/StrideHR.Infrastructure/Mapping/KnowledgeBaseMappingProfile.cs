using AutoMapper;
using StrideHR.Core.Entities;
using StrideHR.Core.Models.KnowledgeBase;

namespace StrideHR.Infrastructure.Mapping;

public class KnowledgeBaseMappingProfile : Profile
{
    public KnowledgeBaseMappingProfile()
    {
        // Document mappings
        CreateMap<KnowledgeBaseDocument, KnowledgeBaseDocumentDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => $"{src.Author.FirstName} {src.Author.LastName}"))
            .ForMember(dest => dest.ReviewerName, opt => opt.MapFrom(src => src.Reviewer != null ? $"{src.Reviewer.FirstName} {src.Reviewer.LastName}" : null))
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments))
            .ForMember(dest => dest.Versions, opt => opt.Ignore()); // Will be populated separately

        CreateMap<CreateKnowledgeBaseDocumentDto, KnowledgeBaseDocument>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Core.Enums.DocumentStatus.Draft))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.ViewCount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.DownloadCount, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.Version, opt => opt.MapFrom(src => 1))
            .ForMember(dest => dest.IsCurrentVersion, opt => opt.MapFrom(src => true));

        CreateMap<UpdateKnowledgeBaseDocumentDto, KnowledgeBaseDocument>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Core.Enums.DocumentStatus.Draft));

        // Category mappings
        CreateMap<KnowledgeBaseCategory, KnowledgeBaseCategoryDto>()
            .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
            .ForMember(dest => dest.DocumentCount, opt => opt.MapFrom(src => src.Documents.Count))
            .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.SubCategories));

        // Attachment mappings
        CreateMap<KnowledgeBaseDocumentAttachment, KnowledgeBaseDocumentAttachmentDto>()
            .ForMember(dest => dest.UploadedByName, opt => opt.MapFrom(src => $"{src.UploadedByEmployee.FirstName} {src.UploadedByEmployee.LastName}"));

        // Version mappings
        CreateMap<KnowledgeBaseDocument, KnowledgeBaseDocumentVersionDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => $"{src.Author.FirstName} {src.Author.LastName}"))
            .ForMember(dest => dest.VersionNotes, opt => opt.Ignore()); // This would come from a separate field if needed
    }
}