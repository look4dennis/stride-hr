import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { NotificationService } from '../../core/services/notification.service';
import { LoadingService } from '../../core/services/loading.service';
import { AuthService } from '../../core/auth/auth.service';

export interface ButtonAction {
  id: string;
  type: 'navigate' | 'api' | 'modal' | 'download' | 'custom';
  target?: string;
  params?: any;
  confirmation?: {
    title: string;
    message: string;
    confirmText?: string;
    cancelText?: string;
  };
  loading?: boolean;
  disabled?: boolean;
  roles?: string[];
}

export interface ButtonValidationResult {
  buttonId: string;
  isWorking: boolean;
  hasClickHandler: boolean;
  hasProperIntegration: boolean;
  error?: string;
  suggestions?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class ButtonHandlerService {
  private buttonActions = new Map<string, ButtonAction>();
  private buttonElements = new Map<string, HTMLElement>();

  constructor(
    private router: Router,
    private notificationService: NotificationService,
    private loadingService: LoadingService,
    private authService: AuthService
  ) {
    this.initializeCommonButtons();
  }

  /**
   * Initialize common button actions
   */
  private initializeCommonButtons(): void {
    // Navigation buttons
    this.registerButton('btn-dashboard', {
      id: 'btn-dashboard',
      type: 'navigate',
      target: '/dashboard'
    });

    this.registerButton('btn-employees', {
      id: 'btn-employees',
      type: 'navigate',
      target: '/employees'
    });

    this.registerButton('btn-add-employee', {
      id: 'btn-add-employee',
      type: 'navigate',
      target: '/employees/add',
      roles: ['HR', 'Admin', 'SuperAdmin']
    });

    this.registerButton('btn-attendance', {
      id: 'btn-attendance',
      type: 'navigate',
      target: '/attendance'
    });

    this.registerButton('btn-attendance-now', {
      id: 'btn-attendance-now',
      type: 'navigate',
      target: '/attendance/now',
      roles: ['HR', 'Admin', 'Manager', 'SuperAdmin']
    });

    // Action buttons
    this.registerButton('btn-check-in', {
      id: 'btn-check-in',
      type: 'api',
      target: 'attendance.checkIn',
      loading: true,
      confirmation: {
        title: 'Check In',
        message: 'Are you ready to check in for today?',
        confirmText: 'Check In',
        cancelText: 'Cancel'
      }
    });

    this.registerButton('btn-check-out', {
      id: 'btn-check-out',
      type: 'api',
      target: 'attendance.checkOut',
      loading: true,
      confirmation: {
        title: 'Check Out',
        message: 'Are you ready to check out for today?',
        confirmText: 'Check Out',
        cancelText: 'Cancel'
      }
    });

    this.registerButton('btn-start-break', {
      id: 'btn-start-break',
      type: 'api',
      target: 'attendance.startBreak',
      loading: true
    });

    this.registerButton('btn-end-break', {
      id: 'btn-end-break',
      type: 'api',
      target: 'attendance.endBreak',
      loading: true
    });

    // CRUD buttons
    this.registerButton('btn-save-employee', {
      id: 'btn-save-employee',
      type: 'api',
      target: 'employee.save',
      loading: true,
      roles: ['HR', 'Admin', 'SuperAdmin']
    });

    this.registerButton('btn-edit-employee', {
      id: 'btn-edit-employee',
      type: 'navigate',
      target: '/employees/:id/edit',
      roles: ['HR', 'Admin', 'SuperAdmin']
    });

    this.registerButton('btn-delete-employee', {
      id: 'btn-delete-employee',
      type: 'api',
      target: 'employee.delete',
      loading: true,
      roles: ['HR', 'Admin', 'SuperAdmin'],
      confirmation: {
        title: 'Delete Employee',
        message: 'Are you sure you want to delete this employee? This action cannot be undone.',
        confirmText: 'Delete',
        cancelText: 'Cancel'
      }
    });

    this.registerButton('btn-view-employee', {
      id: 'btn-view-employee',
      type: 'navigate',
      target: '/employees/:id'
    });

    // Report buttons
    this.registerButton('btn-generate-report', {
      id: 'btn-generate-report',
      type: 'api',
      target: 'report.generate',
      loading: true,
      roles: ['HR', 'Admin', 'Manager', 'SuperAdmin']
    });

    this.registerButton('btn-export-report', {
      id: 'btn-export-report',
      type: 'download',
      target: 'report.export',
      loading: true,
      roles: ['HR', 'Admin', 'Manager', 'SuperAdmin']
    });

    // Settings buttons
    this.registerButton('btn-save-settings', {
      id: 'btn-save-settings',
      type: 'api',
      target: 'settings.save',
      loading: true,
      roles: ['Admin', 'SuperAdmin']
    });

    // Auth buttons
    this.registerButton('btn-logout', {
      id: 'btn-logout',
      type: 'custom',
      target: 'auth.logout',
      confirmation: {
        title: 'Logout',
        message: 'Are you sure you want to logout?',
        confirmText: 'Logout',
        cancelText: 'Cancel'
      }
    });

    this.registerButton('btn-reset-password', {
      id: 'btn-reset-password',
      type: 'api',
      target: 'auth.resetPassword',
      loading: true
    });
  }

  /**
   * Register a button with its action configuration
   */
  registerButton(buttonId: string, action: ButtonAction): void {
    this.buttonActions.set(buttonId, action);
  }

  /**
   * Register a button element for validation
   */
  registerButtonElement(buttonId: string, element: HTMLElement): void {
    this.buttonElements.set(buttonId, element);
    this.attachClickHandler(buttonId, element);
  }

  /**
   * Attach click handler to button element
   */
  private attachClickHandler(buttonId: string, element: HTMLElement): void {
    const action = this.buttonActions.get(buttonId);
    if (!action) {
      console.warn(`No action registered for button: ${buttonId}`);
      return;
    }

    element.addEventListener('click', (event) => {
      event.preventDefault();
      this.handleButtonClick(buttonId, action);
    });
  }

  /**
   * Handle button click based on action configuration
   */
  async handleButtonClick(buttonId: string, action?: ButtonAction): Promise<void> {
    const buttonAction = action || this.buttonActions.get(buttonId);
    if (!buttonAction) {
      console.error(`No action found for button: ${buttonId}`);
      this.notificationService.showError('Button action not configured');
      return;
    }

    try {
      // Check permissions
      if (buttonAction.roles && !this.authService.hasAnyRole(buttonAction.roles)) {
        this.notificationService.showError('You do not have permission to perform this action');
        return;
      }

      // Check if button is disabled
      if (buttonAction.disabled) {
        return;
      }

      // Show confirmation if required
      if (buttonAction.confirmation) {
        const confirmed = await this.showConfirmation(buttonAction.confirmation);
        if (!confirmed) {
          return;
        }
      }

      // Show loading if required
      if (buttonAction.loading) {
        this.loadingService.setGlobalLoading(true);
      }

      // Execute action based on type
      switch (buttonAction.type) {
        case 'navigate':
          await this.handleNavigationAction(buttonAction);
          break;
        case 'api':
          await this.handleApiAction(buttonAction);
          break;
        case 'modal':
          await this.handleModalAction(buttonAction);
          break;
        case 'download':
          await this.handleDownloadAction(buttonAction);
          break;
        case 'custom':
          await this.handleCustomAction(buttonAction);
          break;
        default:
          throw new Error(`Unknown button action type: ${buttonAction.type}`);
      }

    } catch (error) {
      console.error(`Button action failed for ${buttonId}:`, error);
      this.notificationService.showError('Action failed. Please try again.');
    } finally {
      if (buttonAction?.loading) {
        this.loadingService.setGlobalLoading(false);
      }
    }
  }

  /**
   * Handle navigation actions
   */
  private async handleNavigationAction(action: ButtonAction): Promise<void> {
    if (!action.target) {
      throw new Error('Navigation target not specified');
    }

    let route = action.target;
    
    // Replace parameters if provided
    if (action.params) {
      Object.keys(action.params).forEach(key => {
        route = route.replace(`:${key}`, action.params[key]);
      });
    }

    const success = await this.router.navigate([route]);
    if (!success) {
      throw new Error(`Navigation to ${route} failed`);
    }
  }

  /**
   * Handle API actions
   */
  private async handleApiAction(action: ButtonAction): Promise<void> {
    if (!action.target) {
      throw new Error('API target not specified');
    }

    // This would typically call the appropriate service method
    // For now, we'll simulate the API call
    console.log(`API action: ${action.target}`, action.params);
    
    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 1000));
    
    this.notificationService.showSuccess('Action completed successfully');
  }

  /**
   * Handle modal actions
   */
  private async handleModalAction(action: ButtonAction): Promise<void> {
    // This would typically open a modal
    console.log(`Modal action: ${action.target}`, action.params);
    this.notificationService.showInfo('Modal action triggered');
  }

  /**
   * Handle download actions
   */
  private async handleDownloadAction(action: ButtonAction): Promise<void> {
    // This would typically trigger a file download
    console.log(`Download action: ${action.target}`, action.params);
    this.notificationService.showSuccess('Download started');
  }

  /**
   * Handle custom actions
   */
  private async handleCustomAction(action: ButtonAction): Promise<void> {
    switch (action.target) {
      case 'auth.logout':
        this.authService.logout();
        break;
      default:
        console.log(`Custom action: ${action.target}`, action.params);
        this.notificationService.showInfo('Custom action triggered');
    }
  }

  /**
   * Show confirmation dialog
   */
  private async showConfirmation(confirmation: NonNullable<ButtonAction['confirmation']>): Promise<boolean> {
    // This would typically show a proper modal dialog
    // For now, use browser confirm
    return window.confirm(`${confirmation.title}\n\n${confirmation.message}`);
  }

  /**
   * Validate all registered buttons
   */
  validateAllButtons(): Observable<ButtonValidationResult[]> {
    const results: ButtonValidationResult[] = [];

    this.buttonActions.forEach((action, buttonId) => {
      const element = this.buttonElements.get(buttonId);
      const hasClickHandler = element ? this.hasClickHandler(element) : false;
      const hasProperIntegration = this.validateButtonIntegration(action);

      results.push({
        buttonId,
        isWorking: hasClickHandler && hasProperIntegration,
        hasClickHandler,
        hasProperIntegration,
        error: !hasClickHandler ? 'No click handler attached' : 
               !hasProperIntegration ? 'Integration validation failed' : undefined,
        suggestions: this.getButtonSuggestions(buttonId, action, hasClickHandler, hasProperIntegration)
      });
    });

    return of(results);
  }

  /**
   * Check if element has click handler
   */
  private hasClickHandler(element: HTMLElement): boolean {
    // Check if element has event listeners (simplified check)
    return element.onclick !== null || 
           element.getAttribute('ng-click') !== null ||
           element.getAttribute('(click)') !== null;
  }

  /**
   * Validate button integration
   */
  private validateButtonIntegration(action: ButtonAction): boolean {
    switch (action.type) {
      case 'navigate':
        return !!action.target && action.target.startsWith('/');
      case 'api':
        return !!action.target && action.target.includes('.');
      case 'modal':
      case 'download':
      case 'custom':
        return !!action.target;
      default:
        return false;
    }
  }

  /**
   * Get suggestions for button improvements
   */
  private getButtonSuggestions(
    buttonId: string, 
    action: ButtonAction, 
    hasClickHandler: boolean, 
    hasProperIntegration: boolean
  ): string[] {
    const suggestions: string[] = [];

    if (!hasClickHandler) {
      suggestions.push('Add click event handler to button element');
      suggestions.push('Ensure button is properly registered with ButtonHandlerService');
    }

    if (!hasProperIntegration) {
      suggestions.push('Check action configuration');
      suggestions.push('Verify target is properly specified');
      
      if (action.type === 'navigate') {
        suggestions.push('Ensure navigation target starts with "/"');
      } else if (action.type === 'api') {
        suggestions.push('Ensure API target follows "service.method" format');
      }
    }

    if (action.roles && action.roles.length > 0) {
      suggestions.push('Verify user has required roles for this action');
    }

    return suggestions;
  }

  /**
   * Get button action configuration
   */
  getButtonAction(buttonId: string): ButtonAction | undefined {
    return this.buttonActions.get(buttonId);
  }

  /**
   * Update button action configuration
   */
  updateButtonAction(buttonId: string, updates: Partial<ButtonAction>): void {
    const existing = this.buttonActions.get(buttonId);
    if (existing) {
      this.buttonActions.set(buttonId, { ...existing, ...updates });
    }
  }

  /**
   * Enable/disable button
   */
  setButtonEnabled(buttonId: string, enabled: boolean): void {
    this.updateButtonAction(buttonId, { disabled: !enabled });
    
    const element = this.buttonElements.get(buttonId);
    if (element) {
      if (enabled) {
        element.removeAttribute('disabled');
        element.classList.remove('disabled');
      } else {
        element.setAttribute('disabled', 'true');
        element.classList.add('disabled');
      }
    }
  }

  /**
   * Set button loading state
   */
  setButtonLoading(buttonId: string, loading: boolean): void {
    const element = this.buttonElements.get(buttonId);
    if (element) {
      if (loading) {
        element.classList.add('loading');
        element.setAttribute('disabled', 'true');
      } else {
        element.classList.remove('loading');
        element.removeAttribute('disabled');
      }
    }
  }
}