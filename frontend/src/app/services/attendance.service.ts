import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, interval, switchMap, startWith } from 'rxjs';
import { 
  AttendanceRecord,
  CheckInDto,
  CheckOutDto,
  StartBreakDto,
  EndBreakDto,
  AttendanceStatus,
  AttendanceReport,
  AttendanceReportCriteria,
  TodayAttendanceOverview,
  LocationInfo,
  BreakType,
  AttendanceStatusType,
  EmployeeAttendanceStatus,
  AttendanceReportRequest,
  AttendanceReportResponse,
  AttendanceCalendarResponse,
  AttendanceAlertResponse,
  AttendanceCorrectionRequest,
  AddMissingAttendanceRequest
} from '../models/attendance.models';

@Injectable({
  providedIn: 'root'
})
export class AttendanceService {
  private readonly API_URL = 'http://localhost:5000/api';
  
  private attendanceStatusSubject = new BehaviorSubject<AttendanceStatus | null>(null);
  public attendanceStatus$ = this.attendanceStatusSubject.asObservable();

  private todayOverviewSubject = new BehaviorSubject<TodayAttendanceOverview | null>(null);
  public todayOverview$ = this.todayOverviewSubject.asObservable();

  constructor(private http: HttpClient) {
    // Only initialize real-time updates in production
    if (typeof window !== 'undefined' && !window.location.href.includes('localhost:9876')) {
      this.initializeRealTimeUpdates();
    }
  }

  // Real-time updates every 30 seconds
  private initializeRealTimeUpdates(): void {
    interval(30000).pipe(
      startWith(0),
      switchMap(() => this.getCurrentEmployeeStatus())
    ).subscribe(status => {
      this.attendanceStatusSubject.next(status);
    });
  }

  // Check-in/Check-out Operations
  checkIn(dto: CheckInDto = {}): Observable<AttendanceRecord> {
    return new Observable(observer => {
      this.getCurrentLocation().then(location => {
        const checkInData = {
          ...dto,
          location: location ? `${location.latitude},${location.longitude}` : undefined,
          timestamp: dto.timestamp || new Date().toISOString()
        };

        this.http.post<AttendanceRecord>(`${this.API_URL}/attendance/checkin`, checkInData)
          .subscribe({
            next: (record) => {
              this.refreshAttendanceStatus();
              observer.next(record);
              observer.complete();
            },
            error: (error) => observer.error(error)
          });
      }).catch(error => {
        // Proceed without location if geolocation fails
        const checkInData = {
          ...dto,
          timestamp: dto.timestamp || new Date().toISOString()
        };

        this.http.post<AttendanceRecord>(`${this.API_URL}/attendance/checkin`, checkInData)
          .subscribe({
            next: (record) => {
              this.refreshAttendanceStatus();
              observer.next(record);
              observer.complete();
            },
            error: (error) => observer.error(error)
          });
      });
    });
  }

  checkOut(dto: CheckOutDto = {}): Observable<AttendanceRecord> {
    const checkOutData = {
      ...dto,
      timestamp: dto.timestamp || new Date().toISOString()
    };

    return new Observable(observer => {
      this.http.post<AttendanceRecord>(`${this.API_URL}/attendance/checkout`, checkOutData)
        .subscribe({
          next: (record) => {
            this.refreshAttendanceStatus();
            observer.next(record);
            observer.complete();
          },
          error: (error) => observer.error(error)
        });
    });
  }

  // Break Management
  startBreak(type: BreakType, dto: StartBreakDto = {}): Observable<AttendanceRecord> {
    const breakData = {
      type,
      timestamp: dto.timestamp || new Date().toISOString()
    };

    return new Observable(observer => {
      this.http.post<AttendanceRecord>(`${this.API_URL}/attendance/break/start`, breakData)
        .subscribe({
          next: (record) => {
            this.refreshAttendanceStatus();
            observer.next(record);
            observer.complete();
          },
          error: (error) => observer.error(error)
        });
    });
  }

  endBreak(dto: EndBreakDto = {}): Observable<AttendanceRecord> {
    const endBreakData = {
      ...dto,
      timestamp: dto.timestamp || new Date().toISOString()
    };

    return new Observable(observer => {
      this.http.post<AttendanceRecord>(`${this.API_URL}/attendance/break/end`, endBreakData)
        .subscribe({
          next: (record) => {
            this.refreshAttendanceStatus();
            observer.next(record);
            observer.complete();
          },
          error: (error) => observer.error(error)
        });
    });
  }

  // Status and Reports
  getCurrentEmployeeStatus(): Observable<AttendanceStatus> {
    return this.http.get<AttendanceStatus>(`${this.API_URL}/attendance/status`);
  }

  getTodayAttendanceOverview(branchId?: number): Observable<TodayAttendanceOverview> {
    let params = new HttpParams();
    if (branchId) params = params.set('branchId', branchId.toString());
    
    return this.http.get<TodayAttendanceOverview>(`${this.API_URL}/attendance/today`, { params });
  }

  getAttendanceReport(criteria: AttendanceReportCriteria): Observable<AttendanceReport> {
    let params = new HttpParams();
    
    if (criteria.branchId) params = params.set('branchId', criteria.branchId.toString());
    if (criteria.departmentId) params = params.set('departmentId', criteria.departmentId);
    if (criteria.employeeId) params = params.set('employeeId', criteria.employeeId.toString());
    if (criteria.status) params = params.set('status', criteria.status);
    params = params.set('startDate', criteria.startDate);
    params = params.set('endDate', criteria.endDate);

    return this.http.get<AttendanceReport>(`${this.API_URL}/attendance/report`, { params });
  }

  getEmployeeAttendanceHistory(employeeId: number, startDate: string, endDate: string): Observable<AttendanceRecord[]> {
    let params = new HttpParams()
      .set('startDate', startDate)
      .set('endDate', endDate);

    return this.http.get<AttendanceRecord[]>(`${this.API_URL}/attendance/employee/${employeeId}`, { params });
  }

  // Utility Methods
  private refreshAttendanceStatus(): void {
    this.getCurrentEmployeeStatus().subscribe(status => {
      this.attendanceStatusSubject.next(status);
    });
  }

  refreshTodayOverview(branchId?: number): void {
    this.getTodayAttendanceOverview(branchId).subscribe(overview => {
      this.todayOverviewSubject.next(overview);
    });
  }

  private getCurrentLocation(): Promise<LocationInfo> {
    return new Promise((resolve, reject) => {
      if (!navigator.geolocation) {
        reject(new Error('Geolocation is not supported by this browser.'));
        return;
      }

      navigator.geolocation.getCurrentPosition(
        (position) => {
          resolve({
            latitude: position.coords.latitude,
            longitude: position.coords.longitude,
            accuracy: position.coords.accuracy
          });
        },
        (error) => {
          reject(error);
        },
        {
          enableHighAccuracy: true,
          timeout: 10000,
          maximumAge: 300000 // 5 minutes
        }
      );
    });
  }

  // Mock data for development
  getMockAttendanceStatus(): AttendanceStatus {
    return {
      employeeId: 1,
      isCheckedIn: true,
      currentStatus: AttendanceStatusType.Present,
      checkInTime: '2025-01-08T09:15:00Z',
      totalWorkingHours: '02:45:00',
      totalBreakTime: '00:15:00',
      location: '40.7128,-74.0060'
    };
  }

  getMockTodayOverview(): TodayAttendanceOverview {
    const mockEmployeeStatuses: EmployeeAttendanceStatus[] = [
      {
        employee: {
          id: 1,
          employeeId: 'EMP001',
          firstName: 'John',
          lastName: 'Doe',
          profilePhoto: '/assets/images/avatars/john-doe.jpg',
          designation: 'Senior Developer',
          department: 'Development'
        },
        status: AttendanceStatusType.Present,
        checkInTime: '2025-01-08T09:15:00Z',
        totalWorkingHours: '02:45:00',
        totalBreakTime: '00:15:00',
        location: '40.7128,-74.0060',
        isLate: false
      },
      {
        employee: {
          id: 2,
          employeeId: 'EMP002',
          firstName: 'Jane',
          lastName: 'Smith',
          profilePhoto: '/assets/images/avatars/jane-smith.jpg',
          designation: 'Development Manager',
          department: 'Development'
        },
        status: AttendanceStatusType.OnBreak,
        checkInTime: '2025-01-08T09:00:00Z',
        totalWorkingHours: '03:00:00',
        totalBreakTime: '00:30:00',
        location: '40.7128,-74.0060',
        isLate: false,
        currentBreak: {
          id: 1,
          attendanceRecordId: 2,
          type: BreakType.Lunch,
          startTime: '2025-01-08T12:00:00Z'
        }
      },
      {
        employee: {
          id: 3,
          employeeId: 'EMP003',
          firstName: 'Mike',
          lastName: 'Johnson',
          profilePhoto: '/assets/images/avatars/mike-johnson.jpg',
          designation: 'Junior Developer',
          department: 'Development'
        },
        status: AttendanceStatusType.Late,
        checkInTime: '2025-01-08T09:45:00Z',
        totalWorkingHours: '02:15:00',
        totalBreakTime: '00:00:00',
        location: '40.7128,-74.0060',
        isLate: true
      },
      {
        employee: {
          id: 4,
          employeeId: 'EMP004',
          firstName: 'Sarah',
          lastName: 'Wilson',
          profilePhoto: '/assets/images/avatars/sarah-wilson.jpg',
          designation: 'HR Manager',
          department: 'Human Resources'
        },
        status: AttendanceStatusType.Absent,
        totalWorkingHours: '00:00:00',
        totalBreakTime: '00:00:00',
        isLate: false
      }
    ];

    return {
      branchId: 1,
      date: '2025-01-08',
      summary: {
        totalEmployees: 4,
        presentCount: 1,
        absentCount: 1,
        lateCount: 1,
        onBreakCount: 1,
        onLeaveCount: 0,
        averageWorkingHours: '02:30:00',
        totalOvertimeHours: '00:00:00'
      },
      employeeStatuses: mockEmployeeStatuses
    };
  }

  // New methods for attendance management and reporting
  generateAttendanceReport(request: AttendanceReportRequest): Observable<AttendanceReportResponse> {
    return this.http.post<AttendanceReportResponse>(`${this.API_URL}/attendance/reports/generate`, request);
  }

  exportAttendanceReport(request: AttendanceReportRequest, format: string = 'excel'): Observable<Blob> {
    return this.http.post(`${this.API_URL}/attendance/reports/export?format=${format}`, request, {
      responseType: 'blob'
    });
  }

  getAttendanceCalendar(employeeId: number, year: number, month: number): Observable<AttendanceCalendarResponse> {
    return this.http.get<AttendanceCalendarResponse>(`${this.API_URL}/attendance/calendar/${employeeId}/${year}/${month}`);
  }

  getMyAttendanceCalendar(year: number, month: number): Observable<AttendanceCalendarResponse> {
    return this.http.get<AttendanceCalendarResponse>(`${this.API_URL}/attendance/calendar/${year}/${month}`);
  }

  getAttendanceAlerts(branchId?: number, unreadOnly: boolean = false): Observable<AttendanceAlertResponse[]> {
    let params = new HttpParams().set('unreadOnly', unreadOnly.toString());
    if (branchId) {
      params = params.set('branchId', branchId.toString());
    }

    return this.http.get<AttendanceAlertResponse[]>(`${this.API_URL}/attendance/alerts`, { params });
  }

  markAlertAsRead(alertId: number): Observable<boolean> {
    return this.http.put<boolean>(`${this.API_URL}/attendance/alerts/${alertId}/read`, {});
  }

  getPendingCorrections(branchId: number, startDate?: Date, endDate?: Date): Observable<AttendanceRecord[]> {
    let params = new HttpParams().set('branchId', branchId.toString());
    if (startDate) {
      params = params.set('startDate', startDate.toISOString().split('T')[0]);
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString().split('T')[0]);
    }

    return this.http.get<AttendanceRecord[]>(`${this.API_URL}/attendance/corrections/pending`, { params });
  }

  correctAttendance(attendanceRecordId: number, request: AttendanceCorrectionRequest): Observable<AttendanceRecord> {
    return this.http.put<AttendanceRecord>(`${this.API_URL}/attendance/${attendanceRecordId}/correct`, request);
  }

  addMissingAttendance(request: AddMissingAttendanceRequest): Observable<AttendanceRecord> {
    return this.http.post<AttendanceRecord>(`${this.API_URL}/attendance/add-missing`, request);
  }

  deleteAttendanceRecord(attendanceRecordId: number, reason: string): Observable<boolean> {
    return this.http.delete<boolean>(`${this.API_URL}/attendance/${attendanceRecordId}?reason=${encodeURIComponent(reason)}`);
  }
}