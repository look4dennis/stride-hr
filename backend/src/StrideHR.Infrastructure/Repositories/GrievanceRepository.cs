using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.Grievance;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class GrievanceRepository : Repository<Grievance>, IGrievanceRepository
{
    public GrievanceRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<Grievance?> GetByGrievanceNumberAsync(string grievanceNumber)
    {
        return await _context.Grievances
            .Include(g => g.SubmittedBy)
            .Include(g => g.AssignedTo)
            .Include(g => g.ResolvedBy)
            .Include(g => g.EscalatedBy)
            .FirstOrDefaultAsync(g => g.GrievanceNumber == grievanceNumber && !g.IsDeleted);
    }

    public async Task<Grievance?> GetWithDetailsAsync(int id)
    {
        return await _context.Grievances
            .Include(g => g.SubmittedBy)
            .Include(g => g.AssignedTo)
            .Include(g => g.ResolvedBy)
            .Include(g => g.EscalatedBy)
            .Include(g => g.Comments.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Author)
            .Include(g => g.StatusHistory.Where(sh => !sh.IsDeleted))
                .ThenInclude(sh => sh.ChangedBy)
            .Include(g => g.Escalations.Where(e => !e.IsDeleted))
                .ThenInclude(e => e.EscalatedBy)
            .Include(g => g.FollowUps.Where(f => !f.IsDeleted))
                .ThenInclude(f => f.ScheduledBy)
            .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);
    }

    public async Task<(List<Grievance> Grievances, int TotalCount)> SearchAsync(GrievanceSearchCriteria criteria)
    {
        var query = _context.Grievances
            .Include(g => g.SubmittedBy)
            .Include(g => g.AssignedTo)
            .Include(g => g.ResolvedBy)
            .Where(g => !g.IsDeleted);

        // Apply filters
        if (!string.IsNullOrEmpty(criteria.SearchTerm))
        {
            query = query.Where(g => g.Title.Contains(criteria.SearchTerm) ||
                                   g.Description.Contains(criteria.SearchTerm) ||
                                   g.GrievanceNumber.Contains(criteria.SearchTerm));
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(g => g.Status == criteria.Status.Value);
        }

        if (criteria.Category.HasValue)
        {
            query = query.Where(g => g.Category == criteria.Category.Value);
        }

        if (criteria.Priority.HasValue)
        {
            query = query.Where(g => g.Priority == criteria.Priority.Value);
        }

        if (criteria.EscalationLevel.HasValue)
        {
            query = query.Where(g => g.CurrentEscalationLevel == criteria.EscalationLevel.Value);
        }

        if (criteria.SubmittedById.HasValue)
        {
            query = query.Where(g => g.SubmittedById == criteria.SubmittedById.Value);
        }

        if (criteria.AssignedToId.HasValue)
        {
            query = query.Where(g => g.AssignedToId == criteria.AssignedToId.Value);
        }

        if (criteria.IsAnonymous.HasValue)
        {
            query = query.Where(g => g.IsAnonymous == criteria.IsAnonymous.Value);
        }

        if (criteria.IsEscalated.HasValue)
        {
            query = query.Where(g => g.IsEscalated == criteria.IsEscalated.Value);
        }

        if (criteria.RequiresInvestigation.HasValue)
        {
            query = query.Where(g => g.RequiresInvestigation == criteria.RequiresInvestigation.Value);
        }

        if (criteria.IsOverdue.HasValue && criteria.IsOverdue.Value)
        {
            query = query.Where(g => g.DueDate.HasValue && g.DueDate.Value < DateTime.UtcNow && 
                                   g.Status != GrievanceStatus.Resolved && g.Status != GrievanceStatus.Closed);
        }

        if (criteria.CreatedFrom.HasValue)
        {
            query = query.Where(g => g.CreatedAt >= criteria.CreatedFrom.Value);
        }

        if (criteria.CreatedTo.HasValue)
        {
            query = query.Where(g => g.CreatedAt <= criteria.CreatedTo.Value);
        }

        if (criteria.DueDateFrom.HasValue)
        {
            query = query.Where(g => g.DueDate >= criteria.DueDateFrom.Value);
        }

        if (criteria.DueDateTo.HasValue)
        {
            query = query.Where(g => g.DueDate <= criteria.DueDateTo.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = criteria.SortBy.ToLower() switch
        {
            "title" => criteria.SortDescending ? query.OrderByDescending(g => g.Title) : query.OrderBy(g => g.Title),
            "status" => criteria.SortDescending ? query.OrderByDescending(g => g.Status) : query.OrderBy(g => g.Status),
            "priority" => criteria.SortDescending ? query.OrderByDescending(g => g.Priority) : query.OrderBy(g => g.Priority),
            "category" => criteria.SortDescending ? query.OrderByDescending(g => g.Category) : query.OrderBy(g => g.Category),
            "duedate" => criteria.SortDescending ? query.OrderByDescending(g => g.DueDate) : query.OrderBy(g => g.DueDate),
            _ => criteria.SortDescending ? query.OrderByDescending(g => g.CreatedAt) : query.OrderBy(g => g.CreatedAt)
        };

        // Apply pagination
        var grievances = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync();

        return (grievances, totalCount);
    }

    public async Task<List<Grievance>> GetBySubmitterIdAsync(int submitterId)
    {
        return await _context.Grievances
            .Include(g => g.AssignedTo)
            .Include(g => g.ResolvedBy)
            .Where(g => g.SubmittedById == submitterId && !g.IsDeleted)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Grievance>> GetByAssignedToIdAsync(int assignedToId)
    {
        return await _context.Grievances
            .Include(g => g.SubmittedBy)
            .Include(g => g.ResolvedBy)
            .Where(g => g.AssignedToId == assignedToId && !g.IsDeleted)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Grievance>> GetOverdueGrievancesAsync()
    {
        return await _context.Grievances
            .Include(g => g.SubmittedBy)
            .Include(g => g.AssignedTo)
            .Where(g => g.DueDate.HasValue && 
                       g.DueDate.Value < DateTime.UtcNow && 
                       g.Status != GrievanceStatus.Resolved && 
                       g.Status != GrievanceStatus.Closed && 
                       !g.IsDeleted)
            .OrderBy(g => g.DueDate)
            .ToListAsync();
    }

    public async Task<List<Grievance>> GetEscalatedGrievancesAsync()
    {
        return await _context.Grievances
            .Include(g => g.SubmittedBy)
            .Include(g => g.AssignedTo)
            .Include(g => g.EscalatedBy)
            .Where(g => g.IsEscalated && !g.IsDeleted)
            .OrderByDescending(g => g.EscalatedAt)
            .ToListAsync();
    }

    public async Task<List<Grievance>> GetGrievancesRequiringFollowUpAsync()
    {
        return await _context.Grievances
            .Include(g => g.SubmittedBy)
            .Include(g => g.AssignedTo)
            .Include(g => g.FollowUps)
            .Where(g => g.Status == GrievanceStatus.Resolved && 
                       g.FollowUps.Any(f => !f.IsCompleted && !f.IsDeleted) && 
                       !g.IsDeleted)
            .OrderByDescending(g => g.ResolvedAt)
            .ToListAsync();
    }

    public async Task<string> GenerateGrievanceNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        
        var lastGrievance = await _context.Grievances
            .Where(g => g.GrievanceNumber.StartsWith($"GRV-{year:D4}-{month:D2}"))
            .OrderByDescending(g => g.GrievanceNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastGrievance != null)
        {
            var lastNumberPart = lastGrievance.GrievanceNumber.Split('-').Last();
            if (int.TryParse(lastNumberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"GRV-{year:D4}-{month:D2}-{nextNumber:D4}";
    }

    public async Task<GrievanceAnalyticsDto> GetAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Grievances.Where(g => !g.IsDeleted);

        if (fromDate.HasValue)
            query = query.Where(g => g.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(g => g.CreatedAt <= toDate.Value);

        var grievances = await query.ToListAsync();

        var analytics = new GrievanceAnalyticsDto
        {
            TotalGrievances = grievances.Count,
            OpenGrievances = grievances.Count(g => g.Status == GrievanceStatus.Submitted || 
                                                  g.Status == GrievanceStatus.UnderReview || 
                                                  g.Status == GrievanceStatus.InvestigationInProgress),
            ResolvedGrievances = grievances.Count(g => g.Status == GrievanceStatus.Resolved),
            ClosedGrievances = grievances.Count(g => g.Status == GrievanceStatus.Closed),
            EscalatedGrievances = grievances.Count(g => g.IsEscalated),
            OverdueGrievances = grievances.Count(g => g.DueDate.HasValue && g.DueDate.Value < DateTime.UtcNow && 
                                                     g.Status != GrievanceStatus.Resolved && g.Status != GrievanceStatus.Closed),
            AnonymousGrievances = grievances.Count(g => g.IsAnonymous)
        };

        var resolvedGrievances = grievances.Where(g => g.ResolvedAt.HasValue).ToList();
        if (resolvedGrievances.Any())
        {
            analytics.AverageResolutionTimeHours = resolvedGrievances
                .Average(g => (g.ResolvedAt!.Value - g.CreatedAt).TotalHours);
        }

        var ratedGrievances = grievances.Where(g => g.SatisfactionRating.HasValue).ToList();
        if (ratedGrievances.Any())
        {
            analytics.SatisfactionRating = ratedGrievances.Average(g => g.SatisfactionRating!.Value);
        }

        // Category stats
        analytics.CategoryStats = grievances
            .GroupBy(g => g.Category)
            .Select(group => new GrievanceCategoryStats
            {
                Category = group.Key,
                CategoryName = group.Key.ToString(),
                Count = group.Count(),
                Percentage = (double)group.Count() / grievances.Count * 100,
                AverageResolutionTimeHours = group.Where(g => g.ResolvedAt.HasValue)
                    .DefaultIfEmpty()
                    .Average(g => g?.ResolvedAt.HasValue == true ? (g.ResolvedAt.Value - g.CreatedAt).TotalHours : 0)
            })
            .ToList();

        // Priority stats
        analytics.PriorityStats = grievances
            .GroupBy(g => g.Priority)
            .Select(group => new GrievancePriorityStats
            {
                Priority = group.Key,
                PriorityName = group.Key.ToString(),
                Count = group.Count(),
                Percentage = (double)group.Count() / grievances.Count * 100,
                AverageResolutionTimeHours = group.Where(g => g.ResolvedAt.HasValue)
                    .DefaultIfEmpty()
                    .Average(g => g?.ResolvedAt.HasValue == true ? (g.ResolvedAt.Value - g.CreatedAt).TotalHours : 0)
            })
            .ToList();

        // Status stats
        analytics.StatusStats = grievances
            .GroupBy(g => g.Status)
            .Select(group => new GrievanceStatusStats
            {
                Status = group.Key,
                StatusName = group.Key.ToString(),
                Count = group.Count(),
                Percentage = (double)group.Count() / grievances.Count * 100
            })
            .ToList();

        // Escalation stats
        analytics.EscalationStats = grievances
            .GroupBy(g => g.CurrentEscalationLevel)
            .Select(group => new GrievanceEscalationStats
            {
                Level = group.Key,
                LevelName = group.Key.ToString(),
                Count = group.Count(),
                Percentage = (double)group.Count() / grievances.Count * 100
            })
            .ToList();

        return analytics;
    }

    public async Task<List<Grievance>> GetGrievancesByEscalationLevelAsync(int escalationLevel)
    {
        return await _context.Grievances
            .Include(g => g.SubmittedBy)
            .Include(g => g.AssignedTo)
            .Where(g => (int)g.CurrentEscalationLevel == escalationLevel && !g.IsDeleted)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Grievance>> GetAnonymousGrievancesAsync()
    {
        return await _context.Grievances
            .Include(g => g.AssignedTo)
            .Include(g => g.ResolvedBy)
            .Where(g => g.IsAnonymous && !g.IsDeleted)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }
}