import { Component, Input, Output, EventEmitter, forwardRef, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormControl, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { FormValidationService } from '../../services/form-validation.service';

export type FormFieldType = 'text' | 'email' | 'password' | 'number' | 'tel' | 'url' | 'search' | 'textarea' | 'select' | 'checkbox' | 'radio' | 'date' | 'time' | 'datetime-local' | 'file';

export interface FormFieldOption {
  value: any;
  label: string;
  disabled?: boolean;
}

@Component({
  selector: 'app-form-field',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => FormFieldComponent),
      multi: true
    }
  ],
  template: `
    <div class="form-field" [class.form-field--error]="hasError" [class.form-field--success]="hasSuccess">
      <!-- Label -->
      <label *ngIf="label" [for]="fieldId" class="form-label">
        {{ label }}
        <span *ngIf="required" class="text-danger">*</span>
        <span *ngIf="optional" class="text-muted">(optional)</span>
      </label>

      <!-- Help text (above field) -->
      <small *ngIf="helpText && helpTextPosition === 'top'" class="form-text text-muted mb-2">
        {{ helpText }}
      </small>

      <!-- Input field -->
      <div class="form-field__input-container" [class.input-group]="hasPrefix || hasSuffix">
        <!-- Prefix -->
        <span *ngIf="prefix" class="input-group-text">{{ prefix }}</span>
        <span *ngIf="prefixIcon" class="input-group-text">
          <i [class]="prefixIcon"></i>
        </span>

        <!-- Main input -->
        <input
          *ngIf="type !== 'textarea' && type !== 'select' && type !== 'checkbox' && type !== 'radio'"
          [id]="fieldId"
          [type]="type"
          [class]="inputClasses"
          [placeholder]="placeholder"
          [readonly]="readonly"
          [disabled]="disabled"
          [min]="min"
          [max]="max"
          [step]="step"
          [pattern]="pattern"
          [autocomplete]="autocomplete"
          [value]="value"
          (input)="onInput($event)"
          (blur)="onBlur()"
          (focus)="onFocus()"
          (change)="onInputChange($event)"
        />

        <!-- Textarea -->
        <textarea
          *ngIf="type === 'textarea'"
          [id]="fieldId"
          [class]="inputClasses"
          [placeholder]="placeholder"
          [readonly]="readonly"
          [disabled]="disabled"
          [rows]="rows"
          [cols]="cols"
          [value]="value"
          (input)="onInput($event)"
          (blur)="onBlur()"
          (focus)="onFocus()"
        ></textarea>

        <!-- Select -->
        <select
          *ngIf="type === 'select'"
          [id]="fieldId"
          [class]="inputClasses"
          [disabled]="disabled"
          [value]="value"
          (change)="onSelectChange($event)"
          (blur)="onBlur()"
          (focus)="onFocus()"
        >
          <option *ngIf="placeholder" value="" disabled>{{ placeholder }}</option>
          <option
            *ngFor="let option of options"
            [value]="option.value"
            [disabled]="option.disabled"
          >
            {{ option.label }}
          </option>
        </select>

        <!-- Checkbox -->
        <div *ngIf="type === 'checkbox'" class="form-check">
          <input
            [id]="fieldId"
            type="checkbox"
            class="form-check-input"
            [class.is-invalid]="hasError"
            [class.is-valid]="hasSuccess"
            [disabled]="disabled"
            [checked]="value"
            (change)="onCheckboxChange($event)"
            (blur)="onBlur()"
            (focus)="onFocus()"
          />
          <label *ngIf="checkboxLabel" [for]="fieldId" class="form-check-label">
            {{ checkboxLabel }}
          </label>
        </div>

        <!-- Radio buttons -->
        <div *ngIf="type === 'radio'" class="form-radio-group">
          <div *ngFor="let option of options" class="form-check">
            <input
              [id]="fieldId + '_' + option.value"
              type="radio"
              class="form-check-input"
              [class.is-invalid]="hasError"
              [class.is-valid]="hasSuccess"
              [name]="fieldId"
              [value]="option.value"
              [disabled]="disabled || option.disabled"
              [checked]="value === option.value"
              (change)="onRadioChange($event)"
              (blur)="onBlur()"
              (focus)="onFocus()"
            />
            <label [for]="fieldId + '_' + option.value" class="form-check-label">
              {{ option.label }}
            </label>
          </div>
        </div>

        <!-- Suffix -->
        <span *ngIf="suffix" class="input-group-text">{{ suffix }}</span>
        <span *ngIf="suffixIcon" class="input-group-text">
          <i [class]="suffixIcon"></i>
        </span>
      </div>

      <!-- Validation message -->
      <div *ngIf="hasError && errorMessage" class="invalid-feedback d-block">
        {{ errorMessage }}
      </div>

      <!-- Success message -->
      <div *ngIf="hasSuccess && successMessage" class="valid-feedback d-block">
        {{ successMessage }}
      </div>

      <!-- Help text (below field) -->
      <small *ngIf="helpText && helpTextPosition === 'bottom'" class="form-text text-muted mt-1">
        {{ helpText }}
      </small>

      <!-- Character count -->
      <small *ngIf="showCharacterCount && maxLength" class="form-text text-muted mt-1 text-end">
        {{ (value || '').length }}/{{ maxLength }}
      </small>
    </div>
  `,
  styles: [`
    .form-field {
      margin-bottom: 1rem;
    }

    .form-field--error .form-control,
    .form-field--error .form-select {
      border-color: #dc3545;
    }

    .form-field--success .form-control,
    .form-field--success .form-select {
      border-color: #198754;
    }

    .form-label {
      font-weight: 500;
      color: #374151;
      margin-bottom: 0.5rem;
      display: block;
    }

    .form-control,
    .form-select {
      border: 2px solid #e5e7eb;
      border-radius: 8px;
      padding: 0.75rem 1rem;
      font-size: 14px;
      transition: all 0.15s ease-in-out;
    }

    .form-control:focus,
    .form-select:focus {
      border-color: #3b82f6;
      box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
      outline: none;
    }

    .form-control:disabled,
    .form-select:disabled {
      background-color: #f8f9fa;
      opacity: 0.6;
    }

    .input-group-text {
      background-color: #f8f9fa;
      border: 2px solid #e5e7eb;
      color: #6c757d;
      font-weight: 500;
    }

    .input-group .form-control:not(:first-child) {
      border-left: none;
    }

    .input-group .form-control:not(:last-child) {
      border-right: none;
    }

    .form-check {
      margin-bottom: 0.5rem;
    }

    .form-check-input {
      margin-top: 0.25rem;
    }

    .form-check-label {
      margin-left: 0.5rem;
      cursor: pointer;
    }

    .form-radio-group {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .invalid-feedback,
    .valid-feedback {
      font-size: 0.875rem;
      margin-top: 0.25rem;
    }

    .invalid-feedback {
      color: #dc3545;
    }

    .valid-feedback {
      color: #198754;
    }

    .text-danger {
      color: #dc3545 !important;
    }

    .text-muted {
      color: #6c757d !important;
    }

    .form-text {
      font-size: 0.875rem;
    }

    /* Responsive adjustments */
    @media (max-width: 576px) {
      .form-control,
      .form-select {
        font-size: 16px; /* Prevent zoom on iOS */
      }
    }
  `]
})
export class FormFieldComponent implements ControlValueAccessor, OnInit, OnDestroy {
  @Input() label?: string;
  @Input() type: FormFieldType = 'text';
  @Input() placeholder?: string;
  @Input() helpText?: string;
  @Input() helpTextPosition: 'top' | 'bottom' = 'bottom';
  @Input() required = false;
  @Input() optional = false;
  @Input() disabled = false;
  @Input() readonly = false;
  @Input() autocomplete?: string;
  
  // Input attributes
  @Input() min?: number | string;
  @Input() max?: number | string;
  @Input() step?: number | string;
  @Input() pattern?: string;
  @Input() maxLength?: number;
  @Input() minLength?: number;
  
  // Textarea attributes
  @Input() rows = 3;
  @Input() cols?: number;
  
  // Select/Radio options
  @Input() options: FormFieldOption[] = [];
  
  // Checkbox
  @Input() checkboxLabel?: string;
  
  // Styling
  @Input() prefix?: string;
  @Input() suffix?: string;
  @Input() prefixIcon?: string;
  @Input() suffixIcon?: string;
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  
  // Validation
  @Input() control?: FormControl;
  @Input() errorMessage?: string;
  @Input() successMessage?: string;
  @Input() showCharacterCount = false;
  
  // Events
  @Output() valueChange = new EventEmitter<any>();
  @Output() blur = new EventEmitter<void>();
  @Output() focus = new EventEmitter<void>();

  value: any = '';
  fieldId = `form-field-${Math.random().toString(36).substr(2, 9)}`;
  
  private destroy$ = new Subject<void>();
  private onChange = (value: any) => {};
  private onTouched = () => {};

  constructor(private validationService: FormValidationService) {}

  ngOnInit(): void {
    if (this.control) {
      this.setupControlValidation();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ControlValueAccessor implementation
  writeValue(value: any): void {
    this.value = value;
  }

  registerOnChange(fn: (value: any) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  // Event handlers
  onInput(event: Event): void {
    const target = event.target as HTMLInputElement | HTMLTextAreaElement;
    this.updateValue(target.value);
  }

  onInputChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.updateValue(target.value);
  }

  onCheckboxChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.updateValue(target.checked);
  }

  onRadioChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.updateValue(target.value);
  }

  onSelectChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.updateValue(target.value);
  }

  onBlur(): void {
    this.onTouched();
    this.blur.emit();
  }

  onFocus(): void {
    this.focus.emit();
  }

  private updateValue(newValue: any): void {
    this.value = newValue;
    this.onChange(newValue);
    this.valueChange.emit(newValue);
  }

  private setupControlValidation(): void {
    if (!this.control) return;

    this.control.statusChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.updateValidationState();
      });

    this.control.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.updateValidationState();
      });
  }

  private updateValidationState(): void {
    if (!this.control) return;

    if (!this.errorMessage) {
      this.errorMessage = this.validationService.getValidationMessage(this.control) || undefined;
    }
  }

  // Computed properties
  get hasError(): boolean {
    if (this.control) {
      return this.validationService.isFieldInvalid({ controls: { field: this.control } } as any, 'field');
    }
    return false;
  }

  get hasSuccess(): boolean {
    if (this.control) {
      return this.validationService.isFieldValid({ controls: { field: this.control } } as any, 'field');
    }
    return false;
  }

  get inputClasses(): string {
    const classes = ['form-control'];
    
    if (this.type === 'select') {
      classes[0] = 'form-select';
    }
    
    if (this.size === 'sm') {
      classes.push('form-control-sm');
    } else if (this.size === 'lg') {
      classes.push('form-control-lg');
    }
    
    if (this.hasError) {
      classes.push('is-invalid');
    } else if (this.hasSuccess) {
      classes.push('is-valid');
    }
    
    return classes.join(' ');
  }

  get hasPrefix(): boolean {
    return !!(this.prefix || this.prefixIcon);
  }

  get hasSuffix(): boolean {
    return !!(this.suffix || this.suffixIcon);
  }
}