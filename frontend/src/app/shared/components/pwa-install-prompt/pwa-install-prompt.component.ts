import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { PwaService } from '../../../services/pwa.service';

@Component({
  selector: 'app-pwa-install-prompt',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="pwa-install-banner" *ngIf="showInstallPrompt && !isStandalone">
      <div class="container-fluid">
        <div class="row align-items-center">
          <div class="col-auto">
            <img src="/icons/icon-72x72.png" alt="StrideHR" class="app-icon">
          </div>
          <div class="col">
            <h6 class="mb-1">Install StrideHR</h6>
            <p class="mb-0 text-muted small">
              Get the full app experience with offline access and push notifications
            </p>
          </div>
          <div class="col-auto">
            <button 
              class="btn btn-primary btn-sm me-2" 
              (click)="installApp()"
              [disabled]="installing">
              <i class="fas fa-download me-1" *ngIf="!installing"></i>
              <i class="fas fa-spinner fa-spin me-1" *ngIf="installing"></i>
              {{ installing ? 'Installing...' : 'Install' }}
            </button>
            <button 
              class="btn btn-outline-secondary btn-sm" 
              (click)="dismissPrompt()">
              <i class="fas fa-times"></i>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Update Available Banner -->
    <div class="pwa-update-banner" *ngIf="updateAvailable">
      <div class="container-fluid">
        <div class="row align-items-center">
          <div class="col-auto">
            <i class="fas fa-sync-alt text-info"></i>
          </div>
          <div class="col">
            <h6 class="mb-1">Update Available</h6>
            <p class="mb-0 text-muted small">
              A new version of StrideHR is available
            </p>
          </div>
          <div class="col-auto">
            <button 
              class="btn btn-info btn-sm me-2" 
              (click)="applyUpdate()"
              [disabled]="updating">
              <i class="fas fa-download me-1" *ngIf="!updating"></i>
              <i class="fas fa-spinner fa-spin me-1" *ngIf="updating"></i>
              {{ updating ? 'Updating...' : 'Update' }}
            </button>
            <button 
              class="btn btn-outline-secondary btn-sm" 
              (click)="dismissUpdate()">
              Later
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Offline Status -->
    <div class="offline-status" *ngIf="!isOnline">
      <div class="container-fluid">
        <div class="row align-items-center">
          <div class="col-auto">
            <i class="fas fa-wifi text-warning"></i>
          </div>
          <div class="col">
            <span class="text-warning">
              <strong>You're offline</strong> - Some features may be limited
            </span>
          </div>
          <div class="col-auto" *ngIf="pendingActions > 0">
            <span class="badge bg-warning text-dark">
              {{ pendingActions }} pending sync{{ pendingActions > 1 ? 's' : '' }}
            </span>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .pwa-install-banner,
    .pwa-update-banner {
      background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
      color: white;
      padding: 0.75rem 0;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      position: sticky;
      top: 0;
      z-index: 1030;
    }

    .pwa-update-banner {
      background: linear-gradient(135deg, #06b6d4 0%, #0891b2 100%);
    }

    .offline-status {
      background-color: #fef3c7;
      border-bottom: 1px solid #f59e0b;
      padding: 0.5rem 0;
      position: sticky;
      top: 0;
      z-index: 1029;
    }

    .app-icon {
      width: 40px;
      height: 40px;
      border-radius: 8px;
    }

    .btn-sm {
      font-size: 0.875rem;
      padding: 0.375rem 0.75rem;
    }

    h6 {
      font-weight: 600;
      margin-bottom: 0.25rem;
    }

    .text-muted {
      opacity: 0.8;
    }

    @media (max-width: 768px) {
      .pwa-install-banner .col,
      .pwa-update-banner .col {
        margin-bottom: 0.5rem;
      }
      
      .pwa-install-banner .col-auto:last-child,
      .pwa-update-banner .col-auto:last-child {
        margin-top: 0.5rem;
      }
    }
  `]
})
export class PwaInstallPromptComponent implements OnInit, OnDestroy {
  showInstallPrompt = false;
  updateAvailable = false;
  isOnline = true;
  isStandalone = false;
  installing = false;
  updating = false;
  pendingActions = 0;

  private destroy$ = new Subject<void>();

  constructor(private pwaService: PwaService) {}

  ngOnInit(): void {
    this.isStandalone = this.pwaService.isStandalone();

    // Subscribe to PWA service observables
    this.pwaService.canInstall$
      .pipe(takeUntil(this.destroy$))
      .subscribe(canInstall => {
        this.showInstallPrompt = canInstall && !this.isStandalone;
      });

    this.pwaService.updateAvailable$
      .pipe(takeUntil(this.destroy$))
      .subscribe(available => {
        this.updateAvailable = available;
      });

    this.pwaService.isOnline$
      .pipe(takeUntil(this.destroy$))
      .subscribe(online => {
        this.isOnline = online;
      });

    // Check for pending offline actions
    // This would be connected to your offline storage service
    // this.offlineStorageService.pendingActions$
    //   .pipe(takeUntil(this.destroy$))
    //   .subscribe(actions => {
    //     this.pendingActions = actions.filter(a => !a.synced).length;
    //   });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  async installApp(): Promise<void> {
    this.installing = true;
    
    try {
      const installed = await this.pwaService.promptInstall();
      if (installed) {
        this.showInstallPrompt = false;
      }
    } catch (error) {
      console.error('Error installing app:', error);
    } finally {
      this.installing = false;
    }
  }

  dismissPrompt(): void {
    this.showInstallPrompt = false;
    // Store dismissal in localStorage to avoid showing again for a while
    localStorage.setItem('pwa-install-dismissed', Date.now().toString());
  }

  async applyUpdate(): Promise<void> {
    this.updating = true;
    
    try {
      await this.pwaService.applyUpdate();
    } catch (error) {
      console.error('Error applying update:', error);
    } finally {
      this.updating = false;
    }
  }

  dismissUpdate(): void {
    this.updateAvailable = false;
  }
}