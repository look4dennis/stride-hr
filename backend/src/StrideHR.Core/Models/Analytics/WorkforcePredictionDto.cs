namespace StrideHR.Core.Models.Analytics;

public class WorkforcePredictionRequest
{
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int ForecastMonths { get; set; } = 12;
    public List<string>? SkillCategories { get; set; }
    public bool IncludeSeasonalFactors { get; set; } = true;
}

public class WorkforcePredictionDto
{
    public DateTime GeneratedAt { get; set; }
    public int CurrentHeadcount { get; set; }
    public List<WorkforceForecastPoint> Forecast { get; set; } = new();
    public WorkforceGrowthTrend GrowthTrend { get; set; } = new();
    public List<DepartmentForecast> DepartmentForecasts { get; set; } = new();
    public List<SkillDemandForecast> SkillDemandForecasts { get; set; } = new();
    public decimal ConfidenceScore { get; set; }
    public List<string> Assumptions { get; set; } = new();
    public List<string> RiskFactors { get; set; } = new();
}

public class WorkforceForecastPoint
{
    public DateTime Date { get; set; }
    public int PredictedHeadcount { get; set; }
    public int MinRange { get; set; }
    public int MaxRange { get; set; }
    public decimal ConfidenceLevel { get; set; }
    public string Reasoning { get; set; } = string.Empty;
}

public class WorkforceGrowthTrend
{
    public decimal MonthlyGrowthRate { get; set; }
    public decimal YearOverYearGrowth { get; set; }
    public TrendDirection Direction { get; set; }
    public string TrendDescription { get; set; } = string.Empty;
}

public class DepartmentForecast
{
    public string DepartmentName { get; set; } = string.Empty;
    public int CurrentCount { get; set; }
    public int PredictedCount { get; set; }
    public decimal GrowthPercentage { get; set; }
    public List<string> KeyDrivers { get; set; } = new();
}

public class SkillDemandForecast
{
    public string SkillName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DemandLevel CurrentDemand { get; set; }
    public DemandLevel PredictedDemand { get; set; }
    public int EstimatedPositions { get; set; }
    public decimal UrgencyScore { get; set; }
}

public enum TrendDirection
{
    Increasing,
    Decreasing,
    Stable,
    Volatile
}

public enum DemandLevel
{
    Low,
    Medium,
    High,
    Critical
}