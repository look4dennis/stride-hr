import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ElementRef, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ResponsiveService } from '../../../core/services/responsive.service';
import { Subject, takeUntil } from 'rxjs';

export interface ModalConfig {
  size?: 'sm' | 'md' | 'lg' | 'xl' | 'fullscreen';
  centered?: boolean;
  backdrop?: boolean | 'static';
  keyboard?: boolean;
  scrollable?: boolean;
  animation?: boolean;
  mobileFullscreen?: boolean;
  showCloseButton?: boolean;
  closeOnBackdropClick?: boolean;
}

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div 
      class="modal-backdrop"
      [class.show]="isVisible"
      [class.fade]="config.animation !== false"
      *ngIf="isVisible && config.backdrop !== false"
      (click)="onBackdropClick()"
      [@fadeAnimation]="isVisible ? 'in' : 'out'">
    </div>
    
    <div 
      #modalElement
      class="modal"
      [class.show]="isVisible"
      [class.fade]="config.animation !== false"
      [class.modal-static]="config.backdrop === 'static'"
      [style.display]="isVisible ? 'block' : 'none'"
      tabindex="-1"
      role="dialog"
      [attr.aria-labelledby]="modalId + '-title'"
      [attr.aria-hidden]="!isVisible"
      [@slideAnimation]="isVisible ? 'in' : 'out'"
      (keydown.escape)="onEscapeKey($event)">
      
      <div 
        class="modal-dialog"
        [class.modal-dialog-centered]="config.centered !== false"
        [class.modal-dialog-scrollable]="config.scrollable"
        [class.modal-sm]="config.size === 'sm'"
        [class.modal-lg]="config.size === 'lg'"
        [class.modal-xl]="config.size === 'xl'"
        [class.modal-fullscreen]="config.size === 'fullscreen' || (isMobile && config.mobileFullscreen !== false)"
        [class.modal-fullscreen-sm-down]="isMobile && config.mobileFullscreen !== false"
        role="document">
        
        <div class="modal-content">
          <!-- Header -->
          <div class="modal-header" *ngIf="title || config.showCloseButton !== false">
            <h5 class="modal-title" [id]="modalId + '-title'" *ngIf="title">
              {{ title }}
            </h5>
            <button 
              type="button" 
              class="btn-close" 
              [attr.aria-label]="closeButtonLabel"
              (click)="close()"
              *ngIf="config.showCloseButton !== false">
            </button>
          </div>
          
          <!-- Body -->
          <div class="modal-body" [class.modal-body-scrollable]="config.scrollable">
            <ng-content></ng-content>
          </div>
          
          <!-- Footer -->
          <div class="modal-footer" *ngIf="hasFooterContent">
            <ng-content select="[slot=footer]"></ng-content>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    /* Modal backdrop */
    .modal-backdrop {
      position: fixed;
      top: 0;
      left: 0;
      z-index: 1040;
      width: 100vw;
      height: 100vh;
      background-color: rgba(0, 0, 0, 0.5);
      backdrop-filter: blur(2px);
      transition: opacity 0.15s linear;
      opacity: 0;
    }

    .modal-backdrop.show {
      opacity: 1;
    }

    .modal-backdrop.fade {
      transition: opacity 0.15s linear;
    }

    /* Modal */
    .modal {
      position: fixed;
      top: 0;
      left: 0;
      z-index: 1050;
      width: 100%;
      height: 100%;
      overflow-x: hidden;
      overflow-y: auto;
      outline: 0;
      transition: opacity 0.15s linear;
      opacity: 0;
    }

    .modal.show {
      opacity: 1;
    }

    .modal.fade {
      transition: opacity 0.15s linear, transform 0.15s ease-out;
      transform: translate(0, -50px);
    }

    .modal.fade.show {
      transform: translate(0, 0);
    }

    /* Modal dialog */
    .modal-dialog {
      position: relative;
      width: auto;
      margin: 0.5rem;
      pointer-events: none;
      transition: transform 0.3s ease-out;
    }

    .modal-dialog-centered {
      display: flex;
      align-items: center;
      min-height: calc(100% - 1rem);
    }

    .modal-dialog-scrollable {
      height: calc(100% - 1rem);
    }

    .modal-dialog-scrollable .modal-content {
      max-height: 100%;
      overflow: hidden;
    }

    .modal-dialog-scrollable .modal-body {
      overflow-y: auto;
    }

    /* Modal content */
    .modal-content {
      position: relative;
      display: flex;
      flex-direction: column;
      width: 100%;
      pointer-events: auto;
      background-color: #fff;
      background-clip: padding-box;
      border: 1px solid rgba(0, 0, 0, 0.125);
      border-radius: 0.5rem;
      box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
      outline: 0;
    }

    /* Modal header */
    .modal-header {
      display: flex;
      flex-shrink: 0;
      align-items: center;
      justify-content: space-between;
      padding: 1rem 1rem;
      border-bottom: 1px solid #dee2e6;
      border-top-left-radius: calc(0.5rem - 1px);
      border-top-right-radius: calc(0.5rem - 1px);
    }

    .modal-title {
      margin-bottom: 0;
      line-height: 1.5;
      font-weight: 500;
      color: var(--text-primary);
    }

    .btn-close {
      box-sizing: content-box;
      width: 1em;
      height: 1em;
      padding: 0.25em 0.25em;
      color: #000;
      background: transparent url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 16 16' fill='%23000'%3e%3cpath d='m.235.867 8.832 8.832m-8.832 0L8.067.867'/%3e%3c/svg%3e") center/1em auto no-repeat;
      border: 0;
      border-radius: 0.375rem;
      opacity: 0.5;
      cursor: pointer;
      transition: opacity 0.15s ease;
    }

    .btn-close:hover {
      opacity: 0.75;
    }

    .btn-close:focus {
      outline: 0;
      box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25);
      opacity: 1;
    }

    /* Modal body */
    .modal-body {
      position: relative;
      flex: 1 1 auto;
      padding: 1rem;
    }

    .modal-body-scrollable {
      max-height: 60vh;
      overflow-y: auto;
    }

    /* Modal footer */
    .modal-footer {
      display: flex;
      flex-wrap: wrap;
      flex-shrink: 0;
      align-items: center;
      justify-content: flex-end;
      padding: 0.75rem;
      border-top: 1px solid #dee2e6;
      border-bottom-right-radius: calc(0.5rem - 1px);
      border-bottom-left-radius: calc(0.5rem - 1px);
      gap: 0.5rem;
    }

    /* Size variations */
    .modal-sm {
      max-width: 300px;
    }

    .modal-lg {
      max-width: 800px;
    }

    .modal-xl {
      max-width: 1140px;
    }

    .modal-fullscreen {
      width: 100vw;
      max-width: none;
      height: 100%;
      margin: 0;
    }

    .modal-fullscreen .modal-content {
      height: 100%;
      border: 0;
      border-radius: 0;
    }

    /* Responsive design */
    @media (min-width: 576px) {
      .modal-dialog {
        max-width: 500px;
        margin: 1.75rem auto;
      }

      .modal-dialog-centered {
        min-height: calc(100% - 3.5rem);
      }

      .modal-dialog-scrollable {
        height: calc(100% - 3.5rem);
      }

      .modal-sm {
        max-width: 300px;
      }
    }

    @media (min-width: 992px) {
      .modal-lg,
      .modal-xl {
        max-width: 800px;
      }
    }

    @media (min-width: 1200px) {
      .modal-xl {
        max-width: 1140px;
      }
    }

    /* Mobile optimizations */
    @media (max-width: 575.98px) {
      .modal-dialog {
        margin: 0.5rem;
      }

      .modal-fullscreen-sm-down {
        width: 100vw;
        max-width: none;
        height: 100%;
        margin: 0;
      }

      .modal-fullscreen-sm-down .modal-content {
        height: 100%;
        border: 0;
        border-radius: 0;
      }

      .modal-header {
        padding: 1rem 1rem 0.5rem;
      }

      .modal-body {
        padding: 0.75rem 1rem;
      }

      .modal-footer {
        padding: 0.5rem 1rem 1rem;
        flex-direction: column;
        align-items: stretch;
      }

      .modal-footer > * {
        width: 100%;
        margin-bottom: 0.5rem;
      }

      .modal-footer > *:last-child {
        margin-bottom: 0;
      }
    }

    /* Touch improvements */
    @media (hover: none) and (pointer: coarse) {
      .btn-close {
        width: 44px;
        height: 44px;
        padding: 0.5rem;
      }
    }

    /* Dark mode support */
    @media (prefers-color-scheme: dark) {
      .modal-content {
        background-color: var(--dark-bg, #1a1a1a);
        border-color: var(--dark-border, #333);
        color: var(--dark-text, #fff);
      }

      .modal-header {
        border-bottom-color: var(--dark-border, #333);
      }

      .modal-footer {
        border-top-color: var(--dark-border, #333);
      }

      .btn-close {
        filter: invert(1) grayscale(100%) brightness(200%);
      }
    }

    /* Animation improvements */
    .modal-static {
      animation: modalStaticShake 0.3s ease-in-out;
    }

    @keyframes modalStaticShake {
      0%, 100% { transform: translateX(0); }
      25% { transform: translateX(-5px); }
      75% { transform: translateX(5px); }
    }

    /* Accessibility improvements */
    .modal:focus {
      outline: none;
    }

    .modal-content:focus {
      outline: 2px solid var(--primary, #007bff);
      outline-offset: 2px;
    }

    /* High contrast mode */
    @media (prefers-contrast: high) {
      .modal-content {
        border: 2px solid;
      }

      .btn-close {
        border: 1px solid;
      }
    }

    /* Reduced motion */
    @media (prefers-reduced-motion: reduce) {
      .modal,
      .modal-backdrop {
        transition: none;
      }

      .modal-dialog {
        transition: none;
      }
    }
  `],
  animations: [
    // Add Angular animations here if needed
  ]
})
export class ModalComponent implements OnInit, OnDestroy {
  private readonly responsiveService = inject(ResponsiveService);
  private destroy$ = new Subject<void>();

  @ViewChild('modalElement') modalElement!: ElementRef;

  @Input() isVisible: boolean = false;
  @Input() title: string = '';
  @Input() modalId: string = 'modal-' + Math.random().toString(36).substr(2, 9);
  @Input() closeButtonLabel: string = 'Close';
  @Input() config: ModalConfig = {};
  @Input() hasFooterContent: boolean = false;

  @Output() modalClose = new EventEmitter<void>();
  @Output() modalDismiss = new EventEmitter<void>();
  @Output() modalShow = new EventEmitter<void>();
  @Output() modalHide = new EventEmitter<void>();

  isMobile = false;

  ngOnInit(): void {
    // Set default config
    this.config = {
      size: 'md',
      centered: true,
      backdrop: true,
      keyboard: true,
      scrollable: false,
      animation: true,
      mobileFullscreen: true,
      showCloseButton: true,
      closeOnBackdropClick: true,
      ...this.config
    };

    // Subscribe to responsive changes
    this.responsiveService.isMobile$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(isMobile => {
      this.isMobile = isMobile;
    });

    // Handle body scroll lock
    if (this.isVisible) {
      this.lockBodyScroll();
      this.modalShow.emit();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.unlockBodyScroll();
  }

  ngOnChanges(): void {
    if (this.isVisible) {
      this.lockBodyScroll();
      this.modalShow.emit();
      // Focus the modal for accessibility
      setTimeout(() => {
        if (this.modalElement?.nativeElement) {
          this.modalElement.nativeElement.focus();
        }
      }, 100);
    } else {
      this.unlockBodyScroll();
      this.modalHide.emit();
    }
  }

  close(): void {
    this.isVisible = false;
    this.modalClose.emit();
  }

  dismiss(): void {
    this.isVisible = false;
    this.modalDismiss.emit();
  }

  onBackdropClick(): void {
    if (this.config.closeOnBackdropClick !== false) {
      if (this.config.backdrop === 'static') {
        // Add shake animation for static backdrop
        this.modalElement?.nativeElement?.classList.add('modal-static');
        setTimeout(() => {
          this.modalElement?.nativeElement?.classList.remove('modal-static');
        }, 300);
      } else {
        this.dismiss();
      }
    }
  }

  onEscapeKey(event: Event): void {
    const keyboardEvent = event as KeyboardEvent;
    if (this.config.keyboard !== false && keyboardEvent.key === 'Escape') {
      this.dismiss();
    }
  }

  private lockBodyScroll(): void {
    if (typeof document !== 'undefined') {
      document.body.style.overflow = 'hidden';
      document.body.style.paddingRight = this.getScrollbarWidth() + 'px';
    }
  }

  private unlockBodyScroll(): void {
    if (typeof document !== 'undefined') {
      document.body.style.overflow = '';
      document.body.style.paddingRight = '';
    }
  }

  private getScrollbarWidth(): number {
    if (typeof window === 'undefined') return 0;
    
    const outer = document.createElement('div');
    outer.style.visibility = 'hidden';
    outer.style.overflow = 'scroll';
    document.body.appendChild(outer);

    const inner = document.createElement('div');
    outer.appendChild(inner);

    const scrollbarWidth = outer.offsetWidth - inner.offsetWidth;
    outer.parentNode?.removeChild(outer);

    return scrollbarWidth;
  }
}