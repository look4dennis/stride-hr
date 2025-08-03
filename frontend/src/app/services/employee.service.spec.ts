import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { EmployeeService } from './employee.service';
import { 
  Employee, 
  CreateEmployeeDto, 
  UpdateEmployeeDto, 
  EmployeeSearchCriteria, 
  PagedResult,
  EmployeeStatus,
  EmployeeOnboarding,
  EmployeeExitProcess,
  OrganizationalChart
} from '../models/employee.models';

describe('EmployeeService', () => {
  let service: EmployeeService;
  let httpMock: HttpTestingController;
  const API_URL = 'http://localhost:5000/api';

  const mockEmployee: Employee = {
    id: 1,
    employeeId: 'EMP001',
    branchId: 1,
    firstName: 'John',
    lastName: 'Doe',
    email: 'john.doe@company.com',
    phone: '+1-555-0101',
    profilePhoto: '/assets/images/avatars/john-doe.jpg',
    dateOfBirth: '1990-05-15',
    joiningDate: '2020-01-15',
    designation: 'Senior Developer',
    department: 'Development',
    basicSalary: 75000,
    status: EmployeeStatus.Active,
    reportingManagerId: 2,
    createdAt: '2020-01-15T00:00:00Z'
  };

  const mockPagedResult: PagedResult<Employee> = {
    items: [mockEmployee],
    totalCount: 1,
    page: 1,
    pageSize: 10,
    totalPages: 1
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [EmployeeService]
    });
    service = TestBed.inject(EmployeeService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getEmployees', () => {
    it('should get employees without criteria', () => {
      service.getEmployees().subscribe(result => {
        expect(result).toEqual(mockPagedResult);
      });

      const req = httpMock.expectOne(`${API_URL}/employees`);
      expect(req.request.method).toBe('GET');
      req.flush(mockPagedResult);
    });

    it('should get employees with search criteria', () => {
      const criteria: EmployeeSearchCriteria = {
        searchTerm: 'John',
        department: 'Development',
        status: EmployeeStatus.Active,
        page: 1,
        pageSize: 10
      };

      service.getEmployees(criteria).subscribe(result => {
        expect(result).toEqual(mockPagedResult);
      });

      const req = httpMock.expectOne(req => 
        req.url === `${API_URL}/employees` && 
        req.params.get('searchTerm') === 'John' &&
        req.params.get('department') === 'Development' &&
        req.params.get('status') === 'Active' &&
        req.params.get('page') === '1' &&
        req.params.get('pageSize') === '10'
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockPagedResult);
    });
  });

  describe('getEmployeeById', () => {
    it('should get employee by id', () => {
      service.getEmployeeById(1).subscribe(employee => {
        expect(employee).toEqual(mockEmployee);
      });

      const req = httpMock.expectOne(`${API_URL}/employees/1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockEmployee);
    });
  });

  describe('createEmployee', () => {
    it('should create employee with FormData', () => {
      const createDto: CreateEmployeeDto = {
        branchId: 1,
        firstName: 'John',
        lastName: 'Doe',
        email: 'john.doe@company.com',
        phone: '+1-555-0101',
        dateOfBirth: '1990-05-15',
        joiningDate: '2020-01-15',
        designation: 'Senior Developer',
        department: 'Development',
        basicSalary: 75000
      };

      service.createEmployee(createDto).subscribe(employee => {
        expect(employee).toEqual(mockEmployee);
      });

      const req = httpMock.expectOne(`${API_URL}/employees`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toBeInstanceOf(FormData);
      req.flush(mockEmployee);
    });

    it('should create employee with profile photo', () => {
      const mockFile = new File([''], 'test.jpg', { type: 'image/jpeg' });
      const createDto: CreateEmployeeDto = {
        branchId: 1,
        firstName: 'John',
        lastName: 'Doe',
        email: 'john.doe@company.com',
        phone: '+1-555-0101',
        dateOfBirth: '1990-05-15',
        joiningDate: '2020-01-15',
        designation: 'Senior Developer',
        department: 'Development',
        basicSalary: 75000,
        profilePhoto: mockFile
      };

      service.createEmployee(createDto).subscribe(employee => {
        expect(employee).toEqual(mockEmployee);
      });

      const req = httpMock.expectOne(`${API_URL}/employees`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toBeInstanceOf(FormData);
      req.flush(mockEmployee);
    });
  });

  describe('updateEmployee', () => {
    it('should update employee', () => {
      const updateDto: UpdateEmployeeDto = {
        firstName: 'John Updated',
        lastName: 'Doe Updated'
      };

      service.updateEmployee(1, updateDto).subscribe(employee => {
        expect(employee).toEqual(mockEmployee);
      });

      const req = httpMock.expectOne(`${API_URL}/employees/1`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toBeInstanceOf(FormData);
      req.flush(mockEmployee);
    });
  });

  describe('deactivateEmployee', () => {
    it('should deactivate employee', () => {
      service.deactivateEmployee(1).subscribe(result => {
        expect(result).toBe(true);
      });

      const req = httpMock.expectOne(`${API_URL}/employees/1`);
      expect(req.request.method).toBe('DELETE');
      req.flush(true);
    });
  });

  describe('uploadProfilePhoto', () => {
    it('should upload profile photo', () => {
      const mockFile = new File([''], 'test.jpg', { type: 'image/jpeg' });
      const mockResponse = { photoUrl: '/path/to/photo.jpg' };

      service.uploadProfilePhoto(1, mockFile).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${API_URL}/employees/1/photo`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toBeInstanceOf(FormData);
      req.flush(mockResponse);
    });
  });

  describe('getOrganizationalChart', () => {
    it('should get organizational chart without branch filter', () => {
      const mockOrgChart: OrganizationalChart[] = [
        {
          employee: mockEmployee,
          children: [],
          level: 0
        }
      ];

      service.getOrganizationalChart().subscribe(chart => {
        expect(chart).toEqual(mockOrgChart);
      });

      const req = httpMock.expectOne(`${API_URL}/employees/org-chart`);
      expect(req.request.method).toBe('GET');
      req.flush(mockOrgChart);
    });

    it('should get organizational chart with branch filter', () => {
      const mockOrgChart: OrganizationalChart[] = [
        {
          employee: mockEmployee,
          children: [],
          level: 0
        }
      ];

      service.getOrganizationalChart(1).subscribe(chart => {
        expect(chart).toEqual(mockOrgChart);
      });

      const req = httpMock.expectOne(req => 
        req.url === `${API_URL}/employees/org-chart` && 
        req.params.get('branchId') === '1'
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockOrgChart);
    });
  });

  describe('getEmployeeOnboarding', () => {
    it('should get employee onboarding', () => {
      const mockOnboarding: EmployeeOnboarding = {
        employeeId: 1,
        steps: [],
        overallProgress: 50,
        startedAt: '2024-01-15T09:00:00Z',
        status: 'InProgress' as any
      };

      service.getEmployeeOnboarding(1).subscribe(onboarding => {
        expect(onboarding).toEqual(mockOnboarding);
      });

      const req = httpMock.expectOne(`${API_URL}/employees/1/onboarding`);
      expect(req.request.method).toBe('GET');
      req.flush(mockOnboarding);
    });
  });

  describe('updateOnboardingStep', () => {
    it('should update onboarding step', () => {
      const mockOnboarding: EmployeeOnboarding = {
        employeeId: 1,
        steps: [],
        overallProgress: 75,
        startedAt: '2024-01-15T09:00:00Z',
        status: 'InProgress' as any
      };

      service.updateOnboardingStep(1, 'step1', true).subscribe(onboarding => {
        expect(onboarding).toEqual(mockOnboarding);
      });

      const req = httpMock.expectOne(`${API_URL}/employees/1/onboarding/steps/step1`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual({ completed: true });
      req.flush(mockOnboarding);
    });
  });

  describe('initiateExitProcess', () => {
    it('should initiate exit process', () => {
      const exitData: Partial<EmployeeExitProcess> = {
        exitDate: '2024-02-15',
        reason: 'Better opportunity',
        exitType: 'Resignation' as any
      };

      const mockExitProcess: EmployeeExitProcess = {
        employeeId: 1,
        exitDate: '2024-02-15',
        reason: 'Better opportunity',
        exitType: 'Resignation' as any,
        assetsToReturn: [],
        clearanceSteps: [],
        status: 'Initiated' as any
      };

      service.initiateExitProcess(1, exitData).subscribe(exitProcess => {
        expect(exitProcess).toEqual(mockExitProcess);
      });

      const req = httpMock.expectOne(`${API_URL}/employees/1/exit`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(exitData);
      req.flush(mockExitProcess);
    });
  });

  describe('getDepartments', () => {
    it('should get departments', () => {
      const mockDepartments = ['Development', 'HR', 'Marketing'];

      service.getDepartments().subscribe(departments => {
        expect(departments).toEqual(mockDepartments);
      });

      const req = httpMock.expectOne(`${API_URL}/employees/departments`);
      expect(req.request.method).toBe('GET');
      req.flush(mockDepartments);
    });
  });

  describe('getDesignations', () => {
    it('should get designations', () => {
      const mockDesignations = ['Developer', 'Manager', 'Analyst'];

      service.getDesignations().subscribe(designations => {
        expect(designations).toEqual(mockDesignations);
      });

      const req = httpMock.expectOne(`${API_URL}/employees/designations`);
      expect(req.request.method).toBe('GET');
      req.flush(mockDesignations);
    });
  });

  describe('getManagers', () => {
    it('should get managers without branch filter', () => {
      const mockManagers = [mockEmployee];

      service.getManagers().subscribe(managers => {
        expect(managers).toEqual(mockManagers);
      });

      const req = httpMock.expectOne(`${API_URL}/employees/managers`);
      expect(req.request.method).toBe('GET');
      req.flush(mockManagers);
    });

    it('should get managers with branch filter', () => {
      const mockManagers = [mockEmployee];

      service.getManagers(1).subscribe(managers => {
        expect(managers).toEqual(mockManagers);
      });

      const req = httpMock.expectOne(req => 
        req.url === `${API_URL}/employees/managers` && 
        req.params.get('branchId') === '1'
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockManagers);
    });
  });

  describe('getMockEmployees', () => {
    it('should return mock employees data', () => {
      const mockData = service.getMockEmployees();
      
      expect(mockData.items).toBeDefined();
      expect(mockData.items.length).toBeGreaterThan(0);
      expect(mockData.totalCount).toBe(mockData.items.length);
      expect(mockData.page).toBe(1);
      expect(mockData.pageSize).toBe(10);
      expect(mockData.totalPages).toBe(1);
    });

    it('should return employees with correct structure', () => {
      const mockData = service.getMockEmployees();
      const firstEmployee = mockData.items[0];
      
      expect(firstEmployee.id).toBeDefined();
      expect(firstEmployee.employeeId).toBeDefined();
      expect(firstEmployee.firstName).toBeDefined();
      expect(firstEmployee.lastName).toBeDefined();
      expect(firstEmployee.email).toBeDefined();
      expect(firstEmployee.designation).toBeDefined();
      expect(firstEmployee.department).toBeDefined();
      expect(firstEmployee.status).toBeDefined();
    });
  });
});