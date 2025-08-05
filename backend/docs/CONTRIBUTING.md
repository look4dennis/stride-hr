# Contributing to StrideHR

Thank you for your interest in contributing to StrideHR! This document provides guidelines and information for contributors.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Process](#development-process)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Documentation Standards](#documentation-standards)
- [Pull Request Process](#pull-request-process)
- [Issue Reporting](#issue-reporting)

## Code of Conduct

### Our Pledge

We are committed to providing a welcoming and inclusive environment for all contributors, regardless of background, experience level, or identity.

### Expected Behavior

- Use welcoming and inclusive language
- Be respectful of differing viewpoints and experiences
- Gracefully accept constructive criticism
- Focus on what is best for the community
- Show empathy towards other community members

### Unacceptable Behavior

- Harassment, discrimination, or offensive comments
- Personal attacks or trolling
- Publishing private information without permission
- Any conduct that would be inappropriate in a professional setting

## Getting Started

### Prerequisites

Before contributing, ensure you have:
- Read the [Developer Setup Guide](./DEVELOPER_SETUP.md)
- Set up your development environment
- Familiarized yourself with the codebase structure
- Reviewed existing issues and pull requests

### Finding Issues to Work On

1. **Good First Issues**: Look for issues labeled `good-first-issue`
2. **Help Wanted**: Issues labeled `help-wanted` are ready for contribution
3. **Bug Reports**: Issues labeled `bug` that need fixing
4. **Feature Requests**: Issues labeled `enhancement` for new features

## Development Process

### Branch Strategy

We use GitFlow branching model:

```
main
├── develop
│   ├── feature/user-authentication
│   ├── feature/payroll-system
│   └── feature/attendance-tracking
├── release/v1.1.0
└── hotfix/critical-security-fix
```

### Workflow Steps

1. **Fork the Repository**
   ```bash
   git clone https://github.com/your-username/stridehr.git
   cd stridehr
   git remote add upstream https://github.com/original-org/stridehr.git
   ```

2. **Create Feature Branch**
   ```bash
   git checkout develop
   git pull upstream develop
   git checkout -b feature/your-feature-name
   ```

3. **Make Changes**
   - Write code following our standards
   - Add/update tests
   - Update documentation
   - Test your changes locally

4. **Commit Changes**
   ```bash
   git add .
   git commit -m "feat: add user authentication system"
   ```

5. **Push and Create PR**
   ```bash
   git push origin feature/your-feature-name
   ```

## Coding Standards

### Backend (.NET) Standards

#### Code Style

- Use **PascalCase** for classes, methods, and properties
- Use **camelCase** for local variables and parameters
- Use **UPPER_CASE** for constants
- Use meaningful and descriptive names

```csharp
// Good
public class EmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    
    public async Task<Employee> GetEmployeeByIdAsync(int employeeId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        return employee;
    }
}

// Bad
public class empSvc
{
    private IEmployeeRepository repo;
    
    public Employee GetEmp(int id)
    {
        return repo.GetById(id);
    }
}
```

#### Architecture Patterns

- Follow **Clean Architecture** principles
- Use **Repository Pattern** for data access
- Implement **Unit of Work** pattern
- Use **Dependency Injection** for loose coupling

```csharp
// Service Layer
public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public EmployeeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
}
```

#### Error Handling

- Use custom exceptions for business logic errors
- Implement global exception handling
- Log errors appropriately
- Return consistent error responses

```csharp
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

// In controllers
try
{
    var result = await _service.ProcessAsync(request);
    return Ok(result);
}
catch (BusinessException ex)
{
    return BadRequest(new { message = ex.Message });
}
```

#### API Documentation

- Use XML documentation comments
- Include parameter descriptions
- Document response types
- Provide example requests/responses

```csharp
/// <summary>
/// Creates a new employee in the system
/// </summary>
/// <param name="request">Employee creation request containing personal and job details</param>
/// <returns>Created employee with generated ID and system fields</returns>
/// <response code="201">Employee created successfully</response>
/// <response code="400">Invalid request data</response>
/// <response code="409">Employee with email already exists</response>
[HttpPost]
[ProducesResponseType(typeof(EmployeeDto), 201)]
[ProducesResponseType(typeof(ErrorResponse), 400)]
[ProducesResponseType(typeof(ErrorResponse), 409)]
public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
```

### Frontend (Angular) Standards

#### Code Style

- Use **camelCase** for variables and functions
- Use **PascalCase** for classes and interfaces
- Use **kebab-case** for file names and selectors
- Use **UPPER_SNAKE_CASE** for constants

```typescript
// Good
export class EmployeeListComponent implements OnInit {
  private readonly ITEMS_PER_PAGE = 20;
  
  employees: Employee[] = [];
  isLoading = false;
  
  ngOnInit(): void {
    this.loadEmployees();
  }
  
  private async loadEmployees(): Promise<void> {
    this.isLoading = true;
    try {
      this.employees = await this.employeeService.getEmployees();
    } finally {
      this.isLoading = false;
    }
  }
}
```

#### Component Structure

- Keep components focused and single-purpose
- Use OnPush change detection when possible
- Implement proper lifecycle hooks
- Use reactive forms for complex forms

```typescript
@Component({
  selector: 'app-employee-form',
  templateUrl: './employee-form.component.html',
  styleUrls: ['./employee-form.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EmployeeFormComponent implements OnInit, OnDestroy {
  employeeForm: FormGroup;
  private destroy$ = new Subject<void>();
  
  constructor(
    private fb: FormBuilder,
    private employeeService: EmployeeService,
    private cdr: ChangeDetectorRef
  ) {
    this.buildForm();
  }
  
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
```

#### Service Patterns

- Use RxJS operators effectively
- Implement proper error handling
- Cache data when appropriate
- Use interceptors for cross-cutting concerns

```typescript
@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private readonly apiUrl = `${environment.apiUrl}/employees`;
  
  constructor(private http: HttpClient) {}
  
  getEmployees(): Observable<Employee[]> {
    return this.http.get<ApiResponse<Employee[]>>(this.apiUrl).pipe(
      map(response => response.data),
      catchError(this.handleError)
    );
  }
  
  private handleError(error: HttpErrorResponse): Observable<never> {
    console.error('Employee service error:', error);
    return throwError(() => new Error('Failed to load employees'));
  }
}
```

## Testing Guidelines

### Backend Testing

#### Unit Tests

- Test business logic thoroughly
- Use AAA pattern (Arrange, Act, Assert)
- Mock external dependencies
- Aim for 80%+ code coverage

```csharp
[TestMethod]
public async Task CreateEmployee_ValidData_ReturnsEmployee()
{
    // Arrange
    var request = new CreateEmployeeRequest
    {
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@example.com"
    };
    
    _mockRepository.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Employee, bool>>>()))
                   .ReturnsAsync(false);
    
    // Act
    var result = await _service.CreateEmployeeAsync(request);
    
    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual("John", result.FirstName);
    _mockRepository.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Once);
}
```

#### Integration Tests

- Test API endpoints end-to-end
- Use test database
- Test authentication and authorization
- Verify database operations

```csharp
[TestMethod]
public async Task POST_Employee_ReturnsCreatedEmployee()
{
    // Arrange
    var request = new CreateEmployeeRequest
    {
        FirstName = "John",
        LastName = "Doe",
        Email = "john.doe@test.com"
    };
    
    // Act
    var response = await _client.PostAsJsonAsync("/api/employees", request);
    
    // Assert
    response.EnsureSuccessStatusCode();
    var employee = await response.Content.ReadFromJsonAsync<Employee>();
    Assert.AreEqual("John", employee.FirstName);
}
```

### Frontend Testing

#### Unit Tests

- Test component logic
- Mock services and dependencies
- Test user interactions
- Use TestBed for component testing

```typescript
describe('EmployeeListComponent', () => {
  let component: EmployeeListComponent;
  let fixture: ComponentFixture<EmployeeListComponent>;
  let mockEmployeeService: jasmine.SpyObj<EmployeeService>;

  beforeEach(() => {
    const spy = jasmine.createSpyObj('EmployeeService', ['getEmployees']);
    
    TestBed.configureTestingModule({
      declarations: [EmployeeListComponent],
      providers: [{ provide: EmployeeService, useValue: spy }]
    });
    
    fixture = TestBed.createComponent(EmployeeListComponent);
    component = fixture.componentInstance;
    mockEmployeeService = TestBed.inject(EmployeeService) as jasmine.SpyObj<EmployeeService>;
  });

  it('should load employees on init', () => {
    const mockEmployees = [{ id: 1, name: 'John Doe' }];
    mockEmployeeService.getEmployees.and.returnValue(of(mockEmployees));
    
    component.ngOnInit();
    
    expect(component.employees).toEqual(mockEmployees);
  });
});
```

#### E2E Tests

- Test critical user workflows
- Use Page Object Model
- Test across different browsers
- Include accessibility testing

## Documentation Standards

### Code Documentation

#### XML Documentation (Backend)
```csharp
/// <summary>
/// Calculates the total payroll amount for a given period
/// </summary>
/// <param name="organizationId">The organization identifier</param>
/// <param name="period">The payroll period to calculate</param>
/// <returns>The total payroll amount including all deductions and allowances</returns>
/// <exception cref="ArgumentException">Thrown when organizationId is invalid</exception>
/// <exception cref="BusinessException">Thrown when payroll period is not found</exception>
public async Task<decimal> CalculatePayrollAsync(int organizationId, PayrollPeriod period)
```

#### JSDoc Documentation (Frontend)
```typescript
/**
 * Validates employee form data before submission
 * @param formData - The employee form data to validate
 * @returns Promise that resolves to validation result
 * @throws {ValidationError} When required fields are missing
 * @example
 * ```typescript
 * const result = await validateEmployeeData(formData);
 * if (result.isValid) {
 *   // Process form
 * }
 * ```
 */
async validateEmployeeData(formData: EmployeeFormData): Promise<ValidationResult>
```

### README Files

Each module should have a README.md with:
- Purpose and overview
- Installation instructions
- Usage examples
- API reference
- Contributing guidelines

### API Documentation

- Use OpenAPI/Swagger specifications
- Include request/response examples
- Document error codes and messages
- Provide authentication details

## Pull Request Process

### Before Submitting

1. **Code Quality Checklist**
   - [ ] Code follows style guidelines
   - [ ] All tests pass
   - [ ] Code coverage meets requirements
   - [ ] No linting errors
   - [ ] Documentation updated

2. **Testing Checklist**
   - [ ] Unit tests added/updated
   - [ ] Integration tests pass
   - [ ] Manual testing completed
   - [ ] Edge cases considered

3. **Documentation Checklist**
   - [ ] Code comments added
   - [ ] API documentation updated
   - [ ] README files updated
   - [ ] Migration guides provided (if needed)

### PR Template

```markdown
## Description
Brief description of changes made.

## Type of Change
- [ ] Bug fix (non-breaking change that fixes an issue)
- [ ] New feature (non-breaking change that adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## Testing
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing completed

## Screenshots (if applicable)
Add screenshots to help explain your changes.

## Checklist
- [ ] My code follows the style guidelines
- [ ] I have performed a self-review of my code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have made corresponding changes to the documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix is effective or that my feature works
```

### Review Process

1. **Automated Checks**
   - CI/CD pipeline runs
   - Code quality gates pass
   - Security scans complete

2. **Peer Review**
   - At least 2 reviewers required
   - Focus on code quality, logic, and standards
   - Provide constructive feedback

3. **Final Approval**
   - All feedback addressed
   - All checks passing
   - Approved by maintainers

## Issue Reporting

### Bug Reports

Use the bug report template:

```markdown
**Describe the bug**
A clear and concise description of what the bug is.

**To Reproduce**
Steps to reproduce the behavior:
1. Go to '...'
2. Click on '....'
3. Scroll down to '....'
4. See error

**Expected behavior**
A clear and concise description of what you expected to happen.

**Screenshots**
If applicable, add screenshots to help explain your problem.

**Environment:**
 - OS: [e.g. Windows 10]
 - Browser [e.g. chrome, safari]
 - Version [e.g. 22]

**Additional context**
Add any other context about the problem here.
```

### Feature Requests

Use the feature request template:

```markdown
**Is your feature request related to a problem? Please describe.**
A clear and concise description of what the problem is.

**Describe the solution you'd like**
A clear and concise description of what you want to happen.

**Describe alternatives you've considered**
A clear and concise description of any alternative solutions or features you've considered.

**Additional context**
Add any other context or screenshots about the feature request here.
```

## Recognition

Contributors will be recognized in:
- CONTRIBUTORS.md file
- Release notes
- Annual contributor awards
- Community highlights

## Questions?

- **Slack**: #stridehr-dev
- **GitHub Discussions**: Use for general questions
- **GitHub Issues**: Use for specific bugs or features

Thank you for contributing to StrideHR! 🎉