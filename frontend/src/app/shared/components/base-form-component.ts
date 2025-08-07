import { Component, OnInit, inject } from '@angular/core';
import { FormGroup, FormBuilder, AbstractControl } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { BaseComponent } from './base-component';
import { ValidationError } from './validation-errors/validation-errors.component';

export interface FormValidationConfig {
  showInlineErrors?: boolean;
  showSummaryErrors?: boolean;
  validateOnSubmit?: boolean;
  validateOnChange?: boolean;
  fieldDisplayNames?: { [key: string]: string };
}

export interface FormSubmissionResult<T = any> {
  success: boolean;
  data?: T;
  errors?: any;
  message?: string;
}

@Component({
  template: ''
})
export abstract class BaseFormComponent<T = any> extends BaseComponent implements OnInit {
  protected readonly formBuilder = inject(FormBuilder);

  form!: FormGroup;
  validationErrors: { [key: string]: string } = {};
  serverErrors: ValidationError[] = [];
  isSubmitting = false;
  hasBeenSubmitted = false;
  
  protected validationConfig: FormValidationConfig = {
    showInlineErrors: true,
    showSummaryErrors: true,
    validateOnSubmit: true,
    validateOnChange: false,
    fieldDisplayNames: {}
  };

  override ngOnInit(): void {
    super.ngOnInit();
    this.form = this.createForm();
    this.setupFormValidation();
  }

  // Abstract methods to be implemented by derived classes
  protected abstract createForm(): FormGroup;
  protected abstract submitForm(data: T): Observable<FormSubmissionResult<T>>;

  // Optional method for custom form initialization
  protected initializeFormData(): void {
    // Override in derived classes if needed
  }

  // Form validation setup
  private setupFormValidation(): void {
    if (this.validationConfig.validateOnChange) {
      this.form.valueChanges.subscribe(() => {
        if (this.hasBeenSubmitted) {
          this.validateForm();
        }
      });
    }

    // Clear server errors when form values change
    this.form.valueChanges.subscribe(() => {
      if (this.serverErrors.length > 0) {
        this.serverErrors = [];
      }
    });
  }

  // Form submission handling
  onSubmit(): void {
    if (this.isSubmitting) {
      return;
    }

    this.hasBeenSubmitted = true;
    this.clearErrors();

    if (this.validationConfig.validateOnSubmit && !this.validateForm()) {
      this.showWarning('Please correct the validation errors before submitting.');
      return;
    }

    if (!this.isOnline) {
      this.showWarning('You are currently offline. The form will be submitted when connection is restored.');
      // In a real implementation, you might queue the form submission
      return;
    }

    this.isSubmitting = true;
    this.showLoading('Submitting form...');

    const formData = this.getFormData();

    this.submitForm(formData).pipe(
      catchError(error => {
        this.handleSubmissionError(error);
        return of({ success: false, errors: error });
      }),
      finalize(() => {
        this.isSubmitting = false;
        this.hideLoading();
      })
    ).subscribe(result => {
      if (result.success) {
        this.handleSubmissionSuccess(result);
      } else {
        this.handleSubmissionError(result.errors);
      }
    });
  }

  // Form validation
  protected validateForm(): boolean {
    this.validationErrors = {};

    if (!this.form.valid) {
      this.validationErrors = this.extractFormValidationErrors();
      return false;
    }

    return true;
  }

  // Extract validation errors from form
  private extractFormValidationErrors(): { [key: string]: string } {
    const errors: { [key: string]: string } = {};

    Object.keys(this.form.controls).forEach(key => {
      const control = this.form.get(key);
      if (control && control.errors && (control.dirty || control.touched || this.hasBeenSubmitted)) {
        errors[key] = this.getControlErrorMessage(key, control);
      }
    });

    return errors;
  }

  // Get error message for a specific control
  private getControlErrorMessage(fieldName: string, control: AbstractControl): string {
    const errors = control.errors;
    if (!errors) {
      return '';
    }

    const displayName = this.getFieldDisplayName(fieldName);

    if (errors['required']) {
      return `${displayName} is required.`;
    }
    if (errors['email']) {
      return `${displayName} must be a valid email address.`;
    }
    if (errors['minlength']) {
      return `${displayName} must be at least ${errors['minlength'].requiredLength} characters long.`;
    }
    if (errors['maxlength']) {
      return `${displayName} cannot exceed ${errors['maxlength'].requiredLength} characters.`;
    }
    if (errors['min']) {
      return `${displayName} must be at least ${errors['min'].min}.`;
    }
    if (errors['max']) {
      return `${displayName} cannot exceed ${errors['max'].max}.`;
    }
    if (errors['pattern']) {
      return `${displayName} format is invalid.`;
    }
    if (errors['custom']) {
      return errors['custom'].message || `${displayName} is invalid.`;
    }

    // Default error message
    return `${displayName} is invalid.`;
  }

  // Get display name for field
  protected getFieldDisplayName(fieldName: string): string {
    if (this.validationConfig.fieldDisplayNames?.[fieldName]) {
      return this.validationConfig.fieldDisplayNames[fieldName];
    }

    // Convert camelCase to readable format
    return fieldName
      .replace(/([A-Z])/g, ' $1')
      .replace(/^./, str => str.toUpperCase())
      .trim();
  }

  // Get form data
  protected getFormData(): T {
    return this.form.value as T;
  }

  // Reset form
  protected resetForm(data?: Partial<T>): void {
    this.form.reset(data);
    this.clearErrors();
    this.hasBeenSubmitted = false;
    this.isSubmitting = false;
  }

  // Clear all errors
  protected clearErrors(): void {
    this.validationErrors = {};
    this.serverErrors = [];
    this.clearError();
  }

  // Handle successful form submission
  protected handleSubmissionSuccess(result: FormSubmissionResult<T>): void {
    this.showSuccess(result.message || 'Form submitted successfully!');
    
    // Override in derived classes for custom success handling
    this.onSubmissionSuccess(result);
  }

  // Handle form submission errors
  protected handleSubmissionError(error: any): void {
    if (error?.status === 422 || error?.status === 400) {
      // Handle validation errors from server
      const validationErrors = this.handleValidationErrors(error.error || error);
      this.serverErrors = Object.keys(validationErrors).map(field => ({
        field,
        message: validationErrors[field],
        severity: 'error' as const
      }));
      this.showWarning('Please correct the validation errors and try again.');
    } else {
      // Handle other errors
      this.handleError(error, 'Failed to submit form. Please try again.');
    }

    // Override in derived classes for custom error handling
    this.onSubmissionError(error);
  }

  // Check if field has errors
  hasFieldError(fieldName: string): boolean {
    return !!(this.validationErrors[fieldName] || 
             this.serverErrors.find(err => err.field === fieldName));
  }

  // Get field error message
  getFieldError(fieldName: string): string {
    if (this.validationErrors[fieldName]) {
      return this.validationErrors[fieldName];
    }

    const serverError = this.serverErrors.find(err => err.field === fieldName);
    return serverError?.message || '';
  }

  // Check if form is valid
  isFormValid(): boolean {
    return this.form.valid && Object.keys(this.validationErrors).length === 0;
  }

  // Check if form is dirty
  isFormDirty(): boolean {
    return this.form.dirty;
  }

  // Check if form can be submitted
  canSubmit(): boolean {
    return !this.isSubmitting && this.isOnline && this.isFormValid();
  }

  // Mark all fields as touched (useful for showing validation errors)
  markAllFieldsAsTouched(): void {
    Object.keys(this.form.controls).forEach(key => {
      const control = this.form.get(key);
      if (control) {
        control.markAsTouched();
      }
    });
  }

  // Set field value with validation
  setFieldValue(fieldName: string, value: any, options?: { emitEvent?: boolean }): void {
    const control = this.form.get(fieldName);
    if (control) {
      control.setValue(value, options);
      
      if (this.hasBeenSubmitted && this.validationConfig.validateOnChange) {
        control.updateValueAndValidity();
      }
    }
  }

  // Get field value
  getFieldValue(fieldName: string): any {
    return this.form.get(fieldName)?.value;
  }

  // Disable/enable form
  setFormEnabled(enabled: boolean): void {
    if (enabled) {
      this.form.enable();
    } else {
      this.form.disable();
    }
  }

  // Override points for derived classes
  protected onSubmissionSuccess(result: FormSubmissionResult<T>): void {
    // Override in derived classes
  }

  protected onSubmissionError(error: any): void {
    // Override in derived classes
  }

  // Initialize component (required by BaseComponent)
  protected initializeComponent(): void {
    this.initializeFormData();
  }
}