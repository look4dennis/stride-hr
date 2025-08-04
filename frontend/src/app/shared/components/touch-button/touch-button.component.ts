import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-touch-button',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button 
      [type]="type"
      [class]="getButtonClasses()"
      [disabled]="disabled || loading"
      (click)="onClick()"
      [attr.aria-label]="ariaLabel"
      [title]="title">
      
      <!-- Loading spinner -->
      <span *ngIf="loading" class="spinner-border spinner-border-sm me-2" role="status">
        <span class="visually-hidden">Loading...</span>
      </span>
      
      <!-- Icon -->
      <i *ngIf="icon && !loading" [class]="icon" [class.me-2]="hasContent()"></i>
      
      <!-- Content -->
      <ng-content></ng-content>
      
      <!-- Badge -->
      <span *ngIf="badge" class="badge bg-light text-dark ms-2">{{ badge }}</span>
    </button>
  `,
  styles: [`
    button {
      min-height: 44px;
      font-weight: 500;
      border-radius: 12px;
      transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
      touch-action: manipulation;
      -webkit-tap-highlight-color: rgba(0, 0, 0, 0.1);
      position: relative;
      overflow: hidden;
      border: 2px solid transparent;
      font-size: 1rem;
      padding: 0.75rem 1.5rem;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      text-decoration: none;
      cursor: pointer;
      user-select: none;
    }

    /* Size variants */
    .btn-sm {
      min-height: 36px;
      padding: 0.5rem 1rem;
      font-size: 0.875rem;
    }

    .btn-lg {
      min-height: 52px;
      padding: 0.875rem 2rem;
      font-size: 1.125rem;
    }

    /* Style variants */
    .btn-primary {
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      color: white;
      border-color: var(--primary);
    }

    .btn-primary:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(59, 130, 246, 0.4);
    }

    .btn-primary:active:not(:disabled) {
      transform: translateY(0) scale(0.98);
    }

    .btn-secondary {
      background-color: var(--secondary);
      color: white;
      border-color: var(--secondary);
    }

    .btn-secondary:hover:not(:disabled) {
      background-color: var(--gray-600);
      border-color: var(--gray-600);
      transform: translateY(-1px);
    }

    .btn-success {
      background: linear-gradient(135deg, var(--success) 0%, #059669 100%);
      color: white;
      border-color: var(--success);
    }

    .btn-success:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(16, 185, 129, 0.4);
    }

    .btn-warning {
      background: linear-gradient(135deg, var(--warning) 0%, #d97706 100%);
      color: white;
      border-color: var(--warning);
    }

    .btn-warning:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(245, 158, 11, 0.4);
    }

    .btn-danger {
      background: linear-gradient(135deg, var(--danger) 0%, #dc2626 100%);
      color: white;
      border-color: var(--danger);
    }

    .btn-danger:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(239, 68, 68, 0.4);
    }

    .btn-info {
      background: linear-gradient(135deg, var(--info) 0%, #0891b2 100%);
      color: white;
      border-color: var(--info);
    }

    .btn-info:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(6, 182, 212, 0.4);
    }

    /* Outline variants */
    .btn-outline-primary {
      background-color: transparent;
      color: var(--primary);
      border-color: var(--primary);
    }

    .btn-outline-primary:hover:not(:disabled) {
      background-color: var(--primary);
      color: white;
      transform: translateY(-1px);
    }

    .btn-outline-secondary {
      background-color: transparent;
      color: var(--secondary);
      border-color: var(--secondary);
    }

    .btn-outline-secondary:hover:not(:disabled) {
      background-color: var(--secondary);
      color: white;
      transform: translateY(-1px);
    }

    .btn-outline-success {
      background-color: transparent;
      color: var(--success);
      border-color: var(--success);
    }

    .btn-outline-success:hover:not(:disabled) {
      background-color: var(--success);
      color: white;
      transform: translateY(-1px);
    }

    .btn-outline-warning {
      background-color: transparent;
      color: var(--warning);
      border-color: var(--warning);
    }

    .btn-outline-warning:hover:not(:disabled) {
      background-color: var(--warning);
      color: white;
      transform: translateY(-1px);
    }

    .btn-outline-danger {
      background-color: transparent;
      color: var(--danger);
      border-color: var(--danger);
    }

    .btn-outline-danger:hover:not(:disabled) {
      background-color: var(--danger);
      color: white;
      transform: translateY(-1px);
    }

    .btn-outline-info {
      background-color: transparent;
      color: var(--info);
      border-color: var(--info);
    }

    .btn-outline-info:hover:not(:disabled) {
      background-color: var(--info);
      color: white;
      transform: translateY(-1px);
    }

    /* Special variants */
    .btn-rounded {
      border-radius: 50px;
    }

    .btn-block {
      width: 100%;
    }

    .btn-floating {
      width: 56px;
      height: 56px;
      border-radius: 50%;
      padding: 0;
      box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
    }

    .btn-floating:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 6px 16px rgba(0, 0, 0, 0.3);
    }

    /* Disabled state */
    button:disabled {
      opacity: 0.6;
      cursor: not-allowed;
      transform: none !important;
      box-shadow: none !important;
    }

    /* Focus state */
    button:focus {
      outline: 2px solid var(--primary);
      outline-offset: 2px;
    }

    /* Mobile optimizations */
    @media (max-width: 768px) {
      button {
        min-height: 48px;
        padding: 0.75rem 1.25rem;
      }
      
      .btn-sm {
        min-height: 40px;
        padding: 0.625rem 1rem;
      }
      
      .btn-lg {
        min-height: 56px;
        padding: 1rem 2rem;
        font-size: 1.125rem;
      }
      
      .btn-floating {
        width: 60px;
        height: 60px;
      }
    }

    /* Extra small screens */
    @media (max-width: 576px) {
      .btn-block-mobile {
        width: 100%;
      }
      
      button {
        font-size: 1rem;
      }
      
      .btn-sm {
        font-size: 0.9rem;
      }
    }

    /* High contrast mode support */
    @media (prefers-contrast: high) {
      button {
        border-width: 3px;
      }
    }

    /* Reduced motion support */
    @media (prefers-reduced-motion: reduce) {
      button {
        transition: none;
      }
      
      button:hover:not(:disabled) {
        transform: none;
      }
      
      button:active:not(:disabled) {
        transform: none;
      }
    }

    /* Badge styling */
    .badge {
      font-size: 0.75em;
      border-radius: 50px;
      padding: 0.25em 0.5em;
    }
  `]
})
export class TouchButtonComponent {
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Input() variant: 'primary' | 'secondary' | 'success' | 'warning' | 'danger' | 'info' = 'primary';
  @Input() outline: boolean = false;
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() disabled: boolean = false;
  @Input() loading: boolean = false;
  @Input() rounded: boolean = false;
  @Input() block: boolean = false;
  @Input() blockMobile: boolean = false;
  @Input() floating: boolean = false;
  @Input() icon: string = '';
  @Input() badge: string = '';
  @Input() ariaLabel: string = '';
  @Input() title: string = '';

  @Output() buttonClick = new EventEmitter<Event>();

  onClick(): void {
    if (!this.disabled && !this.loading) {
      this.buttonClick.emit();
    }
  }

  getButtonClasses(): string {
    const classes = ['btn'];
    
    // Variant
    if (this.outline) {
      classes.push(`btn-outline-${this.variant}`);
    } else {
      classes.push(`btn-${this.variant}`);
    }
    
    // Size
    if (this.size !== 'md') {
      classes.push(`btn-${this.size}`);
    }
    
    // Modifiers
    if (this.rounded) classes.push('btn-rounded');
    if (this.block) classes.push('btn-block');
    if (this.blockMobile) classes.push('btn-block-mobile');
    if (this.floating) classes.push('btn-floating');
    
    return classes.join(' ');
  }

  hasContent(): boolean {
    return true; // This will be determined by ng-content projection
  }
}