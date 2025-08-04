import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { PwaInstallPromptComponent } from './shared/components/pwa-install-prompt/pwa-install-prompt.component';
import { OfflineIndicatorComponent } from './shared/components/offline-indicator/offline-indicator.component';
import { PwaService } from './services/pwa.service';
import { PushNotificationService } from './services/push-notification.service';

@Component({
    selector: 'app-root',
    imports: [
      CommonModule, 
      RouterOutlet, 
      PwaInstallPromptComponent, 
      OfflineIndicatorComponent
    ],
    template: `
    <!-- PWA Install Prompt -->
    <app-pwa-install-prompt></app-pwa-install-prompt>
    
    <!-- Offline Status Indicator -->
    <app-offline-indicator></app-offline-indicator>
    
    <!-- Main Application -->
    <router-outlet></router-outlet>
  `,
    styles: []
})
export class AppComponent implements OnInit {
  title = 'StrideHR';

  constructor(
    private pwaService: PwaService,
    private pushNotificationService: PushNotificationService
  ) {}

  ngOnInit(): void {
    // Initialize PWA features
    this.initializePwaFeatures();
  }

  private async initializePwaFeatures(): Promise<void> {
    try {
      // Request notification permission if supported
      if (this.pushNotificationService.isSupported()) {
        await this.pushNotificationService.requestPermission();
      }

      // Subscribe to push notifications if permission granted
      if (Notification.permission === 'granted') {
        await this.pushNotificationService.subscribe();
      }

      // Show welcome notification for first-time users
      if (this.pwaService.isStandalone() && !localStorage.getItem('pwa-welcome-shown')) {
        await this.pwaService.showNotification('Welcome to StrideHR!', {
          body: 'You can now use StrideHR offline and receive push notifications.',
          tag: 'welcome-notification'
        });
        localStorage.setItem('pwa-welcome-shown', 'true');
      }
    } catch (error) {
      console.error('Error initializing PWA features:', error);
    }
  }
}