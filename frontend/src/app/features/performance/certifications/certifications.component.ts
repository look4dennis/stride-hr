import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { PerformanceService } from '../../../services/performance.service';
import { EmployeeService } from '../../../services/employee.service';
import { Certification, EmployeeTraining, TrainingStatus } from '../../../models/performance.models';
import { Employee } from '../../../models/employee.models';

@Component({
  selector: 'app-certifications',
  standalone: true,
  imports: [CommonModule, FormsModule, NgbModule],
  template: `
    <div class="container-fluid">
      <div class="page-header d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 class="h3 mb-0">Certifications</h1>
          <p class="text-muted">Track employee certifications and achievements</p>
        </div>
        <div class="btn-group">
          <button class="btn btn-outline-primary" (click)="viewMode = 'certifications'" 
                  [class.active]="viewMode === 'certifications'">
            <i class="fas fa-certificate me-2"></i>Certifications
          </button>
          <button class="btn btn-outline-primary" (click)="viewMode = 'progress'" 
                  [class.active]="viewMode === 'progress'">
            <i class="fas fa-chart-line me-2"></i>Training Progress
          </button>
        </div>
      </div>

      <!-- Filters -->
      <div class="card mb-4">
        <div class="card-body">
          <div class="row g-3">
            <div class="col-md-3">
              <label class="form-label">Employee</label>
              <select class="form-select" [(ngModel)]="selectedEmployeeId" (change)="loadData()">
                <option value="">All Employees</option>
                <option *ngFor="let employee of employees" [value]="employee.id">
                  {{employee.firstName}} {{employee.lastName}} ({{employee.employeeId}})
                </option>
              </select>
            </div>
            <div class="col-md-3" *ngIf="viewMode === 'certifications'">
              <label class="form-label">Status</label>
              <select class="form-select" [(ngModel)]="selectedStatus" (change)="loadData()">
                <option value="">All Status</option>
                <option value="true">Valid</option>
                <option value="false">Expired</option>
              </select>
            </div>
            <div class="col-md-3" *ngIf="viewMode === 'progress'">
              <label class="form-label">Training Status</label>
              <select class="form-select" [(ngModel)]="selectedTrainingStatus" (change)="loadData()">
                <option value="">All Status</option>
                <option value="NotStarted">Not Started</option>
                <option value="InProgress">In Progress</option>
                <option value="Completed">Completed</option>
                <option value="Failed">Failed</option>
              </select>
            </div>
            <div class="col-md-4">
              <label class="form-label">Search</label>
              <input type="text" class="form-control" [(ngModel)]="searchTerm" 
                     placeholder="Search..." (input)="loadData()">
            </div>
            <div class="col-md-2 d-flex align-items-end">
              <button class="btn btn-outline-secondary w-100" (click)="clearFilters()">
                <i class="fas fa-times me-2"></i>Clear
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Certifications View -->
      <div *ngIf="viewMode === 'certifications'">
        <div class="row" *ngIf="certifications.length > 0; else noCertifications">
          <div class="col-lg-4 col-md-6 mb-4" *ngFor="let cert of certifications">
            <div class="card h-100" [class.border-success]="cert.isValid" [class.border-warning]="!cert.isValid">
              <div class="card-header d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="card-title mb-0">{{cert.trainingModule?.title}}</h6>
                  <small class="text-muted">{{cert.trainingModule?.category}}</small>
                </div>
                <span class="badge" [ngClass]="cert.isValid ? 'bg-success' : 'bg-warning'">
                  {{cert.isValid ? 'Valid' : 'Expired'}}
                </span>
              </div>
              <div class="card-body">
                <!-- Employee Info -->
                <div class="d-flex align-items-center mb-3">
                  <img [src]="cert.employee?.profilePhoto || '/assets/images/default-avatar.png'" 
                       class="rounded-circle me-2" width="32" height="32" alt="Profile">
                  <div>
                    <div class="fw-medium">{{cert.employee?.firstName}} {{cert.employee?.lastName}}</div>
                    <small class="text-muted">{{cert.employee?.employeeId}}</small>
                  </div>
                </div>

                <!-- Certificate Details -->
                <div class="mb-3">
                  <div class="row text-center">
                    <div class="col-6">
                      <div class="fw-bold text-primary">{{cert.score}}/100</div>
                      <small class="text-muted">Score</small>
                    </div>
                    <div class="col-6">
                      <div class="fw-bold">{{cert.certificateNumber}}</div>
                      <small class="text-muted">Certificate #</small>
                    </div>
                  </div>
                </div>

                <!-- Dates -->
                <div class="mb-3">
                  <div class="row">
                    <div class="col-6">
                      <small class="text-muted d-block">Issued Date</small>
                      <small class="fw-medium">{{cert.issuedDate | date:'MMM dd, yyyy'}}</small>
                    </div>
                    <div class="col-6" *ngIf="cert.expiryDate">
                      <small class="text-muted d-block">Expiry Date</small>
                      <small class="fw-medium" [class.text-warning]="!cert.isValid">
                        {{cert.expiryDate | date:'MMM dd, yyyy'}}
                      </small>
                    </div>
                  </div>
                </div>

                <!-- Validity Indicator -->
                <div class="alert" [ngClass]="cert.isValid ? 'alert-success' : 'alert-warning'" role="alert">
                  <i class="fas" [ngClass]="cert.isValid ? 'fa-check-circle' : 'fa-exclamation-triangle'"></i>
                  {{cert.isValid ? 'Certificate is valid' : 'Certificate has expired'}}
                </div>
              </div>
              <div class="card-footer">
                <button class="btn btn-primary btn-sm w-100" (click)="downloadCertificate(cert)">
                  <i class="fas fa-download me-2"></i>Download Certificate
                </button>
              </div>
            </div>
          </div>
        </div>

        <ng-template #noCertifications>
          <div class="card">
            <div class="card-body text-center py-5">
              <i class="fas fa-certificate text-muted mb-3" style="font-size: 3rem;"></i>
              <h4>No Certifications Found</h4>
              <p class="text-muted">Employees will receive certifications after completing training modules.</p>
            </div>
          </div>
        </ng-template>
      </div>

      <!-- Training Progress View -->
      <div *ngIf="viewMode === 'progress'">
        <div class="card" *ngIf="trainings.length > 0; else noTrainings">
          <div class="card-body">
            <div class="table-responsive">
              <table class="table table-hover">
                <thead>
                  <tr>
                    <th>Employee</th>
                    <th>Training Module</th>
                    <th>Status</th>
                    <th>Progress</th>
                    <th>Score</th>
                    <th>Enrolled Date</th>
                    <th>Completion Date</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let training of trainings">
                    <td>
                      <div class="d-flex align-items-center">
                        <img [src]="training.employee?.profilePhoto || '/assets/images/default-avatar.png'" 
                             class="rounded-circle me-2" width="32" height="32" alt="Profile">
                        <div>
                          <div class="fw-medium">{{training.employee?.firstName}} {{training.employee?.lastName}}</div>
                          <small class="text-muted">{{training.employee?.employeeId}}</small>
                        </div>
                      </div>
                    </td>
                    <td>
                      <div>
                        <div class="fw-medium">{{training.trainingModule?.title}}</div>
                        <small class="text-muted">{{training.trainingModule?.category}} - {{training.trainingModule?.difficulty}}</small>
                      </div>
                    </td>
                    <td>
                      <span class="badge" [ngClass]="getTrainingStatusBadgeClass(training.status)">
                        {{training.status}}
                      </span>
                    </td>
                    <td>
                      <div class="d-flex align-items-center">
                        <div class="progress me-2" style="width: 100px; height: 8px;">
                          <div class="progress-bar" 
                               [style.width.%]="training.progress"
                               [ngClass]="getProgressBarClass(training.progress)"></div>
                        </div>
                        <small>{{training.progress}}%</small>
                      </div>
                    </td>
                    <td>
                      <span *ngIf="training.score !== null && training.score !== undefined; else noScore" 
                            class="fw-medium" [class.text-success]="training.score! >= 70" 
                            [class.text-warning]="training.score! < 70 && training.score! >= 50"
                            [class.text-danger]="training.score! < 50">
                        {{training.score}}/100
                      </span>
                      <ng-template #noScore>
                        <span class="text-muted">-</span>
                      </ng-template>
                    </td>
                    <td>{{training.enrolledDate | date:'MMM dd, yyyy'}}</td>
                    <td>
                      <span *ngIf="training.completedDate; else notCompleted">
                        {{training.completedDate | date:'MMM dd, yyyy'}}
                      </span>
                      <ng-template #notCompleted>
                        <span class="text-muted">-</span>
                      </ng-template>
                    </td>
                    <td>
                      <div class="btn-group btn-group-sm">
                        <button class="btn btn-outline-primary" (click)="viewTrainingDetails(training)">
                          <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn btn-outline-success" (click)="startTraining(training)" 
                                *ngIf="training.status === 'NotStarted'">
                          <i class="fas fa-play"></i>
                        </button>
                        <button class="btn btn-outline-info" (click)="downloadCertificateFromTraining(training)" 
                                *ngIf="training.certificateIssued">
                          <i class="fas fa-certificate"></i>
                        </button>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <ng-template #noTrainings>
          <div class="card">
            <div class="card-body text-center py-5">
              <i class="fas fa-chart-line text-muted mb-3" style="font-size: 3rem;"></i>
              <h4>No Training Progress Found</h4>
              <p class="text-muted">Enroll employees in training modules to track their progress.</p>
            </div>
          </div>
        </ng-template>
      </div>
    </div>
  `,
  styles: [`
    .card {
      transition: transform 0.2s ease-in-out;
    }
    
    .card:hover {
      transform: translateY(-2px);
    }
    
    .badge {
      font-size: 0.75rem;
    }
    
    .progress {
      background-color: #e9ecef;
    }
    
    .card-title {
      color: var(--text-primary);
      font-weight: 600;
    }
    
    .btn-group .btn.active {
      background-color: var(--primary);
      color: white;
      border-color: var(--primary);
    }
    
    .alert {
      padding: 0.5rem 0.75rem;
      margin-bottom: 0;
      font-size: 0.875rem;
    }
    
    .border-success {
      border-color: #28a745 !important;
    }
    
    .border-warning {
      border-color: #ffc107 !important;
    }
  `]
})
export class CertificationsComponent implements OnInit {
  certifications: Certification[] = [];
  trainings: EmployeeTraining[] = [];
  employees: Employee[] = [];
  
  viewMode: 'certifications' | 'progress' = 'certifications';
  selectedEmployeeId: string = '';
  selectedStatus: string = '';
  selectedTrainingStatus: string = '';
  searchTerm: string = '';
  loading = false;

  constructor(
    private performanceService: PerformanceService,
    private employeeService: EmployeeService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadEmployees();
    this.loadData();
  }

  loadEmployees() {
    this.employeeService.getEmployees().subscribe({
      next: (result) => {
        this.employees = result.items;
      },
      error: (error) => {
        console.error('Error loading employees:', error);
      }
    });
  }

  loadData() {
    if (this.viewMode === 'certifications') {
      this.loadCertifications();
    } else {
      this.loadTrainings();
    }
  }

  loadCertifications() {
    this.loading = true;
    const employeeId = this.selectedEmployeeId ? parseInt(this.selectedEmployeeId) : undefined;
    
    this.performanceService.getCertifications(employeeId).subscribe({
      next: (certifications) => {
        this.certifications = certifications.filter(cert => {
          const matchesStatus = !this.selectedStatus || 
            (this.selectedStatus === 'true' ? cert.isValid : !cert.isValid);
          const matchesSearch = !this.searchTerm || 
            cert.trainingModule?.title.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
            cert.employee?.firstName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
            cert.employee?.lastName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
            cert.certificateNumber.toLowerCase().includes(this.searchTerm.toLowerCase());
          
          return matchesStatus && matchesSearch;
        });
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading certifications:', error);
        this.loading = false;
      }
    });
  }

  loadTrainings() {
    this.loading = true;
    const employeeId = this.selectedEmployeeId ? parseInt(this.selectedEmployeeId) : undefined;
    
    this.performanceService.getEmployeeTrainings(employeeId, this.selectedTrainingStatus).subscribe({
      next: (trainings) => {
        this.trainings = trainings.filter(training => {
          const matchesSearch = !this.searchTerm || 
            training.trainingModule?.title.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
            training.employee?.firstName.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
            training.employee?.lastName.toLowerCase().includes(this.searchTerm.toLowerCase());
          
          return matchesSearch;
        });
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading trainings:', error);
        this.loading = false;
      }
    });
  }

  clearFilters() {
    this.selectedEmployeeId = '';
    this.selectedStatus = '';
    this.selectedTrainingStatus = '';
    this.searchTerm = '';
    this.loadData();
  }

  downloadCertificate(certification: Certification) {
    this.performanceService.downloadCertificate(certification.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `certificate-${certification.certificateNumber}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Error downloading certificate:', error);
      }
    });
  }

  downloadCertificateFromTraining(training: EmployeeTraining) {
    if (training.certificateUrl) {
      const link = document.createElement('a');
      link.href = training.certificateUrl;
      link.download = `certificate-${training.employee?.employeeId}-${training.trainingModule?.title}.pdf`;
      link.click();
    }
  }

  viewTrainingDetails(training: EmployeeTraining) {
    this.router.navigate(['/performance/training/enrollments', training.id]);
  }

  startTraining(training: EmployeeTraining) {
    this.performanceService.startTraining(training.id).subscribe({
      next: (updatedTraining) => {
        this.loadTrainings();
      },
      error: (error) => {
        console.error('Error starting training:', error);
      }
    });
  }

  getTrainingStatusBadgeClass(status: TrainingStatus): string {
    const classes = {
      'NotStarted': 'bg-secondary',
      'InProgress': 'bg-primary',
      'Completed': 'bg-success',
      'Failed': 'bg-danger',
      'Expired': 'bg-warning'
    };
    return classes[status] || 'bg-secondary';
  }

  getProgressBarClass(progress: number): string {
    if (progress >= 80) return 'bg-success';
    if (progress >= 60) return 'bg-warning';
    return 'bg-danger';
  }
}