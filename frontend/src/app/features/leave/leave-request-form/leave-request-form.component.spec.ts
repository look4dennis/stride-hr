import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { NgbModule, NgbCalendar } from '@ng-bootstrap/ng-bootstrap';
import { of, throwError } from 'rxjs';
import { LeaveRequestFormComponent } from './leave-request-form.component';
import { LeaveService } from '../../../services/leave.service';
import { 
  LeavePolicy, 
  LeaveBalance, 
  LeaveConflict, 
  LeaveType, 
  CreateLeaveRequest 
} from '../../../models/leave.models';

describe('LeaveRequestFormComponent', () => {
  let component: LeaveRequestFormComponent;
  let fixture: ComponentFixture<LeaveRequestFormComponent>;
  let mockLeaveService: jasmine.SpyObj<LeaveService>;
  let mockCalendar: jasmine.SpyObj<NgbCalendar>;

  const mockLeavePolicies: LeavePolicy[] = [
    {
      id: 1,
      branchId: 1,
      leaveType: LeaveType.Annual,
      name: 'Annual Leave',
      description: 'Annual vacation leave',
      annualAllocation: 20,
      maxConsecutiveDays: 10,
      minAdvanceNoticeDays: 7,
      requiresApproval: true,
      isCarryForwardAllowed: true,
      maxCarryForwardDays: 5,
      isEncashmentAllowed: true,
      encashmentRate: 1.0,
      isActive: true
    }
  ];

  const mockLeaveBalances: LeaveBalance[] = [
    {
      id: 1,
      employeeId: 1,
      leavePolicyId: 1,
      leaveType: LeaveType.Annual,
      leaveTypeName: 'Annual Leave',
      year: 2024,
      allocatedDays: 20,
      usedDays: 5,
      carriedForwardDays: 2,
      encashedDays: 0,
      remainingDays: 17
    }
  ];

  beforeEach(async () => {
    const leaveServiceSpy = jasmine.createSpyObj('LeaveService', [
      'getLeavePolicies',
      'getMyLeaveBalances',
      'calculateLeaveDays',
      'detectLeaveConflicts'
    ]);

    const calendarSpy = jasmine.createSpyObj('NgbCalendar', ['getToday', 'isValid']);

    await TestBed.configureTestingModule({
      imports: [ReactiveFormsModule, NgbModule, LeaveRequestFormComponent],
      providers: [
        { provide: LeaveService, useValue: leaveServiceSpy },
        { provide: NgbCalendar, useValue: calendarSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LeaveRequestFormComponent);
    component = fixture.componentInstance;
    mockLeaveService = TestBed.inject(LeaveService) as jasmine.SpyObj<LeaveService>;
    mockCalendar = TestBed.inject(NgbCalendar) as jasmine.SpyObj<NgbCalendar>;

    // Setup default mock returns
    mockLeaveService.getLeavePolicies.and.returnValue(of(mockLeavePolicies));
    mockLeaveService.getMyLeaveBalances.and.returnValue(of(mockLeaveBalances));
    mockLeaveService.calculateLeaveDays.and.returnValue(of(3));
    mockLeaveService.detectLeaveConflicts.and.returnValue(of([]));
    mockCalendar.isValid.and.returnValue(true);
    // mockCalendar.getToday.and.returnValue({ year: 2024, month: 1, day: 1 });
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with default values', () => {
    fixture.detectChanges();
    
    expect(component.leaveForm).toBeTruthy();
    expect(component.leaveForm.get('leavePolicyId')?.value).toBe('');
    expect(component.leaveForm.get('reason')?.value).toBe('');
    expect(component.leaveForm.get('isEmergency')?.value).toBe(false);
  });

  it('should load leave policies on init', () => {
    fixture.detectChanges();
    
    expect(mockLeaveService.getLeavePolicies).toHaveBeenCalled();
    expect(component.leavePolicies).toEqual(mockLeavePolicies);
  });

  it('should load leave balances on init', () => {
    fixture.detectChanges();
    
    expect(mockLeaveService.getMyLeaveBalances).toHaveBeenCalled();
    expect(component.leaveBalances).toEqual(mockLeaveBalances);
  });

  it('should validate required fields', () => {
    fixture.detectChanges();
    
    const form = component.leaveForm;
    expect(form.valid).toBeFalsy();
    
    // Test required field validation
    expect(form.get('leavePolicyId')?.hasError('required')).toBeTruthy();
    expect(form.get('startDate')?.hasError('required')).toBeTruthy();
    expect(form.get('endDate')?.hasError('required')).toBeTruthy();
    expect(form.get('reason')?.hasError('required')).toBeTruthy();
  });

  it('should validate reason minimum length', () => {
    fixture.detectChanges();
    
    const reasonControl = component.leaveForm.get('reason');
    reasonControl?.setValue('short');
    
    expect(reasonControl?.hasError('minlength')).toBeTruthy();
    
    reasonControl?.setValue('This is a valid reason with more than 10 characters');
    expect(reasonControl?.hasError('minlength')).toBeFalsy();
  });

  it('should calculate leave days when dates change', () => {
    fixture.detectChanges();
    
    const startDate = { year: 2024, month: 1, day: 15 };
    const endDate = { year: 2024, month: 1, day: 17 };
    
    component.leaveForm.patchValue({
      leavePolicyId: 1,
      startDate: startDate,
      endDate: endDate
    });
    
    // Trigger change detection
    fixture.detectChanges();
    
    // Wait for debounced call
    setTimeout(() => {
      expect(mockLeaveService.calculateLeaveDays).toHaveBeenCalled();
      expect(component.calculatedDays).toBe(3);
    }, 400);
  });

  it('should detect conflicts when dates change', () => {
    fixture.detectChanges();
    
    const conflicts: LeaveConflict[] = [
      {
        employeeId: 2,
        employeeName: 'Jane Smith',
        department: 'Engineering',
        conflictDate: new Date('2024-01-16'),
        conflictReason: 'Already on Annual Leave',
        conflictingRequestId: 2
      }
    ];
    
    mockLeaveService.detectLeaveConflicts.and.returnValue(of(conflicts));
    
    const startDate = { year: 2024, month: 1, day: 15 };
    const endDate = { year: 2024, month: 1, day: 17 };
    
    component.leaveForm.patchValue({
      leavePolicyId: 1,
      startDate: startDate,
      endDate: endDate
    });
    
    fixture.detectChanges();
    
    setTimeout(() => {
      expect(mockLeaveService.detectLeaveConflicts).toHaveBeenCalled();
      expect(component.conflicts).toEqual(conflicts);
    }, 400);
  });

  it('should get available balance for policy', () => {
    fixture.detectChanges();
    
    const balance = component.getAvailableBalance(1);
    expect(balance).toBe(17);
    
    const nonExistentBalance = component.getAvailableBalance(999);
    expect(nonExistentBalance).toBe(0);
  });

  it('should validate file selection', () => {
    fixture.detectChanges();
    
    // Mock file that's too large
    const largeFile = new File([''], 'large.pdf', { 
      type: 'application/pdf',
      lastModified: Date.now()
    });
    Object.defineProperty(largeFile, 'size', { value: 6 * 1024 * 1024 }); // 6MB
    
    spyOn(window, 'alert');
    
    const event = { target: { files: [largeFile] } };
    component.onFileSelected(event);
    
    expect(window.alert).toHaveBeenCalledWith('File size must be less than 5MB');
    expect(component.selectedFile).toBeUndefined();
  });

  it('should validate file type', () => {
    fixture.detectChanges();
    
    // Mock invalid file type
    const invalidFile = new File([''], 'test.txt', { 
      type: 'text/plain',
      lastModified: Date.now()
    });
    
    spyOn(window, 'alert');
    
    const event = { target: { files: [invalidFile] } };
    component.onFileSelected(event);
    
    expect(window.alert).toHaveBeenCalledWith('Please select a valid file type (PDF, DOC, DOCX, JPG, PNG)');
    expect(component.selectedFile).toBeUndefined();
  });

  it('should accept valid file', () => {
    fixture.detectChanges();
    
    const validFile = new File([''], 'document.pdf', { 
      type: 'application/pdf',
      lastModified: Date.now()
    });
    Object.defineProperty(validFile, 'size', { value: 1024 * 1024 }); // 1MB
    
    const event = { target: { files: [validFile] } };
    component.onFileSelected(event);
    
    expect(component.selectedFile).toBe(validFile);
  });

  it('should emit form submit when valid', () => {
    fixture.detectChanges();
    
    spyOn(component.formSubmit, 'emit');
    
    // Fill form with valid data
    component.leaveForm.patchValue({
      leavePolicyId: 1,
      startDate: { year: 2024, month: 1, day: 15 },
      endDate: { year: 2024, month: 1, day: 17 },
      reason: 'Valid reason for leave request',
      comments: 'Additional comments',
      isEmergency: false
    });
    
    component.onSubmit();
    
    expect(component.formSubmit.emit).toHaveBeenCalled();
    const emittedValue = (component.formSubmit.emit as jasmine.Spy).calls.mostRecent().args[0];
    expect(emittedValue.leavePolicyId).toBe(1);
    expect(emittedValue.reason).toBe('Valid reason for leave request');
  });

  it('should not submit when form is invalid', () => {
    fixture.detectChanges();
    
    spyOn(component.formSubmit, 'emit');
    
    // Form is invalid by default
    component.onSubmit();
    
    expect(component.formSubmit.emit).not.toHaveBeenCalled();
  });

  it('should emit cancel event', () => {
    fixture.detectChanges();
    
    spyOn(component.formCancel, 'emit');
    
    component.onCancel();
    
    expect(component.formCancel.emit).toHaveBeenCalled();
  });

  it('should check if field is invalid', () => {
    fixture.detectChanges();
    
    const reasonControl = component.leaveForm.get('reason');
    reasonControl?.markAsTouched();
    reasonControl?.setValue('');
    
    expect(component.isFieldInvalid('reason')).toBeTruthy();
    
    reasonControl?.setValue('Valid reason');
    expect(component.isFieldInvalid('reason')).toBeFalsy();
  });

  it('should handle errors when loading policies', () => {
    mockLeaveService.getLeavePolicies.and.returnValue(throwError('Error loading policies'));
    spyOn(console, 'error');
    
    fixture.detectChanges();
    
    expect(console.error).toHaveBeenCalledWith('Error loading leave policies:', 'Error loading policies');
  });

  it('should handle errors when loading balances', () => {
    mockLeaveService.getMyLeaveBalances.and.returnValue(throwError('Error loading balances'));
    spyOn(console, 'error');
    
    fixture.detectChanges();
    
    expect(console.error).toHaveBeenCalledWith('Error loading leave balances:', 'Error loading balances');
  });
});