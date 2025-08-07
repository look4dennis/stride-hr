import { Injectable } from '@angular/core';
import { FormGroup, FormControl, AbstractControl } from '@angular/forms';

@Injectable({
  providedIn: 'root'
})
export class FormValidationService {

  constructor() {}

  isFieldInvalid(form: FormGroup, fieldName: string): boolean {
    const field = form.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getValidationMessage(control: AbstractControl, fieldName: string): string | null {
    if (!control || !control.errors) {
      return null;
    }

    const errors = control.errors;
    const fieldDisplayName = this.getFieldDisplayName(fieldName);

    if (errors['required']) {
      return `${fieldDisplayName} is required`;
    }

    if (errors['email']) {
      return 'Please enter a valid email address';
    }

    if (errors['minlength']) {
      const requiredLength = errors['minlength'].requiredLength;
      return `${fieldDisplayName} must be at least ${requiredLength} characters long`;
    }

    if (errors['maxlength']) {
      const requiredLength = errors['maxlength'].requiredLength;
      return `${fieldDisplayName} cannot exceed ${requiredLength} characters`;
    }

    if (errors['min']) {
      const minValue = errors['min'].min;
      return `${fieldDisplayName} must be at least ${minValue}`;
    }

    if (errors['max']) {
      const maxValue = errors['max'].max;
      return `${fieldDisplayName} cannot exceed ${maxValue}`;
    }

    if (errors['pattern']) {
      return `${fieldDisplayName} format is invalid`;
    }

    if (errors['phone']) {
      return 'Please enter a valid phone number';
    }

    if (errors['date']) {
      return 'Please enter a valid date';
    }

    if (errors['passwordMismatch']) {
      return 'Passwords do not match';
    }

    if (errors['uniqueEmail']) {
      return 'This email address is already in use';
    }

    if (errors['uniqueEmployeeId']) {
      return 'This employee ID is already in use';
    }

    // Generic error message for unknown validation errors
    return `${fieldDisplayName} is invalid`;
  }

  validateFormAndGetFirstError(form: FormGroup): string | null {
    if (form.valid) {
      return null;
    }

    // Mark all fields as touched to show validation errors
    this.markFormGroupTouched(form);

    // Find the first invalid field and return its error message
    for (const fieldName in form.controls) {
      const control = form.get(fieldName);
      if (control && control.invalid) {
        return this.getValidationMessage(control, fieldName);
      }
    }

    return 'Please correct the errors in the form';
  }

  markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      if (control) {
        control.markAsTouched();

        if (control instanceof FormGroup) {
          this.markFormGroupTouched(control);
        }
      }
    });
  }

  resetFormValidation(form: FormGroup): void {
    form.markAsUntouched();
    form.markAsPristine();
    
    Object.keys(form.controls).forEach(key => {
      const control = form.get(key);
      if (control) {
        control.markAsUntouched();
        control.markAsPristine();
        
        if (control instanceof FormGroup) {
          this.resetFormValidation(control);
        }
      }
    });
  }

  private getFieldDisplayName(fieldName: string): string {
    // Convert camelCase to readable format
    const displayName = fieldName
      .replace(/([A-Z])/g, ' $1')
      .replace(/^./, str => str.toUpperCase())
      .trim();

    // Handle special cases
    const specialCases: { [key: string]: string } = {
      'firstName': 'First Name',
      'lastName': 'Last Name',
      'dateOfBirth': 'Date of Birth',
      'joiningDate': 'Joining Date',
      'basicSalary': 'Basic Salary',
      'reportingManagerId': 'Reporting Manager',
      'branchId': 'Branch',
      'employeeId': 'Employee ID',
      'profilePhoto': 'Profile Photo'
    };

    return specialCases[fieldName] || displayName;
  }

  // Custom validators
  static phoneValidator(control: AbstractControl): { [key: string]: any } | null {
    if (!control.value) {
      return null;
    }

    const phoneRegex = /^[\+]?[1-9][\d]{0,15}$/;
    const valid = phoneRegex.test(control.value);
    return valid ? null : { phone: true };
  }

  static dateValidator(control: AbstractControl): { [key: string]: any } | null {
    if (!control.value) {
      return null;
    }

    const date = new Date(control.value);
    const valid = date instanceof Date && !isNaN(date.getTime());
    return valid ? null : { date: true };
  }

  static futureDateValidator(control: AbstractControl): { [key: string]: any } | null {
    if (!control.value) {
      return null;
    }

    const inputDate = new Date(control.value);
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return inputDate > today ? { futureDate: true } : null;
  }

  static pastDateValidator(control: AbstractControl): { [key: string]: any } | null {
    if (!control.value) {
      return null;
    }

    const inputDate = new Date(control.value);
    const today = new Date();
    today.setHours(23, 59, 59, 999);

    return inputDate < today ? null : { pastDate: true };
  }
}