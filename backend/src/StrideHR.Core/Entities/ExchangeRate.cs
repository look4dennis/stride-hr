namespace StrideHR.Core.Entities;

public class ExchangeRate : BaseEntity
{
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string Source { get; set; } = string.Empty; // API source or manual
    public DateTime LastUpdated { get; set; }
}