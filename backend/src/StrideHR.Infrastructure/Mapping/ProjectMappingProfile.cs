using AutoMapper;
using StrideHR.Core.Entities;
using StrideHR.Core.Models.Project;

namespace StrideHR.Infrastructure.Mapping;

public class ProjectMappingProfile : Profile
{
    public ProjectMappingProfile()
    {
        // Project mappings
        CreateMap<CreateProjectDto, Project>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByEmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByEmployee, opt => opt.Ignore())
            .ForMember(dest => dest.Branch, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectAssignments, opt => opt.Ignore())
            .ForMember(dest => dest.Tasks, opt => opt.Ignore())
            .ForMember(dest => dest.DSRs, opt => opt.Ignore());

        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.CreatedByEmployeeName, opt => opt.MapFrom(src => 
                src.CreatedByEmployee != null ? $"{src.CreatedByEmployee.FirstName} {src.CreatedByEmployee.LastName}" : string.Empty))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => 
                src.Branch != null ? src.Branch.Name : string.Empty))
            .ForMember(dest => dest.TeamMembers, opt => opt.MapFrom(src => src.ProjectAssignments))
            .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.Tasks))
            .ForMember(dest => dest.Progress, opt => opt.Ignore());

        // Task mappings
        CreateMap<CreateTaskDto, ProjectTask>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedToEmployee, opt => opt.Ignore())
            .ForMember(dest => dest.TaskAssignments, opt => opt.Ignore())
            .ForMember(dest => dest.DSRs, opt => opt.Ignore());

        CreateMap<ProjectTask, ProjectTaskDto>()
            .ForMember(dest => dest.AssignedToEmployeeName, opt => opt.MapFrom(src => 
                src.AssignedToEmployee != null ? $"{src.AssignedToEmployee.FirstName} {src.AssignedToEmployee.LastName}" : string.Empty))
            .ForMember(dest => dest.Assignments, opt => opt.MapFrom(src => src.TaskAssignments))
            .ForMember(dest => dest.ActualHoursWorked, opt => opt.Ignore())
            .ForMember(dest => dest.IsOverdue, opt => opt.Ignore());

        // Assignment mappings
        CreateMap<ProjectAssignment, ProjectTeamMemberDto>()
            .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId))
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => 
                src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}" : string.Empty))
            .ForMember(dest => dest.EmployeeEmail, opt => opt.MapFrom(src => 
                src.Employee != null ? src.Employee.Email : string.Empty))
            .ForMember(dest => dest.ProfilePhoto, opt => opt.MapFrom(src => 
                src.Employee != null ? src.Employee.ProfilePhoto : null));

        CreateMap<TaskAssignment, TaskAssignmentDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => 
                src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}" : string.Empty));

        // DSR mappings
        CreateMap<DSR, DailyHoursDto>()
            .ForMember(dest => dest.TaskTitle, opt => opt.MapFrom(src => 
                src.Task != null ? src.Task.Title : string.Empty));

        // Project Alert mappings
        CreateMap<ProjectAlert, ProjectAlertDto>()
            .ForMember(dest => dest.AlertType, opt => opt.MapFrom(src => src.AlertType.ToString()))
            .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => src.Severity.ToString()));

        CreateMap<ProjectAlertDto, ProjectAlert>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Project, opt => opt.Ignore())
            .ForMember(dest => dest.ResolvedByEmployee, opt => opt.Ignore());
    }
}