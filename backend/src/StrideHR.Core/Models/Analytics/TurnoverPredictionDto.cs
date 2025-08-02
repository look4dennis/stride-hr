namespace StrideHR.Core.Models.Analytics;

public class TurnoverPredictionRequest
{
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public int? ManagerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<int>? EmployeeIds { get; set; }
    public bool IncludeRiskFactors { get; set; } = true;
}

public class TurnoverPredictionDto
{
    public DateTime GeneratedAt { get; set; }
    public decimal OverallTurnoverRisk { get; set; }
    public decimal PredictedTurnoverRate { get; set; }
    public int EstimatedLeavers { get; set; }
    public List<EmployeeTurnoverRisk> EmployeeRisks { get; set; } = new();
    public List<DepartmentTurnoverRisk> DepartmentRisks { get; set; } = new();
    public List<TurnoverRiskFactor> TopRiskFactors { get; set; } = new();
    public List<RetentionRecommendation> RetentionRecommendations { get; set; } = new();
    public TurnoverTrendAnalysis TrendAnalysis { get; set; } = new();
}

public class EmployeeTurnoverRisk
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal RiskScore { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public List<string> RiskFactors { get; set; } = new();
    public DateTime? PredictedLeaveDate { get; set; }
    public decimal ConfidenceLevel { get; set; }
    public List<string> RecommendedActions { get; set; } = new();
}

public class DepartmentTurnoverRisk
{
    public string DepartmentName { get; set; } = string.Empty;
    public decimal AverageRiskScore { get; set; }
    public int HighRiskEmployees { get; set; }
    public decimal PredictedTurnoverRate { get; set; }
    public List<string> KeyRiskFactors { get; set; } = new();
}

public class TurnoverRiskFactor
{
    public string FactorName { get; set; } = string.Empty;
    public decimal Impact { get; set; }
    public int AffectedEmployees { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> MitigationStrategies { get; set; } = new();
}

public class RetentionRecommendation
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PotentialImpact { get; set; }
    public ImplementationDifficulty Difficulty { get; set; }
    public int Priority { get; set; }
    public List<string> ActionItems { get; set; } = new();
    public decimal EstimatedCost { get; set; }
    public decimal ExpectedROI { get; set; }
}

public class TurnoverTrendAnalysis
{
    public List<MonthlyTurnoverData> HistoricalData { get; set; } = new();
    public List<MonthlyTurnoverData> PredictedData { get; set; } = new();
    public List<string> SeasonalPatterns { get; set; } = new();
    public List<string> TrendInsights { get; set; } = new();
}

public class MonthlyTurnoverData
{
    public DateTime Month { get; set; }
    public decimal TurnoverRate { get; set; }
    public int Leavers { get; set; }
    public int Headcount { get; set; }
    public List<string> KeyReasons { get; set; } = new();
}

public enum RiskLevel
{
    Low,
    Medium,
    High,
    Critical
}

public enum ImplementationDifficulty
{
    Easy,
    Medium,
    Hard,
    VeryHard
}