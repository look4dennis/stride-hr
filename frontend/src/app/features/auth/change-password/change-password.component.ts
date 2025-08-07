import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LoadingService } from '../../../core/services/loading.service';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="change-password-container">
      <div class="change-password-card">
        <div class="change-password-header">
          <h2 class="text-center mb-4">
            <strong>Change Password</strong>
          </h2>
          <p class="text-center text-muted">
            {{ isForced ? 'You must change your password to continue' : 'Update your password' }}
          </p>
        </div>
        
        <form [formGroup]="changePasswordForm" (ngSubmit)="onSubmit()">
          <div class="mb-3">
            <label for="currentPassword" class="form-label">Current Password</label>
            <div class="input-group">
              <input 
                [type]="showCurrentPassword ? 'text' : 'password'" 
                class="form-control" 
                id="currentPassword"
                formControlName="currentPassword"
                [class.is-invalid]="isFieldInvalid('currentPassword')"
                placeholder="Enter your current password">
              <button 
                class="btn btn-outline-secondary" 
                type="button"
                (click)="toggleCurrentPasswordVisibility()">
                <i [class]="showCurrentPassword ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
              </button>
            </div>
            <div class="invalid-feedback" *ngIf="isFieldInvalid('currentPassword')">
              <div *ngIf="changePasswordForm.get('currentPassword')?.errors?.['required']">Current password is required</div>
            </div>
          </div>
          
          <div class="mb-3">
            <label for="newPassword" class="form-label">New Password</label>
            <div class="input-group">
              <input 
                [type]="showNewPassword ? 'text' : 'password'" 
                class="form-control" 
                id="newPassword"
                formControlName="newPassword"
                [class.is-invalid]="isFieldInvalid('newPassword')"
                placeholder="Enter your new password">
              <button 
                class="btn btn-outline-secondary" 
                type="button"
                (click)="toggleNewPasswordVisibility()">
                <i [class]="showNewPassword ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
              </button>
            </div>
            <div class="invalid-feedback" *ngIf="isFieldInvalid('newPassword')">
              <div *ngIf="changePasswordForm.get('newPassword')?.errors?.['required']">New password is required</div>
              <div *ngIf="changePasswordForm.get('newPassword')?.errors?.['minlength']">Password must be at least 8 characters</div>
              <div *ngIf="changePasswordForm.get('newPassword')?.errors?.['passwordStrength']">
                Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character
              </div>
            </div>
            
            <!-- Password strength indicator -->
            <div class="password-strength mt-2" *ngIf="changePasswordForm.get('newPassword')?.value">
              <div class="strength-bar">
                <div class="strength-fill" [class]="getPasswordStrengthClass()"></div>
              </div>
              <small class="text-muted">{{ getPasswordStrengthText() }}</small>
            </div>
          </div>
          
          <div class="mb-3">
            <label for="confirmPassword" class="form-label">Confirm New Password</label>
            <div class="input-group">
              <input 
                [type]="showConfirmPassword ? 'text' : 'password'" 
                class="form-control" 
                id="confirmPassword"
                formControlName="confirmPassword"
                [class.is-invalid]="isFieldInvalid('confirmPassword')"
                placeholder="Confirm your new password">
              <button 
                class="btn btn-outline-secondary" 
                type="button"
                (click)="toggleConfirmPasswordVisibility()">
                <i [class]="showConfirmPassword ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
              </button>
            </div>
            <div class="invalid-feedback" *ngIf="isFieldInvalid('confirmPassword')">
              <div *ngIf="changePasswordForm.get('confirmPassword')?.errors?.['required']">Please confirm your new password</div>
              <div *ngIf="changePasswordForm.get('confirmPassword')?.errors?.['passwordMismatch']">Passwords do not match</div>
            </div>
          </div>
          
          <button 
            type="submit" 
            class="btn btn-primary w-100 btn-lg"
            [disabled]="changePasswordForm.invalid || isSubmitting">
            <span *ngIf="isSubmitting" class="spinner-border spinner-border-sm me-2" role="status"></span>
            {{ isSubmitting ? 'Changing Password...' : 'Change Password' }}
          </button>
        </form>
        
        <div class="change-password-footer mt-4 text-center" *ngIf="!isForced">
          <a (click)="goBack()" class="text-decoration-none cursor-pointer">Cancel</a>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .change-password-container {
      min-height: 100vh;
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 2rem;
    }

    .change-password-card {
      background: white;
      border-radius: 16px;
      padding: 3rem;
      width: 100%;
      max-width: 450px;
      box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
    }

    .change-password-header h2 {
      font-family: var(--font-headings);
      color: var(--primary);
      font-size: 2rem;
    }

    .form-control {
      padding: 0.75rem 1rem;
      border-radius: 8px;
      border: 2px solid var(--gray-200);
      transition: all 0.15s ease-in-out;
    }

    .form-control:focus {
      border-color: var(--primary);
      box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
    }

    .btn-primary {
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      border: none;
      border-radius: 8px;
      padding: 0.75rem 1.5rem;
      font-weight: 500;
      transition: all 0.15s ease-in-out;
    }

    .btn-primary:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 4px 8px rgba(59, 130, 246, 0.4);
    }

    .btn-primary:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .form-label {
      font-weight: 500;
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    .password-strength {
      margin-top: 0.5rem;
    }

    .strength-bar {
      height: 4px;
      background-color: #e5e7eb;
      border-radius: 2px;
      overflow: hidden;
      margin-bottom: 0.25rem;
    }

    .strength-fill {
      height: 100%;
      transition: all 0.3s ease;
      border-radius: 2px;
    }

    .strength-weak {
      width: 25%;
      background-color: #ef4444;
    }

    .strength-fair {
      width: 50%;
      background-color: #f59e0b;
    }

    .strength-good {
      width: 75%;
      background-color: #10b981;
    }

    .strength-strong {
      width: 100%;
      background-color: #059669;
    }

    .cursor-pointer {
      cursor: pointer;
    }

    @media (max-width: 576px) {
      .change-password-card {
        padding: 2rem;
        margin: 1rem;
      }
    }
  `]
})
export class ChangePasswordComponent implements OnInit {
  changePasswordForm: FormGroup;
  showCurrentPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;
  isSubmitting = false;
  isForced = false;
  returnUrl = '/dashboard';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private notificationService: NotificationService,
    private loadingService: LoadingService
  ) {
    this.changePasswordForm = this.fb.group({
      currentPassword: ['', [Validators.required]],
      newPassword: ['', [Validators.required, Validators.minLength(8), this.passwordStrengthValidator]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit(): void {
    // Check if password change is forced
    const currentUser = this.authService.currentUser;
    this.isForced = currentUser?.forcePasswordChange || false;
    
    // Get return url from route parameters
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
  }

  onSubmit(): void {
    if (this.changePasswordForm.valid && !this.isSubmitting) {
      this.isSubmitting = true;
      this.loadingService.setLoading(true, 'change-password');

      const { currentPassword, newPassword } = this.changePasswordForm.value;

      this.authService.changePassword(currentPassword, newPassword).subscribe({
        next: (response) => {
          if (response.success) {
            this.notificationService.showSuccess(
              'Your password has been changed successfully. Please log in with your new password.',
              'Password Changed'
            );
            
            // Logout user to force re-login with new password
            setTimeout(() => {
              this.authService.logout();
            }, 2000);
          } else {
            this.notificationService.showError(
              response.message || 'Failed to change password',
              'Password Change Failed'
            );
          }
        },
        error: (error) => {
          console.error('Change password error:', error);
          this.notificationService.showError(
            error.message || 'Failed to change password. Please try again.',
            'Password Change Failed'
          );
        },
        complete: () => {
          this.isSubmitting = false;
          this.loadingService.setLoading(false, 'change-password');
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  toggleCurrentPasswordVisibility(): void {
    this.showCurrentPassword = !this.showCurrentPassword;
  }

  toggleNewPasswordVisibility(): void {
    this.showNewPassword = !this.showNewPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.changePasswordForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getPasswordStrengthClass(): string {
    const password = this.changePasswordForm.get('newPassword')?.value || '';
    const strength = this.calculatePasswordStrength(password);
    
    if (strength < 2) return 'strength-weak';
    if (strength < 3) return 'strength-fair';
    if (strength < 4) return 'strength-good';
    return 'strength-strong';
  }

  getPasswordStrengthText(): string {
    const password = this.changePasswordForm.get('newPassword')?.value || '';
    const strength = this.calculatePasswordStrength(password);
    
    if (strength < 2) return 'Weak';
    if (strength < 3) return 'Fair';
    if (strength < 4) return 'Good';
    return 'Strong';
  }

  goBack(): void {
    this.router.navigate([this.returnUrl]);
  }

  private calculatePasswordStrength(password: string): number {
    let strength = 0;
    
    if (password.length >= 8) strength++;
    if (/[a-z]/.test(password)) strength++;
    if (/[A-Z]/.test(password)) strength++;
    if (/[0-9]/.test(password)) strength++;
    if (/[^A-Za-z0-9]/.test(password)) strength++;
    
    return strength;
  }

  private passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.value;
    if (!password) return null;

    const hasUpperCase = /[A-Z]/.test(password);
    const hasLowerCase = /[a-z]/.test(password);
    const hasNumeric = /[0-9]/.test(password);
    const hasSpecialChar = /[^A-Za-z0-9]/.test(password);

    const isValid = hasUpperCase && hasLowerCase && hasNumeric && hasSpecialChar;
    return isValid ? null : { passwordStrength: true };
  }

  private passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const newPassword = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;

    if (newPassword && confirmPassword && newPassword !== confirmPassword) {
      group.get('confirmPassword')?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }

    // Clear the error if passwords match
    const confirmControl = group.get('confirmPassword');
    if (confirmControl?.errors?.['passwordMismatch']) {
      delete confirmControl.errors['passwordMismatch'];
      if (Object.keys(confirmControl.errors).length === 0) {
        confirmControl.setErrors(null);
      }
    }

    return null;
  }

  private markFormGroupTouched(): void {
    Object.keys(this.changePasswordForm.controls).forEach(key => {
      const control = this.changePasswordForm.get(key);
      control?.markAsTouched();
    });
  }
}