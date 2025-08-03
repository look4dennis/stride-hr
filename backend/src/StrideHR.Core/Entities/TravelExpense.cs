using StrideHR.Core.Enums;

namespace StrideHR.Core.Entities;

public class TravelExpense : BaseEntity
{
    public int ExpenseClaimId { get; set; }
    public string TravelPurpose { get; set; } = string.Empty;
    public string FromLocation { get; set; } = string.Empty;
    public string ToLocation { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public TravelMode TravelMode { get; set; }
    public decimal? MileageDistance { get; set; }
    public decimal? MileageRate { get; set; }
    public decimal? CalculatedMileageAmount { get; set; }
    public string? VehicleDetails { get; set; }
    public string? RouteDetails { get; set; }
    public bool IsRoundTrip { get; set; }
    public int? ProjectId { get; set; }

    // Navigation Properties
    public virtual ExpenseClaim ExpenseClaim { get; set; } = null!;
    public virtual Project? Project { get; set; }
    public virtual ICollection<TravelExpenseItem> TravelItems { get; set; } = new List<TravelExpenseItem>();
}

public class TravelExpenseItem : BaseEntity
{
    public int TravelExpenseId { get; set; }
    public TravelExpenseType ExpenseType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime ExpenseDate { get; set; }
    public string? Vendor { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public bool RequiresReceipt { get; set; }
    public bool HasReceipt { get; set; }

    // Navigation Properties
    public virtual TravelExpense TravelExpense { get; set; } = null!;
}