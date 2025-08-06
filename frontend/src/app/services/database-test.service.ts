import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface DatabaseHealthStatus {
  isHealthy: boolean;
  checkedAt: string;
  errorMessage?: string;
  details: { [key: string]: any };
}

export interface ConnectionTestResult {
  canConnect: boolean;
  message: string;
  timestamp: string;
}

export interface InitializationResult {
  message: string;
  success: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class DatabaseTestService {
  private readonly apiUrl = `${environment.apiUrl}/DatabaseTest`;

  constructor(private http: HttpClient) {}

  checkDatabaseHealth(): Observable<DatabaseHealthStatus> {
    return this.http.get<DatabaseHealthStatus>(`${this.apiUrl}/health`);
  }

  testConnection(): Observable<ConnectionTestResult> {
    return this.http.get<ConnectionTestResult>(`${this.apiUrl}/connection-test`);
  }

  initializeDatabase(): Observable<InitializationResult> {
    return this.http.post<InitializationResult>(`${this.apiUrl}/initialize`, {});
  }
}