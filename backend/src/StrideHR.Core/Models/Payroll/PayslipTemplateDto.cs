namespace StrideHR.Core.Models.Payroll;

public class PayslipTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public int? BranchId { get; set; }
    public PayslipTemplateConfig TemplateConfig { get; set; } = new();
    public PayslipHeaderConfig HeaderConfig { get; set; } = new();
    public PayslipFooterConfig FooterConfig { get; set; } = new();
    public PayslipFieldConfig FieldConfig { get; set; } = new();
    public PayslipStylingConfig StylingConfig { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
}

public class PayslipTemplateConfig
{
    public List<PayslipSection> Sections { get; set; } = new();
    public PayslipLayout Layout { get; set; } = new();
}

public class PayslipSection
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "header", "employee-info", "earnings", "deductions", "summary", "footer"
    public int Order { get; set; }
    public bool IsVisible { get; set; } = true;
    public PayslipSectionStyle Style { get; set; } = new();
    public List<PayslipField> Fields { get; set; } = new();
}

public class PayslipField
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty; // Property path in payroll data
    public string Format { get; set; } = string.Empty; // "currency", "date", "text", "number"
    public bool IsVisible { get; set; } = true;
    public bool IsRequired { get; set; } = false;
    public PayslipFieldStyle Style { get; set; } = new();
}

public class PayslipLayout
{
    public string Orientation { get; set; } = "portrait"; // "portrait" or "landscape"
    public string PageSize { get; set; } = "A4"; // "A4", "Letter", etc.
    public PayslipMargins Margins { get; set; } = new();
    public int Columns { get; set; } = 1;
}

public class PayslipMargins
{
    public int Top { get; set; } = 20;
    public int Right { get; set; } = 20;
    public int Bottom { get; set; } = 20;
    public int Left { get; set; } = 20;
}

public class PayslipSectionStyle
{
    public string BackgroundColor { get; set; } = "#ffffff";
    public string BorderColor { get; set; } = "#e5e7eb";
    public int BorderWidth { get; set; } = 1;
    public int Padding { get; set; } = 10;
    public int Margin { get; set; } = 5;
}

public class PayslipFieldStyle
{
    public string FontFamily { get; set; } = "Inter";
    public int FontSize { get; set; } = 12;
    public string FontWeight { get; set; } = "normal"; // "normal", "bold"
    public string Color { get; set; } = "#1f2937";
    public string Alignment { get; set; } = "left"; // "left", "center", "right"
}

public class PayslipHeaderConfig
{
    public bool ShowOrganizationLogo { get; set; } = true;
    public string HeaderText { get; set; } = string.Empty;
    public string HeaderColor { get; set; } = "#3b82f6";
    public PayslipFieldStyle HeaderStyle { get; set; } = new();
}

public class PayslipFooterConfig
{
    public string FooterText { get; set; } = string.Empty;
    public bool ShowDigitalSignature { get; set; } = true;
    public PayslipFieldStyle FooterStyle { get; set; } = new();
}

public class PayslipFieldConfig
{
    public List<string> VisibleFields { get; set; } = new();
    public Dictionary<string, string> FieldLabels { get; set; } = new();
}

public class PayslipStylingConfig
{
    public string PrimaryColor { get; set; } = "#3b82f6";
    public string SecondaryColor { get; set; } = "#6b7280";
    public string FontFamily { get; set; } = "Inter";
    public int FontSize { get; set; } = 12;
}