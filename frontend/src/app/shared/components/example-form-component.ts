import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, Validators } from '@angular/forms';
import { Observable, of, throwError } from 'rxjs';
import { delay } from 'rxjs/operators';
import { BaseFormComponent } from './base-form-component';

interface ExampleFormData {
  name: string;
  email: string;
  age: number;
  description?: string;
}

@Component({
  selector: 'app-example-form-component',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h5>Base Form Component Example</h5>
      </div>
      <div class="card-body">
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
          <div class="mb-3">
            <label class="form-label">Name <span class="text-danger">*</span></label>
            <input 
              type="text" 
              class="form-control" 
              formControlName="name"
              [class.is-invalid]="isFieldInvalid('name')"
              placeholder="Enter your name">
            <div class="invalid-feedback" *ngIf="hasFieldError('name')">
              {{ getFieldError('name') }}
            </div>
          </div>

          <div class="mb-3">
            <label class="form-label">Email <span class="text-danger">*</span></label>
            <input 
              type="email" 
              class="form-control" 
              formControlName="email"
              [class.is-invalid]="isFieldInvalid('email')"
              placeholder="Enter your email">
            <div class="invalid-feedback" *ngIf="hasFieldError('email')">
              {{ getFieldError('email') }}
            </div>
          </div>

          <div class="mb-3">
            <label class="form-label">Age <span class="text-danger">*</span></label>
            <input 
              type="number" 
              class="form-control" 
              formControlName="age"
              [class.is-invalid]="isFieldInvalid('age')"
              placeholder="Enter your age"
              min="1"
              max="120">
            <div class="invalid-feedback" *ngIf="hasFieldError('age')">
              {{ getFieldError('age') }}
            </div>
          </div>

          <div class="mb-3">
            <label class="form-label">Description</label>
            <textarea 
              class="form-control" 
              formControlName="description"
              [class.is-invalid]="isFieldInvalid('description')"
              placeholder="Enter a description (optional)"
              rows="3"></textarea>
            <div class="invalid-feedback" *ngIf="hasFieldError('description')">
              {{ getFieldError('description') }}
            </div>
          </div>

          <!-- Validation Errors Summary -->
          <div *ngIf="validationErrors.length > 0" class="alert alert-danger">
            <h6>Please correct the following errors:</h6>
            <ul class="mb-0">
              <li *ngFor="let error of validationErrors">{{ error.message }}</li>
            </ul>
          </div>

          <div class="d-flex gap-2">
            <button 
              type="submit" 
              class="btn btn-primary"
              [disabled]="isSubmitting">
              <span *ngIf="isSubmitting" class="spinner-border spinner-border-sm me-2"></span>
              {{ isSubmitting ? 'Submitting...' : 'Submit' }}
            </button>
            
            <button 
              type="button" 
              class="btn btn-secondary"
              (click)="resetForm()"
              [disabled]="isSubmitting">
              Reset
            </button>
            
            <button 
              type="button" 
              class="btn btn-warning"
              (click)="simulateServerError()"
              [disabled]="isSubmitting">
              Test Server Error
            </button>
          </div>
        </form>

        <!-- Form State Debug Info -->
        <div class="mt-4 p-3 bg-light rounded">
          <h6>Form State (Debug Info)</h6>
          <p><strong>Valid:</strong> {{ form.valid }}</p>
          <p><strong>Dirty:</strong> {{ isDirty }}</p>
          <p><strong>Submitting:</strong> {{ isSubmitting }}</p>
          <p><strong>Loading:</strong> {{ isLoading }}</p>
          <p><strong>Form Value:</strong></p>
          <pre>{{ form.value | json }}</pre>
        </div>
      </div>
    </div>
  `
})
export class ExampleFormComponent extends BaseFormComponent<ExampleFormData> {
  
  protected createForm(): FormGroup {
    return this.formBuilder.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, BaseFormComponent.emailValidator]],
      age: ['', [Validators.required, Validators.min(1), Validators.max(120)]],
      description: ['', [Validators.maxLength(500)]]
    });
  }

  protected submitForm(data: ExampleFormData): Observable<any> {
    console.log('Submitting form data:', data);
    
    // Simulate API call
    return of({ 
      success: true, 
      message: 'Form submitted successfully!',
      data: data 
    }).pipe(
      delay(2000) // Simulate network delay
    );
  }

  protected override onSubmitSuccess(result: any): void {
    super.onSubmitSuccess(result);
    console.log('Form submission result:', result);
  }

  simulateServerError(): void {
    if (this.isSubmitting) return;

    this.isSubmitting = true;
    this.showLoading('Testing server error...');

    // Simulate server validation error
    const serverError = {
      error: {
        errors: {
          name: ['Name is already taken'],
          email: ['Email domain is not allowed'],
          general: ['Server validation failed']
        }
      }
    };

    setTimeout(() => {
      this.isSubmitting = false;
      this.hideLoading();
      this.onSubmitError(serverError);
    }, 1500);
  }
}