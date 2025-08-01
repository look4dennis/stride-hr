namespace StrideHR.Core.Models;

/// <summary>
/// JWT configuration settings
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JWT";
    
    /// <summary>
    /// Secret key for signing tokens
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Token issuer
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    
    /// <summary>
    /// Token audience
    /// </summary>
    public string Audience { get; set; } = string.Empty;
    
    /// <summary>
    /// Access token expiration in hours
    /// </summary>
    public int ExpirationHours { get; set; } = 24;
    
    /// <summary>
    /// Refresh token expiration in days
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 30;
    
    /// <summary>
    /// Clock skew in minutes (tolerance for token expiration)
    /// </summary>
    public int ClockSkewMinutes { get; set; } = 5;
    
    /// <summary>
    /// Validate token issuer
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;
    
    /// <summary>
    /// Validate token audience
    /// </summary>
    public bool ValidateAudience { get; set; } = true;
    
    /// <summary>
    /// Validate token lifetime
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;
    
    /// <summary>
    /// Validate issuer signing key
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;
    
    /// <summary>
    /// Require token expiration
    /// </summary>
    public bool RequireExpirationTime { get; set; } = true;
    
    /// <summary>
    /// Save token in authentication properties
    /// </summary>
    public bool SaveToken { get; set; } = true;
}