using AutoMapper;
using StrideHR.Core.Entities;
using StrideHR.Core.Models.Asset;

namespace StrideHR.Infrastructure.Mapping;

public class AssetMappingProfile : Profile
{
    public AssetMappingProfile()
    {
        // Asset mappings
        CreateMap<Asset, AssetDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch.Name))
            .ForMember(dest => dest.AssignedToEmployee, opt => opt.MapFrom(src => 
                src.AssetAssignments.Where(aa => aa.IsActive).FirstOrDefault() != null && 
                src.AssetAssignments.Where(aa => aa.IsActive).FirstOrDefault()!.Employee != null
                    ? $"{src.AssetAssignments.Where(aa => aa.IsActive).FirstOrDefault()!.Employee!.FirstName} {src.AssetAssignments.Where(aa => aa.IsActive).FirstOrDefault()!.Employee!.LastName}"
                    : null))
            .ForMember(dest => dest.AssignedToProject, opt => opt.MapFrom(src => 
                src.AssetAssignments.Where(aa => aa.IsActive).FirstOrDefault() != null && 
                src.AssetAssignments.Where(aa => aa.IsActive).FirstOrDefault()!.Project != null
                    ? src.AssetAssignments.Where(aa => aa.IsActive).FirstOrDefault()!.Project!.Name
                    : null))
            .ForMember(dest => dest.AssignedDate, opt => opt.MapFrom(src => 
                src.AssetAssignments.Where(aa => aa.IsActive).FirstOrDefault() != null
                    ? src.AssetAssignments.Where(aa => aa.IsActive).FirstOrDefault()!.AssignedDate
                    : (DateTime?)null));

        CreateMap<CreateAssetDto, Asset>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Core.Enums.AssetStatus.Available))
            .ForMember(dest => dest.CurrentValue, opt => opt.MapFrom(src => src.PurchasePrice));

        CreateMap<UpdateAssetDto, Asset>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AssetTag, opt => opt.Ignore())
            .ForMember(dest => dest.BranchId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore());

        // Asset Assignment mappings
        CreateMap<AssetAssignment, AssetAssignmentDto>()
            .ForMember(dest => dest.AssetName, opt => opt.MapFrom(src => src.Asset.Name))
            .ForMember(dest => dest.AssetTag, opt => opt.MapFrom(src => src.Asset.AssetTag))
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => 
                src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}" : null))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : null))
            .ForMember(dest => dest.AssignedByName, opt => opt.MapFrom(src => 
                $"{src.AssignedByEmployee.FirstName} {src.AssignedByEmployee.LastName}"))
            .ForMember(dest => dest.ReturnedByName, opt => opt.MapFrom(src => 
                src.ReturnedByEmployee != null ? $"{src.ReturnedByEmployee.FirstName} {src.ReturnedByEmployee.LastName}" : null));

        CreateMap<CreateAssetAssignmentDto, AssetAssignment>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        // Asset Maintenance mappings
        CreateMap<AssetMaintenance, AssetMaintenanceDto>()
            .ForMember(dest => dest.AssetName, opt => opt.MapFrom(src => src.Asset.Name))
            .ForMember(dest => dest.AssetTag, opt => opt.MapFrom(src => src.Asset.AssetTag))
            .ForMember(dest => dest.TechnicianName, opt => opt.MapFrom(src => 
                src.Technician != null ? $"{src.Technician.FirstName} {src.Technician.LastName}" : null))
            .ForMember(dest => dest.RequestedByName, opt => opt.MapFrom(src => 
                $"{src.RequestedByEmployee.FirstName} {src.RequestedByEmployee.LastName}"));

        CreateMap<CreateAssetMaintenanceDto, AssetMaintenance>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Core.Enums.MaintenanceStatus.Scheduled));

        CreateMap<UpdateAssetMaintenanceDto, AssetMaintenance>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AssetId, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.ScheduledDate, opt => opt.Ignore())
            .ForMember(dest => dest.Description, opt => opt.Ignore())
            .ForMember(dest => dest.RequestedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore());

        // Asset Handover mappings
        CreateMap<AssetHandover, AssetHandoverDto>()
            .ForMember(dest => dest.AssetName, opt => opt.MapFrom(src => src.Asset.Name))
            .ForMember(dest => dest.AssetTag, opt => opt.MapFrom(src => src.Asset.AssetTag))
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => 
                $"{src.Employee.FirstName} {src.Employee.LastName}"))
            .ForMember(dest => dest.InitiatedByName, opt => opt.MapFrom(src => 
                $"{src.InitiatedByEmployee.FirstName} {src.InitiatedByEmployee.LastName}"))
            .ForMember(dest => dest.CompletedByName, opt => opt.MapFrom(src => 
                src.CompletedByEmployee != null ? $"{src.CompletedByEmployee.FirstName} {src.CompletedByEmployee.LastName}" : null))
            .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => 
                src.ApprovedByEmployee != null ? $"{src.ApprovedByEmployee.FirstName} {src.ApprovedByEmployee.LastName}" : null));

        CreateMap<CreateAssetHandoverDto, AssetHandover>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Core.Enums.HandoverStatus.Pending));

        CreateMap<CompleteAssetHandoverDto, AssetHandover>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AssetId, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeExitId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Core.Enums.HandoverStatus.Completed))
            .ForMember(dest => dest.InitiatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.DueDate, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.InitiatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore());
    }
}