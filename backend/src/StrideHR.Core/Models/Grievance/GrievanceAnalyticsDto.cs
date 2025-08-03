using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Grievance;

public class GrievanceAnalyticsDto
{
    public int TotalGrievances { get; set; }
    public int OpenGrievances { get; set; }
    public int ResolvedGrievances { get; set; }
    public int ClosedGrievances { get; set; }
    public int EscalatedGrievances { get; set; }
    public int OverdueGrievances { get; set; }
    public int AnonymousGrievances { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public double SatisfactionRating { get; set; }
    public List<GrievanceCategoryStats> CategoryStats { get; set; } = new();
    public List<GrievancePriorityStats> PriorityStats { get; set; } = new();
    public List<GrievanceStatusStats> StatusStats { get; set; } = new();
    public List<GrievanceEscalationStats> EscalationStats { get; set; } = new();
    public List<GrievanceTrendData> TrendData { get; set; } = new();
}

public class GrievanceCategoryStats
{
    public GrievanceCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
    public double AverageResolutionTimeHours { get; set; }
}

public class GrievancePriorityStats
{
    public GrievancePriority Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
    public double AverageResolutionTimeHours { get; set; }
}

public class GrievanceStatusStats
{
    public GrievanceStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class GrievanceEscalationStats
{
    public EscalationLevel Level { get; set; }
    public string LevelName { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class GrievanceTrendData
{
    public DateTime Date { get; set; }
    public int Submitted { get; set; }
    public int Resolved { get; set; }
    public int Escalated { get; set; }
}