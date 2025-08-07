import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil, Observable } from 'rxjs';

// Import all the enhanced components
import { BaseFormComponent } from '../base-form-component';
import { ButtonComponent } from '../button/button.component';
import { FormFieldComponent } from '../form-field/form-field.component';
import { ModalComponent } from '../modal/modal.component';
import { ProgressIndicatorComponent } from '../progress-indicator/progress-indicator.component';
import { MobileFormComponent } from '../mobile-form/mobile-form.component';
import { MobileNavComponent } from '../mobile-nav/mobile-nav.component';

// Import services
import { ResponsiveService } from '../../../core/services/responsive.service';
import { ModalService } from '../../../services/modal.service';
import { FormValidationService } from '../../services/form-validation.service';

@Component({
    selector: 'app-ui-showcase',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonComponent,
        FormFieldComponent,
        ModalComponent,
        ProgressIndicatorComponent,
        MobileFormComponent,
        MobileNavComponent
    ],
    template: `
    <div class="ui-showcase">
      <div class="container-fluid">
        <h1>StrideHR UI Component Showcase</h1>
        <p class="lead">Demonstration of all enhanced UI components with responsive design and modern features.</p>

        <!-- Responsive Info -->
        <div class="alert alert-info mb-4">
          <strong>Current Breakpoint:</strong> 
          <span *ngIf="isMobile">Mobile</span>
          <span *ngIf="isTablet">Tablet</span>
          <span *ngIf="isDesktop">Desktop</span>
          <span *ngIf="isLargeDesktop">Large Desktop</span>
          | <strong>Screen:</strong> {{ screenWidth }}x{{ screenHeight }}
          | <strong>Touch:</strong> {{ isTouch ? 'Yes' : 'No' }}
        </div>

        <!-- Button Showcase -->
        <section class="mb-5">
          <h2>Enhanced Buttons</h2>
          <div class="row">
            <div class="col-md-6">
              <h4>Button Variants</h4>
              <div class="d-flex flex-wrap gap-2 mb-3">
                <app-button variant="primary" text="Primary" (buttonClick)="onButtonClick('primary')"></app-button>
                <app-button variant="secondary" text="Secondary" (buttonClick)="onButtonClick('secondary')"></app-button>
                <app-button variant="success" text="Success" (buttonClick)="onButtonClick('success')"></app-button>
                <app-button variant="warning" text="Warning" (buttonClick)="onButtonClick('warning')"></app-button>
                <app-button variant="danger" text="Danger" (buttonClick)="onButtonClick('danger')"></app-button>
              </div>

              <h4>Button Sizes</h4>
              <div class="d-flex flex-wrap align-items-center gap-2 mb-3">
                <app-button variant="primary" size="sm" text="Small"></app-button>
                <app-button variant="primary" size="md" text="Medium"></app-button>
                <app-button variant="primary" size="lg" text="Large"></app-button>
                <app-button variant="primary" size="xl" text="Extra Large"></app-button>
              </div>

              <h4>Button States</h4>
              <div class="d-flex flex-wrap gap-2 mb-3">
                <app-button variant="primary" text="Normal"></app-button>
                <app-button variant="primary" text="Loading" [loading]="true" loadingText="Please wait..."></app-button>
                <app-button variant="primary" text="Disabled" [disabled]="true"></app-button>
                <app-button variant="primary" text="With Icon" iconLeft="fas fa-user"></app-button>
                <app-button variant="primary" text="With Badge" badge="5"></app-button>
              </div>
            </div>

            <div class="col-md-6">
              <h4>Outline Variants</h4>
              <div class="d-flex flex-wrap gap-2 mb-3">
                <app-button variant="outline-primary" text="Outline Primary"></app-button>
                <app-button variant="outline-secondary" text="Outline Secondary"></app-button>
                <app-button variant="outline-success" text="Outline Success"></app-button>
              </div>

              <h4>Special Variants</h4>
              <div class="d-flex flex-wrap gap-2 mb-3">
                <app-button variant="link" text="Link Button"></app-button>
                <app-button variant="ghost" text="Ghost Button"></app-button>
                <app-button variant="primary" text="Rounded" [rounded]="true"></app-button>
                <app-button variant="primary" text="Elevated" [elevated]="true"></app-button>
              </div>

              <h4>Full Width</h4>
              <app-button variant="primary" text="Full Width Button" [block]="true" class="mb-2"></app-button>
            </div>
          </div>
        </section>

        <!-- Form Components Showcase -->
        <section class="mb-5">
          <h2>Enhanced Form Components</h2>
          <div class="row">
            <div class="col-md-8">
              <form [formGroup]="form" (ngSubmit)="onFormSubmit()" class="needs-validation" novalidate>
                <div class="row">
                  <div class="col-md-6">
                    <app-form-field
                      label="First Name"
                      type="text"
                      placeholder="Enter your first name"
                      [control]="form.get('firstName')"
                      [required]="true"
                      helpText="This field is required">
                    </app-form-field>
                  </div>
                  <div class="col-md-6">
                    <app-form-field
                      label="Last Name"
                      type="text"
                      placeholder="Enter your last name"
                      [control]="form.get('lastName')"
                      [required]="true">
                    </app-form-field>
                  </div>
                </div>

                <app-form-field
                  label="Email Address"
                  type="email"
                  placeholder="Enter your email"
                  [control]="form.get('email')"
                  [required]="true"
                  prefixIcon="fas fa-envelope"
                  helpText="We'll never share your email with anyone else">
                </app-form-field>

                <app-form-field
                  label="Phone Number"
                  type="tel"
                  placeholder="Enter your phone number"
                  [control]="form.get('phone')"
                  prefixIcon="fas fa-phone">
                </app-form-field>

                <app-form-field
                  label="Department"
                  type="select"
                  placeholder="Select your department"
                  [control]="form.get('department')"
                  [options]="departmentOptions"
                  [required]="true">
                </app-form-field>

                <app-form-field
                  label="Bio"
                  type="textarea"
                  placeholder="Tell us about yourself"
                  [control]="form.get('bio')"
                  [rows]="4"
                  [showCharacterCount]="true"
                  [maxLength]="500">
                </app-form-field>

                <app-form-field
                  label="Newsletter"
                  type="checkbox"
                  checkboxLabel="Subscribe to our newsletter"
                  [control]="form.get('newsletter')">
                </app-form-field>

                <app-form-field
                  label="Preferred Contact Method"
                  type="radio"
                  [control]="form.get('contactMethod')"
                  [options]="contactMethodOptions"
                  [required]="true">
                </app-form-field>

                <div class="d-flex gap-2">
                  <app-button 
                    type="submit" 
                    variant="primary" 
                    text="Submit Form"
                    [loading]="isSubmitting"
                    loadingText="Submitting..."
                    [disabled]="form.invalid">
                  </app-button>
                  <app-button 
                    type="button" 
                    variant="outline-secondary" 
                    text="Reset Form"
                    (buttonClick)="resetForm()">
                  </app-button>
                </div>
              </form>
            </div>

            <div class="col-md-4">
              <h4>Form State</h4>
              <div class="card">
                <div class="card-body">
                  <p><strong>Valid:</strong> {{ form.valid ? 'Yes' : 'No' }}</p>
                  <p><strong>Dirty:</strong> {{ form.dirty ? 'Yes' : 'No' }}</p>
                  <p><strong>Touched:</strong> {{ form.touched ? 'Yes' : 'No' }}</p>
                  <p><strong>Errors:</strong></p>
                  <pre class="small">{{ getFormErrors() | json }}</pre>
                </div>
              </div>
            </div>
          </div>
        </section>

        <!-- Progress Indicators -->
        <section class="mb-5">
          <h2>Progress Indicators</h2>
          <div class="row">
            <div class="col-md-3">
              <h4>Linear Progress</h4>
              <app-progress-indicator 
                type="linear" 
                [value]="progressValue" 
                [showLabel]="true"
                class="mb-3">
              </app-progress-indicator>
              <app-progress-indicator 
                type="linear" 
                [indeterminate]="true"
                variant="success"
                class="mb-3">
              </app-progress-indicator>
            </div>

            <div class="col-md-3">
              <h4>Circular Progress</h4>
              <app-progress-indicator 
                type="circular" 
                [value]="progressValue" 
                [showLabel]="true"
                size="lg"
                class="mb-3">
              </app-progress-indicator>
              <app-progress-indicator 
                type="circular" 
                [indeterminate]="true"
                variant="warning"
                size="md">
              </app-progress-indicator>
            </div>

            <div class="col-md-3">
              <h4>Dots & Pulse</h4>
              <app-progress-indicator 
                type="dots" 
                variant="primary"
                size="lg"
                class="mb-3">
              </app-progress-indicator>
              <app-progress-indicator 
                type="pulse" 
                variant="info"
                size="md">
              </app-progress-indicator>
            </div>

            <div class="col-md-3">
              <h4>Skeleton Loading</h4>
              <app-progress-indicator 
                type="skeleton" 
                size="md">
              </app-progress-indicator>
            </div>
          </div>

          <div class="mt-3">
            <app-button 
              variant="outline-primary" 
              text="Update Progress" 
              (buttonClick)="updateProgress()">
            </app-button>
          </div>
        </section>

        <!-- Modal Showcase -->
        <section class="mb-5">
          <h2>Enhanced Modals</h2>
          <div class="d-flex flex-wrap gap-2">
            <app-button 
              variant="primary" 
              text="Show Basic Modal" 
              (buttonClick)="showBasicModal()">
            </app-button>
            <app-button 
              variant="success" 
              text="Show Confirmation" 
              (buttonClick)="showConfirmation()">
            </app-button>
            <app-button 
              variant="warning" 
              text="Show Alert" 
              (buttonClick)="showAlert()">
            </app-button>
            <app-button 
              variant="info" 
              text="Show Large Modal" 
              (buttonClick)="showLargeModal()">
            </app-button>
          </div>
        </section>

        <!-- Mobile Components (visible on mobile) -->
        <section class="mb-5" *ngIf="isMobile">
          <h2>Mobile Components</h2>
          
          <h4>Mobile Navigation</h4>
          <app-mobile-nav
            [bottomNavItems]="mobileNavItems"
            [showFab]="true"
            fabIcon="fas fa-plus"
            fabLabel="Add"
            (navItemClick)="onMobileNavClick($event)"
            (fabClick)="onFabClick()">
          </app-mobile-nav>
        </section>

        <!-- Responsive Features -->
        <section class="mb-5">
          <h2>Responsive Features</h2>
          <div class="row">
            <div class="col-12">
              <div class="alert alert-secondary">
                <h5>Current Responsive State:</h5>
                <ul class="mb-0">
                  <li>Screen Width: {{ screenWidth }}px</li>
                  <li>Screen Height: {{ screenHeight }}px</li>
                  <li>Orientation: {{ orientation }}</li>
                  <li>Touch Device: {{ isTouch ? 'Yes' : 'No' }}</li>
                  <li>Mobile: {{ isMobile ? 'Yes' : 'No' }}</li>
                  <li>Tablet: {{ isTablet ? 'Yes' : 'No' }}</li>
                  <li>Desktop: {{ isDesktop ? 'Yes' : 'No' }}</li>
                </ul>
              </div>
            </div>
          </div>
        </section>
      </div>

      <!-- Sample Modal -->
      <app-modal
        [isVisible]="showModal"
        [title]="modalTitle"
        [config]="modalConfig"
        (modalClose)="onModalClose()"
        (modalDismiss)="onModalDismiss()">
        <div class="p-3">
          <p>{{ modalContent }}</p>
          <div slot="footer">
            <app-button 
              variant="outline-secondary" 
              text="Cancel" 
              (buttonClick)="onModalDismiss()">
            </app-button>
            <app-button 
              variant="primary" 
              text="Confirm" 
              (buttonClick)="onModalClose()">
            </app-button>
          </div>
        </div>
      </app-modal>
    </div>
  `,
    styles: [`
    .ui-showcase {
      padding: 2rem 0;
    }

    .alert {
      border-radius: 8px;
    }

    .card {
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    pre {
      background-color: #f8f9fa;
      padding: 0.5rem;
      border-radius: 4px;
      max-height: 200px;
      overflow-y: auto;
    }

    section {
      border-bottom: 1px solid #e5e7eb;
      padding-bottom: 2rem;
    }

    section:last-child {
      border-bottom: none;
    }

    .gap-2 {
      gap: 0.5rem;
    }

    .gap-3 {
      gap: 1rem;
    }

    @media (max-width: 768px) {
      .ui-showcase {
        padding: 1rem 0;
      }
      
      .d-flex.flex-wrap {
        flex-direction: column;
        align-items: stretch;
      }
      
      .d-flex.flex-wrap > * {
        margin-bottom: 0.5rem;
      }
    }
  `]
})
export class UiShowcaseComponent extends BaseFormComponent implements OnInit, OnDestroy {
    private readonly responsiveService = inject(ResponsiveService);
    private readonly modalService = inject(ModalService);
    private readonly validationService = inject(FormValidationService);

    // Responsive state
    isMobile = false;
    isTablet = false;
    isDesktop = false;
    isLargeDesktop = false;
    isTouch = false;
    screenWidth = 0;
    screenHeight = 0;
    orientation: 'portrait' | 'landscape' = 'landscape';

    // Form state - isSubmitting is inherited from BaseFormComponent

    // Progress state
    progressValue = 45;

    // Modal state
    showModal = false;
    modalTitle = '';
    modalContent = '';
    modalConfig = {};

    // Form options
    departmentOptions = [
        { value: 'hr', label: 'Human Resources' },
        { value: 'it', label: 'Information Technology' },
        { value: 'finance', label: 'Finance' },
        { value: 'marketing', label: 'Marketing' },
        { value: 'operations', label: 'Operations' }
    ];

    contactMethodOptions = [
        { value: 'email', label: 'Email' },
        { value: 'phone', label: 'Phone' },
        { value: 'sms', label: 'SMS' }
    ];

    mobileNavItems = [
        { label: 'Home', icon: 'fas fa-home', route: '/dashboard', active: true },
        { label: 'Employees', icon: 'fas fa-users', route: '/employees' },
        { label: 'Attendance', icon: 'fas fa-clock', route: '/attendance', badge: '3' },
        { label: 'Reports', icon: 'fas fa-chart-bar', route: '/reports' },
        { label: 'Profile', icon: 'fas fa-user', route: '/profile' }
    ];

    protected override createForm(): FormGroup {
        return this.formBuilder.group({
            firstName: ['', [Validators.required, Validators.minLength(2)]],
            lastName: ['', [Validators.required, Validators.minLength(2)]],
            email: ['', [Validators.required, FormValidationService.emailValidator]],
            phone: ['', [FormValidationService.phoneValidator]],
            department: ['', [Validators.required]],
            bio: ['', [Validators.maxLength(500)]],
            newsletter: [false],
            contactMethod: ['', [Validators.required]]
        });
    }

    protected override submitForm(data: any): Observable<any> {
        // Simulate API call and return Observable
        return new Observable(observer => {
            setTimeout(() => {
                console.log('Form submitted:', data);
                observer.next(data);
                observer.complete();
            }, 2000);
        });
    }

    override ngOnInit(): void {
        super.ngOnInit();
        this.setupResponsiveSubscriptions();
    }

    private setupResponsiveSubscriptions(): void {
        this.responsiveService.breakpoint$.pipe(
            takeUntil(this.destroy$)
        ).subscribe(breakpoint => {
            this.isMobile = breakpoint.isMobile;
            this.isTablet = breakpoint.isTablet;
            this.isDesktop = breakpoint.isDesktop;
            this.isLargeDesktop = breakpoint.isLargeDesktop;
            this.isTouch = breakpoint.isTouch;
            this.screenWidth = breakpoint.screenWidth;
            this.screenHeight = breakpoint.screenHeight;
            this.orientation = breakpoint.orientation;
        });
    }

    onButtonClick(variant: string): void {
        this.showSuccess(`${variant} button clicked!`);
    }

    onFormSubmit(): void {
        if (this.form.valid) {
            this.isSubmitting = true;
            this.onSubmit();
        } else {
            this.showWarning('Please fill in all required fields');
        }
    }

    protected override onSubmissionSuccess(result: any): void {
        super.onSubmissionSuccess(result);
        this.isSubmitting = false;
    }

    protected override onSubmissionError(error: any): void {
        super.onSubmissionError(error);
        this.isSubmitting = false;
    }

    override resetForm(): void {
        this.form.reset();
        this.showInfo('Form has been reset');
    }

    getFormErrors(): any {
        return this.validationService.getFormErrors(this.form);
    }

    updateProgress(): void {
        this.progressValue = Math.floor(Math.random() * 100);
    }

    showBasicModal(): void {
        this.modalTitle = 'Basic Modal';
        this.modalContent = 'This is a basic modal with responsive design and proper backdrop handling.';
        this.modalConfig = { size: 'md', centered: true };
        this.showModal = true;
    }

    showConfirmation(): void {
        this.modalService.confirm(
            'Are you sure you want to perform this action?',
            'Confirm Action'
        ).subscribe(result => {
            if (result) {
                this.showSuccess('Action confirmed!');
            } else {
                this.showInfo('Action cancelled');
            }
        });
    }

    showAlert(): void {
        this.modalService.alert(
            'This is an important alert message.',
            'Alert'
        ).subscribe(() => {
            this.showInfo('Alert acknowledged');
        });
    }

    showLargeModal(): void {
        this.modalTitle = 'Large Modal';
        this.modalContent = 'This is a large modal that demonstrates responsive behavior. On mobile devices, it will take up the full screen for better usability.';
        this.modalConfig = { size: 'lg', centered: true, scrollable: true };
        this.showModal = true;
    }

    onModalClose(): void {
        this.showModal = false;
        this.showSuccess('Modal closed');
    }

    onModalDismiss(): void {
        this.showModal = false;
        this.showInfo('Modal dismissed');
    }

    onMobileNavClick(item: any): void {
        this.showInfo(`Navigated to ${item.label}`);
    }

    onFabClick(): void {
        this.showSuccess('FAB clicked!');
    }
}