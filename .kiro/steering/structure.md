# Project Structure & Organization

## Solution Architecture

StrideHR follows Clean Architecture principles with clear separation of concerns across multiple projects:

```
StrideHR/
├── src/
│   ├── StrideHR.API/          # Web API layer (controllers, middleware, configuration)
│   ├── StrideHR.Core/         # Domain layer (entities, interfaces, business logic)
│   └── StrideHR.Infrastructure/ # Data access layer (repositories, DbContext, external services)
├── tests/
│   └── StrideHR.Tests/        # Unit and integration tests
├── frontend/                  # Angular application
├── docker-compose.yml         # Container orchestration
└── StrideHR.sln              # Visual Studio solution file
```

## Project Dependencies

- **StrideHR.API** → References Core + Infrastructure
- **StrideHR.Infrastructure** → References Core only
- **StrideHR.Core** → No dependencies (pure domain layer)
- **StrideHR.Tests** → References all projects for testing

## Key Directories

### Backend Structure
- **StrideHR.API/**: Controllers, DTOs, middleware, startup configuration
- **StrideHR.Core/Entities/**: Domain models and business entities
- **StrideHR.Core/Interfaces/**: Repository and service contracts
- **StrideHR.Infrastructure/Data/**: DbContext, configurations, migrations
- **StrideHR.Infrastructure/Repositories/**: Data access implementations

### Frontend Structure
- **frontend/src/app/**: Angular components, services, modules
- **frontend/src/assets/**: Static assets (images, styles, etc.)
- **frontend/package.json**: Dependencies and build scripts

## Architectural Patterns

### Clean Architecture Layers
1. **API Layer**: HTTP concerns, controllers, DTOs
2. **Core Layer**: Business logic, domain entities, interfaces
3. **Infrastructure Layer**: Data access, external services, implementations

### Key Principles
- **Dependency Inversion**: Core layer defines interfaces, Infrastructure implements them
- **Single Responsibility**: Each project has a clear, focused purpose
- **Separation of Concerns**: Business logic separated from data access and presentation
- **Testability**: Clean separation enables comprehensive unit testing

## Configuration Files
- **appsettings.json**: API configuration (database, JWT, Redis)
- **docker-compose.yml**: Multi-container development environment
- **package.json**: Frontend dependencies and scripts
- **.csproj files**: Project dependencies and build configuration

## Development Workflow
- Use the solution file (StrideHR.sln) for backend development
- Frontend development is independent with Angular CLI
- Docker Compose for full-stack development and testing
- Entity Framework migrations for database schema changes