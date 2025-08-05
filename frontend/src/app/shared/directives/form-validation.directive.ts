import { Directive, Input, OnInit, OnDestroy, ElementRef, Renderer2 } from '@angular/core';
import { FormGroup, AbstractControl } from '@angular/forms';
import { Subject, takeUntil, debounceTime } from 'rxjs';
import { FormValidationService } from '../services/form-validation.service';

@Directive({
  selector: '[appFormValidation]',
  standalone: true
})
export class FormValidationDirective implements OnInit, OnDestroy {
  @Input('appFormValidation') form!: FormGroup;
  @Input() validationConfig: {
    showValidationOnChange?: boolean;
    showValidationOnBlur?: boolean;
    debounceTime?: number;
    autoMarkTouched?: boolean;
  } = {};

  private destroy$ = new Subject<void>();
  private validationElements = new Map<string, HTMLElement>();

  constructor(
    private el: ElementRef,
    private renderer: Renderer2,
    private validationService: FormValidationService
  ) {}

  ngOnInit(): void {
    if (!this.form) {
      console.warn('FormValidationDirective: No form provided');
      return;
    }

    this.setupValidationConfig();
    this.setupValidationElements();
    this.setupFormValidation();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupValidationConfig(): void {
    const defaultConfig = {
      showValidationOnChange: true,
      showValidationOnBlur: true,
      debounceTime: 300,
      autoMarkTouched: false
    };

    this.validationConfig = { ...defaultConfig, ...this.validationConfig };
  }

  private setupValidationElements(): void {
    const formElement = this.el.nativeElement as HTMLFormElement;
    const formControls = formElement.querySelectorAll('[formControlName]');

    formControls.forEach((controlElement: Element) => {
      const controlName = controlElement.getAttribute('formControlName');
      if (controlName) {
        this.createValidationElement(controlElement as HTMLElement, controlName);
      }
    });
  }

  private createValidationElement(controlElement: HTMLElement, controlName: string): void {
    // Create validation message element
    const validationElement = this.renderer.createElement('div');
    this.renderer.addClass(validationElement, 'invalid-feedback');
    this.renderer.addClass(validationElement, 'form-validation-message');
    this.renderer.setStyle(validationElement, 'display', 'none');

    // Insert after the form control
    const parent = controlElement.parentElement;
    if (parent) {
      this.renderer.insertBefore(parent, validationElement, controlElement.nextSibling);
      this.validationElements.set(controlName, validationElement);
    }
  }

  private setupFormValidation(): void {
    // Watch for form status changes
    this.form.statusChanges
      .pipe(
        debounceTime(this.validationConfig.debounceTime || 300),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.updateValidationDisplay();
      });

    // Watch individual control changes
    Object.keys(this.form.controls).forEach(controlName => {
      const control = this.form.get(controlName);
      if (control) {
        this.setupControlValidation(control, controlName);
      }
    });

    // Handle form submission
    const formElement = this.el.nativeElement as HTMLFormElement;
    this.renderer.listen(formElement, 'submit', (event) => {
      this.handleFormSubmit(event);
    });
  }

  private setupControlValidation(control: AbstractControl, controlName: string): void {
    // Value changes
    if (this.validationConfig.showValidationOnChange) {
      control.valueChanges
        .pipe(
          debounceTime(this.validationConfig.debounceTime || 300),
          takeUntil(this.destroy$)
        )
        .subscribe(() => {
          if (control.dirty || this.validationConfig.autoMarkTouched) {
            control.markAsTouched();
            this.updateControlValidation(controlName);
          }
        });
    }

    // Status changes
    control.statusChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.updateControlValidation(controlName);
      });

    // Find the actual DOM element and add blur listener
    const formElement = this.el.nativeElement as HTMLFormElement;
    const controlElement = formElement.querySelector(`[formControlName="${controlName}"]`) as HTMLElement;
    
    if (controlElement && this.validationConfig.showValidationOnBlur) {
      this.renderer.listen(controlElement, 'blur', () => {
        control.markAsTouched();
        this.updateControlValidation(controlName);
      });
    }
  }

  private updateValidationDisplay(): void {
    Object.keys(this.form.controls).forEach(controlName => {
      this.updateControlValidation(controlName);
    });
  }

  private updateControlValidation(controlName: string): void {
    const control = this.form.get(controlName);
    const validationElement = this.validationElements.get(controlName);
    
    if (!control || !validationElement) return;

    const message = this.validationService.getValidationMessage(control, controlName);
    
    if (message) {
      this.renderer.setProperty(validationElement, 'textContent', message);
      this.renderer.setStyle(validationElement, 'display', 'block');
      this.addValidationClasses(controlName, 'invalid');
    } else {
      this.renderer.setStyle(validationElement, 'display', 'none');
      this.removeValidationClasses(controlName);
      
      if (control.valid && control.touched) {
        this.addValidationClasses(controlName, 'valid');
      }
    }
  }

  private addValidationClasses(controlName: string, type: 'valid' | 'invalid'): void {
    const formElement = this.el.nativeElement as HTMLFormElement;
    const controlElement = formElement.querySelector(`[formControlName="${controlName}"]`) as HTMLElement;
    
    if (controlElement) {
      this.renderer.removeClass(controlElement, type === 'valid' ? 'is-invalid' : 'is-valid');
      this.renderer.addClass(controlElement, type === 'valid' ? 'is-valid' : 'is-invalid');
    }
  }

  private removeValidationClasses(controlName: string): void {
    const formElement = this.el.nativeElement as HTMLFormElement;
    const controlElement = formElement.querySelector(`[formControlName="${controlName}"]`) as HTMLElement;
    
    if (controlElement) {
      this.renderer.removeClass(controlElement, 'is-valid');
      this.renderer.removeClass(controlElement, 'is-invalid');
    }
  }

  private handleFormSubmit(event: Event): void {
    if (this.form.invalid) {
      event.preventDefault();
      event.stopPropagation();
      
      // Mark all fields as touched to show validation errors
      this.validationService.markAllFieldsAsTouched(this.form);
      this.updateValidationDisplay();
      
      // Focus on first invalid field
      this.focusFirstInvalidField();
      
      // Show form-level error message
      this.showFormError();
    }
  }

  private focusFirstInvalidField(): void {
    const formElement = this.el.nativeElement as HTMLFormElement;
    
    for (const controlName of Object.keys(this.form.controls)) {
      const control = this.form.get(controlName);
      if (control && control.invalid) {
        const controlElement = formElement.querySelector(`[formControlName="${controlName}"]`) as HTMLElement;
        if (controlElement && controlElement.focus) {
          controlElement.focus();
          break;
        }
      }
    }
  }

  private showFormError(): void {
    const firstError = this.validationService.validateFormAndGetFirstError(this.form);
    if (firstError) {
      // You could integrate with a notification service here
      console.warn('Form validation error:', firstError);
    }
  }
}