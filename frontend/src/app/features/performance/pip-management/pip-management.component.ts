import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NgbModal, NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { PerformanceService } from '../../../services/performance.service';
import { EmployeeService } from '../../../services/employee.service';
import { PIP, PIPStatus, PIPOutcome, CreatePIPDto, ImprovementStatus, MilestoneStatus } from '../../../models/performance.models';
import { Employee } from '../../../models/employee.models';

@Component({
  selector: 'app-pip-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, NgbModule],
  template: `
    <div class="container-fluid">
      <div class="page-header d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 class="h3 mb-0">Performance Improvement Plans</h1>
          <p class="text-muted">Manage employee performance improvement plans</p>
        </div>
        <button class="btn btn-primary" (click)="openCreateModal()">
          <i class="fas fa-plus me-2"></i>Create PIP
        </button>
      </div>

      <!-- Filters -->
      <div class="card mb-4">
        <div class="card-body">
          <div class="row g-3">
            <div class="col-md-4">
              <label class="form-label">Employee</label>
              <select class="form-select" [(ngModel)]="selectedEmployeeId" (change)="loadPIPs()">
                <option value="">All Employees</option>
                <option *ngFor="let employee of employees" [value]="employee.id">
                  {{employee.firstName}} {{employee.lastName}} ({{employee.employeeId}})
                </option>
              </select>
            </div>
            <div class="col-md-4">
              <label class="form-label">Status</label>
              <select class="form-select" [(ngModel)]="selectedStatus" (change)="loadPIPs()">
                <option value="">All Status</option>
                <option value="Active">Active</option>
                <option value="OnTrack">On Track</option>
                <option value="AtRisk">At Risk</option>
                <option value="Completed">Completed</option>
                <option value="Extended">Extended</option>
                <option value="Terminated">Terminated</option>
              </select>
            </div>
            <div class="col-md-4 d-flex align-items-end">
              <button class="btn btn-outline-secondary" (click)="clearFilters()">
                <i class="fas fa-times me-2"></i>Clear Filters
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- PIPs List -->
      <div class="row">
        <div class="col-12" *ngIf="pips.length > 0; else noPIPs">
          <div class="row">
            <div class="col-lg-4 mb-4" *ngFor="let pip of pips">
              <div class="card h-100">
                <div class="card-header d-flex justify-content-between align-items-center">
                  <h6 class="card-title mb-0">{{pip.title}}</h6>
                  <span class="badge" [ngClass]="getStatusBadgeClass(pip.status)">
                    {{pip.status}}
                  </span>
                </div>
                <div class="card-body">
                  <!-- Employee Info -->
                  <div class="d-flex align-items-center mb-3">
                    <img [src]="pip.employee?.profilePhoto || '/assets/images/default-avatar.png'" 
                         class="rounded-circle me-2" width="40" height="40" alt="Profile">
                    <div>
                      <div class="fw-medium">{{pip.employee?.firstName}} {{pip.employee?.lastName}}</div>
                      <small class="text-muted">{{pip.employee?.employeeId}} - {{pip.employee?.designation}}</small>
                    </div>
                  </div>

                  <!-- Progress Overview -->
                  <div class="mb-3">
                    <div class="d-flex justify-content-between align-items-center mb-1">
                      <small class="text-muted">Overall Progress</small>
                      <small class="text-muted">{{getOverallProgress(pip)}}%</small>
                    </div>
                    <div class="progress" style="height: 8px;">
                      <div class="progress-bar" 
                           [style.width.%]="getOverallProgress(pip)"
                           [ngClass]="getProgressBarClass(getOverallProgress(pip))"></div>
                    </div>
                  </div>

                  <!-- Timeline -->
                  <div class="mb-3">
                    <div class="row text-center">
                      <div class="col-6">
                        <small class="text-muted d-block">Start Date</small>
                        <small class="fw-medium">{{pip.startDate | date:'MMM dd, yyyy'}}</small>
                      </div>
                      <div class="col-6">
                        <small class="text-muted d-block">End Date</small>
                        <small class="fw-medium">{{pip.endDate | date:'MMM dd, yyyy'}}</small>
                      </div>
                    </div>
                  </div>

                  <!-- Improvement Areas Summary -->
                  <div class="mb-3">
                    <small class="text-muted d-block mb-2">Improvement Areas</small>
                    <div class="d-flex flex-wrap gap-1">
                      <span *ngFor="let area of pip.improvementAreas?.slice(0, 3)" 
                            class="badge bg-light text-dark">{{area.area}}</span>
                      <span *ngIf="pip.improvementAreas && pip.improvementAreas.length > 3" 
                            class="badge bg-light text-dark">+{{pip.improvementAreas.length - 3}} more</span>
                    </div>
                  </div>

                  <!-- Milestones Summary -->
                  <div class="mb-3">
                    <small class="text-muted d-block mb-2">Milestones</small>
                    <div class="d-flex justify-content-between">
                      <small>{{getCompletedMilestones(pip.milestones)}}/{{pip.milestones?.length || 0}} completed</small>
                      <small class="text-muted">{{getOverdueMilestones(pip.milestones)}} overdue</small>
                    </div>
                  </div>
                </div>
                <div class="card-footer">
                  <div class="btn-group w-100">
                    <button class="btn btn-outline-primary btn-sm" (click)="viewPIP(pip)">
                      <i class="fas fa-eye me-1"></i>View
                    </button>
                    <button class="btn btn-outline-secondary btn-sm" (click)="editPIP(pip)" 
                            *ngIf="pip.status === 'Active' || pip.status === 'OnTrack' || pip.status === 'AtRisk'">
                      <i class="fas fa-edit me-1"></i>Edit
                    </button>
                    <button class="btn btn-outline-success btn-sm" (click)="updateProgress(pip)" 
                            *ngIf="pip.status !== 'Completed' && pip.status !== 'Terminated'">
                      <i class="fas fa-tasks me-1"></i>Update
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <ng-template #noPIPs>
          <div class="col-12">
            <div class="card">
              <div class="card-body text-center py-5">
                <i class="fas fa-clipboard-list text-muted mb-3" style="font-size: 3rem;"></i>
                <h4>No Performance Improvement Plans Found</h4>
                <p class="text-muted">Create your first PIP to help employees improve their performance.</p>
                <button class="btn btn-primary" (click)="openCreateModal()">
                  <i class="fas fa-plus me-2"></i>Create PIP
                </button>
              </div>
            </div>
          </div>
        </ng-template>
      </div>
    </div>

    <!-- Create/Edit PIP Modal -->
    <ng-template #pipModal let-modal>
      <div class="modal-header">
        <h4 class="modal-title">{{isEditMode ? 'Edit' : 'Create'}} Performance Improvement Plan</h4>
        <button type="button" class="btn-close" (click)="modal.dismiss()"></button>
      </div>
      <form [formGroup]="pipForm" (ngSubmit)="savePIP(modal)">
        <div class="modal-body">
          <div class="row g-3">
            <div class="col-md-6">
              <label class="form-label">Employee *</label>
              <select class="form-select" formControlName="employeeId" [disabled]="isEditMode">
                <option value="">Select Employee</option>
                <option *ngFor="let employee of employees" [value]="employee.id">
                  {{employee.firstName}} {{employee.lastName}} ({{employee.employeeId}})
                </option>
              </select>
              <div class="invalid-feedback" *ngIf="pipForm.get('employeeId')?.invalid && pipForm.get('employeeId')?.touched">
                Please select an employee
              </div>
            </div>
            <div class="col-md-6">
              <label class="form-label">Title *</label>
              <input type="text" class="form-control" formControlName="title" 
                     placeholder="PIP Title">
              <div class="invalid-feedback" *ngIf="pipForm.get('title')?.invalid && pipForm.get('title')?.touched">
                Title is required
              </div>
            </div>
            <div class="col-12">
              <label class="form-label">Description *</label>
              <textarea class="form-control" formControlName="description" rows="3"
                        placeholder="Describe the performance issues and improvement plan..."></textarea>
              <div class="invalid-feedback" *ngIf="pipForm.get('description')?.invalid && pipForm.get('description')?.touched">
                Description is required
              </div>
            </div>
            <div class="col-md-6">
              <label class="form-label">Start Date *</label>
              <input type="date" class="form-control" formControlName="startDate">
              <div class="invalid-feedback" *ngIf="pipForm.get('startDate')?.invalid && pipForm.get('startDate')?.touched">
                Start date is required
              </div>
            </div>
            <div class="col-md-6">
              <label class="form-label">End Date *</label>
              <input type="date" class="form-control" formControlName="endDate">
              <div class="invalid-feedback" *ngIf="pipForm.get('endDate')?.invalid && pipForm.get('endDate')?.touched">
                End date is required
              </div>
            </div>
            <div class="col-12">
              <label class="form-label">Support Resources</label>
              <textarea class="form-control" formControlName="supportResources" rows="2"
                        placeholder="Training, mentoring, tools, or other resources provided..."></textarea>
            </div>
          </div>

          <!-- Improvement Areas -->
          <div class="mt-4">
            <div class="d-flex justify-content-between align-items-center mb-3">
              <h5>Improvement Areas</h5>
              <button type="button" class="btn btn-sm btn-outline-primary" (click)="addImprovementArea()">
                <i class="fas fa-plus me-1"></i>Add Area
              </button>
            </div>
            
            <div formArrayName="improvementAreas">
              <div *ngFor="let area of improvementAreasArray.controls; let i = index" 
                   [formGroupName]="i" class="card mb-3">
                <div class="card-body">
                  <div class="d-flex justify-content-between align-items-start mb-2">
                    <h6 class="card-title mb-0">Area {{i + 1}}</h6>
                    <button type="button" class="btn btn-sm btn-outline-danger" 
                            (click)="removeImprovementArea(i)" *ngIf="improvementAreasArray.length > 1">
                      <i class="fas fa-trash"></i>
                    </button>
                  </div>
                  <div class="row g-2">
                    <div class="col-12">
                      <label class="form-label">Area *</label>
                      <input type="text" class="form-control" formControlName="area" 
                             placeholder="e.g., Communication Skills">
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Current State *</label>
                      <textarea class="form-control" formControlName="currentState" rows="2"
                                placeholder="Describe current performance..."></textarea>
                    </div>
                    <div class="col-md-6">
                      <label class="form-label">Expected State *</label>
                      <textarea class="form-control" formControlName="expectedState" rows="2"
                                placeholder="Describe expected improvement..."></textarea>
                    </div>
                    <div class="col-12">
                      <label class="form-label">Action Plan *</label>
                      <textarea class="form-control" formControlName="actionPlan" rows="2"
                                placeholder="Specific actions to achieve improvement..."></textarea>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Milestones -->
          <div class="mt-4">
            <div class="d-flex justify-content-between align-items-center mb-3">
              <h5>Milestones</h5>
              <button type="button" class="btn btn-sm btn-outline-primary" (click)="addMilestone()">
                <i class="fas fa-plus me-1"></i>Add Milestone
              </button>
            </div>
            
            <div formArrayName="milestones">
              <div *ngFor="let milestone of milestonesArray.controls; let i = index" 
                   [formGroupName]="i" class="card mb-3">
                <div class="card-body">
                  <div class="d-flex justify-content-between align-items-start mb-2">
                    <h6 class="card-title mb-0">Milestone {{i + 1}}</h6>
                    <button type="button" class="btn btn-sm btn-outline-danger" 
                            (click)="removeMilestone(i)" *ngIf="milestonesArray.length > 1">
                      <i class="fas fa-trash"></i>
                    </button>
                  </div>
                  <div class="row g-2">
                    <div class="col-md-8">
                      <label class="form-label">Title *</label>
                      <input type="text" class="form-control" formControlName="title" 
                             placeholder="Milestone title">
                    </div>
                    <div class="col-md-4">
                      <label class="form-label">Due Date *</label>
                      <input type="date" class="form-control" formControlName="dueDate">
                    </div>
                    <div class="col-12">
                      <label class="form-label">Description *</label>
                      <textarea class="form-control" formControlName="description" rows="2"
                                placeholder="Describe what needs to be achieved..."></textarea>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" (click)="modal.dismiss()">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="pipForm.invalid || loading">
            <span *ngIf="loading" class="spinner-border spinner-border-sm me-2"></span>
            {{isEditMode ? 'Update' : 'Create'}} PIP
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
  `]
})
export class PIPManagementComponent implements OnInit {
  @ViewChild('pipModal') pipModal!: TemplateRef<any>;
  
  pips: PIP[] = [];
  employees: Employee[] = [];
  selectedEmployeeId: string = '';
  selectedStatus: string = '';
  loading = false;
  
  pipForm: FormGroup;
  isEditMode = false;
  currentPIP: PIP | null = null;

  constructor(
    private performanceService: PerformanceService,
    private employeeService: EmployeeService,
    private modalService: NgbModal,
    private fb: FormBuilder,
    private router: Router
  ) {
    this.pipForm = this.createPIPForm();
  }

  ngOnInit() {
    this.loadEmployees();
    this.loadPIPs();
  }

  createPIPForm(): FormGroup {
    return this.fb.group({
      employeeId: ['', Validators.required],
      title: ['', Validators.required],
      description: ['', Validators.required],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      supportResources: [''],
      improvementAreas: this.fb.array([this.createImprovementAreaForm()]),
      milestones: this.fb.array([this.createMilestoneForm()])
    });
  }

  createImprovementAreaForm(): FormGroup {
    return this.fb.group({
      area: ['', Validators.required],
      currentState: ['', Validators.required],
      expectedState: ['', Validators.required],
      actionPlan: ['', Validators.required]
    });
  }

  createMilestoneForm(): FormGroup {
    return this.fb.group({
      title: ['', Validators.required],
      description: ['', Validators.required],
      dueDate: ['', Validators.required]
    });
  }

  get improvementAreasArray(): FormArray {
    return this.pipForm.get('improvementAreas') as FormArray;
  }

  get milestonesArray(): FormArray {
    return this.pipForm.get('milestones') as FormArray;
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

  loadPIPs() {
    this.loading = true;
    const employeeId = this.selectedEmployeeId ? parseInt(this.selectedEmployeeId) : undefined;
    
    this.performanceService.getPIPs(employeeId, this.selectedStatus).subscribe({
      next: (pips) => {
        this.pips = pips;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading PIPs:', error);
        this.loading = false;
      }
    });
  }

  clearFilters() {
    this.selectedEmployeeId = '';
    this.selectedStatus = '';
    this.loadPIPs();
  }

  openCreateModal() {
    this.isEditMode = false;
    this.currentPIP = null;
    this.pipForm = this.createPIPForm();
    this.modalService.open(this.pipModal, { size: 'xl', backdrop: 'static' });
  }

  editPIP(pip: PIP) {
    this.isEditMode = true;
    this.currentPIP = pip;
    this.populateForm(pip);
    this.modalService.open(this.pipModal, { size: 'xl', backdrop: 'static' });
  }

  populateForm(pip: PIP) {
    // Clear existing arrays
    while (this.improvementAreasArray.length !== 0) {
      this.improvementAreasArray.removeAt(0);
    }
    while (this.milestonesArray.length !== 0) {
      this.milestonesArray.removeAt(0);
    }

    // Add improvement areas
    pip.improvementAreas?.forEach(area => {
      this.improvementAreasArray.push(this.fb.group({
        area: [area.area, Validators.required],
        currentState: [area.currentState, Validators.required],
        expectedState: [area.expectedState, Validators.required],
        actionPlan: [area.actionPlan, Validators.required]
      }));
    });

    // Add milestones
    pip.milestones?.forEach(milestone => {
      this.milestonesArray.push(this.fb.group({
        title: [milestone.title, Validators.required],
        description: [milestone.description, Validators.required],
        dueDate: [new Date(milestone.dueDate).toISOString().split('T')[0], Validators.required]
      }));
    });

    this.pipForm.patchValue({
      employeeId: pip.employeeId,
      title: pip.title,
      description: pip.description,
      startDate: new Date(pip.startDate).toISOString().split('T')[0],
      endDate: new Date(pip.endDate).toISOString().split('T')[0],
      supportResources: pip.supportResources
    });
  }

  addImprovementArea() {
    this.improvementAreasArray.push(this.createImprovementAreaForm());
  }

  removeImprovementArea(index: number) {
    this.improvementAreasArray.removeAt(index);
  }

  addMilestone() {
    this.milestonesArray.push(this.createMilestoneForm());
  }

  removeMilestone(index: number) {
    this.milestonesArray.removeAt(index);
  }

  savePIP(modal: any) {
    if (this.pipForm.valid) {
      this.loading = true;
      const formValue = this.pipForm.value;
      
      const pipData: CreatePIPDto = {
        employeeId: parseInt(formValue.employeeId),
        title: formValue.title,
        description: formValue.description,
        startDate: new Date(formValue.startDate),
        endDate: new Date(formValue.endDate),
        supportResources: formValue.supportResources,
        improvementAreas: formValue.improvementAreas,
        milestones: formValue.milestones.map((m: any) => ({
          ...m,
          dueDate: new Date(m.dueDate)
        }))
      };

      const request = this.isEditMode && this.currentPIP
        ? this.performanceService.updatePIP(this.currentPIP.id, pipData as any)
        : this.performanceService.createPIP(pipData);

      request.subscribe({
        next: (pip) => {
          this.loadPIPs();
          modal.close();
          this.loading = false;
        },
        error: (error) => {
          console.error('Error saving PIP:', error);
          this.loading = false;
        }
      });
    }
  }

  viewPIP(pip: PIP) {
    this.router.navigate(['/performance/pips', pip.id]);
  }

  updateProgress(pip: PIP) {
    this.router.navigate(['/performance/pips', pip.id, 'progress']);
  }

  getStatusBadgeClass(status: PIPStatus): string {
    const classes = {
      'Active': 'bg-primary',
      'OnTrack': 'bg-success',
      'AtRisk': 'bg-warning',
      'Completed': 'bg-success',
      'Extended': 'bg-info',
      'Terminated': 'bg-danger'
    };
    return classes[status] || 'bg-secondary';
  }

  getOverallProgress(pip: PIP): number {
    if (!pip.improvementAreas || pip.improvementAreas.length === 0) return 0;
    const totalProgress = pip.improvementAreas.reduce((sum, area) => sum + area.progress, 0);
    return Math.round(totalProgress / pip.improvementAreas.length);
  }

  getProgressBarClass(progress: number): string {
    if (progress >= 80) return 'bg-success';
    if (progress >= 60) return 'bg-warning';
    return 'bg-danger';
  }

  getCompletedMilestones(milestones: any[]): number {
    if (!milestones) return 0;
    return milestones.filter(m => m.status === MilestoneStatus.Completed).length;
  }

  getOverdueMilestones(milestones: any[]): number {
    if (!milestones) return 0;
    return milestones.filter(m => m.status === MilestoneStatus.Overdue).length;
  }
}