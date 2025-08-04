import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

export interface MobileFormField {
  name: string;
  label: string;
  type: 'text' | 'email' | 'password' | 'number' | 'tel' | 'select' | 'textarea' | 'date' | 'time';
  placeholder?: string;
  required?: boolean;
  options?: { value: any; label: string }[];
  rows?: number;
  validation?: {
    pattern?: string;
    min?: number;
    max?: number;
    minLength?: number;
    maxLength?: number;
  };
}

@Component({
  selector: 'app-mobile-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <form [formGroup]="formGroup" (ngSubmit)="onSubmit()" class="mobile-form">
      <div class="form-field" *ngFor="let field of fields">
        <label class="form-label" [for]="field.name">
          {{ field.label }}
          <span class="text-danger" *ngIf="field.required">*</span>
        </label>
        
        <!-- Text inputs -->
        <input 
          *ngIf="['text', 'email', 'password', 'number', 'tel', 'date', 'time'].includes(field.type)"
          [type]="field.type"
          [id]="field.name"
          [formControlName]="field.name"
          class="form-control mobile-input"
          [placeholder]="field.placeholder || ''"
          [required]="field.required || false"
          [pattern]="field.validation?.pattern"
          [min]="field.validation?.min"
          [max]="field.validation?.max"
          [minlength]="field.validation?.minLength"
          [maxlength]="field.validation?.maxLength">
        
        <!-- Select dropdown -->
        <select 
          *ngIf="field.type === 'select'"
          [id]="field.name"
          [formControlName]="field.name"
          class="form-select mobile-select"
          [required]="field.required || false">
          <option value="">{{ field.placeholder || 'Select an option' }}</option>
          <option 
            *ngFor="let option of field.options" 
            [value]="option.value">
            {{ option.label }}
          </option>
        </select>
        
        <!-- Textarea -->
        <textarea 
          *ngIf="field.type === 'textarea'"
          [id]="field.name"
          [formControlName]="field.name"
          class="form-control mobile-textarea"
          [placeholder]="field.placeholder || ''"
          [required]="field.required || false"
          [rows]="field.rows || 3"
          [minlength]="field.validation?.minLength"
          [maxlength]="field.validation?.maxLength">
        </textarea>
        
        <!-- Validation errors -->
        <div class="invalid-feedback" *ngIf="getFieldError(field.name)">
          {{ getFieldError(field.name) }}
        </div>
      </div>
      
      <div class="form-actions">
        <button 
          type="button" 
          class="btn btn-outline-secondary mobile-btn"
          (click)="onCancel()"
          *ngIf="showCancelButton">
          {{ cancelButtonText }}
        </button>
        <button 
          type="submit" 
          class="btn btn-primary mobile-btn"
          [disabled]="formGroup.invalid || isSubmitting">
          <span *ngIf="!isSubmitting">{{ submitButtonText }}</span>
          <span *ngIf="isSubmitting">
            <span class="spinner-border spinner-border-sm me-2"></span>
            {{ submittingText }}
          </span>
        </button>
      </div>
    </form>
  `,
  styles: [`
    .mobile-form {
      width: 100%;
    }

    .form-field {
      margin-bottom: 1.5rem;
    }

    .form-label {
      font-weight: 500;
      color: var(--text-primary);
      margin-bottom: 0.5rem;
      display: block;
      font-size: 0.95rem;
    }

    .mobile-input,
    .mobile-select,
    .mobile-textarea {
      width: 100%;
      min-height: 48px;
      font-size: 16px; /* Prevent zoom on iOS */
      padding: 0.75rem 1rem;
      border: 2px solid var(--gray-200);
      border-radius: 12px;
      transition: all 0.2s ease;
      background-color: white;
    }

    .mobile-input:focus,
    .mobile-select:focus,
    .mobile-textarea:focus {
      border-color: var(--primary);
      box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
      outline: none;
    }

    .mobile-textarea {
      min-height: auto;
      resize: vertical;
    }

    .form-actions {
      display: flex;
      gap: 1rem;
      margin-top: 2rem;
      flex-direction: column;
    }

    .mobile-btn {
      min-height: 48px;
      font-size: 1rem;
      font-weight: 500;
      border-radius: 12px;
      padding: 0.75rem 1.5rem;
      transition: all 0.2s ease;
      touch-action: manipulation;
      -webkit-tap-highlight-color: rgba(0, 0, 0, 0.1);
    }

    .mobile-btn:active {
      transform: scale(0.98);
    }

    .invalid-feedback {
      display: block;
      color: var(--danger);
      font-size: 0.875rem;
      margin-top: 0.25rem;
    }

    .form-control.is-invalid,
    .form-select.is-invalid {
      border-color: var(--danger);
    }

    /* Tablet and desktop improvements */
    @media (min-width: 768px) {
      .form-actions {
        flex-direction: row;
        justify-content: flex-end;
      }
      
      .mobile-btn {
        width: auto;
        min-width: 120px;
      }
    }

    /* Enhanced touch targets for small screens */
    @media (max-width: 576px) {
      .form-field {
        margin-bottom: 1.25rem;
      }
      
      .mobile-input,
      .mobile-select,
      .mobile-textarea {
        min-height: 52px;
        font-size: 16px;
        padding: 0.875rem 1rem;
      }
      
      .mobile-btn {
        min-height: 52px;
        font-size: 1.05rem;
      }
    }

    /* Improved accessibility */
    .form-label:focus-within {
      color: var(--primary);
    }

    .mobile-input:disabled,
    .mobile-select:disabled,
    .mobile-textarea:disabled {
      background-color: var(--gray-100);
      color: var(--gray-500);
      cursor: not-allowed;
    }
  `]
})
export class MobileFormComponent {
  @Input() fields: MobileFormField[] = [];
  @Input() formGroup: any;
  @Input() submitButtonText: string = 'Submit';
  @Input() cancelButtonText: string = 'Cancel';
  @Input() submittingText: string = 'Submitting...';
  @Input() showCancelButton: boolean = true;
  @Input() isSubmitting: boolean = false;
  
  @Output() formSubmit = new EventEmitter<any>();
  @Output() formCancel = new EventEmitter<void>();

  onSubmit(): void {
    if (this.formGroup.valid) {
      this.formSubmit.emit(this.formGroup.value);
    } else {
      this.markAllFieldsAsTouched();
    }
  }

  onCancel(): void {
    this.formCancel.emit();
  }

  getFieldError(fieldName: string): string | null {
    const field = this.formGroup.get(fieldName);
    if (field && field.invalid && (field.dirty || field.touched)) {
      const errors = field.errors;
      if (errors) {
        if (errors['required']) return 'This field is required';
        if (errors['email']) return 'Please enter a valid email address';
        if (errors['pattern']) return 'Please enter a valid format';
        if (errors['minlength']) return `Minimum length is ${errors['minlength'].requiredLength}`;
        if (errors['maxlength']) return `Maximum length is ${errors['maxlength'].requiredLength}`;
        if (errors['min']) return `Minimum value is ${errors['min'].min}`;
        if (errors['max']) return `Maximum value is ${errors['max'].max}`;
      }
    }
    return null;
  }

  private markAllFieldsAsTouched(): void {
    Object.keys(this.formGroup.controls).forEach(key => {
      this.formGroup.get(key)?.markAsTouched();
    });
  }
}