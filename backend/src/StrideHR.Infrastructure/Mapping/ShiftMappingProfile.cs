using AutoMapper;
using StrideHR.Core.Entities;
using StrideHR.Core.Models.Shift;
using System.Text.Json;

namespace StrideHR.Infrastructure.Mapping;

public class ShiftMappingProfile : Profile
{
    public ShiftMappingProfile()
    {
        // Shift mappings
        CreateMap<Shift, ShiftDto>()
            .ForMember(dest => dest.WorkingDays, opt => opt.MapFrom<WorkingDaysDeserializer>())
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null))
            .ForMember(dest => dest.AssignedEmployeesCount, opt => opt.MapFrom(src => src.ShiftAssignments.Count(sa => sa.IsActive)));

        CreateMap<CreateShiftDto, Shift>()
            .ForMember(dest => dest.WorkingDays, opt => opt.MapFrom<WorkingDaysSerializer>())
            .ForMember(dest => dest.WorkingHours, opt => opt.MapFrom<WorkingHoursResolver>())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<UpdateShiftDto, Shift>()
            .ForMember(dest => dest.WorkingDays, opt => opt.MapFrom<WorkingDaysSerializer>())
            .ForMember(dest => dest.WorkingHours, opt => opt.MapFrom<WorkingHoursResolver>())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Shift, ShiftTemplateDto>()
            .ForMember(dest => dest.WorkingDays, opt => opt.MapFrom<WorkingDaysDeserializer>());

        // ShiftAssignment mappings
        CreateMap<ShiftAssignment, ShiftAssignmentDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => 
                src.Employee != null ? $"{src.Employee.FirstName} {src.Employee.LastName}" : string.Empty))
            .ForMember(dest => dest.EmployeeId_Display, opt => opt.MapFrom(src => 
                src.Employee != null ? src.Employee.EmployeeId : string.Empty))
            .ForMember(dest => dest.ShiftName, opt => opt.MapFrom(src => 
                src.Shift != null ? src.Shift.Name : string.Empty))
            .ForMember(dest => dest.ShiftStartTime, opt => opt.MapFrom(src => 
                src.Shift != null ? src.Shift.StartTime : TimeSpan.Zero))
            .ForMember(dest => dest.ShiftEndTime, opt => opt.MapFrom(src => 
                src.Shift != null ? src.Shift.EndTime : TimeSpan.Zero));

        CreateMap<CreateShiftAssignmentDto, ShiftAssignment>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<UpdateShiftAssignmentDto, ShiftAssignment>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<BulkShiftAssignmentDto, CreateShiftAssignmentDto>()
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore()); // Will be set individually for each employee

        // Shift Swap mappings
        CreateMap<CreateShiftSwapRequestDto, ShiftSwapRequest>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<ShiftSwapRequest, ShiftSwapRequestDto>()
            .ForMember(dest => dest.RequesterName, opt => opt.MapFrom(src => 
                src.Requester != null ? $"{src.Requester.FirstName} {src.Requester.LastName}" : string.Empty))
            .ForMember(dest => dest.RequesterEmployeeId, opt => opt.MapFrom(src => 
                src.Requester != null ? src.Requester.EmployeeId : string.Empty))
            .ForMember(dest => dest.RequesterShiftName, opt => opt.MapFrom(src => 
                src.RequesterShiftAssignment != null && src.RequesterShiftAssignment.Shift != null ? src.RequesterShiftAssignment.Shift.Name : string.Empty))
            .ForMember(dest => dest.RequesterShiftDate, opt => opt.MapFrom(src => 
                src.RequesterShiftAssignment != null ? src.RequesterShiftAssignment.StartDate : DateTime.MinValue))
            .ForMember(dest => dest.RequesterShiftStartTime, opt => opt.MapFrom(src => 
                src.RequesterShiftAssignment != null && src.RequesterShiftAssignment.Shift != null ? src.RequesterShiftAssignment.Shift.StartTime : TimeSpan.Zero))
            .ForMember(dest => dest.RequesterShiftEndTime, opt => opt.MapFrom(src => 
                src.RequesterShiftAssignment != null && src.RequesterShiftAssignment.Shift != null ? src.RequesterShiftAssignment.Shift.EndTime : TimeSpan.Zero))
            .ForMember(dest => dest.TargetEmployeeName, opt => opt.MapFrom(src => 
                src.TargetEmployee != null ? $"{src.TargetEmployee.FirstName} {src.TargetEmployee.LastName}" : null))
            .ForMember(dest => dest.TargetEmployeeId_Display, opt => opt.MapFrom(src => 
                src.TargetEmployee != null ? src.TargetEmployee.EmployeeId : null))
            .ForMember(dest => dest.TargetShiftName, opt => opt.MapFrom(src => 
                src.TargetShiftAssignment != null && src.TargetShiftAssignment.Shift != null ? src.TargetShiftAssignment.Shift.Name : null))
            .ForMember(dest => dest.TargetShiftDate, opt => opt.MapFrom(src => 
                src.TargetShiftAssignment != null ? src.TargetShiftAssignment.StartDate : (DateTime?)null))
            .ForMember(dest => dest.TargetShiftStartTime, opt => opt.MapFrom(src => 
                src.TargetShiftAssignment != null && src.TargetShiftAssignment.Shift != null ? src.TargetShiftAssignment.Shift.StartTime : (TimeSpan?)null))
            .ForMember(dest => dest.TargetShiftEndTime, opt => opt.MapFrom(src => 
                src.TargetShiftAssignment != null && src.TargetShiftAssignment.Shift != null ? src.TargetShiftAssignment.Shift.EndTime : (TimeSpan?)null))
            .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => 
                src.ApprovedByEmployee != null ? $"{src.ApprovedByEmployee.FirstName} {src.ApprovedByEmployee.LastName}" : null));

        CreateMap<CreateShiftSwapResponseDto, ShiftSwapResponse>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<ShiftSwapResponse, ShiftSwapResponseDto>()
            .ForMember(dest => dest.ResponderName, opt => opt.MapFrom(src => 
                src.Responder != null ? $"{src.Responder.FirstName} {src.Responder.LastName}" : string.Empty))
            .ForMember(dest => dest.ResponderEmployeeId, opt => opt.MapFrom(src => 
                src.Responder != null ? src.Responder.EmployeeId : string.Empty))
            .ForMember(dest => dest.ResponderShiftName, opt => opt.MapFrom(src => 
                src.ResponderShiftAssignment != null && src.ResponderShiftAssignment.Shift != null ? src.ResponderShiftAssignment.Shift.Name : string.Empty))
            .ForMember(dest => dest.ResponderShiftDate, opt => opt.MapFrom(src => 
                src.ResponderShiftAssignment != null ? src.ResponderShiftAssignment.StartDate : DateTime.MinValue))
            .ForMember(dest => dest.ResponderShiftStartTime, opt => opt.MapFrom(src => 
                src.ResponderShiftAssignment != null && src.ResponderShiftAssignment.Shift != null ? src.ResponderShiftAssignment.Shift.StartTime : TimeSpan.Zero))
            .ForMember(dest => dest.ResponderShiftEndTime, opt => opt.MapFrom(src => 
                src.ResponderShiftAssignment != null && src.ResponderShiftAssignment.Shift != null ? src.ResponderShiftAssignment.Shift.EndTime : TimeSpan.Zero));

        // Shift Coverage mappings
        CreateMap<CreateShiftCoverageRequestDto, ShiftCoverageRequest>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<ShiftCoverageRequest, ShiftCoverageRequestDto>()
            .ForMember(dest => dest.RequesterName, opt => opt.MapFrom(src => 
                src.Requester != null ? $"{src.Requester.FirstName} {src.Requester.LastName}" : string.Empty))
            .ForMember(dest => dest.RequesterEmployeeId, opt => opt.MapFrom(src => 
                src.Requester != null ? src.Requester.EmployeeId : string.Empty))
            .ForMember(dest => dest.ShiftName, opt => opt.MapFrom(src => 
                src.ShiftAssignment != null && src.ShiftAssignment.Shift != null ? src.ShiftAssignment.Shift.Name : string.Empty))
            .ForMember(dest => dest.ShiftStartTime, opt => opt.MapFrom(src => 
                src.ShiftAssignment != null && src.ShiftAssignment.Shift != null ? src.ShiftAssignment.Shift.StartTime : TimeSpan.Zero))
            .ForMember(dest => dest.ShiftEndTime, opt => opt.MapFrom(src => 
                src.ShiftAssignment != null && src.ShiftAssignment.Shift != null ? src.ShiftAssignment.Shift.EndTime : TimeSpan.Zero))
            .ForMember(dest => dest.AcceptedByName, opt => opt.MapFrom(src => 
                src.AcceptedByEmployee != null ? $"{src.AcceptedByEmployee.FirstName} {src.AcceptedByEmployee.LastName}" : null))
            .ForMember(dest => dest.AcceptedByEmployeeId, opt => opt.MapFrom(src => 
                src.AcceptedByEmployee != null ? src.AcceptedByEmployee.EmployeeId : null))
            .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => 
                src.ApprovedByEmployee != null ? $"{src.ApprovedByEmployee.FirstName} {src.ApprovedByEmployee.LastName}" : null));

        CreateMap<CreateShiftCoverageResponseDto, ShiftCoverageResponse>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<ShiftCoverageResponse, ShiftCoverageResponseDto>()
            .ForMember(dest => dest.ResponderName, opt => opt.MapFrom(src => 
                src.Responder != null ? $"{src.Responder.FirstName} {src.Responder.LastName}" : string.Empty))
            .ForMember(dest => dest.ResponderEmployeeId, opt => opt.MapFrom(src => 
                src.Responder != null ? src.Responder.EmployeeId : string.Empty));
    }

    private static TimeSpan CalculateWorkingHours(TimeSpan startTime, TimeSpan endTime, TimeSpan? breakDuration)
    {
        var totalTime = endTime - startTime;
        if (totalTime < TimeSpan.Zero)
        {
            // Handle overnight shifts
            totalTime = totalTime.Add(TimeSpan.FromDays(1));
        }

        if (breakDuration.HasValue)
        {
            totalTime = totalTime.Subtract(breakDuration.Value);
        }

        return totalTime;
    }
}

public class WorkingHoursResolver : IValueResolver<CreateShiftDto, Shift, TimeSpan>, IValueResolver<UpdateShiftDto, Shift, TimeSpan>
{
    public TimeSpan Resolve(CreateShiftDto source, Shift destination, TimeSpan destMember, ResolutionContext context)
    {
        return CalculateWorkingHours(source.StartTime, source.EndTime, source.BreakDuration);
    }

    public TimeSpan Resolve(UpdateShiftDto source, Shift destination, TimeSpan destMember, ResolutionContext context)
    {
        return CalculateWorkingHours(source.StartTime, source.EndTime, source.BreakDuration);
    }

    private static TimeSpan CalculateWorkingHours(TimeSpan startTime, TimeSpan endTime, TimeSpan? breakDuration)
    {
        var totalTime = endTime - startTime;
        if (totalTime < TimeSpan.Zero)
        {
            // Handle overnight shifts
            totalTime = totalTime.Add(TimeSpan.FromDays(1));
        }

        if (breakDuration.HasValue)
        {
            totalTime = totalTime.Subtract(breakDuration.Value);
        }

        return totalTime;
    }

}

public class WorkingDaysSerializer : IValueResolver<CreateShiftDto, Shift, string>, IValueResolver<UpdateShiftDto, Shift, string>
{
    public string Resolve(CreateShiftDto source, Shift destination, string destMember, ResolutionContext context)
    {
        return JsonSerializer.Serialize(source.WorkingDays);
    }

    public string Resolve(UpdateShiftDto source, Shift destination, string destMember, ResolutionContext context)
    {
        return JsonSerializer.Serialize(source.WorkingDays);
    }
}

public class WorkingDaysDeserializer : IValueResolver<Shift, ShiftDto, List<int>>, IValueResolver<Shift, ShiftTemplateDto, List<int>>
{
    public List<int> Resolve(Shift source, ShiftDto destination, List<int> destMember, ResolutionContext context)
    {
        return DeserializeWorkingDays(source.WorkingDays);
    }

    public List<int> Resolve(Shift source, ShiftTemplateDto destination, List<int> destMember, ResolutionContext context)
    {
        return DeserializeWorkingDays(source.WorkingDays);
    }

    private static List<int> DeserializeWorkingDays(string? workingDaysJson)
    {
        if (string.IsNullOrEmpty(workingDaysJson))
            return new List<int>();

        try
        {
            return JsonSerializer.Deserialize<List<int>>(workingDaysJson) ?? new List<int>();
        }
        catch
        {
            return new List<int>();
        }
    }
}