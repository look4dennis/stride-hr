# StrideHR Configuration Reference

This document provides comprehensive information about all configuration options available in StrideHR.

## Table of Contents

- [Configuration Files](#configuration-files)
- [Environment Variables](#environment-variables)
- [Database Configuration](#database-configuration)
- [Authentication & Security](#authentication--security)
- [Logging Configuration](#logging-configuration)
- [Email Configuration](#email-configuration)
- [File Storage Configuration](#file-storage-configuration)
- [Cache Configuration](#cache-configuration)
- [External Integrations](#external-integrations)
- [Performance Settings](#performance-settings)
- [Monitoring & Health Checks](#monitoring--health-checks)

## Configuration Files

StrideHR uses a hierarchical configuration system with the following precedence (highest to lowest):

1. **Environment Variables**
2. **appsettings.{Environment}.json**
3. **appsettings.json**
4. **User Secrets** (development only)

### Configuration File Locations

```
backend/src/StrideHR.API/
├── appsettings.json                    # Base configuration
├── appsettings.Development.json        # Development overrides
├── appsettings.Staging.json           # Staging overrides
├── appsettings.Production.json        # Production overrides
└── appsettings.Testing.json           # Testing overrides
```

## Environment Variables

### Core Environment Variables

```bash
# Application Environment
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5000;https://+:5001

# Database Configuration
ConnectionStrings__DefaultConnection="Server=localhost;Database=stridehr_prod;User=stridehr_user;Password=secure_password;"

# JWT Configuration
JwtSettings__SecretKey="your-super-secret-jwt-key-minimum-64-characters-long"
JwtSettings__Issuer="StrideHR-Production"
JwtSettings__Audience="StrideHR-Users"
JwtSettings__ExpirationHours=4

# Redis Configuration
Redis__ConnectionString="localhost:6379"
Redis__Database=0

# Email Configuration
EmailSettings__SmtpHost="smtp.company.com"
EmailSettings__SmtpPort=587
EmailSettings__SmtpUsername="noreply@company.com"
EmailSettings__SmtpPassword="smtp_password"
EmailSettings__FromEmail="noreply@company.com"
EmailSettings__FromName="StrideHR System"

# File Storage
FileStorage__BasePath="/var/stridehr/uploads"
FileStorage__MaxFileSizeMB=10
FileStorage__AllowedExtensions="jpg,jpeg,png,pdf,doc,docx,xls,xlsx"

# External API Keys
OpenWeatherMap__ApiKey="your_openweather_api_key"
GoogleCalendar__ClientId="your_google_client_id"
GoogleCalendar__ClientSecret="your_google_client_secret"

# Monitoring
ApplicationInsights__InstrumentationKey="your_app_insights_key"
Sentry__Dsn="your_sentry_dsn"
```

## Database Configuration

### Connection String Format

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server={host};Port={port};Database={database};User={username};Password={password};SslMode={ssl_mode};CharSet=utf8mb4;"
  }
}
```

### Database Settings

```json
{
  "DatabaseSettings": {
    "CommandTimeout": 30,
    "EnableRetryOnFailure": true,
    "MaxRetryCount": 3,
    "MaxRetryDelay": "00:00:30",
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "EnableServiceProviderCaching": true,
    "MigrationsAssembly": "StrideHR.Infrastructure"
  }
}
```

### Connection Pool Settings

```json
{
  "ConnectionPoolSettings": {
    "MaxPoolSize": 100,
    "MinPoolSize": 5,
    "ConnectionIdleTimeout": 300,
    "ConnectionLifetime": 0
  }
}
```

## Authentication & Security

### JWT Configuration

```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-minimum-64-characters-long",
    "Issuer": "StrideHR",
    "Audience": "StrideHR-Users",
    "ExpirationHours": 24,
    "RefreshTokenExpirationDays": 7,
    "ClockSkewMinutes": 5,
    "ValidateIssuer": true,
    "ValidateAudience": true,
    "ValidateLifetime": true,
    "ValidateIssuerSigningKey": true,
    "RequireExpirationTime": true,
    "RequireSignedTokens": true
  }
}
```

### Password Policy

```json
{
  "PasswordPolicy": {
    "RequiredLength": 8,
    "RequireDigit": true,
    "RequireLowercase": true,
    "RequireUppercase": true,
    "RequireNonAlphanumeric": true,
    "RequiredUniqueChars": 6,
    "MaxFailedAccessAttempts": 5,
    "DefaultLockoutTimeSpan": "00:15:00",
    "PasswordHistoryLimit": 5,
    "PasswordExpirationDays": 90
  }
}
```

### Security Headers

```json
{
  "SecurityHeaders": {
    "EnableHsts": true,
    "HstsMaxAge": 31536000,
    "EnableXFrameOptions": true,
    "XFrameOptionsValue": "DENY",
    "EnableXContentTypeOptions": true,
    "EnableReferrerPolicy": true,
    "ReferrerPolicyValue": "strict-origin-when-cross-origin",
    "EnableContentSecurityPolicy": true,
    "ContentSecurityPolicyValue": "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';"
  }
}
```

### Encryption Settings

```json
{
  "EncryptionSettings": {
    "Key": "32-character-encryption-key-here",
    "IV": "16-character-iv-here",
    "Algorithm": "AES",
    "KeySize": 256,
    "BlockSize": 128
  }
}
```

## Logging Configuration

### Serilog Configuration

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.File", "Serilog.Sinks.Console", "Serilog.Sinks.ApplicationInsights"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "Microsoft.EntityFrameworkCore": "Warning",
        "System": "Warning",
        "StrideHR": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/stridehr-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "fileSizeLimitBytes": 10485760,
          "rollOnFileSizeLimit": true,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "ApplicationInsights",
        "Args": {
          "restrictedToMinimumLevel": "Warning",
          "telemetryConverter": "Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters.TraceTelemetryConverter, Serilog.Sinks.ApplicationInsights"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"],
    "Properties": {
      "Application": "StrideHR"
    }
  }
}
```

### Log Levels by Environment

#### Development
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "Microsoft.EntityFrameworkCore": "Information"
      }
    }
  }
}
```

#### Production
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "StrideHR": "Information",
        "Microsoft": "Error",
        "System": "Error"
      }
    }
  }
}
```

## Email Configuration

### SMTP Settings

```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.company.com",
    "SmtpPort": 587,
    "SmtpUsername": "noreply@company.com",
    "SmtpPassword": "smtp_password",
    "EnableSsl": true,
    "UseDefaultCredentials": false,
    "FromEmail": "noreply@company.com",
    "FromName": "StrideHR System",
    "ReplyToEmail": "support@company.com",
    "BccEmail": "admin@company.com",
    "Timeout": 30000,
    "MaxRetryAttempts": 3,
    "RetryDelay": 5000
  }
}
```

### Email Templates

```json
{
  "EmailTemplates": {
    "BasePath": "Templates/Email",
    "DefaultLanguage": "en",
    "SupportedLanguages": ["en", "es", "fr", "de"],
    "Templates": {
      "WelcomeEmail": {
        "Subject": "Welcome to StrideHR",
        "Template": "welcome.html"
      },
      "PasswordReset": {
        "Subject": "Password Reset Request",
        "Template": "password-reset.html"
      },
      "LeaveApproval": {
        "Subject": "Leave Request Approved",
        "Template": "leave-approval.html"
      }
    }
  }
}
```

## File Storage Configuration

### Local File Storage

```json
{
  "FileStorage": {
    "Provider": "Local",
    "BasePath": "/var/stridehr/uploads",
    "MaxFileSizeMB": 10,
    "AllowedExtensions": ["jpg", "jpeg", "png", "gif", "pdf", "doc", "docx", "xls", "xlsx", "ppt", "pptx"],
    "AllowedMimeTypes": [
      "image/jpeg",
      "image/png",
      "image/gif",
      "application/pdf",
      "application/msword",
      "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    ],
    "EnableVirusScanning": false,
    "CreateThumbnails": true,
    "ThumbnailSizes": [150, 300, 600],
    "EnableCompression": true,
    "CompressionQuality": 85
  }
}
```

### Cloud Storage (AWS S3)

```json
{
  "FileStorage": {
    "Provider": "S3",
    "S3Settings": {
      "BucketName": "stridehr-uploads",
      "Region": "us-east-1",
      "AccessKey": "your-access-key",
      "SecretKey": "your-secret-key",
      "UseHttps": true,
      "EnableServerSideEncryption": true,
      "ServerSideEncryptionMethod": "AES256",
      "CdnDomain": "cdn.stridehr.com"
    }
  }
}
```

### Azure Blob Storage

```json
{
  "FileStorage": {
    "Provider": "AzureBlob",
    "AzureBlobSettings": {
      "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=stridehr;AccountKey=your-key;EndpointSuffix=core.windows.net",
      "ContainerName": "uploads",
      "EnableCdn": true,
      "CdnEndpoint": "https://stridehr.azureedge.net"
    }
  }
}
```

## Cache Configuration

### Redis Configuration

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "Database": 0,
    "Password": "redis_password",
    "ConnectTimeout": 5000,
    "SyncTimeout": 5000,
    "ResponseTimeout": 5000,
    "ConnectRetry": 3,
    "KeepAlive": 60,
    "AbortOnConnectFail": false,
    "AllowAdmin": false,
    "ChannelPrefix": "stridehr:",
    "DefaultDatabase": 0,
    "KeyPrefix": "stridehr:",
    "SlidingExpiration": "01:00:00",
    "AbsoluteExpiration": "24:00:00"
  }
}
```

### Memory Cache Configuration

```json
{
  "MemoryCache": {
    "SizeLimit": 1024,
    "CompactionPercentage": 0.25,
    "ExpirationScanFrequency": "00:05:00",
    "DefaultSlidingExpiration": "00:30:00",
    "DefaultAbsoluteExpiration": "02:00:00"
  }
}
```

## External Integrations

### Google Calendar Integration

```json
{
  "GoogleCalendar": {
    "ClientId": "your-google-client-id",
    "ClientSecret": "your-google-client-secret",
    "RedirectUri": "https://your-domain.com/auth/google/callback",
    "Scopes": [
      "https://www.googleapis.com/auth/calendar",
      "https://www.googleapis.com/auth/calendar.events"
    ],
    "ApplicationName": "StrideHR",
    "TimeZone": "UTC"
  }
}
```

### Microsoft Graph (Outlook) Integration

```json
{
  "MicrosoftGraph": {
    "ClientId": "your-azure-app-client-id",
    "ClientSecret": "your-azure-app-client-secret",
    "TenantId": "your-azure-tenant-id",
    "RedirectUri": "https://your-domain.com/auth/microsoft/callback",
    "Scopes": [
      "https://graph.microsoft.com/Calendars.ReadWrite",
      "https://graph.microsoft.com/User.Read"
    ]
  }
}
```

### OpenWeatherMap Integration

```json
{
  "OpenWeatherMap": {
    "ApiKey": "your-openweather-api-key",
    "BaseUrl": "https://api.openweathermap.org/data/2.5",
    "Units": "metric",
    "Language": "en",
    "CacheExpirationMinutes": 30
  }
}
```

### Webhook Configuration

```json
{
  "WebhookSettings": {
    "MaxRetryAttempts": 5,
    "RetryDelaySeconds": [1, 5, 15, 60, 300],
    "TimeoutSeconds": 30,
    "MaxPayloadSizeKB": 1024,
    "EnableSignatureValidation": true,
    "SignatureHeader": "X-StrideHR-Signature",
    "SignatureAlgorithm": "HMAC-SHA256",
    "UserAgent": "StrideHR-Webhook/1.0"
  }
}
```

## Performance Settings

### API Rate Limiting

```json
{
  "RateLimiting": {
    "EnableRateLimiting": true,
    "GeneralRules": {
      "PermitLimit": 1000,
      "Window": "01:00:00",
      "ReplenishmentPeriod": "00:01:00",
      "TokensPerPeriod": 100,
      "QueueLimit": 100
    },
    "AuthenticationRules": {
      "PermitLimit": 10,
      "Window": "00:01:00",
      "ReplenishmentPeriod": "00:01:00",
      "TokensPerPeriod": 1
    },
    "BulkOperationRules": {
      "PermitLimit": 100,
      "Window": "01:00:00",
      "ReplenishmentPeriod": "00:10:00",
      "TokensPerPeriod": 10
    }
  }
}
```

### Response Compression

```json
{
  "ResponseCompression": {
    "EnableForHttps": true,
    "Providers": ["Brotli", "Gzip"],
    "MimeTypes": [
      "text/plain",
      "text/css",
      "application/javascript",
      "text/html",
      "application/xml",
      "text/xml",
      "application/json",
      "text/json"
    ],
    "Level": "Optimal"
  }
}
```

### Response Caching

```json
{
  "ResponseCaching": {
    "MaximumBodySize": 67108864,
    "UseCaseSensitivePaths": false,
    "SizeLimit": 104857600,
    "DefaultCacheProfiles": {
      "Default": {
        "Duration": 300,
        "Location": "Any",
        "NoStore": false,
        "VaryByHeader": "Accept-Encoding"
      },
      "StaticFiles": {
        "Duration": 86400,
        "Location": "Any",
        "NoStore": false
      }
    }
  }
}
```

## Monitoring & Health Checks

### Health Check Configuration

```json
{
  "HealthChecks": {
    "Enabled": true,
    "DetailedErrors": false,
    "Checks": {
      "Database": {
        "Enabled": true,
        "Timeout": "00:00:30",
        "Tags": ["database", "sql"]
      },
      "Redis": {
        "Enabled": true,
        "Timeout": "00:00:10",
        "Tags": ["cache", "redis"]
      },
      "ExternalAPIs": {
        "Enabled": true,
        "Timeout": "00:00:15",
        "Tags": ["external", "api"]
      }
    }
  }
}
```

### Application Insights

```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key",
    "EnableAdaptiveSampling": true,
    "EnableQuickPulseMetricStream": true,
    "EnableAuthenticationTrackingJavaScript": false,
    "EnableHeartbeat": true,
    "AddAutoCollectedMetricExtractor": true,
    "RequestCollectionOptions": {
      "TrackExceptions": true,
      "EnableW3CDistributedTracing": true
    },
    "TelemetryProcessors": [
      {
        "Type": "Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel.AdaptiveSamplingTelemetryProcessor",
        "Settings": {
          "MaxTelemetryItemsPerSecond": 5,
          "SamplingPercentageIncreaseTimeout": "00:02:00",
          "SamplingPercentageDecreaseTimeout": "00:02:00",
          "EvaluationInterval": "00:00:15",
          "InitialSamplingPercentage": 100
        }
      }
    ]
  }
}
```

### Sentry Configuration

```json
{
  "Sentry": {
    "Dsn": "your-sentry-dsn",
    "Environment": "production",
    "Release": "1.0.0",
    "Debug": false,
    "AttachStacktrace": true,
    "SendDefaultPii": false,
    "MaxBreadcrumbs": 100,
    "SampleRate": 1.0,
    "MaxQueueItems": 30,
    "ShutdownTimeout": "00:00:02",
    "BeforeSend": "Sentry.SentryOptions.BeforeSendTransaction",
    "BeforeBreadcrumb": "Sentry.SentryOptions.BeforeBreadcrumb"
  }
}
```

## Environment-Specific Configurations

### Development Environment

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "DetailedErrors": true,
  "EnableSensitiveDataLogging": true,
  "EnableDeveloperExceptionPage": true,
  "EnableSwagger": true,
  "CorsPolicy": {
    "AllowAnyOrigin": true,
    "AllowAnyMethod": true,
    "AllowAnyHeader": true
  }
}
```

### Staging Environment

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "StrideHR": "Debug"
    }
  },
  "DetailedErrors": false,
  "EnableSensitiveDataLogging": false,
  "EnableSwagger": true,
  "RequireHttps": true,
  "CorsPolicy": {
    "AllowedOrigins": ["https://staging.stridehr.com"],
    "AllowCredentials": true
  }
}
```

### Production Environment

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error",
      "StrideHR": "Information"
    }
  },
  "DetailedErrors": false,
  "EnableSensitiveDataLogging": false,
  "EnableSwagger": false,
  "RequireHttps": true,
  "UseHsts": true,
  "CorsPolicy": {
    "AllowedOrigins": ["https://stridehr.com", "https://app.stridehr.com"],
    "AllowCredentials": true,
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowedHeaders": ["Content-Type", "Authorization"]
  }
}
```

## Configuration Validation

### Startup Validation

```csharp
// In Program.cs or Startup.cs
services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
services.Configure<FileStorageSettings>(configuration.GetSection("FileStorage"));

// Add validation
services.AddOptions<JwtSettings>()
    .Bind(configuration.GetSection("JwtSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

services.AddOptions<EmailSettings>()
    .Bind(configuration.GetSection("EmailSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

### Configuration Models with Validation

```csharp
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    [Required]
    [MinLength(64)]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 168)]
    public int ExpirationHours { get; set; } = 24;

    [Range(1, 30)]
    public int RefreshTokenExpirationDays { get; set; } = 7;

    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public bool ValidateIssuerSigningKey { get; set; } = true;
}
```

## Configuration Best Practices

### Security Best Practices

1. **Never store secrets in configuration files**
   - Use environment variables or secure key vaults
   - Use Azure Key Vault, AWS Secrets Manager, or similar services

2. **Use different configurations per environment**
   - Separate development, staging, and production settings
   - Use environment-specific connection strings and API keys

3. **Validate configuration on startup**
   - Use data annotations and custom validation
   - Fail fast if configuration is invalid

4. **Encrypt sensitive configuration sections**
   - Use ASP.NET Core Data Protection for sensitive data
   - Consider using external key management services

### Performance Best Practices

1. **Use configuration binding**
   - Bind configuration sections to strongly-typed classes
   - Use IOptions<T> pattern for dependency injection

2. **Cache configuration values**
   - Avoid reading configuration repeatedly
   - Use IOptionsSnapshot<T> for scoped configuration changes

3. **Minimize configuration complexity**
   - Keep configuration files simple and focused
   - Use reasonable defaults where possible

### Maintenance Best Practices

1. **Document all configuration options**
   - Provide clear descriptions and examples
   - Include valid value ranges and formats

2. **Use configuration validation**
   - Validate configuration values at startup
   - Provide clear error messages for invalid configuration

3. **Version your configuration**
   - Track configuration changes in version control
   - Use configuration migration strategies for breaking changes

---

*This configuration reference is regularly updated. For the latest information, check the official documentation or contact the development team.*