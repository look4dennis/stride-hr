using StrideHR.Core.Entities;
using StrideHR.Core.Models.Grievance;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IGrievanceRepository : IRepository<Grievance>
{
    Task<Grievance?> GetByGrievanceNumberAsync(string grievanceNumber);
    Task<Grievance?> GetWithDetailsAsync(int id);
    Task<(List<Grievance> Grievances, int TotalCount)> SearchAsync(GrievanceSearchCriteria criteria);
    Task<List<Grievance>> GetBySubmitterIdAsync(int submitterId);
    Task<List<Grievance>> GetByAssignedToIdAsync(int assignedToId);
    Task<List<Grievance>> GetOverdueGrievancesAsync();
    Task<List<Grievance>> GetEscalatedGrievancesAsync();
    Task<List<Grievance>> GetGrievancesRequiringFollowUpAsync();
    Task<string> GenerateGrievanceNumberAsync();
    Task<GrievanceAnalyticsDto> GetAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<Grievance>> GetGrievancesByEscalationLevelAsync(int escalationLevel);
    Task<List<Grievance>> GetAnonymousGrievancesAsync();
}