import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
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
  TodayAttendanceOverview,
  AttendanceReportRequest,
  AttendanceReportResponse,
  AttendanceCalendarResponse,
  AttendanceAlertResponse,
  AttendanceCorrectionRequest,
  AddMissingAttendanceRequest
} from '../models/attendance.models';
import { of } from 'rxjs';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

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
    location: '40.7128,-74.0060',
    isLate: false,
    isEarlyOut: false
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
    imports: [],
    providers: [AttendanceService, provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
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
    it('should perform check-in with location', fakeAsync(() => {
      const checkInDto: CheckInDto = { location: 'Office' };

      // Mock geolocation
      spyOn(service as any, 'getCurrentLocation').and.returnValue(
        Promise.resolve({ latitude: 40.7128, longitude: -74.0060 })
      );

      let result: AttendanceRecord | undefined;
      service.checkIn(checkInDto).subscribe(record => {
        result = record;
      });

      tick(); // Wait for geolocation promise to resolve

      const req = httpMock.expectOne(`${API_URL}/attendance/checkin`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body.location).toBeDefined();
      req.flush(mockAttendanceRecord);

      expect(result).toEqual(mockAttendanceRecord);
    }));

    it('should perform check-in without location when geolocation fails', fakeAsync(() => {
      const checkInDto: CheckInDto = {};

      // Mock geolocation failure
      spyOn(service as any, 'getCurrentLocation').and.returnValue(
        Promise.reject(new Error('Geolocation failed'))
      );

      let result: AttendanceRecord | undefined;
      service.checkIn(checkInDto).subscribe(record => {
        result = record;
      });

      tick(); // Wait for geolocation promise to reject

      const req = httpMock.expectOne(`${API_URL}/attendance/checkin`);
      expect(req.request.method).toBe('POST');
      req.flush(mockAttendanceRecord);

      expect(result).toEqual(mockAttendanceRecord);
    }));

    it('should handle check-in error', fakeAsync(() => {
      // Mock geolocation to resolve quickly
      spyOn(service as any, 'getCurrentLocation').and.returnValue(
        Promise.resolve({ latitude: 40.7128, longitude: -74.0060 })
      );

      let error: any;
      service.checkIn().subscribe({
        next: () => fail('Should have failed'),
        error: (err) => {
          error = err;
        }
      });

      tick(); // Wait for geolocation promise to resolve

      const req = httpMock.expectOne(`${API_URL}/attendance/checkin`);
      req.flush('Check-in failed', { status: 400, statusText: 'Bad Request' });

      expect(error).toBeDefined();
    }));
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
      req.flush('Check-out failed', { status: 400, statusText: 'Bad Request' });
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
      // Mock unsupported geolocation by spying on the property
      const geolocationSpy = spyOnProperty(navigator, 'geolocation', 'get').and.returnValue(undefined as any);

      try {
        await service['getCurrentLocation']();
        fail('Should have thrown error');
      } catch (error: any) {
        expect(error.message).toContain('not supported');
      } finally {
        geolocationSpy.and.callThrough();
      }
    });
  });

  // New tests for attendance management and reporting
  describe('Attendance Management and Reporting', () => {
    it('should generate attendance report', () => {
      const request: AttendanceReportRequest = {
        startDate: new Date('2025-01-01'),
        endDate: new Date('2025-01-31'),
        reportType: 'summary'
      };

      const mockResponse: AttendanceReportResponse = {
        reportType: 'summary',
        startDate: new Date('2025-01-01'),
        endDate: new Date('2025-01-31'),
        generatedAt: new Date(),
        totalEmployees: 5,
        items: [],
        summary: {
          totalEmployees: 5,
          totalWorkingDays: 22,
          averageAttendancePercentage: 95.5,
          totalPresentDays: 105,
          totalAbsentDays: 5,
          totalLateDays: 3,
          totalEarlyDepartures: 1,
          totalWorkingHours: '880:00:00',
          totalOvertimeHours: '15:30:00',
          averageWorkingHoursPerDay: '08:00:00',
          averageOvertimePerDay: '00:15:00'
        }
      };

      service.generateAttendanceReport(request).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.totalEmployees).toBe(5);
        expect(response.summary.averageAttendancePercentage).toBe(95.5);
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/reports/generate`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(request);
      req.flush(mockResponse);
    });

    it('should export attendance report', () => {
      const request: AttendanceReportRequest = {
        startDate: new Date('2025-01-01'),
        endDate: new Date('2025-01-31'),
        reportType: 'detailed'
      };
      const format = 'excel';
      const mockBlob = new Blob(['mock excel data'], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });

      service.exportAttendanceReport(request, format).subscribe(response => {
        expect(response).toEqual(mockBlob);
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/reports/export?format=${format}`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(request);
      req.flush(mockBlob);
    });

    it('should get attendance calendar', () => {
      const employeeId = 1;
      const year = 2025;
      const month = 1;

      const mockResponse: AttendanceCalendarResponse = {
        year: 2025,
        month: 1,
        days: [
          {
            date: new Date('2025-01-01'),
            status: AttendanceStatusType.Present,
            checkInTime: new Date('2025-01-01T09:00:00Z'),
            checkOutTime: new Date('2025-01-01T17:00:00Z'),
            workingHours: '08:00:00',
            isLate: false,
            isEarlyOut: false,
            isWeekend: false,
            isHoliday: false,
            breaks: []
          }
        ],
        summary: {
          totalWorkingDays: 22,
          presentDays: 20,
          absentDays: 2,
          lateDays: 1,
          earlyDepartures: 0,
          weekends: 8,
          holidays: 1,
          totalWorkingHours: '160:00:00',
          totalOvertimeHours: '05:30:00',
          attendancePercentage: 90.9
        }
      };

      service.getAttendanceCalendar(employeeId, year, month).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.year).toBe(2025);
        expect(response.month).toBe(1);
        expect(response.days.length).toBe(1);
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/calendar/${employeeId}/${year}/${month}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should get my attendance calendar', () => {
      const year = 2025;
      const month = 1;

      const mockResponse: AttendanceCalendarResponse = {
        year: 2025,
        month: 1,
        days: [],
        summary: {
          totalWorkingDays: 22,
          presentDays: 20,
          absentDays: 2,
          lateDays: 1,
          earlyDepartures: 0,
          weekends: 8,
          holidays: 1,
          totalWorkingHours: '160:00:00',
          totalOvertimeHours: '05:30:00',
          attendancePercentage: 90.9
        }
      };

      service.getMyAttendanceCalendar(year, month).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/calendar/${year}/${month}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should get attendance alerts', () => {
      const branchId = 1;
      const unreadOnly = true;

      const mockResponse: AttendanceAlertResponse[] = [
        {
          id: 1,
          alertType: 'LateArrival' as any,
          alertMessage: 'Employee 1 arrived late',
          employeeId: 1,
          employeeName: 'John Doe',
          branchId: 1,
          branchName: 'Main Branch',
          createdAt: new Date(),
          isRead: false,
          severity: 'Medium',
          metadata: {}
        }
      ];

      service.getAttendanceAlerts(branchId, unreadOnly).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.length).toBe(1);
        expect(response[0].isRead).toBe(false);
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/alerts?unreadOnly=true&branchId=1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should get attendance alerts without filters', () => {
      service.getAttendanceAlerts().subscribe();

      const req = httpMock.expectOne(`${API_URL}/attendance/alerts?unreadOnly=false`);
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });

    it('should mark alert as read', () => {
      const alertId = 1;

      service.markAlertAsRead(alertId).subscribe(response => {
        expect(response).toBe(true);
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/alerts/${alertId}/read`);
      expect(req.request.method).toBe('PUT');
      req.flush(true);
    });

    it('should get pending corrections', () => {
      const branchId = 1;
      const startDate = new Date('2025-01-01');
      const endDate = new Date('2025-01-31');

      const mockResponse: AttendanceRecord[] = [
        {
          id: 1,
          employeeId: 1,
          date: '2025-01-08',
          checkInTime: undefined,
          checkOutTime: undefined,
          status: AttendanceStatusType.Absent,
          isLate: false,
          isEarlyOut: false,
          employee: {
            id: 1,
            employeeId: 'EMP001',
            firstName: 'John',
            lastName: 'Doe',
            designation: 'Developer',
            department: 'IT'
          }
        }
      ];

      service.getPendingCorrections(branchId, startDate, endDate).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.length).toBe(1);
        expect(response[0].checkInTime).toBeUndefined();
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/corrections/pending?branchId=1&startDate=2025-01-01&endDate=2025-01-31`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should get pending corrections without date filters', () => {
      const branchId = 1;

      service.getPendingCorrections(branchId).subscribe();

      const req = httpMock.expectOne(`${API_URL}/attendance/corrections/pending?branchId=1`);
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });

    it('should correct attendance', () => {
      const attendanceRecordId = 1;
      const request: AttendanceCorrectionRequest = {
        checkInTime: new Date('2025-01-08T09:00:00Z'),
        checkOutTime: new Date('2025-01-08T17:00:00Z'),
        reason: 'System error correction'
      };

      const mockResponse: AttendanceRecord = {
        id: 1,
        employeeId: 1,
        date: '2025-01-08',
        checkInTime: '2025-01-08T09:00:00Z',
        checkOutTime: '2025-01-08T17:00:00Z',
        correctionReason: 'System error correction',
        correctedBy: 2,
        correctedAt: new Date(),
        status: AttendanceStatusType.Present,
        isLate: false,
        isEarlyOut: false
      };

      service.correctAttendance(attendanceRecordId, request).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.correctionReason).toBe('System error correction');
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/${attendanceRecordId}/correct`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(request);
      req.flush(mockResponse);
    });

    it('should add missing attendance', () => {
      const request: AddMissingAttendanceRequest = {
        employeeId: 1,
        date: new Date('2025-01-08'),
        checkInTime: new Date('2025-01-08T09:00:00Z'),
        checkOutTime: new Date('2025-01-08T17:00:00Z'),
        reason: 'Employee forgot to check in'
      };

      const mockResponse: AttendanceRecord = {
        id: 1,
        employeeId: 1,
        date: '2025-01-08',
        checkInTime: '2025-01-08T09:00:00Z',
        checkOutTime: '2025-01-08T17:00:00Z',
        correctionReason: 'Employee forgot to check in',
        correctedBy: 2,
        correctedAt: new Date(),
        status: AttendanceStatusType.Present,
        isLate: false,
        isEarlyOut: false
      };

      service.addMissingAttendance(request).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.correctionReason).toBe('Employee forgot to check in');
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/add-missing`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(request);
      req.flush(mockResponse);
    });

    it('should delete attendance record', () => {
      const attendanceRecordId = 1;
      const reason = 'Duplicate entry';

      service.deleteAttendanceRecord(attendanceRecordId, reason).subscribe(response => {
        expect(response).toBe(true);
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/${attendanceRecordId}?reason=${encodeURIComponent(reason)}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(true);
    });

    it('should handle attendance management errors', () => {
      const request: AttendanceReportRequest = {
        startDate: new Date('2025-01-01'),
        endDate: new Date('2025-01-31')
      };

      service.generateAttendanceReport(request).subscribe({
        next: () => fail('Should have failed'),
        error: (error) => expect(error).toBeDefined()
      });

      const req = httpMock.expectOne(`${API_URL}/attendance/reports/generate`);
      req.flush('Report generation failed', { status: 500, statusText: 'Internal Server Error' });
    });
  });
});