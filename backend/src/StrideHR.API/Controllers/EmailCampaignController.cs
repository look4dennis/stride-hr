using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Attributes;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Email;

namespace StrideHR.API.Controllers;

[Authorize]
public class EmailCampaignController : BaseController
{
    private readonly IEmailService _emailService;

    public EmailCampaignController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpGet]
    [RequirePermission("EmailCampaign", "Read")]
    public async Task<IActionResult> GetCampaigns([FromQuery] EmailCampaignFilterDto filter)
    {
        var campaigns = await _emailService.GetCampaignsAsync(filter);
        return Success(campaigns);
    }

    [HttpGet("{id}")]
    [RequirePermission("EmailCampaign", "Read")]
    public async Task<IActionResult> GetCampaign(int id)
    {
        var campaign = await _emailService.GetCampaignAsync(id);
        if (campaign == null)
            return NotFound();

        return Success(campaign);
    }

    [HttpPost]
    [RequirePermission("EmailCampaign", "Create")]
    public async Task<IActionResult> CreateCampaign([FromBody] CreateEmailCampaignDto dto)
    {
        try
        {
            var campaign = await _emailService.CreateCampaignAsync(dto);
            return Success(campaign, "Email campaign created successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [RequirePermission("EmailCampaign", "Update")]
    public async Task<IActionResult> UpdateCampaign(int id, [FromBody] UpdateEmailCampaignDto dto)
    {
        try
        {
            var campaign = await _emailService.UpdateCampaignAsync(id, dto);
            return Success(campaign, "Email campaign updated successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost("{id}/start")]
    [RequirePermission("EmailCampaign", "Execute")]
    public async Task<IActionResult> StartCampaign(int id)
    {
        var result = await _emailService.StartCampaignAsync(id);
        if (!result)
            return NotFound();

        return Success("Email campaign started successfully");
    }

    [HttpPost("{id}/pause")]
    [RequirePermission("EmailCampaign", "Execute")]
    public async Task<IActionResult> PauseCampaign(int id)
    {
        var result = await _emailService.PauseCampaignAsync(id);
        if (!result)
            return NotFound();

        return Success("Email campaign paused successfully");
    }

    [HttpPost("{id}/cancel")]
    [RequirePermission("EmailCampaign", "Execute")]
    public async Task<IActionResult> CancelCampaign(int id)
    {
        var result = await _emailService.CancelCampaignAsync(id);
        if (!result)
            return NotFound();

        return Success("Email campaign cancelled successfully");
    }

    [HttpGet("{id}/stats")]
    [RequirePermission("EmailCampaign", "Read")]
    public async Task<IActionResult> GetCampaignStats(int id)
    {
        try
        {
            var stats = await _emailService.GetCampaignStatsAsync(id);
            return Success(stats);
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
    }
}