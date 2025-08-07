import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, catchError, of } from 'rxjs';

export interface BirthdayEmployee {
  id: number;
  employeeId: string;
  firstName: string;
  lastName: string;
  profilePhoto?: string;
  department: string;
  designation: string;
  dateOfBirth: string;
  age: number;
}

export interface BirthdayWish {
  id: number;
  fromEmployeeId: number;
  toEmployeeId: number;
  message: string;
  sentAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class BirthdayService {
  private readonly API_URL = 'https://localhost:5001/api';
  
  private todayBirthdaysSubject = new BehaviorSubject<BirthdayEmployee[]>([]);
  public todayBirthdays$ = this.todayBirthdaysSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadTodayBirthdays();
  }

  getTodayBirthdays(): Observable<BirthdayEmployee[]> {
    return this.http.get<BirthdayEmployee[]>(`${this.API_URL}/employees/birthdays/today`).pipe(
      catchError(error => {
        console.warn('Birthday API not available, using mock data:', error.message);
        return of(this.getMockBirthdays());
      })
    );
  }

  getUpcomingBirthdays(days: number = 7): Observable<BirthdayEmployee[]> {
    return this.http.get<BirthdayEmployee[]>(`${this.API_URL}/employees/birthdays/upcoming?days=${days}`).pipe(
      catchError(error => {
        console.warn('Birthday API not available, using mock data:', error.message);
        return of(this.getMockBirthdays());
      })
    );
  }

  sendBirthdayWish(toEmployeeId: number, message: string): Observable<BirthdayWish> {
    return this.http.post<BirthdayWish>(`${this.API_URL}/employees/birthday-wishes`, {
      toEmployeeId,
      message
    });
  }

  getBirthdayWishes(employeeId: number): Observable<BirthdayWish[]> {
    return this.http.get<BirthdayWish[]>(`${this.API_URL}/employees/${employeeId}/birthday-wishes`);
  }

  private loadTodayBirthdays(): void {
    this.getTodayBirthdays().subscribe({
      next: (birthdays) => {
        this.todayBirthdaysSubject.next(birthdays);
      },
      error: (error) => {
        // This should not happen now since we handle errors in getTodayBirthdays()
        console.warn('Fallback: Loading mock birthday data');
        this.todayBirthdaysSubject.next(this.getMockBirthdays());
      }
    });
  }

  private getMockBirthdays(): BirthdayEmployee[] {
    const today = new Date();
    return [
      {
        id: 1,
        employeeId: 'EMP001',
        firstName: 'John',
        lastName: 'Doe',
        profilePhoto: '/assets/images/avatars/john-doe.jpg',
        department: 'Development',
        designation: 'Senior Developer',
        dateOfBirth: `${today.getFullYear() - 28}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`,
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
        dateOfBirth: `${today.getFullYear() - 32}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`,
        age: 32
      }
    ];
  }

  getCurrentBirthdays(): BirthdayEmployee[] {
    return this.todayBirthdaysSubject.value;
  }

  refreshBirthdays(): void {
    this.loadTodayBirthdays();
  }
}