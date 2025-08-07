import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, forkJoin, of } from 'rxjs';
import { map, catchError, tap, switchMap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse } from './base-api.service';
import { NotificationService } from './notification.service';
import { LoadingService } from './loading.service';

export interface SetupStatus {
  isSetupComplete: boolean;
  hasOrganization: boolean;
  hasAdminUser: boolean;
  hasBranches: boolean;
  hasRoles: boolean;
  currentStep: number;
  totalSteps: number;
}

export interface SetupWizardStep {
  id: string;
  title: string;
  description: string;
  isComplete: boolean;
  isRequired: boolean;
}

export interface OrganizationSetupData {
  id?: number;
  name: string;
  email: string;
  phone: string;
  address: string;
  website?: string;
  taxId?: string;
  registrationNumber?: string;
  normalWorkingHours: string;
  overtimeRate: number;
  productiveHoursThreshold: number;
}

export interface AdminUserSetupData {
  id?: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  password: string;
  confirmPassword: string;
}

export interface BranchSetupData {
  id?: number;
  name: string;
  address: string;
  phone: string;
  email: string;
  isHeadOffice: boolean;
}

export interface SystemPreferencesData {
  timezone: string;
  dateFormat: string;
  currency: string;
  language: string;
  enableNotifications: boolean;
  enableRealTimeUpdates: boolean;
}

export interface RoleConfigurationData {
  selectedRoles: string[];
  customRoles?: {
    name: string;
    description: string;
    permissions: string[];
  }[];
}

export interface SetupCompletionData {
  organizationId: number;
  adminUserId: number;
  branchId: number;
  setupCompletedAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class SetupWizardService {
  private readonly apiUrl = `${environment.apiUrl}`;
  private setupStatusSubject = new BehaviorSubject<SetupStatus | null>(null);

  public setupStatus$ = this.setupStatusSubject.asObservable();

  // Store setup data during the wizard process
  private setupData: {
    organization?: OrganizationSetupData;
    adminUser?: AdminUserSetupData;
    branch?: BranchSetupData;
    roles?: RoleConfigurationData;
    preferences?: SystemPreferencesData;
  } = {};

  constructor(
    private http: HttpClient,
    private notificationService: NotificationService,
    private loadingService: LoadingService
  ) { }

  // Setup status management
  checkSetupStatus(): Observable<SetupStatus> {
    this.loadingService.setLoading(true, 'setup-status-check');

    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/setup/status`).pipe(
      map(response => {
        if (response.success && response.data) {
          const data = response.data;
          const status: SetupStatus = {
            isSetupComplete: data.isSetupComplete ?? false,
            hasOrganization: data.hasOrganization ?? false,
            hasAdminUser: data.hasAdminUser ?? false,
            hasBranches: data.hasBranches ?? false,
            hasRoles: data.hasRoles ?? false,
            currentStep: data.currentStep ?? 1,
            totalSteps: data.totalSteps ?? 5
          };

          this.setupStatusSubject.next(status);
          return status;
        } else {
          throw new Error('Failed to get setup status');
        }
      }),
      tap(() => this.loadingService.setLoading(false, 'setup-status-check')),
      catchError(error => {
        this.loadingService.setLoading(false, 'setup-status-check');
        console.error('Error checking setup status:', error);

        // Fallback to individual API calls if setup endpoint doesn't exist
        return forkJoin({
          organizations: this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/organization`).pipe(
            catchError(() => of({ success: false, data: [] }))
          ),
          branches: this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/branch`).pipe(
            catchError(() => of({ success: false, data: [] }))
          ),
          roles: this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/role`).pipe(
            catchError(() => of({ success: false, data: [] }))
          ),
          users: this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/employees`).pipe(
            catchError(() => of({ success: false, data: [] }))
          )
        }).pipe(
          map(results => {
            const hasOrganization = results.organizations.success && results.organizations.data && results.organizations.data.length > 0;
            const hasBranches = results.branches.success && results.branches.data && results.branches.data.length > 0;
            const hasRoles = results.roles.success && results.roles.data && results.roles.data.length > 0;
            const hasAdminUser = results.users.success && results.users.data && results.users.data.length > 0;

            const isSetupComplete = hasOrganization && hasBranches && hasRoles && hasAdminUser;

            let currentStep = 1;
            if (hasOrganization) currentStep = 2;
            if (hasAdminUser) currentStep = 3;
            if (hasBranches) currentStep = 4;
            if (hasRoles) currentStep = 5;
            if (isSetupComplete) currentStep = 5;

            const status: SetupStatus = {
              isSetupComplete: isSetupComplete ?? false,
              hasOrganization: hasOrganization ?? false,
              hasAdminUser: hasAdminUser ?? false,
              hasBranches: hasBranches ?? false,
              hasRoles: hasRoles ?? false,
              currentStep: currentStep ?? 1,
              totalSteps: 5
            };

            this.setupStatusSubject.next(status);
            return status;
          }),
          catchError(() => {
            // Return default status if all API calls fail
            const defaultStatus: SetupStatus = {
              isSetupComplete: false,
              hasOrganization: false,
              hasAdminUser: false,
              hasBranches: false,
              hasRoles: false,
              currentStep: 1,
              totalSteps: 5
            };

            this.setupStatusSubject.next(defaultStatus);
            return of(defaultStatus);
          })
        );
      })
    );
  }

  getSetupSteps(): SetupWizardStep[] {
    return [
      {
        id: 'organization',
        title: 'Organization Setup',
        description: 'Configure your organization details and basic settings',
        isComplete: false,
        isRequired: true
      },
      {
        id: 'admin-user',
        title: 'Admin User Setup',
        description: 'Create the first administrator account',
        isComplete: false,
        isRequired: true
      },
      {
        id: 'branch',
        title: 'Branch Configuration',
        description: 'Set up your first branch or head office',
        isComplete: false,
        isRequired: true
      },
      {
        id: 'roles',
        title: 'Role Configuration',
        description: 'Define user roles and permissions',
        isComplete: false,
        isRequired: true
      },
      {
        id: 'preferences',
        title: 'System Preferences',
        description: 'Configure system-wide preferences and settings',
        isComplete: false,
        isRequired: false
      }
    ];
  }

  // Step data management
  saveOrganizationData(data: OrganizationSetupData): Observable<ApiResponse<any>> {
    this.loadingService.setLoading(true, 'save-organization');

    // Store data temporarily for the wizard process
    this.setupData.organization = data;

    const organizationDto = {
      name: data.name,
      email: data.email,
      phone: data.phone,
      address: data.address,
      website: data.website,
      taxId: data.taxId,
      registrationNumber: data.registrationNumber,
      normalWorkingHours: data.normalWorkingHours,
      overtimeRate: data.overtimeRate,
      productiveHoursThreshold: data.productiveHoursThreshold,
      branchIsolationEnabled: false // Default value for setup
    };

    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/organization`, organizationDto).pipe(
      tap(response => {
        if (response.success) {
          this.setupData.organization = { ...data };
          if (response.data?.id) {
            this.setupData.organization.id = response.data.id;
          }
          this.notificationService.showSuccess('Organization created successfully');
        }
        this.loadingService.setLoading(false, 'save-organization');
      }),
      catchError(error => {
        this.loadingService.setLoading(false, 'save-organization');
        this.notificationService.showError('Failed to create organization: ' + (error.message || 'Unknown error'));
        return of({
          success: false,
          message: 'Failed to create organization',
          errors: [error.message || 'Unknown error'],
          timestamp: new Date().toISOString()
        } as ApiResponse<any>);
      })
    );
  }

  saveAdminUserData(data: AdminUserSetupData): Observable<ApiResponse<any>> {
    this.loadingService.setLoading(true, 'save-admin-user');

    // Store data temporarily for the wizard process
    this.setupData.adminUser = data;

    const adminUserDto = {
      firstName: data.firstName,
      lastName: data.lastName,
      email: data.email,
      phone: data.phone,
      password: data.password, // Backend will handle hashing
      employeeId: 'ADMIN001', // Default admin employee ID
      designation: 'System Administrator',
      department: 'Administration',
      basicSalary: 0, // Default for admin
      joiningDate: new Date().toISOString(),
      status: 'Active',
      branchId: 1, // Will be updated after branch creation
      roles: ['SuperAdmin'] // Default super admin role
    };

    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/employees`, adminUserDto).pipe(
      tap(response => {
        if (response.success) {
          this.setupData.adminUser = { ...data };
          if (response.data?.id) {
            this.setupData.adminUser.id = response.data.id;
          }
          this.notificationService.showSuccess('Administrator account created successfully');
        }
        this.loadingService.setLoading(false, 'save-admin-user');
      }),
      catchError(error => {
        this.loadingService.setLoading(false, 'save-admin-user');
        this.notificationService.showError('Failed to create administrator account: ' + (error.message || 'Unknown error'));
        return of({
          success: false,
          message: 'Failed to create administrator account',
          errors: [error.message || 'Unknown error'],
          timestamp: new Date().toISOString()
        } as ApiResponse<any>);
      })
    );
  }

  saveBranchData(data: BranchSetupData): Observable<ApiResponse<any>> {
    this.loadingService.setLoading(true, 'save-branch');

    // Store data temporarily for the wizard process
    this.setupData.branch = data;

    const branchDto = {
      organizationId: this.setupData.organization?.id || 1, // Use created organization ID
      name: data.name,
      address: data.address,
      phone: data.phone,
      email: data.email,
      country: 'United States', // Default - can be made configurable
      currency: 'USD', // Default - can be made configurable
      timeZone: 'UTC', // Default - can be made configurable
      localHolidays: [], // Empty initially
      complianceSettings: {}, // Empty initially
      isHeadOffice: data.isHeadOffice
    };

    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/branch`, branchDto).pipe(
      tap(response => {
        if (response.success) {
          this.setupData.branch = { ...data };
          if (response.data?.id) {
            this.setupData.branch.id = response.data.id;
          }
          this.notificationService.showSuccess('Branch created successfully');
        }
        this.loadingService.setLoading(false, 'save-branch');
      }),
      catchError(error => {
        this.loadingService.setLoading(false, 'save-branch');
        this.notificationService.showError('Failed to create branch: ' + (error.message || 'Unknown error'));
        return of({
          success: false,
          message: 'Failed to create branch',
          errors: [error.message || 'Unknown error'],
          timestamp: new Date().toISOString()
        } as ApiResponse<any>);
      })
    );
  }

  saveRoleConfiguration(roleData: RoleConfigurationData): Observable<ApiResponse<any>> {
    this.loadingService.setLoading(true, 'save-roles');

    // Store data temporarily for the wizard process
    this.setupData.roles = roleData;

    // Define default roles with their configurations
    const defaultRoles = [
      {
        name: 'SuperAdmin',
        description: 'Full system access and control',
        hierarchyLevel: 1,
        permissions: ['*'] // All permissions
      },
      {
        name: 'Admin',
        description: 'Organization-wide administrative access',
        hierarchyLevel: 2,
        permissions: ['user.manage', 'organization.manage', 'reports.view']
      },
      {
        name: 'HR',
        description: 'Human resources management',
        hierarchyLevel: 3,
        permissions: ['employee.manage', 'leave.manage', 'payroll.view']
      },
      {
        name: 'Manager',
        description: 'Team and project management',
        hierarchyLevel: 4,
        permissions: ['team.manage', 'project.manage', 'attendance.view']
      },
      {
        name: 'Employee',
        description: 'Standard employee access',
        hierarchyLevel: 5,
        permissions: ['profile.view', 'attendance.own', 'leave.own']
      }
    ];

    // Filter roles based on selection
    const rolesToCreate = defaultRoles.filter(role =>
      roleData.selectedRoles.includes(role.name.toLowerCase())
    );

    // Add custom roles if any
    if (roleData.customRoles) {
      rolesToCreate.push(...roleData.customRoles.map((role, index) => ({
        name: role.name,
        description: role.description,
        hierarchyLevel: 6 + index, // Custom roles get higher hierarchy levels
        permissions: role.permissions
      })));
    }

    // Create roles via API calls
    const roleCreationObservables = rolesToCreate.map(role =>
      this.http.post<ApiResponse<any>>(`${this.apiUrl}/role`, {
        name: role.name,
        description: role.description,
        hierarchyLevel: role.hierarchyLevel,
        permissionIds: [] // Will be handled by backend based on permissions array
      }).pipe(
        catchError(error => {
          console.error(`Failed to create role ${role.name}:`, error);
          return of({
            success: false,
            message: `Failed to create role ${role.name}`,
            timestamp: new Date().toISOString()
          } as ApiResponse<any>);
        })
      )
    );

    return forkJoin(roleCreationObservables).pipe(
      map(results => {
        const successCount = results.filter(r => r.success).length;
        const totalCount = results.length;

        if (successCount === totalCount) {
          this.notificationService.showSuccess(`All ${totalCount} roles created successfully`);
          return {
            success: true,
            message: 'Roles configured successfully',
            data: results,
            timestamp: new Date().toISOString()
          } as ApiResponse<any>;
        } else if (successCount > 0) {
          this.notificationService.showWarning(`${successCount} of ${totalCount} roles created successfully`);
          return {
            success: true,
            message: 'Some roles configured successfully',
            data: results,
            timestamp: new Date().toISOString()
          } as ApiResponse<any>;
        } else {
          this.notificationService.showError('Failed to create any roles');
          return {
            success: false,
            message: 'Failed to configure roles',
            data: results,
            timestamp: new Date().toISOString()
          } as ApiResponse<any>;
        }
      }),
      tap(() => this.loadingService.setLoading(false, 'save-roles')),
      catchError(error => {
        this.loadingService.setLoading(false, 'save-roles');
        this.notificationService.showError('Failed to configure roles: ' + (error.message || 'Unknown error'));
        return of({
          success: false,
          message: 'Failed to configure roles',
          errors: [error.message || 'Unknown error'],
          timestamp: new Date().toISOString()
        } as ApiResponse<any>);
      })
    );
  }

  saveSystemPreferences(data: SystemPreferencesData): Observable<ApiResponse<any>> {
    this.loadingService.setLoading(true, 'save-preferences');

    // Store data temporarily for the wizard process
    this.setupData.preferences = data;

    // Create system configuration entries
    const configurationEntries = [
      { key: 'system.timezone', value: data.timezone, category: 'System', description: 'Default system timezone' },
      { key: 'system.dateFormat', value: data.dateFormat, category: 'System', description: 'Default date format' },
      { key: 'system.currency', value: data.currency, category: 'System', description: 'Default currency' },
      { key: 'system.language', value: data.language, category: 'System', description: 'Default language' },
      { key: 'notifications.enabled', value: data.enableNotifications.toString(), category: 'Notifications', description: 'Enable email notifications' },
      { key: 'realtime.enabled', value: data.enableRealTimeUpdates.toString(), category: 'System', description: 'Enable real-time updates' }
    ];

    // Save configuration entries (assuming there's a system config endpoint)
    const configObservables = configurationEntries.map(config =>
      this.http.post<ApiResponse<any>>(`${this.apiUrl}/system-config`, config).pipe(
        catchError(error => {
          console.error(`Failed to save config ${config.key}:`, error);
          return of({
            success: false,
            message: `Failed to save ${config.key}`,
            timestamp: new Date().toISOString()
          } as ApiResponse<any>);
        })
      )
    );

    return forkJoin(configObservables).pipe(
      map(results => {
        const successCount = results.filter(r => r.success).length;
        const totalCount = results.length;

        if (successCount === totalCount) {
          this.notificationService.showSuccess('System preferences saved successfully');
          return {
            success: true,
            message: 'System preferences saved successfully',
            data: results,
            timestamp: new Date().toISOString()
          } as ApiResponse<any>;
        } else if (successCount > 0) {
          this.notificationService.showWarning(`${successCount} of ${totalCount} preferences saved successfully`);
          return {
            success: true,
            message: 'Some preferences saved successfully',
            data: results,
            timestamp: new Date().toISOString()
          } as ApiResponse<any>;
        } else {
          // If system config endpoint doesn't exist, save to localStorage as fallback
          localStorage.setItem('system-preferences', JSON.stringify(data));
          this.notificationService.showSuccess('System preferences saved locally');
          return {
            success: true,
            message: 'System preferences saved locally',
            data: data,
            timestamp: new Date().toISOString()
          } as ApiResponse<any>;
        }
      }),
      tap(() => this.loadingService.setLoading(false, 'save-preferences')),
      catchError(error => {
        this.loadingService.setLoading(false, 'save-preferences');
        // Fallback to localStorage if API fails
        try {
          localStorage.setItem('system-preferences', JSON.stringify(data));
          this.notificationService.showSuccess('System preferences saved locally');
          return of({
            success: true,
            message: 'System preferences saved locally',
            data: data,
            timestamp: new Date().toISOString()
          } as ApiResponse<any>);
        } catch (localError) {
          this.notificationService.showError('Failed to save system preferences');
          return of({
            success: false,
            message: 'Failed to save system preferences',
            errors: [error.message || 'Unknown error'],
            timestamp: new Date().toISOString()
          } as ApiResponse<any>);
        }
      })
    );
  }

  // Complete setup
  completeSetup(): Observable<ApiResponse<SetupCompletionData>> {
    this.loadingService.setLoading(true, 'complete-setup');

    const completionRequest = {
      organizationId: this.setupData.organization?.id || 0,
      adminUserId: this.setupData.adminUser?.id || 0,
      branchId: this.setupData.branch?.id || 0
    };

    // Mark setup as complete in the system
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/setup/complete`, completionRequest).pipe(
      switchMap(response => {
        if (response.success) {
          // Update setup status
          const completedStatus: SetupStatus = {
            isSetupComplete: true,
            hasOrganization: true,
            hasAdminUser: true,
            hasBranches: true,
            hasRoles: true,
            currentStep: 5,
            totalSteps: 5
          };

          this.setupStatusSubject.next(completedStatus);

          // Create completion data from response
          const completionData: SetupCompletionData = {
            organizationId: response.data?.organizationId || completionRequest.organizationId,
            adminUserId: response.data?.adminUserId || completionRequest.adminUserId,
            branchId: response.data?.branchId || completionRequest.branchId,
            setupCompletedAt: response.data?.setupCompletedAt || new Date().toISOString()
          };

          // Clear temporary setup data
          this.setupData = {};

          // Also save to localStorage as backup
          localStorage.setItem('organization-setup-complete', 'true');
          localStorage.setItem('setup-completion-data', JSON.stringify(completionData));

          this.notificationService.showSuccess('Setup completed successfully! Welcome to StrideHR.');

          return of({
            success: true,
            message: 'Setup completed successfully',
            data: completionData,
            timestamp: new Date().toISOString()
          } as ApiResponse<SetupCompletionData>);
        } else {
          throw new Error(response.message || 'Failed to complete setup');
        }
      }),
      tap(() => this.loadingService.setLoading(false, 'complete-setup')),
      catchError(error => {
        this.loadingService.setLoading(false, 'complete-setup');

        // Fallback to localStorage if API fails
        try {
          const fallbackCompletionData: SetupCompletionData = {
            organizationId: completionRequest.organizationId,
            adminUserId: completionRequest.adminUserId,
            branchId: completionRequest.branchId,
            setupCompletedAt: new Date().toISOString()
          };

          localStorage.setItem('organization-setup-complete', 'true');
          localStorage.setItem('setup-completion-data', JSON.stringify(fallbackCompletionData));

          const completedStatus: SetupStatus = {
            isSetupComplete: true,
            hasOrganization: true,
            hasAdminUser: true,
            hasBranches: true,
            hasRoles: true,
            currentStep: 5,
            totalSteps: 5
          };

          this.setupStatusSubject.next(completedStatus);
          this.setupData = {};

          this.notificationService.showSuccess('Setup completed locally! Welcome to StrideHR.');

          return of({
            success: true,
            message: 'Setup completed locally',
            data: fallbackCompletionData,
            timestamp: new Date().toISOString()
          } as ApiResponse<SetupCompletionData>);
        } catch (localError) {
          this.notificationService.showError('Failed to complete setup: ' + (error.message || 'Unknown error'));
          return of({
            success: false,
            message: 'Failed to complete setup',
            errors: [error.message || 'Unknown error'],
            timestamp: new Date().toISOString()
          } as ApiResponse<SetupCompletionData>);
        }
      })
    );
  }

  // Utility methods
  isSetupRequired(): Observable<boolean> {
    return this.checkSetupStatus().pipe(
      map(status => !status.isSetupComplete),
      catchError(() => of(true)) // If we can't check setup status, assume setup is required
    );
  }

  // Get current setup data
  getCurrentSetupData() {
    return { ...this.setupData };
  }

  // Clear setup data
  clearSetupData(): void {
    this.setupData = {};
  }

  // Get step data for resuming setup
  getStepData(stepId: string): any {
    switch (stepId) {
      case 'organization':
        return this.setupData.organization;
      case 'admin-user':
        return this.setupData.adminUser;
      case 'branch':
        return this.setupData.branch;
      case 'roles':
        return this.setupData.roles;
      case 'preferences':
        return this.setupData.preferences;
      default:
        return null;
    }
  }

  // Create default super admin user with specified credentials
  createDefaultSuperAdmin(): Observable<ApiResponse<any>> {
    const defaultAdminData: AdminUserSetupData = {
      firstName: 'Super',
      lastName: 'Admin',
      email: 'superadmin@stridehr.com',
      phone: '+1-555-0100',
      password: 'adminsuper2025$',
      confirmPassword: 'adminsuper2025$'
    };

    return this.saveAdminUserData(defaultAdminData);
  }

  // Get default roles from backend
  getDefaultRoles(): Observable<ApiResponse<any[]>> {
    return this.http.get<ApiResponse<any[]>>(`${this.apiUrl}/setup/default-roles`).pipe(
      catchError(error => {
        console.error('Error getting default roles from backend:', error);
        // Return hardcoded default roles as fallback
        const defaultRoles = [
          {
            id: 'superadmin',
            name: 'SuperAdmin',
            description: 'Full system access and control',
            icon: 'fas fa-crown',
            colorClass: 'bg-danger',
            required: true
          },
          {
            id: 'admin',
            name: 'Admin',
            description: 'Organization-wide administrative access',
            icon: 'fas fa-user-shield',
            colorClass: 'bg-primary',
            required: false
          },
          {
            id: 'hr',
            name: 'HR',
            description: 'Human resources management',
            icon: 'fas fa-users',
            colorClass: 'bg-success',
            required: false
          },
          {
            id: 'manager',
            name: 'Manager',
            description: 'Team and project management',
            icon: 'fas fa-user-tie',
            colorClass: 'bg-warning',
            required: false
          },
          {
            id: 'employee',
            name: 'Employee',
            description: 'Standard employee access',
            icon: 'fas fa-user',
            colorClass: 'bg-info',
            required: false
          }
        ];

        return of({
          success: true,
          message: 'Default roles retrieved from fallback',
          data: defaultRoles,
          timestamp: new Date().toISOString()
        } as ApiResponse<any[]>);
      })
    );
  }

  updateSetupStatus(status: SetupStatus): void {
    this.setupStatusSubject.next(status);
  }

  getCurrentSetupStatus(): SetupStatus | null {
    return this.setupStatusSubject.value;
  }

  // Validation methods
  validateOrganizationData(data: OrganizationSetupData): string[] {
    const errors: string[] = [];

    if (!data.name?.trim()) {
      errors.push('Organization name is required');
    }

    if (!data.email?.trim()) {
      errors.push('Email is required');
    } else if (!this.isValidEmail(data.email)) {
      errors.push('Invalid email format');
    }

    if (!data.phone?.trim()) {
      errors.push('Phone number is required');
    }

    if (!data.address?.trim()) {
      errors.push('Address is required');
    }

    if (!data.normalWorkingHours?.trim()) {
      errors.push('Normal working hours is required');
    }

    if (data.overtimeRate < 0) {
      errors.push('Overtime rate cannot be negative');
    }

    if (data.productiveHoursThreshold < 0) {
      errors.push('Productive hours threshold cannot be negative');
    }

    return errors;
  }

  validateAdminUserData(data: AdminUserSetupData): string[] {
    const errors: string[] = [];

    if (!data.firstName?.trim()) {
      errors.push('First name is required');
    }

    if (!data.lastName?.trim()) {
      errors.push('Last name is required');
    }

    if (!data.email?.trim()) {
      errors.push('Email is required');
    } else if (!this.isValidEmail(data.email)) {
      errors.push('Invalid email format');
    }

    if (!data.phone?.trim()) {
      errors.push('Phone number is required');
    }

    if (!data.password?.trim()) {
      errors.push('Password is required');
    } else if (data.password.length < 8) {
      errors.push('Password must be at least 8 characters long');
    }

    if (data.password !== data.confirmPassword) {
      errors.push('Passwords do not match');
    }

    return errors;
  }

  validateBranchData(data: BranchSetupData): string[] {
    const errors: string[] = [];

    if (!data.name?.trim()) {
      errors.push('Branch name is required');
    }

    if (!data.address?.trim()) {
      errors.push('Branch address is required');
    }

    if (!data.phone?.trim()) {
      errors.push('Branch phone is required');
    }

    if (!data.email?.trim()) {
      errors.push('Branch email is required');
    } else if (!this.isValidEmail(data.email)) {
      errors.push('Invalid email format');
    }

    return errors;
  }

  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }
}