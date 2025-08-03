import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AttendanceService } from './attendance.service';
import { 
  AttendanceRecord,
  AttendanceStatus,
  AttendanceStatusType,
  BreakType,
  CheckInDto,
  CheckOutDto,
  StartBreakDto,
  EndBreakDto,
  AttendanceReportCriteria,
  TodayAttendanceOverview
} from '../models/attendance.models';
import { of } from 'rxjs';

describe('AttendanceService', () => {
  let service: AttendanceService;
  let httpMock: HttpTestingController;
  const API_URL = 'http://localhost:5000/api';

  const mockAttendanceRecord: AttendanceRecord = {
    id: 1,
    employeeId: 1,
    date: '2025-01-08',
    checkInTime: '2025-01-08T09:15:00Z',
    status: AttendanceStatusType.Present,
    location: '40.7128,-74.0060'
  };

  const mockAttendanceStatus: AttendanceStatus = {
    employeeId: 1,
    isCheckedIn: true,
    currentStatus: AttendanceStatusType.Present,
    checkInTime: '2025-01-08T09:15:00Z',
    totalWorkingHours: '02:45:00',
    totalBreakTime: '00:15:00',
    location: '40.7128,-74.0060'
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AttendanceService]
    });
    
    service = TestBed.inject(AttendanceService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    // Flush any pending requests before verification
    try {
      httpMock.match(() => true).forEach(req => req.flush({}));
    } catch (e) {
      // Ignore errors during cleanup
    }
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Check-in functionality', () => {
    it('should perform check-in with location', (done) => {
      const checkInDto: CheckInDto = { location: 'Office' };
      
      // Mock geolocation
      spyOn(service as any, 'getCurrentLocation').and.returnValue(
        Promise.resolve({ latitude: 40.7128, longitude: -74.0060 })
      );

      service.checkIn(checkInDto).subscribe(record => {
        expect(record).toEqual(mockAttendanceRecord);
        done();
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/checkin`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body.location).toBeDefined();
      req.flush(mockAttendanceRecord);
    });

    it('should perform check-in without location when geolocation fails', (done) => {
      const checkInDto: CheckInDto = {};
      
      // Mock geolocation failure
      spyOn(service as any, 'getCurrentLocation').and.returnValue(
        Promise.reject(new Error('Geolocation failed'))
      );

      service.checkIn(checkInDto).subscribe(record => {
        expect(record).toEqual(mockAttendanceRecord);
        done();
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/checkin`);
      expect(req.request.method).toBe('POST');
      req.flush(mockAttendanceRecord);
    });

    it('should handle check-in error', (done) => {
      service.checkIn().subscribe({
        next: () => fail('Should have failed'),
        error: (error) => {
          expect(error).toBeDefined();
          done();
        }
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/checkin`);
      req.flush('Check-in failed', { status: 400, statusText: 'Bad Request' });
    });
  });

  describe('Check-out functionality', () => {
    it('should perform check-out successfully', () => {
      const checkOutDto: CheckOutDto = {};
      const checkOutRecord = { ...mockAttendanceRecord, checkOutTime: '2025-01-08T17:30:00Z' };

      service.checkOut(checkOutDto).subscribe(record => {
        expect(record).toEqual(checkOutRecord);
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/checkout`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body.timestamp).toBeDefined();
      req.flush(checkOutRecord);
    });

    it('should handle check-out error', () => {
      service.checkOut().subscribe({
        next: () => fail('Should have failed'),
        error: (error) => expect(error).toBeDefined()
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/checkout`);
      req.flush('Check-out failed', { status:400, statusText: 'Bad Request' });
    });
  });

  describe('Break management', () => {
    it('should start break successfully', () => {
      const breakType = BreakType.Lunch;
      const startBreakDto: StartBreakDto = {};

      service.startBreak(breakType, startBreakDto).subscribe(record => {
        expect(record).toEqual(mockAttendanceRecord);
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/break/start`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body.type).toBe(breakType);
      expect(req.request.body.timestamp).toBeDefined();
      req.flush(mockAttendanceRecord);
    });

    it('should end break successfully', () => {
      const endBreakDto: EndBreakDto = {};

      service.endBreak(endBreakDto).subscribe(record => {
        expect(record).toEqual(mockAttendanceRecord);
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/break/end`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body.timestamp).toBeDefined();
      req.flush(mockAttendanceRecord);
    });

    it('should handle break start error', () => {
      service.startBreak(BreakType.Tea).subscribe({
        next: () => fail('Should have failed'),
        error: (error) => expect(error).toBeDefined()
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/break/start`);
      req.flush('Break start failed', { status: 400, statusText: 'Bad Request' });
    });
  });

  describe('Status and reports', () => {
    it('should get current employee status', () => {
      service.getCurrentEmployeeStatus().subscribe(status => {
        expect(status).toEqual(mockAttendanceStatus);
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/status`);
      expect(req.request.method).toBe('GET');
      req.flush(mockAttendanceStatus);
    });

    it('should get today attendance overview', () => {
      const mockOverview: TodayAttendanceOverview = {
        branchId: 1,
        date: '2025-01-08',
        summary: {
          totalEmployees: 4,
          presentCount: 2,
          absentCount: 1,
          lateCount: 1,
          onBreakCount: 0,
          onLeaveCount: 0,
          averageWorkingHours: '02:30:00',
          totalOvertimeHours: '00:00:00'
        },
        employeeStatuses: []
      };

      service.getTodayAttendanceOverview(1).subscribe(overview => {
        expect(overview).toEqual(mockOverview);
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/today?branchId=1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockOverview);
    });

    it('should get today attendance overview without branch filter', () => {
      service.getTodayAttendanceOverview().subscribe();

      const req = httpMock.expectOne(`${API_URL}/attendance/today`);
      expect(req.request.method).toBe('GET');
      req.flush({});
    });

    it('should get attendance report', () => {
      const criteria: AttendanceReportCriteria = {
        branchId: 1,
        startDate: '2025-01-01',
        endDate: '2025-01-31'
      };

      service.getAttendanceReport(criteria).subscribe();

      const req = httpMock.expectOne(
        `${API_URL}/attendance/report?branchId=1&startDate=2025-01-01&endDate=2025-01-31`
      );
      expect(req.request.method).toBe('GET');
      req.flush({ criteria, records: [], summary: {} });
    });

    it('should get employee attendance history', () => {
      const employeeId = 1;
      const startDate = '2025-01-01';
      const endDate = '2025-01-31';

      service.getEmployeeAttendanceHistory(employeeId, startDate, endDate).subscribe();

      const req = httpMock.expectOne(
        `${API_URL}/attendance/employee/1?startDate=2025-01-01&endDate=2025-01-31`
      );
      expect(req.request.method).toBe('GET');
      req.flush([mockAttendanceRecord]);
    });
  });

  describe('Utility methods', () => {
    it('should refresh attendance status', () => {
      spyOn(service, 'getCurrentEmployeeStatus').and.returnValue(
        of(mockAttendanceStatus)
      );

      service['refreshAttendanceStatus']();

      expect(service.getCurrentEmployeeStatus).toHaveBeenCalled();
    });

    it('should refresh today overview', () => {
      spyOn(service, 'getTodayAttendanceOverview').and.returnValue(
        of({} as TodayAttendanceOverview)
      );

      service.refreshTodayOverview(1);

      expect(service.getTodayAttendanceOverview).toHaveBeenCalledWith(1);
    });
  });

  describe('Mock data methods', () => {
    it('should return mock attendance status', () => {
      const mockStatus = service.getMockAttendanceStatus();
      
      expect(mockStatus.employeeId).toBe(1);
      expect(mockStatus.isCheckedIn).toBe(true);
      expect(mockStatus.currentStatus).toBe(AttendanceStatusType.Present);
    });

    it('should return mock today overview', () => {
      const mockOverview = service.getMockTodayOverview();
      
      expect(mockOverview.branchId).toBe(1);
      expect(mockOverview.summary.totalEmployees).toBe(4);
      expect(mockOverview.employeeStatuses.length).toBeGreaterThan(0);
    });
  });

  describe('Geolocation', () => {
    it('should get current location successfully', async () => {
      const mockPosition = {
        coords: {
          latitude: 40.7128,
          longitude: -74.0060,
          accuracy: 10
        }
      };

      spyOn(navigator.geolocation, 'getCurrentPosition').and.callFake((success: any) => {
        success(mockPosition);
      });

      const location = await service['getCurrentLocation']();
      
      expect(location.latitude).toBe(40.7128);
      expect(location.longitude).toBe(-74.0060);
      expect(location.accuracy).toBe(10);
    });

    it('should handle geolocation error', async () => {
      spyOn(navigator.geolocation, 'getCurrentPosition').and.callFake((success: any, error: any) => {
        error(new Error('Geolocation failed'));
      });

      try {
        await service['getCurrentLocation']();
        fail('Should have thrown error');
      } catch (error) {
        expect(error).toBeDefined();
      }
    });

    it('should handle unsupported geolocation', async () => {
      // Mock unsupported geolocation
      const originalGeolocation = navigator.geolocation;
      (navigator as any).geolocation = undefined;

      try {
        await service['getCurrentLocation']();
        fail('Should have thrown error');
      } catch (error: any) {
        expect(error.message).toContain('not supported');
      } finally {
        (navigator as any).geolocation = originalGeolocation;
      }
    });
  });
});