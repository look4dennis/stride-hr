import { Injectable } from '@angular/core';
import { AbstractControl, FormGroup, ValidationErrors, ValidatorFn } from '@angular/forms';

export interface ValidationMessage {
    type: string;
    message: string;
}

export interface FieldValidationConfig {
    required?: boolean;
    minLength?: number;
    maxLength?: number;
    pattern?: string | RegExp;
    email?: boolean;
    min?: number;
    max?: number;
    custom?: ValidatorFn[];
}

@Injectable({
    providedIn: 'root'
})
export class FormValidationService {
    private defaultMessages: { [key: string]: string } = {
        required: 'This field is required',
        email: 'Please enter a valid email address',
        minlength: 'This field must be at least {requiredLength} characters long',
        maxlength: 'This field cannot exceed {requiredLength} characters',
        pattern: 'Please enter a valid format',
        min: 'Value must be at least {min}',
        max: 'Value cannot exceed {max}',
        phone: 'Please enter a valid phone number',
        url: 'Please enter a valid URL',
        date: 'Please enter a valid date',
        time: 'Please enter a valid time',
        number: 'Please enter a valid number',
        integer: 'Please enter a whole number',
        decimal: 'Please enter a valid decimal number',
        password: 'Password must contain at least 8 characters with uppercase, lowercase, and numbers',
        confirmPassword: 'Passwords do not match',
        unique: 'This value already exists',
        custom: 'Invalid value'
    };

    private customMessages: { [key: string]: { [key: string]: string } } = {};

    /**
     * Get validation message for a form control
     */
    getValidationMessage(control: AbstractControl, fieldName?: string): string | null {
        if (!control || !control.errors || !control.touched) {
            return null;
        }

        const errors = control.errors;
        const errorKey = Object.keys(errors)[0];

        // Check for custom field-specific message
        if (fieldName && this.customMessages[fieldName] && this.customMessages[fieldName][errorKey]) {
            return this.interpolateMessage(this.customMessages[fieldName][errorKey], errors[errorKey]);
        }

        // Check for default message
        if (this.defaultMessages[errorKey]) {
            return this.interpolateMessage(this.defaultMessages[errorKey], errors[errorKey]);
        }

        return 'Invalid value';
    }

    /**
     * Get all validation messages for a form
     */
    getFormValidationMessages(form: FormGroup): { [key: string]: string } {
        const messages: { [key: string]: string } = {};

        Object.keys(form.controls).forEach(key => {
            const control = form.get(key);
            if (control) {
                const message = this.getValidationMessage(control, key);
                if (message) {
                    messages[key] = message;
                }
            }
        });

        return messages;
    }

    /**
     * Check if a form field is invalid and should show error
     */
    isFieldInvalid(form: FormGroup, fieldName: string): boolean {
        const field = form.get(fieldName);
        return !!(field && field.invalid && (field.dirty || field.touched));
    }

    /**
     * Check if a form field is valid and should show success
     */
    isFieldValid(form: FormGroup, fieldName: string): boolean {
        const field = form.get(fieldName);
        return !!(field && field.valid && (field.dirty || field.touched));
    }

    /**
     * Mark all form fields as touched to trigger validation display
     */
    markAllFieldsAsTouched(form: FormGroup): void {
        Object.keys(form.controls).forEach(key => {
            const control = form.get(key);
            if (control) {
                control.markAsTouched();

                // Handle nested form groups
                if (control instanceof FormGroup) {
                    this.markAllFieldsAsTouched(control);
                }
            }
        });
    }

    /**
     * Set custom validation messages for specific fields
     */
    setCustomMessages(fieldName: string, messages: { [key: string]: string }): void {
        this.customMessages[fieldName] = messages;
    }

    /**
     * Set default validation message for an error type
     */
    setDefaultMessage(errorType: string, message: string): void {
        this.defaultMessages[errorType] = message;
    }

    /**
     * Custom validators
     */
    static validators = {
        /**
         * Phone number validator
         */
        phone(): ValidatorFn {
            return (control: AbstractControl): ValidationErrors | null => {
                if (!control.value) return null;

                const phoneRegex = /^[\+]?[1-9][\d]{0,15}$/;
                return phoneRegex.test(control.value) ? null : { phone: true };
            };
        },

        /**
         * Strong password validator
         */
        strongPassword(): ValidatorFn {
            return (control: AbstractControl): ValidationErrors | null => {
                if (!control.value) return null;

                const hasUpperCase = /[A-Z]/.test(control.value);
                const hasLowerCase = /[a-z]/.test(control.value);
                const hasNumeric = /[0-9]/.test(control.value);
                const hasMinLength = control.value.length >= 8;

                const valid = hasUpperCase && hasLowerCase && hasNumeric && hasMinLength;
                return valid ? null : { password: true };
            };
        },

        /**
         * Confirm password validator
         */
        confirmPassword(passwordField: string): ValidatorFn {
            return (control: AbstractControl): ValidationErrors | null => {
                if (!control.parent) return null;

                const password = control.parent.get(passwordField);
                if (!password) return null;

                return password.value === control.value ? null : { confirmPassword: true };
            };
        },

        /**
         * URL validator
         */
        url(): ValidatorFn {
            return (control: AbstractControl): ValidationErrors | null => {
                if (!control.value) return null;

                try {
                    new URL(control.value);
                    return null;
                } catch {
                    return { url: true };
                }
            };
        },

        /**
         * Date range validator
         */
        dateRange(minDate?: Date, maxDate?: Date): ValidatorFn {
            return (control: AbstractControl): ValidationErrors | null => {
                if (!control.value) return null;

                const date = new Date(control.value);

                if (minDate && date < minDate) {
                    return { dateMin: { min: minDate, actual: date } };
                }

                if (maxDate && date > maxDate) {
                    return { dateMax: { max: maxDate, actual: date } };
                }

                return null;
            };
        },

        /**
         * Unique value validator (async)
         */
        unique(checkFunction: (value: any) => Promise<boolean>): ValidatorFn {
            return (control: AbstractControl): Promise<ValidationErrors | null> => {
                if (!control.value) return Promise.resolve(null);

                return checkFunction(control.value).then(isUnique => {
                    return isUnique ? null : { unique: true };
                });
            };
        },

        /**
         * File type validator
         */
        fileType(allowedTypes: string[]): ValidatorFn {
            return (control: AbstractControl): ValidationErrors | null => {
                if (!control.value) return null;

                const file = control.value as File;
                if (!file || !file.type) return null;

                const isValid = allowedTypes.some(type => file.type.includes(type));
                return isValid ? null : { fileType: { allowedTypes, actualType: file.type } };
            };
        },

        /**
         * File size validator
         */
        fileSize(maxSizeInMB: number): ValidatorFn {
            return (control: AbstractControl): ValidationErrors | null => {
                if (!control.value) return null;

                const file = control.value as File;
                if (!file || !file.size) return null;

                const maxSizeInBytes = maxSizeInMB * 1024 * 1024;
                return file.size <= maxSizeInBytes ? null : {
                    fileSize: { maxSize: maxSizeInMB, actualSize: Math.round(file.size / 1024 / 1024 * 100) / 100 }
                };
            };
        }
    };

    /**
     * Interpolate message with error details
     */
    private interpolateMessage(message: string, errorDetails: any): string {
        if (!errorDetails || typeof errorDetails !== 'object') {
            return message;
        }

        let interpolated = message;
        Object.keys(errorDetails).forEach(key => {
            const placeholder = `{${key}}`;
            if (interpolated.includes(placeholder)) {
                interpolated = interpolated.replace(placeholder, errorDetails[key]);
            }
        });

        return interpolated;
    }

    /**
     * Get CSS classes for form field based on validation state
     */
    getFieldClasses(form: FormGroup, fieldName: string): string[] {
        const classes: string[] = ['form-control'];

        if (this.isFieldInvalid(form, fieldName)) {
            classes.push('is-invalid');
        } else if (this.isFieldValid(form, fieldName)) {
            classes.push('is-valid');
        }

        return classes;
    }

    /**
     * Validate form and return first error message
     */
    validateFormAndGetFirstError(form: FormGroup): string | null {
        if (form.valid) return null;

        this.markAllFieldsAsTouched(form);
        const messages = this.getFormValidationMessages(form);
        const firstErrorField = Object.keys(messages)[0];

        return firstErrorField ? messages[firstErrorField] : 'Please fix the form errors';
    }

    /**
     * Real-time validation configuration
     */
    configureRealTimeValidation(form: FormGroup, options: {
        debounceTime?: number;
        validateOnChange?: boolean;
        validateOnBlur?: boolean;
    } = {}): void {
        const { debounceTime = 300, validateOnChange = true, validateOnBlur = true } = options;

        Object.keys(form.controls).forEach(key => {
            const control = form.get(key);
            if (control) {
                if (validateOnChange) {
                    control.valueChanges.subscribe(() => {
                        setTimeout(() => {
                            if (control.dirty) {
                                control.markAsTouched();
                            }
                        }, debounceTime);
                    });
                }
            }
        });
    }
}