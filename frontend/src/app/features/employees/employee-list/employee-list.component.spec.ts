import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { EmployeeListComponent } from './employee-list.component';
import { EmployeeService } from '../../../services/employee.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LoadingService } from '../../../core/services/loading.service';
import { Employee, EmployeeStatus, PagedResult } from '../../../models/employee.models';

describe('EmployeeListComponent', () => {
  let component: EmployeeListComponent;
  let fixture: ComponentFixture<EmployeeListComponent>;
  let mockEmployeeService: jasmine.SpyObj<EmployeeService>;
  let mockNotificationService: jasmine.SpyObj<NotificationService>;
  let mockLoadingService: jasmine.SpyObj<LoadingService>;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockEmployees: Employee[] = [
    {
      id: 1,
      employeeId: 'EMP001',
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
      status: EmployeeStatus.Active,
      createdAt: '2020-01-15T00:00:00Z'
    },
    {
      id: 2,
      employeeId: 'EMP002',
      branchId: 1,
      firstName: 'Jane',
      lastName: 'Smith',
      email: 'jane.smith@company.com',
      phone: '+1-555-0102',
      dateOfBirth: '1985-08-22',
      joiningDate: '2018-03-10',
      designation: 'Development Manager',
      department: 'Development',
      basicSalary: 95000,
      status: EmployeeStatus.Active,
      createdAt: '2018-03-10T00:00:00Z'
    }
  ];

  const mockPagedResult: PagedResult<Employee> = {
    items: mockEmployees,
    totalCount: 2,
    page: 1,
    pageSize: 10,
    totalPages: 1
  };

  beforeEach(async () => {
    const employeeServiceSpy = jasmine.createSpyObj('EmployeeService', [
      'getEmployees',
      'getDepartments',
      'getDesignations',
      'deactivateEmployee',
      'getMockEmployees'
    ]);
    const notificationServiceSpy = jasmine.createSpyObj('NotificationService', [
      'showSuccess',
      'showError'
    ]);
    const loadingServiceSpy = jasmine.createSpyObj('LoadingService', ['show', 'hide']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [EmployeeListComponent, ReactiveFormsModule],
      providers: [
        { provide: EmployeeService, useValue: employeeServiceSpy },
        { provide: NotificationService, useValue: notificationServiceSpy },
        { provide: LoadingService, useValue: loadingServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(EmployeeListComponent);
    component = fixture.componentInstance;
    mockEmployeeService = TestBed.inject(EmployeeService) as jasmine.SpyObj<EmployeeService>;
    mockNotificationService = TestBed.inject(NotificationService) as jasmine.SpyObj<NotificationService>;
    mockLoadingService = TestBed.inject(LoadingService) as jasmine.SpyObj<LoadingService>;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;

    // Setup default mock returns
    mockEmployeeService.getMockEmployees.and.returnValue(mockPagedResult);
    mockEmployeeService.getEmployees.and.returnValue(of(mockPagedResult));
    mockEmployeeService.getDepartments.and.returnValue(of(['Development', 'HR']));
    mockEmployeeService.getDesignations.and.returnValue(of(['Developer', 'Manager']));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with default values', () => {
    expect(component.employees).toEqual([]);
    expect(component.viewMode).toBe('grid');
    expect(component.loading).toBe(false);
    expect(component.searchForm).toBeDefined();
  });

  it('should load employees on init', () => {
    component.ngOnInit();
    
    expect(component.loading).toBe(true);
    
    // Wait for setTimeout to complete
    setTimeout(() => {
      expect(component.employees).toEqual(mockEmployees);
      expect(component.pagedResult).toEqual(mockPagedResult);
      expect(component.loading).toBe(false);
    }, 600);
  });

  it('should load filter options on init', () => {
    component.ngOnInit();
    
    expect(component.departments).toEqual(['Development', 'Human Resources', 'Marketing', 'Sales', 'Finance']);
    expect(component.designations).toEqual(['Senior Developer', 'Junior Developer', 'Development Manager', 'HR Manager', 'Marketing Manager']);
  });

  it('should perform search with form values', () => {
    component.searchForm.patchValue({
      searchTerm: 'John',
      department: 'Development',
      status: 'Active'
    });

    spyOn(component, 'loadEmployees');
    component.onSearch();

    expect(component.loadEmployees).toHaveBeenCalledWith({
      searchTerm: 'John',
      department: 'Development',
      designation: undefined,
      status: EmployeeStatus.Active,
      page: 1,
      pageSize: 10,
      sortBy: undefined,
      sortDirection: 'asc'
    });
  });

  it('should clear filters and reload employees', () => {
    component.searchForm.patchValue({
      searchTerm: 'John',
      department: 'Development'
    });

    spyOn(component, 'loadEmployees');
    component.clearFilters();

    expect(component.searchForm.value).toEqual({
      searchTerm: null,
      department: null,
      designation: null,
      status: null
    });
    expect(component.currentSort).toEqual({ field: '', direction: 'asc' });
    expect(component.loadEmployees).toHaveBeenCalledWith();
  });

  it('should toggle view mode', () => {
    expect(component.viewMode).toBe('grid');
    
    component.setViewMode('list');
    expect(component.viewMode).toBe('list');
    
    component.setViewMode('grid');
    expect(component.viewMode).toBe('grid');
  });

  it('should handle sorting', () => {
    spyOn(component, 'onSearch');
    
    // First click should set field and direction to asc
    component.sort('firstName');
    expect(component.currentSort).toEqual({ field: 'firstName', direction: 'asc' });
    expect(component.onSearch).toHaveBeenCalled();
    
    // Second click on same field should toggle direction
    component.sort('firstName');
    expect(component.currentSort).toEqual({ field: 'firstName', direction: 'desc' });
  });

  it('should navigate to employee profile on view', () => {
    component.viewEmployee(1);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/employees', 1]);
  });

  it('should navigate to employee edit on edit', () => {
    component.editEmployee(1);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/employees', 1, 'edit']);
  });

  it('should navigate to onboarding on view onboarding', () => {
    component.viewOnboarding(1);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/employees', 1, 'onboarding']);
  });

  it('should navigate to exit process on initiate exit', () => {
    component.initiateExit(1);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/employees', 1, 'exit']);
  });

  it('should deactivate employee with confirmation', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    mockEmployeeService.deactivateEmployee.and.returnValue(of(true));
    spyOn(component, 'loadEmployees');

    component.deactivateEmployee(1);

    expect(window.confirm).toHaveBeenCalledWith('Are you sure you want to deactivate this employee?');
    expect(mockEmployeeService.deactivateEmployee).toHaveBeenCalledWith(1);
    expect(mockNotificationService.showSuccess).toHaveBeenCalledWith('Employee deactivated successfully');
    expect(component.loadEmployees).toHaveBeenCalled();
  });

  it('should not deactivate employee without confirmation', () => {
    spyOn(window, 'confirm').and.returnValue(false);

    component.deactivateEmployee(1);

    expect(mockEmployeeService.deactivateEmployee).not.toHaveBeenCalled();
  });

  it('should handle deactivation error', () => {
    spyOn(window, 'confirm').and.returnValue(true);
    mockEmployeeService.deactivateEmployee.and.returnValue(throwError('Error'));

    component.deactivateEmployee(1);

    expect(mockNotificationService.showError).toHaveBeenCalledWith('Failed to deactivate employee');
  });

  it('should get correct status color', () => {
    expect(component.getStatusColor(EmployeeStatus.Active)).toBe('success');
    expect(component.getStatusColor(EmployeeStatus.Inactive)).toBe('secondary');
    expect(component.getStatusColor(EmployeeStatus.OnLeave)).toBe('warning');
    expect(component.getStatusColor(EmployeeStatus.Terminated)).toBe('danger');
    expect(component.getStatusColor(EmployeeStatus.Resigned)).toBe('info');
  });

  it('should get profile photo with fallback', () => {
    const employeeWithPhoto: Employee = { ...mockEmployees[0], profilePhoto: '/path/to/photo.jpg' };
    const employeeWithoutPhoto: Employee = { ...mockEmployees[0], profilePhoto: undefined };

    expect(component.getProfilePhoto(employeeWithPhoto)).toBe('/path/to/photo.jpg');
    expect(component.getProfilePhoto(employeeWithoutPhoto)).toBe('/assets/images/avatars/default-avatar.png');
  });

  it('should format date correctly', () => {
    const dateString = '2020-01-15T00:00:00Z';
    const formatted = component.formatDate(dateString);
    
    expect(formatted).toBe(new Date(dateString).toLocaleDateString());
  });

  it('should generate correct page numbers', () => {
    component.pagedResult = {
      items: [],
      totalCount: 50,
      page: 5,
      pageSize: 10,
      totalPages: 10
    };

    const pageNumbers = component.getPageNumbers();
    expect(pageNumbers).toEqual([3, 4, 5, 6, 7]);
  });

  it('should navigate to correct page', () => {
    component.pagedResult = mockPagedResult;
    spyOn(component, 'loadEmployees');

    component.goToPage(2);

    expect(component.loadEmployees).toHaveBeenCalledWith({
      searchTerm: undefined,
      department: undefined,
      designation: undefined,
      status: undefined,
      page: 2,
      pageSize: 10
    });
  });

  it('should not navigate to invalid page', () => {
    component.pagedResult = mockPagedResult;
    spyOn(component, 'loadEmployees');

    component.goToPage(0); // Invalid page
    component.goToPage(5); // Page beyond total pages

    expect(component.loadEmployees).not.toHaveBeenCalled();
  });

  it('should handle page size change', () => {
    const event = { target: { value: '25' } };
    spyOn(component, 'loadEmployees');

    component.onPageSizeChange(event);

    expect(component.loadEmployees).toHaveBeenCalledWith({
      searchTerm: undefined,
      department: undefined,
      designation: undefined,
      status: undefined,
      page: 1,
      pageSize: 25
    });
  });
});