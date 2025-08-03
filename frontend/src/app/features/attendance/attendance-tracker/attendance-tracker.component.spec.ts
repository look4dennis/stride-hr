import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AttendanceTrackerComponent } from './attendance-tracker.component';
import { AttendanceService } from '../../../services/attendance.service';
import { 
  AttendanceStatus, 
  AttendanceStatusType, 
  BreakType,
  AttendanceRecord 
} from '../../../models/attendance.models';

describe('AttendanceTrackerComponent', () => {
  let component: AttendanceTrackerComponent;
  let fixture: ComponentFixture<AttendanceTrackerComponent>;
  let mockAttendanceService: jasmine.SpyObj<AttendanceService>;
  let mockRouter: jasmine.SpyObj<Router>;

  const mockAttendanceStatus: AttendanceStatus = {
    employeeId: 1,
    isCheckedIn: true,
    currentStatus: AttendanceStatusType.Present,
    checkInTime: '2025-01-08T09:15:00Z',
    totalWorkingHours: '02:45:00',
    totalBreakTime: '00:15:00',
    location: '40.7128,-74.0060'
  };

  const mockAttendanceRecord: AttendanceRecord = {
    id: 1,
    employeeId: 1,
    date: '2025-01-08',
    checkInTime: '2025-01-08T09:15:00Z',
    status: AttendanceStatusType.Present,
    location: '40.7128,-74.0060'
  };

  beforeEach(async () => {
    const attendanceServiceSpy = jasmine.createSpyObj('AttendanceService', [
      'getCurrentEmployeeStatus',
      'checkIn',
      'checkOut',
      'startBreak',
      'endBreak',
      'getMockAttendanceStatus'
    ], {
      attendanceStatus$: of(mockAttendanceStatus)
    });

    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [AttendanceTrackerComponent],
      providers: [
        { provide: AttendanceService, useValue: attendanceServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AttendanceTrackerComponent);
    component = fixture.componentInstance;
    mockAttendanceService = TestBed.inject(AttendanceService) as jasmine.SpyObj<AttendanceService>;
    mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;

    mockAttendanceService.getMockAttendanceStatus.and.returnValue(mockAttendanceStatus);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load attendance status on init', () => {
    component.ngOnInit();
    expect(component.attendanceStatus).toEqual(mockAttendanceStatus);
  });

  it('should subscribe to attendance status updates', () => {
    component.ngOnInit();
    expect(component.attendanceStatus).toEqual(mockAttendanceStatus);
  });

  describe('Check-in functionality', () => {
    it('should perform check-in successfully', () => {
      mockAttendanceService.checkIn.and.returnValue(of(mockAttendanceRecord));
      
      component.checkIn();
      
      expect(mockAttendanceService.checkIn).toHaveBeenCalled();
      expect(component.isLoading).toBeFalse();
      expect(component.successMessage).toContain('Check-in successful');
      expect(component.errorMessage).toBeNull();
    });

    it('should handle check-in error', () => {
      const error = { error: { message: 'Check-in failed' } };
      mockAttendanceService.checkIn.and.returnValue(throwError(() => error));
      
      component.checkIn();
      
      expect(component.isLoading).toBeFalse();
      expect(component.errorMessage).toBe('Check-in failed');
      expect(component.successMessage).toBeNull();
    });

    it('should set loading state during check-in', () => {
      mockAttendanceService.checkIn.and.returnValue(of(mockAttendanceRecord));
      
      component.checkIn();
      
      expect(component.isLoading).toBeFalse(); // Should be false after completion
    });
  });

  describe('Check-out functionality', () => {
    it('should perform check-out successfully', () => {
      const checkOutRecord = { ...mockAttendanceRecord, checkOutTime: '2025-01-08T17:30:00Z', totalWorkingHours: '08:15:00' };
      mockAttendanceService.checkOut.and.returnValue(of(checkOutRecord));
      
      component.checkOut();
      
      expect(mockAttendanceService.checkOut).toHaveBeenCalled();
      expect(component.successMessage).toContain('Check-out successful');
    });

    it('should handle check-out error', () => {
      const error = { error: { message: 'Check-out failed' } };
      mockAttendanceService.checkOut.and.returnValue(throwError(() => error));
      
      component.checkOut();
      
      expect(component.errorMessage).toBe('Check-out failed');
    });
  });

  describe('Break management', () => {
    it('should start break successfully', () => {
      mockAttendanceService.startBreak.and.returnValue(of(mockAttendanceRecord));
      
      component.startBreak(BreakType.Lunch);
      
      expect(mockAttendanceService.startBreak).toHaveBeenCalledWith(BreakType.Lunch);
      expect(component.successMessage).toContain('Lunch Break started successfully');
    });

    it('should end break successfully', () => {
      mockAttendanceService.endBreak.and.returnValue(of(mockAttendanceRecord));
      
      component.endBreak();
      
      expect(mockAttendanceService.endBreak).toHaveBeenCalled();
      expect(component.successMessage).toContain('Break ended successfully');
    });

    it('should handle break start error', () => {
      const error = { error: { message: 'Failed to start break' } };
      mockAttendanceService.startBreak.and.returnValue(throwError(() => error));
      
      component.startBreak(BreakType.Tea);
      
      expect(component.errorMessage).toBe('Failed to start break');
    });
  });

  describe('Navigation', () => {
    it('should navigate to attendance now page', () => {
      component.navigateToAttendanceNow();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/attendance/now']);
    });
  });

  describe('Utility methods', () => {
    it('should get correct status label', () => {
      expect(component.getStatusLabel(AttendanceStatusType.Present)).toBe('Present');
      expect(component.getStatusLabel(AttendanceStatusType.OnBreak)).toBe('On Break');
      expect(component.getStatusLabel(undefined)).toBe('Not Available');
    });

    it('should get correct status color', () => {
      expect(component.getStatusColor(AttendanceStatusType.Present)).toBe('success');
      expect(component.getStatusColor(AttendanceStatusType.Absent)).toBe('danger');
      expect(component.getStatusColor(undefined)).toBe('secondary');
    });

    it('should get correct status icon', () => {
      expect(component.getStatusIcon(AttendanceStatusType.Present)).toBe('fa-check-circle');
      expect(component.getStatusIcon(AttendanceStatusType.OnBreak)).toBe('fa-coffee');
      expect(component.getStatusIcon(undefined)).toBe('fa-question-circle');
    });

    it('should get correct break label', () => {
      expect(component.getBreakLabel(BreakType.Lunch)).toBe('Lunch Break');
      expect(component.getBreakLabel(BreakType.Tea)).toBe('Tea Break');
      expect(component.getBreakLabel(undefined)).toBe('');
    });

    it('should get correct break icon', () => {
      expect(component.getBreakIcon(BreakType.Lunch)).toBe('fa-utensils');
      expect(component.getBreakIcon(BreakType.Tea)).toBe('fa-coffee');
      expect(component.getBreakIcon(BreakType.Meeting)).toBe('fa-users');
    });

    it('should format time correctly', () => {
      expect(component.formatTime('2025-01-08T09:15:00Z')).toMatch(/\d{1,2}:\d{2}\s?(AM|PM)/);
      expect(component.formatTime('02:45:00')).toBe('02:45');
      expect(component.formatTime(undefined)).toBe('--:--');
      expect(component.formatTime('')).toBe('--:--');
    });
  });

  describe('Message handling', () => {
    it('should clear messages', () => {
      component.successMessage = 'Success';
      component.errorMessage = 'Error';
      
      component['clearMessages']();
      
      expect(component.successMessage).toBeNull();
      expect(component.errorMessage).toBeNull();
    });

    it('should handle error correctly', () => {
      const error = { error: { message: 'Custom error' } };
      
      component['handleError']('Test error', error);
      
      expect(component.errorMessage).toBe('Custom error');
    });

    it('should use default error message when no specific message', () => {
      const error = {};
      
      component['handleError']('Default error', error);
      
      expect(component.errorMessage).toBe('Default error');
    });
  });

  describe('Component lifecycle', () => {
    it('should initialize break types', () => {
      expect(component.breakTypes).toEqual(Object.values(BreakType));
    });

    it('should clean up subscriptions on destroy', () => {
      spyOn(component['destroy$'], 'next');
      spyOn(component['destroy$'], 'complete');
      
      component.ngOnDestroy();
      
      expect(component['destroy$'].next).toHaveBeenCalled();
      expect(component['destroy$'].complete).toHaveBeenCalled();
    });
  });
});