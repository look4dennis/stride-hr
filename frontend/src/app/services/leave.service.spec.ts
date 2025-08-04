import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { LeaveService } from './leave.service';
import { 
  LeaveRequest, 
  CreateLeaveRequest, 
  LeaveBalance, 
  LeavePolicy,
  LeaveCalendarEntry,
  LeaveConflict,
  LeaveStatus,
  LeaveType 
} from '../models/leave.models';
import { environment } from '../../environments/environment';

describe('LeaveService', () => {
  let service: LeaveService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/api/leave`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [LeaveService]
    });
    service = TestBed.inject(LeaveService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('Leave Request Management', () => {
    it('should create a leave request', () => {
      const mockRequest: CreateLeaveRequest = {
        leavePolicyId: 1,
        startDate: new Date('2024-01-15'),
        endDate: new Date('2024-01-17'),
        reason: 'Personal vacation',
        comments: 'Family trip',
        isEmergency: false
      };

      const mockResponse = {
        success: true,
        data: {
          id: 1,
          employeeId: 1,
          employeeName: 'John Doe',
          leavePolicyId: 1,
          leaveType: LeaveType.Annual,
          leaveTypeName: 'Annual Leave',
          startDate: '2024-01-15T00:00:00Z',
          endDate: '2024-01-17T00:00:00Z',
          requestedDays: 3,
          approvedDays: 0,
          reason: 'Personal vacation',
          comments: 'Family trip',
          status: LeaveStatus.Pending,
          isEmergency: false,
          createdAt: '2024-01-10T00:00:00Z',
          approvalHistory: []
        }
      };

      service.createLeaveRequest(mockRequest).subscribe(response => {
        expect(response).toBeTruthy();
        expect(response.id).toBe(1);
        expect(response.reason).toBe('Personal vacation');
        expect(response.status).toBe(LeaveStatus.Pending);
        expect(response.startDate).toEqual(new Date('2024-01-15T00:00:00Z'));
      });

      const req = httpMock.expectOne(`${apiUrl}/requests`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockRequest);
      req.flush(mockResponse);

      // Expect the refresh call
      const refreshReq = httpMock.expectOne(`${apiUrl}/requests`);
      expect(refreshReq.request.method).toBe('GET');
      refreshReq.flush({ success: true, data: [] });
    });

    it('should get my leave requests', () => {
      const mockResponse = {
        success: true,
        data: [
          {
            id: 1,
            employeeId: 1,
            employeeName: 'John Doe',
            leavePolicyId: 1,
            leaveType: LeaveType.Annual,
            leaveTypeName: 'Annual Leave',
            startDate: '2024-01-15T00:00:00Z',
            endDate: '2024-01-17T00:00:00Z',
            requestedDays: 3,
            approvedDays: 3,
            reason: 'Personal vacation',
            status: LeaveStatus.Approved,
            isEmergency: false,
            createdAt: '2024-01-10T00:00:00Z',
            approvalHistory: []
          }
        ]
      };

      service.getMyLeaveRequests().subscribe(requests => {
        expect(requests).toBeTruthy();
        expect(requests.length).toBe(1);
        expect(requests[0].id).toBe(1);
        expect(requests[0].startDate).toEqual(new Date('2024-01-15T00:00:00Z'));
      });

      const req = httpMock.expectOne(`${apiUrl}/requests`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should update a leave request', () => {
      const requestId = 1;
      const mockRequest: CreateLeaveRequest = {
        leavePolicyId: 1,
        startDate: new Date('2024-01-16'),
        endDate: new Date('2024-01-18'),
        reason: 'Updated vacation dates',
        comments: 'Changed dates',
        isEmergency: false
      };

      const mockResponse = {
        success: true,
        data: {
          id: 1,
          employeeId: 1,
          employeeName: 'John Doe',
          leavePolicyId: 1,
          leaveType: LeaveType.Annual,
          leaveTypeName: 'Annual Leave',
          startDate: '2024-01-16T00:00:00Z',
          endDate: '2024-01-18T00:00:00Z',
          requestedDays: 3,
          approvedDays: 0,
          reason: 'Updated vacation dates',
          comments: 'Changed dates',
          status: LeaveStatus.Pending,
          isEmergency: false,
          createdAt: '2024-01-10T00:00:00Z',
          approvalHistory: []
        }
      };

      service.updateLeaveRequest(requestId, mockRequest).subscribe(response => {
        expect(response).toBeTruthy();
        expect(response.reason).toBe('Updated vacation dates');
        expect(response.startDate).toEqual(new Date('2024-01-16T00:00:00Z'));
      });

      const req = httpMock.expectOne(`${apiUrl}/requests/${requestId}`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(mockRequest);
      req.flush(mockResponse);

      // Expect the refresh call
      const refreshReq = httpMock.expectOne(`${apiUrl}/requests`);
      expect(refreshReq.request.method).toBe('GET');
      refreshReq.flush({ success: true, data: [] });
    });

    it('should cancel a leave request', () => {
      const requestId = 1;
      const mockResponse = {
        success: true,
        message: 'Leave request cancelled successfully'
      };

      service.cancelLeaveRequest(requestId).subscribe(response => {
        expect(response).toBe(true);
      });

      const req = httpMock.expectOne(`${apiUrl}/requests/${requestId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(mockResponse);

      // Expect the refresh call
      const refreshReq = httpMock.expectOne(`${apiUrl}/requests`);
      expect(refreshReq.request.method).toBe('GET');
      refreshReq.flush({ success: true, data: [] });
    });
  });

  describe('Leave Balance Management', () => {
    it('should get my leave balances', () => {
      const mockResponse = {
        success: true,
        data: [
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
        ]
      };

      service.getMyLeaveBalances().subscribe(balances => {
        expect(balances).toBeTruthy();
        expect(balances.length).toBe(1);
        expect(balances[0].remainingDays).toBe(17);
        expect(balances[0].leaveType).toBe(LeaveType.Annual);
      });

      const req = httpMock.expectOne(`${apiUrl}/balances`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('Leave Policy Management', () => {
    it('should get leave policies', () => {
      const mockResponse = {
        success: true,
        data: [
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
        ]
      };

      service.getLeavePolicies().subscribe(policies => {
        expect(policies).toBeTruthy();
        expect(policies.length).toBe(1);
        expect(policies[0].name).toBe('Annual Leave');
        expect(policies[0].annualAllocation).toBe(20);
      });

      const req = httpMock.expectOne(`${apiUrl}/policies`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('Leave Calendar', () => {
    it('should get leave calendar', () => {
      const startDate = new Date('2024-01-01');
      const endDate = new Date('2024-01-31');
      
      const mockResponse = {
        success: true,
        data: [
          {
            id: 1,
            employeeId: 1,
            employeeName: 'John Doe',
            leaveRequestId: 1,
            date: '2024-01-15T00:00:00Z',
            isFullDay: true,
            leaveType: LeaveType.Annual,
            leaveTypeName: 'Annual Leave',
            status: LeaveStatus.Approved
          }
        ]
      };

      service.getLeaveCalendar(startDate, endDate).subscribe(entries => {
        expect(entries).toBeTruthy();
        expect(entries.length).toBe(1);
        expect(entries[0].employeeName).toBe('John Doe');
        expect(entries[0].date).toEqual(new Date('2024-01-15T00:00:00Z'));
      });

      const req = httpMock.expectOne(request => 
        request.url === `${apiUrl}/calendar` && 
        request.params.get('startDate') === startDate.toISOString() &&
        request.params.get('endDate') === endDate.toISOString()
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('Conflict Detection', () => {
    it('should detect leave conflicts', () => {
      const startDate = new Date('2024-01-15');
      const endDate = new Date('2024-01-17');
      
      const mockResponse = {
        success: true,
        data: [
          {
            employeeId: 2,
            employeeName: 'Jane Smith',
            department: 'Engineering',
            conflictDate: '2024-01-16T00:00:00Z',
            conflictReason: 'Already on Annual Leave',
            conflictingRequestId: 2
          }
        ]
      };

      service.detectLeaveConflicts(startDate, endDate).subscribe(conflicts => {
        expect(conflicts).toBeTruthy();
        expect(conflicts.length).toBe(1);
        expect(conflicts[0].employeeName).toBe('Jane Smith');
        expect(conflicts[0].conflictDate).toEqual(new Date('2024-01-16T00:00:00Z'));
      });

      const req = httpMock.expectOne(request => 
        request.url === `${apiUrl}/conflicts` && 
        request.params.get('startDate') === startDate.toISOString() &&
        request.params.get('endDate') === endDate.toISOString()
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('Utility Methods', () => {
    it('should calculate leave days', () => {
      const startDate = new Date('2024-01-15');
      const endDate = new Date('2024-01-17');
      
      const mockResponse = {
        success: true,
        data: { days: 3 }
      };

      service.calculateLeaveDays(startDate, endDate).subscribe(days => {
        expect(days).toBe(3);
      });

      const req = httpMock.expectOne(request => 
        request.url === `${apiUrl}/calculate-days` && 
        request.params.get('startDate') === startDate.toISOString() &&
        request.params.get('endDate') === endDate.toISOString()
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });

    it('should convert calendar entries to calendar events', () => {
      const calendarEntries: LeaveCalendarEntry[] = [
        {
          id: 1,
          employeeId: 1,
          employeeName: 'John Doe',
          leaveRequestId: 1,
          date: new Date('2024-01-15'),
          isFullDay: true,
          leaveType: LeaveType.Annual,
          leaveTypeName: 'Annual Leave',
          status: LeaveStatus.Approved
        }
      ];

      const events = service.convertToCalendarEvents(calendarEntries);
      
      expect(events).toBeTruthy();
      expect(events.length).toBe(1);
      expect(events[0].id).toBe('leave-1');
      expect(events[0].title).toBe('John Doe - Annual Leave');
      expect(events[0].allDay).toBe(true);
      expect(events[0].extendedProps.employeeId).toBe(1);
    });

    it('should get correct leave type colors', () => {
      expect(service.getLeaveTypeColor(LeaveType.Annual)).toBe('#28a745');
      expect(service.getLeaveTypeColor(LeaveType.Sick)).toBe('#dc3545');
      expect(service.getLeaveTypeColor(LeaveType.Personal)).toBe('#17a2b8');
    });

    it('should get correct leave status colors', () => {
      expect(service.getLeaveStatusColor(LeaveStatus.Pending)).toBe('#ffc107');
      expect(service.getLeaveStatusColor(LeaveStatus.Approved)).toBe('#28a745');
      expect(service.getLeaveStatusColor(LeaveStatus.Rejected)).toBe('#dc3545');
    });

    it('should get correct leave type text', () => {
      expect(service.getLeaveTypeText(LeaveType.Annual)).toBe('Annual Leave');
      expect(service.getLeaveTypeText(LeaveType.Sick)).toBe('Sick Leave');
      expect(service.getLeaveTypeText(LeaveType.Personal)).toBe('Personal Leave');
    });

    it('should get correct leave status text', () => {
      expect(service.getLeaveStatusText(LeaveStatus.Pending)).toBe('Pending');
      expect(service.getLeaveStatusText(LeaveStatus.Approved)).toBe('Approved');
      expect(service.getLeaveStatusText(LeaveStatus.Rejected)).toBe('Rejected');
    });
  });
});