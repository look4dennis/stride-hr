import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  PerformanceReview, 
  PIP, 
  TrainingModule, 
  EmployeeTraining, 
  Certification,
  CreatePerformanceReviewDto,
  CreatePIPDto,
  CreateTrainingModuleDto,
  EnrollEmployeeDto
} from '../models/performance.models';

@Injectable({
  providedIn: 'root'
})
export class PerformanceService {
  private readonly apiUrl = '/api/performance';

  constructor(private http: HttpClient) {}

  // Performance Review Methods
  getPerformanceReviews(employeeId?: number, status?: string): Observable<PerformanceReview[]> {
    let params = new HttpParams();
    if (employeeId) params = params.set('employeeId', employeeId.toString());
    if (status) params = params.set('status', status);
    
    return this.http.get<PerformanceReview[]>(`${this.apiUrl}/reviews`, { params });
  }

  getPerformanceReview(id: number): Observable<PerformanceReview> {
    return this.http.get<PerformanceReview>(`${this.apiUrl}/reviews/${id}`);
  }

  createPerformanceReview(review: CreatePerformanceReviewDto): Observable<PerformanceReview> {
    return this.http.post<PerformanceReview>(`${this.apiUrl}/reviews`, review);
  }

  updatePerformanceReview(id: number, review: Partial<PerformanceReview>): Observable<PerformanceReview> {
    return this.http.put<PerformanceReview>(`${this.apiUrl}/reviews/${id}`, review);
  }

  submitPerformanceReview(id: number): Observable<PerformanceReview> {
    return this.http.post<PerformanceReview>(`${this.apiUrl}/reviews/${id}/submit`, {});
  }

  approvePerformanceReview(id: number, comments: string): Observable<PerformanceReview> {
    return this.http.post<PerformanceReview>(`${this.apiUrl}/reviews/${id}/approve`, { comments });
  }

  // PIP Methods
  getPIPs(employeeId?: number, status?: string): Observable<PIP[]> {
    let params = new HttpParams();
    if (employeeId) params = params.set('employeeId', employeeId.toString());
    if (status) params = params.set('status', status);
    
    return this.http.get<PIP[]>(`${this.apiUrl}/pips`, { params });
  }

  getPIP(id: number): Observable<PIP> {
    return this.http.get<PIP>(`${this.apiUrl}/pips/${id}`);
  }

  createPIP(pip: CreatePIPDto): Observable<PIP> {
    return this.http.post<PIP>(`${this.apiUrl}/pips`, pip);
  }

  updatePIP(id: number, pip: Partial<PIP>): Observable<PIP> {
    return this.http.put<PIP>(`${this.apiUrl}/pips/${id}`, pip);
  }

  updatePIPProgress(id: number, progress: any): Observable<PIP> {
    return this.http.post<PIP>(`${this.apiUrl}/pips/${id}/progress`, progress);
  }

  completePIP(id: number, outcome: string, comments: string): Observable<PIP> {
    return this.http.post<PIP>(`${this.apiUrl}/pips/${id}/complete`, { outcome, comments });
  }

  // Training Module Methods
  getTrainingModules(category?: string, difficulty?: string): Observable<TrainingModule[]> {
    let params = new HttpParams();
    if (category) params = params.set('category', category);
    if (difficulty) params = params.set('difficulty', difficulty);
    
    return this.http.get<TrainingModule[]>(`${this.apiUrl}/training/modules`, { params });
  }

  getTrainingModule(id: number): Observable<TrainingModule> {
    return this.http.get<TrainingModule>(`${this.apiUrl}/training/modules/${id}`);
  }

  createTrainingModule(module: CreateTrainingModuleDto): Observable<TrainingModule> {
    return this.http.post<TrainingModule>(`${this.apiUrl}/training/modules`, module);
  }

  updateTrainingModule(id: number, module: Partial<TrainingModule>): Observable<TrainingModule> {
    return this.http.put<TrainingModule>(`${this.apiUrl}/training/modules/${id}`, module);
  }

  deleteTrainingModule(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/training/modules/${id}`);
  }

  // Employee Training Methods
  getEmployeeTrainings(employeeId?: number, status?: string): Observable<EmployeeTraining[]> {
    let params = new HttpParams();
    if (employeeId) params = params.set('employeeId', employeeId.toString());
    if (status) params = params.set('status', status);
    
    return this.http.get<EmployeeTraining[]>(`${this.apiUrl}/training/enrollments`, { params });
  }

  enrollEmployee(enrollment: EnrollEmployeeDto): Observable<EmployeeTraining> {
    return this.http.post<EmployeeTraining>(`${this.apiUrl}/training/enroll`, enrollment);
  }

  startTraining(enrollmentId: number): Observable<EmployeeTraining> {
    return this.http.post<EmployeeTraining>(`${this.apiUrl}/training/enrollments/${enrollmentId}/start`, {});
  }

  updateTrainingProgress(enrollmentId: number, progress: number): Observable<EmployeeTraining> {
    return this.http.put<EmployeeTraining>(`${this.apiUrl}/training/enrollments/${enrollmentId}/progress`, { progress });
  }

  completeTraining(enrollmentId: number, score: number): Observable<EmployeeTraining> {
    return this.http.post<EmployeeTraining>(`${this.apiUrl}/training/enrollments/${enrollmentId}/complete`, { score });
  }

  // Certification Methods
  getCertifications(employeeId?: number): Observable<Certification[]> {
    let params = new HttpParams();
    if (employeeId) params = params.set('employeeId', employeeId.toString());
    
    return this.http.get<Certification[]>(`${this.apiUrl}/certifications`, { params });
  }

  getCertification(id: number): Observable<Certification> {
    return this.http.get<Certification>(`${this.apiUrl}/certifications/${id}`);
  }

  downloadCertificate(id: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/certifications/${id}/download`, { responseType: 'blob' });
  }

  // Analytics Methods
  getPerformanceAnalytics(employeeId?: number, period?: string): Observable<any> {
    let params = new HttpParams();
    if (employeeId) params = params.set('employeeId', employeeId.toString());
    if (period) params = params.set('period', period);
    
    return this.http.get<any>(`${this.apiUrl}/analytics`, { params });
  }

  getTrainingAnalytics(employeeId?: number): Observable<any> {
    let params = new HttpParams();
    if (employeeId) params = params.set('employeeId', employeeId.toString());
    
    return this.http.get<any>(`${this.apiUrl}/training/analytics`, { params });
  }
}