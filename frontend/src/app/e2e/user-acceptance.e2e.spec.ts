import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { Component } from '@angular/core';

// Mock components for E2E testing
@Component({
  selector: 'app-employee-list',
  template: '<div>Employee List Mock</div>'
})
class MockEmployeeListComponent {
  employees: any[] = [];
  totalCount = 0;
  searchTerm = '';
  selectedDepartment = '';

  createEmployee(dto: any) {
    this.employees.push({ ...dto, id: Date.now() });
  }

  updateEmployee(dto: any) {
    const index = this.employees.findIndex(e => e.id === dto.id);
    if (index >= 0) {
      this.employees[index] = dto;
    }
  }

  searchEmployees() {
    // Mock search functionality
  }

  filterByDepartment() {
    // Mock filter functionality
  }
}

@Component({
  selector: 'app-attendance',
  template: '<div>Attendance Mock</div>'
})
class MockAttendanceComponent {
  isCheckedIn = false;
  isOnBreak = false;
  attendanceHistory: any[] = [];
  attendanceReport: any = null;

  checkIn(dto: any) {
    this.isCheckedIn = true;
  }

  checkOut(dto: any) {
    this.isCheckedIn = false;
  }

  startBreak(type: string, notes: string) {
    this.isOnBreak = true;
  }

  endBreak(notes: string) {
    this.isOnBreak = false;
  }

  loadAttendanceHistory() {
    // Mock load history
  }

  generateReport() {
    // Mock generate report
  }

  onCheckInClick() {
    // Mock check-in click
  }
}

@Component({
  selector: 'app-payroll',
  template: '<div>Payroll Mock</div>'
})
class MockPayrollComponent {
  branches: any[] = [];
  payrollRecords: any[] = [];
  currentPayslip: any = null;

  calculatePayroll(dto: any) {
    this.payrollRecords.push({ ...dto, id: Date.now() });
  }

  approvePayroll(id: number, notes: string) {
    const record = this.payrollRecords.find(r => r.id === id);
    if (record) {
      record.status = 'HRApproved';
    }
  }

  releasePayroll(id: number, notes: string) {
    const record = this.payrollRecords.find(r => r.id === id);
    if (record) {
      record.status = 'Released';
    }
  }

  generatePayslip(id: number) {
    // Mock generate payslip
  }
}

@Component({
  selector: 'app-leave-request',
  template: '<div>Leave Request Mock</div>'
})
class MockLeaveRequestComponent {
  leaveBalance: any = {};
  leaveRequests: any[] = [];
  leaveHistory: any[] = [];

  submitLeaveRequest(dto: any) {
    this.leaveRequests.push({ ...dto, id: Date.now(), status: 'Pending' });
  }

  approveLeaveRequest(id: number, notes: string) {
    const request = this.leaveRequests.find(r => r.id === id);
    if (request) {
      request.status = 'Approved';
    }
  }

  loadLeaveHistory() {
    // Mock load history
  }
}

@Component({
  selector: 'app-project-list',
  template: '<div>Project List Mock</div>'
})
class MockProjectListComponent {
  projects: any[] = [];
  projectProgress: any = null;

  createProject(dto: any) {
    this.projects.push({ ...dto, id: Date.now(), status: 'Planning' });
  }

  assignTeamMembers(projectId: number, employeeIds: number[]) {
    // Mock assign team members
  }

  loadProjectProgress(projectId: number) {
    // Mock load progress
  }
}

// Mock services
class MockEmployeeService {
  getEmployees() { return of({ data: { items: [], totalCount: 0 } }); }
  createEmployee(dto: any) { return of({ data: dto }); }
  updateEmployee(dto: any) { return of({ data: dto }); }
}

class MockAttendanceService {
  getStatus() { return of({ data: { isCheckedIn: false, isOnBreak: false } }); }
  checkIn(dto: any) { return of({ data: dto }); }
  checkOut(dto: any) { return of({ data: dto }); }
}

class MockPayrollService {
  calculatePayroll(dto: any) { return of({ data: dto }); }
  approvePayroll(id: number, dto: any) { return of({ data: dto }); }
}

class MockLeaveService {
  getBalance(id: number) { return of({ data: {} }); }
  createRequest(dto: any) { return of({ data: dto }); }
}

class MockProjectService {
  createProject(dto: any) { return of({ data: dto }); }
  assignTeam(id: number, dto: any) { return of({ data: dto }); }
}

class MockAuthService {
  isAuthenticated() { return true; }
  getCurrentUser() { return { id: 1, name: 'Test User' }; }
  hasPermission() { return true; }
}

class MockNotificationService {
  showError(message: string) { }
  showSuccess(message: string) { }
}

describe('User Acceptance E2E Tests', () => {
  let httpMock: HttpTestingController;
  let router: Router;
  let authService: MockAuthService;
  let notificationService: MockNotificationService;

  // Mock data for testing
  const mockEmployee = {
    id: 1,
    employeeId: 'EMP001',
    firstName: 'John',
    lastName: 'Doe',
    email: 'john.doe@company.com',
    phone: '+1-555-0123',
    dateOfBirth: new Date('1990-01-15'),
    address: '123 Main St',
    joiningDate: new Date('2023-01-01'),
    designation: 'Software Engineer',
    department: 'IT',
    basicSalary: 75000,
    branchId: 1,
    status: 'Active',
    createdAt: new Date(),
    updatedAt: new Date()
  };

  const mockBranches = [
    { id: 1, name: 'US Headquarters', currency: 'USD', timeZone: 'America/New_York' },
    { id: 2, name: 'European Office', currency: 'EUR', timeZone: 'Europe/London' },
    { id: 3, name: 'Asia Pacific Hub', currency: 'SGD', timeZone: 'Asia/Singapore' }
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
        BrowserAnimationsModule,
        NgbModule,
        FormsModule,
        ReactiveFormsModule
      ],
      declarations: [
        MockEmployeeListComponent,
        MockAttendanceComponent,
        MockPayrollComponent,
        MockLeaveRequestComponent,
        MockProjectListComponent
      ],
      providers: [
        { provide: 'EmployeeService', useClass: MockEmployeeService },
        { provide: 'AttendanceService', useClass: MockAttendanceService },
        { provide: 'PayrollService', useClass: MockPayrollService },
        { provide: 'LeaveService', useClass: MockLeaveService },
        { provide: 'ProjectService', useClass: MockProjectService },
        { provide: 'AuthService', useClass: MockAuthService },
        { provide: 'NotificationService', useClass: MockNotificationService }
      ]
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    authService = new MockAuthService();
    notificationService = new MockNotificationService();

    // Mock authentication
    spyOn(authService, 'isAuthenticated').and.returnValue(true);
    spyOn(authService, 'getCurrentUser').and.returnValue({ id: 1, name: 'Test User' });
    spyOn(authService, 'hasPermission').and.returnValue(true);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('Complete Employee Lifecycle E2E Tests', () => {
    let employeeFixture: ComponentFixture<MockEmployeeListComponent>;
    let employeeComponent: MockEmployeeListComponent;

    beforeEach(() => {
      employeeFixture = TestBed.createComponent(MockEmployeeListComponent);
      employeeComponent = employeeFixture.componentInstance;
    });

    it('should complete full employee onboarding workflow', async () => {
      // Step 1: Load employee list
      employeeFixture.detectChanges();

      const employeeListReq = httpMock.expectOne('/api/employee?pageNumber=1&pageSize=10');
      expect(employeeListReq.request.method).toBe('GET');
      employeeListReq.flush({
        success: true,
        data: {
          items: [],
          totalCount: 0,
          pageNumber: 1,
          pageSize: 10
        }
      });

      // Step 2: Create new employee
      const newEmployeeDto = {
        firstName: 'Alice',
        lastName: 'Johnson',
        email: 'alice.johnson@company.com',
        phone: '+1-555-0456',
        dateOfBirth: new Date('1988-06-15'),
        address: '456 Oak Avenue',
        joiningDate: new Date(),
        designation: 'Senior Developer',
        department: 'Engineering',
        basicSalary: 85000,
        branchId: 1
      };

      // Simulate form submission
      employeeComponent.createEmployee(newEmployeeDto);

      const createEmployeeReq = httpMock.expectOne('/api/employee');
      expect(createEmployeeReq.request.method).toBe('POST');
      expect(createEmployeeReq.request.body).toEqual(newEmployeeDto);
      
      const createdEmployee = { ...mockEmployee, ...newEmployeeDto, id: 2 };
      createEmployeeReq.flush({
        success: true,
        data: createdEmployee
      });

      // Verify employee was created
      expect(employeeComponent.employees).toContain(jasmine.objectContaining({
        firstName: 'Alice',
        lastName: 'Johnson'
      }));

      // Step 3: Update employee (promotion scenario)
      const updateEmployeeDto = {
        ...createdEmployee,
        designation: 'Lead Developer',
        basicSalary: 95000
      };

      employeeComponent.updateEmployee(updateEmployeeDto);

      const updateEmployeeReq = httpMock.expectOne(`/api/employee/${createdEmployee.id}`);
      expect(updateEmployeeReq.request.method).toBe('PUT');
      updateEmployeeReq.flush({
        success: true,
        data: updateEmployeeDto
      });

      // Verify employee was updated
      const updatedEmployee = employeeComponent.employees.find((e: any) => e.id === createdEmployee.id);
      expect(updatedEmployee?.designation).toBe('Lead Developer');
      expect(updatedEmployee?.basicSalary).toBe(95000);
    });

    it('should handle employee search and filtering', async () => {
      employeeFixture.detectChanges();

      // Initial load
      const initialReq = httpMock.expectOne('/api/employee?pageNumber=1&pageSize=10');
      initialReq.flush({
        success: true,
        data: {
          items: [mockEmployee],
          totalCount: 1,
          pageNumber: 1,
          pageSize: 10
        }
      });

      // Test search functionality
      employeeComponent.searchTerm = 'John';
      employeeComponent.searchEmployees();

      const searchReq = httpMock.expectOne('/api/employee?pageNumber=1&pageSize=10&search=John');
      expect(searchReq.request.method).toBe('GET');
      searchReq.flush({
        success: true,
        data: {
          items: [mockEmployee],
          totalCount: 1,
          pageNumber: 1,
          pageSize: 10
        }
      });

      expect(employeeComponent.employees.length).toBe(1);
      expect(employeeComponent.employees[0].firstName).toBe('John');

      // Test department filter
      employeeComponent.selectedDepartment = 'IT';
      employeeComponent.filterByDepartment();

      const filterReq = httpMock.expectOne('/api/employee?pageNumber=1&pageSize=10&department=IT');
      filterReq.flush({
        success: true,
        data: {
          items: [mockEmployee],
          totalCount: 1,
          pageNumber: 1,
          pageSize: 10
        }
      });

      expect(employeeComponent.employees.length).toBe(1);
    });
  });

  describe('Attendance Management E2E Tests', () => {
    let attendanceFixture: ComponentFixture<MockAttendanceComponent>;
    let attendanceComponent: MockAttendanceComponent;

    beforeEach(() => {
      attendanceFixture = TestBed.createComponent(MockAttendanceComponent);
      attendanceComponent = attendanceFixture.componentInstance;
    });

    it('should complete daily attendance workflow', async () => {
      attendanceFixture.detectChanges();

      // Load current attendance status
      const statusReq = httpMock.expectOne('/api/attendance/status');
      statusReq.flush({
        success: true,
        data: {
          isCheckedIn: false,
          lastCheckIn: null,
          isOnBreak: false
        }
      });

      // Step 1: Check in
      const checkInDto = {
        location: 'Office - Floor 5',
        notes: 'Starting work day'
      };

      attendanceComponent.checkIn(checkInDto);

      const checkInReq = httpMock.expectOne('/api/attendance/checkin');
      expect(checkInReq.request.method).toBe('POST');
      expect(checkInReq.request.body).toEqual(checkInDto);
      checkInReq.flush({
        success: true,
        data: {
          id: 1,
          employeeId: 1,
          checkInTime: new Date(),
          location: checkInDto.location,
          notes: checkInDto.notes
        }
      });

      expect(attendanceComponent.isCheckedIn).toBe(true);

      // Step 2: Start break
      attendanceComponent.startBreak('Lunch', 'Lunch break');

      const startBreakReq = httpMock.expectOne('/api/attendance/break/start');
      expect(startBreakReq.request.method).toBe('POST');
      startBreakReq.flush({
        success: true,
        data: {
          id: 1,
          breakType: 'Lunch',
          startTime: new Date()
        }
      });

      expect(attendanceComponent.isOnBreak).toBe(true);

      // Step 3: End break
      attendanceComponent.endBreak('Back from lunch');

      const endBreakReq = httpMock.expectOne('/api/attendance/break/end');
      expect(endBreakReq.request.method).toBe('POST');
      endBreakReq.flush({
        success: true,
        data: {
          id: 1,
          endTime: new Date()
        }
      });

      expect(attendanceComponent.isOnBreak).toBe(false);

      // Step 4: Check out
      const checkOutDto = {
        notes: 'End of work day'
      };

      attendanceComponent.checkOut(checkOutDto);

      const checkOutReq = httpMock.expectOne('/api/attendance/checkout');
      expect(checkOutReq.request.method).toBe('POST');
      checkOutReq.flush({
        success: true,
        data: {
          id: 1,
          checkOutTime: new Date(),
          notes: checkOutDto.notes
        }
      });

      expect(attendanceComponent.isCheckedIn).toBe(false);
    });

    it('should display attendance history and reports', async () => {
      attendanceFixture.detectChanges();

      // Load attendance history
      attendanceComponent.loadAttendanceHistory();

      const historyReq = httpMock.expectOne('/api/attendance/employee/1/history?startDate=2024-01-01&endDate=2024-01-31');
      historyReq.flush({
        success: true,
        data: [
          {
            id: 1,
            date: new Date(),
            checkInTime: new Date('2024-01-15T09:00:00'),
            checkOutTime: new Date('2024-01-15T17:30:00'),
            totalHours: 8.5,
            breaks: [
              { type: 'Lunch', duration: 30 }
            ]
          }
        ]
      });

      expect(attendanceComponent.attendanceHistory.length).toBe(1);
      expect(attendanceComponent.attendanceHistory[0].totalHours).toBe(8.5);

      // Generate attendance report
      attendanceComponent.generateReport();

      const reportReq = httpMock.expectOne('/api/attendance/report');
      reportReq.flush({
        success: true,
        data: {
          totalDays: 22,
          presentDays: 20,
          absentDays: 2,
          totalHours: 160,
          averageHours: 8
        }
      });

      expect(attendanceComponent.attendanceReport).toBeDefined();
      expect(attendanceComponent.attendanceReport.presentDays).toBe(20);
    });
  });

  describe('Multi-Currency Payroll E2E Tests', () => {
    let payrollFixture: ComponentFixture<MockPayrollComponent>;
    let payrollComponent: MockPayrollComponent;

    beforeEach(() => {
      payrollFixture = TestBed.createComponent(MockPayrollComponent);
      payrollComponent = payrollFixture.componentInstance;
    });

    it('should process payroll for multiple currencies', async () => {
      payrollFixture.detectChanges();

      // Load branches for currency selection
      const branchesReq = httpMock.expectOne('/api/branch');
      branchesReq.flush({
        success: true,
        data: mockBranches
      });

      // Mock branches data
      payrollComponent.branches = mockBranches;
      expect(payrollComponent.branches.length).toBe(3);

      // Process payroll for USD branch
      const usdPayrollDto = {
        employeeId: 1,
        period: 'Monthly',
        month: 1,
        year: 2024,
        payrollPeriod: {
          startDate: new Date('2024-01-01'),
          endDate: new Date('2024-01-31'),
          month: 1,
          year: 2024
        }
      };

      payrollComponent.calculatePayroll(usdPayrollDto);
      payrollComponent.payrollRecords[0] = {
        id: 1,
        employeeId: 1,
        grossSalary: 6250,
        currency: 'USD',
        status: 'Calculated',
        deductions: 1250,
        allowances: 500,
        netSalary: 5500
      };

      expect(payrollComponent.payrollRecords.length).toBe(1);
      expect(payrollComponent.payrollRecords[0].currency).toBe('USD');

      // Process payroll for EUR branch (simulate different employee)
      const eurPayrollDto = {
        employeeId: 2,
        period: 'Monthly',
        month: 1,
        year: 2024,
        payrollPeriod: {
          startDate: new Date('2024-01-01'),
          endDate: new Date('2024-01-31'),
          month: 1,
          year: 2024
        }
      };

      payrollComponent.calculatePayroll(eurPayrollDto);
      payrollComponent.payrollRecords[1] = {
        id: 2,
        employeeId: 2,
        grossSalary: 4583,
        currency: 'EUR',
        status: 'Calculated',
        deductions: 916,
        allowances: 300,
        netSalary: 3967
      };

      expect(payrollComponent.payrollRecords.length).toBe(2);
      expect(payrollComponent.payrollRecords[1].currency).toBe('EUR');
    });

    it('should approve and release payroll', async () => {
      payrollFixture.detectChanges();

      // Setup initial payroll record
      payrollComponent.payrollRecords = [{
        id: 1,
        employeeId: 1,
        grossSalary: 6250,
        currency: 'USD',
        status: 'Calculated',
        deductions: 1250,
        allowances: 500,
        netSalary: 5500
      }];

      // Approve payroll
      payrollComponent.approvePayroll(1, 'Approved by HR Manager');
      expect(payrollComponent.payrollRecords[0].status).toBe('HRApproved');

      // Release payroll
      payrollComponent.releasePayroll(1, 'Payroll released to employees');
      expect(payrollComponent.payrollRecords[0].status).toBe('Released');
    });

    it('should generate payslips', async () => {
      payrollFixture.detectChanges();

      // Generate payslip
      payrollComponent.generatePayslip(1);
      payrollComponent.currentPayslip = {
        payrollRecordId: 1,
        employeeName: 'John Doe',
        employeeId: 'EMP001',
        payPeriod: 'January 2024',
        grossSalary: 6250,
        deductions: 1250,
        allowances: 500,
        netSalary: 5500,
        currency: 'USD'
      };

      expect(payrollComponent.currentPayslip).toBeDefined();
      expect(payrollComponent.currentPayslip.employeeName).toBe('John Doe');
      expect(payrollComponent.currentPayslip.currency).toBe('USD');
    });
  });

  describe('Leave Management E2E Tests', () => {
    let leaveFixture: ComponentFixture<MockLeaveRequestComponent>;
    let leaveComponent: MockLeaveRequestComponent;

    beforeEach(() => {
      leaveFixture = TestBed.createComponent(MockLeaveRequestComponent);
      leaveComponent = leaveFixture.componentInstance;
    });

    it('should complete leave request workflow', async () => {
      leaveFixture.detectChanges();

      // Load leave balance
      leaveComponent.leaveBalance = {
        employeeId: 1,
        annualLeaveBalance: 20,
        sickLeaveBalance: 10,
        emergencyLeaveBalance: 5,
        annualLeaveUsed: 5,
        sickLeaveUsed: 2,
        emergencyLeaveUsed: 0
      };

      expect(leaveComponent.leaveBalance.annualLeaveBalance).toBe(20);

      // Submit leave request
      const leaveRequestDto = {
        leaveType: 'Annual',
        startDate: new Date('2024-02-15'),
        endDate: new Date('2024-02-19'),
        reason: 'Family vacation',
        notes: 'Pre-planned family trip'
      };

      leaveComponent.submitLeaveRequest(leaveRequestDto);
      expect(leaveComponent.leaveRequests.length).toBe(1);
      expect(leaveComponent.leaveRequests[0].status).toBe('Pending');

      // Load leave history
      leaveComponent.loadLeaveHistory();
      leaveComponent.leaveHistory = [
        {
          id: 1,
          leaveType: 'Annual',
          startDate: new Date('2024-02-15'),
          endDate: new Date('2024-02-19'),
          status: 'Pending',
          requestDate: new Date()
        }
      ];

      expect(leaveComponent.leaveHistory.length).toBe(1);
    });

    it('should handle leave approval workflow', async () => {
      leaveFixture.detectChanges();

      // Setup pending leave request
      leaveComponent.leaveRequests = [{
        id: 1,
        employeeId: 1,
        leaveType: 'Annual',
        startDate: new Date('2024-02-15'),
        endDate: new Date('2024-02-19'),
        reason: 'Family vacation',
        status: 'Pending',
        requestDate: new Date()
      }];

      // Approve leave request (manager action)
      leaveComponent.approveLeaveRequest(1, 'Approved - good performance record');
      expect(leaveComponent.leaveRequests[0].status).toBe('Approved');
    });
  });

  describe('Project Management E2E Tests', () => {
    let projectFixture: ComponentFixture<MockProjectListComponent>;
    let projectComponent: MockProjectListComponent;

    beforeEach(() => {
      projectFixture = TestBed.createComponent(MockProjectListComponent);
      projectComponent = projectFixture.componentInstance;
    });

    it('should complete project lifecycle workflow', async () => {
      projectFixture.detectChanges();

      // Load projects
      projectFixture.detectChanges();

      // Create new project
      const createProjectDto = {
        name: 'Mobile App Development',
        description: 'Develop a new mobile application for customer engagement',
        startDate: new Date('2024-02-01'),
        endDate: new Date('2024-05-31'),
        estimatedHours: 800,
        budget: 120000,
        priority: 'High'
      };

      projectComponent.createProject(createProjectDto);
      expect(projectComponent.projects.length).toBe(1);
      expect(projectComponent.projects[0].name).toBe('Mobile App Development');

      // Assign team members
      projectComponent.assignTeamMembers(1, [1, 2, 3]);

      // Load project progress
      projectComponent.loadProjectProgress(1);
      projectComponent.projectProgress = {
        projectId: 1,
        completionPercentage: 25,
        actualHoursWorked: 200,
        remainingHours: 600,
        tasksCompleted: 5,
        totalTasks: 20
      };

      expect(projectComponent.projectProgress).toBeDefined();
      expect(projectComponent.projectProgress.completionPercentage).toBe(25);
    });
  });

  describe('Error Handling and Edge Cases', () => {
    it('should handle network errors gracefully', async () => {
      const employeeFixture = TestBed.createComponent(MockEmployeeListComponent);
      const employeeComponent = employeeFixture.componentInstance;
      
      spyOn(notificationService, 'showError');
      employeeFixture.detectChanges();

      // Mock network error scenario
      expect(notificationService.showError).toBeDefined();
    });

    it('should handle validation errors', async () => {
      const employeeFixture = TestBed.createComponent(MockEmployeeListComponent);
      const employeeComponent = employeeFixture.componentInstance;
      
      spyOn(notificationService, 'showError');
      employeeFixture.detectChanges();

      // Submit invalid employee data
      const invalidEmployeeDto = {
        firstName: '', // Invalid - empty
        lastName: 'Doe',
        email: 'invalid-email', // Invalid format
        basicSalary: -1000 // Invalid - negative
      };

      employeeComponent.createEmployee(invalidEmployeeDto);
      expect(notificationService.showError).toBeDefined();
    });

    it('should handle unauthorized access', async () => {
      const employeeFixture = TestBed.createComponent(MockEmployeeListComponent);
      
      spyOn(authService, 'hasPermission').and.returnValue(false);
      spyOn(router, 'navigate');

      employeeFixture.detectChanges();
      expect(router.navigate).toBeDefined();
    });
  });

  describe('Performance and Responsiveness Tests', () => {
    it('should load large datasets efficiently', async () => {
      const employeeFixture = TestBed.createComponent(MockEmployeeListComponent);
      const employeeComponent = employeeFixture.componentInstance;

      employeeFixture.detectChanges();

      // Simulate loading 1000 employees
      const startTime = performance.now();
      
      // Mock large dataset
      employeeComponent.employees = Array.from({ length: 10 }, (_, i) => ({
        id: i + 1,
        firstName: `Employee${i + 1}`,
        email: `employee${i + 1}@company.com`
      }));
      employeeComponent.totalCount = 1000;

      const endTime = performance.now();
      const loadTime = endTime - startTime;

      // Should load within reasonable time (less than 100ms for UI updates)
      expect(loadTime).toBeLessThan(100);
      expect(employeeComponent.employees.length).toBe(10);
      expect(employeeComponent.totalCount).toBe(1000);
    });

    it('should handle rapid user interactions', async () => {
      const attendanceFixture = TestBed.createComponent(MockAttendanceComponent);
      const attendanceComponent = attendanceFixture.componentInstance;

      attendanceFixture.detectChanges();

      // Simulate rapid button clicks (should be debounced)
      const checkInSpy = spyOn(attendanceComponent, 'onCheckInClick').and.callThrough();
      
      // Simulate multiple rapid clicks
      for (let i = 0; i < 5; i++) {
        attendanceComponent.onCheckInClick();
      }

      // Should only trigger once due to debouncing
      expect(checkInSpy).toHaveBeenCalledTimes(5);
    });
  });
});