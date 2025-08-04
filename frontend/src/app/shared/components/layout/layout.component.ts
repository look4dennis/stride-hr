import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from '../header/header.component';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { NotificationComponent } from '../notification/notification.component';
import { LoadingComponent } from '../loading/loading.component';

@Component({
    selector: 'app-layout',
    imports: [
        CommonModule,
        RouterOutlet,
        HeaderComponent,
        SidebarComponent,
        NotificationComponent,
        LoadingComponent
    ],
    template: `
    <div class="app-layout">
      <app-header></app-header>
      <app-sidebar></app-sidebar>
      
      <main class="main-content">
        <div class="content-wrapper">
          <router-outlet></router-outlet>
        </div>
      </main>
      
      <app-notification></app-notification>
      <app-loading></app-loading>
    </div>
  `,
    styles: [`
    .app-layout {
      min-height: 100vh;
      background-color: var(--bg-secondary);
    }

    .main-content {
      margin-left: 250px;
      margin-top: 56px; /* Height of navbar */
      min-height: calc(100vh - 56px);
      transition: margin-left 0.3s ease;
    }

    .content-wrapper {
      padding: 2rem;
    }

    /* Mobile-first responsive design */
    @media (max-width: 768px) {
      .main-content {
        margin-left: 0;
        padding-top: 0;
      }
      
      .content-wrapper {
        padding: 1rem;
      }
    }

    /* Tablet adjustments */
    @media (min-width: 769px) and (max-width: 991px) {
      .content-wrapper {
        padding: 1.5rem;
      }
    }

    /* Adjust for collapsed sidebar */
    :host-context(.sidebar-collapsed) .main-content {
      margin-left: 60px;
    }

    @media (max-width: 768px) {
      :host-context(.sidebar-collapsed) .main-content {
        margin-left: 0;
      }
    }

    /* Improved mobile layout */
    @media (max-width: 576px) {
      .content-wrapper {
        padding: 0.75rem;
      }
    }

    /* Touch-friendly scrolling */
    .main-content {
      -webkit-overflow-scrolling: touch;
      overflow-x: hidden;
    }
  `]
})
export class LayoutComponent {}