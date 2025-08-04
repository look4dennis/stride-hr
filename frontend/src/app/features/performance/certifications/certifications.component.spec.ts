import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { CertificationsComponent } from './certifications.component';
import { PerformanceService } from '../../../services/performance.service';
import { EmployeeService } from '../../../services/employee.service';
import { Certification, EmployeeTraining, TrainingStatus } from '../../../models/performance.models';
import { Employee } from '../../../models/employee.models';

describe('CertificationsComponent', () => {
  let component: CertificationsComponent;
  let fixture: ComponentFixture<CertificationsComponent>;
  let mockPerformanceService: jasmine.SpyObj<PerformanceService>;
  let mockEmployeeService: jasmine.SpyObj<EmployeeService>;
  let mockRouter: jasmine.SpyObj<Router>;

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

  const mockCertifications: Certification[] = [
    {
      id: 1,
      employeeId: 1,
      trainingModuleId: 1,
      certificateNumber: 'CERT-001',
      issuedDate: new Date('2024-01-15'),
      expiryDate: new Date('2025-01-15'),
      score: 85,
      certificateUrl: 'http://example.com/cert1.pdf',
      isValid: true,
      employee: {
        id: 1,
        firstName: 'John',
        lastName: 'Doe',
        employeeId: 'EMP001'
      },
      trainingModule: {
        id: 1,
        title: 'Communication Skills',
        category: 'Soft Skills'
      }
    },
    {
      id: 2,
      employeeId: 1,
      trainingModuleId: 2,
      certificateNumber: 'CERT-002',
      issuedDate: new Date('2023-01-15'),
      expiryDate: new Date('2024-01-15'),
      score: 92,
      certificateUrl: 'http://example.com/cert2.pdf',
      isValid: false,
      employee: {
        id: 1,
        firstName: 'John',
        lastName: 'Doe',
        employeeId: 'EMP001'
      },
      trainingModule: {
        id: 2,
        title: 'Technical Skills',
        category: 'Technical'
      }
    }
  ];

  const mockTrainings: EmployeeTraining[] = [
    {
      id: 1,
      employeeId: 1,
      trainingModuleId: 1,
      enrolledDate: new Date('2024-01-01'),
      startedDate: new Date('2024-01-02'),
      completedDate: new Date('2024-01-15'),
      status: TrainingStatus.Completed,
      progress: 100,
      score: 85,
      attempts: 1,
      certificateIssued: true,
      certificateUrl: 'http://example.com/cert1.pdf',
      employee: {
        id: 1,
        firstName: 'John',
        lastName: 'Doe',
        employeeId: 'EMP001'
      },
      trainingModule: {
        id: 1,
        title: 'Communication Skills',
        description: 'Learn effective communication',
        category: 'Soft Skills',
        duration: 120,
        difficulty: 'Beginner' as any,
        content: 'Training content',
        materials: [],
        assessments: [],
        prerequisites: [],
        isActive: true,
        createdBy: 1,
        createdAt: new Date(),
        updatedAt: new Date()
      }
    }
  ];

  beforeEach(async () => {
    const performanceServiceSpy = jasmine.createSpyObj('PerformanceService', [
      'getCertifications',
      'getCertification',
      'downloadCertificate',
      'getEmployeeTrainings',
      'startTraining'
    ]);
    const employeeServiceSpy = jasmine.createSpyObj('EmployeeService', ['getEmployees']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [CertificationsComponent, FormsModule],
      providers: [
        { provide: PerformanceService, useValue: performanceServiceSpy },
        { provide: EmployeeService, useValue: employeeServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CertificationsComponent);
    component = fixture.componentInstance;
    mockPerformanceService = TestBed.inject(PerformanceService) as jasmine.SpyObj<PerformanceService>;
    mockEmployeeService = TestBed.inject(EmployeeService) as jasmine.SpyObj<EmployeeService>;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
  });

  beforeEach(() => {
    mockEmployeeService.getEmployees.and.returnValue(of({ items: mockEmployees, totalCount: 1, page: 1, pageSize: 10, totalPages: 1 }));
    mockPerformanceService.getCertifications.and.returnValue(of(mockCertifications));
    mockPerformanceService.getEmployeeTrainings.and.returnValue(of(mockTrainings));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load employees and data on init', () => {
    component.ngOnInit();

    expect(mockEmployeeService.getEmployees).toHaveBeenCalled();
    expect(component.employees).toEqual(mockEmployees);
    expect(component.viewMode).toBe('certifications');
  });

  it('should load certifications when view mode is certifications', () => {
    component.viewMode = 'certifications';
    component.loadData();

    expect(mockPerformanceService.getCertifications).toHaveBeenCalled();
    expect(component.certifications).toEqual(mockCertifications);
  });

  it('should load trainings when view mode is progress', () => {
    component.viewMode = 'progress';
    component.loadData();

    expect(mockPerformanceService.getEmployeeTrainings).toHaveBeenCalled();
    expect(component.trainings).toEqual(mockTrainings);
  });

  it('should filter certifications by status', () => {
    component.viewMode = 'certifications';
    component.selectedStatus = 'true'; // Valid certificates only
    component.loadCertifications();

    expect(mockPerformanceService.getCertifications).toHaveBeenCalled();
    // Should filter to only valid certificates
    expect(component.certifications.length).toBe(1);
    expect(component.certifications[0].isValid).toBe(true);
  });

  it('should filter certifications by search term', () => {
    component.viewMode = 'certifications';
    component.searchTerm = 'communication';
    component.loadCertifications();

    expect(mockPerformanceService.getCertifications).toHaveBeenCalled();
    // Should filter to only certificates matching search term
    expect(component.certifications.length).toBe(1);
    expect(component.certifications[0].trainingModule?.title).toBe('Communication Skills');
  });

  it('should filter trainings by status', () => {
    component.viewMode = 'progress';
    component.selectedTrainingStatus = 'Completed';
    component.loadTrainings();

    expect(mockPerformanceService.getEmployeeTrainings).toHaveBeenCalledWith(undefined, 'Completed');
  });

  it('should clear filters', () => {
    component.selectedEmployeeId = '1';
    component.selectedStatus = 'true';
    component.selectedTrainingStatus = 'Completed';
    component.searchTerm = 'test';

    component.clearFilters();

    expect(component.selectedEmployeeId).toBe('');
    expect(component.selectedStatus).toBe('');
    expect(component.selectedTrainingStatus).toBe('');
    expect(component.searchTerm).toBe('');
  });

  it('should download certificate', () => {
    const mockBlob = new Blob(['certificate content'], { type: 'application/pdf' });
    mockPerformanceService.downloadCertificate.and.returnValue(of(mockBlob));
    
    // Mock URL and link creation
    spyOn(window.URL, 'createObjectURL').and.returnValue('blob:url');
    spyOn(window.URL, 'revokeObjectURL');
    const mockLink = jasmine.createSpyObj('a', ['click']);
    spyOn(document, 'createElement').and.returnValue(mockLink);

    const certification = mockCertifications[0];
    component.downloadCertificate(certification);

    expect(mockPerformanceService.downloadCertificate).toHaveBeenCalledWith(certification.id);
    expect(window.URL.createObjectURL).toHaveBeenCalledWith(mockBlob);
    expect(mockLink.href).toBe('blob:url');
    expect(mockLink.download).toBe(`certificate-${certification.certificateNumber}.pdf`);
    expect(mockLink.click).toHaveBeenCalled();
    expect(window.URL.revokeObjectURL).toHaveBeenCalledWith('blob:url');
  });

  it('should handle download certificate error', () => {
    const consoleSpy = spyOn(console, 'error');
    mockPerformanceService.downloadCertificate.and.returnValue(throwError('Download error'));

    const certification = mockCertifications[0];
    component.downloadCertificate(certification);

    expect(consoleSpy).toHaveBeenCalledWith('Error downloading certificate:', 'Download error');
  });

  it('should download certificate from training', () => {
    const mockLink = jasmine.createSpyObj('a', ['click']);
    spyOn(document, 'createElement').and.returnValue(mockLink);

    const training = mockTrainings[0];
    component.downloadCertificateFromTraining(training);

    expect(mockLink.href).toBe(training.certificateUrl!);
    expect(mockLink.download).toBe(`certificate-${training.employee?.employeeId}-${training.trainingModule?.title}.pdf`);
    expect(mockLink.click).toHaveBeenCalled();
  });

  it('should navigate to training details', () => {
    const training = mockTrainings[0];
    component.viewTrainingDetails(training);

    expect(mockRouter.navigate).toHaveBeenCalledWith(['/performance/training/enrollments', training.id]);
  });

  it('should start training', () => {
    const updatedTraining = { ...mockTrainings[0], status: TrainingStatus.InProgress };
    mockPerformanceService.startTraining.and.returnValue(of(updatedTraining));

    const training = mockTrainings[0];
    component.startTraining(training);

    expect(mockPerformanceService.startTraining).toHaveBeenCalledWith(training.id);
  });

  it('should handle start training error', () => {
    const consoleSpy = spyOn(console, 'error');
    mockPerformanceService.startTraining.and.returnValue(throwError('Start error'));

    const training = mockTrainings[0];
    component.startTraining(training);

    expect(consoleSpy).toHaveBeenCalledWith('Error starting training:', 'Start error');
  });

  it('should get correct training status badge class', () => {
    expect(component.getTrainingStatusBadgeClass(TrainingStatus.NotStarted)).toBe('bg-secondary');
    expect(component.getTrainingStatusBadgeClass(TrainingStatus.InProgress)).toBe('bg-primary');
    expect(component.getTrainingStatusBadgeClass(TrainingStatus.Completed)).toBe('bg-success');
    expect(component.getTrainingStatusBadgeClass(TrainingStatus.Failed)).toBe('bg-danger');
    expect(component.getTrainingStatusBadgeClass(TrainingStatus.Expired)).toBe('bg-warning');
  });

  it('should get correct progress bar class', () => {
    expect(component.getProgressBarClass(85)).toBe('bg-success');
    expect(component.getProgressBarClass(65)).toBe('bg-warning');
    expect(component.getProgressBarClass(45)).toBe('bg-danger');
  });

  it('should switch view modes', () => {
    component.viewMode = 'certifications';
    expect(component.viewMode).toBe('certifications');

    component.viewMode = 'progress';
    expect(component.viewMode).toBe('progress');
  });

  it('should filter certifications by employee', () => {
    component.selectedEmployeeId = '1';
    component.loadCertifications();

    expect(mockPerformanceService.getCertifications).toHaveBeenCalledWith(1);
  });

  it('should filter trainings by employee', () => {
    component.selectedEmployeeId = '1';
    component.loadTrainings();

    expect(mockPerformanceService.getEmployeeTrainings).toHaveBeenCalledWith(1, undefined);
  });
});