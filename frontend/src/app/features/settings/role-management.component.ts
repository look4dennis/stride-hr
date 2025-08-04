import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { RoleService } from '../../services/role.service';
import { Role, Permission, CreateRoleDto, UpdateRoleDto } from '../../models/admin.models';

@Component({
  selector: 'app-role-management',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="role-management-container">
      <!-- Header -->
      <div class="page-header">
        <div class="d-flex justify-content-between align-items-center">
          <div>
            <h1>Role & Permission Management</h1>
            <p class="text-muted">Configure user roles and access permissions</p>
          </div>
          <div class="d-flex gap-2">
            <button class="btn btn-primary" (click)="showCreateModal()">
              <i class="fas fa-plus me-2"></i>Create Role
            </button>
            <button class="btn btn-outline-secondary" routerLink="/settings">
              <i class="fas fa-arrow-left me-2"></i>Back to Settings
            </button>
          </div>
        </div>
      </div>

      <!-- Roles List -->
      <div class="card">
        <div class="card-header">
          <div class="d-flex justify-content-between align-items-center">
            <h5 class="card-title mb-0">System Roles</h5>
            <div class="search-box">
              <input 
                type="text" 
                class="form-control form-control-sm" 
                placeholder="Search roles..."
                [(ngModel)]="searchTerm"
                (input)="filterRoles()">
            </div>
          </div>
        </div>
        <div class="card-body">
          <div class="table-responsive">
            <table class="table table-hover">
              <thead>
                <tr>
                  <th>Role Name</th>
                  <th>Description</th>
                  <th>Hierarchy Level</th>
                  <th>Permissions</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let role of filteredRoles">
                  <td>
                    <div class="d-flex align-items-center">
                      <div class="role-icon me-2">
                        <i class="fas fa-user-tag text-primary"></i>
                      </div>
                      <div>
                        <div class="fw-semibold">{{ role.name }}</div>
                        <small class="text-muted">Level {{ role.hierarchyLevel }}</small>
                      </div>
                    </div>
                  </td>
                  <td>{{ role.description }}</td>
                  <td>
                    <span class="badge" [class]="getHierarchyBadgeClass(role.hierarchyLevel)">
                      {{ getHierarchyLabel(role.hierarchyLevel) }}
                    </span>
                  </td>
                  <td>
                    <span class="badge bg-info">{{ getRolePermissionCount(role) }} permissions</span>
                  </td>
                  <td>
                    <span class="badge" [class]="role.isActive ? 'bg-success' : 'bg-secondary'">
                      {{ role.isActive ? 'Active' : 'Inactive' }}
                    </span>
                  </td>
                  <td>
                    <div class="btn-group btn-group-sm">
                      <button class="btn btn-outline-primary" (click)="editRole(role)">
                        <i class="fas fa-edit"></i>
                      </button>
                      <button class="btn btn-outline-info" (click)="viewRoleDetails(role)">
                        <i class="fas fa-eye"></i>
                      </button>
                      <button class="btn btn-outline-warning" (click)="managePermissions(role)">
                        <i class="fas fa-key"></i>
                      </button>
                      <button class="btn btn-outline-danger" (click)="deleteRole(role)" [disabled]="!role.isActive">
                        <i class="fas fa-trash"></i>
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Create/Edit Role Modal -->
      <div class="modal fade" [class.show]="showModal" [style.display]="showModal ? 'block' : 'none'" tabindex="-1">
        <div class="modal-dialog modal-lg">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">{{ isEditMode ? 'Edit Role' : 'Create New Role' }}</h5>
              <button type="button" class="btn-close" (click)="closeModal()"></button>
            </div>
            <div class="modal-body">
              <form [formGroup]="roleForm" (ngSubmit)="saveRole()">
                <div class="row g-3">
                  <!-- Role Name -->
                  <div class="col-md-6">
                    <label class="form-label">Role Name *</label>
                    <input 
                      type="text" 
                      class="form-control"
                      formControlName="name"
                      [class.is-invalid]="roleForm.get('name')?.invalid && roleForm.get('name')?.touched">
                    <div class="invalid-feedback" *ngIf="roleForm.get('name')?.invalid && roleForm.get('name')?.touched">
                      Role name is required
                    </div>
                  </div>

                  <!-- Hierarchy Level -->
                  <div class="col-md-6">
                    <label class="form-label">Hierarchy Level *</label>
                    <select 
                      class="form-select"
                      formControlName="hierarchyLevel"
                      [class.is-invalid]="roleForm.get('hierarchyLevel')?.invalid && roleForm.get('hierarchyLevel')?.touched">
                      <option value="">Select Level</option>
                      <option *ngFor="let level of hierarchyLevels" [value]="level.value">{{ level.label }}</option>
                    </select>
                    <div class="invalid-feedback" *ngIf="roleForm.get('hierarchyLevel')?.invalid && roleForm.get('hierarchyLevel')?.touched">
                      Hierarchy level is required
                    </div>
                  </div>

                  <!-- Description -->
                  <div class="col-12">
                    <label class="form-label">Description *</label>
                    <textarea 
                      class="form-control" 
                      rows="3"
                      formControlName="description"
                      [class.is-invalid]="roleForm.get('description')?.invalid && roleForm.get('description')?.touched"></textarea>
                    <div class="invalid-feedback" *ngIf="roleForm.get('description')?.invalid && roleForm.get('description')?.touched">
                      Description is required
                    </div>
                  </div>

                  <!-- Permissions -->
                  <div class="col-12">
                    <label class="form-label">Permissions *</label>
                    <div class="permissions-container">
                      <div *ngFor="let module of permissionModules" class="permission-module mb-3">
                        <div class="module-header">
                          <div class="form-check">
                            <input 
                              class="form-check-input" 
                              type="checkbox" 
                              [id]="'module-' + module.name"
                              [checked]="isModuleSelected(module.name)"
                              (change)="toggleModule(module.name, $event)">
                            <label class="form-check-label fw-semibold" [for]="'module-' + module.name">
                              {{ module.name }} Module
                            </label>
                          </div>
                        </div>
                        <div class="module-permissions ms-4">
                          <div class="row">
                            <div class="col-md-6" *ngFor="let permission of module.permissions">
                              <div class="form-check">
                                <input 
                                  class="form-check-input" 
                                  type="checkbox" 
                                  [id]="'perm-' + permission.id"
                                  [checked]="selectedPermissions.includes(permission.id)"
                                  (change)="togglePermission(permission.id, $event)">
                                <label class="form-check-label" [for]="'perm-' + permission.id">
                                  {{ permission.name }}
                                  <small class="text-muted d-block">{{ permission.description }}</small>
                                </label>
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="text-danger" *ngIf="selectedPermissions.length === 0 && roleForm.touched">
                      At least one permission must be selected
                    </div>
                  </div>
                </div>
              </form>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-outline-secondary" (click)="closeModal()">Cancel</button>
              <button type="button" class="btn btn-primary" (click)="saveRole()" [disabled]="roleForm.invalid || selectedPermissions.length === 0 || isLoading">
                <span *ngIf="isLoading" class="spinner-border spinner-border-sm me-2"></span>
                {{ isEditMode ? 'Update Role' : 'Create Role' }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Role Details Modal -->
      <div class="modal fade" [class.show]="showDetailsModal" [style.display]="showDetailsModal ? 'block' : 'none'" tabindex="-1">
        <div class="modal-dialog modal-lg">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Role Details: {{ selectedRole?.name }}</h5>
              <button type="button" class="btn-close" (click)="closeDetailsModal()"></button>
            </div>
            <div class="modal-body" *ngIf="selectedRole">
              <div class="row g-3">
                <div class="col-md-6">
                  <strong>Role Name:</strong>
                  <p>{{ selectedRole.name }}</p>
                </div>
                <div class="col-md-6">
                  <strong>Hierarchy Level:</strong>
                  <p>{{ getHierarchyLabel(selectedRole.hierarchyLevel) }}</p>
                </div>
                <div class="col-12">
                  <strong>Description:</strong>
                  <p>{{ selectedRole.description }}</p>
                </div>
                <div class="col-12">
                  <strong>Permissions:</strong>
                  <div class="permissions-list">
                    <span class="badge bg-primary me-2 mb-2" *ngFor="let permission of selectedRole.permissions">
                      {{ permission.name }}
                    </span>
                  </div>
                </div>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" (click)="closeDetailsModal()">Close</button>
            </div>
          </div>
        </div>
      </div>

      <!-- Modal Backdrop -->
      <div class="modal-backdrop fade" [class.show]="showModal || showDetailsModal" *ngIf="showModal || showDetailsModal"></div>
    </div>
  `,
  styles: [`
    .role-management-container {
      padding: 2rem;
    }

    .page-header {
      margin-bottom: 2rem;
    }

    .page-header h1 {
      font-size: 2rem;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    .card {
      border: 1px solid var(--gray-200);
      border-radius: 12px;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    }

    .card-header {
      background: linear-gradient(135deg, var(--bg-secondary) 0%, var(--bg-tertiary) 100%);
      border-bottom: 1px solid var(--gray-200);
      padding: 1.25rem 1.5rem;
      border-radius: 12px 12px 0 0;
    }

    .card-title {
      font-weight: 600;
      color: var(--text-primary);
    }

    .search-box {
      width: 250px;
    }

    .table th {
      font-weight: 600;
      color: var(--text-primary);
      background-color: var(--bg-secondary);
      border-bottom: 2px solid var(--gray-200);
    }

    .table-hover tbody tr:hover {
      background-color: var(--bg-tertiary);
    }

    .role-icon {
      width: 32px;
      height: 32px;
      display: flex;
      align-items: center;
      justify-content: center;
      background-color: var(--primary-light);
      border-radius: 8px;
    }

    .modal.show {
      background-color: rgba(0, 0, 0, 0.5);
    }

    .modal-content {
      border-radius: 12px;
      border: none;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
    }

    .modal-header {
      border-bottom: 1px solid var(--gray-200);
      padding: 1.5rem;
    }

    .modal-title {
      font-weight: 600;
      color: var(--text-primary);
    }

    .form-label {
      font-weight: 500;
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    .form-control, .form-select {
      border: 2px solid var(--gray-200);
      border-radius: 8px;
      padding: 0.75rem 1rem;
      transition: all 0.15s ease-in-out;
    }

    .form-control:focus, .form-select:focus {
      border-color: var(--primary);
      box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
    }

    .btn {
      border-radius: 8px;
      font-weight: 500;
      padding: 0.625rem 1.25rem;
      transition: all 0.15s ease-in-out;
    }

    .btn-primary {
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      border: none;
      color: white;
    }

    .btn-primary:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 8px rgba(59, 130, 246, 0.4);
    }

    .permissions-container {
      max-height: 400px;
      overflow-y: auto;
      border: 1px solid var(--gray-200);
      border-radius: 8px;
      padding: 1rem;
    }

    .permission-module {
      border-bottom: 1px solid var(--gray-100);
      padding-bottom: 1rem;
    }

    .permission-module:last-child {
      border-bottom: none;
    }

    .module-header {
      margin-bottom: 0.75rem;
    }

    .module-permissions {
      background-color: var(--bg-tertiary);
      padding: 0.75rem;
      border-radius: 6px;
    }

    .badge {
      font-size: 0.75rem;
      padding: 0.375rem 0.75rem;
    }

    .permissions-list .badge {
      font-size: 0.8rem;
    }
  `]
})
export class RoleManagementComponent implements OnInit {
  roles: Role[] = [];
  filteredRoles: Role[] = [];
  permissions: Permission[] = [];
  permissionModules: { name: string; permissions: Permission[] }[] = [];
  roleForm: FormGroup;
  showModal = false;
  showDetailsModal = false;
  isEditMode = false;
  isLoading = false;
  searchTerm = '';
  currentRole: Role | null = null;
  selectedRole: Role | null = null;
  selectedPermissions: number[] = [];
  hierarchyLevels: { value: number; label: string }[] = [];

  constructor(
    private fb: FormBuilder,
    private roleService: RoleService
  ) {
    this.roleForm = this.createForm();
  }

  ngOnInit(): void {
    this.loadRoles();
    this.loadPermissions();
    this.loadHierarchyLevels();
  }

  private createForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required]],
      description: ['', [Validators.required]],
      hierarchyLevel: ['', [Validators.required]]
    });
  }

  private loadRoles(): void {
    this.roleService.getAllRoles().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.roles = response.data.roles;
          this.filteredRoles = [...this.roles];
        }
      },
      error: (error) => {
        console.error('Error loading roles:', error);
      }
    });
  }

  private loadPermissions(): void {
    this.roleService.getAllPermissions().subscribe({
      next: (permissions) => {
        this.permissions = permissions;
        this.groupPermissionsByModule();
      },
      error: (error) => {
        console.error('Error loading permissions:', error);
      }
    });
  }

  private loadHierarchyLevels(): void {
    this.hierarchyLevels = this.roleService.getHierarchyLevels();
  }

  private groupPermissionsByModule(): void {
    const modules = [...new Set(this.permissions.map(p => p.module))];
    this.permissionModules = modules.map(module => ({
      name: module,
      permissions: this.permissions.filter(p => p.module === module)
    }));
  }

  filterRoles(): void {
    if (!this.searchTerm.trim()) {
      this.filteredRoles = [...this.roles];
    } else {
      const term = this.searchTerm.toLowerCase();
      this.filteredRoles = this.roles.filter(role =>
        role.name.toLowerCase().includes(term) ||
        role.description.toLowerCase().includes(term)
      );
    }
  }

  showCreateModal(): void {
    this.isEditMode = false;
    this.currentRole = null;
    this.roleForm.reset();
    this.selectedPermissions = [];
    this.showModal = true;
  }

  editRole(role: Role): void {
    this.isEditMode = true;
    this.currentRole = role;
    this.roleForm.patchValue({
      name: role.name,
      description: role.description,
      hierarchyLevel: role.hierarchyLevel
    });
    this.selectedPermissions = role.permissions?.map(p => p.id) || [];
    this.showModal = true;
  }

  viewRoleDetails(role: Role): void {
    // Load full role details with permissions
    this.roleService.getRole(role.id).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.selectedRole = response.data.role;
          this.showDetailsModal = true;
        }
      },
      error: (error) => {
        console.error('Error loading role details:', error);
      }
    });
  }

  managePermissions(role: Role): void {
    // This could open a dedicated permissions management modal
    this.editRole(role);
  }

  deleteRole(role: Role): void {
    if (confirm(`Are you sure you want to delete the role "${role.name}"?`)) {
      this.roleService.deleteRole(role.id).subscribe({
        next: (response) => {
          if (response.success) {
            console.log('Role deleted successfully');
            this.loadRoles();
          }
        },
        error: (error) => {
          console.error('Error deleting role:', error);
        }
      });
    }
  }

  toggleModule(moduleName: string, event: any): void {
    const modulePermissions = this.permissionModules.find(m => m.name === moduleName)?.permissions || [];
    
    if (event.target.checked) {
      // Add all module permissions
      modulePermissions.forEach(permission => {
        if (!this.selectedPermissions.includes(permission.id)) {
          this.selectedPermissions.push(permission.id);
        }
      });
    } else {
      // Remove all module permissions
      modulePermissions.forEach(permission => {
        const index = this.selectedPermissions.indexOf(permission.id);
        if (index > -1) {
          this.selectedPermissions.splice(index, 1);
        }
      });
    }
  }

  togglePermission(permissionId: number, event: any): void {
    if (event.target.checked) {
      if (!this.selectedPermissions.includes(permissionId)) {
        this.selectedPermissions.push(permissionId);
      }
    } else {
      const index = this.selectedPermissions.indexOf(permissionId);
      if (index > -1) {
        this.selectedPermissions.splice(index, 1);
      }
    }
  }

  isModuleSelected(moduleName: string): boolean {
    const modulePermissions = this.permissionModules.find(m => m.name === moduleName)?.permissions || [];
    return modulePermissions.length > 0 && modulePermissions.every(p => this.selectedPermissions.includes(p.id));
  }

  saveRole(): void {
    if (this.roleForm.invalid || this.selectedPermissions.length === 0) {
      this.roleForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const formData = {
      ...this.roleForm.value,
      permissionIds: this.selectedPermissions
    };

    if (this.isEditMode && this.currentRole) {
      // Update existing role
      this.roleService.updateRole(this.currentRole.id, formData).subscribe({
        next: (response) => {
          if (response.success) {
            console.log('Role updated successfully');
            this.loadRoles();
            this.closeModal();
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error updating role:', error);
          this.isLoading = false;
        }
      });
    } else {
      // Create new role
      this.roleService.createRole(formData).subscribe({
        next: (response) => {
          if (response.success) {
            console.log('Role created successfully');
            this.loadRoles();
            this.closeModal();
          }
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error creating role:', error);
          this.isLoading = false;
        }
      });
    }
  }

  closeModal(): void {
    this.showModal = false;
    this.isEditMode = false;
    this.currentRole = null;
    this.roleForm.reset();
    this.selectedPermissions = [];
  }

  closeDetailsModal(): void {
    this.showDetailsModal = false;
    this.selectedRole = null;
  }

  getHierarchyLabel(level: number): string {
    const hierarchyLevel = this.hierarchyLevels.find(h => h.value === level);
    return hierarchyLevel ? hierarchyLevel.label : `Level ${level}`;
  }

  getHierarchyBadgeClass(level: number): string {
    if (level <= 2) return 'bg-danger';
    if (level <= 4) return 'bg-warning';
    if (level <= 6) return 'bg-info';
    return 'bg-secondary';
  }

  getRolePermissionCount(role: Role): number {
    return role.permissions?.length || 0;
  }
}