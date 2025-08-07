import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { NavigationService } from './core/services/navigation.service';
import { setupConsoleNavigationTest } from './shared/utils/console-navigation-test';
import './shared/utils/dashboard-test';
import './shared/utils/route-verification';
import './shared/utils/quick-fix-test';
import './shared/utils/navigation-test';
import './shared/utils/module-test';

@Component({
    selector: 'app-root',
    imports: [
      CommonModule, 
      RouterOutlet
    ],
    template: `
    <!-- Main Application -->
    <router-outlet></router-outlet>
  `,
    styles: []
})
export class AppComponent implements OnInit {
  title = 'StrideHR';

  constructor(private navigationService: NavigationService) {}

  ngOnInit() {
    // Initialize navigation service
    this.navigationService.setupKeyboardShortcuts();
    this.navigationService.restoreNavigationState();

    // Save navigation state on page unload
    window.addEventListener('beforeunload', () => {
      this.navigationService.saveNavigationState();
    });

    // Setup console navigation test functions
    if (typeof window !== 'undefined') {
      setupConsoleNavigationTest();
    }
  }
}