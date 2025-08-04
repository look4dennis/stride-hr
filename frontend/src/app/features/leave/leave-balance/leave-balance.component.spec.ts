import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { of, throwError } from 'rxjs';
import { LeaveBalanceComponent } from './leave-balance.component';
import { LeaveService } from '../../../services/leave.service';
import { LeaveBalance, LeaveType } from '../../../models/leave.models';

describe('LeaveBalanceComponent', () => {
  let component: LeaveBalanceComponent;
  let fixture: ComponentFixture<LeaveBalanceComponent>;
  let mockLeaveService: jasmine.SpyObj<LeaveService>;

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
    },
    {
      id: 2,
      employeeId: 1,
      leavePolicyId: 2,
      leaveType: LeaveType.Sick,
      leaveTypeName: 'Sick Leave',
      year: 2024,
      allocatedDays: 10,
      usedDays: 8,
      carriedForwardDays: 0,
      encashedDays: 0,
      remainingDays: 2
    }
  ];

  beforeEach(async () => {
    const leaveServiceSpy = jasmine.createSpyObj('LeaveService', [
      'getMyLeaveBalances',
      'getEmployeeLeaveBalances',
      'getLeaveTypeColor',
      'getLeaveTypeText'
    ]);

    await TestBed.configureTestingModule({
      imports: [NgbModule, LeaveBalanceComponent],
      providers: [
        { provide: LeaveService, useValue: leaveServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LeaveBalanceComponent);
    component = fixture.componentInstance;
    mockLeaveService = TestBed.inject(LeaveService) as jasmine.SpyObj<LeaveService>;

    // Setup default mock returns
    mockLeaveService.getMyLeaveBalances.and.returnValue(of(mockLeaveBalances));
    mockLeaveService.getEmployeeLeaveBalances.and.returnValue(of(mockLeaveBalances));
    mockLeaveService.getLeaveTypeColor.and.returnValue('#28a745');
    mockLeaveService.getLeaveTypeText.and.returnValue('Annual Leave');
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load my leave balances by default', () => {
    fixture.detectChanges();
    
    expect(mockLeaveService.getMyLeaveBalances).toHaveBeenCalled();
    expect(component.leaveBalances).toEqual(mockLeaveBalances);
  });

  it('should load employee leave balances when employeeId is provided', () => {
    component.employeeId = 123;
    fixture.detectChanges();
    
    expect(mockLeaveService.getEmployeeLeaveBalances).toHaveBeenCalledWith(123);
    expect(component.leaveBalances).toEqual(mockLeaveBalances);
  });

  it('should filter balances by current year', () => {
    const balancesWithDifferentYears = [
      ...mockLeaveBalances,
      {
        id: 3,
        employeeId: 1,
        leavePolicyId: 3,
        leaveType: LeaveType.Personal,
        leaveTypeName: 'Personal Leave',
        year: 2023, // Different year
        allocatedDays: 5,
        usedDays: 2,
        carriedForwardDays: 0,
        encashedDays: 0,
        remainingDays: 3
      }
    ];

    mockLeaveService.getMyLeaveBalances.and.returnValue(of(balancesWithDifferentYears));
    fixture.detectChanges();
    
    // Should only show current year balances
    expect(component.leaveBalances.length).toBe(2);
    expect(component.leaveBalances.every(b => b.year === component.currentYear)).toBeTruthy();
  });

  it('should calculate usage percentage correctly', () => {
    fixture.detectChanges();
    
    const balance = mockLeaveBalances[0]; // 5 used out of 20 allocated
    const percentage = component.getUsagePercentage(balance);
    
    expect(percentage).toBe(25); // (5/20) * 100 = 25%
  });

  it('should handle zero allocated days in usage percentage', () => {
    const zeroAllocatedBalance: LeaveBalance = {
      ...mockLeaveBalances[0],
      allocatedDays: 0
    };
    
    const percentage = component.getUsagePercentage(zeroAllocatedBalance);
    expect(percentage).toBe(0);
  });

  it('should return correct progress color based on usage', () => {
    fixture.detectChanges();
    
    // Test different usage scenarios
    const lowUsage = { ...mockLeaveBalances[0], usedDays: 5, allocatedDays: 20 }; // 25%
    const mediumUsage = { ...mockLeaveBalances[0], usedDays: 12, allocatedDays: 20 }; // 60%
    const highUsage = { ...mockLeaveBalances[0], usedDays: 15, allocatedDays: 20 }; // 75%
    const veryHighUsage = { ...mockLeaveBalances[0], usedDays: 19, allocatedDays: 20 }; // 95%
    
    expect(component.getProgressColor(lowUsage)).toBe('#28a745'); // Green
    expect(component.getProgressColor(mediumUsage)).toBe('#ffc107'); // Yellow
    expect(component.getProgressColor(highUsage)).toBe('#fd7e14'); // Orange
    expect(component.getProgressColor(veryHighUsage)).toBe('#dc3545'); // Red
  });

  it('should identify low balance correctly', () => {
    fixture.detectChanges();
    
    const lowBalance = { ...mockLeaveBalances[0], remainingDays: 2 };
    const normalBalance = { ...mockLeaveBalances[0], remainingDays: 10 };
    const zeroBalance = { ...mockLeaveBalances[0], remainingDays: 0 };
    
    expect(component.isLowBalance(lowBalance)).toBeTruthy();
    expect(component.isLowBalance(normalBalance)).toBeFalsy();
    expect(component.isLowBalance(zeroBalance)).toBeFalsy(); // Zero is not considered "low"
  });

  it('should calculate total allocated days', () => {
    fixture.detectChanges();
    
    const total = component.getTotalAllocated();
    expect(total).toBe(30); // 20 + 10
  });

  it('should calculate total used days', () => {
    fixture.detectChanges();
    
    const total = component.getTotalUsed();
    expect(total).toBe(13); // 5 + 8
  });

  it('should calculate total remaining days', () => {
    fixture.detectChanges();
    
    const total = component.getTotalRemaining();
    expect(total).toBe(19); // 17 + 2
  });

  it('should calculate total usage percentage', () => {
    fixture.detectChanges();
    
    const percentage = component.getUsagePercentageTotal();
    expect(percentage).toBe(43); // (13/30) * 100 = 43.33... rounded to 43
  });

  it('should handle zero total allocated in usage percentage', () => {
    component.leaveBalances = [];
    
    const percentage = component.getUsagePercentageTotal();
    expect(percentage).toBe(0);
  });

  it('should refresh balances', () => {
    fixture.detectChanges();
    
    mockLeaveService.getMyLeaveBalances.calls.reset();
    
    component.refreshBalances();
    
    expect(mockLeaveService.getMyLeaveBalances).toHaveBeenCalled();
  });

  it('should download report', () => {
    fixture.detectChanges();
    
    // Mock URL.createObjectURL and document.createElement
    spyOn(URL, 'createObjectURL').and.returnValue('mock-url');
    spyOn(URL, 'revokeObjectURL');
    
    const mockLink = jasmine.createSpyObj('HTMLAnchorElement', ['click']);
    spyOn(document, 'createElement').and.returnValue(mockLink);
    
    component.downloadReport();
    
    expect(URL.createObjectURL).toHaveBeenCalled();
    expect(mockLink.href).toBe('mock-url');
    expect(mockLink.download).toContain('leave-balance-report-');
    expect(mockLink.click).toHaveBeenCalled();
    expect(URL.revokeObjectURL).toHaveBeenCalledWith('mock-url');
  });

  it('should call leave service methods for colors and text', () => {
    fixture.detectChanges();
    
    component.getLeaveTypeColor(LeaveType.Annual);
    component.getLeaveTypeText(LeaveType.Annual);
    
    expect(mockLeaveService.getLeaveTypeColor).toHaveBeenCalledWith(LeaveType.Annual);
    expect(mockLeaveService.getLeaveTypeText).toHaveBeenCalledWith(LeaveType.Annual);
  });

  it('should handle errors when loading balances', () => {
    mockLeaveService.getMyLeaveBalances.and.returnValue(throwError('Error loading balances'));
    spyOn(console, 'error');
    
    fixture.detectChanges();
    
    expect(console.error).toHaveBeenCalledWith('Error loading leave balances:', 'Error loading balances');
    expect(component.isLoading).toBeFalsy();
  });

  it('should set loading state correctly', () => {
    expect(component.isLoading).toBeFalsy();
    
    // Create a delayed observable to test loading state
    let resolveObservable: (value: LeaveBalance[]) => void;
    const delayedObservable = new Promise<LeaveBalance[]>(resolve => {
      resolveObservable = resolve;
    });
    
    mockLeaveService.getMyLeaveBalances.and.returnValue(of(mockLeaveBalances).pipe());
    
    fixture.detectChanges();
    
    // Loading should be false after successful load
    expect(component.isLoading).toBeFalsy();
  });
});