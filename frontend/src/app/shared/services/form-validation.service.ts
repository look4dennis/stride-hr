import { Injectable } from '@angular/core';
import { AbstractControl, FormGroup, ValidationErrors } from '@angular/forms';
import { Observable, of } from 'rxjs';

export interface FormValidationResult {
  formId: string;
  isValid: boolean;
  hasEventHandlers: boolean;
  hasProperValidation: boolean;
  fieldCount: number;
  errorCount: number;
  errors: { [key: string]: string };
  suggestions?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class FormValidationService {
  private registeredForms = new Map<string, FormGroup>();

  constructor() { }

  /**
   * Get validation message for a form control
   */
  getValidationMessage(control: AbstractControl): string | null {
    if (!control || !control.errors || !control.touched) {
      return null;
    }

    const errors = control.errors;

    // Required validation
    if (errors['required']) {
      return 'This field is required';
    }

    // Email validation
    if (errors['email']) {
      return 'Please enter a valid email address';
    }

    // Pattern validation
    if (errors['pattern']) {
      return 'Please enter a valid format';
    }

    // Min length validation
    if (errors['minlength']) {
      const requiredLength = errors['minlength'].requiredLength;
      const actualLength = errors['minlength'].actualLength;
      return `Minimum length is ${requiredLength} characters (current: ${actualLength})`;
    }

    // Max length validation
    if (errors['maxlength']) {
      const requiredLength = errors['maxlength'].requiredLength;
      const actualLength = errors['maxlength'].actualLength;
      return `Maximum length is ${requiredLength} characters (current: ${actualLength})`;
    }

    // Min value validation
    if (errors['min']) {
      return `Minimum value is ${errors['min'].min}`;
    }

    // Max value validation
    if (errors['max']) {
      return `Maximum value is ${errors['max'].max}`;
    }

    // Custom validation messages
    if (errors['custom']) {
      return errors['custom'];
    }

    // Phone validation
    if (errors['phone']) {
      return 'Please enter a valid phone number';
    }

    // Password validation
    if (errors['password']) {
      return errors['password'].message || 'Password does not meet requirements';
    }

    // Confirm password validation
    if (errors['confirmPassword']) {
      return 'Passwords do not match';
    }

    // URL validation
    if (errors['url']) {
      return 'Please enter a valid URL';
    }

    // Date validation
    if (errors['date']) {
      return 'Please enter a valid date';
    }

    // Number validation
    if (errors['number']) {
      return 'Please enter a valid number';
    }

    // File validation
    if (errors['fileSize']) {
      return `File size must be less than ${errors['fileSize'].maxSize}`;
    }

    if (errors['fileType']) {
      return `File type must be one of: ${errors['fileType'].allowedTypes.join(', ')}`;
    }

    // Return first error message if available
    const firstErrorKey = Object.keys(errors)[0];
    const firstError = errors[firstErrorKey];
    
    if (typeof firstError === 'string') {
      return firstError;
    }
    
    if (firstError && firstError.message) {
      return firstError.message;
    }

    return 'This field is invalid';
  }

  /**
   * Check if a field is invalid and should show error
   */
  isFieldInvalid(form: FormGroup, fieldName: string): boolean {
    const field = form.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  /**
   * Check if a field is valid and should show success
   */
  isFieldValid(form: FormGroup, fieldName: string): boolean {
    const field = form.get(fieldName);
    return !!(field && field.valid && (field.dirty || field.touched) && field.value);
  }

  /**
   * Get all validation errors for a form
   */
  getFormErrors(form: FormGroup): { [key: string]: string } {
    const errors: { [key: string]: string } = {};

    Object.keys(form.controls).forEach(key => {
      const control = form.get(key);
      if (control && control.invalid && (control.dirty || control.touched)) {
        const message = this.getValidationMessage(control);
        if (message) {
          errors[key] = message;
        }
      }
    });

    return errors;
  }

  /**
   * Mark all fields in a form as touched
   */
  markAllFieldsAsTouched(form: FormGroup): void {
    Object.keys(form.controls).forEach(key => {
      const control = form.get(key);
      if (control) {
        control.markAsTouched();
        
        if (control instanceof FormGroup) {
          this.markAllFieldsAsTouched(control);
        }
      }
    });
  }

  /**
   * Clear all validation errors from a form
   */
  clearFormErrors(form: FormGroup): void {
    Object.keys(form.controls).forEach(key => {
      const control = form.get(key);
      if (control) {
        control.setErrors(null);
        control.markAsUntouched();
        control.markAsPristine();
      }
    });
  }

  /**
   * Register a form for validation tracking
   */
  registerForm(formId: string, form: FormGroup): void {
    this.registeredForms.set(formId, form);
  }

  /**
   * Get form by ID
   */
  getForm(formId: string): FormGroup | undefined {
    return this.registeredForms.get(formId);
  }

  /**
   * Validate entire form and return first error message
   */
  validateFormAndGetFirstError(form: FormGroup): string | null {
    if (form.valid) {
      return null;
    }

    // Mark all fields as touched to show validation errors
    this.markAllFieldsAsTouched(form);

    // Find first invalid field and return its error message
    for (const fieldName in form.controls) {
      const field = form.get(fieldName);
      if (field && field.invalid) {
        return this.getValidationMessage(field);
      }
    }

    return 'Please correct the errors in the form';
  }

  /**
   * Clear all validation errors from form
   */
  clearValidationErrors(form: FormGroup): void {
    this.clearFormErrors(form);
  }

  /**
   * Get all validation errors from form
   */
  getAllValidationErrors(form: FormGroup): { [key: string]: string } {
    return this.getFormErrors(form);
  }

  /**
   * Phone number validator
   */
  phoneNumberValidator(control: AbstractControl): ValidationErrors | null {
    return FormValidationService.phoneValidator(control);
  }

  /**
   * Validate all registered forms
   */
  validateAllForms(): Observable<FormValidationResult[]> {
    const results: FormValidationResult[] = [];

    this.registeredForms.forEach((form, formId) => {
      const errors = this.getAllValidationErrors(form);
      const fieldCount = Object.keys(form.controls).length;
      const errorCount = Object.keys(errors).length;

      const result: FormValidationResult = {
        formId,
        isValid: form.valid,
        hasEventHandlers: true, // Assume forms have proper event handlers
        hasProperValidation: true, // Assume forms have proper validation
        fieldCount,
        errorCount,
        errors,
        suggestions: errorCount > 0 ? ['Fix validation errors in form fields'] : undefined
      };

      results.push(result);
    });

    return of(results);
  }

  /**
   * Set server validation errors on form controls
   */
  setServerErrors(form: FormGroup, serverErrors: { [key: string]: string[] | string }): void {
    Object.keys(serverErrors).forEach(fieldName => {
      const control = form.get(fieldName);
      if (control) {
        const errors = serverErrors[fieldName];
        const errorMessage = Array.isArray(errors) ? errors[0] : errors;
        control.setErrors({ server: errorMessage });
        control.markAsTouched();
      }
    });
  }

  /**
   * Custom validators
   */
  static emailValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(control.value) ? null : { email: true };
  }

  static phoneValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    
    const phoneRegex = /^[\+]?[1-9][\d]{0,15}$/;
    const cleanPhone = control.value.replace(/[\s\-\(\)]/g, '');
    return phoneRegex.test(cleanPhone) ? null : { phone: true };
  }

  static passwordValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    
    const password = control.value;
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

  static confirmPasswordValidator(passwordField: string) {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.parent) return null;
      
      const password = control.parent.get(passwordField);
      const confirmPassword = control;
      
      if (!password || !confirmPassword) return null;
      
      if (password.value !== confirmPassword.value) {
        return { confirmPassword: true };
      }
      
      return null;
    };
  }

  static urlValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    
    try {
      new URL(control.value);
      return null;
    } catch {
      return { url: true };
    }
  }

  static fileSizeValidator(maxSizeInMB: number) {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value || !(control.value instanceof File)) return null;
      
      const file = control.value as File;
      const maxSizeInBytes = maxSizeInMB * 1024 * 1024;
      
      if (file.size > maxSizeInBytes) {
        return {
          fileSize: {
            maxSize: `${maxSizeInMB}MB`,
            actualSize: `${(file.size / 1024 / 1024).toFixed(2)}MB`
          }
        };
      }
      
      return null;
    };
  }

  static fileTypeValidator(allowedTypes: string[]) {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value || !(control.value instanceof File)) return null;
      
      const file = control.value as File;
      const fileExtension = file.name.split('.').pop()?.toLowerCase();
      
      if (!fileExtension || !allowedTypes.includes(fileExtension)) {
        return {
          fileType: {
            allowedTypes,
            actualType: fileExtension
          }
        };
      }
      
      return null;
    };
  }

  static numberValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    
    const value = parseFloat(control.value);
    if (isNaN(value)) {
      return { number: true };
    }
    
    return null;
  }

  static dateValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    
    const date = new Date(control.value);
    if (isNaN(date.getTime())) {
      return { date: true };
    }
    
    return null;
  }

  static minDateValidator(minDate: Date) {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      
      const date = new Date(control.value);
      if (date < minDate) {
        return {
          minDate: {
            minDate: minDate.toISOString().split('T')[0],
            actualDate: date.toISOString().split('T')[0]
          }
        };
      }
      
      return null;
    };
  }

  static maxDateValidator(maxDate: Date) {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      
      const date = new Date(control.value);
      if (date > maxDate) {
        return {
          maxDate: {
            maxDate: maxDate.toISOString().split('T')[0],
            actualDate: date.toISOString().split('T')[0]
          }
        };
      }
      
      return null;
    };
  }
}