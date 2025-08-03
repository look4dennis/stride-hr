import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { LoadingService } from '../../../core/services/loading.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="login-container">
      <div class="login-card">
        <div class="login-header">
          <h2 class="text-center mb-4">
            <strong>StrideHR</strong>
          </h2>
          <p class="text-center text-muted">Sign in to your account</p>
        </div>
        
        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
          <div class="mb-3">
            <label for="email" class="form-label">Email Address</label>
            <input 
              type="email" 
              class="form-control" 
              id="email"
              formControlName="email"
              [class.is-invalid]="isFieldInvalid('email')"
              placeholder="Enter your email">
            <div class="invalid-feedback" *ngIf="isFieldInvalid('email')">
              <div *ngIf="loginForm.get('email')?.errors?.['required']">Email is required</div>
              <div *ngIf="loginForm.get('email')?.errors?.['email']">Please enter a valid email</div>
            </div>
          </div>
          
          <div class="mb-3">
            <label for="password" class="form-label">Password</label>
            <div class="input-group">
              <input 
                [type]="showPassword ? 'text' : 'password'" 
                class="form-control" 
                id="password"
                formControlName="password"
                [class.is-invalid]="isFieldInvalid('password')"
                placeholder="Enter your password">
              <button 
                class="btn btn-outline-secondary" 
                type="button"
                (click)="togglePasswordVisibility()">
                <i [class]="showPassword ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
              </button>
            </div>
            <div class="invalid-feedback" *ngIf="isFieldInvalid('password')">
              <div *ngIf="loginForm.get('password')?.errors?.['required']">Password is required</div>
              <div *ngIf="loginForm.get('password')?.errors?.['minlength']">Password must be at least 6 characters</div>
            </div>
          </div>
          
          <div class="mb-3 form-check">
            <input 
              type="checkbox" 
              class="form-check-input" 
              id="rememberMe"
              formControlName="rememberMe">
            <label class="form-check-label" for="rememberMe">
              Remember me
            </label>
          </div>
          
          <button 
            type="submit" 
            class="btn btn-primary w-100 btn-lg"
            [disabled]="loginForm.invalid || isSubmitting">
            <span *ngIf="isSubmitting" class="spinner-border spinner-border-sm me-2" role="status"></span>
            {{ isSubmitting ? 'Signing in...' : 'Sign In' }}
          </button>
        </form>
        
        <div class="login-footer mt-4 text-center">
          <a href="#" class="text-decoration-none">Forgot your password?</a>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      min-height: 100vh;
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 2rem;
    }

    .login-card {
      background: white;
      border-radius: 16px;
      padding: 3rem;
      width: 100%;
      max-width: 400px;
      box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
    }

    .login-header h2 {
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

    .form-check-label {
      color: var(--text-secondary);
    }

    @media (max-width: 576px) {
      .login-card {
        padding: 2rem;
        margin: 1rem;
      }
    }
  `]
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  showPassword = false;
  isSubmitting = false;
  returnUrl = '/dashboard';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private notificationService: NotificationService,
    private loadingService: LoadingService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });
  }

  ngOnInit(): void {
    // Get return url from route parameters or default to dashboard
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';

    // Redirect if already authenticated
    if (this.authService.isAuthenticated) {
      this.router.navigate([this.returnUrl]);
    }
  }

  onSubmit(): void {
    if (this.loginForm.valid && !this.isSubmitting) {
      this.isSubmitting = true;
      this.loadingService.setLoading(true, 'login');

      const credentials = {
        email: this.loginForm.value.email,
        password: this.loginForm.value.password
      };

      this.authService.login(credentials).subscribe({
        next: (response) => {
          this.notificationService.showSuccess(
            `Welcome back, ${response.user.firstName}!`,
            'Login Successful'
          );
          this.router.navigate([this.returnUrl]);
        },
        error: (error) => {
          this.notificationService.showError(
            error.message || 'Invalid email or password',
            'Login Failed'
          );
        },
        complete: () => {
          this.isSubmitting = false;
          this.loadingService.setLoading(false, 'login');
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  private markFormGroupTouched(): void {
    Object.keys(this.loginForm.controls).forEach(key => {
      const control = this.loginForm.get(key);
      control?.markAsTouched();
    });
  }
}