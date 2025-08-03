using AutoMapper;
using StrideHR.Core.Entities;
using StrideHR.Core.Models.Expense;

namespace StrideHR.Infrastructure.Mapping;

public class ExpenseMappingProfile : Profile
{
    public ExpenseMappingProfile()
    {
        // ExpenseClaim mappings
        CreateMap<ExpenseClaim, ExpenseClaimDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => $"{src.Employee.FirstName} {src.Employee.LastName}"))
            .ForMember(dest => dest.StatusText, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedByEmployee != null ? $"{src.ApprovedByEmployee.FirstName} {src.ApprovedByEmployee.LastName}" : null));

        CreateMap<CreateExpenseClaimDto, ExpenseClaim>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ClaimNumber, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.SubmissionDate, opt => opt.Ignore())
            .ForMember(dest => dest.ExpenseItems, opt => opt.Ignore());

        CreateMap<ExpenseClaim, CreateExpenseClaimDto>()
            .ForMember(dest => dest.ExpenseItems, opt => opt.MapFrom(src => src.ExpenseItems));

        // ExpenseItem mappings
        CreateMap<ExpenseItem, ExpenseItemDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.ExpenseCategory.Name))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : null));

        CreateMap<CreateExpenseItemDto, ExpenseItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ExpenseClaimId, opt => opt.Ignore());

        CreateMap<ExpenseItem, CreateExpenseItemDto>();

        CreateMap<UpdateExpenseItemDto, ExpenseItem>()
            .ForMember(dest => dest.ExpenseClaimId, opt => opt.Ignore());

        // ExpenseDocument mappings
        CreateMap<ExpenseDocument, ExpenseDocumentDto>()
            .ForMember(dest => dest.UploadedByName, opt => opt.MapFrom(src => $"{src.UploadedByEmployee.FirstName} {src.UploadedByEmployee.LastName}"));

        // ExpenseApprovalHistory mappings
        CreateMap<ExpenseApprovalHistory, ExpenseApprovalHistoryDto>()
            .ForMember(dest => dest.ApproverName, opt => opt.MapFrom(src => $"{src.Approver.FirstName} {src.Approver.LastName}"))
            .ForMember(dest => dest.ApprovalLevelText, opt => opt.MapFrom(src => src.ApprovalLevel.ToString()))
            .ForMember(dest => dest.ActionText, opt => opt.MapFrom(src => src.Action.ToString()));

        // ExpenseCategory mappings
        CreateMap<ExpenseCategory, ExpenseCategoryDto>();
        CreateMap<CreateExpenseCategoryDto, ExpenseCategory>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());
    }
}