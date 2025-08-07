import { Injectable } from '@angular/core';
import { Observable, combineLatest, of } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { UIElementValidatorService } from './ui-element-validator.service';
import { ButtonHandlerService } from './button-handler.service';
import { DropdownDataService } from './dropdown-data.service';
import { SearchService } from './search.service';
import { FormValidationService } from './form-validation.service';
import { CRUDOperationsService } from './crud-operations.service';
import { NavigationService } from '../../core/services/navigation.service';
import { NotificationService } from '../../core/services/notification.service';

export interface UIIntegrationReport {
  timestamp: string;
  overallHealth: 'excellent' | 'good' | 'fair' | 'poor';
  totalElements: number;
  workingElements: number;
  brokenElements: number;
  categories: {
    navigation: UIElementCategoryReport;
    buttons: UIElementCategoryReport;
    dropdowns: UIElementCategoryReport;
    search: UIElementCategoryReport;
    forms: UIElementCategoryReport;
    crud: UIElementCategoryReport;
  };
  recommendations: string[];
  autoFixAvailable: boolean;
}

export interface UIElementCategoryReport {
  name: string;
  totalElements: number;
  workingElements: number;
  brokenElements: number;
  healthScore: number;
  issues: string[];
  suggestions: string[];
}

@Injectable({
  providedIn: 'root'
})
export class UIIntegrationService {
  constructor(
    private uiValidator: UIElementValidatorService,
    private buttonHandler: ButtonHandlerService,
    private dropdownData: DropdownDataService,
    private searchService: SearchService,
    private formValidation: FormValidationService,
    private crudOperations: CRUDOperationsService,
    private navigationService: NavigationService,
    private notificationService: NotificationService
  ) { }

  /**
   * Generate comprehensive UI integration report
   */
  generateIntegrationReport(): Observable<UIIntegrationReport> {
    const validations = [
      this.navigationService.validateAllRoutes(),
      this.buttonHandler.validateAllButtons(),
      this.dropdownData.validateAllDropdowns(),
      this.searchService.validateAllSearches(),
      this.formValidation.validateAllForms(),
      this.crudOperations.validateAllCRUDOperations()
    ];

    return combineLatest(validations).pipe(
      map((results: any[]) => {
        const [navigation, buttons, dropdowns, searches, forms, crud] = results;

        // Process navigation results
        const navigationReport: UIElementCategoryReport = {
          name: 'Navigation',
          totalElements: 1,
          workingElements: (navigation as any)?.valid ? 1 : 0,
          brokenElements: (navigation as any)?.valid ? 0 : 1,
          healthScore: (navigation as any)?.valid ? 100 : 0,
          issues: (navigation as any)?.errors || [],
          suggestions: (navigation as any)?.valid ? [] : [
            'Check route configurations',
            'Verify all navigation paths are accessible',
            'Ensure proper route guards are in place'
          ]
        };

        // Process button results
        const buttonReport: UIElementCategoryReport = {
          name: 'Buttons',
          totalElements: buttons?.length || 0,
          workingElements: buttons?.filter((b: any) => b.isWorking).length || 0,
          brokenElements: buttons?.filter((b: any) => !b.isWorking).length || 0,
          healthScore: buttons && buttons.length > 0 ? Math.round((buttons.filter((b: any) => b.isWorking).length / buttons.length) * 100) : 100,
          issues: buttons?.filter((b: any) => !b.isWorking).map((b: any) => `${b.buttonId}: ${b.error || 'Not working'}`) || [],
          suggestions: this.extractSuggestions(buttons?.filter((b: any) => !b.isWorking).map((b: any) => b.suggestions || []) || [])
        };

        // Process dropdown results
        const dropdownReport: UIElementCategoryReport = {
          name: 'Dropdowns',
          totalElements: dropdowns?.length || 0,
          workingElements: dropdowns?.filter((d: any) => d.isWorking).length || 0,
          brokenElements: dropdowns?.filter((d: any) => !d.isWorking).length || 0,
          healthScore: dropdowns && dropdowns.length > 0 ? Math.round((dropdowns.filter((d: any) => d.isWorking).length / dropdowns.length) * 100) : 100,
          issues: dropdowns?.filter((d: any) => !d.isWorking).map((d: any) => `${d.dropdownId}: ${d.error || 'Not working'}`) || [],
          suggestions: this.extractSuggestions(dropdowns?.filter((d: any) => !d.isWorking).map((d: any) => d.suggestions || []) || [])
        };

        // Process search results
        const searchReport: UIElementCategoryReport = {
          name: 'Search',
          totalElements: searches?.length || 0,
          workingElements: searches?.filter((s: any) => s.isWorking).length || 0,
          brokenElements: searches?.filter((s: any) => !s.isWorking).length || 0,
          healthScore: searches && searches.length > 0 ? Math.round((searches.filter((s: any) => s.isWorking).length / searches.length) * 100) : 100,
          issues: searches?.filter((s: any) => !s.isWorking).map((s: any) => `${s.searchId}: ${s.error || 'Not working'}`) || [],
          suggestions: this.extractSuggestions(searches?.filter((s: any) => !s.isWorking).map((s: any) => s.suggestions || []) || [])
        };

        // Process form results
        const formReport: UIElementCategoryReport = {
          name: 'Forms',
          totalElements: forms?.length || 0,
          workingElements: forms?.filter((f: any) => f.isValid && f.hasEventHandlers).length || 0,
          brokenElements: forms?.filter((f: any) => !f.isValid || !f.hasEventHandlers).length || 0,
          healthScore: forms && forms.length > 0 ? Math.round((forms.filter((f: any) => f.isValid && f.hasEventHandlers).length / forms.length) * 100) : 100,
          issues: forms?.filter((f: any) => !f.isValid || !f.hasEventHandlers).map((f: any) => `${f.formId}: ${Object.keys(f.errors).length} validation errors`) || [],
          suggestions: this.extractSuggestions(forms?.filter((f: any) => !f.isValid || !f.hasEventHandlers).map((f: any) => f.suggestions || []) || [])
        };

        // Process CRUD results
        const crudReport: UIElementCategoryReport = {
          name: 'CRUD Operations',
          totalElements: crud?.length || 0,
          workingElements: crud?.filter((c: any) => c.isWorking).length || 0,
          brokenElements: crud?.filter((c: any) => !c.isWorking).length || 0,
          healthScore: crud && crud.length > 0 ? Math.round((crud.filter((c: any) => c.isWorking).length / crud.length) * 100) : 100,
          issues: crud?.filter((c: any) => !c.isWorking).map((c: any) => `${c.operationId}: ${c.error || 'Not working'}`) || [],
          suggestions: this.extractSuggestions(crud?.filter((c: any) => !c.isWorking).map((c: any) => c.suggestions || []) || [])
        };

        // Calculate overall metrics
        const totalElements = navigationReport.totalElements + buttonReport.totalElements +
          dropdownReport.totalElements + searchReport.totalElements +
          formReport.totalElements + crudReport.totalElements;

        const workingElements = navigationReport.workingElements + buttonReport.workingElements +
          dropdownReport.workingElements + searchReport.workingElements +
          formReport.workingElements + crudReport.workingElements;

        const brokenElements = totalElements - workingElements;
        const overallHealthScore = totalElements > 0 ? Math.round((workingElements / totalElements) * 100) : 100;

        // Determine overall health
        let overallHealth: 'excellent' | 'good' | 'fair' | 'poor';
        if (overallHealthScore >= 95) {
          overallHealth = 'excellent';
        } else if (overallHealthScore >= 80) {
          overallHealth = 'good';
        } else if (overallHealthScore >= 60) {
          overallHealth = 'fair';
        } else {
          overallHealth = 'poor';
        }

        // Generate recommendations
        const recommendations = this.generateRecommendations({
          navigationReport,
          buttonReport,
          dropdownReport,
          searchReport,
          formReport,
          crudReport,
          overallHealthScore
        });

        return {
          timestamp: new Date().toISOString(),
          overallHealth,
          totalElements,
          workingElements,
          brokenElements,
          categories: {
            navigation: navigationReport,
            buttons: buttonReport,
            dropdowns: dropdownReport,
            search: searchReport,
            forms: formReport,
            crud: crudReport
          },
          recommendations,
          autoFixAvailable: this.canAutoFix({
            navigationReport,
            buttonReport,
            dropdownReport,
            searchReport,
            formReport,
            crudReport
          })
        };
      }),
      catchError(error => {
        console.error('Failed to generate UI integration report:', error);
        return of(this.getEmptyReport());
      })
    );
  }

  /**
   * Extract unique suggestions from arrays
   */
  private extractSuggestions(suggestionArrays: string[][]): string[] {
    const allSuggestions = suggestionArrays.flat();
    return [...new Set(allSuggestions)];
  }

  /**
   * Generate recommendations based on report data
   */
  private generateRecommendations(reportData: any): string[] {
    const recommendations: string[] = [];

    // Navigation recommendations
    if (reportData.navigationReport.healthScore < 100) {
      recommendations.push('Fix navigation routing issues to ensure all pages are accessible');
    }

    // Button recommendations
    if (reportData.buttonReport.healthScore < 90) {
      recommendations.push('Ensure all buttons have proper click handlers and API integration');
    }

    // Dropdown recommendations
    if (reportData.dropdownReport.healthScore < 90) {
      recommendations.push('Verify dropdown data sources and API connectivity');
    }

    // Search recommendations
    if (reportData.searchReport.healthScore < 90) {
      recommendations.push('Fix search functionality to ensure database queries work correctly');
    }

    // Form recommendations
    if (reportData.formReport.healthScore < 90) {
      recommendations.push('Add proper validation and event handlers to all form elements');
    }

    // CRUD recommendations
    if (reportData.crudReport.healthScore < 90) {
      recommendations.push('Ensure all CRUD operations have proper API integration');
    }

    // Overall recommendations
    if (reportData.overallHealthScore < 80) {
      recommendations.push('Consider running the auto-fix utility to resolve common issues');
      recommendations.push('Review API connectivity and ensure backend services are running');
    }

    if (reportData.overallHealthScore < 60) {
      recommendations.push('Critical: Multiple UI elements are not functioning properly');
      recommendations.push('Prioritize fixing navigation and form validation issues');
    }

    return recommendations;
  }

  /**
   * Check if auto-fix is available
   */
  private canAutoFix(reportData: any): boolean {
    // Auto-fix is available if there are common, fixable issues
    const fixableIssues = [
      reportData.buttonReport.brokenElements > 0,
      reportData.formReport.brokenElements > 0,
      reportData.dropdownReport.brokenElements > 0
    ];

    return fixableIssues.some(issue => issue);
  }

  /**
   * Get empty report for error cases
   */
  private getEmptyReport(): UIIntegrationReport {
    const emptyCategory: UIElementCategoryReport = {
      name: '',
      totalElements: 0,
      workingElements: 0,
      brokenElements: 0,
      healthScore: 0,
      issues: [],
      suggestions: []
    };

    return {
      timestamp: new Date().toISOString(),
      overallHealth: 'poor',
      totalElements: 0,
      workingElements: 0,
      brokenElements: 0,
      categories: {
        navigation: { ...emptyCategory, name: 'Navigation' },
        buttons: { ...emptyCategory, name: 'Buttons' },
        dropdowns: { ...emptyCategory, name: 'Dropdowns' },
        search: { ...emptyCategory, name: 'Search' },
        forms: { ...emptyCategory, name: 'Forms' },
        crud: { ...emptyCategory, name: 'CRUD Operations' }
      },
      recommendations: ['Failed to generate report - check system connectivity'],
      autoFixAvailable: false
    };
  }

  /**
   * Auto-fix common UI issues
   */
  autoFixUIIssues(): Observable<boolean> {
    this.notificationService.showInfo('Starting auto-fix process...');

    const fixOperations = [
      this.uiValidator.fixCommonIssues(),
      this.dropdownData.refreshAllDropdowns().pipe(map(() => true)),
      // Add more auto-fix operations as needed
    ];

    return combineLatest(fixOperations).pipe(
      map(results => results.every(result => result)),
      tap((success: any) => {
        if (success) {
          this.notificationService.showSuccess('Auto-fix completed successfully');
        } else {
          this.notificationService.showWarning('Auto-fix completed with some issues');
        }
      }),
      catchError(error => {
        console.error('Auto-fix failed:', error);
        this.notificationService.showError('Auto-fix failed - please check manually');
        return of(false);
      })
    );
  }

  /**
   * Initialize all UI services
   */
  initializeUIServices(): Observable<boolean> {
    try {
      // Initialize services that need setup
      this.notificationService.showInfo('Initializing UI services...');

      // Refresh all dropdowns
      this.dropdownData.refreshAllDropdowns().subscribe({
        next: () => {
          console.log('Dropdowns initialized successfully');
        },
        error: (error) => {
          console.warn('Some dropdowns failed to initialize:', error);
        }
      });

      // Validate navigation
      this.navigationService.validateAllRoutes().subscribe({
        next: (result) => {
          if (!result.valid) {
            console.warn('Navigation validation issues:', result.errors);
          }
        },
        error: (error) => {
          console.warn('Navigation validation failed:', error);
        }
      });

      this.notificationService.showSuccess('UI services initialized successfully');
      return of(true);
    } catch (error) {
      console.error('Failed to initialize UI services:', error);
      this.notificationService.showError('Failed to initialize UI services');
      return of(false);
    }
  }

  /**
   * Get health status for a specific UI category
   */
  getCategoryHealth(category: keyof UIIntegrationReport['categories']): Observable<UIElementCategoryReport> {
    return this.generateIntegrationReport().pipe(
      map(report => report.categories[category])
    );
  }

  /**
   * Test specific UI element
   */
  testUIElement(elementType: string, elementId: string): Observable<boolean> {
    switch (elementType) {
      case 'button':
        const buttonAction = this.buttonHandler.getButtonAction(elementId);
        return of(!!buttonAction);

      case 'dropdown':
        return this.dropdownData.getDropdownData(elementId).pipe(
          map(data => data.length > 0)
        );

      case 'search':
        const searchConfig = this.searchService.getSearchConfig(elementId);
        return of(!!searchConfig);

      case 'form':
        const form = this.formValidation.getForm(elementId);
        return of(!!form);

      case 'crud':
        const crudOperation = this.crudOperations.getCRUDOperation(elementId);
        return of(!!crudOperation);

      default:
        return of(false);
    }
  }

  /**
   * Get UI element suggestions
   */
  getUIElementSuggestions(elementType: string, elementId: string): string[] {
    const suggestions: string[] = [];

    switch (elementType) {
      case 'button':
        suggestions.push('Ensure button has proper click handler');
        suggestions.push('Verify button action is configured correctly');
        suggestions.push('Check if user has required permissions');
        break;

      case 'dropdown':
        suggestions.push('Verify dropdown data source is accessible');
        suggestions.push('Check API endpoint connectivity');
        suggestions.push('Ensure proper error handling for failed data loads');
        break;

      case 'search':
        suggestions.push('Verify search endpoint is configured');
        suggestions.push('Check search parameters and filters');
        suggestions.push('Ensure proper debouncing is implemented');
        break;

      case 'form':
        suggestions.push('Add proper validation to all form fields');
        suggestions.push('Ensure form submission handlers are working');
        suggestions.push('Check for proper error display');
        break;

      case 'crud':
        suggestions.push('Verify CRUD endpoints are accessible');
        suggestions.push('Check HTTP methods are correctly configured');
        suggestions.push('Ensure proper error handling and user feedback');
        break;

      default:
        suggestions.push('Element type not recognized');
    }

    return suggestions;
  }

  /**
   * Export UI integration report
   */
  exportReport(format: 'json' | 'csv' = 'json'): Observable<Blob> {
    return this.generateIntegrationReport().pipe(
      map(report => {
        if (format === 'json') {
          const jsonString = JSON.stringify(report, null, 2);
          return new Blob([jsonString], { type: 'application/json' });
        } else {
          // Convert to CSV format
          const csvContent = this.convertReportToCSV(report);
          return new Blob([csvContent], { type: 'text/csv' });
        }
      })
    );
  }

  /**
   * Convert report to CSV format
   */
  private convertReportToCSV(report: UIIntegrationReport): string {
    const headers = ['Category', 'Total Elements', 'Working Elements', 'Broken Elements', 'Health Score', 'Issues'];
    const rows = Object.values(report.categories).map(category => [
      category.name,
      category.totalElements.toString(),
      category.workingElements.toString(),
      category.brokenElements.toString(),
      category.healthScore.toString() + '%',
      category.issues.join('; ')
    ]);

    const csvContent = [headers, ...rows]
      .map(row => row.map(cell => `"${cell}"`).join(','))
      .join('\n');

    return csvContent;
  }
}