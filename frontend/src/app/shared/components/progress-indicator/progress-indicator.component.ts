import { Component, Input, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ResponsiveService } from '../../../core/services/responsive.service';
import { Subject, takeUntil } from 'rxjs';

export type ProgressType = 'linear' | 'circular' | 'dots' | 'pulse' | 'skeleton';
export type ProgressSize = 'sm' | 'md' | 'lg' | 'xl';
export type ProgressVariant = 'primary' | 'secondary' | 'success' | 'warning' | 'danger' | 'info';

@Component({
  selector: 'app-progress-indicator',
  standalone: true,
  imports: [CommonModule],
  template: `
    <!-- Linear Progress -->
    <div *ngIf="type === 'linear'" class="progress-linear" [class]="progressClasses">
      <div class="progress-bar" [style.width.%]="value" [attr.aria-valuenow]="value">
        <span *ngIf="showLabel" class="progress-label">{{ label || value + '%' }}</span>
      </div>
      <div *ngIf="indeterminate" class="progress-bar-indeterminate"></div>
    </div>

    <!-- Circular Progress -->
    <div *ngIf="type === 'circular'" class="progress-circular" [class]="progressClasses">
      <svg class="progress-circle" [attr.width]="circleSize" [attr.height]="circleSize" viewBox="0 0 100 100">
        <circle
          class="progress-circle-bg"
          cx="50"
          cy="50"
          [attr.r]="circleRadius"
          fill="none"
          [attr.stroke-width]="strokeWidth">
        </circle>
        <circle
          class="progress-circle-fill"
          cx="50"
          cy="50"
          [attr.r]="circleRadius"
          fill="none"
          [attr.stroke-width]="strokeWidth"
          [attr.stroke-dasharray]="circumference"
          [attr.stroke-dashoffset]="strokeDashoffset"
          [class.indeterminate]="indeterminate">
        </circle>
      </svg>
      <div *ngIf="showLabel" class="progress-circle-label">
        {{ label || (indeterminate ? '' : value + '%') }}
      </div>
    </div>

    <!-- Dots Progress -->
    <div *ngIf="type === 'dots'" class="progress-dots" [class]="progressClasses">
      <div class="dot" [style.animation-delay]="i * 0.1 + 's'" *ngFor="let dot of dots; let i = index"></div>
    </div>

    <!-- Pulse Progress -->
    <div *ngIf="type === 'pulse'" class="progress-pulse" [class]="progressClasses">
      <div class="pulse-ring" [style.animation-delay]="i * 0.2 + 's'" *ngFor="let ring of pulseRings; let i = index"></div>
    </div>

    <!-- Skeleton Progress -->
    <div *ngIf="type === 'skeleton'" class="progress-skeleton" [class]="progressClasses">
      <div class="skeleton-line" [style.width]="line.width" *ngFor="let line of skeletonLines"></div>
    </div>

    <!-- Progress Text -->
    <div *ngIf="showText && text" class="progress-text" [class]="textClasses">
      {{ text }}
    </div>
  `,
  styles: [`
    /* Base styles */
    .progress-linear,
    .progress-circular,
    .progress-dots,
    .progress-pulse,
    .progress-skeleton {
      display: flex;
      align-items: center;
      justify-content: center;
    }

    /* Linear Progress */
    .progress-linear {
      width: 100%;
      height: 8px;
      background-color: var(--gray-200, #e5e7eb);
      border-radius: 4px;
      overflow: hidden;
      position: relative;
    }

    .progress-linear.size-sm {
      height: 4px;
    }

    .progress-linear.size-lg {
      height: 12px;
    }

    .progress-linear.size-xl {
      height: 16px;
    }

    .progress-bar {
      height: 100%;
      background: linear-gradient(90deg, var(--progress-color, #3b82f6), var(--progress-color-light, #60a5fa));
      border-radius: inherit;
      transition: width 0.3s ease;
      position: relative;
      display: flex;
      align-items: center;
      justify-content: center;
      min-width: 0;
    }

    .progress-bar-indeterminate {
      position: absolute;
      top: 0;
      left: 0;
      height: 100%;
      width: 30%;
      background: linear-gradient(90deg, transparent, var(--progress-color, #3b82f6), transparent);
      animation: indeterminate 2s infinite linear;
    }

    .progress-label {
      color: white;
      font-size: 0.75rem;
      font-weight: 500;
      text-shadow: 0 1px 2px rgba(0, 0, 0, 0.1);
    }

    /* Circular Progress */
    .progress-circular {
      position: relative;
      display: inline-flex;
      align-items: center;
      justify-content: center;
    }

    .progress-circle {
      transform: rotate(-90deg);
    }

    .progress-circle-bg {
      stroke: var(--gray-200, #e5e7eb);
    }

    .progress-circle-fill {
      stroke: var(--progress-color, #3b82f6);
      stroke-linecap: round;
      transition: stroke-dashoffset 0.3s ease;
    }

    .progress-circle-fill.indeterminate {
      animation: circularIndeterminate 2s infinite linear;
    }

    .progress-circle-label {
      position: absolute;
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--text-primary, #374151);
    }

    /* Dots Progress */
    .progress-dots {
      gap: 0.5rem;
    }

    .dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background-color: var(--progress-color, #3b82f6);
      animation: dotBounce 1.4s infinite ease-in-out;
    }

    .progress-dots.size-sm .dot {
      width: 6px;
      height: 6px;
    }

    .progress-dots.size-lg .dot {
      width: 12px;
      height: 12px;
    }

    .progress-dots.size-xl .dot {
      width: 16px;
      height: 16px;
    }

    /* Pulse Progress */
    .progress-pulse {
      position: relative;
    }

    .pulse-ring {
      width: 40px;
      height: 40px;
      border: 2px solid var(--progress-color, #3b82f6);
      border-radius: 50%;
      position: absolute;
      animation: pulseRing 2s infinite ease-out;
    }

    .progress-pulse.size-sm .pulse-ring {
      width: 30px;
      height: 30px;
    }

    .progress-pulse.size-lg .pulse-ring {
      width: 60px;
      height: 60px;
    }

    .progress-pulse.size-xl .pulse-ring {
      width: 80px;
      height: 80px;
    }

    /* Skeleton Progress */
    .progress-skeleton {
      flex-direction: column;
      gap: 0.5rem;
      width: 100%;
    }

    .skeleton-line {
      height: 12px;
      background: linear-gradient(90deg, var(--gray-200, #e5e7eb) 25%, var(--gray-100, #f3f4f6) 50%, var(--gray-200, #e5e7eb) 75%);
      background-size: 200% 100%;
      border-radius: 6px;
      animation: skeletonShimmer 2s infinite;
    }

    .progress-skeleton.size-sm .skeleton-line {
      height: 8px;
    }

    .progress-skeleton.size-lg .skeleton-line {
      height: 16px;
    }

    .progress-skeleton.size-xl .skeleton-line {
      height: 20px;
    }

    /* Progress Text */
    .progress-text {
      margin-top: 0.5rem;
      font-size: 0.875rem;
      color: var(--text-secondary, #6b7280);
      text-align: center;
    }

    .progress-text.size-sm {
      font-size: 0.75rem;
    }

    .progress-text.size-lg {
      font-size: 1rem;
    }

    .progress-text.size-xl {
      font-size: 1.125rem;
    }

    /* Color variants */
    .variant-primary {
      --progress-color: #3b82f6;
      --progress-color-light: #60a5fa;
    }

    .variant-secondary {
      --progress-color: #6b7280;
      --progress-color-light: #9ca3af;
    }

    .variant-success {
      --progress-color: #10b981;
      --progress-color-light: #34d399;
    }

    .variant-warning {
      --progress-color: #f59e0b;
      --progress-color-light: #fbbf24;
    }

    .variant-danger {
      --progress-color: #ef4444;
      --progress-color-light: #f87171;
    }

    .variant-info {
      --progress-color: #06b6d4;
      --progress-color-light: #22d3ee;
    }

    /* Animations */
    @keyframes indeterminate {
      0% {
        left: -30%;
      }
      100% {
        left: 100%;
      }
    }

    @keyframes circularIndeterminate {
      0% {
        stroke-dasharray: 1, 200;
        stroke-dashoffset: 0;
      }
      50% {
        stroke-dasharray: 100, 200;
        stroke-dashoffset: -15;
      }
      100% {
        stroke-dasharray: 100, 200;
        stroke-dashoffset: -125;
      }
    }

    @keyframes dotBounce {
      0%, 80%, 100% {
        transform: scale(0.8);
        opacity: 0.5;
      }
      40% {
        transform: scale(1);
        opacity: 1;
      }
    }

    @keyframes pulseRing {
      0% {
        transform: scale(0.8);
        opacity: 1;
      }
      100% {
        transform: scale(2);
        opacity: 0;
      }
    }

    @keyframes skeletonShimmer {
      0% {
        background-position: -200% 0;
      }
      100% {
        background-position: 200% 0;
      }
    }

    /* Responsive adjustments */
    @media (max-width: 576px) {
      .progress-circular {
        transform: scale(0.8);
      }
      
      .progress-text {
        font-size: 0.75rem;
      }
    }

    /* Reduced motion */
    @media (prefers-reduced-motion: reduce) {
      .progress-bar-indeterminate,
      .progress-circle-fill.indeterminate,
      .dot,
      .pulse-ring,
      .skeleton-line {
        animation: none;
      }
      
      .progress-bar {
        transition: none;
      }
    }

    /* High contrast mode */
    @media (prefers-contrast: high) {
      .progress-linear {
        border: 1px solid;
      }
      
      .progress-circle-bg,
      .progress-circle-fill {
        stroke-width: 3;
      }
    }

    /* Dark mode */
    @media (prefers-color-scheme: dark) {
      .progress-linear {
        background-color: var(--dark-gray-700, #374151);
      }
      
      .progress-circle-bg {
        stroke: var(--dark-gray-600, #4b5563);
      }
      
      .progress-text {
        color: var(--dark-text-secondary, #d1d5db);
      }
      
      .skeleton-line {
        background: linear-gradient(90deg, var(--dark-gray-700, #374151) 25%, var(--dark-gray-600, #4b5563) 50%, var(--dark-gray-700, #374151) 75%);
      }
    }
  `]
})
export class ProgressIndicatorComponent implements OnInit, OnDestroy {
  private readonly responsiveService = inject(ResponsiveService);
  private destroy$ = new Subject<void>();

  @Input() type: ProgressType = 'linear';
  @Input() value: number = 0;
  @Input() size: ProgressSize = 'md';
  @Input() variant: ProgressVariant = 'primary';
  @Input() indeterminate: boolean = false;
  @Input() showLabel: boolean = false;
  @Input() showText: boolean = false;
  @Input() label: string = '';
  @Input() text: string = '';
  @Input() strokeWidth: number = 4;

  // Computed properties
  dots = Array(3).fill(0);
  pulseRings = Array(3).fill(0);
  skeletonLines = [
    { width: '100%' },
    { width: '80%' },
    { width: '60%' }
  ];

  isMobile = false;

  ngOnInit(): void {
    this.responsiveService.isMobile$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(isMobile => {
      this.isMobile = isMobile;
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get progressClasses(): string {
    const classes = [
      `size-${this.size}`,
      `variant-${this.variant}`
    ];

    if (this.isMobile) {
      classes.push('mobile');
    }

    return classes.join(' ');
  }

  get textClasses(): string {
    return `size-${this.size}`;
  }

  get circleSize(): number {
    const sizes = {
      sm: 40,
      md: 60,
      lg: 80,
      xl: 100
    };
    return sizes[this.size];
  }

  get circleRadius(): number {
    return (this.circleSize - this.strokeWidth * 2) / 2;
  }

  get circumference(): number {
    return 2 * Math.PI * this.circleRadius;
  }

  get strokeDashoffset(): number {
    if (this.indeterminate) {
      return this.circumference * 0.75;
    }
    return this.circumference - (this.value / 100) * this.circumference;
  }
}