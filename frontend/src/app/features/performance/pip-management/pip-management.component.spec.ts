import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { of, throwError } from 'rxjs';

import { PIPManagementComponent } from './pip-management.component';
import { PerformanceService } from '../../../services/performance.service';
import { EmployeeService } from '../../../services/employee.service';
import { PIP, PIPStatus, ImprovementStatus, MilestoneStatus } from '../../../models/performance.models';
import { Employee } from '../../../models/employee.models';

describe('PIPManagementComponent', () => {
  let component: PIPManagementComponent;
  let fixture: ComponentFixture<PIPManagementComponent>;
  let mockPerformanceService: jasmine.SpyObj<PerformanceService>;
  let mockEmployeeService: jasmine.SpyObj<EmployeeService>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockModalService: jasmine.SpyObj<NgbModal>;

  const mockEmployees: Employee[] = [
    {
      id: 1,
      employeeId: 'EMP001',
      firstName: 'John',
      lastName: 'Doe',
      email: 'john.doe@example.com',
      phone: '1234567890',
      designation: 'Developer',
      department: 'IT',
      branchId: 1,
      basicSalary: 50000,
      dateOfBirth: '1990-01-01',
      joiningDate: '2020-01-01',
      status: 'Active' as any,
      createdAt: '2020-01-01T00:00:00Z',
      reportingManagerId: undefined
    }
  ];

  const mockPIPs: PIP[] = [
    {
      id: 1,
      employeeId: 1,
      managerId: 2,
      title: 'Communication Improvement',
      description: 'Improve communication skills',
      startDate: new Date('2024-01-01'),
      endDate: new Date('2024-06-30'),
      status: PIPStatus.Active,
      improvementAreas: [
        {
          id: 1,
          pipId: 1,
          area: 'Communication',
          currentState: 'Poor communication',
          expectedState: 'Clear communication',
          actionPlan: 'Take communication course',
          progress: 50,
          status: ImprovementStatus.InProgress
        }
      ],
      milestones: [
        {
          id: 1,
          pipId: 1,
          title: 'Complete training',
          description: 'Complete communication training',
          dueDate: new Date('2024-03-31'),
          status: MilestoneStatus.Completed,
          feedback: 'Good progress'
        }
      ],
      supportResources: 'Training materials',
      createdAt: new Date(),
      updatedAt: new Date(),
      employee: {
        id: 1,
        firstName: 'John',
        lastName: 'Doe',
        employeeId: 'EMP001',
        designation: 'Developer'
      }
    }
  ];

  beforeEach(async () => {
    const performanceServiceSpy = jasmine.createSpyObj('PerformanceService', [
      'getPIPs',
      'getPIP',
      'createPIP',
      'updatePIP',
      'updatePIPProgress',
      'completePIP'
    ]);
    const employeeServiceSpy = jasmine.createSpyObj('EmployeeService', ['getEmployees']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    const modalServiceSpy = jasmine.createSpyObj('NgbModal', ['open']);

    await TestBed.configureTestingModule({
      imports: [PIPManagementComponent, ReactiveFormsModule],
      providers: [
        { provide: PerformanceService, useValue: performanceServiceSpy },
        { provide: EmployeeService, useValue: employeeServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: NgbModal, useValue: modalServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PIPManagementComponent);
    component = fixture.componentInstance;
    mockPerformanceService = TestBed.inject(PerformanceService) as jasmine.SpyObj<PerformanceService>;
    mockEmployeeService = TestBed.inject(EmployeeService) as jasmine.SpyObj<EmployeeService>;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    mockModalService = TestBed.inject(NgbModal) as jasmine.SpyObj<NgbModal>;
  });

  beforeEach(() => {
    mockEmployeeService.getEmployees.and.returnValue(of({ items: mockEmployees, totalCount: 1, page: 1, pageSize: 10, totalPages: 1 }));
    mockPerformanceService.getPIPs.and.returnValue(of(mockPIPs));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load employees and PIPs on init', () => {
    component.ngOnInit();

    expect(mockEmployeeService.getEmployees).toHaveBeenCalled();
    expect(mockPerformanceService.getPIPs).toHaveBeenCalled();
    expect(component.employees).toEqual(mockEmployees);
    expect(component.pips).toEqual(mockPIPs);
  });

  it('should create PIP form with default improvement area and milestone', () => {
    const form = component.createPIPForm();

    expect(form.get('employeeId')).toBeTruthy();
    expect(form.get('title')).toBeTruthy();
    expect(form.get('description')).toBeTruthy();
    expect(form.get('startDate')).toBeTruthy();
    expect(form.get('endDate')).toBeTruthy();
    expect(form.get('supportResources')).toBeTruthy();
    expect(form.get('improvementAreas')).toBeTruthy();
    expect(form.get('milestones')).toBeTruthy();
    expect(component.improvementAreasArray.length).toBe(1);
    expect(component.milestonesArray.length).toBe(1);
  });

  it('should add and remove improvement areas', () => {
    component.addImprovementArea();
    expect(component.improvementAreasArray.length).toBe(2);

    component.removeImprovementArea(1);
    expect(component.improvementAreasArray.length).toBe(1);
  });

  it('should add and remove milestones', () => {
    component.addMilestone();
    expect(component.milestonesArray.length).toBe(2);

    component.removeMilestone(1);
    expect(component.milestonesArray.length).toBe(1);
  });

  it('should filter PIPs based on search criteria', () => {
    component.selectedEmployeeId = '1';
    component.selectedStatus = 'Active';

    component.loadPIPs();

    expect(mockPerformanceService.getPIPs).toHaveBeenCalledWith(1, 'Active');
  });

  it('should clear filters', () => {
    component.selectedEmployeeId = '1';
    component.selectedStatus = 'Active';

    component.clearFilters();

    expect(component.selectedEmployeeId).toBe('');
    expect(component.selectedStatus).toBe('');
    expect(mockPerformanceService.getPIPs).toHaveBeenCalled();
  });

  it('should open create modal', () => {
    const mockModalRef = { close: jasmine.createSpy(), dismiss: jasmine.createSpy() };
    mockModalService.open.and.returnValue(mockModalRef as any);

    component.openCreateModal();

    expect(component.isEditMode).toBeFalse();
    expect(component.currentPIP).toBeNull();
    expect(mockModalService.open).toHaveBeenCalled();
  });

  it('should populate form for editing', () => {
    const pip = mockPIPs[0];
    component.editPIP(pip);

    expect(component.isEditMode).toBeTrue();
    expect(component.currentPIP).toEqual(pip);
    expect(component.pipForm.get('employeeId')?.value).toBe(pip.employeeId);
    expect(component.pipForm.get('title')?.value).toBe(pip.title);
  });

  it('should save new PIP', () => {
    const mockModalRef = { close: jasmine.createSpy(), dismiss: jasmine.createSpy() };
    const newPIP = { ...mockPIPs[0], id: 2 };
    
    mockPerformanceService.createPIP.and.returnValue(of(newPIP));
    
    component.pipForm.patchValue({
      employeeId: 1,
      title: 'New PIP',
      description: 'New PIP description',
      startDate: '2024-01-01',
      endDate: '2024-06-30',
      supportResources: 'Resources'
    });

    component.savePIP(mockModalRef);

    expect(mockPerformanceService.createPIP).toHaveBeenCalled();
    expect(mockModalRef.close).toHaveBeenCalled();
  });

  it('should handle save PIP error', () => {
    const mockModalRef = { close: jasmine.createSpy(), dismiss: jasmine.createSpy() };
    const consoleSpy = spyOn(console, 'error');
    
    mockPerformanceService.createPIP.and.returnValue(throwError('Error'));
    
    component.pipForm.patchValue({
      employeeId: 1,
      title: 'New PIP',
      description: 'New PIP description',
      startDate: '2024-01-01',
      endDate: '2024-06-30',
      supportResources: 'Resources'
    });

    component.savePIP(mockModalRef);

    expect(consoleSpy).toHaveBeenCalledWith('Error saving PIP:', 'Error');
    expect(component.loading).toBeFalse();
  });

  it('should navigate to view PIP', () => {
    const pip = mockPIPs[0];
    component.viewPIP(pip);

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/performance/pips', pip.id]);
  });

  it('should navigate to update progress', () => {
    const pip = mockPIPs[0];
    component.updateProgress(pip);

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/performance/pips', pip.id, 'progress']);
  });

  it('should get correct status badge class', () => {
    expect(component.getStatusBadgeClass(PIPStatus.Active)).toBe('bg-primary');
    expect(component.getStatusBadgeClass(PIPStatus.OnTrack)).toBe('bg-success');
    expect(component.getStatusBadgeClass(PIPStatus.AtRisk)).toBe('bg-warning');
    expect(component.getStatusBadgeClass(PIPStatus.Completed)).toBe('bg-success');
    expect(component.getStatusBadgeClass(PIPStatus.Terminated)).toBe('bg-danger');
  });

  it('should calculate overall progress correctly', () => {
    const pip = mockPIPs[0];
    const progress = component.getOverallProgress(pip);
    expect(progress).toBe(50); // Average of improvement areas progress
  });

  it('should get correct progress bar class', () => {
    expect(component.getProgressBarClass(85)).toBe('bg-success');
    expect(component.getProgressBarClass(65)).toBe('bg-warning');
    expect(component.getProgressBarClass(45)).toBe('bg-danger');
  });

  it('should count completed milestones correctly', () => {
    const milestones = [
      { status: MilestoneStatus.Completed },
      { status: MilestoneStatus.InProgress },
      { status: MilestoneStatus.Completed }
    ];

    const completed = component.getCompletedMilestones(milestones);
    expect(completed).toBe(2);
  });

  it('should count overdue milestones correctly', () => {
    const milestones = [
      { status: MilestoneStatus.Overdue },
      { status: MilestoneStatus.InProgress },
      { status: MilestoneStatus.Overdue }
    ];

    const overdue = component.getOverdueMilestones(milestones);
    expect(overdue).toBe(2);
  });

  it('should handle empty improvement areas', () => {
    const pipWithoutAreas = { ...mockPIPs[0], improvementAreas: [] };
    const progress = component.getOverallProgress(pipWithoutAreas);
    expect(progress).toBe(0);
  });

  it('should handle null milestones', () => {
    expect(component.getCompletedMilestones(null as any)).toBe(0);
    expect(component.getOverdueMilestones(null as any)).toBe(0);
  });
});