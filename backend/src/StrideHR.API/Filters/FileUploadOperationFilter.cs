using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace StrideHR.API.Filters;

/// <summary>
/// Operation filter to handle file upload operations in Swagger documentation
/// </summary>
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasFileParameter = context.MethodInfo.GetParameters()
            .Any(p => p.ParameterType == typeof(IFormFile) || 
                     p.ParameterType == typeof(IFormFile[]) ||
                     p.ParameterType == typeof(IEnumerable<IFormFile>));

        if (!hasFileParameter)
            return;

        // Clear existing parameters for file upload operations
        operation.Parameters?.Clear();

        // Set request body for multipart/form-data
        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>()
                    }
                }
            }
        };

        var formDataSchema = operation.RequestBody.Content["multipart/form-data"].Schema;

        // Add properties for each parameter
        foreach (var parameter in context.MethodInfo.GetParameters())
        {
            if (parameter.ParameterType == typeof(IFormFile))
            {
                formDataSchema.Properties[parameter.Name!] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                    Description = "Upload file"
                };
            }
            else if (parameter.ParameterType == typeof(IFormFile[]) || 
                     parameter.ParameterType == typeof(IEnumerable<IFormFile>))
            {
                formDataSchema.Properties[parameter.Name!] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    },
                    Description = "Upload multiple files"
                };
            }
            else if (parameter.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromFormAttribute>() != null)
            {
                // Handle other form parameters
                var propertySchema = new OpenApiSchema();
                
                if (parameter.ParameterType == typeof(string))
                {
                    propertySchema.Type = "string";
                }
                else if (parameter.ParameterType == typeof(int) || parameter.ParameterType == typeof(int?))
                {
                    propertySchema.Type = "integer";
                    propertySchema.Format = "int32";
                }
                else if (parameter.ParameterType == typeof(bool) || parameter.ParameterType == typeof(bool?))
                {
                    propertySchema.Type = "boolean";
                }
                else if (parameter.ParameterType.IsEnum)
                {
                    propertySchema.Type = "string";
                    propertySchema.Enum = Enum.GetNames(parameter.ParameterType)
                        .Select(name => new Microsoft.OpenApi.Any.OpenApiString(name))
                        .Cast<Microsoft.OpenApi.Any.IOpenApiAny>()
                        .ToList();
                }
                else
                {
                    propertySchema.Type = "string";
                }

                // Set default value if parameter is optional
                if (parameter.HasDefaultValue && parameter.DefaultValue != null)
                {
                    if (parameter.ParameterType == typeof(string))
                    {
                        propertySchema.Default = new Microsoft.OpenApi.Any.OpenApiString(parameter.DefaultValue.ToString());
                    }
                    else if (parameter.ParameterType == typeof(int) || parameter.ParameterType == typeof(int?))
                    {
                        propertySchema.Default = new Microsoft.OpenApi.Any.OpenApiInteger((int)parameter.DefaultValue);
                    }
                    else if (parameter.ParameterType == typeof(bool) || parameter.ParameterType == typeof(bool?))
                    {
                        propertySchema.Default = new Microsoft.OpenApi.Any.OpenApiBoolean((bool)parameter.DefaultValue);
                    }
                }

                formDataSchema.Properties[parameter.Name!] = propertySchema;
            }
        }

        // Mark file parameters as required
        var fileParameters = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) && 
                       p.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromFormAttribute>() != null)
            .Select(p => p.Name!)
            .ToList();

        if (fileParameters.Any())
        {
            formDataSchema.Required = new HashSet<string>(fileParameters);
        }
    }
}