using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IGrievanceFollowUpRepository : IRepository<GrievanceFollowUp>
{
    Task<List<GrievanceFollowUp>> GetByGrievanceIdAsync(int grievanceId);
    Task<List<GrievanceFollowUp>> GetPendingFollowUpsAsync();
    Task<List<GrievanceFollowUp>> GetOverdueFollowUpsAsync();
    Task<int> GetFollowUpsCountAsync(int grievanceId);
}