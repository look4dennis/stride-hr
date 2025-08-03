using StrideHR.Core.Entities;

namespace StrideHR.Core.Interfaces.Repositories;

public interface IGrievanceCommentRepository : IRepository<GrievanceComment>
{
    Task<List<GrievanceComment>> GetByGrievanceIdAsync(int grievanceId, bool includeInternal = false);
    Task<int> GetCommentsCountAsync(int grievanceId);
}