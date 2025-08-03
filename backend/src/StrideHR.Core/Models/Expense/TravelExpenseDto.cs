using System.ComponentModel.DataAnnotations;
using StrideHR.Core.Enums;

namespace StrideHR.Core.Models.Expense;

public class TravelExpenseDto
{
    public int Id { get; set; }
    public int ExpenseClaimId { get; set; }
    public string TravelPurpose { get; set; } = string.Empty;
    public string FromLocation { get; set; } = string.Empty;
    public string ToLocation { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public TravelMode TravelMode { get; set; }
    public string TravelModeText { get; set; } = string.Empty;
    public decimal? MileageDistance { get; set; }
    public decimal? MileageRate { get; set; }
    public decimal? CalculatedMileageAmount { get; set; }
    public string? VehicleDetails { get; set; }
    public string? RouteDetails { get; set; }
    public bool IsRoundTrip { get; set; }
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public List<TravelExpenseItemDto> TravelItems { get; set; } = new();
}

public class TravelExpenseItemDto
{
    public int Id { get; set; }
    public int TravelExpenseId { get; set; }
    public TravelExpenseType ExpenseType { get; set; }
    public string ExpenseTypeText { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public string? Vendor { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }
    public bool RequiresReceipt { get; set; }
    public bool HasReceipt { get; set; }
}

public class CreateTravelExpenseDto
{
    [Required]
    [StringLength(500)]
    public string TravelPurpose { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string FromLocation { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string ToLocation { get; set; } = string.Empty;

    [Required]
    public DateTime DepartureDate { get; set; }

    [Required]
    public DateTime ReturnDate { get; set; }

    [Required]
    public TravelMode TravelMode { get; set; }

    public decimal? MileageDistance { get; set; }

    public decimal? MileageRate { get; set; }

    [StringLength(200)]
    public string? VehicleDetails { get; set; }

    [StringLength(1000)]
    public string? RouteDetails { get; set; }

    public bool IsRoundTrip { get; set; } = true;

    public int? ProjectId { get; set; }

    public List<CreateTravelExpenseItemDto> TravelItems { get; set; } = new();
}

public class CreateTravelExpenseItemDto
{
    [Required]
    public TravelExpenseType ExpenseType { get; set; }

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [StringLength(3)]
    public string Currency { get; set; } = "USD";

    [Required]
    public DateTime ExpenseDate { get; set; }

    [StringLength(200)]
    public string? Vendor { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class MileageCalculationDto
{
    public decimal Distance { get; set; }
    public decimal Rate { get; set; }
    public bool IsRoundTrip { get; set; }
    public string FromLocation { get; set; } = string.Empty;
    public string ToLocation { get; set; } = string.Empty;
    public TravelMode TravelMode { get; set; }
    public string? RouteDetails { get; set; }
}

public class MileageCalculationResultDto
{
    public decimal TotalDistance { get; set; }
    public decimal Rate { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsRoundTrip { get; set; }
    public string CalculationDetails { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; }
}