using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Attributes;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Email;

namespace StrideHR.API.Controllers;

[Authorize]
public class EmailTemplateController : BaseController
{
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;

    public EmailTemplateController(IEmailService emailService, IEmailTemplateService templateService)
    {
        _emailService = emailService;
        _templateService = templateService;
    }

    [HttpGet]
    [RequirePermission("EmailTemplate", "Read")]
    public async Task<IActionResult> GetTemplates([FromQuery] EmailTemplateFilterDto filter)
    {
        var templates = await _emailService.GetTemplatesAsync(filter);
        return Success(templates);
    }

    [HttpGet("{id}")]
    [RequirePermission("EmailTemplate", "Read")]
    public async Task<IActionResult> GetTemplate(int id)
    {
        var template = await _emailService.GetTemplateAsync(id);
        if (template == null)
            return NotFound();

        return Success(template);
    }

    [HttpGet("by-name/{name}")]
    [RequirePermission("EmailTemplate", "Read")]
    public async Task<IActionResult> GetTemplateByName(string name)
    {
        var template = await _emailService.GetTemplateByNameAsync(name);
        if (template == null)
            return NotFound();

        return Success(template);
    }

    [HttpPost]
    [RequirePermission("EmailTemplate", "Create")]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateEmailTemplateDto dto)
    {
        try
        {
            var template = await _emailService.CreateTemplateAsync(dto);
            return Success(template, "Email template created successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [RequirePermission("EmailTemplate", "Update")]
    public async Task<IActionResult> UpdateTemplate(int id, [FromBody] UpdateEmailTemplateDto dto)
    {
        try
        {
            var template = await _emailService.UpdateTemplateAsync(id, dto);
            return Success(template, "Email template updated successfully");
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [RequirePermission("EmailTemplate", "Delete")]
    public async Task<IActionResult> DeleteTemplate(int id)
    {
        var result = await _emailService.DeleteTemplateAsync(id);
        if (!result)
            return NotFound();

        return Success("Email template deleted successfully");
    }

    [HttpPost("{id}/preview")]
    [RequirePermission("EmailTemplate", "Read")]
    public async Task<IActionResult> PreviewTemplate(int id, [FromBody] Dictionary<string, object> parameters)
    {
        try
        {
            var preview = await _templateService.GeneratePreviewAsync(id, parameters);
            return Success(preview);
        }
        catch (ArgumentException ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost("validate")]
    [RequirePermission("EmailTemplate", "Create")]
    public async Task<IActionResult> ValidateTemplate([FromBody] ValidateTemplateRequest request)
    {
        var validation = await _templateService.ValidateTemplateAsync(request.TemplateContent, request.RequiredParameters);
        return Success(validation);
    }

    [HttpPost("render")]
    [RequirePermission("EmailTemplate", "Read")]
    public async Task<IActionResult> RenderTemplate([FromBody] RenderTemplateRequest request)
    {
        try
        {
            var rendered = await _templateService.RenderTemplateAsync(request.TemplateContent, request.Parameters);
            return Success(new { RenderedContent = rendered });
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("system")]
    [RequirePermission("EmailTemplate", "Read")]
    public async Task<IActionResult> GetSystemTemplates()
    {
        var templates = await _emailService.GetSystemTemplatesAsync();
        return Success(templates);
    }

    [HttpPost("seed-defaults")]
    [RequirePermission("EmailTemplate", "Create")]
    public async Task<IActionResult> SeedDefaultTemplates()
    {
        await _emailService.SeedDefaultTemplatesAsync();
        return Success("Default email templates seeded successfully");
    }
}

public class ValidateTemplateRequest
{
    public string TemplateContent { get; set; } = string.Empty;
    public List<string> RequiredParameters { get; set; } = new();
}

public class RenderTemplateRequest
{
    public string TemplateContent { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}