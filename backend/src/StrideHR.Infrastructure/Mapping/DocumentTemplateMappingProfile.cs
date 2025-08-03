using AutoMapper;
using StrideHR.Core.Entities;
using StrideHR.Core.Models.DocumentTemplate;

namespace StrideHR.Infrastructure.Mapping;

public class DocumentTemplateMappingProfile : Profile
{
    public DocumentTemplateMappingProfile()
    {
        // DocumentTemplate mappings
        CreateMap<DocumentTemplate, DocumentTemplateDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => $"{src.CreatedByEmployee.FirstName} {src.CreatedByEmployee.LastName}"))
            .ForMember(dest => dest.LastModifiedByName, opt => opt.MapFrom(src => src.LastModifiedByEmployee != null ? $"{src.LastModifiedByEmployee.FirstName} {src.LastModifiedByEmployee.LastName}" : null))
            .ForMember(dest => dest.UsageCount, opt => opt.Ignore());

        CreateMap<CreateDocumentTemplateDto, DocumentTemplate>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.IsSystemTemplate, opt => opt.MapFrom(src => false));

        CreateMap<UpdateDocumentTemplateDto, DocumentTemplate>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsSystemTemplate, opt => opt.Ignore());

        // GeneratedDocument mappings
        CreateMap<GeneratedDocument, GeneratedDocumentDto>()
            .ForMember(dest => dest.DocumentTemplateName, opt => opt.MapFrom(src => src.DocumentTemplate.Name))
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => $"{src.Employee.FirstName} {src.Employee.LastName}"))
            .ForMember(dest => dest.GeneratedByName, opt => opt.MapFrom(src => $"{src.GeneratedByEmployee.FirstName} {src.GeneratedByEmployee.LastName}"))
            .ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Signatures, opt => opt.MapFrom(src => src.Signatures))
            .ForMember(dest => dest.Approvals, opt => opt.MapFrom(src => src.Approvals));

        CreateMap<GenerateDocumentDto, GeneratedDocument>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DocumentNumber, opt => opt.Ignore())
            .ForMember(dest => dest.Title, opt => opt.Ignore())
            .ForMember(dest => dest.Content, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.FilePath, opt => opt.Ignore())
            .ForMember(dest => dest.FileHash, opt => opt.Ignore())
            .ForMember(dest => dest.GeneratedBy, opt => opt.Ignore())
            .ForMember(dest => dest.GeneratedAt, opt => opt.Ignore());

        // DocumentSignature mappings
        CreateMap<DocumentSignature, DocumentSignatureDto>()
            .ForMember(dest => dest.SignerName, opt => opt.MapFrom(src => $"{src.Signer.FirstName} {src.Signer.LastName}"));

        // DocumentApproval mappings
        CreateMap<DocumentApproval, DocumentApprovalDto>()
            .ForMember(dest => dest.ApproverName, opt => opt.MapFrom(src => $"{src.Approver.FirstName} {src.Approver.LastName}"));

        // DocumentRetentionPolicy mappings
        CreateMap<DocumentRetentionPolicy, DocumentRetentionPolicyDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => $"{src.CreatedByEmployee.FirstName} {src.CreatedByEmployee.LastName}"))
            .ForMember(dest => dest.AffectedDocumentsCount, opt => opt.Ignore());

        CreateMap<CreateDocumentRetentionPolicyDto, DocumentRetentionPolicy>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<UpdateDocumentRetentionPolicyDto, DocumentRetentionPolicy>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.DocumentType, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

        // DocumentRetentionExecution mappings
        CreateMap<DocumentRetentionExecution, DocumentRetentionExecutionDto>()
            .ForMember(dest => dest.PolicyName, opt => opt.MapFrom(src => src.DocumentRetentionPolicy.Name))
            .ForMember(dest => dest.DocumentTitle, opt => opt.MapFrom(src => src.GeneratedDocument.Title))
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => $"{src.GeneratedDocument.Employee.FirstName} {src.GeneratedDocument.Employee.LastName}"))
            .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedByEmployee != null ? $"{src.ApprovedByEmployee.FirstName} {src.ApprovedByEmployee.LastName}" : null));
    }
}