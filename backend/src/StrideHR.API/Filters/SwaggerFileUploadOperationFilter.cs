using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace StrideHR.API.Filters;

/// <summary>
/// Swagger operation filter to properly document file upload endpoints
/// with multipart/form-data content type and file parameter examples
/// </summary>
public class SwaggerFileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasFileParameter = context.MethodInfo.GetParameters()
            .Any(p => p.ParameterType == typeof(IFormFile) || 
                     p.ParameterType == typeof(IFormFile[]) ||
                     p.ParameterType == typeof(IEnumerable<IFormFile>) ||
                     p.ParameterType == typeof(List<IFormFile>));

        if (!hasFileParameter)
            return;

        // Remove any existing request body
        operation.RequestBody = new OpenApiRequestBody
        {
            Description = "File upload request",
            Required = true,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>(),
                        Required = new HashSet<string>()
                    }
                }
            }
        };

        var schema = operation.RequestBody.Content["multipart/form-data"].Schema;
        var parameters = context.MethodInfo.GetParameters();

        foreach (var parameter in parameters)
        {
            if (parameter.ParameterType == typeof(IFormFile))
            {
                schema.Properties[parameter.Name ?? "file"] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                    Description = GetFileParameterDescription(parameter.Name)
                };
                schema.Required.Add(parameter.Name ?? "file");
            }
            else if (parameter.ParameterType == typeof(IFormFile[]) || 
                     parameter.ParameterType == typeof(IEnumerable<IFormFile>) ||
                     parameter.ParameterType == typeof(List<IFormFile>))
            {
                schema.Properties[parameter.Name ?? "files"] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    },
                    Description = "Multiple files to upload"
                };
                schema.Required.Add(parameter.Name ?? "files");
            }
            else if (parameter.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromFormAttribute>() != null)
            {
                // Handle other form parameters
                var parameterSchema = GetSchemaForType(parameter.ParameterType);
                if (parameterSchema != null)
                {
                    schema.Properties[parameter.Name ?? "parameter"] = parameterSchema;
                    
                    // Add to required if parameter is not nullable
                    if (!IsNullableType(parameter.ParameterType) && !parameter.HasDefaultValue)
                    {
                        schema.Required.Add(parameter.Name ?? "parameter");
                    }
                }
            }
        }

        // Add specific examples and descriptions based on endpoint
        AddEndpointSpecificDocumentation(operation, context);

        // Remove parameters that are now part of the request body
        if (operation.Parameters != null)
        {
            var parametersToRemove = operation.Parameters
                .Where(p => schema.Properties.ContainsKey(p.Name))
                .ToList();

            foreach (var param in parametersToRemove)
            {
                operation.Parameters.Remove(param);
            }
        }
    }

    private static string GetFileParameterDescription(string? parameterName)
    {
        return parameterName?.ToLower() switch
        {
            "file" => "File to upload",
            "photo" => "Profile photo image file (JPEG, PNG, GIF supported, max 5MB)",
            "logo" => "Organization logo image file (JPEG, PNG supported, max 2MB)",
            "document" => "Document file (PDF, DOC, DOCX supported, max 10MB)",
            "content" => "Training content file (PDF, MP4, ZIP supported, max 50MB)",
            _ => "File to upload"
        };
    }

    private static void AddEndpointSpecificDocumentation(OpenApiOperation operation, OperationFilterContext context)
    {
        var controllerName = context.MethodInfo.DeclaringType?.Name.Replace("Controller", "");
        var actionName = context.MethodInfo.Name;

        switch ($"{controllerName}.{actionName}")
        {
            case "Employee.UploadProfilePhoto":
                operation.Summary = "Upload employee profile photo";
                operation.Description = @"
Upload a profile photo for an employee. 

**Supported formats:** JPEG, PNG, GIF
**Maximum file size:** 5MB
**Recommended dimensions:** 300x300 pixels (square aspect ratio)

The uploaded image will be automatically resized and optimized for display in the application.
";
                AddFileUploadResponses(operation, "Profile photo uploaded successfully", "profilePhotoUrl");
                break;

            case "Organization.UploadLogo":
                operation.Summary = "Upload organization logo";
                operation.Description = @"
Upload a logo for an organization.

**Supported formats:** JPEG, PNG
**Maximum file size:** 2MB
**Recommended dimensions:** 200x200 pixels or 400x100 pixels
**Background:** Transparent PNG recommended

The logo will be displayed in the application header and reports.
";
                AddFileUploadResponses(operation, "Organization logo uploaded successfully", "logoUrl");
                break;

            case "Expense.UploadDocument":
                operation.Summary = "Upload expense receipt or document";
                operation.Description = @"
Upload a receipt or supporting document for an expense claim.

**Supported formats:** PDF, JPEG, PNG, DOC, DOCX
**Maximum file size:** 10MB

Documents are used for expense verification and audit purposes.
";
                AddFileUploadResponses(operation, "Expense document uploaded successfully", "documentUrl");
                break;

            case "Training.UploadTrainingContent":
                operation.Summary = "Upload training module content";
                operation.Description = @"
Upload content files for a training module.

**Supported formats:** PDF, MP4, ZIP, PPTX, DOCX
**Maximum file size:** 50MB

Content files are used for training delivery and can include presentations, videos, and documents.
";
                AddFileUploadResponses(operation, "Training content uploaded successfully", "contentUrl");
                break;
        }
    }

    private static void AddFileUploadResponses(OpenApiOperation operation, string successMessage, string dataProperty)
    {
        operation.Responses.Clear();

        operation.Responses.Add("200", new OpenApiResponse
        {
            Description = "File uploaded successfully",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["success"] = new OpenApiSchema { Type = "boolean", Example = new Microsoft.OpenApi.Any.OpenApiBoolean(true) },
                            ["message"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString(successMessage) },
                            ["data"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("/uploads/files/example.jpg") }
                        }
                    }
                }
            }
        });

        operation.Responses.Add("400", new OpenApiResponse
        {
            Description = "Bad request - Invalid file or missing required parameters",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["success"] = new OpenApiSchema { Type = "boolean", Example = new Microsoft.OpenApi.Any.OpenApiBoolean(false) },
                            ["message"] = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("Invalid file type or file too large") },
                            ["errors"] = new OpenApiSchema 
                            { 
                                Type = "array", 
                                Items = new OpenApiSchema { Type = "string" },
                                Example = new Microsoft.OpenApi.Any.OpenApiArray
                                {
                                    new Microsoft.OpenApi.Any.OpenApiString("File size exceeds maximum limit"),
                                    new Microsoft.OpenApi.Any.OpenApiString("Unsupported file format")
                                }
                            }
                        }
                    }
                }
            }
        });

        operation.Responses.Add("401", new OpenApiResponse
        {
            Description = "Unauthorized - Invalid or missing JWT token"
        });

        operation.Responses.Add("403", new OpenApiResponse
        {
            Description = "Forbidden - Insufficient permissions to upload files"
        });

        operation.Responses.Add("404", new OpenApiResponse
        {
            Description = "Not found - Target entity does not exist"
        });

        operation.Responses.Add("413", new OpenApiResponse
        {
            Description = "Payload too large - File exceeds maximum size limit"
        });

        operation.Responses.Add("415", new OpenApiResponse
        {
            Description = "Unsupported media type - File format not supported"
        });
    }

    private static OpenApiSchema? GetSchemaForType(Type type)
    {
        if (type == typeof(string))
            return new OpenApiSchema { Type = "string" };
        
        if (type == typeof(int) || type == typeof(int?))
            return new OpenApiSchema { Type = "integer", Format = "int32" };
        
        if (type == typeof(long) || type == typeof(long?))
            return new OpenApiSchema { Type = "integer", Format = "int64" };
        
        if (type == typeof(bool) || type == typeof(bool?))
            return new OpenApiSchema { Type = "boolean" };
        
        if (type == typeof(DateTime) || type == typeof(DateTime?))
            return new OpenApiSchema { Type = "string", Format = "date-time" };
        
        if (type.IsEnum)
            return new OpenApiSchema 
            { 
                Type = "string", 
                Enum = Enum.GetNames(type).Select(name => new Microsoft.OpenApi.Any.OpenApiString(name)).Cast<Microsoft.OpenApi.Any.IOpenApiAny>().ToList()
            };

        return new OpenApiSchema { Type = "string" }; // Default fallback
    }

    private static bool IsNullableType(Type type)
    {
        return Nullable.GetUnderlyingType(type) != null || 
               !type.IsValueType || 
               type == typeof(string);
    }
}