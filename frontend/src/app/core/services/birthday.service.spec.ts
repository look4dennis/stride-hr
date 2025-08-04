import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { BirthdayService, BirthdayEmployee, BirthdayWish } from './birthday.service';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('BirthdayService', () => {
  let service: BirthdayService;
  let httpMock: HttpTestingController;

  const mockBirthdayEmployees: BirthdayEmployee[] = [
    {
      id: 1,
      employeeId: 'EMP001',
      firstName: 'John',
      lastName: 'Doe',
      profilePhoto: '/assets/images/avatars/john-doe.jpg',
      department: 'Development',
      designation: 'Senior Developer',
      dateOfBirth: '1995-01-15',
      age: 28
    },
    {
      id: 2,
      employeeId: 'EMP002',
      firstName: 'Jane',
      lastName: 'Smith',
      profilePhoto: '/assets/images/avatars/jane-smith.jpg',
      department: 'HR',
      designation: 'HR Manager',
      dateOfBirth: '1991-01-15',
      age: 32
    }
  ];

  const mockBirthdayWish: BirthdayWish = {
    id: 1,
    fromEmployeeId: 1,
    toEmployeeId: 2,
    message: 'Happy Birthday!',
    sentAt: '2023-01-15T10:00:00Z'
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [],
    providers: [BirthdayService, provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    
    httpMock = TestBed.inject(HttpTestingController);
    service = TestBed.inject(BirthdayService);
    
    // Handle the initialization request
    const initReq = httpMock.expectOne('http://localhost:5000/api/employees/birthdays/today');
    initReq.flush(mockBirthdayEmployees);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get today\'s birthdays', () => {
    service.getTodayBirthdays().subscribe(birthdays => {
      expect(birthdays).toEqual(mockBirthdayEmployees);
    });

    const req = httpMock.expectOne('http://localhost:5000/api/employees/birthdays/today');
    expect(req.request.method).toBe('GET');
    req.flush(mockBirthdayEmployees);
  });

  it('should get upcoming birthdays with default days parameter', () => {
    service.getUpcomingBirthdays().subscribe(birthdays => {
      expect(birthdays).toEqual(mockBirthdayEmployees);
    });

    const req = httpMock.expectOne('http://localhost:5000/api/employees/birthdays/upcoming?days=7');
    expect(req.request.method).toBe('GET');
    req.flush(mockBirthdayEmployees);
  });

  it('should get upcoming birthdays with custom days parameter', () => {
    service.getUpcomingBirthdays(14).subscribe(birthdays => {
      expect(birthdays).toEqual(mockBirthdayEmployees);
    });

    const req = httpMock.expectOne('http://localhost:5000/api/employees/birthdays/upcoming?days=14');
    expect(req.request.method).toBe('GET');
    req.flush(mockBirthdayEmployees);
  });

  it('should send birthday wish', () => {
    const toEmployeeId = 2;
    const message = 'Happy Birthday!';

    service.sendBirthdayWish(toEmployeeId, message).subscribe(wish => {
      expect(wish).toEqual(mockBirthdayWish);
    });

    const req = httpMock.expectOne('http://localhost:5000/api/employees/birthday-wishes');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ toEmployeeId, message });
    req.flush(mockBirthdayWish);
  });

  it('should get birthday wishes for employee', () => {
    const employeeId = 1;
    const mockWishes = [mockBirthdayWish];

    service.getBirthdayWishes(employeeId).subscribe(wishes => {
      expect(wishes).toEqual(mockWishes);
    });

    const req = httpMock.expectOne(`http://localhost:5000/api/employees/${employeeId}/birthday-wishes`);
    expect(req.request.method).toBe('GET');
    req.flush(mockWishes);
  });

  it('should load today\'s birthdays on initialization', () => {
    // The service should have already made an HTTP request on initialization (handled in beforeEach)
    expect(service).toBeTruthy();
  });

  it('should handle API error and load mock data', () => {
    // Create a new service instance to test error handling
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
    imports: [],
    providers: [BirthdayService, provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
    
    const httpMockNew = TestBed.inject(HttpTestingController);
    const newService = TestBed.inject(BirthdayService);
    
    // The initialization request will fail
    const req = httpMockNew.expectOne('http://localhost:5000/api/employees/birthdays/today');
    req.error(new ErrorEvent('Network error'));

    // Should fall back to mock data
    const currentBirthdays = newService.getCurrentBirthdays();
    expect(currentBirthdays.length).toBeGreaterThan(0);
    expect(currentBirthdays[0].firstName).toBeDefined();
    
    httpMockNew.verify();
  });

  it('should provide mock birthday data', () => {
    const mockData = service['getMockBirthdays']();
    expect(mockData.length).toBe(2);
    expect(mockData[0].firstName).toBe('John');
    expect(mockData[0].lastName).toBe('Doe');
    expect(mockData[1].firstName).toBe('Jane');
    expect(mockData[1].lastName).toBe('Smith');
  });

  it('should generate correct date format for mock data', () => {
    const mockData = service['getMockBirthdays']();
    
    mockData.forEach(employee => {
      expect(employee.dateOfBirth).toMatch(/\d{4}-\d{2}-\d{2}/);
    });
  });

  it('should get current birthdays from subject', () => {
    // After initialization (handled in beforeEach), should have birthday data
    const currentBirthdays = service.getCurrentBirthdays();
    expect(Array.isArray(currentBirthdays)).toBe(true);
    expect(currentBirthdays).toEqual(mockBirthdayEmployees);
  });

  it('should refresh birthday data', () => {
    // Call refresh (initial request was handled in beforeEach)
    service.refreshBirthdays();

    // Should make a new HTTP request
    const refreshReq = httpMock.expectOne('http://localhost:5000/api/employees/birthdays/today');
    expect(refreshReq.request.method).toBe('GET');
    refreshReq.flush(mockBirthdayEmployees);
  });

  it('should emit birthday data through observable', (done) => {
    service.todayBirthdays$.subscribe(birthdays => {
      if (birthdays.length > 0) {
        expect(birthdays).toEqual(mockBirthdayEmployees);
        done?.();
      }
    });
    
    // The data should already be available from initialization
  });

  it('should handle empty birthday response', () => {
    service.getTodayBirthdays().subscribe(birthdays => {
      expect(birthdays).toEqual([]);
    });

    const req = httpMock.expectOne('http://localhost:5000/api/employees/birthdays/today');
    req.flush([]);
  });

  it('should handle HTTP error when sending birthday wish', () => {
    service.sendBirthdayWish(1, 'Happy Birthday!').subscribe({
      next: () => fail('Should have failed'),
      error: (error) => {
        expect(error).toBeDefined();
      }
    });

    const req = httpMock.expectOne('http://localhost:5000/api/employees/birthday-wishes');
    req.error(new ErrorEvent('Network error'));
  });

  it('should handle HTTP error when getting birthday wishes', () => {
    service.getBirthdayWishes(1).subscribe({
      next: () => fail('Should have failed'),
      error: (error) => {
        expect(error).toBeDefined();
      }
    });

    const req = httpMock.expectOne('http://localhost:5000/api/employees/1/birthday-wishes');
    req.error(new ErrorEvent('Network error'));
  });
});