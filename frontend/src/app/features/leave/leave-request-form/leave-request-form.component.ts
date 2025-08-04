import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbModule, NgbCalendar, NgbDate, NgbDateStruct } from '@ng-bootstrap/ng-bootstrap';
import { LeaveService } from '../../../services/leave.service';
import { 
  CreateLeaveRequest, 
  LeaveRequest, 
  LeavePolicy, 
  LeaveBalance,
  LeaveConflict 
} from '../../../models/leave.models';
import { Observable, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, catchError } from 'rxjs/operators';

@Component({
  selector: 'app-leave-request-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NgbModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h5 class="card-title mb-0">
          <i class="fas fa-calendar-plus me-2"></i>
          {{ isEditMode ? 'Edit Leave Request' : 'New Leave Request' }}
        </h5>
      </div>
      <div class="card-body">
        <form [formGroup]="leaveForm" (ngSubmit)="onSubmit()">
          <!-- Leave Type Selection -->
          <div class="row mb-3">
            <div class="col-md-6">
              <label for="leavePolicyId" class="form-label">Leave Type *</label>
              <select 
                class="form-select" 
                id="leavePolicyId" 
                formControlName="leavePolicyId"
                [class.is-invalid]="isFieldInvalid('leavePolicyId')">
                <option value="">Select Leave Type</option>
                <option 
                  *ngFor="let policy of leavePolicies" 
                  [value]="policy.id">
                  {{ policy.name }} ({{ getAvailableBalance(policy.id) }} days available)
                </option>
              </select>
              <div class="invalid-feedback" *ngIf="isFieldInvalid('leavePolicyId')">
                Please select a leave type
              </div>
            </div>
            <div class="col-md-6">
              <label class="form-label">Emergency Leave</label>
              <div class="form-check">
                <input 
                  class="form-check-input" 
                  type="checkbox" 
                  id="isEmergency" 
                  formControlName="isEmergency">
                <label class="form-check-label" for="isEmergency">
                  This is an emergency leave request
                </label>
              </div>
              <small class="text-muted">Emergency leaves may bypass advance notice requirements</small>
            </div>
          </div>

          <!-- Date Selection -->
          <div class="row mb-3">
            <div class="col-md-6">
              <label for="startDate" class="form-label">Start Date *</label>
              <div class="input-group">
                <input 
                  class="form-control" 
                  placeholder="yyyy-mm-dd"
                  name="startDate" 
                  formControlName="startDate"
                  ngbDatepicker 
                  #startDatePicker="ngbDatepicker"
                  [class.is-invalid]="isFieldInvalid('startDate')"
                  readonly>
                <button 
                  class="btn btn-outline-secondary" 
                  type="button" 
                  (click)="startDatePicker.toggle()">
                  <i class="fas fa-calendar-alt"></i>
                </button>
              </div>
              <div class="invalid-feedback" *ngIf="isFieldInvalid('startDate')">
                Please select a start date
              </div>
            </div>
            <div class="col-md-6">
              <label for="endDate" class="form-label">End Date *</label>
              <div class="input-group">
                <input 
                  class="form-control" 
                  placeholder="yyyy-mm-dd"
                  name="endDate" 
                  formControlName="endDate"
                  ngbDatepicker 
                  #endDatePicker="ngbDatepicker"
                  [class.is-invalid]="isFieldInvalid('endDate')"
                  readonly>
                <button 
                  class="btn btn-outline-secondary" 
                  type="button" 
                  (click)="endDatePicker.toggle()">
                  <i class="fas fa-calendar-alt"></i>
                </button>
              </div>
              <div class="invalid-feedback" *ngIf="isFieldInvalid('endDate')">
                Please select an end date
              </div>
            </div>
          </div>

          <!-- Calculated Days and Conflicts -->
          <div class="row mb-3" *ngIf="calculatedDays > 0 || conflicts.length > 0">
            <div class="col-md-6" *ngIf="calculatedDays > 0">
              <div class="alert alert-info">
                <i class="fas fa-info-circle me-2"></i>
                <strong>Total Leave Days:</strong> {{ calculatedDays }}
                <div class="mt-1" *ngIf="selectedPolicy">
                  <small>
                    Available Balance: {{ getAvailableBalance(selectedPolicy.id) }} days
                  </small>
                </div>
              </div>
            </div>
            <div class="col-md-6" *ngIf="conflicts.length > 0">
              <div class="alert alert-warning">
                <i class="fas fa-exclamation-triangle me-2"></i>
                <strong>Potential Conflicts:</strong>
                <ul class="mb-0 mt-1">
                  <li *ngFor="let conflict of conflicts">
                    {{ conflict.employeeName }} ({{ conflict.department }}) - {{ conflict.conflictReason }}
                  </li>
                </ul>
              </div>
            </div>
          </div>

          <!-- Reason -->
          <div class="mb-3">
            <label for="reason" class="form-label">Reason *</label>
            <textarea 
              class="form-control" 
              id="reason" 
              rows="3" 
              formControlName="reason"
              placeholder="Please provide a reason for your leave request"
              [class.is-invalid]="isFieldInvalid('reason')"></textarea>
            <div class="invalid-feedback" *ngIf="isFieldInvalid('reason')">
              Please provide a reason (minimum 10 characters)
            </div>
          </div>

          <!-- Comments -->
          <div class="mb-3">
            <label for="comments" class="form-label">Additional Comments</label>
            <textarea 
              class="form-control" 
              id="comments" 
              rows="2" 
              formControlName="comments"
              placeholder="Any additional information (optional)"></textarea>
          </div>

          <!-- File Attachment -->
          <div class="mb-3">
            <label for="attachment" class="form-label">Supporting Document</label>
            <input 
              type="file" 
              class="form-control" 
              id="attachment" 
              accept=".pdf,.doc,.docx,.jpg,.jpeg,.png"
              (change)="onFileSelected($event)">
            <small class="text-muted">
              Accepted formats: PDF, DOC, DOCX, JPG, PNG (Max 5MB)
            </small>
          </div>

          <!-- Form Actions -->
          <div class="d-flex justify-content-between">
            <button 
              type="button" 
              class="btn btn-secondary" 
              (click)="onCancel()">
              <i class="fas fa-times me-2"></i>Cancel
            </button>
            <div>
              <button 
                type="button" 
                class="btn btn-outline-primary me-2" 
                (click)="onSaveDraft()"
                [disabled]="isSubmitting">
                <i class="fas fa-save me-2"></i>Save Draft
              </button>
              <button 
                type="submit" 
                class="btn btn-primary" 
                [disabled]="leaveForm.invalid || isSubmitting">
                <span *ngIf="isSubmitting" class="spinner-border spinner-border-sm me-2"></span>
                <i *ngIf="!isSubmitting" class="fas fa-paper-plane me-2"></i>
                {{ isEditMode ? 'Update Request' : 'Submit Request' }}
              </button>
            </div>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .card {
      border-radius: 12px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .card-header {
      background: linear-gradient(135deg, var(--bs-primary) 0%, #0056b3 100%);
      color: white;
      border-radius: 12px 12px 0 0;
    }

    .form-label {
      font-weight: 500;
      color: var(--bs-gray-700);
    }

    .form-control, .form-select {
      border-radius: 8px;
      border: 2px solid var(--bs-gray-200);
      transition: all 0.15s ease-in-out;
    }

    .form-control:focus, .form-select:focus {
      border-color: var(--bs-primary);
      box-shadow: 0 0 0 0.2rem rgba(13, 110, 253, 0.25);
    }

    .btn {
      border-radius: 8px;
      font-weight: 500;
      padding: 0.625rem 1.25rem;
    }

    .alert {
      border-radius: 8px;
      border: none;
    }

    .alert-info {
      background-color: #e7f3ff;
      color: #0c5460;
    }

    .alert-warning {
      background-color: #fff3cd;
      color: #856404;
    }

    .form-check-input:checked {
      background-color: var(--bs-primary);
      border-color: var(--bs-primary);
    }

    .input-group .btn {
      border-color: var(--bs-gray-200);
    }

    .spinner-border-sm {
      width: 1rem;
      height: 1rem;
    }
  `]
})
export class LeaveRequestFormComponent implements OnInit {
  @Input() leaveRequest?: LeaveRequest;
  @Input() isEditMode = false;
  @Output() formSubmit = new EventEmitter<CreateLeaveRequest>();
  @Output() formCancel = new EventEmitter<void>();

  leaveForm: FormGroup;
  leavePolicies: LeavePolicy[] = [];
  leaveBalances: LeaveBalance[] = [];
  conflicts: LeaveConflict[] = [];
  calculatedDays = 0;
  selectedPolicy?: LeavePolicy;
  isSubmitting = false;
  selectedFile?: File;

  constructor(
    private fb: FormBuilder,
    private leaveService: LeaveService,
    private calendar: NgbCalendar
  ) {
    this.leaveForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadLeavePolicies();
    this.loadLeaveBalances();
    this.setupFormSubscriptions();
    
    if (this.isEditMode && this.leaveRequest) {
      this.populateForm();
    }
  }

  private createForm(): FormGroup {
    return this.fb.group({
      leavePolicyId: ['', Validators.required],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      reason: ['', [Validators.required, Validators.minLength(10)]],
      comments: [''],
      isEmergency: [false]
    });
  }

  private setupFormSubscriptions(): void {
    // Watch for leave policy changes
    this.leaveForm.get('leavePolicyId')?.valueChanges.subscribe(policyId => {
      this.selectedPolicy = this.leavePolicies.find(p => p.id === parseInt(policyId));
      this.calculateDaysAndConflicts();
    });

    // Watch for date changes
    this.leaveForm.get('startDate')?.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(() => this.calculateDaysAndConflicts());

    this.leaveForm.get('endDate')?.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(() => this.calculateDaysAndConflicts());
  }

  private loadLeavePolicies(): void {
    this.leaveService.getLeavePolicies().subscribe({
      next: (policies) => {
        this.leavePolicies = policies.filter(p => p.isActive);
      },
      error: (error) => {
        console.error('Error loading leave policies:', error);
      }
    });
  }

  private loadLeaveBalances(): void {
    this.leaveService.getMyLeaveBalances().subscribe({
      next: (balances) => {
        this.leaveBalances = balances;
      },
      error: (error) => {
        console.error('Error loading leave balances:', error);
      }
    });
  }

  private populateForm(): void {
    if (!this.leaveRequest) return;

    this.leaveForm.patchValue({
      leavePolicyId: this.leaveRequest.leavePolicyId,
      startDate: this.convertDateToNgbDate(this.leaveRequest.startDate),
      endDate: this.convertDateToNgbDate(this.leaveRequest.endDate),
      reason: this.leaveRequest.reason,
      comments: this.leaveRequest.comments,
      isEmergency: this.leaveRequest.isEmergency
    });
  }

  private calculateDaysAndConflicts(): void {
    const startDate = this.convertNgbDateToDate(this.leaveForm.get('startDate')?.value);
    const endDate = this.convertNgbDateToDate(this.leaveForm.get('endDate')?.value);

    if (startDate && endDate && startDate <= endDate) {
      // Calculate leave days
      this.leaveService.calculateLeaveDays(startDate, endDate).subscribe({
        next: (days) => {
          this.calculatedDays = days;
        },
        error: (error) => {
          console.error('Error calculating leave days:', error);
          this.calculatedDays = 0;
        }
      });

      // Check for conflicts
      this.leaveService.detectLeaveConflicts(startDate, endDate).subscribe({
        next: (conflicts) => {
          this.conflicts = conflicts;
        },
        error: (error) => {
          console.error('Error detecting conflicts:', error);
          this.conflicts = [];
        }
      });
    } else {
      this.calculatedDays = 0;
      this.conflicts = [];
    }
  }

  getAvailableBalance(policyId: number): number {
    const balance = this.leaveBalances.find(b => b.leavePolicyId === policyId);
    return balance ? balance.remainingDays : 0;
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      // Validate file size (5MB max)
      if (file.size > 5 * 1024 * 1024) {
        alert('File size must be less than 5MB');
        return;
      }

      // Validate file type
      const allowedTypes = ['application/pdf', 'application/msword', 
                           'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
                           'image/jpeg', 'image/jpg', 'image/png'];
      
      if (!allowedTypes.includes(file.type)) {
        alert('Please select a valid file type (PDF, DOC, DOCX, JPG, PNG)');
        return;
      }

      this.selectedFile = file;
    }
  }

  onSubmit(): void {
    if (this.leaveForm.valid) {
      this.isSubmitting = true;
      
      const formValue = this.leaveForm.value;
      const request: CreateLeaveRequest = {
        leavePolicyId: parseInt(formValue.leavePolicyId),
        startDate: this.convertNgbDateToDate(formValue.startDate)!,
        endDate: this.convertNgbDateToDate(formValue.endDate)!,
        reason: formValue.reason,
        comments: formValue.comments,
        isEmergency: formValue.isEmergency,
        attachmentPath: undefined // Will be handled by file upload service
      };

      this.formSubmit.emit(request);
    }
  }

  onSaveDraft(): void {
    // Implement save draft functionality
    console.log('Save draft functionality to be implemented');
  }

  onCancel(): void {
    this.formCancel.emit();
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.leaveForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  private convertDateToNgbDate(date: Date): NgbDateStruct {
    return {
      year: date.getFullYear(),
      month: date.getMonth() + 1,
      day: date.getDate()
    };
  }

  private convertNgbDateToDate(ngbDate: NgbDateStruct): Date | null {
    if (!ngbDate) return null;
    return new Date(ngbDate.year, ngbDate.month - 1, ngbDate.day);
  }
}