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

    @media (max-width: 768px) {
      .main-content {
        margin-left: 0;
      }
      
      .content-wrapper {
        padding: 1rem;
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
  `]
})
export class LayoutComponent {}