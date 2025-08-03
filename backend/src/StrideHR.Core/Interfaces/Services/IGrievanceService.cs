using StrideHR.Core.Models.Grievance;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Interfaces.Services;

public interface IGrievanceService
{
    Task<GrievanceDto> CreateGrievanceAsync(CreateGrievanceDto dto, int submitterId);
    Task<GrievanceDto> GetGrievanceByIdAsync(int id);
    Task<GrievanceDto?> GetGrievanceByNumberAsync(string grievanceNumber);
    Task<(List<GrievanceDto> Grievances, int TotalCount)> SearchGrievancesAsync(GrievanceSearchCriteria criteria);
    Task<GrievanceDto> UpdateGrievanceAsync(int id, UpdateGrievanceDto dto, int updatedById);
    Task<GrievanceDto> AssignGrievanceAsync(int id, int assignedToId, int assignedById);
    Task<GrievanceDto> UpdateStatusAsync(int id, GrievanceStatus status, int updatedById, string? reason = null);
    Task<GrievanceDto> ResolveGrievanceAsync(int id, string resolution, string? resolutionNotes, int resolvedById);
    Task<GrievanceDto> CloseGrievanceAsync(int id, int closedById, int? satisfactionRating = null, string? feedbackComments = null);
    Task<GrievanceDto> EscalateGrievanceAsync(int id, EscalationLevel toLevel, string reason, int escalatedById, int? escalatedToId = null);
    Task<GrievanceDto> WithdrawGrievanceAsync(int id, string reason, int withdrawnById);
    Task<List<GrievanceDto>> GetMyGrievancesAsync(int employeeId);
    Task<List<GrievanceDto>> GetAssignedGrievancesAsync(int employeeId);
    Task<List<GrievanceDto>> GetOverdueGrievancesAsync();
    Task<List<GrievanceDto>> GetEscalatedGrievancesAsync();
    Task<List<GrievanceDto>> GetAnonymousGrievancesAsync();
    Task<GrievanceCommentDto> AddCommentAsync(int grievanceId, CreateGrievanceCommentDto dto, int authorId);
    Task<List<GrievanceCommentDto>> GetGrievanceCommentsAsync(int grievanceId, bool includeInternal = false);
    Task<GrievanceFollowUpDto> ScheduleFollowUpAsync(int grievanceId, CreateGrievanceFollowUpDto dto, int scheduledById);
    Task<GrievanceFollowUpDto> CompleteFollowUpAsync(int followUpId, CompleteGrievanceFollowUpDto dto, int completedById);
    Task<List<GrievanceFollowUpDto>> GetGrievanceFollowUpsAsync(int grievanceId);
    Task<List<GrievanceFollowUpDto>> GetPendingFollowUpsAsync();
    Task<GrievanceAnalyticsDto> GetAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task DeleteGrievanceAsync(int id);
}