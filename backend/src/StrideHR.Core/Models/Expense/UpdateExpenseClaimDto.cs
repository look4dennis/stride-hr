using System.ComponentModel.DataAnnotations;

namespace StrideHR.Core.Models.Expense;

public class UpdateExpenseClaimDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime ExpenseDate { get; set; }

    [StringLength(3)]
    public string Currency { get; set; } = "USD";

    public bool IsAdvanceClaim { get; set; }

    public decimal? AdvanceAmount { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public List<UpdateExpenseItemDto> ExpenseItems { get; set; } = new();
}

public class UpdateExpenseItemDto
{
    public int? Id { get; set; } // Null for new items

    [Required]
    public int ExpenseCategoryId { get; set; }

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

    public bool IsBillable { get; set; }

    public int? ProjectId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public decimal? MileageDistance { get; set; }

    public decimal? MileageRate { get; set; }
}