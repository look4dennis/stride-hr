import { Component, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EnhancedEmployeeService } from '../../../services/enhanced-employee.service';
import { RoleService } from '../../../services/role.service';
import { Employee, EmployeeRole, Role, AssignRoleDto, RevokeRoleDto } from '../../../models/employee.models';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-employee-roles',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div class="card">
      <div class="card-header">
        <div class="d-flex justify-content-between align-items-center">
          <h6 class="card-title mb-0">
            <i class="fas fa-user-shield me-2"></i>Role Management
          </h6>
          <button class="btn btn-primary btn-sm" 
                  (click)="showAssignRoleModal = true"
                  [disabled]="loading">
            <i class="fas fa-plus me-1"></i>Assign Role
          </button>
        </div>
      </div>
      
      <div class="card-body">
        <!-- Current Roles -->
        <div class="current-roles" *ngIf="employeeRoles.length > 0; else noRoles">
          <h6 class="mb-3">Current Roles</h6>
          <div class="role-list">
            <div class="role-item" *ngFor="let employeeRole of employeeRoles">
              <div class="role-info">
                <div class="role-header">
                  <h6 class="role-name">{{ employeeRole.roleName }}</h6>
                  <span class="badge" 
                        [class]="employeeRole.isActive ? 'bg-success' : 'bg-secondary'">
                    {{ employeeRole.isActive ? 'Active' : 'Revoked' }}
                  </span>
                </div>
                <p class="role-description">{{ employeeRole.roleDescription }}</p>
                <div class="role-meta">
                  <small class="text-muted">
                    Assigned on {{ formatDate(employeeRole.assignedDate) }} 
                    by {{ employeeRole.assignedByName }}
                    <span *ngIf="employeeRole.revokedDate">
                      • Revoked on {{ formatDate(employeeRole.revokedDate) }}
                      by {{ employeeRole.revokedByName }}
                    </span>
                  </small>
                </div>
                <div class="role-notes" *ngIf="employeeRole.notes">
                  <small><strong>Notes:</strong> {{ employeeRole.notes }}</small>
                </div>
              </div>
              <div class="role-actions" *ngIf="employeeRole.isActive">
                <button class="btn btn-outline-danger btn-sm" 
                        (click)="openRevokeModal(employeeRole)"
                        [disabled]="loading">
                  <i class="fas fa-times me-1"></i>Revoke
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- No Roles Template -->
        <ng-template #noRoles>
          <div class="text-center py-4">
            <i class="fas fa-user-shield text-muted mb-3" style="font-size: 2rem;"></i>
            <h6>No roles assigned</h6>
            <p class="text-muted">This employee has no roles assigned yet.</p>
            <button class="btn btn-primary btn-sm" 
                    (click)="showAssignRoleModal = true">
              <i class="fas fa-plus me-1"></i>Assign First Role
            </button>
          </div>
        </ng-template>
      </div>
    </div>

    <!-- Assign Role Modal -->
    <div class="modal fade" 
         [class.show]="showAssignRoleModal" 
         [style.display]="showAssignRoleModal ? 'block' : 'none'"
         *ngIf="showAssignRoleModal">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Assign Role</h5>
            <button type="button" 
                    class="btn-close" 
                    (click)="closeAssignRoleModal()"></button>
          </div>
          <form [formGroup]="assignRoleForm" (ngSubmit)="assignRole()">
            <div class="modal-body">
              <div class="mb-3">
                <label class="form-label">Role <span class="text-danger">*</span></label>
                <select class="form-select" 
                        formControlName="roleId"
                        [class.is-invalid]="isFieldInvalid('roleId')">
                  <option value="">Select a role</option>
                  <option *ngFor="let role of availableRoles" [value]="role.id">
                    {{ role.name }} - {{ role.description }}
                  </option>
                </select>
                <div class="invalid-feedback" *ngIf="isFieldInvalid('roleId')">
                  Please select a role
                </div>
              </div>
              
              <div class="mb-3">
                <label class="form-label">Notes</label>
                <textarea class="form-control" 
                          formControlName="notes"
                          rows="3"
                          placeholder="Optional notes about this role assignment"></textarea>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" 
                      class="btn btn-secondary" 
                      (click)="closeAssignRoleModal()">
                Cancel
              </button>
              <button type="submit" 
                      class="btn btn-primary"
                      [disabled]="!assignRoleForm.valid || loading">
                <span *ngIf="loading" class="spinner-border spinner-border-sm me-2" role="status"></span>
                <i *ngIf="!loading" class="fas fa-check me-1"></i>
                {{ loading ? 'Assigning...' : 'Assign Role' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Revoke Role Modal -->
    <div class="modal fade" 
         [class.show]="showRevokeRoleModal" 
         [style.display]="showRevokeRoleModal ? 'block' : 'none'"
         *ngIf="showRevokeRoleModal">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Revoke Role</h5>
            <button type="button" 
                    class="btn-close" 
                    (click)="closeRevokeRoleModal()"></button>
          </div>
          <form [formGroup]="revokeRoleForm" (ngSubmit)="revokeRole()">
            <div class="modal-body">
              <div class="alert alert-warning">
                <i class="fas fa-exclamation-triangle me-2"></i>
                Are you sure you want to revoke the <strong>{{ selectedRole?.roleName }}</strong> role 
                from this employee?
              </div>
              
              <div class="mb-3">
                <label class="form-label">Reason for revocation</label>
                <textarea class="form-control" 
                          formControlName="notes"
                          rows="3"
                          placeholder="Please provide a reason for revoking this role"></textarea>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" 
                      class="btn btn-secondary" 
                      (click)="closeRevokeRoleModal()">
                Cancel
              </button>
              <button type="submit" 
                      class="btn btn-danger"
                      [disabled]="loading">
                <span *ngIf="loading" class="spinner-border spinner-border-sm me-2" role="status"></span>
                <i *ngIf="!loading" class="fas fa-times me-1"></i>
                {{ loading ? 'Revoking...' : 'Revoke Role' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Modal Backdrop -->
    <div class="modal-backdrop fade show" 
         *ngIf="showAssignRoleModal || showRevokeRoleModal"
         (click)="closeModals()"></div>
  `,
  styles: [`
    .role-list {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .role-item {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      padding: 1rem;
      border: 1px solid #e9ecef;
      border-radius: 8px;
      background: #f8f9fa;
    }

    .role-info {
      flex: 1;
    }

    .role-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 0.5rem;
    }

    .role-name {
      margin: 0;
      color: #495057;
      font-weight: 600;
    }

    .role-description {
      color: #6c757d;
      margin-bottom: 0.5rem;
      font-size: 0.9rem;
    }

    .role-meta {
      margin-bottom: 0.5rem;
    }

    .role-notes {
      font-style: italic;
      color: #6c757d;
    }

    .role-actions {
      margin-left: 1rem;
    }

    .modal {
      background: rgba(0, 0, 0, 0.5);
    }

    .modal.show {
      display: block !important;
    }

    .modal-backdrop {
      position: fixed;
      top: 0;
      left: 0;
      z-index: 1040;
      width: 100vw;
      height: 100vh;
      background-color: #000;
      opacity: 0.5;
    }

    .modal-dialog {
      position: relative;
      z-index: 1050;
      margin: 1.75rem auto;
    }

    @media (max-width: 768px) {
      .role-item {
        flex-direction: column;
        gap: 1rem;
      }
      
      .role-actions {
        margin-left: 0;
        width: 100%;
      }
      
      .role-actions .btn {
        width: 100%;
      }
    }
  `]
})
export class EmployeeRolesComponent implements OnInit {
  @Input() employee!: Employee;
  
  employeeRoles: EmployeeRole[] = [];
  availableRoles: Role[] = [];
  selectedRole: EmployeeRole | null = null;
  
  assignRoleForm: FormGroup;
  revokeRoleForm: FormGroup;
  
  showAssignRoleModal = false;
  showRevokeRoleModal = false;
  loading = false;

  constructor(
    private employeeService: EnhancedEmployeeService,
    private roleService: RoleService,
    private fb: FormBuilder,
    private notificationService: NotificationService
  ) {
    this.assignRoleForm = this.fb.group({
      roleId: ['', Validators.required],
      notes: ['']
    });

    this.revokeRoleForm = this.fb.group({
      notes: ['']
    });
  }

  ngOnInit(): void {
    if (this.employee) {
      this.loadEmployeeRoles();
      this.loadAvailableRoles();
    }
  }

  loadEmployeeRoles(): void {
    this.loading = true;
    
    this.employeeService.getEmployeeRoles(this.employee.id).subscribe({
      next: (roles) => {
        this.employeeRoles = roles;
        this.loading = false;
      },
      error: (error) => {
        console.error('Failed to load employee roles:', error);
        this.employeeRoles = []; // Use empty array as fallback
        this.loading = false;
      }
    });
  }

  loadAvailableRoles(): void {
    this.roleService.getAllRoles().subscribe({
      next: (roles) => {
        // Filter out roles that are already assigned and active
        const activeRoleIds = this.employeeRoles
          .filter(er => er.isActive)
          .map(er => er.roleId);
        
        this.availableRoles = roles.filter(role => !activeRoleIds.includes(role.id));
      },
      error: (error) => {
        console.error('Failed to load available roles:', error);
        this.availableRoles = [];
      }
    });
  }

  assignRole(): void {
    if (!this.assignRoleForm.valid) {
      this.markFormGroupTouched(this.assignRoleForm);
      return;
    }

    this.loading = true;
    const formValue = this.assignRoleForm.value;
    
    const assignDto: AssignRoleDto = {
      employeeId: this.employee.id,
      roleId: parseInt(formValue.roleId),
      notes: formValue.notes || undefined
    };

    this.employeeService.assignRole(assignDto.employeeId, assignDto.roleId, assignDto.notes).subscribe({
      next: (success) => {
        if (success) {
          this.notificationService.showSuccess('Role assigned successfully');
          this.loadEmployeeRoles();
          this.loadAvailableRoles();
          this.closeAssignRoleModal();
        } else {
          this.notificationService.showError('Failed to assign role');
        }
        this.loading = false;
      },
      error: (error) => {
        this.notificationService.showError('Failed to assign role');
        this.loading = false;
      }
    });
  }

  openRevokeModal(employeeRole: EmployeeRole): void {
    this.selectedRole = employeeRole;
    this.showRevokeRoleModal = true;
    this.revokeRoleForm.reset();
  }

  revokeRole(): void {
    if (!this.selectedRole) return;

    this.loading = true;
    const formValue = this.revokeRoleForm.value;
    
    const revokeDto: RevokeRoleDto = {
      employeeId: this.employee.id,
      roleId: this.selectedRole.roleId,
      notes: formValue.notes || undefined
    };

    this.employeeService.revokeRole(revokeDto.employeeId, revokeDto.roleId, revokeDto.notes).subscribe({
      next: (success) => {
        if (success) {
          this.notificationService.showSuccess('Role revoked successfully');
          this.loadEmployeeRoles();
          this.loadAvailableRoles();
          this.closeRevokeRoleModal();
        } else {
          this.notificationService.showError('Failed to revoke role');
        }
        this.loading = false;
      },
      error: (error) => {
        this.notificationService.showError('Failed to revoke role');
        this.loading = false;
      }
    });
  }

  closeAssignRoleModal(): void {
    this.showAssignRoleModal = false;
    this.assignRoleForm.reset();
  }

  closeRevokeRoleModal(): void {
    this.showRevokeRoleModal = false;
    this.selectedRole = null;
    this.revokeRoleForm.reset();
  }

  closeModals(): void {
    this.closeAssignRoleModal();
    this.closeRevokeRoleModal();
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.assignRoleForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      if (control) {
        control.markAsTouched();
      }
    });
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }
}