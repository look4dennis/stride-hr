import { Component, inject } from '@angular/core';
import { FormGroup, FormBuilder, AbstractControl, ValidationErrors } from '@angular/forms';
import { Observable } from 'rxjs';
import { BaseComponent } from './base-component';

export interface FormValidationError {
  field: string;
  message: string;
  code?: string;
}

@Component({
  template: ''
})
export abstract class BaseFormComponent<T = any> extends BaseComponent {
  protected readonly formBuilder = inject(FormBuilder);
  
  form!: FormGroup;
  validationErrors: FormValidationError[] = [];
  isSubmitting = false;
  isDirty = false;

  protected override initializeComponent(): void {
    this.form = this.createForm();
    this.setupFormSubscriptions();
  }

  // Abstract methods to be implemented by subclasses
  protected abstract createForm(): FormGroup;
  protected abstract submitForm(data: T): Observable<any>;

  // Form management
  protected getFormData(): T {
    return this.form.value as T;
  }

  protected resetForm(data?: Partial<T>): void {
    this.form.reset(data);
    this.clearValidationErrors();
    this.isDirty = false;
  }

  protected patchForm(data: Partial<T>): void {
    this.form.patchValue(data);
  }

  protected markFormGroupTouched(): void {
    Object.keys(this.form.controls).forEach(key => {
      const control = this.form.get(key);
      control?.markAsTouched();
      
      if (control instanceof FormGroup) {
        this.markFormGroupTouchedRecursive(control);
      }
    });
  }

  private markFormGroupTouchedRecursive(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
      
      if (control instanceof FormGroup) {
        this.markFormGroupTouchedRecursive(control);
      }
    });
  }

  // Validation
  protected validateForm(): boolean {
    this.clearValidationErrors();
    
    if (this.form.invalid) {
      this.markFormGroupTouched();
      this.extractValidationErrors();
      return false;
    }
    
    return true;
  }

  protected isFieldInvalid(fieldName: string): boolean {
    const field = this.form.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  protected getFieldError(fieldName: string): string | null {
    const field = this.form.get(fieldName);
    
    if (field && field.errors && (field.dirty || field.touched)) {
      const errors = field.errors;
      
      if (errors['required']) {
        return `${this.getFieldDisplayName(fieldName)} is required`;
      }
      
      if (errors['email']) {
        return 'Please enter a valid email address';
      }
      
      if (errors['minlength']) {
        return `${this.getFieldDisplayName(fieldName)} must be at least ${errors['minlength'].requiredLength} characters`;
      }
      
      if (errors['maxlength']) {
        return `${this.getFieldDisplayName(fieldName)} cannot exceed ${errors['maxlength'].requiredLength} characters`;
      }
      
      if (errors['pattern']) {
        return `${this.getFieldDisplayName(fieldName)} format is invalid`;
      }
      
      if (errors['min']) {
        return `${this.getFieldDisplayName(fieldName)} must be at least ${errors['min'].min}`;
      }
      
      if (errors['max']) {
        return `${this.getFieldDisplayName(fieldName)} cannot exceed ${errors['max'].max}`;
      }
      
      // Custom error messages
      if (errors['custom']) {
        return errors['custom'];
      }
      
      // Return first error message if available
      const firstErrorKey = Object.keys(errors)[0];
      return errors[firstErrorKey]?.message || `${this.getFieldDisplayName(fieldName)} is invalid`;
    }
    
    return null;
  }

  protected hasFieldError(fieldName: string): boolean {
    return this.getFieldError(fieldName) !== null;
  }

  // Form submission
  onSubmit(): void {
    if (this.isSubmitting) {
      return;
    }

    if (!this.validateForm()) {
      this.showWarning('Please correct the form errors before submitting');
      return;
    }

    this.isSubmitting = true;
    this.showLoading('Submitting form...');

    const formData = this.getFormData();
    
    this.submitForm(formData).subscribe({
      next: (result) => {
        this.isSubmitting = false;
        this.hideLoading();
        this.onSubmitSuccess(result);
      },
      error: (error) => {
        this.isSubmitting = false;
        this.hideLoading();
        this.onSubmitError(error);
      }
    });
  }

  // Override these methods in subclasses for custom behavior
  protected onSubmitSuccess(result: any): void {
    this.showSuccess('Form submitted successfully');
    this.resetForm();
  }

  protected onSubmitError(error: any): void {
    this.handleError(error, 'Failed to submit form');
    
    // Handle server-side validation errors
    if (error?.error?.errors) {
      this.handleServerValidationErrors(error.error.errors);
    }
  }

  // Validation error handling
  private clearValidationErrors(): void {
    this.validationErrors = [];
  }

  private extractValidationErrors(): void {
    this.validationErrors = [];
    
    Object.keys(this.form.controls).forEach(key => {
      const control = this.form.get(key);
      if (control && control.errors) {
        const errorMessage = this.getFieldError(key);
        if (errorMessage) {
          this.validationErrors.push({
            field: key,
            message: errorMessage
          });
        }
      }
    });
  }

  private handleServerValidationErrors(errors: any): void {
    if (Array.isArray(errors)) {
      // Handle array of validation errors
      this.validationErrors = errors.map(error => ({
        field: error.field || 'general',
        message: error.message,
        code: error.code
      }));
    } else if (typeof errors === 'object') {
      // Handle object with field-specific errors
      this.validationErrors = [];
      Object.keys(errors).forEach(field => {
        const fieldErrors = Array.isArray(errors[field]) ? errors[field] : [errors[field]];
        fieldErrors.forEach((message: string) => {
          this.validationErrors.push({
            field,
            message
          });
        });
        
        // Set form control errors
        const control = this.form.get(field);
        if (control) {
          control.setErrors({ server: fieldErrors[0] });
        }
      });
    }
  }

  // Utility methods
  private getFieldDisplayName(fieldName: string): string {
    // Convert camelCase to Title Case
    return fieldName
      .replace(/([A-Z])/g, ' $1')
      .replace(/^./, str => str.toUpperCase())
      .trim();
  }

  private setupFormSubscriptions(): void {
    // Track form dirty state
    this.form.valueChanges.subscribe(() => {
      this.isDirty = this.form.dirty;
    });

    // Clear server errors when user starts typing
    this.form.valueChanges.subscribe(() => {
      Object.keys(this.form.controls).forEach(key => {
        const control = this.form.get(key);
        if (control && control.errors && control.errors['server']) {
          const errors = { ...control.errors };
          delete errors['server'];
          
          if (Object.keys(errors).length === 0) {
            control.setErrors(null);
          } else {
            control.setErrors(errors);
          }
        }
      });
    });
  }

  // File handling utilities
  protected handleFileSelect(event: Event, fieldName: string): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.form.patchValue({ [fieldName]: file });
      this.form.get(fieldName)?.markAsDirty();
    }
  }

  protected removeFile(fieldName: string): void {
    this.form.patchValue({ [fieldName]: null });
    this.form.get(fieldName)?.markAsDirty();
  }

  // Custom validators
  protected static emailValidator(control: AbstractControl): ValidationErrors | null {
    const email = control.value;
    if (!email) return null;
    
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email) ? null : { email: true };
  }

  protected static phoneValidator(control: AbstractControl): ValidationErrors | null {
    const phone = control.value;
    if (!phone) return null;
    
    const phoneRegex = /^[\+]?[1-9][\d]{0,15}$/;
    return phoneRegex.test(phone.replace(/[\s\-\(\)]/g, '')) ? null : { phone: true };
  }

  protected static passwordValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.value;
    if (!password) return null;
    
    const hasMinLength = password.length >= 8;
    const hasUpperCase = /[A-Z]/.test(password);
    const hasLowerCase = /[a-z]/.test(password);
    const hasNumbers = /\d/.test(password);
    const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(password);
    
    const isValid = hasMinLength && hasUpperCase && hasLowerCase && hasNumbers && hasSpecialChar;
    
    if (!isValid) {
      return {
        password: {
          message: 'Password must be at least 8 characters long and contain uppercase, lowercase, number, and special character'
        }
      };
    }
    
    return null;
  }
}