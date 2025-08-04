import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

export interface MobileNavItem {
  label: string;
  icon: string;
  route?: string;
  action?: () => void;
  badge?: string;
  active?: boolean;
  disabled?: boolean;
}

@Component({
  selector: 'app-mobile-nav',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <!-- Bottom Navigation for Mobile -->
    <nav class="mobile-bottom-nav d-md-none" *ngIf="showBottomNav">
      <div class="nav-container">
        <a 
          *ngFor="let item of bottomNavItems; trackBy: trackByLabel"
          [routerLink]="item.route"
          [class.active]="item.active"
          [class.disabled]="item.disabled"
          class="nav-item"
          (click)="onNavItemClick(item)"
          [attr.aria-label]="item.label">
          <div class="nav-icon">
            <i [class]="item.icon"></i>
            <span *ngIf="item.badge" class="nav-badge">{{ item.badge }}</span>
          </div>
          <span class="nav-label">{{ item.label }}</span>
        </a>
      </div>
    </nav>

    <!-- Floating Action Button -->
    <div class="fab-container" *ngIf="showFab && fabAction">
      <button 
        class="fab"
        [class.fab-extended]="fabExtended"
        (click)="onFabClick()"
        [attr.aria-label]="fabLabel">
        <i [class]="fabIcon"></i>
        <span *ngIf="fabExtended && fabLabel" class="fab-text">{{ fabLabel }}</span>
      </button>
    </div>

    <!-- Quick Actions Menu -->
    <div class="quick-actions-overlay" 
         *ngIf="showQuickActions" 
         (click)="closeQuickActions()">
      <div class="quick-actions-menu" (click)="$event.stopPropagation()">
        <div class="quick-actions-header">
          <h6>Quick Actions</h6>
          <button class="btn-close" (click)="closeQuickActions()"></button>
        </div>
        <div class="quick-actions-grid">
          <button 
            *ngFor="let action of quickActionItems"
            class="quick-action-item"
            (click)="onQuickActionClick(action)"
            [disabled]="action.disabled">
            <div class="action-icon" [style.background-color]="action.color || 'var(--primary)'">
              <i [class]="action.icon"></i>
            </div>
            <span class="action-label">{{ action.label }}</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Swipe Gesture Indicator -->
    <div class="swipe-indicator" *ngIf="showSwipeIndicator">
      <div class="swipe-line"></div>
      <span class="swipe-text">{{ swipeText }}</span>
    </div>
  `,
  styles: [`
    /* Bottom Navigation */
    .mobile-bottom-nav {
      position: fixed;
      bottom: 0;
      left: 0;
      right: 0;
      background: white;
      border-top: 1px solid var(--gray-200);
      box-shadow: 0 -2px 8px rgba(0, 0, 0, 0.1);
      z-index: 1000;
      padding: env(safe-area-inset-bottom) 0 0 0;
    }

    .nav-container {
      display: flex;
      justify-content: space-around;
      align-items: center;
      padding: 0.5rem 0;
      max-width: 100%;
    }

    .nav-item {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 0.5rem 0.75rem;
      text-decoration: none;
      color: var(--gray-500);
      transition: all 0.2s ease;
      border-radius: 12px;
      min-width: 60px;
      position: relative;
      -webkit-tap-highlight-color: rgba(0, 0, 0, 0.1);
      touch-action: manipulation;
    }

    .nav-item:hover,
    .nav-item.active {
      color: var(--primary);
      background-color: rgba(59, 130, 246, 0.1);
    }

    .nav-item.disabled {
      opacity: 0.5;
      pointer-events: none;
    }

    .nav-item:active {
      transform: scale(0.95);
    }

    .nav-icon {
      position: relative;
      font-size: 1.25rem;
      margin-bottom: 0.25rem;
    }

    .nav-label {
      font-size: 0.75rem;
      font-weight: 500;
      text-align: center;
      line-height: 1.2;
    }

    .nav-badge {
      position: absolute;
      top: -8px;
      right: -8px;
      background: var(--danger);
      color: white;
      border-radius: 50%;
      width: 18px;
      height: 18px;
      font-size: 0.7rem;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 600;
    }

    /* Floating Action Button */
    .fab-container {
      position: fixed;
      bottom: 80px;
      right: 1rem;
      z-index: 1001;
    }

    .fab {
      width: 56px;
      height: 56px;
      border-radius: 50%;
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      color: white;
      border: none;
      box-shadow: 0 4px 12px rgba(59, 130, 246, 0.4);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.5rem;
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      cursor: pointer;
      -webkit-tap-highlight-color: rgba(0, 0, 0, 0.1);
    }

    .fab:hover {
      transform: translateY(-2px);
      box-shadow: 0 6px 16px rgba(59, 130, 246, 0.5);
    }

    .fab:active {
      transform: translateY(0) scale(0.95);
    }

    .fab-extended {
      width: auto;
      padding: 0 1.5rem;
      border-radius: 28px;
      gap: 0.5rem;
    }

    .fab-text {
      font-weight: 500;
      font-size: 0.9rem;
    }

    /* Quick Actions Menu */
    .quick-actions-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.5);
      z-index: 1050;
      display: flex;
      align-items: flex-end;
      animation: fadeIn 0.3s ease;
    }

    .quick-actions-menu {
      background: white;
      border-radius: 20px 20px 0 0;
      padding: 1.5rem;
      width: 100%;
      max-height: 70vh;
      overflow-y: auto;
      animation: slideUp 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    }

    .quick-actions-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
      padding-bottom: 1rem;
      border-bottom: 1px solid var(--gray-200);
    }

    .quick-actions-header h6 {
      margin: 0;
      font-weight: 600;
      color: var(--text-primary);
    }

    .btn-close {
      background: none;
      border: none;
      font-size: 1.5rem;
      color: var(--gray-500);
      cursor: pointer;
      padding: 0.5rem;
      border-radius: 50%;
      transition: all 0.2s ease;
    }

    .btn-close:hover {
      background-color: var(--gray-100);
      color: var(--gray-700);
    }

    .quick-actions-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(80px, 1fr));
      gap: 1rem;
    }

    .quick-action-item {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 1rem 0.5rem;
      background: none;
      border: none;
      border-radius: 12px;
      transition: all 0.2s ease;
      cursor: pointer;
      -webkit-tap-highlight-color: rgba(0, 0, 0, 0.05);
    }

    .quick-action-item:hover {
      background-color: var(--gray-50);
    }

    .quick-action-item:active {
      transform: scale(0.95);
    }

    .quick-action-item:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .action-icon {
      width: 48px;
      height: 48px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 1.25rem;
      margin-bottom: 0.5rem;
    }

    .action-label {
      font-size: 0.8rem;
      font-weight: 500;
      color: var(--text-primary);
      text-align: center;
      line-height: 1.2;
    }

    /* Swipe Indicator */
    .swipe-indicator {
      position: fixed;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      background: rgba(0, 0, 0, 0.8);
      color: white;
      padding: 1rem 1.5rem;
      border-radius: 12px;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.5rem;
      z-index: 1100;
      animation: fadeIn 0.3s ease;
    }

    .swipe-line {
      width: 40px;
      height: 4px;
      background: white;
      border-radius: 2px;
      opacity: 0.7;
    }

    .swipe-text {
      font-size: 0.9rem;
      font-weight: 500;
    }

    /* Animations */
    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }

    @keyframes slideUp {
      from { transform: translateY(100%); }
      to { transform: translateY(0); }
    }

    /* Safe area adjustments */
    @supports (padding-bottom: env(safe-area-inset-bottom)) {
      .mobile-bottom-nav {
        padding-bottom: env(safe-area-inset-bottom);
      }
    }

    /* Landscape orientation adjustments */
    @media (orientation: landscape) and (max-height: 500px) {
      .fab-container {
        bottom: 60px;
        right: 0.5rem;
      }
      
      .fab {
        width: 48px;
        height: 48px;
        font-size: 1.25rem;
      }
      
      .nav-item {
        padding: 0.375rem 0.5rem;
      }
      
      .nav-icon {
        font-size: 1.1rem;
        margin-bottom: 0.125rem;
      }
      
      .nav-label {
        font-size: 0.7rem;
      }
    }
  `]
})
export class MobileNavComponent {
  @Input() showBottomNav: boolean = true;
  @Input() bottomNavItems: MobileNavItem[] = [];
  @Input() showFab: boolean = false;
  @Input() fabIcon: string = 'fas fa-plus';
  @Input() fabLabel: string = '';
  @Input() fabExtended: boolean = false;
  @Input() fabAction: (() => void) | null = null;
  @Input() showQuickActions: boolean = false;
  @Input() quickActionItems: any[] = [];
  @Input() showSwipeIndicator: boolean = false;
  @Input() swipeText: string = 'Swipe to navigate';

  @Output() navItemClick = new EventEmitter<MobileNavItem>();
  @Output() fabClick = new EventEmitter<void>();
  @Output() quickActionClick = new EventEmitter<any>();
  @Output() quickActionsClose = new EventEmitter<void>();

  onNavItemClick(item: MobileNavItem): void {
    if (!item.disabled) {
      if (item.action) {
        item.action();
      }
      this.navItemClick.emit(item);
    }
  }

  onFabClick(): void {
    if (this.fabAction) {
      this.fabAction();
    }
    this.fabClick.emit();
  }

  onQuickActionClick(action: any): void {
    if (!action.disabled) {
      if (action.action) {
        action.action();
      }
      this.quickActionClick.emit(action);
      this.closeQuickActions();
    }
  }

  closeQuickActions(): void {
    this.quickActionsClose.emit();
  }

  trackByLabel(index: number, item: MobileNavItem): string {
    return item.label;
  }
}