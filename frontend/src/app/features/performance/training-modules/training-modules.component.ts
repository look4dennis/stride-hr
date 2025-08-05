import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NgbModal, NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { PerformanceService } from '../../../services/performance.service';
import { EmployeeService } from '../../../services/employee.service';
import { 
  TrainingModule, 
  TrainingDifficulty, 
  MaterialType, 
  QuestionType,
  CreateTrainingModuleDto,
  EmployeeTraining,
  TrainingStatus
} from '../../../models/performance.models';
import { Employee } from '../../../models/employee.models';

@Component({
  selector: 'app-training-modules',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, NgbModule],
  template: `
    <div class="container-fluid">
      <div class="page-header d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 class="h3 mb-0">Training Modules</h1>
          <p class="text-muted">Manage training content and employee enrollments</p>
        </div>
        <div class="btn-group">
          <button class="btn btn-primary" (click)="openCreateModuleModal()">
            <i class="fas fa-plus me-2"></i>Create Module
          </button>
          <button class="btn btn-outline-primary" (click)="openEnrollModal()">
            <i class="fas fa-user-plus me-2"></i>Enroll Employee
          </button>
        </div>
      </div>

      <!-- Filters -->
      <div class="card mb-4">
        <div class="card-body">
          <div class="row g-3">
            <div class="col-md-3">
              <label class="form-label">Category</label>
              <select class="form-select" [(ngModel)]="selectedCategory" (change)="loadModules()">
                <option value="">All Categories</option>
                <option value="Technical">Technical</option>
                <option value="Soft Skills">Soft Skills</option>
                <option value="Leadership">Leadership</option>
                <option value="Compliance">Compliance</option>
                <option value="Safety">Safety</option>
              </select>
            </div>
            <div class="col-md-3">
              <label class="form-label">Difficulty</label>
              <select class="form-select" [(ngModel)]="selectedDifficulty" (change)="loadModules()">
                <option value="">All Levels</option>
                <option value="Beginner">Beginner</option>
                <option value="Intermediate">Intermediate</option>
                <option value="Advanced">Advanced</option>
                <option value="Expert">Expert</option>
              </select>
            </div>
            <div class="col-md-4">
              <label class="form-label">Search</label>
              <input type="text" class="form-control" [(ngModel)]="searchTerm" 
                     placeholder="Search modules..." (input)="loadModules()">
            </div>
            <div class="col-md-2 d-flex align-items-end">
              <button class="btn btn-outline-secondary w-100" (click)="clearFilters()">
                <i class="fas fa-times me-2"></i>Clear
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Modules Grid -->
      <div class="row">
        <div class="col-12" *ngIf="modules.length > 0; else noModules">
          <div class="row">
            <div class="col-lg-4 col-md-6 mb-4" *ngFor="let module of modules">
              <div class="card h-100">
                <div class="card-header d-flex justify-content-between align-items-center">
                  <div>
                    <h6 class="card-title mb-0">{{module.title}}</h6>
                    <small class="text-muted">{{module.category}}</small>
                  </div>
                  <span class="badge" [ngClass]="getDifficultyBadgeClass(module.difficulty)">
                    {{module.difficulty}}
                  </span>
                </div>
                <div class="card-body">
                  <p class="card-text text-muted small">{{module.description | slice:0:100}}{{module.description.length > 100 ? '...' : ''}}</p>
                  
                  <!-- Module Stats -->
                  <div class="row text-center mb-3">
                    <div class="col-4">
                      <div class="fw-bold">{{module.duration}}</div>
                      <small class="text-muted">Minutes</small>
                    </div>
                    <div class="col-4">
                      <div class="fw-bold">{{module.materials?.length || 0}}</div>
                      <small class="text-muted">Materials</small>
                    </div>
                    <div class="col-4">
                      <div class="fw-bold">{{module.assessments?.length || 0}}</div>
                      <small class="text-muted">Assessments</small>
                    </div>
                  </div>

                  <!-- Prerequisites -->
                  <div *ngIf="module.prerequisites && module.prerequisites.length > 0" class="mb-3">
                    <small class="text-muted d-block mb-1">Prerequisites:</small>
                    <div class="d-flex flex-wrap gap-1">
                      <span *ngFor="let prereq of module.prerequisites.slice(0, 2)" 
                            class="badge bg-light text-dark">Module {{prereq}}</span>
                      <span *ngIf="module.prerequisites.length > 2" 
                            class="badge bg-light text-dark">+{{module.prerequisites.length - 2}} more</span>
                    </div>
                  </div>

                  <!-- Enrollment Stats -->
                  <div class="mb-3">
                    <div class="d-flex justify-content-between align-items-center mb-1">
                      <small class="text-muted">Enrollments</small>
                      <small class="text-muted">{{getEnrollmentStats(module.id).total}} total</small>
                    </div>
                    <div class="progress" style="height: 6px;">
                      <div class="progress-bar bg-success" 
                           [style.width.%]="getCompletionRate(module.id)"></div>
                    </div>
                    <small class="text-muted">{{getCompletionRate(module.id)}}% completion rate</small>
                  </div>
                </div>
                <div class="card-footer">
                  <div class="btn-group w-100">
                    <button class="btn btn-outline-primary btn-sm" (click)="viewModule(module)">
                      <i class="fas fa-eye me-1"></i>View
                    </button>
                    <button class="btn btn-outline-secondary btn-sm" (click)="editModule(module)">
                      <i class="fas fa-edit me-1"></i>Edit
                    </button>
                    <button class="btn btn-outline-info btn-sm" (click)="viewEnrollments(module)">
                      <i class="fas fa-users me-1"></i>Enrollments
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <ng-template #noModules>
          <div class="col-12">
            <div class="card">
              <div class="card-body text-center py-5">
                <i class="fas fa-graduation-cap text-muted mb-3" style="font-size: 3rem;"></i>
                <h4>No Training Modules Found</h4>
                <p class="text-muted">Create your first training module to get started.</p>
                <button class="btn btn-primary" (click)="openCreateModuleModal()">
                  <i class="fas fa-plus me-2"></i>Create Module
                </button>
              </div>
            </div>
          </div>
        </ng-template>
      </div>
    </div>

    <!-- Create/Edit Module Modal -->
    <ng-template #moduleModal let-modal>
      <div class="modal-header">
        <h4 class="modal-title">{{isEditMode ? 'Edit' : 'Create'}} Training Module</h4>
        <button type="button" class="btn-close" (click)="modal.dismiss()"></button>
      </div>
      <form [formGroup]="moduleForm" (ngSubmit)="saveModule(modal)">
        <div class="modal-body">
          <div class="row g-3">
            <div class="col-md-8">
              <label class="form-label">Title *</label>
              <input type="text" class="form-control" formControlName="title" 
                     placeholder="Training module title">
              <div class="invalid-feedback" *ngIf="moduleForm.get('title')?.invalid && moduleForm.get('title')?.touched">
                Title is required
              </div>
            </div>
            <div class="col-md-4">
              <label class="form-label">Duration (minutes) *</label>
              <input type="number" class="form-control" formControlName="duration" 
                     min="1" placeholder="60">
              <div class="invalid-feedback" *ngIf="moduleForm.get('duration')?.invalid && moduleForm.get('duration')?.touched">
                Duration is required
              </div>
            </div>
            <div class="col-md-6">
              <label class="form-label">Category *</label>
              <select class="form-select" formControlName="category">
                <option value="">Select Category</option>
                <option value="Technical">Technical</option>
                <option value="Soft Skills">Soft Skills</option>
                <option value="Leadership">Leadership</option>
                <option value="Compliance">Compliance</option>
                <option value="Safety">Safety</option>
              </select>
              <div class="invalid-feedback" *ngIf="moduleForm.get('category')?.invalid && moduleForm.get('category')?.touched">
                Category is required
              </div>
            </div>
            <div class="col-md-6">
              <label class="form-label">Difficulty *</label>
              <select class="form-select" formControlName="difficulty">
                <option value="">Select Difficulty</option>
                <option value="Beginner">Beginner</option>
                <option value="Intermediate">Intermediate</option>
                <option value="Advanced">Advanced</option>
                <option value="Expert">Expert</option>
              </select>
              <div class="invalid-feedback" *ngIf="moduleForm.get('difficulty')?.invalid && moduleForm.get('difficulty')?.touched">
                Difficulty is required
              </div>
            </div>
            <div class="col-12">
              <label class="form-label">Description *</label>
              <textarea class="form-control" formControlName="description" rows="3"
                        placeholder="Describe what this training module covers..."></textarea>
              <div class="invalid-feedback" *ngIf="moduleForm.get('description')?.invalid && moduleForm.get('description')?.touched">
                Description is required
              </div>
            </div>
            <div class="col-12">
              <label class="form-label">Content *</label>
              <textarea class="form-control" formControlName="content" rows="5"
                        placeholder="Enter the training content, instructions, and learning objectives..."></textarea>
              <div class="invalid-feedback" *ngIf="moduleForm.get('content')?.invalid && moduleForm.get('content')?.touched">
                Content is required
              </div>
            </div>
            <div class="col-12">
              <label class="form-label">Prerequisites</label>
              <select class="form-select" multiple formControlName="prerequisites">
                <option *ngFor="let module of availableModules" [value]="module.id">
                  {{module.title}}
                </option>
              </select>
              <small class="form-text text-muted">Hold Ctrl/Cmd to select multiple modules</small>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" (click)="modal.dismiss()">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="moduleForm.invalid || loading">
            <span *ngIf="loading" class="spinner-border spinner-border-sm me-2"></span>
            {{isEditMode ? 'Update' : 'Create'}} Module
          </button>
        </div>
      </form>
    </ng-template>

    <!-- Enroll Employee Modal -->
    <ng-template #enrollModal let-modal>
      <div class="modal-header">
        <h4 class="modal-title">Enroll Employee in Training</h4>
        <button type="button" class="btn-close" (click)="modal.dismiss()"></button>
      </div>
      <form [formGroup]="enrollForm" (ngSubmit)="enrollEmployee(modal)">
        <div class="modal-body">
          <div class="row g-3">
            <div class="col-12">
              <label class="form-label">Employee *</label>
              <select class="form-select" formControlName="employeeId">
                <option value="">Select Employee</option>
                <option *ngFor="let employee of employees" [value]="employee.id">
                  {{employee.firstName}} {{employee.lastName}} ({{employee.employeeId}}) - {{employee.designation}}
                </option>
              </select>
              <div class="invalid-feedback" *ngIf="enrollForm.get('employeeId')?.invalid && enrollForm.get('employeeId')?.touched">
                Please select an employee
              </div>
            </div>
            <div class="col-12">
              <label class="form-label">Training Module *</label>
              <select class="form-select" formControlName="trainingModuleId">
                <option value="">Select Training Module</option>
                <option *ngFor="let module of modules" [value]="module.id">
                  {{module.title}} ({{module.category}} - {{module.difficulty}})
                </option>
              </select>
              <div class="invalid-feedback" *ngIf="enrollForm.get('trainingModuleId')?.invalid && enrollForm.get('trainingModuleId')?.touched">
                Please select a training module
              </div>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" (click)="modal.dismiss()">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="enrollForm.invalid || loading">
            <span *ngIf="loading" class="spinner-border spinner-border-sm me-2"></span>
            Enroll Employee
          </button>
        </div>
      </form>
    </ng-template>
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
    
    .modal-body {
      max-height: 70vh;
      overflow-y: auto;
    }
    
    .btn-group .btn {
      flex: 1;
    }
  `]
})
export class TrainingModulesComponent implements OnInit {
  modules: TrainingModule[] = [];
  availableModules: TrainingModule[] = [];
  employees: Employee[] = [];
  enrollments: EmployeeTraining[] = [];
  
  selectedCategory: string = '';
  selectedDifficulty: string = '';
  searchTerm: string = '';
  loading = false;
  
  moduleForm: FormGroup;
  enrollForm: FormGroup;
  isEditMode = false;
  currentModule: TrainingModule | null = null;

  constructor(
    private performanceService: PerformanceService,
    private employeeService: EmployeeService,
    private modalService: NgbModal,
    private fb: FormBuilder,
    private router: Router
  ) {
    this.moduleForm = this.createModuleForm();
    this.enrollForm = this.createEnrollForm();
  }

  ngOnInit() {
    this.loadModules();
    this.loadEmployees();
    this.loadEnrollments();
  }

  createModuleForm(): FormGroup {
    return this.fb.group({
      title: ['', Validators.required],
      description: ['', Validators.required],
      category: ['', Validators.required],
      duration: ['', [Validators.required, Validators.min(1)]],
      difficulty: ['', Validators.required],
      content: ['', Validators.required],
      prerequisites: [[]]
    });
  }

  createEnrollForm(): FormGroup {
    return this.fb.group({
      employeeId: ['', Validators.required],
      trainingModuleId: ['', Validators.required]
    });
  }

  loadModules() {
    this.loading = true;
    this.performanceService.getTrainingModules(this.selectedCategory, this.selectedDifficulty).subscribe({
      next: (modules) => {
        this.modules = modules.filter(module => 
          !this.searchTerm || 
          module.title.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
          module.description.toLowerCase().includes(this.searchTerm.toLowerCase())
        );
        this.availableModules = modules;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading modules:', error);
        this.loading = false;
      }
    });
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

  loadEnrollments() {
    this.performanceService.getEmployeeTrainings().subscribe({
      next: (enrollments) => {
        this.enrollments = enrollments;
      },
      error: (error) => {
        console.error('Error loading enrollments:', error);
      }
    });
  }

  clearFilters() {
    this.selectedCategory = '';
    this.selectedDifficulty = '';
    this.searchTerm = '';
    this.loadModules();
  }

  openCreateModuleModal() {
    this.isEditMode = false;
    this.currentModule = null;
    this.moduleForm = this.createModuleForm();
    this.modalService.open(this.moduleModal, { size: 'lg', backdrop: 'static' });
  }

  editModule(module: TrainingModule) {
    this.isEditMode = true;
    this.currentModule = module;
    this.moduleForm.patchValue({
      title: module.title,
      description: module.description,
      category: module.category,
      duration: module.duration,
      difficulty: module.difficulty,
      content: module.content,
      prerequisites: module.prerequisites || []
    });
    this.modalService.open(this.moduleModal, { size: 'lg', backdrop: 'static' });
  }

  saveModule(modal: any) {
    if (this.moduleForm.valid) {
      this.loading = true;
      const formValue = this.moduleForm.value;
      
      const moduleData: CreateTrainingModuleDto = {
        title: formValue.title,
        description: formValue.description,
        category: formValue.category,
        duration: parseInt(formValue.duration),
        difficulty: formValue.difficulty as TrainingDifficulty,
        content: formValue.content,
        prerequisites: formValue.prerequisites || []
      };

      const request = this.isEditMode && this.currentModule
        ? this.performanceService.updateTrainingModule(this.currentModule.id, moduleData)
        : this.performanceService.createTrainingModule(moduleData);

      request.subscribe({
        next: (module) => {
          this.loadModules();
          modal.close();
          this.loading = false;
        },
        error: (error) => {
          console.error('Error saving module:', error);
          this.loading = false;
        }
      });
    }
  }

  openEnrollModal() {
    this.enrollForm = this.createEnrollForm();
    this.modalService.open(this.enrollModalRef, { backdrop: 'static' });
  }

  enrollEmployee(modal: any) {
    if (this.enrollForm.valid) {
      this.loading = true;
      const formValue = this.enrollForm.value;
      
      const enrollmentData = {
        employeeId: parseInt(formValue.employeeId),
        trainingModuleId: parseInt(formValue.trainingModuleId)
      };

      this.performanceService.enrollEmployee(enrollmentData).subscribe({
        next: (enrollment) => {
          this.loadEnrollments();
          modal.close();
          this.loading = false;
        },
        error: (error) => {
          console.error('Error enrolling employee:', error);
          this.loading = false;
        }
      });
    }
  }

  viewModule(module: TrainingModule) {
    this.router.navigate(['/performance/training/modules', module.id]);
  }

  viewEnrollments(module: TrainingModule) {
    this.router.navigate(['/performance/training/modules', module.id, 'enrollments']);
  }

  getDifficultyBadgeClass(difficulty: TrainingDifficulty): string {
    const classes = {
      'Beginner': 'bg-success',
      'Intermediate': 'bg-primary',
      'Advanced': 'bg-warning',
      'Expert': 'bg-danger'
    };
    return classes[difficulty] || 'bg-secondary';
  }

  getEnrollmentStats(moduleId: number): { total: number; completed: number; inProgress: number } {
    const moduleEnrollments = this.enrollments.filter(e => e.trainingModuleId === moduleId);
    return {
      total: moduleEnrollments.length,
      completed: moduleEnrollments.filter(e => e.status === TrainingStatus.Completed).length,
      inProgress: moduleEnrollments.filter(e => e.status === TrainingStatus.InProgress).length
    };
  }

  getCompletionRate(moduleId: number): number {
    const stats = this.getEnrollmentStats(moduleId);
    if (stats.total === 0) return 0;
    return Math.round((stats.completed / stats.total) * 100);
  }

  @ViewChild('moduleModal') moduleModal!: TemplateRef<any>;
  @ViewChild('enrollModal') enrollModalRef!: TemplateRef<any>;
}