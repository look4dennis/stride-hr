import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ResponsiveService } from '../../../core/services/responsive.service';
import { Subject, takeUntil } from 'rxjs';

export type ButtonVariant = 'primary' | 'secondary' | 'success' | 'warning' | 'danger' | 'info' | 'light' | 'dark' | 'outline-primary' | 'outline-secondary' | 'outline-success' | 'outline-warning' | 'outline-danger' | 'outline-info' | 'outline-light' | 'outline-dark' | 'link' | 'ghost';
export type ButtonSize = 'sm' | 'md' | 'lg' | 'xl';
export type ButtonType = 'button' | 'submit' | 'reset';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button
      [type]="type"
      [class]="buttonClasses"
      [disabled]="disabled || loading"
      [attr.aria-label]="ariaLabel"
      [attr.aria-describedby]="ariaDescribedBy"
      (click)="onClick($event)"
      (focus)="onFocus($event)"
      (blur)="onBlur($event)">
      
      <!-- Loading spinner -->
      <span *ngIf="loading" class="btn-spinner" [attr.aria-hidden]="true">
        <svg class="spinner" viewBox="0 0 24 24">
          <circle class="spinner-circle" cx="12" cy="12" r="10" fill="none" stroke="currentColor" stroke-width="2"/>
        </svg>
      </span>
      
      <!-- Icon (left) -->
      <i *ngIf="iconLeft && !loading" [class]="iconLeft" [attr.aria-hidden]="true"></i>
      
      <!-- Button content -->
      <span class="btn-content" [class.visually-hidden]="loading && hideTextOnLoading">
        <ng-content></ng-content>
        <span *ngIf="!hasContent && text">{{ text }}</span>
      </span>
      
      <!-- Loading text -->
      <span *ngIf="loading && loadingText" class="btn-loading-text">
        {{ loadingText }}
      </span>
      
      <!-- Icon (right) -->
      <i *ngIf="iconRight && !loading" [class]="iconRight" [attr.aria-hidden]="true"></i>
      
      <!-- Badge -->
      <span *ngIf="badge" class="btn-badge" [class]="badgeClasses">
        {{ badge }}
      </span>
    </button>
  `,
  styles: [`
    /* Base button styles */
    button {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      padding: 0.5rem 1rem;
      font-family: inherit;
      font-size: 0.875rem;
      font-weight: 500;
      line-height: 1.5;
      text-align: center;
      text-decoration: none;
      vertical-align: middle;
      cursor: pointer;
      user-select: none;
      border: 1px solid transparent;
      border-radius: 0.375rem;
      transition: all 0.15s ease-in-out;
      position: relative;
      overflow: hidden;
      -webkit-tap-highlight-color: rgba(0, 0, 0, 0.1);
      touch-action: manipulation;
    }

    button:focus {
      outline: 2px solid transparent;
      outline-offset: 2px;
      box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.5);
    }

    button:disabled {
      pointer-events: none;
      opacity: 0.6;
      cursor: not-allowed;
    }

    button:active:not(:disabled) {
      transform: translateY(1px);
    }

    /* Size variants */
    .btn-sm {
      padding: 0.25rem 0.75rem;
      font-size: 0.75rem;
      border-radius: 0.25rem;
    }

    .btn-lg {
      padding: 0.75rem 1.5rem;
      font-size: 1rem;
      border-radius: 0.5rem;
    }

    .btn-xl {
      padding: 1rem 2rem;
      font-size: 1.125rem;
      border-radius: 0.5rem;
    }

    /* Color variants */
    .btn-primary {
      color: #ffffff;
      background-color: #3b82f6;
      border-color: #3b82f6;
    }

    .btn-primary:hover:not(:disabled) {
      background-color: #2563eb;
      border-color: #2563eb;
    }

    .btn-secondary {
      color: #ffffff;
      background-color: #6b7280;
      border-color: #6b7280;
    }

    .btn-secondary:hover:not(:disabled) {
      background-color: #4b5563;
      border-color: #4b5563;
    }

    .btn-success {
      color: #ffffff;
      background-color: #10b981;
      border-color: #10b981;
    }

    .btn-success:hover:not(:disabled) {
      background-color: #059669;
      border-color: #059669;
    }

    .btn-warning {
      color: #ffffff;
      background-color: #f59e0b;
      border-color: #f59e0b;
    }

    .btn-warning:hover:not(:disabled) {
      background-color: #d97706;
      border-color: #d97706;
    }

    .btn-danger {
      color: #ffffff;
      background-color: #ef4444;
      border-color: #ef4444;
    }

    .btn-danger:hover:not(:disabled) {
      background-color: #dc2626;
      border-color: #dc2626;
    }

    .btn-info {
      color: #ffffff;
      background-color: #06b6d4;
      border-color: #06b6d4;
    }

    .btn-info:hover:not(:disabled) {
      background-color: #0891b2;
      border-color: #0891b2;
    }

    .btn-light {
      color: #374151;
      background-color: #f9fafb;
      border-color: #e5e7eb;
    }

    .btn-light:hover:not(:disabled) {
      background-color: #f3f4f6;
      border-color: #d1d5db;
    }

    .btn-dark {
      color: #ffffff;
      background-color: #374151;
      border-color: #374151;
    }

    .btn-dark:hover:not(:disabled) {
      background-color: #1f2937;
      border-color: #1f2937;
    }

    /* Outline variants */
    .btn-outline-primary {
      color: #3b82f6;
      background-color: transparent;
      border-color: #3b82f6;
    }

    .btn-outline-primary:hover:not(:disabled) {
      color: #ffffff;
      background-color: #3b82f6;
    }

    .btn-outline-secondary {
      color: #6b7280;
      background-color: transparent;
      border-color: #6b7280;
    }

    .btn-outline-secondary:hover:not(:disabled) {
      color: #ffffff;
      background-color: #6b7280;
    }

    .btn-outline-success {
      color: #10b981;
      background-color: transparent;
      border-color: #10b981;
    }

    .btn-outline-success:hover:not(:disabled) {
      color: #ffffff;
      background-color: #10b981;
    }

    .btn-outline-warning {
      color: #f59e0b;
      background-color: transparent;
      border-color: #f59e0b;
    }

    .btn-outline-warning:hover:not(:disabled) {
      color: #ffffff;
      background-color: #f59e0b;
    }

    .btn-outline-danger {
      color: #ef4444;
      background-color: transparent;
      border-color: #ef4444;
    }

    .btn-outline-danger:hover:not(:disabled) {
      color: #ffffff;
      background-color: #ef4444;
    }

    .btn-outline-info {
      color: #06b6d4;
      background-color: transparent;
      border-color: #06b6d4;
    }

    .btn-outline-info:hover:not(:disabled) {
      color: #ffffff;
      background-color: #06b6d4;
    }

    .btn-outline-light {
      color: #f9fafb;
      background-color: transparent;
      border-color: #f9fafb;
    }

    .btn-outline-light:hover:not(:disabled) {
      color: #374151;
      background-color: #f9fafb;
    }

    .btn-outline-dark {
      color: #374151;
      background-color: transparent;
      border-color: #374151;
    }

    .btn-outline-dark:hover:not(:disabled) {
      color: #ffffff;
      background-color: #374151;
    }

    /* Link variant */
    .btn-link {
      color: #3b82f6;
      background-color: transparent;
      border-color: transparent;
      text-decoration: underline;
    }

    .btn-link:hover:not(:disabled) {
      color: #2563eb;
      text-decoration: none;
    }

    /* Ghost variant */
    .btn-ghost {
      color: #374151;
      background-color: transparent;
      border-color: transparent;
    }

    .btn-ghost:hover:not(:disabled) {
      background-color: #f3f4f6;
    }

    /* Loading spinner */
    .btn-spinner {
      display: inline-flex;
      align-items: center;
      justify-content: center;
    }

    .spinner {
      width: 1em;
      height: 1em;
      animation: spin 1s linear infinite;
    }

    .spinner-circle {
      stroke-dasharray: 31.416;
      stroke-dashoffset: 31.416;
      animation: spinCircle 2s linear infinite;
    }

    .btn-loading-text {
      margin-left: 0.5rem;
    }

    /* Badge */
    .btn-badge {
      position: absolute;
      top: -0.5rem;
      right: -0.5rem;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-width: 1.25rem;
      height: 1.25rem;
      padding: 0.125rem 0.375rem;
      font-size: 0.75rem;
      font-weight: 600;
      line-height: 1;
      color: #ffffff;
      background-color: #ef4444;
      border-radius: 0.625rem;
      border: 2px solid #ffffff;
    }

    .badge-primary {
      background-color: #3b82f6;
    }

    .badge-secondary {
      background-color: #6b7280;
    }

    .badge-success {
      background-color: #10b981;
    }

    .badge-warning {
      background-color: #f59e0b;
    }

    .badge-info {
      background-color: #06b6d4;
    }

    /* Mobile optimizations */
    .btn-mobile {
      min-height: 44px;
      padding: 0.75rem 1rem;
      font-size: 1rem;
    }

    .btn-mobile.btn-sm {
      min-height: 36px;
      padding: 0.5rem 0.75rem;
      font-size: 0.875rem;
    }

    .btn-mobile.btn-lg {
      min-height: 52px;
      padding: 1rem 1.5rem;
      font-size: 1.125rem;
    }

    .btn-mobile.btn-xl {
      min-height: 60px;
      padding: 1.25rem 2rem;
      font-size: 1.25rem;
    }

    /* Full width */
    .btn-block {
      width: 100%;
    }

    /* Rounded variants */
    .btn-rounded {
      border-radius: 9999px;
    }

    /* Elevated (shadow) */
    .btn-elevated {
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
    }

    .btn-elevated:hover:not(:disabled) {
      box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);
      transform: translateY(-1px);
    }

    /* Animations */
    @keyframes spin {
      from {
        transform: rotate(0deg);
      }
      to {
        transform: rotate(360deg);
      }
    }

    @keyframes spinCircle {
      0% {
        stroke-dashoffset: 31.416;
      }
      50% {
        stroke-dashoffset: 7.854;
      }
      100% {
        stroke-dashoffset: 31.416;
      }
    }

    /* Utility classes */
    .visually-hidden {
      position: absolute !important;
      width: 1px !important;
      height: 1px !important;
      padding: 0 !important;
      margin: -1px !important;
      overflow: hidden !important;
      clip: rect(0, 0, 0, 0) !important;
      white-space: nowrap !important;
      border: 0 !important;
    }

    /* Responsive adjustments */
    @media (max-width: 576px) {
      button {
        font-size: 1rem;
        min-height: 44px;
      }
      
      .btn-sm {
        font-size: 0.875rem;
        min-height: 36px;
      }
      
      .btn-lg {
        font-size: 1.125rem;
        min-height: 52px;
      }
      
      .btn-xl {
        font-size: 1.25rem;
        min-height: 60px;
      }
    }

    /* High contrast mode */
    @media (prefers-contrast: high) {
      button {
        border-width: 2px;
      }
      
      button:focus {
        outline: 3px solid;
        outline-offset: 2px;
      }
    }

    /* Reduced motion */
    @media (prefers-reduced-motion: reduce) {
      button {
        transition: none;
      }
      
      button:active:not(:disabled) {
        transform: none;
      }
      
      .spinner,
      .spinner-circle {
        animation: none;
      }
      
      .btn-elevated:hover:not(:disabled) {
        transform: none;
      }
    }

    /* Dark mode */
    @media (prefers-color-scheme: dark) {
      .btn-light {
        color: #f9fafb;
        background-color: #374151;
        border-color: #4b5563;
      }
      
      .btn-light:hover:not(:disabled) {
        background-color: #4b5563;
        border-color: #6b7280;
      }
      
      .btn-ghost {
        color: #f9fafb;
      }
      
      .btn-ghost:hover:not(:disabled) {
        background-color: #374151;
      }
    }
  `]
})
export class ButtonComponent implements OnInit, OnDestroy {
  private readonly responsiveService = inject(ResponsiveService);
  private destroy$ = new Subject<void>();

  @Input() variant: ButtonVariant = 'primary';
  @Input() size: ButtonSize = 'md';
  @Input() type: ButtonType = 'button';
  @Input() text: string = '';
  @Input() loading: boolean = false;
  @Input() loadingText: string = '';
  @Input() hideTextOnLoading: boolean = false;
  @Input() disabled: boolean = false;
  @Input() iconLeft: string = '';
  @Input() iconRight: string = '';
  @Input() badge: string = '';
  @Input() badgeVariant: ButtonVariant = 'danger';
  @Input() block: boolean = false;
  @Input() rounded: boolean = false;
  @Input() elevated: boolean = false;
  @Input() ariaLabel: string = '';
  @Input() ariaDescribedBy: string = '';

  @Output() buttonClick = new EventEmitter<Event>();
  @Output() buttonFocus = new EventEmitter<Event>();
  @Output() buttonBlur = new EventEmitter<Event>();

  isMobile = false;
  hasContent = false;

  ngOnInit(): void {
    this.responsiveService.isMobile$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(isMobile => {
      this.isMobile = isMobile;
    });

    // Check if there's content projected
    this.hasContent = true; // This would need to be determined by checking ng-content
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onClick(event: Event): void {
    if (!this.disabled && !this.loading) {
      this.buttonClick.emit(event);
    }
  }

  onFocus(event: Event): void {
    this.buttonFocus.emit(event);
  }

  onBlur(event: Event): void {
    this.buttonBlur.emit(event);
  }

  get buttonClasses(): string {
    const classes = [
      `btn-${this.variant}`,
      `btn-${this.size}`
    ];

    if (this.isMobile) {
      classes.push('btn-mobile');
    }

    if (this.block) {
      classes.push('btn-block');
    }

    if (this.rounded) {
      classes.push('btn-rounded');
    }

    if (this.elevated) {
      classes.push('btn-elevated');
    }

    if (this.loading) {
      classes.push('btn-loading');
    }

    return classes.join(' ');
  }

  get badgeClasses(): string {
    return `badge-${this.badgeVariant}`;
  }
}