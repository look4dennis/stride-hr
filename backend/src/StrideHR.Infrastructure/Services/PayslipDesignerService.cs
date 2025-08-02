using Microsoft.Extensions.Logging;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Payroll;
using System.Text;
using System.Text.Json;

namespace StrideHR.Infrastructure.Services;

public class PayslipDesignerService : IPayslipDesignerService
{
    private readonly ILogger<PayslipDesignerService> _logger;

    public PayslipDesignerService(ILogger<PayslipDesignerService> logger)
    {
        _logger = logger;
    }

    public async Task<(byte[] pdfContent, string fileName)> GeneratePdfPayslipAsync(PayslipTemplateDto template, PayrollCalculationResult payrollData)
    {
        try
        {
            // Generate HTML content
            var htmlContent = await GenerateHtmlPreviewAsync(template, payrollData);
            
            // For now, we'll create a simple PDF representation
            // In a real implementation, you would use a library like iTextSharp, PuppeteerSharp, or similar
            var fileName = $"Payslip_{payrollData.EmployeeName.Replace(" ", "_")}_{payrollData.PayrollYear:D4}{payrollData.PayrollMonth:D2}.pdf";
            
            // Convert HTML to PDF (simplified implementation)
            var pdfContent = await ConvertHtmlToPdfAsync(htmlContent);
            
            return (pdfContent, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF payslip for employee {EmployeeId}", payrollData.EmployeeId);
            throw;
        }
    }

    public async Task<string> GenerateHtmlPreviewAsync(PayslipTemplateDto template, PayrollCalculationResult payrollData)
    {
        try
        {
            var html = new StringBuilder();
            
            // Start HTML document
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<title>Payslip</title>");
            html.AppendLine(GenerateStyleSheet(template));
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            // Generate payslip content based on template sections
            foreach (var section in template.TemplateConfig.Sections.OrderBy(s => s.Order))
            {
                if (!section.IsVisible) continue;
                
                html.AppendLine($"<div class='section section-{section.Type}' id='{section.Id}'>");
                
                switch (section.Type.ToLower())
                {
                    case "header":
                        html.AppendLine(GenerateHeaderSection(template, payrollData));
                        break;
                    case "employee-info":
                        html.AppendLine(GenerateEmployeeInfoSection(template, payrollData, section));
                        break;
                    case "earnings":
                        html.AppendLine(GenerateEarningsSection(template, payrollData, section));
                        break;
                    case "deductions":
                        html.AppendLine(GenerateDeductionsSection(template, payrollData, section));
                        break;
                    case "summary":
                        html.AppendLine(GenerateSummarySection(template, payrollData, section));
                        break;
                    case "footer":
                        html.AppendLine(GenerateFooterSection(template, payrollData));
                        break;
                }
                
                html.AppendLine("</div>");
            }
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating HTML preview for employee {EmployeeId}", payrollData.EmployeeId);
            throw;
        }
    }

    public async Task<Dictionary<string, string>> GetAvailableFieldMappingsAsync()
    {
        return new Dictionary<string, string>
        {
            // Employee Information
            ["employee.name"] = "Employee Name",
            ["employee.id"] = "Employee ID",
            ["employee.designation"] = "Designation",
            ["employee.department"] = "Department",
            ["employee.joiningDate"] = "Joining Date",
            
            // Payroll Period
            ["payroll.month"] = "Payroll Month",
            ["payroll.year"] = "Payroll Year",
            ["payroll.periodStart"] = "Period Start",
            ["payroll.periodEnd"] = "Period End",
            
            // Salary Components
            ["salary.basic"] = "Basic Salary",
            ["salary.gross"] = "Gross Salary",
            ["salary.net"] = "Net Salary",
            ["salary.currency"] = "Currency",
            
            // Allowances
            ["allowances.total"] = "Total Allowances",
            ["allowances.hra"] = "House Rent Allowance",
            ["allowances.transport"] = "Transport Allowance",
            ["allowances.medical"] = "Medical Allowance",
            ["allowances.food"] = "Food Allowance",
            ["allowances.other"] = "Other Allowances",
            
            // Deductions
            ["deductions.total"] = "Total Deductions",
            ["deductions.tax"] = "Income Tax",
            ["deductions.pf"] = "Provident Fund",
            ["deductions.esi"] = "Employee State Insurance",
            ["deductions.professional"] = "Professional Tax",
            
            // Overtime
            ["overtime.hours"] = "Overtime Hours",
            ["overtime.amount"] = "Overtime Amount",
            
            // Organization
            ["organization.name"] = "Organization Name",
            ["organization.address"] = "Organization Address",
            ["organization.logo"] = "Organization Logo"
        };
    }

    public async Task<(bool isValid, List<string> errors)> ValidateTemplateDesignAsync(PayslipTemplateConfig templateConfig)
    {
        var errors = new List<string>();
        
        if (templateConfig.Sections == null || !templateConfig.Sections.Any())
        {
            errors.Add("Template must have at least one section");
        }
        
        // Check for required sections
        var sectionTypes = templateConfig.Sections.Select(s => s.Type.ToLower()).ToList();
        if (!sectionTypes.Contains("header"))
            errors.Add("Template must include a header section");
        
        if (!sectionTypes.Contains("summary"))
            errors.Add("Template must include a summary section");
        
        // Validate section orders
        var orders = templateConfig.Sections?.Select(s => s.Order).ToList() ?? new List<int>();
        if (orders.Count != orders.Distinct().Count())
            errors.Add("Section orders must be unique");
        
        // Validate field mappings
        var availableFields = await GetAvailableFieldMappingsAsync();
        foreach (var section in templateConfig.Sections)
        {
            foreach (var field in section.Fields)
            {
                if (!string.IsNullOrEmpty(field.DataSource) && !availableFields.ContainsKey(field.DataSource))
                {
                    errors.Add($"Invalid field mapping: {field.DataSource}");
                }
            }
        }
        
        return (errors.Count == 0, errors);
    }

    public async Task<PayslipTemplateConfig> GetDefaultTemplateConfigAsync()
    {
        return new PayslipTemplateConfig
        {
            Layout = new PayslipLayout
            {
                Orientation = "portrait",
                PageSize = "A4",
                Margins = new PayslipMargins { Top = 20, Right = 20, Bottom = 20, Left = 20 },
                Columns = 1
            },
            Sections = new List<PayslipSection>
            {
                new PayslipSection
                {
                    Id = "header",
                    Name = "Header",
                    Type = "header",
                    Order = 1,
                    IsVisible = true,
                    Fields = new List<PayslipField>
                    {
                        new PayslipField { Id = "org-logo", Name = "Organization Logo", DataSource = "organization.logo", Format = "image" },
                        new PayslipField { Id = "org-name", Name = "Organization Name", DataSource = "organization.name", Format = "text" }
                    }
                },
                new PayslipSection
                {
                    Id = "employee-info",
                    Name = "Employee Information",
                    Type = "employee-info",
                    Order = 2,
                    IsVisible = true,
                    Fields = new List<PayslipField>
                    {
                        new PayslipField { Id = "emp-name", Name = "Employee Name", DataSource = "employee.name", Format = "text" },
                        new PayslipField { Id = "emp-id", Name = "Employee ID", DataSource = "employee.id", Format = "text" },
                        new PayslipField { Id = "designation", Name = "Designation", DataSource = "employee.designation", Format = "text" },
                        new PayslipField { Id = "department", Name = "Department", DataSource = "employee.department", Format = "text" }
                    }
                },
                new PayslipSection
                {
                    Id = "earnings",
                    Name = "Earnings",
                    Type = "earnings",
                    Order = 3,
                    IsVisible = true,
                    Fields = new List<PayslipField>
                    {
                        new PayslipField { Id = "basic-salary", Name = "Basic Salary", DataSource = "salary.basic", Format = "currency" },
                        new PayslipField { Id = "allowances", Name = "Total Allowances", DataSource = "allowances.total", Format = "currency" },
                        new PayslipField { Id = "overtime", Name = "Overtime Amount", DataSource = "overtime.amount", Format = "currency" }
                    }
                },
                new PayslipSection
                {
                    Id = "deductions",
                    Name = "Deductions",
                    Type = "deductions",
                    Order = 4,
                    IsVisible = true,
                    Fields = new List<PayslipField>
                    {
                        new PayslipField { Id = "tax", Name = "Income Tax", DataSource = "deductions.tax", Format = "currency" },
                        new PayslipField { Id = "pf", Name = "Provident Fund", DataSource = "deductions.pf", Format = "currency" },
                        new PayslipField { Id = "total-deductions", Name = "Total Deductions", DataSource = "deductions.total", Format = "currency" }
                    }
                },
                new PayslipSection
                {
                    Id = "summary",
                    Name = "Summary",
                    Type = "summary",
                    Order = 5,
                    IsVisible = true,
                    Fields = new List<PayslipField>
                    {
                        new PayslipField { Id = "gross-salary", Name = "Gross Salary", DataSource = "salary.gross", Format = "currency" },
                        new PayslipField { Id = "net-salary", Name = "Net Salary", DataSource = "salary.net", Format = "currency" }
                    }
                },
                new PayslipSection
                {
                    Id = "footer",
                    Name = "Footer",
                    Type = "footer",
                    Order = 6,
                    IsVisible = true,
                    Fields = new List<PayslipField>()
                }
            }
        };
    }

    private string GenerateStyleSheet(PayslipTemplateDto template)
    {
        return $@"
        <style>
            body {{
                font-family: {template.StylingConfig.FontFamily}, Arial, sans-serif;
                font-size: {template.StylingConfig.FontSize}px;
                color: #333;
                margin: 0;
                padding: 20px;
                background-color: #fff;
            }}
            
            .section {{
                margin-bottom: 20px;
                padding: 15px;
                border: 1px solid #e0e0e0;
                border-radius: 5px;
            }}
            
            .section-header {{
                background-color: {template.StylingConfig.PrimaryColor};
                color: white;
                text-align: center;
                padding: 20px;
                margin-bottom: 0;
            }}
            
            .section-employee-info,
            .section-earnings,
            .section-deductions {{
                background-color: #f9f9f9;
            }}
            
            .section-summary {{
                background-color: {template.StylingConfig.PrimaryColor}20;
                border: 2px solid {template.StylingConfig.PrimaryColor};
            }}
            
            .field-row {{
                display: flex;
                justify-content: space-between;
                padding: 8px 0;
                border-bottom: 1px solid #eee;
            }}
            
            .field-label {{
                font-weight: bold;
                color: {template.StylingConfig.SecondaryColor};
            }}
            
            .field-value {{
                text-align: right;
            }}
            
            .currency {{
                font-weight: bold;
                color: {template.StylingConfig.PrimaryColor};
            }}
            
            .total-row {{
                font-weight: bold;
                font-size: 1.1em;
                border-top: 2px solid {template.StylingConfig.PrimaryColor};
                padding-top: 10px;
                margin-top: 10px;
            }}
            
            .footer {{
                text-align: center;
                color: {template.StylingConfig.SecondaryColor};
                font-size: 0.9em;
                margin-top: 30px;
            }}
        </style>";
    }

    private string GenerateHeaderSection(PayslipTemplateDto template, PayrollCalculationResult payrollData)
    {
        var html = new StringBuilder();
        
        if (template.HeaderConfig.ShowOrganizationLogo)
        {
            html.AppendLine("<div style='text-align: center; margin-bottom: 10px;'>");
            html.AppendLine("<img src='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==' alt='Logo' style='height: 60px;' />");
            html.AppendLine("</div>");
        }
        
        html.AppendLine($"<h1 style='text-align: center; margin: 0; color: white;'>{template.HeaderConfig.HeaderText}</h1>");
        html.AppendLine($"<h2 style='text-align: center; margin: 10px 0 0 0; color: white;'>PAYSLIP</h2>");
        html.AppendLine($"<p style='text-align: center; margin: 5px 0 0 0; color: white;'>For the month of {GetMonthName(payrollData.PayrollMonth)} {payrollData.PayrollYear}</p>");
        
        return html.ToString();
    }

    private string GenerateEmployeeInfoSection(PayslipTemplateDto template, PayrollCalculationResult payrollData, PayslipSection section)
    {
        var html = new StringBuilder();
        html.AppendLine("<h3>Employee Information</h3>");
        
        foreach (var field in section.Fields.Where(f => f.IsVisible))
        {
            var value = GetFieldValue(field.DataSource, payrollData);
            html.AppendLine($"<div class='field-row'>");
            html.AppendLine($"<span class='field-label'>{field.Label}:</span>");
            html.AppendLine($"<span class='field-value'>{value}</span>");
            html.AppendLine("</div>");
        }
        
        return html.ToString();
    }

    private string GenerateEarningsSection(PayslipTemplateDto template, PayrollCalculationResult payrollData, PayslipSection section)
    {
        var html = new StringBuilder();
        html.AppendLine("<h3>Earnings</h3>");
        
        foreach (var field in section.Fields.Where(f => f.IsVisible))
        {
            var value = GetFieldValue(field.DataSource, payrollData);
            var formattedValue = field.Format == "currency" ? FormatCurrency(decimal.Parse(value), payrollData.Currency) : value;
            
            html.AppendLine($"<div class='field-row'>");
            html.AppendLine($"<span class='field-label'>{field.Label}:</span>");
            html.AppendLine($"<span class='field-value currency'>{formattedValue}</span>");
            html.AppendLine("</div>");
        }
        
        return html.ToString();
    }

    private string GenerateDeductionsSection(PayslipTemplateDto template, PayrollCalculationResult payrollData, PayslipSection section)
    {
        var html = new StringBuilder();
        html.AppendLine("<h3>Deductions</h3>");
        
        foreach (var field in section.Fields.Where(f => f.IsVisible))
        {
            var value = GetFieldValue(field.DataSource, payrollData);
            var formattedValue = field.Format == "currency" ? FormatCurrency(decimal.Parse(value), payrollData.Currency) : value;
            
            html.AppendLine($"<div class='field-row'>");
            html.AppendLine($"<span class='field-label'>{field.Label}:</span>");
            html.AppendLine($"<span class='field-value currency'>{formattedValue}</span>");
            html.AppendLine("</div>");
        }
        
        return html.ToString();
    }

    private string GenerateSummarySection(PayslipTemplateDto template, PayrollCalculationResult payrollData, PayslipSection section)
    {
        var html = new StringBuilder();
        html.AppendLine("<h3>Summary</h3>");
        
        html.AppendLine($"<div class='field-row'>");
        html.AppendLine($"<span class='field-label'>Gross Salary:</span>");
        html.AppendLine($"<span class='field-value currency'>{FormatCurrency(payrollData.GrossSalary, payrollData.Currency)}</span>");
        html.AppendLine("</div>");
        
        html.AppendLine($"<div class='field-row total-row'>");
        html.AppendLine($"<span class='field-label'>Net Salary:</span>");
        html.AppendLine($"<span class='field-value currency'>{FormatCurrency(payrollData.NetSalary, payrollData.Currency)}</span>");
        html.AppendLine("</div>");
        
        return html.ToString();
    }

    private string GenerateFooterSection(PayslipTemplateDto template, PayrollCalculationResult payrollData)
    {
        var html = new StringBuilder();
        
        html.AppendLine("<div class='footer'>");
        html.AppendLine($"<p>{template.FooterConfig.FooterText}</p>");
        
        if (template.FooterConfig.ShowDigitalSignature)
        {
            html.AppendLine("<p>This is a computer-generated payslip and does not require a physical signature.</p>");
        }
        
        html.AppendLine($"<p>Generated on: {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
        html.AppendLine("</div>");
        
        return html.ToString();
    }

    private string GetFieldValue(string dataSource, PayrollCalculationResult payrollData)
    {
        return dataSource switch
        {
            "employee.name" => payrollData.EmployeeName,
            "employee.id" => payrollData.EmployeeId.ToString(),
            "salary.basic" => payrollData.BasicSalary.ToString("F2"),
            "salary.gross" => payrollData.GrossSalary.ToString("F2"),
            "salary.net" => payrollData.NetSalary.ToString("F2"),
            "salary.currency" => payrollData.Currency,
            "allowances.total" => payrollData.TotalAllowances.ToString("F2"),
            "deductions.total" => payrollData.TotalDeductions.ToString("F2"),
            "overtime.amount" => payrollData.OvertimeAmount.ToString("F2"),
            "payroll.month" => GetMonthName(payrollData.PayrollMonth),
            "payroll.year" => payrollData.PayrollYear.ToString(),
            _ => ""
        };
    }

    private string FormatCurrency(decimal amount, string currency)
    {
        return $"{currency} {amount:N2}";
    }

    private string GetMonthName(int month)
    {
        return month switch
        {
            1 => "January", 2 => "February", 3 => "March", 4 => "April",
            5 => "May", 6 => "June", 7 => "July", 8 => "August",
            9 => "September", 10 => "October", 11 => "November", 12 => "December",
            _ => "Unknown"
        };
    }

    private async Task<byte[]> ConvertHtmlToPdfAsync(string htmlContent)
    {
        // This is a simplified implementation
        // In a real application, you would use a library like:
        // - PuppeteerSharp
        // - iTextSharp
        // - DinkToPdf
        // - SelectPdf
        
        // For now, we'll just convert the HTML to bytes as a placeholder
        return Encoding.UTF8.GetBytes(htmlContent);
    }
}