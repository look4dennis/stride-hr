using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Attributes;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Email;

namespace StrideHR.API.Controllers;

[Authorize]
public class EmailController : BaseController
{
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send")]
    [RequirePermission("Email", "Send")]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailDto dto)
    {
        try
        {
            var result = await _emailService.SendEmailAsync(dto);
            return Success(result, "Email sent successfully");
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost("send-bulk")]
    [RequirePermission("Email", "SendBulk")]
    public async Task<IActionResult> SendBulkEmail([FromBody] BulkEmailDto dto)
    {
        try
        {
            var results = await _emailService.SendBulkEmailAsync(dto);
            return Success(results, $"Bulk email sent to {results.Count} recipients");
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost("send-template")]
    [RequirePermission("Email", "Send")]
    public async Task<IActionResult> SendTemplateEmail([FromBody] SendTemplateEmailDto dto)
    {
        try
        {
            var result = await _emailService.SendTemplateEmailAsync(dto);
            return Success(result, "Template email sent successfully");
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost("send-bulk-template")]
    [RequirePermission("Email", "SendBulk")]
    public async Task<IActionResult> SendBulkTemplateEmail([FromBody] BulkTemplateEmailDto dto)
    {
        try
        {
            var results = await _emailService.SendBulkTemplateEmailAsync(dto);
            return Success(results, $"Bulk template email sent to {results.Count} recipients");
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("logs")]
    [RequirePermission("Email", "Read")]
    public async Task<IActionResult> GetEmailLogs([FromQuery] EmailLogFilterDto filter)
    {
        var logs = await _emailService.GetEmailLogsAsync(filter);
        return Success(logs);
    }

    [HttpGet("logs/{id}")]
    [RequirePermission("Email", "Read")]
    public async Task<IActionResult> GetEmailLog(int id)
    {
        var log = await _emailService.GetEmailLogAsync(id);
        if (log == null)
            return NotFound();

        return Success(log);
    }

    [HttpGet("pending")]
    [RequirePermission("Email", "Read")]
    public async Task<IActionResult> GetPendingEmails([FromQuery] int limit = 100)
    {
        var pendingEmails = await _emailService.GetPendingEmailsAsync(limit);
        return Success(pendingEmails);
    }

    [HttpPost("retry-failed")]
    [RequirePermission("Email", "Retry")]
    public async Task<IActionResult> RetryFailedEmails([FromQuery] int maxRetries = 3)
    {
        var result = await _emailService.RetryFailedEmailsAsync(maxRetries);
        return Success(result, "Failed emails retry initiated");
    }

    [HttpPost("process-queue")]
    [RequirePermission("Email", "Process")]
    public async Task<IActionResult> ProcessEmailQueue([FromQuery] int batchSize = 100)
    {
        var processedCount = await _emailService.ProcessEmailQueueAsync(batchSize);
        return Success(new { ProcessedCount = processedCount }, $"Processed {processedCount} emails from queue");
    }

    [HttpPost("validate-email")]
    public async Task<IActionResult> ValidateEmail([FromBody] ValidateEmailRequest request)
    {
        var isValid = await _emailService.ValidateEmailAddressAsync(request.Email);
        return Success(new { IsValid = isValid, Email = request.Email });
    }

    [HttpPost("validate-bulk-emails")]
    public async Task<IActionResult> ValidateBulkEmails([FromBody] ValidateBulkEmailsRequest request)
    {
        var validEmails = await _emailService.ValidateBulkEmailAddressesAsync(request.Emails);
        return Success(new { ValidEmails = validEmails, ValidCount = validEmails.Count, TotalCount = request.Emails.Count });
    }

    [HttpPost("track/open/{externalId}")]
    public async Task<IActionResult> TrackEmailOpen(string externalId)
    {
        var result = await _emailService.TrackEmailOpenAsync(externalId);
        if (!result)
            return NotFound();

        return Success("Email open tracked successfully");
    }

    [HttpPost("track/click/{externalId}")]
    public async Task<IActionResult> TrackEmailClick(string externalId, [FromBody] TrackClickRequest request)
    {
        var result = await _emailService.TrackEmailClickAsync(externalId, request.Url);
        if (!result)
            return NotFound();

        return Success("Email click tracked successfully");
    }

    [HttpPost("unsubscribe/{token}")]
    public async Task<IActionResult> ProcessUnsubscribe(string token)
    {
        var result = await _emailService.ProcessUnsubscribeAsync(token);
        if (!result)
            return BadRequest("Invalid unsubscribe token");

        return Success("Unsubscribed successfully");
    }

    [HttpGet("analytics")]
    [RequirePermission("Email", "Analytics")]
    public async Task<IActionResult> GetEmailAnalytics([FromQuery] EmailAnalyticsFilterDto filter)
    {
        var analytics = await _emailService.GetEmailAnalyticsAsync(filter);
        return Success(analytics);
    }

    [HttpGet("delivery-stats")]
    [RequirePermission("Email", "Analytics")]
    public async Task<IActionResult> GetDeliveryStats([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, [FromQuery] int? branchId = null)
    {
        var stats = await _emailService.GetDeliveryStatsAsync(fromDate, toDate, branchId);
        return Success(stats);
    }

    [HttpGet("template-stats")]
    [RequirePermission("Email", "Analytics")]
    public async Task<IActionResult> GetTemplateUsageStats([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var stats = await _emailService.GetTemplateUsageStatsAsync(fromDate, toDate);
        return Success(stats);
    }

    [HttpGet("campaign-performance")]
    [RequirePermission("Email", "Analytics")]
    public async Task<IActionResult> GetCampaignPerformance([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var stats = await _emailService.GetCampaignPerformanceAsync(fromDate, toDate);
        return Success(stats);
    }
}

public class ValidateEmailRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ValidateBulkEmailsRequest
{
    public List<string> Emails { get; set; } = new();
}

public class TrackClickRequest
{
    public string Url { get; set; } = string.Empty;
}