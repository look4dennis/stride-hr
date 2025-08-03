using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using StrideHR.Core.Models.Webhooks;
using StrideHR.Core.Models.Calendar;
using StrideHR.Core.Models.Integrations;

namespace StrideHR.API.Filters;

public class SwaggerSchemaExampleFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(CreateWebhookSubscriptionDto))
        {
            schema.Example = new OpenApiObject
            {
                ["name"] = new OpenApiString("Employee Events Webhook"),
                ["url"] = new OpenApiString("https://your-app.com/webhooks/stridehr"),
                ["secret"] = new OpenApiString("your-webhook-secret-key"),
                ["events"] = new OpenApiArray
                {
                    new OpenApiString("employee.created"),
                    new OpenApiString("employee.updated"),
                    new OpenApiString("attendance.checked_in"),
                    new OpenApiString("leave.approved")
                },
                ["isActive"] = new OpenApiBoolean(true)
            };
        }
        else if (context.Type == typeof(CreateCalendarEventDto))
        {
            schema.Example = new OpenApiObject
            {
                ["title"] = new OpenApiString("Team Meeting"),
                ["description"] = new OpenApiString("Weekly team sync meeting"),
                ["startTime"] = new OpenApiString("2024-12-01T10:00:00Z"),
                ["endTime"] = new OpenApiString("2024-12-01T11:00:00Z"),
                ["location"] = new OpenApiString("Conference Room A"),
                ["isAllDay"] = new OpenApiBoolean(false),
                ["attendeeEmails"] = new OpenApiArray
                {
                    new OpenApiString("john.doe@company.com"),
                    new OpenApiString("jane.smith@company.com")
                },
                ["eventType"] = new OpenApiString("Meeting")
            };
        }
        else if (context.Type == typeof(PayrollSystemConfig))
        {
            schema.Example = new OpenApiObject
            {
                ["apiUrl"] = new OpenApiString("https://api.adp.com/hr/v2"),
                ["apiKey"] = new OpenApiString("your-api-key"),
                ["username"] = new OpenApiString("your-username"),
                ["password"] = new OpenApiString("your-password"),
                ["customFields"] = new OpenApiObject
                {
                    ["companyId"] = new OpenApiString("COMP123"),
                    ["environment"] = new OpenApiString("production")
                }
            };
        }
        else if (context.Type == typeof(AccountingSystemConfig))
        {
            schema.Example = new OpenApiObject
            {
                ["apiUrl"] = new OpenApiString("https://api.quickbooks.com/v3"),
                ["apiKey"] = new OpenApiString("your-api-key"),
                ["companyId"] = new OpenApiString("123456789"),
                ["username"] = new OpenApiString("your-username"),
                ["password"] = new OpenApiString("your-password"),
                ["accountMappings"] = new OpenApiObject
                {
                    ["salariesAccount"] = new OpenApiString("6000"),
                    ["benefitsAccount"] = new OpenApiString("6100"),
                    ["taxesAccount"] = new OpenApiString("2200")
                }
            };
        }
        else if (context.Type == typeof(PayrollExportRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["payrollPeriodStart"] = new OpenApiString("2024-11-01T00:00:00Z"),
                ["payrollPeriodEnd"] = new OpenApiString("2024-11-30T23:59:59Z"),
                ["employeeIds"] = new OpenApiArray
                {
                    new OpenApiInteger(1),
                    new OpenApiInteger(2),
                    new OpenApiInteger(3)
                },
                ["branchIds"] = new OpenApiArray
                {
                    new OpenApiInteger(1)
                },
                ["format"] = new OpenApiString("Json"),
                ["customParameters"] = new OpenApiObject
                {
                    ["includeDeductions"] = new OpenApiBoolean(true),
                    ["includeBenefits"] = new OpenApiBoolean(true)
                }
            };
        }

        // Add validation examples for common patterns
        if (schema.Properties != null)
        {
            foreach (var property in schema.Properties)
            {
                if (property.Key.ToLower().Contains("email") && property.Value.Type == "string")
                {
                    property.Value.Example = new OpenApiString("user@example.com");
                    property.Value.Format = "email";
                }
                else if (property.Key.ToLower().Contains("url") && property.Value.Type == "string")
                {
                    property.Value.Example = new OpenApiString("https://example.com");
                    property.Value.Format = "uri";
                }
                else if (property.Key.ToLower().Contains("phone") && property.Value.Type == "string")
                {
                    property.Value.Example = new OpenApiString("+1-555-123-4567");
                }
                else if (property.Key.ToLower().Contains("date") && property.Value.Type == "string")
                {
                    property.Value.Example = new OpenApiString("2024-12-01T10:00:00Z");
                    property.Value.Format = "date-time";
                }
            }
        }
    }
}