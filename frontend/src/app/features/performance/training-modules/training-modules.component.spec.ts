import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { of, throwError } from 'rxjs';

import { TrainingModulesComponent } from './training-modules.component';
import { PerformanceService } from '../../../services/performance.service';
import { EmployeeService } from '../../../services/employee.service';
import { TrainingModule, TrainingDifficulty, EmployeeTraining, TrainingStatus } from '../../../models/performance.models';
import { Employee } from '../../../models/employee.models';

describe('TrainingModulesComponent', () => {
  let component: TrainingModulesComponent;
  let fixture: ComponentFixture<TrainingModulesComponent>;
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

  const mockModules: TrainingModule[] = [
    {
      id: 1,
      title: 'Communication Skills',
      description: 'Learn effective communication techniques',
      category: 'Soft Skills',
      duration: 120,
      difficulty: TrainingDifficulty.Beginner,
      content: 'Training content here',
      materials: [],
      assessments: [],
      prerequisites: [],
      isActive: true,
      createdBy: 1,
      createdAt: new Date(),
      updatedAt: new Date()
    }
  ];

  const mockEnrollments: EmployeeTraining[] = [
    {
      id: 1,
      employeeId: 1,
      trainingModuleId: 1,
      enrolledDate: new Date(),
      status: TrainingStatus.Completed,
      progress: 100,
      score: 85,
      attempts: 1,
      certificateIssued: true
    },
    {
      id: 2,
      employeeId: 2,
      trainingModuleId: 1,
      enrolledDate: new Date(),
      status: TrainingStatus.InProgress,
      progress: 50,
      attempts: 1,
      certificateIssued: false
    }
  ];

  beforeEach(async () => {
    const performanceServiceSpy = jasmine.createSpyObj('PerformanceService', [
      'getTrainingModules',
      'getTrainingModule',
      'createTrainingModule',
      'updateTrainingModule',
      'deleteTrainingModule',
      'getEmployeeTrainings',
      'enrollEmployee'
    ]);
    const employeeServiceSpy = jasmine.createSpyObj('EmployeeService', ['getEmployees']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    const modalServiceSpy = jasmine.createSpyObj('NgbModal', ['open']);
    modalServiceSpy.open.and.returnValue({
      result: Promise.resolve(),
      close: jasmine.createSpy('close'),
      dismiss: jasmine.createSpy('dismiss')
    });

    await TestBed.configureTestingModule({
      imports: [TrainingModulesComponent, ReactiveFormsModule],
      providers: [
        { provide: PerformanceService, useValue: performanceServiceSpy },
        { provide: EmployeeService, useValue: employeeServiceSpy },
        { provide: Router, useValue: routerSpy },
        { provide: NgbModal, useValue: modalServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TrainingModulesComponent);
    component = fixture.componentInstance;
    mockPerformanceService = TestBed.inject(PerformanceService) as jasmine.SpyObj<PerformanceService>;
    mockEmployeeService = TestBed.inject(EmployeeService) as jasmine.SpyObj<EmployeeService>;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    mockModalService = TestBed.inject(NgbModal) as jasmine.SpyObj<NgbModal>;
  });

  beforeEach(() => {
    mockEmployeeService.getEmployees.and.returnValue(of({ items: mockEmployees, totalCount: 1, page: 1, pageSize: 10, totalPages: 1 }));
    mockPerformanceService.getTrainingModules.and.returnValue(of(mockModules));
    mockPerformanceService.getEmployeeTrainings.and.returnValue(of(mockEnrollments));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load data on init', () => {
    component.ngOnInit();

    expect(mockPerformanceService.getTrainingModules).toHaveBeenCalled();
    expect(mockEmployeeService.getEmployees).toHaveBeenCalled();
    expect(mockPerformanceService.getEmployeeTrainings).toHaveBeenCalled();
    expect(component.modules).toEqual(mockModules);
    expect(component.employees).toEqual(mockEmployees);
    expect(component.enrollments).toEqual(mockEnrollments);
  });

  it('should create module form', () => {
    const form = component.createModuleForm();

    expect(form.get('title')).toBeTruthy();
    expect(form.get('description')).toBeTruthy();
    expect(form.get('category')).toBeTruthy();
    expect(form.get('duration')).toBeTruthy();
    expect(form.get('difficulty')).toBeTruthy();
    expect(form.get('content')).toBeTruthy();
    expect(form.get('prerequisites')).toBeTruthy();
  });

  it('should create enroll form', () => {
    const form = component.createEnrollForm();

    expect(form.get('employeeId')).toBeTruthy();
    expect(form.get('trainingModuleId')).toBeTruthy();
  });

  it('should filter modules based on search criteria', () => {
    component.selectedCategory = 'Soft Skills';
    component.selectedDifficulty = 'Beginner';
    component.searchTerm = 'communication';

    component.loadModules();

    expect(mockPerformanceService.getTrainingModules).toHaveBeenCalledWith('Soft Skills', 'Beginner');
  });

  it('should clear filters', () => {
    component.selectedCategory = 'Soft Skills';
    component.selectedDifficulty = 'Beginner';
    component.searchTerm = 'communication';

    component.clearFilters();

    expect(component.selectedCategory).toBe('');
    expect(component.selectedDifficulty).toBe('');
    expect(component.searchTerm).toBe('');
    expect(mockPerformanceService.getTrainingModules).toHaveBeenCalled();
  });

  it('should open create module modal', () => {
    const mockModalRef = { 
      close: jasmine.createSpy(), 
      dismiss: jasmine.createSpy(),
      result: Promise.resolve()
    };
    mockModalService.open.and.returnValue(mockModalRef as any);
    
    // Initialize component and ViewChild
    component.ngOnInit();
    fixture.detectChanges();

    // Create a proper mock template reference
    const mockTemplateRef = {
      createEmbeddedView: jasmine.createSpy('createEmbeddedView'),
      elementRef: { nativeElement: document.createElement('div') }
    };
    component.moduleModal = mockTemplateRef as any;

    component.openCreateModuleModal();

    expect(component.isEditMode).toBeFalse();
    expect(component.currentModule).toBeNull();
    expect(mockModalService.open).toHaveBeenCalledWith(mockTemplateRef, { size: 'lg' });
  });

  it('should edit module', () => {
    const module = mockModules[0];
    const mockModalRef = { 
      close: jasmine.createSpy(), 
      dismiss: jasmine.createSpy(),
      result: Promise.resolve()
    };
    mockModalService.open.and.returnValue(mockModalRef as any);
    
    // Initialize component and ViewChild
    component.ngOnInit();
    fixture.detectChanges();

    // Create a proper mock template reference
    const mockTemplateRef = {
      createEmbeddedView: jasmine.createSpy('createEmbeddedView'),
      elementRef: { nativeElement: document.createElement('div') }
    };
    component.moduleModal = mockTemplateRef as any;

    component.editModule(module);

    expect(component.isEditMode).toBeTrue();
    expect(component.currentModule).toEqual(module);
    expect(component.moduleForm.get('title')?.value).toBe(module.title);
    expect(mockModalService.open).toHaveBeenCalledWith(mockTemplateRef, { size: 'lg' });
  });

  it('should save new module', () => {
    const mockModalRef = { close: jasmine.createSpy(), dismiss: jasmine.createSpy() };
    const newModule = { ...mockModules[0], id: 2 };
    
    mockPerformanceService.createTrainingModule.and.returnValue(of(newModule));
    
    component.moduleForm.patchValue({
      title: 'New Module',
      description: 'New module description',
      category: 'Technical',
      duration: 60,
      difficulty: 'Intermediate',
      content: 'Module content',
      prerequisites: []
    });

    component.saveModule(mockModalRef);

    expect(mockPerformanceService.createTrainingModule).toHaveBeenCalled();
    expect(mockModalRef.close).toHaveBeenCalled();
  });

  it('should handle save module error', () => {
    const mockModalRef = { close: jasmine.createSpy(), dismiss: jasmine.createSpy() };
    const consoleSpy = spyOn(console, 'error');
    
    mockPerformanceService.createTrainingModule.and.returnValue(throwError('Error'));
    
    component.moduleForm.patchValue({
      title: 'New Module',
      description: 'New module description',
      category: 'Technical',
      duration: 60,
      difficulty: 'Intermediate',
      content: 'Module content',
      prerequisites: []
    });

    component.saveModule(mockModalRef);

    expect(consoleSpy).toHaveBeenCalledWith('Error saving module:', 'Error');
    expect(component.loading).toBeFalse();
  });

  it('should enroll employee', () => {
    const mockModalRef = { close: jasmine.createSpy(), dismiss: jasmine.createSpy() };
    const newEnrollment = mockEnrollments[0];
    
    mockPerformanceService.enrollEmployee.and.returnValue(of(newEnrollment));
    
    component.enrollForm.patchValue({
      employeeId: 1,
      trainingModuleId: 1
    });

    component.enrollEmployee(mockModalRef);

    expect(mockPerformanceService.enrollEmployee).toHaveBeenCalledWith({
      employeeId: 1,
      trainingModuleId: 1
    });
    expect(mockModalRef.close).toHaveBeenCalled();
  });

  it('should navigate to view module', () => {
    const module = mockModules[0];
    component.viewModule(module);

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/performance/training/modules', module.id]);
  });

  it('should navigate to view enrollments', () => {
    const module = mockModules[0];
    component.viewEnrollments(module);

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/performance/training/modules', module.id, 'enrollments']);
  });

  it('should get correct difficulty badge class', () => {
    expect(component.getDifficultyBadgeClass(TrainingDifficulty.Beginner)).toBe('bg-success');
    expect(component.getDifficultyBadgeClass(TrainingDifficulty.Intermediate)).toBe('bg-primary');
    expect(component.getDifficultyBadgeClass(TrainingDifficulty.Advanced)).toBe('bg-warning');
    expect(component.getDifficultyBadgeClass(TrainingDifficulty.Expert)).toBe('bg-danger');
  });

  it('should calculate enrollment stats correctly', () => {
    component.enrollments = mockEnrollments; // Ensure enrollments are set
    const stats = component.getEnrollmentStats(1);
    
    expect(stats.total).toBe(2);
    expect(stats.completed).toBe(1);
    expect(stats.inProgress).toBe(1);
  });

  it('should calculate completion rate correctly', () => {
    component.enrollments = mockEnrollments; // Ensure enrollments are set
    const completionRate = component.getCompletionRate(1);
    expect(completionRate).toBe(50); // 1 completed out of 2 total
  });

  it('should handle module with no enrollments', () => {
    const stats = component.getEnrollmentStats(999); // Non-existent module
    
    expect(stats.total).toBe(0);
    expect(stats.completed).toBe(0);
    expect(stats.inProgress).toBe(0);
    
    const completionRate = component.getCompletionRate(999);
    expect(completionRate).toBe(0);
  });

  it('should filter modules by search term', () => {
    component.searchTerm = 'communication';
    mockPerformanceService.getTrainingModules.and.returnValue(of([
      mockModules[0],
      {
        ...mockModules[0],
        id: 2,
        title: 'Leadership Skills',
        description: 'Learn leadership techniques'
      }
    ]));

    component.loadModules();

    // Should filter to only include modules with 'communication' in title or description
    expect(component.modules.length).toBe(1);
    expect(component.modules[0].title).toBe('Communication Skills');
  });
});