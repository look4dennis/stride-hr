using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.SupportTicket;

public class SupportTicketAnalyticsDto
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public double FirstResponseTimeHours { get; set; }
    public double CustomerSatisfactionScore { get; set; }
    public List<CategoryAnalyticsDto> CategoryBreakdown { get; set; } = new();
    public List<PriorityAnalyticsDto> PriorityBreakdown { get; set; } = new();
    public List<AgentPerformanceDto> AgentPerformance { get; set; } = new();
    public List<MonthlyTrendDto> MonthlyTrends { get; set; } = new();
}

public class CategoryAnalyticsDto
{
    public SupportTicketCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double AverageResolutionTimeHours { get; set; }
}

public class PriorityAnalyticsDto
{
    public SupportTicketPriority Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double AverageResolutionTimeHours { get; set; }
}

public class AgentPerformanceDto
{
    public int AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public int AssignedTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public double CustomerSatisfactionScore { get; set; }
}

public class MonthlyTrendDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int TotalTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public double AverageResolutionTimeHours { get; set; }
}