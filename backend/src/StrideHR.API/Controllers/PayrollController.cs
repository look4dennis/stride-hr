using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.API.Attributes;
using StrideHR.API.Models;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Payroll;
using System.IdentityModel.Tokens.Jwt;

namespace StrideHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PayrollController : BaseController
{
    private readonly IPayrollService _payrollService;
    private readonly IPayslipGenerationService _payslipGenerationService;
    private readonly IPayslipTemplateService _payslipTemplateService;

    public PayrollController(
        IPayrollService payrollService,
        IPayslipGenerationService payslipGenerationService,
        IPayslipTemplateService payslipTemplateService)
    {
        _payrollService = payrollService;
        _payslipGenerationService = payslipGenerationService;
        _payslipTemplateService = payslipTemplateService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }

    private int GetCurrentUserOrganizationId()
    {
        // For now, we'll assume organization ID is 1
        // In a real implementation, this would come from the user's claims or be fetched from the database
        return 1;
    }

    #region Payroll Calculation

    [HttpPost("calculate")]
    [RequirePermission("payroll.calculate")]
    public async Task<IActionResult> CalculatePayroll(
        [FromBody] PayrollCalculationRequest request)
    {
        try
        {
            var result = await _payrollService.CalculatePayrollAsync(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost("create")]
    [RequirePermission("payroll.create")]
    public async Task<IActionResult> CreatePayrollRecord(
        [FromBody] PayrollCalculationRequest request)
    {
        try
        {
            var result = await _payrollService.CreatePayrollRecordAsync(request);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost("process-branch")]
    [RequirePermission("payroll.process")]
    public async Task<IActionResult> ProcessBranchPayroll(
        [FromQuery] int branchId, [FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var results = await _payrollService.ProcessBranchPayrollAsync(branchId, year, month);
            return Success(results);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost("{payrollRecordId}/approve")]
    [RequirePermission("payroll.approve")]
    public async Task<IActionResult> ApprovePayrollRecord(int payrollRecordId)
    {
        try
        {
            var result = await _payrollService.ApprovePayrollRecordAsync(payrollRecordId, GetCurrentUserId());
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    #endregion

    #region Payslip Templates

    [HttpGet("templates")]
    [RequirePermission("payroll.templates.view")]
    public async Task<IActionResult> GetTemplates([FromQuery] int? branchId = null)
    {
        try
        {
            var organizationId = GetCurrentUserOrganizationId();
            var templates = branchId.HasValue 
                ? await _payslipTemplateService.GetBranchTemplatesAsync(branchId.Value)
                : await _payslipTemplateService.GetOrganizationTemplatesAsync(organizationId);
            
            return Success(templates);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("templates/{templateId}")]
    [RequirePermission("payroll.templates.view")]
    public async Task<IActionResult> GetTemplate(int templateId)
    {
        try
        {
            var template = await _payslipTemplateService.GetTemplateAsync(templateId);
            if (template == null)
                return NotFound();

            return Success(template);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost("templates")]
    [RequirePermission("payroll.templates.create")]
    public async Task<IActionResult> CreateTemplate(
        [FromBody] PayslipTemplateDto templateDto)
    {
        try
        {
            templateDto.OrganizationId = GetCurrentUserOrganizationId();
            var result = await _payslipTemplateService.CreateTemplateAsync(templateDto, GetCurrentUserId());
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPut("templates/{templateId}")]
    [RequirePermission("payroll.templates.update")]
    public async Task<IActionResult> UpdateTemplate(
        int templateId, [FromBody] PayslipTemplateDto templateDto)
    {
        try
        {
            templateDto.OrganizationId = GetCurrentUserOrganizationId();
            var result = await _payslipTemplateService.UpdateTemplateAsync(templateId, templateDto, GetCurrentUserId());
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost("templates/{templateId}/set-default")]
    [RequirePermission("payroll.templates.manage")]
    public async Task<IActionResult> SetTemplateAsDefault(
        int templateId, [FromQuery] int? branchId = null)
    {
        try
        {
            var organizationId = GetCurrentUserOrganizationId();
            var result = await _payslipTemplateService.SetAsDefaultAsync(templateId, organizationId, branchId);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpDelete("templates/{templateId}")]
    [RequirePermission("payroll.templates.delete")]
    public async Task<IActionResult> DeactivateTemplate(int templateId)
    {
        try
        {
            var result = await _payslipTemplateService.DeactivateTemplateAsync(templateId);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    #endregion

    #region Payslip Generation

    [HttpPost("payslips/generate")]
    [RequirePermission("payroll.payslips.generate")]
    public async Task<IActionResult> GeneratePayslip(
        [FromBody] CreatePayslipGenerationRequest request)
    {
        try
        {
            var result = await _payslipGenerationService.GeneratePayslipAsync(request, GetCurrentUserId());
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost("payslips/generate-bulk")]
    [RequirePermission("payroll.payslips.generate")]
    public async Task<IActionResult> GenerateBulkPayslips(
        [FromBody] BulkPayslipGenerationRequest request)
    {
        try
        {
            var results = await _payslipGenerationService.GenerateBulkPayslipsAsync(request, GetCurrentUserId());
            return Success(results);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("payslips/{payslipGenerationId}")]
    [RequirePermission("payroll.payslips.view")]
    public async Task<IActionResult> GetPayslipGeneration(int payslipGenerationId)
    {
        try
        {
            var result = await _payslipGenerationService.GetPayslipGenerationAsync(payslipGenerationId);
            if (result == null)
                return NotFound();

            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("payslips/pending-approvals")]
    [RequirePermission("payroll.payslips.approve")]
    public async Task<IActionResult> GetPendingApprovals(
        [FromQuery] PayslipApprovalLevel approvalLevel)
    {
        try
        {
            var results = await _payslipGenerationService.GetPendingApprovalsAsync(approvalLevel);
            return Success(results);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost("payslips/approve")]
    [RequirePermission("payroll.payslips.approve")]
    public async Task<IActionResult> ProcessPayslipApproval(
        [FromBody] PayslipApprovalRequest request)
    {
        try
        {
            var result = await _payslipGenerationService.ProcessApprovalAsync(request, GetCurrentUserId());
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost("payslips/release")]
    [RequirePermission("payroll.payslips.release")]
    public async Task<IActionResult> ReleasePayslips(
        [FromBody] PayslipReleaseRequest request)
    {
        try
        {
            var result = await _payslipGenerationService.ReleasePayslipsAsync(request, GetCurrentUserId());
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("payslips/summary")]
    [RequirePermission("payroll.payslips.view")]
    public async Task<IActionResult> GetApprovalSummary(
        [FromQuery] int branchId, [FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var summary = await _payslipGenerationService.GetApprovalSummaryAsync(branchId, year, month);
            return Success(summary);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("payslips/employee/{employeeId}")]
    [RequirePermission("payroll.payslips.view")]
    public async Task<IActionResult> GetEmployeePayslips(
        int employeeId, [FromQuery] int year, [FromQuery] int? month = null)
    {
        try
        {
            var payslips = await _payslipGenerationService.GetEmployeePayslipsAsync(employeeId, year, month);
            return Success(payslips);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpPost("payslips/{payslipGenerationId}/regenerate")]
    [RequirePermission("payroll.payslips.regenerate")]
    public async Task<IActionResult> RegeneratePayslip(
        int payslipGenerationId, [FromBody] RegeneratePayslipRequest request)
    {
        try
        {
            var result = await _payslipGenerationService.RegeneratePayslipAsync(
                payslipGenerationId, request.Reason, GetCurrentUserId());
            return Success(result);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    [HttpGet("payslips/{payslipGenerationId}/download")]
    [RequirePermission("payroll.payslips.download")]
    public async Task<IActionResult> DownloadPayslip(int payslipGenerationId)
    {
        try
        {
            var (fileContent, fileName, contentType) = await _payslipGenerationService.DownloadPayslipAsync(payslipGenerationId);
            return File(fileContent, contentType, fileName);
        }
        catch (Exception ex)
        {
            return Error(ex.Message);
        }
    }

    #endregion
}

public class RegeneratePayslipRequest
{
    public string Reason { get; set; } = string.Empty;
}