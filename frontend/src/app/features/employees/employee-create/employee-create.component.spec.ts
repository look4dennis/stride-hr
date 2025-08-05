import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { EmployeeCreateComponent } from './employee-create.component';
import { EmployeeService } from '../../../services/employee.service';
import { BranchService } from '../../../services/branch.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LoadingService } from '../../../core/services/loading.service';
import { CreateEmployeeDto, Employee } from '../../../models/employee.models';
import { Branch, ApiResponse } from '../../../models/admin.models';

describe('EmployeeCreateComponent', () => {
  let component: EmployeeCreateComponent;
  let fixture: ComponentFixture<EmployeeCreateComponent>;
  let mockEmployeeService: jasmine.SpyObj<EmployeeService>;
  let mockBranchService: jasmine.SpyObj<BranchService>;
  let mockNotificationService: jasmine.SpyObj<NotificationService>;
  let mockLoadingService: jasmine.SpyObj<LoadingService>;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockBranches: Branch[] = [
    {
      id: 1,
      organizationId: 1,
      name: 'Main Office',
      country: 'United States',
      currency: 'USD',
      timeZone: 'America/New_York',
      address: '123 Business St, New York, NY 10001',
      localHolidays: [],
      complianceSettings: {},
      createdAt: new Date(),
      updatedAt: new Date()
    }
  ];

  const mockEmployee: Employee = {
    id: 1,
    employeeId: 'EMP001',
    branchId: 1,
    firstName: 'John',
    lastName: 'Doe',
    email: 'john.doe@company.com',
    phone: '+1-555-0101',
    dateOfBirth: '1990-05-15',
    joiningDate: '2023-01-15',
    designation: 'Senior Developer',
    department: 'Development',
    basicSalary: 75000,
    status: 'Active' as any,
    createdAt: '2023-01-15T00:00:00Z'
  };

  beforeEach(async () => {
    const employeeServiceSpy = jasmine.createSpyObj('EmployeeService', [
      'createEmployee',
      'getDepartments',
      'getDesignations',
      'getManagers'
    ]);

    const branchServiceSpy = jasmine.createSpyObj('BranchService', [
      'getAllBranches'
    ]);

    const notificationServiceSpy = jasmine.createSpyObj('NotificationService', [
      'showSuccess',
      'showError'
    ]);

    const loadingServiceSpy = jasmine.createSpyObj('LoadingService', [
      'show',
      'hide'
    ]);

    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [EmployeeCreateComponent, ReactiveFormsModule],
      providers: [
        { provide: EmployeeService, useValue: employeeServiceSpy },
        { provide: BranchService, useValue: branchServiceSpy },
        { provide: NotificationService, useValue: notificationServiceSpy },
        { provide: LoadingService, useValue: loadingServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(EmployeeCreateComponent);
    component = fixture.componentInstance;
    mockEmployeeService = TestBed.inject(EmployeeService) as jasmine.SpyObj<EmployeeService>;
    mockBranchService = TestBed.inject(BranchService) as jasmine.SpyObj<BranchService>;
    mockNotificationService = TestBed.inject(NotificationService) as jasmine.SpyObj<NotificationService>;
    mockLoadingService = TestBed.inject(LoadingService) as jasmine.SpyObj<LoadingService>;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;

    // Setup default mock responses
    const successResponse: ApiResponse<Branch[]> = {
      success: true,
      data: mockBranches,
      message: 'Success'
    };

    mockBranchService.getAllBranches.and.returnValue(of(successResponse));
    mockEmployeeService.getDepartments.and.returnValue(of(['Development', 'HR', 'Marketing']));
    mockEmployeeService.getDesignations.and.returnValue(of(['Senior Developer', 'Manager']));
    mockEmployeeService.getManagers.and.returnValue(of([mockEmployee]));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with empty values', () => {
    component.ngOnInit();
    
    expect(component.employeeForm.get('firstName')?.value).toBe('');
    expect(component.employeeForm.get('lastName')?.value).toBe('');
    expect(component.employeeForm.get('email')?.value).toBe('');
    expect(component.employeeForm.valid).toBeFalsy();
  });

  it('should load form data on init', () => {
    component.ngOnInit();

    expect(mockBranchService.getAllBranches).toHaveBeenCalled();
    expect(mockEmployeeService.getDepartments).toHaveBeenCalled();
    expect(mockEmployeeService.getDesignations).toHaveBeenCalled();
    expect(mockEmployeeService.getManagers).toHaveBeenCalled();
  });

  it('should handle branch service error with mock data', () => {
    mockBranchService.getAllBranches.and.returnValue(throwError('API Error'));
    
    component.ngOnInit();
    
    expect(component.branches.length).toBeGreaterThan(0);
    expect(component.branches[0].name).toBe('Main Office');
  });

  it('should validate required fields', () => {
    component.ngOnInit();
    
    // Try to submit empty form
    component.onSubmit();
    
    expect(component.employeeForm.valid).toBeFalsy();
    expect(mockNotificationService.showError).toHaveBeenCalledWith('Please fill in all required fields correctly');
  });

  it('should create employee with valid data', () => {
    mockEmployeeService.createEmployee.and.returnValue(of(mockEmployee));
    
    component.ngOnInit();
    
    // Fill form with valid data
    component.employeeForm.patchValue({
      branchId: '1',
      firstName: 'John',
      lastName: 'Doe',
      email: 'john.doe@company.com',
      phone: '+1-555-0101',
      dateOfBirth: '1990-05-15',
      joiningDate: '2023-01-15',
      designation: 'Senior Developer',
      department: 'Development',
      basicSalary: '75000'
    });
    
    component.onSubmit();
    
    expect(mockEmployeeService.createEmployee).toHaveBeenCalled();
    expect(mockNotificationService.showSuccess).toHaveBeenCalledWith('Employee John Doe created successfully!');
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/employees']);
  });

  it('should handle create employee error', () => {
    mockEmployeeService.createEmployee.and.returnValue(throwError({ error: { message: 'Email already exists' } }));
    
    component.ngOnInit();
    
    // Fill form with valid data
    component.employeeForm.patchValue({
      branchId: '1',
      firstName: 'John',
      lastName: 'Doe',
      email: 'john.doe@company.com',
      phone: '+1-555-0101',
      dateOfBirth: '1990-05-15',
      joiningDate: '2023-01-15',
      designation: 'Senior Developer',
      department: 'Development',
      basicSalary: '75000'
    });
    
    component.onSubmit();
    
    expect(mockNotificationService.showError).toHaveBeenCalledWith('Email already exists');
    expect(component.isSubmitting).toBeFalsy();
  });

  it('should handle file selection', () => {
    const file = new File([''], 'test.jpg', { type: 'image/jpeg' });
    const event = { target: { files: [file] } };
    
    component.onFileSelected(event);
    
    expect(component.selectedFile).toBe(file);
  });

  it('should reject invalid file types', () => {
    const file = new File([''], 'test.txt', { type: 'text/plain' });
    const event = { target: { files: [file] } };
    
    component.onFileSelected(event);
    
    expect(mockNotificationService.showError).toHaveBeenCalledWith('Please select a valid image file');
    expect(component.selectedFile).toBeNull();
  });

  it('should reject files that are too large', () => {
    const file = new File(['x'.repeat(6 * 1024 * 1024)], 'test.jpg', { type: 'image/jpeg' });
    const event = { target: { files: [file] } };
    
    component.onFileSelected(event);
    
    expect(mockNotificationService.showError).toHaveBeenCalledWith('File size must be less than 5MB');
    expect(component.selectedFile).toBeNull();
  });

  it('should navigate back to employee list', () => {
    component.navigateToEmployeeList();
    
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/employees']);
  });

  it('should check field validity correctly', () => {
    component.ngOnInit();
    
    const firstNameField = component.employeeForm.get('firstName');
    firstNameField?.markAsTouched();
    
    expect(component.isFieldInvalid('firstName')).toBeTruthy();
    
    firstNameField?.setValue('John');
    expect(component.isFieldInvalid('firstName')).toBeFalsy();
  });
});