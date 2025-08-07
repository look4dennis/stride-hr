import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, interval, of } from 'rxjs';
import { map, tap, switchMap, startWith, catchError, filter } from 'rxjs/operators';
import { BaseApiService, ApiResponse } from '../core/services/base-api.service';
import { RealTimeAttendanceService } from './real-time-attendance.service';
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
export class EnhancedAttendanceService extends BaseApiService<AttendanceRecord> {
    protected readonly endpoint = 'attendance';

    // Real-time state management
    private attendanceStatusSubject = new BehaviorSubject<AttendanceStatus | null>(null);
    private todayOverviewSubject = new BehaviorSubject<TodayAttendanceOverview | null>(null);

    public attendanceStatus$ = this.attendanceStatusSubject.asObservable();
    public todayOverview$ = this.todayOverviewSubject.asObservable();

    constructor(private realTimeService: RealTimeAttendanceService) {
        super();
        // Initialize real-time updates only in browser environment
        if (typeof window !== 'undefined' && !window.location.href.includes('localhost:9876')) {
            this.initializeRealTimeUpdates();
            this.setupSignalRIntegration();
        }
    }

    // Real-time updates every 30 seconds (fallback when SignalR is not available)
    private initializeRealTimeUpdates(): void {
        interval(30000).pipe(
            startWith(0),
            switchMap(() => this.getCurrentEmployeeStatus().pipe(
                catchError(error => {
                    console.log('Real-time attendance update failed (expected during development):', error);
                    return of(null);
                })
            ))
        ).subscribe(status => {
            if (status) {
                this.attendanceStatusSubject.next(status);
            }
        });
    }

    // Setup SignalR integration for real-time updates
    private setupSignalRIntegration(): void {
        // Subscribe to personal status updates from SignalR
        this.realTimeService.personalStatusUpdates$
            .pipe(filter(status => status !== null))
            .subscribe(status => {
                if (status) {
                    this.attendanceStatusSubject.next(status);
                }
            });

        // Subscribe to team overview updates from SignalR
        this.realTimeService.teamOverviewUpdates$
            .pipe(filter(overview => overview !== null))
            .subscribe(overview => {
                if (overview) {
                    this.todayOverviewSubject.next(overview);
                }
            });

        // Connect to SignalR hub
        this.realTimeService.connect().catch(error => {
            console.log('SignalR connection failed, using polling fallback:', error);
        });
    }

    // Check-in/Check-out Operations
    checkIn(dto: CheckInDto = {}): Observable<AttendanceRecord> {
        const operationKey = `${this.endpoint}-checkin`;

        return new Observable(observer => {
            this.getCurrentLocation().then(location => {
                const checkInData = {
                    ...dto,
                    location: location ? `${location.latitude},${location.longitude}` : undefined,
                    timestamp: dto.timestamp || new Date().toISOString()
                };

                this.executeWithRetry(
                    () => this.http.post<ApiResponse<AttendanceRecord>>(`${this.baseUrl}/${this.endpoint}/checkin`, checkInData),
                    operationKey
                ).pipe(
                    map(response => response.data!),
                    tap((record) => {
                        this.showSuccess('Checked in successfully');
                        this.refreshAttendanceStatus();
                        // Notify SignalR about the action
                        this.realTimeService.notifyAttendanceAction('checkin', record).catch(console.error);
                    })
                ).subscribe({
                    next: (record) => {
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

                this.executeWithRetry(
                    () => this.http.post<ApiResponse<AttendanceRecord>>(`${this.baseUrl}/${this.endpoint}/checkin`, checkInData),
                    operationKey
                ).pipe(
                    map(response => response.data!),
                    tap((record) => {
                        this.showSuccess('Checked in successfully');
                        this.refreshAttendanceStatus();
                        // Notify SignalR about the action
                        this.realTimeService.notifyAttendanceAction('checkin', record).catch(console.error);
                    })
                ).subscribe({
                    next: (record) => {
                        observer.next(record);
                        observer.complete();
                    },
                    error: (error) => observer.error(error)
                });
            });
        });
    }

    checkOut(dto: CheckOutDto = {}): Observable<AttendanceRecord> {
        const operationKey = `${this.endpoint}-checkout`;
        const checkOutData = {
            ...dto,
            timestamp: dto.timestamp || new Date().toISOString()
        };

        return this.executeWithRetry(
            () => this.http.post<ApiResponse<AttendanceRecord>>(`${this.baseUrl}/${this.endpoint}/checkout`, checkOutData),
            operationKey
        ).pipe(
            map(response => response.data!),
            tap((record) => {
                this.showSuccess('Checked out successfully');
                this.refreshAttendanceStatus();
                // Notify SignalR about the action
                this.realTimeService.notifyAttendanceAction('checkout', record).catch(console.error);
            })
        );
    }

    // Break Management
    startBreak(type: BreakType, dto: StartBreakDto = {}): Observable<AttendanceRecord> {
        const operationKey = `${this.endpoint}-startBreak`;
        const breakData = {
            type,
            timestamp: dto.timestamp || new Date().toISOString()
        };

        return this.executeWithRetry(
            () => this.http.post<ApiResponse<AttendanceRecord>>(`${this.baseUrl}/${this.endpoint}/break/start`, breakData),
            operationKey
        ).pipe(
            map(response => response.data!),
            tap((record) => {
                this.showSuccess(`${type} break started`);
                this.refreshAttendanceStatus();
                // Notify SignalR about the action
                this.realTimeService.notifyAttendanceAction('break_start', record).catch(console.error);
            })
        );
    }

    endBreak(dto: EndBreakDto = {}): Observable<AttendanceRecord> {
        const operationKey = `${this.endpoint}-endBreak`;
        const endBreakData = {
            ...dto,
            timestamp: dto.timestamp || new Date().toISOString()
        };

        return this.executeWithRetry(
            () => this.http.post<ApiResponse<AttendanceRecord>>(`${this.baseUrl}/${this.endpoint}/break/end`, endBreakData),
            operationKey
        ).pipe(
            map(response => response.data!),
            tap((record) => {
                this.showSuccess('Break ended');
                this.refreshAttendanceStatus();
                // Notify SignalR about the action
                this.realTimeService.notifyAttendanceAction('break_end', record).catch(console.error);
            })
        );
    }

    // Status and Reports
    getCurrentEmployeeStatus(): Observable<AttendanceStatus> {
        const operationKey = `${this.endpoint}-status`;
        return this.executeWithRetry(
            () => this.http.get<ApiResponse<AttendanceStatus>>(`${this.baseUrl}/${this.endpoint}/status`),
            operationKey
        ).pipe(
            map(response => response.data!),
            catchError(error => {
                console.log('API call failed, using mock data for development:', error);
                return of(this.getMockAttendanceStatus());
            })
        );
    }

    getTodayAttendanceOverview(branchId?: number): Observable<TodayAttendanceOverview> {
        const operationKey = `${this.endpoint}-todayOverview`;
        
        // Use the correct API endpoint structure based on backend controller
        const endpoint = branchId 
            ? `${this.baseUrl}/${this.endpoint}/branch/${branchId}/today`
            : `${this.baseUrl}/${this.endpoint}/today`;

        return this.executeWithRetry(
            () => this.http.get<ApiResponse<TodayAttendanceOverview>>(endpoint),
            operationKey
        ).pipe(
            map(response => response.data!),
            catchError(error => {
                console.log('API call failed, using mock data for development:', error);
                return of(this.getMockTodayOverview());
            })
        );
    }

    getAttendanceReport(criteria: AttendanceReportCriteria): Observable<AttendanceReport> {
        const operationKey = `${this.endpoint}-report`;
        const params = this.buildReportParams(criteria);

        return this.executeWithRetry(
            () => {
                const httpParams = this.buildHttpParams(params);
                return this.http.get<ApiResponse<AttendanceReport>>(`${this.baseUrl}/${this.endpoint}/report`, { params: httpParams });
            },
            operationKey
        ).pipe(
            map(response => response.data!)
        );
    }

    getEmployeeAttendanceHistory(employeeId: number, startDate: string, endDate: string): Observable<AttendanceRecord[]> {
        const operationKey = `${this.endpoint}-history-${employeeId}`;
        const params = { startDate, endDate };

        return this.executeWithRetry(
            () => {
                const httpParams = this.buildHttpParams(params);
                return this.http.get<ApiResponse<AttendanceRecord[]>>(`${this.baseUrl}/${this.endpoint}/employee/${employeeId}`, { params: httpParams });
            },
            operationKey
        ).pipe(
            map(response => response.data || [])
        );
    }

    // Advanced Attendance Management
    generateAttendanceReport(request: AttendanceReportRequest): Observable<AttendanceReportResponse> {
        const operationKey = `${this.endpoint}-generateReport`;
        return this.executeWithRetry(
            () => this.http.post<ApiResponse<AttendanceReportResponse>>(`${this.baseUrl}/${this.endpoint}/reports/generate`, request),
            operationKey
        ).pipe(
            map(response => response.data!),
            tap(() => this.showSuccess('Attendance report generated successfully'))
        );
    }

    exportAttendanceReport(request: AttendanceReportRequest, format: string = 'excel'): Observable<Blob> {
        const operationKey = `${this.endpoint}-exportReport`;
        return this.executeWithRetry(
            () => this.http.post(`${this.baseUrl}/${this.endpoint}/reports/export?format=${format}`, request, {
                responseType: 'blob'
            }),
            operationKey
        ).pipe(
            tap(() => this.showSuccess(`Attendance report exported as ${format.toUpperCase()}`))
        );
    }

    getAttendanceCalendar(employeeId: number, year: number, month: number): Observable<AttendanceCalendarResponse> {
        const operationKey = `${this.endpoint}-calendar-${employeeId}`;
        return this.executeWithRetry(
            () => this.http.get<ApiResponse<AttendanceCalendarResponse>>(`${this.baseUrl}/${this.endpoint}/calendar/${employeeId}/${year}/${month}`),
            operationKey
        ).pipe(
            map(response => response.data!)
        );
    }

    getMyAttendanceCalendar(year: number, month: number): Observable<AttendanceCalendarResponse> {
        const operationKey = `${this.endpoint}-myCalendar`;
        return this.executeWithRetry(
            () => this.http.get<ApiResponse<AttendanceCalendarResponse>>(`${this.baseUrl}/${this.endpoint}/calendar/${year}/${month}`),
            operationKey
        ).pipe(
            map(response => response.data!)
        );
    }

    // Alerts and Corrections
    getAttendanceAlerts(branchId?: number, unreadOnly: boolean = false): Observable<AttendanceAlertResponse[]> {
        const operationKey = `${this.endpoint}-alerts`;
        const params: any = { unreadOnly };
        if (branchId) params.branchId = branchId;

        return this.executeWithRetry(
            () => {
                const httpParams = this.buildHttpParams(params);
                return this.http.get<ApiResponse<AttendanceAlertResponse[]>>(`${this.baseUrl}/${this.endpoint}/alerts`, { params: httpParams });
            },
            operationKey
        ).pipe(
            map(response => response.data || [])
        );
    }

    markAlertAsRead(alertId: number): Observable<boolean> {
        const operationKey = `${this.endpoint}-markAlert-${alertId}`;
        return this.executeWithRetry(
            () => this.http.put<ApiResponse<boolean>>(`${this.baseUrl}/${this.endpoint}/alerts/${alertId}/read`, {}),
            operationKey
        ).pipe(
            map(response => response.data || false),
            tap(() => this.showSuccess('Alert marked as read'))
        );
    }

    getPendingCorrections(branchId: number, startDate?: Date, endDate?: Date): Observable<AttendanceRecord[]> {
        const operationKey = `${this.endpoint}-pendingCorrections`;
        const params: any = { branchId };
        if (startDate) params.startDate = startDate.toISOString().split('T')[0];
        if (endDate) params.endDate = endDate.toISOString().split('T')[0];

        return this.executeWithRetry(
            () => {
                const httpParams = this.buildHttpParams(params);
                return this.http.get<ApiResponse<AttendanceRecord[]>>(`${this.baseUrl}/${this.endpoint}/corrections/pending`, { params: httpParams });
            },
            operationKey
        ).pipe(
            map(response => response.data || [])
        );
    }

    correctAttendance(attendanceRecordId: number, request: AttendanceCorrectionRequest): Observable<AttendanceRecord> {
        const operationKey = `${this.endpoint}-correct-${attendanceRecordId}`;
        return this.executeWithRetry(
            () => this.http.put<ApiResponse<AttendanceRecord>>(`${this.baseUrl}/${this.endpoint}/${attendanceRecordId}/correct`, request),
            operationKey
        ).pipe(
            map(response => response.data!),
            tap(() => this.showSuccess('Attendance corrected successfully'))
        );
    }

    addMissingAttendance(request: AddMissingAttendanceRequest): Observable<AttendanceRecord> {
        const operationKey = `${this.endpoint}-addMissing`;
        return this.executeWithRetry(
            () => this.http.post<ApiResponse<AttendanceRecord>>(`${this.baseUrl}/${this.endpoint}/add-missing`, request),
            operationKey
        ).pipe(
            map(response => response.data!),
            tap(() => this.showSuccess('Missing attendance added successfully'))
        );
    }

    deleteAttendanceRecord(attendanceRecordId: number, reason: string): Observable<boolean> {
        const operationKey = `${this.endpoint}-delete-${attendanceRecordId}`;
        return this.executeWithRetry(
            () => this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${this.endpoint}/${attendanceRecordId}?reason=${encodeURIComponent(reason)}`),
            operationKey
        ).pipe(
            map(response => response.data || false),
            tap(() => this.showSuccess('Attendance record deleted successfully'))
        );
    }

    // Utility Methods
    refreshAttendanceStatus(): void {
        this.getCurrentEmployeeStatus().pipe(
            catchError(error => {
                console.log('Failed to refresh attendance status (expected during development):', error);
                return of(null);
            })
        ).subscribe(status => {
            if (status) {
                this.attendanceStatusSubject.next(status);
            }
        });
    }

    refreshTodayOverview(branchId?: number): void {
        this.getTodayAttendanceOverview(branchId).pipe(
            catchError(error => {
                console.log('Failed to refresh today overview (expected during development):', error);
                return of(null);
            })
        ).subscribe(overview => {
            if (overview) {
                this.todayOverviewSubject.next(overview);
            }
        });
    }

    private getCurrentLocation(): Promise<LocationInfo> {
        return new Promise((resolve, reject) => {
            if (!navigator.geolocation) {
                reject(new Error('Geolocation is not supported by this browser.'));
                return;
            }

            // Show loading state for location
            this.showInfo('Getting your location...');

            navigator.geolocation.getCurrentPosition(
                (position) => {
                    const locationInfo: LocationInfo = {
                        latitude: position.coords.latitude,
                        longitude: position.coords.longitude,
                        accuracy: position.coords.accuracy
                    };

                    // Optionally get address from coordinates
                    this.getAddressFromCoordinates(locationInfo.latitude, locationInfo.longitude)
                        .then(address => {
                            locationInfo.address = address;
                            resolve(locationInfo);
                        })
                        .catch(() => {
                            // Resolve without address if geocoding fails
                            resolve(locationInfo);
                        });
                },
                (error) => {
                    let errorMessage = 'Unable to get your location. ';
                    switch (error.code) {
                        case error.PERMISSION_DENIED:
                            errorMessage += 'Location access denied by user.';
                            break;
                        case error.POSITION_UNAVAILABLE:
                            errorMessage += 'Location information unavailable.';
                            break;
                        case error.TIMEOUT:
                            errorMessage += 'Location request timed out.';
                            break;
                        default:
                            errorMessage += 'Unknown location error.';
                            break;
                    }
                    this.showWarning(errorMessage);
                    reject(new Error(errorMessage));
                },
                {
                    enableHighAccuracy: true,
                    timeout: 15000, // Increased timeout
                    maximumAge: 300000 // 5 minutes
                }
            );
        });
    }

    private getAddressFromCoordinates(latitude: number, longitude: number): Promise<string> {
        return new Promise((resolve, reject) => {
            // Use a reverse geocoding service (you can replace with your preferred service)
            const geocodingUrl = `https://api.opencagedata.com/geocode/v1/json?q=${latitude}+${longitude}&key=YOUR_API_KEY`;
            
            // For now, return a simple formatted location
            resolve(`${latitude.toFixed(6)}, ${longitude.toFixed(6)}`);
        });
    }

    // Helper methods
    private buildReportParams(criteria: AttendanceReportCriteria): any {
        const params: any = {
            startDate: criteria.startDate,
            endDate: criteria.endDate
        };

        if (criteria.branchId) params.branchId = criteria.branchId;
        if (criteria.departmentId) params.departmentId = criteria.departmentId;
        if (criteria.employeeId) params.employeeId = criteria.employeeId;
        if (criteria.status) params.status = criteria.status;

        return params;
    }

    // Mock data fallback for development
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
}