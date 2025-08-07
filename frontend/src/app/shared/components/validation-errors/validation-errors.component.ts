import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormGroup, FormArray } from '@angular/forms';

export interface ValidationError {
    field: string;
    message: string;
    code?: string;
    severity?: 'error' | 'warning';
}

@Component({
    selector: 'app-validation-errors',
    standalone: true,
    imports: [CommonModule],
    template: `
    <!-- Field-specific validation errors -->
    <div *ngIf="fieldName && fieldErrors.length > 0" class="field-validation-errors">
      <div 
        *ngFor="let error of fieldErrors" 
        class="validation-error"
        [class.validation-warning]="error.severity === 'warning'"
        [class.validation-error-critical]="error.severity === 'error'">
        <i class="fas fa-exclamation-circle me-1" *ngIf="error.severity !== 'warning'"></i>
        <i class="fas fa-exclamation-triangle me-1" *ngIf="error.severity === 'warning'"></i>
        <span>{{ error.message }}</span>
      </div>
    </div>

    <!-- Form-level validation errors -->
    <div *ngIf="!fieldName && formErrors.length > 0" class="form-validation-errors">
      <div class="alert alert-danger" role="alert">
        <h6 class="alert-heading mb-2">
          <i class="fas fa-exclamation-triangle me-2"></i>
          Please correct the following errors:
        </h6>
        <ul class="mb-0">
          <li *ngFor="let error of formErrors">
            <strong>{{ getFieldDisplayName(error.field) }}:</strong> {{ error.message }}
          </li>
        </ul>
      </div>
    </div>

    <!-- Server validation errors -->
    <div *ngIf="serverErrors.length > 0" class="server-validation-errors">
      <div class="alert alert-warning" role="alert">
        <h6 class="alert-heading mb-2">
          <i class="fas fa-server me-2"></i>
          Server Validation Errors:
        </h6>
        <ul class="mb-0">
          <li *ngFor="let error of serverErrors">
            <strong>{{ getFieldDisplayName(error.field) }}:</strong> {{ error.message }}
          </li>
        </ul>
      </div>
    </div>

    <!-- Summary for multiple error types -->
    <div *ngIf="showSummary && totalErrorCount > 0" class="validation-summary">
      <div class="alert alert-info" role="alert">
        <i class="fas fa-info-circle me-2"></i>
        Found {{ totalErrorCount }} validation {{ totalErrorCount === 1 ? 'error' : 'errors' }}.
        Please review and correct the highlighted fields.
      </div>
    </div>
  `,
    styles: [`
    .field-validation-errors {
      margin-top: 0.25rem;
    }

    .validation-error {
      color: #dc3545;
      font-size: 0.875rem;
      display: flex;
      align-items: center;
      margin-bottom: 0.25rem;
    }

    .validation-warning {
      color: #fd7e14;
    }

    .validation-error-critical {
      color: #dc3545;
      font-weight: 500;
    }

    .form-validation-errors {
      margin-bottom: 1rem;
    }

    .server-validation-errors {
      margin-bottom: 1rem;
    }

    .validation-summary {
      margin-bottom: 1rem;
    }

    .alert ul {
      padding-left: 1.25rem;
    }

    .alert li {
      margin-bottom: 0.25rem;
    }

    .alert li:last-child {
      margin-bottom: 0;
    }

    @media (max-width: 576px) {
      .validation-error {
        font-size: 0.8125rem;
      }
      
      .alert {
        font-size: 0.875rem;
      }
    }
  `]
})
export class ValidationErrorsComponent implements OnInit, OnChanges {
    @Input() form?: FormGroup;
    @Input() fieldName?: string;
    @Input() errors: ValidationError[] = [];
    @Input() serverErrors: ValidationError[] = [];
    @Input() showSummary: boolean = false;
    @Input() fieldDisplayNames: { [key: string]: string } = {};

    fieldErrors: ValidationError[] = [];
    formErrors: ValidationError[] = [];
    totalErrorCount = 0;

    ngOnInit(): void {
        this.processErrors();
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['errors'] || changes['serverErrors'] || changes['form'] || changes['fieldName']) {
            this.processErrors();
        }
    }

    /**
     * Process and categorize validation errors
     */
    private processErrors(): void {
        this.fieldErrors = [];
        this.formErrors = [];

        // Process explicit errors passed as input
        if (this.errors.length > 0) {
            if (this.fieldName) {
                // Filter errors for specific field
                this.fieldErrors = this.errors.filter(error =>
                    error.field === this.fieldName || error.field === ''
                );
            } else {
                // All errors are form-level
                this.formErrors = [...this.errors];
            }
        }

        // Process server errors
        if (this.serverErrors.length > 0) {
            if (this.fieldName) {
                // Filter server errors for specific field
                const fieldServerErrors = this.serverErrors.filter(error =>
                    error.field === this.fieldName
                );
                this.fieldErrors = [...this.fieldErrors, ...fieldServerErrors];
            } else {
                // All server errors are form-level
                this.formErrors = [...this.formErrors, ...this.serverErrors];
            }
        }

        // Process Angular form validation errors
        if (this.form) {
            if (this.fieldName) {
                // Get errors for specific field
                const control = this.getFormControl(this.form, this.fieldName);
                if (control && control.errors && (control.dirty || control.touched)) {
                    const controlErrors = this.convertControlErrorsToValidationErrors(
                        control.errors,
                        this.fieldName
                    );
                    this.fieldErrors = [...this.fieldErrors, ...controlErrors];
                }
            } else {
                // Get all form errors
                const allFormErrors = this.getAllFormErrors(this.form);
                this.formErrors = [...this.formErrors, ...allFormErrors];
            }
        }

        // Calculate total error count
        this.totalErrorCount = this.fieldErrors.length + this.formErrors.length;
    }

    /**
     * Get form control by field name (supports nested fields)
     */
    private getFormControl(form: FormGroup, fieldName: string): AbstractControl | null {
        const fieldParts = fieldName.split('.');
        let control: AbstractControl | null = form;

        for (const part of fieldParts) {
            if (control instanceof FormGroup) {
                control = control.get(part);
            } else if (control instanceof FormArray) {
                const index = parseInt(part, 10);
                if (!isNaN(index)) {
                    control = control.at(index);
                } else {
                    return null;
                }
            } else {
                return null;
            }
        }

        return control;
    }

    /**
     * Convert Angular form control errors to ValidationError format
     */
    private convertControlErrorsToValidationErrors(
        controlErrors: any,
        fieldName: string
    ): ValidationError[] {
        const validationErrors: ValidationError[] = [];

        Object.keys(controlErrors).forEach(errorKey => {
            const errorValue = controlErrors[errorKey];
            let message = '';

            switch (errorKey) {
                case 'required':
                    message = `${this.getFieldDisplayName(fieldName)} is required.`;
                    break;
                case 'email':
                    message = `${this.getFieldDisplayName(fieldName)} must be a valid email address.`;
                    break;
                case 'minlength':
                    message = `${this.getFieldDisplayName(fieldName)} must be at least ${errorValue.requiredLength} characters long.`;
                    break;
                case 'maxlength':
                    message = `${this.getFieldDisplayName(fieldName)} cannot exceed ${errorValue.requiredLength} characters.`;
                    break;
                case 'min':
                    message = `${this.getFieldDisplayName(fieldName)} must be at least ${errorValue.min}.`;
                    break;
                case 'max':
                    message = `${this.getFieldDisplayName(fieldName)} cannot exceed ${errorValue.max}.`;
                    break;
                case 'pattern':
                    message = `${this.getFieldDisplayName(fieldName)} format is invalid.`;
                    break;
                case 'custom':
                    message = errorValue.message || `${this.getFieldDisplayName(fieldName)} is invalid.`;
                    break;
                default:
                    message = errorValue.message || `${this.getFieldDisplayName(fieldName)} is invalid.`;
            }

            validationErrors.push({
                field: fieldName,
                message,
                code: errorKey,
                severity: 'error'
            });
        });

        return validationErrors;
    }

    /**
     * Get all form errors recursively
     */
    private getAllFormErrors(form: FormGroup | FormArray): ValidationError[] {
        const errors: ValidationError[] = [];

        Object.keys(form.controls).forEach(key => {
            const control = form.get(key);

            if (control && control.errors && (control.dirty || control.touched)) {
                const controlErrors = this.convertControlErrorsToValidationErrors(
                    control.errors,
                    key
                );
                errors.push(...controlErrors);
            }

            if (control instanceof FormGroup || control instanceof FormArray) {
                const nestedErrors = this.getAllFormErrors(control);
                errors.push(...nestedErrors);
            }
        });

        return errors;
    }

    /**
     * Get display name for field
     */
    getFieldDisplayName(fieldName: string): string {
        if (this.fieldDisplayNames[fieldName]) {
            return this.fieldDisplayNames[fieldName];
        }

        // Convert camelCase or snake_case to readable format
        return fieldName
            .replace(/([A-Z])/g, ' $1')
            .replace(/_/g, ' ')
            .replace(/^\w/, c => c.toUpperCase())
            .trim();
    }

    /**
     * Check if field has errors
     */
    hasFieldErrors(): boolean {
        return this.fieldErrors.length > 0;
    }

    /**
     * Check if form has errors
     */
    hasFormErrors(): boolean {
        return this.formErrors.length > 0;
    }

    /**
     * Check if there are any errors
     */
    hasErrors(): boolean {
        return this.totalErrorCount > 0;
    }

    /**
     * Get error count for specific severity
     */
    getErrorCountBySeverity(severity: 'error' | 'warning'): number {
        const allErrors = [...this.fieldErrors, ...this.formErrors];
        return allErrors.filter(error => error.severity === severity).length;
    }
}