# StrideHR Developer Setup Guide

This guide will help you set up the StrideHR development environment on your local machine.

## Prerequisites

### Required Software

- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+** - [Download here](https://nodejs.org/)
- **MySQL 8.0+** - [Download here](https://dev.mysql.com/downloads/mysql/)
- **Redis** (optional for development) - [Download here](https://redis.io/download)
- **Git** - [Download here](https://git-scm.com/downloads)

### Recommended Tools

- **Visual Studio 2022** or **Visual Studio Code**
- **MySQL Workbench** for database management
- **Postman** for API testing
- **Docker Desktop** (optional, for containerized development)

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/stridehr.git
cd stridehr
```

### 2. Backend Setup

#### Database Configuration

1. Create a MySQL database:
```sql
CREATE DATABASE stridehr_dev;
CREATE USER 'stridehr_user'@'localhost' IDENTIFIED BY 'your_password';
GRANT ALL PRIVILEGES ON stridehr_dev.* TO 'stridehr_user'@'localhost';
FLUSH PRIVILEGES;
```

2. Update connection string in `backend/src/StrideHR.API/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=stridehr_dev;User=stridehr_user;Password=your_password;"
  }
}
```

#### Install Dependencies and Run Migrations

```bash
cd backend
dotnet restore
dotnet ef database update --project src/StrideHR.Infrastructure --startup-project src/StrideHR.API
```

#### Run the Backend API

```bash
cd src/StrideHR.API
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `http://localhost:5000/swagger`

### 3. Frontend Setup

```bash
cd frontend
npm install
npm start
```

The frontend will be available at `http://localhost:4200`

### 4. Docker Setup (Alternative)

If you prefer using Docker:

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

## Development Workflow

### Branch Strategy

We use GitFlow branching strategy:

- `main` - Production-ready code
- `develop` - Integration branch for features
- `feature/*` - Feature development branches
- `hotfix/*` - Critical bug fixes
- `release/*` - Release preparation branches

### Creating a Feature Branch

```bash
git checkout develop
git pull origin develop
git checkout -b feature/your-feature-name
```

### Making Changes

1. Make your changes
2. Write/update tests
3. Run tests locally
4. Commit with descriptive messages
5. Push to your feature branch
6. Create a Pull Request

### Running Tests

#### Backend Tests
```bash
cd backend
dotnet test
```

#### Frontend Tests
```bash
cd frontend
npm test
```

#### Integration Tests
```bash
cd backend
dotnet test --filter Category=Integration
```

### Code Quality

#### Backend Code Analysis
```bash
dotnet format
dotnet build --verbosity normal
```

#### Frontend Code Analysis
```bash
cd frontend
npm run lint
npm run lint:fix
```

## Environment Configuration

### Backend Configuration Files

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production settings

### Key Configuration Sections

#### JWT Settings
```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-here-minimum-32-characters",
    "Issuer": "StrideHR",
    "Audience": "StrideHR-Users",
    "ExpirationHours": 24,
    "RefreshTokenExpirationDays": 7
  }
}
```

#### Database Settings
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=stridehr_dev;User=stridehr_user;Password=your_password;"
  }
}
```

#### Redis Settings (Optional)
```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "Database": 0
  }
}
```

### Frontend Configuration

Update `frontend/src/environments/environment.ts`:
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  signalRUrl: 'http://localhost:5000/hubs'
};
```

## Database Management

### Entity Framework Migrations

#### Create a New Migration
```bash
cd backend
dotnet ef migrations add YourMigrationName --project src/StrideHR.Infrastructure --startup-project src/StrideHR.API
```

#### Update Database
```bash
dotnet ef database update --project src/StrideHR.Infrastructure --startup-project src/StrideHR.API
```

#### Rollback Migration
```bash
dotnet ef database update PreviousMigrationName --project src/StrideHR.Infrastructure --startup-project src/StrideHR.API
```

### Seeding Test Data

Run the data seeder to populate your development database:
```bash
cd backend/src/StrideHR.API
dotnet run --seed-data
```

## Debugging

### Backend Debugging

#### Visual Studio
1. Set StrideHR.API as startup project
2. Press F5 to start debugging

#### Visual Studio Code
1. Open the backend folder
2. Use the provided launch configuration
3. Press F5 to start debugging

### Frontend Debugging

#### Browser DevTools
1. Open browser developer tools
2. Use Sources tab for breakpoints
3. Console tab for logging

#### VS Code Debugging
1. Install "Debugger for Chrome" extension
2. Use provided launch configuration
3. Press F5 to start debugging

## Common Issues and Solutions

### Backend Issues

#### Port Already in Use
```bash
# Find process using port 5000
netstat -ano | findstr :5000
# Kill the process
taskkill /PID <process_id> /F
```

#### Database Connection Issues
- Verify MySQL is running
- Check connection string
- Ensure database exists
- Verify user permissions

#### Migration Issues
```bash
# Reset migrations (development only)
dotnet ef database drop --project src/StrideHR.Infrastructure --startup-project src/StrideHR.API
dotnet ef database update --project src/StrideHR.Infrastructure --startup-project src/StrideHR.API
```

### Frontend Issues

#### Node Modules Issues
```bash
cd frontend
rm -rf node_modules package-lock.json
npm install
```

#### CORS Issues
- Verify backend CORS configuration
- Check frontend API URL configuration
- Ensure both services are running

## Performance Optimization

### Backend Performance

#### Database Optimization
- Use appropriate indexes
- Implement query optimization
- Use pagination for large datasets
- Consider caching for frequently accessed data

#### API Optimization
- Implement response caching
- Use compression middleware
- Optimize serialization
- Implement rate limiting

### Frontend Performance

#### Bundle Optimization
```bash
npm run build --prod
npm run analyze
```

#### Lazy Loading
- Implement route-based code splitting
- Use lazy loading for modules
- Optimize image loading

## Security Considerations

### Backend Security
- Always validate input data
- Use parameterized queries
- Implement proper authentication
- Log security events
- Keep dependencies updated

### Frontend Security
- Sanitize user input
- Implement CSP headers
- Use HTTPS in production
- Secure token storage
- Validate API responses

## Deployment

### Development Deployment
```bash
# Backend
cd backend/src/StrideHR.API
dotnet publish -c Release -o ./publish

# Frontend
cd frontend
npm run build --prod
```

### Docker Deployment
```bash
docker-compose -f docker-compose.prod.yml up -d
```

## API Documentation

### Swagger/OpenAPI Documentation

The API documentation is automatically generated using Swagger/OpenAPI and is available at:

- **Development**: `http://localhost:5000/api-docs`
- **Staging**: `https://staging-api.stridehr.com/api-docs`
- **Production**: `https://api.stridehr.com/api-docs`

### Generating API Documentation

To generate comprehensive API documentation:

```bash
# Generate XML documentation files
cd backend
dotnet build --configuration Release

# The XML files are automatically included in Swagger
# Documentation is available at /api-docs when the API is running
```

### Adding API Documentation

When creating new controllers or endpoints, follow these documentation standards:

```csharp
/// <summary>
/// Brief description of the endpoint
/// </summary>
/// <param name="parameter">Description of the parameter</param>
/// <returns>Description of what the endpoint returns</returns>
/// <response code="200">Success response description</response>
/// <response code="400">Bad request description</response>
/// <response code="401">Unauthorized description</response>
/// <response code="404">Not found description</response>
[HttpGet("{id}")]
[ProducesResponseType(typeof(ApiResponse<YourDto>), 200)]
[ProducesResponseType(typeof(object), 400)]
[ProducesResponseType(typeof(object), 401)]
[ProducesResponseType(typeof(object), 404)]
public async Task<ActionResult<ApiResponse<YourDto>>> GetById(int id)
{
    // Implementation
}
```

### Code Documentation Standards

#### XML Documentation Comments

All public classes, methods, and properties should have XML documentation:

```csharp
/// <summary>
/// Service for managing employee data and operations
/// </summary>
public class EmployeeService : IEmployeeService
{
    /// <summary>
    /// Creates a new employee in the system
    /// </summary>
    /// <param name="createEmployeeDto">Employee data for creation</param>
    /// <returns>The created employee with generated ID</returns>
    /// <exception cref="ValidationException">Thrown when employee data is invalid</exception>
    /// <exception cref="DuplicateEmailException">Thrown when email already exists</exception>
    public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto createEmployeeDto)
    {
        // Implementation
    }
}
```

#### Inline Code Comments

Use inline comments for complex business logic:

```csharp
// Calculate overtime hours based on branch-specific rules
// Standard working hours vary by country and branch configuration
var overtimeHours = totalHours > branchWorkingHours 
    ? totalHours - branchWorkingHours 
    : 0;

// Apply country-specific overtime multipliers
// Some countries have different rates for weekends vs weekdays
var overtimeRate = isWeekend ? branch.WeekendOvertimeRate : branch.WeekdayOvertimeRate;
```

## Code Quality and Standards

### Backend Code Quality Tools

```bash
# Install code analysis tools
dotnet tool install --global dotnet-format
dotnet tool install --global dotnet-outdated-tool

# Run code formatting
dotnet format

# Check for outdated packages
dotnet outdated

# Run static code analysis
dotnet build --verbosity normal
```

### Frontend Code Quality Tools

```bash
cd frontend

# Install quality tools
npm install --save-dev @angular-eslint/eslint-plugin
npm install --save-dev prettier
npm install --save-dev husky
npm install --save-dev lint-staged

# Run linting
npm run lint

# Run formatting
npm run format

# Run tests with coverage
npm run test:coverage
```

### Pre-commit Hooks

Create `.husky/pre-commit`:

```bash
#!/bin/sh
. "$(dirname "$0")/_/husky.sh"

# Backend checks
cd backend
dotnet format --verify-no-changes
dotnet test --no-build --verbosity quiet

# Frontend checks
cd ../frontend
npm run lint
npm run test:ci
```

## Getting Help

### Documentation
- [API Documentation](./API_DOCUMENTATION.md)
- [Architecture Guide](./ARCHITECTURE.md)
- [Deployment Guide](./DEPLOYMENT.md)

### Support Channels
- **Internal Team**: Slack #stridehr-dev
- **Issues**: GitHub Issues
- **Email**: dev-support@stridehr.com

### Code Review Process
1. Create feature branch
2. Implement changes with tests
3. Create Pull Request
4. Address review feedback
5. Merge after approval

## Contributing Guidelines

Please read our [Contributing Guide](./CONTRIBUTING.md) for detailed information about:
- Code style guidelines
- Testing requirements
- Documentation standards
- Pull request process