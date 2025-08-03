using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace StrideHR.API.Filters;

public class SwaggerDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Add custom tags with descriptions
        swaggerDoc.Tags = new List<OpenApiTag>
        {
            new OpenApiTag
            {
                Name = "Authentication",
                Description = "User authentication and authorization endpoints"
            },
            new OpenApiTag
            {
                Name = "Employee",
                Description = "Employee management operations"
            },
            new OpenApiTag
            {
                Name = "Attendance",
                Description = "Time tracking and attendance management"
            },
            new OpenApiTag
            {
                Name = "Leave",
                Description = "Leave request and balance management"
            },
            new OpenApiTag
            {
                Name = "Payroll",
                Description = "Payroll processing and management"
            },
            new OpenApiTag
            {
                Name = "Project",
                Description = "Project and task management"
            },
            new OpenApiTag
            {
                Name = "Asset",
                Description = "Asset tracking and management"
            },
            new OpenApiTag
            {
                Name = "Training",
                Description = "Training modules and certifications"
            },
            new OpenApiTag
            {
                Name = "Performance",
                Description = "Performance reviews and improvement plans"
            },
            new OpenApiTag
            {
                Name = "Notification",
                Description = "Real-time notifications and messaging"
            },
            new OpenApiTag
            {
                Name = "Reports",
                Description = "Analytics and reporting"
            },
            new OpenApiTag
            {
                Name = "Webhook",
                Description = "Webhook subscriptions and event notifications"
            },
            new OpenApiTag
            {
                Name = "CalendarIntegration",
                Description = "Calendar integration with Google Calendar and Outlook"
            },
            new OpenApiTag
            {
                Name = "ExternalIntegration",
                Description = "External system integrations (Payroll, Accounting)"
            },
            new OpenApiTag
            {
                Name = "DataImportExport",
                Description = "Bulk data import and export operations"
            }
        };

        // Remove any paths that should not be documented
        var pathsToRemove = new List<string>();
        foreach (var path in swaggerDoc.Paths)
        {
            if (path.Key.Contains("/health") || path.Key.Contains("/internal"))
            {
                pathsToRemove.Add(path.Key);
            }
        }

        foreach (var path in pathsToRemove)
        {
            swaggerDoc.Paths.Remove(path);
        }

        // Add common response schemas
        if (swaggerDoc.Components?.Schemas != null)
        {
            swaggerDoc.Components.Schemas.Add("ApiResponse", new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["success"] = new OpenApiSchema { Type = "boolean", Description = "Indicates if the operation was successful" },
                    ["message"] = new OpenApiSchema { Type = "string", Description = "Human-readable message about the operation" },
                    ["data"] = new OpenApiSchema { Type = "object", Description = "The response data" },
                    ["errors"] = new OpenApiSchema 
                    { 
                        Type = "array", 
                        Items = new OpenApiSchema { Type = "string" },
                        Description = "List of error messages if operation failed"
                    }
                }
            });

            swaggerDoc.Components.Schemas.Add("PaginatedResponse", new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["success"] = new OpenApiSchema { Type = "boolean" },
                    ["data"] = new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Type = "object" } },
                    ["totalCount"] = new OpenApiSchema { Type = "integer", Description = "Total number of items" },
                    ["pageSize"] = new OpenApiSchema { Type = "integer", Description = "Number of items per page" },
                    ["currentPage"] = new OpenApiSchema { Type = "integer", Description = "Current page number" },
                    ["totalPages"] = new OpenApiSchema { Type = "integer", Description = "Total number of pages" }
                }
            });
        }
    }
}