# Technology Stack & Build System

## Tech Stack

### Backend (.NET 8)
- **Framework**: .NET 8 Web API
- **ORM**: Entity Framework Core 8.0
- **Database**: MySQL 8.0+ (Pomelo.EntityFrameworkCore.MySql)
- **Caching**: Redis (StackExchange.Redis)
- **Authentication**: JWT Bearer tokens
- **Logging**: Serilog with file and console sinks
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Documentation**: Swagger/OpenAPI

### Frontend (Angular 17+)
- **Framework**: Angular 17+
- **UI Library**: Bootstrap 5 with ng-bootstrap
- **Real-time**: SignalR (@microsoft/signalr)
- **Testing**: Jasmine, Karma
- **Build**: Angular CLI

### Infrastructure
- **Containerization**: Docker & Docker Compose
- **Database**: MySQL 8.0
- **Caching**: Redis 7
- **Reverse Proxy**: Nginx/Apache (production)

## Common Commands

### Backend Development
```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run API (from src/StrideHR.API)
dotnet run

# Run tests
dotnet test

# Entity Framework migrations
dotnet ef migrations add <MigrationName>
dotnet ef database update

# Clean and rebuild
dotnet clean
dotnet build
```

### Frontend Development
```bash
# Install dependencies
npm install

# Start development server
npm start
# or
ng serve

# Build for production
npm run build
# or
ng build --configuration production

# Run tests
npm test
# or
ng test

# Lint code
npm run lint
# or
ng lint
```

### Docker Development
```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Rebuild and start
docker-compose up --build -d

# Access services:
# Frontend: http://localhost:4200
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

## Development Environment Requirements
- .NET 8 SDK
- Node.js 18+ and npm
- MySQL 8.0+
- Redis (optional for development)
- Docker & Docker Compose (recommended)
- Visual Studio Code or Visual Studio 2022