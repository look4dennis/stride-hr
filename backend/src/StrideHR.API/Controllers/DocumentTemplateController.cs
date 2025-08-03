using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Attributes;
using StrideHR.API.Models;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.DocumentTemplate;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentTemplateController : BaseController
{
    private readonly IDocumentTemplateService _documentTemplateService;

    public DocumentTemplateController(IDocumentTemplateService documentTemplateService)
    {
        _documentTemplateService = documentTemplateService;
    }

    [HttpGet]
    [RequirePermission("DocumentTemplate", "Read")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DocumentTemplateDto>>>> GetAllTemplates()
    {
        try
        {
            var templates = await _documentTemplateService.GetAllTemplatesAsync();
            return Ok(ApiResponse<IEnumerable<DocumentTemplateDto>>.CreateSuccess(templates));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<IEnumerable<DocumentTemplateDto>>.CreateFailure(ex.Message));
        }
    }

    [HttpGet("active")]
    [RequirePermission("DocumentTemplate", "Read")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DocumentTemplateDto>>>> GetActiveTemplates()
    {
        try
        {
            var templates = await _documentTemplateService.GetActiveTemplatesAsync();
            return Ok(ApiResponse<IEnumerable<DocumentTemplateDto>>.CreateSuccess(templates));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<IEnumerable<DocumentTemplateDto>>.CreateFailure(ex.Message));
        }
    }

    [HttpGet("by-type/{type}")]
    [RequirePermission("DocumentTemplate", "Read")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DocumentTemplateDto>>>> GetTemplatesByType(DocumentType type)
    {
        try
        {
            var templates = await _documentTemplateService.GetTemplatesByTypeAsync(type);
            return Ok(ApiResponse<IEnumerable<DocumentTemplateDto>>.CreateSuccess(templates));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<IEnumerable<DocumentTemplateDto>>.CreateFailure(ex.Message));
        }
    }

    [HttpGet("by-category/{category}")]
    [RequirePermission("DocumentTemplate", "Read")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DocumentTemplateDto>>>> GetTemplatesByCategory(string category)
    {
        try
        {
            var templates = await _documentTemplateService.GetTemplatesByCategoryAsync(category);
            return Ok(ApiResponse<IEnumerable<DocumentTemplateDto>>.CreateSuccess(templates));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<IEnumerable<DocumentTemplateDto>>.CreateFailure(ex.Message));
        }
    }

    [HttpGet("{id}")]
    [RequirePermission("DocumentTemplate", "Read")]
    public async Task<ActionResult<ApiResponse<DocumentTemplateDto>>> GetTemplateById(int id)
    {
        try
        {
            var template = await _documentTemplateService.GetTemplateByIdAsync(id);
            if (template == null)
            {
                return NotFound(ApiResponse<DocumentTemplateDto>.CreateFailure("Template not found"));
            }

            return Ok(ApiResponse<DocumentTemplateDto>.CreateSuccess(template));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DocumentTemplateDto>.CreateFailure(ex.Message));
        }
    }

    [HttpPost]
    [RequirePermission("DocumentTemplate", "Create")]
    public async Task<ActionResult<ApiResponse<DocumentTemplateDto>>> CreateTemplate([FromBody] CreateDocumentTemplateDto dto)
    {
        try
        {
            var template = await _documentTemplateService.CreateTemplateAsync(dto, GetCurrentUserId());
            return CreatedAtAction(nameof(GetTemplateById), new { id = template.Id }, 
                ApiResponse<DocumentTemplateDto>.CreateSuccess(template));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DocumentTemplateDto>.CreateFailure(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [RequirePermission("DocumentTemplate", "Update")]
    public async Task<ActionResult<ApiResponse<DocumentTemplateDto>>> UpdateTemplate(int id, [FromBody] UpdateDocumentTemplateDto dto)
    {
        try
        {
            var template = await _documentTemplateService.UpdateTemplateAsync(id, dto, GetCurrentUserId());
            return Ok(ApiResponse<DocumentTemplateDto>.CreateSuccess(template));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DocumentTemplateDto>.CreateFailure(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [RequirePermission("DocumentTemplate", "Delete")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTemplate(int id)
    {
        try
        {
            var result = await _documentTemplateService.DeleteTemplateAsync(id, GetCurrentUserId());
            if (!result)
            {
                return NotFound(ApiResponse<bool>.CreateFailure("Template not found"));
            }

            return Ok(ApiResponse<bool>.CreateSuccess(true));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.CreateFailure(ex.Message));
        }
    }

    [HttpPost("{id}/activate")]
    [RequirePermission("DocumentTemplate", "Update")]
    public async Task<ActionResult<ApiResponse<bool>>> ActivateTemplate(int id)
    {
        try
        {
            var result = await _documentTemplateService.ActivateTemplateAsync(id, GetCurrentUserId());
            if (!result)
            {
                return NotFound(ApiResponse<bool>.CreateFailure("Template not found"));
            }

            return Ok(ApiResponse<bool>.CreateSuccess(true));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.CreateFailure(ex.Message));
        }
    }

    [HttpPost("{id}/deactivate")]
    [RequirePermission("DocumentTemplate", "Update")]
    public async Task<ActionResult<ApiResponse<bool>>> DeactivateTemplate(int id)
    {
        try
        {
            var result = await _documentTemplateService.DeactivateTemplateAsync(id, GetCurrentUserId());
            if (!result)
            {
                return NotFound(ApiResponse<bool>.CreateFailure("Template not found"));
            }

            return Ok(ApiResponse<bool>.CreateSuccess(true));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.CreateFailure(ex.Message));
        }
    }

    [HttpGet("{id}/preview")]
    [RequirePermission("DocumentTemplate", "Read")]
    public async Task<ActionResult<ApiResponse<DocumentTemplatePreviewDto>>> PreviewTemplate(int id, [FromQuery] Dictionary<string, object>? sampleData = null)
    {
        try
        {
            var preview = await _documentTemplateService.PreviewTemplateAsync(id, sampleData);
            return Ok(ApiResponse<DocumentTemplatePreviewDto>.CreateSuccess(preview));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DocumentTemplatePreviewDto>.CreateFailure(ex.Message));
        }
    }

    [HttpGet("categories")]
    [RequirePermission("DocumentTemplate", "Read")]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetAvailableCategories()
    {
        try
        {
            var categories = await _documentTemplateService.GetAvailableCategoriesAsync();
            return Ok(ApiResponse<IEnumerable<string>>.CreateSuccess(categories));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<IEnumerable<string>>.CreateFailure(ex.Message));
        }
    }

    [HttpGet("merge-fields/{type}")]
    [RequirePermission("DocumentTemplate", "Read")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> GetAvailableMergeFields(DocumentType type)
    {
        try
        {
            var mergeFields = await _documentTemplateService.GetAvailableMergeFieldsAsync(type);
            return Ok(ApiResponse<Dictionary<string, object>>.CreateSuccess(mergeFields));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<Dictionary<string, object>>.CreateFailure(ex.Message));
        }
    }

    [HttpPost("{id}/validate")]
    [RequirePermission("DocumentTemplate", "Read")]
    public async Task<ActionResult<ApiResponse<bool>>> ValidateTemplate(int id)
    {
        try
        {
            var isValid = await _documentTemplateService.ValidateTemplateAsync(id);
            return Ok(ApiResponse<bool>.CreateSuccess(isValid));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.CreateFailure(ex.Message));
        }
    }

    [HttpPost("{id}/clone")]
    [RequirePermission("DocumentTemplate", "Create")]
    public async Task<ActionResult<ApiResponse<DocumentTemplateDto>>> CloneTemplate(int id, [FromBody] string newName)
    {
        try
        {
            var clonedTemplate = await _documentTemplateService.CloneTemplateAsync(id, newName, GetCurrentUserId());
            return CreatedAtAction(nameof(GetTemplateById), new { id = clonedTemplate.Id }, 
                ApiResponse<DocumentTemplateDto>.CreateSuccess(clonedTemplate));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DocumentTemplateDto>.CreateFailure(ex.Message));
        }
    }
}