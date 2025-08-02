using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.SupportTicket;
using StrideHR.Infrastructure.Data;
using System.Globalization;

namespace StrideHR.Infrastructure.Repositories;

public class SupportTicketRepository : Repository<SupportTicket>, ISupportTicketRepository
{
    public SupportTicketRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<SupportTicket?> GetByTicketNumberAsync(string ticketNumber)
    {
        return await _context.SupportTickets
            .Include(t => t.Requester)
            .Include(t => t.AssignedTo)
            .Include(t => t.Asset)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .Include(t => t.StatusHistory)
                .ThenInclude(h => h.ChangedBy)
            .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber);
    }

    public async Task<SupportTicket?> GetWithDetailsAsync(int id)
    {
        return await _context.SupportTickets
            .Include(t => t.Requester)
            .Include(t => t.AssignedTo)
            .Include(t => t.Asset)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .Include(t => t.StatusHistory)
                .ThenInclude(h => h.ChangedBy)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<(List<SupportTicket> Tickets, int TotalCount)> SearchAsync(SupportTicketSearchCriteria criteria)
    {
        var query = _context.SupportTickets
            .Include(t => t.Requester)
            .Include(t => t.AssignedTo)
            .Include(t => t.Asset)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(criteria.SearchTerm))
        {
            query = query.Where(t => t.Title.Contains(criteria.SearchTerm) ||
                                   t.Description.Contains(criteria.SearchTerm) ||
                                   t.TicketNumber.Contains(criteria.SearchTerm));
        }

        if (criteria.Status.HasValue)
        {
            query = query.Where(t => t.Status == criteria.Status.Value);
        }

        if (criteria.Category.HasValue)
        {
            query = query.Where(t => t.Category == criteria.Category.Value);
        }

        if (criteria.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == criteria.Priority.Value);
        }

        if (criteria.RequesterId.HasValue)
        {
            query = query.Where(t => t.RequesterId == criteria.RequesterId.Value);
        }

        if (criteria.AssignedToId.HasValue)
        {
            query = query.Where(t => t.AssignedToId == criteria.AssignedToId.Value);
        }

        if (criteria.CreatedFrom.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= criteria.CreatedFrom.Value);
        }

        if (criteria.CreatedTo.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= criteria.CreatedTo.Value);
        }

        if (criteria.ResolvedFrom.HasValue)
        {
            query = query.Where(t => t.ResolvedAt >= criteria.ResolvedFrom.Value);
        }

        if (criteria.ResolvedTo.HasValue)
        {
            query = query.Where(t => t.ResolvedAt <= criteria.ResolvedTo.Value);
        }

        if (criteria.RequiresRemoteAccess.HasValue)
        {
            query = query.Where(t => t.RequiresRemoteAccess == criteria.RequiresRemoteAccess.Value);
        }

        if (criteria.AssetId.HasValue)
        {
            query = query.Where(t => t.AssetId == criteria.AssetId.Value);
        }

        var totalCount = await query.CountAsync();

        // Apply sorting
        query = criteria.SortBy.ToLower() switch
        {
            "title" => criteria.SortDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "status" => criteria.SortDescending ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "priority" => criteria.SortDescending ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "category" => criteria.SortDescending ? query.OrderByDescending(t => t.Category) : query.OrderBy(t => t.Category),
            "requester" => criteria.SortDescending ? query.OrderByDescending(t => t.Requester.FirstName) : query.OrderBy(t => t.Requester.FirstName),
            "assignedto" => criteria.SortDescending ? query.OrderByDescending(t => t.AssignedTo!.FirstName) : query.OrderBy(t => t.AssignedTo!.FirstName),
            _ => criteria.SortDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };

        // Apply pagination
        var tickets = await query
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync();

        return (tickets, totalCount);
    }

    public async Task<List<SupportTicket>> GetByRequesterIdAsync(int requesterId)
    {
        return await _context.SupportTickets
            .Include(t => t.AssignedTo)
            .Include(t => t.Asset)
            .Where(t => t.RequesterId == requesterId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<SupportTicket>> GetByAssignedToIdAsync(int assignedToId)
    {
        return await _context.SupportTickets
            .Include(t => t.Requester)
            .Include(t => t.Asset)
            .Where(t => t.AssignedToId == assignedToId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<SupportTicket>> GetOverdueTicketsAsync()
    {
        var cutoffDate = DateTime.UtcNow.AddHours(-24); // Consider tickets older than 24 hours as overdue
        
        return await _context.SupportTickets
            .Include(t => t.Requester)
            .Include(t => t.AssignedTo)
            .Where(t => t.Status != SupportTicketStatus.Closed && 
                       t.Status != SupportTicketStatus.Resolved &&
                       t.Status != SupportTicketStatus.Cancelled &&
                       t.CreatedAt < cutoffDate)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<string> GenerateTicketNumberAsync()
    {
        var today = DateTime.Today;
        var prefix = $"ST{today:yyyyMMdd}";
        
        var lastTicket = await _context.SupportTickets
            .Where(t => t.TicketNumber.StartsWith(prefix))
            .OrderByDescending(t => t.TicketNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastTicket != null)
        {
            var lastNumberPart = lastTicket.TicketNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }

    public async Task<SupportTicketAnalyticsDto> GetAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        fromDate ??= DateTime.UtcNow.AddMonths(-12);
        toDate ??= DateTime.UtcNow;

        var tickets = await _context.SupportTickets
            .Include(t => t.AssignedTo)
            .Where(t => t.CreatedAt >= fromDate && t.CreatedAt <= toDate)
            .ToListAsync();

        var analytics = new SupportTicketAnalyticsDto
        {
            TotalTickets = tickets.Count,
            OpenTickets = tickets.Count(t => t.Status == SupportTicketStatus.Open),
            InProgressTickets = tickets.Count(t => t.Status == SupportTicketStatus.InProgress),
            ResolvedTickets = tickets.Count(t => t.Status == SupportTicketStatus.Resolved),
            ClosedTickets = tickets.Count(t => t.Status == SupportTicketStatus.Closed)
        };

        var resolvedTickets = tickets.Where(t => t.ResolvedAt.HasValue && t.ResolutionTime.HasValue).ToList();
        if (resolvedTickets.Any())
        {
            analytics.AverageResolutionTimeHours = resolvedTickets.Average(t => t.ResolutionTime!.Value.TotalHours);
        }

        var ratedTickets = tickets.Where(t => t.SatisfactionRating.HasValue).ToList();
        if (ratedTickets.Any())
        {
            analytics.CustomerSatisfactionScore = ratedTickets.Average(t => t.SatisfactionRating!.Value);
        }

        // Category breakdown
        analytics.CategoryBreakdown = tickets
            .GroupBy(t => t.Category)
            .Select(g => new CategoryAnalyticsDto
            {
                Category = g.Key,
                CategoryName = g.Key.ToString(),
                Count = g.Count(),
                AverageResolutionTimeHours = g.Where(t => t.ResolutionTime.HasValue)
                    .Select(t => t.ResolutionTime!.Value.TotalHours)
                    .DefaultIfEmpty(0)
                    .Average()
            })
            .ToList();

        // Priority breakdown
        analytics.PriorityBreakdown = tickets
            .GroupBy(t => t.Priority)
            .Select(g => new PriorityAnalyticsDto
            {
                Priority = g.Key,
                PriorityName = g.Key.ToString(),
                Count = g.Count(),
                AverageResolutionTimeHours = g.Where(t => t.ResolutionTime.HasValue)
                    .Select(t => t.ResolutionTime!.Value.TotalHours)
                    .DefaultIfEmpty(0)
                    .Average()
            })
            .ToList();

        // Agent performance
        analytics.AgentPerformance = tickets
            .Where(t => t.AssignedToId.HasValue)
            .GroupBy(t => new { t.AssignedToId, AgentName = $"{t.AssignedTo!.FirstName} {t.AssignedTo.LastName}" })
            .Select(g => new AgentPerformanceDto
            {
                AgentId = g.Key.AssignedToId!.Value,
                AgentName = g.Key.AgentName,
                AssignedTickets = g.Count(),
                ResolvedTickets = g.Count(t => t.Status == SupportTicketStatus.Resolved || t.Status == SupportTicketStatus.Closed),
                AverageResolutionTimeHours = g.Where(t => t.ResolutionTime.HasValue)
                    .Select(t => t.ResolutionTime!.Value.TotalHours)
                    .DefaultIfEmpty(0)
                    .Average(),
                CustomerSatisfactionScore = g.Where(t => t.SatisfactionRating.HasValue)
                    .Select(t => t.SatisfactionRating!.Value)
                    .DefaultIfEmpty(0)
                    .Average()
            })
            .ToList();

        // Monthly trends
        analytics.MonthlyTrends = tickets
            .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
            .Select(g => new MonthlyTrendDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                TotalTickets = g.Count(),
                ResolvedTickets = g.Count(t => t.Status == SupportTicketStatus.Resolved || t.Status == SupportTicketStatus.Closed),
                AverageResolutionTimeHours = g.Where(t => t.ResolutionTime.HasValue)
                    .Select(t => t.ResolutionTime!.Value.TotalHours)
                    .DefaultIfEmpty(0)
                    .Average()
            })
            .OrderBy(t => t.Year)
            .ThenBy(t => t.Month)
            .ToList();

        return analytics;
    }
}