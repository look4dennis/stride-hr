# StrideHR Backend

A comprehensive Human Resource Management System built with .NET 8, Entity Framework Core, and MySQL.

## Architecture

The solution follows Clean Architecture principles with clear separation of concerns:

- **StrideHR.API**: Web API layer (controllers, middleware, configuration)
- **StrideHR.Core**: Domain layer (entities, interfaces, business logic)
- **StrideHR.Infrastructure**: Data access layer (repositories, DbContext, external services)
- **StrideHR.Tests**: Unit and integration tests

## Technology Stack

- **.NET 8**: Latest version of .NET for high performance
- **Entity Framework Core 8**: ORM for database operations
- **MySQL 8.0**: Primary database with Pomelo provider
- **Serilog**: Structured logging
- **AutoMapper**: Object-to-object mapping
- **FluentValidation**: Input validation
- **Swagger/OpenAPI**: API documentation
- **xUnit**: Unit testing framework
- **Moq**: Mocking framework for tests

## Getting Started

### Prerequisites

- .NET 8 SDK
- MySQL 8.0+ (or use Docker Compose)
- Visual Studio 2022 or VS Code

### Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd StrideHR/backend
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update connection string**
   - Edit `appsettings.Development.json`
   - Update the `DefaultConnection` string to match your MySQL setup

4. **Run database migrations** (when available)
   ```bash
   dotnet ef database update --project src/StrideHR.Infrastructure --startup-project src/StrideHR.API
   ```

5. **Build the solution**
   ```bash
   dotnet build
   ```

6. **Run tests**
   ```bash
   dotnet test
   ```

7. **Start the API**
   ```bash
   cd src/StrideHR.API
   dotnet run
   ```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `http://localhost:5000` (in development)

### Docker Development

Use Docker Compose for a complete development environment:

```bash
# From the root directory
docker-compose up -d
```

This will start:
- MySQL database on port 3306
- Redis cache on port 6379
- API on port 5000
- Frontend on port 4200

## Project Structure

```
backend/
├── src/
│   ├── StrideHR.API/          # Web API controllers and configuration
│   │   ├── Controllers/       # API controllers
│   │   ├── Middleware/        # Custom middleware
│   │   ├── Extensions/        # Service registration extensions
│   │   └── Configuration/     # Configuration classes
│   ├── StrideHR.Core/         # Domain layer
│   │   ├── Entities/          # Domain entities
│   │   ├── Interfaces/        # Repository and service contracts
│   │   └── Enums/            # Domain enumerations
│   └── StrideHR.Infrastructure/ # Data access layer
│       ├── Data/             # DbContext and configurations
│       ├── Repositories/     # Repository implementations
│       └── Services/         # Service implementations
├── tests/
│   └── StrideHR.Tests/       # Unit and integration tests
├── docker-compose.yml        # Docker development environment
└── Dockerfile               # API container definition
```

## Key Features Implemented

### Core Infrastructure
- ✅ Clean Architecture with proper separation of concerns
- ✅ Entity Framework Core with MySQL integration
- ✅ Repository pattern with Unit of Work
- ✅ Dependency injection configuration
- ✅ Global exception handling middleware
- ✅ Structured logging with Serilog
- ✅ Comprehensive unit testing setup

### Domain Entities
- ✅ Base entity with audit fields and soft delete
- ✅ Organization and Branch entities for multi-tenancy
- ✅ Employee entity with hierarchical relationships
- ✅ Attendance and break tracking entities
- ✅ Role-based access control entities
- ✅ Shift management entities

### Services
- ✅ Employee management service
- ✅ Attendance tracking service
- ✅ Repository pattern implementation
- ✅ Unit of Work pattern

### API Features
- ✅ RESTful API design
- ✅ Swagger/OpenAPI documentation
- ✅ CORS configuration
- ✅ Health check endpoint
- ✅ Standardized API responses

## Configuration

### Database Connection
Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StrideHR;User=root;Password=password;Port=3306;"
  }
}
```

### JWT Settings
Configure JWT authentication in `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "StrideHR",
    "Audience": "StrideHR-Users",
    "ExpirationHours": 24
  }
}
```

### Logging
Serilog is configured to write to both console and file. Logs are stored in the `logs/` directory.

## Testing

The project includes comprehensive unit tests using xUnit, Moq, and FluentAssertions.

Run all tests:
```bash
dotnet test
```

Run tests with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Next Steps

This foundation provides:
1. ✅ Solid architectural foundation
2. ✅ Database setup with Entity Framework
3. ✅ Basic CRUD operations
4. ✅ Logging and error handling
5. ✅ Testing infrastructure
6. ✅ Docker development environment

Ready for implementing specific business features like:
- Authentication and authorization
- Payroll management
- Project management
- Reporting and analytics
- Real-time notifications

## Contributing

1. Follow Clean Architecture principles
2. Write unit tests for new features
3. Use proper logging
4. Follow C# coding conventions
5. Update documentation as needed