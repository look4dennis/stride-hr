import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService, User } from '../../../core/auth/auth.service';

interface MenuItem {
  label: string;
  icon: string;
  route: string;
  roles?: string[];
  children?: MenuItem[];
}

@Component({
    selector: 'app-sidebar',
    imports: [CommonModule, RouterModule],
    template: `
    <div class="sidebar" [class.collapsed]="isCollapsed" *ngIf="currentUser">
      <div class="sidebar-header">
        <button 
          class="btn btn-link text-white p-0" 
          (click)="toggleSidebar()"
          title="Toggle Sidebar">
          <i class="fas fa-bars"></i>
        </button>
        <span class="sidebar-title" *ngIf="!isCollapsed">Menu</span>
      </div>
      
      <div class="sidebar-content">
        <ul class="nav flex-column">
          <li class="nav-item" *ngFor="let item of menuItems">
            <ng-container *ngIf="hasAccess(item)">
              <a 
                class="nav-link" 
                [routerLink]="item.route" 
                routerLinkActive="active"
                [title]="item.label"
                (click)="onNavigationClick(item, $event)">
                <i [class]="item.icon + ' me-2'"></i>
                <span *ngIf="!isCollapsed">{{ item.label }}</span>
              </a>
            </ng-container>
          </li>
        </ul>
      </div>
    </div>
    
    <!-- Overlay for mobile -->
    <div 
      class="sidebar-overlay" 
      *ngIf="!isCollapsed && isMobile" 
      (click)="closeSidebar()">
    </div>
  `,
    styles: [`
    .sidebar {
      position: fixed;
      top: 56px; /* Height of navbar */
      left: 0;
      height: calc(100vh - 56px);
      width: 250px;
      background: linear-gradient(180deg, var(--primary-dark) 0%, var(--primary) 100%);
      color: white;
      transition: all 0.3s ease;
      z-index: 1000;
      box-shadow: 2px 0 4px rgba(0, 0, 0, 0.1);
    }

    .sidebar.collapsed {
      width: 60px;
    }

    .sidebar-header {
      padding: 1rem;
      border-bottom: 1px solid rgba(255, 255, 255, 0.1);
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .sidebar-title {
      font-weight: 600;
      font-size: 1.1rem;
    }

    .sidebar-content {
      padding: 1rem 0;
    }

    .nav-link {
      color: rgba(255, 255, 255, 0.8) !important;
      padding: 0.75rem 1rem;
      border-radius: 0;
      transition: all 0.2s ease;
      display: flex;
      align-items: center;
    }

    .nav-link:hover {
      background-color: rgba(255, 255, 255, 0.1);
      color: white !important;
    }

    .nav-link.active {
      background-color: rgba(255, 255, 255, 0.2);
      color: white !important;
      border-right: 3px solid white;
    }

    .sidebar.collapsed .nav-link {
      justify-content: center;
      padding: 0.75rem;
    }

    .sidebar.collapsed .nav-link span {
      display: none;
    }

    .sidebar-overlay {
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background-color: rgba(0, 0, 0, 0.5);
      z-index: 999;
    }

    /* Mobile-first sidebar design */
    @media (max-width: 768px) {
      .sidebar {
        transform: translateX(-100%);
        width: 280px; /* Slightly wider on mobile for better touch targets */
        z-index: 1050; /* Higher z-index for mobile overlay */
      }
      
      .sidebar:not(.collapsed) {
        transform: translateX(0);
      }
      
      .sidebar-content {
        padding: 0.5rem 0;
      }
      
      .nav-link {
        padding: 1rem 1.5rem;
        font-size: 1rem;
      }
      
      .sidebar-header {
        padding: 1.25rem 1.5rem;
      }
    }

    /* Extra small screens */
    @media (max-width: 576px) {
      .sidebar {
        width: 100vw;
        max-width: 320px;
      }
    }

    /* Improved touch interactions */
    .nav-link {
      -webkit-tap-highlight-color: rgba(255, 255, 255, 0.1);
      touch-action: manipulation;
    }

    /* Better visual feedback for touch */
    .nav-link:active {
      background-color: rgba(255, 255, 255, 0.15);
      transform: scale(0.98);
    }

    /* Smooth animations for mobile */
    @media (max-width: 768px) {
      .sidebar {
        transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      }
      
      .sidebar-overlay {
        transition: opacity 0.3s ease;
      }
    }
  `]
})
export class SidebarComponent implements OnInit, OnDestroy {
  currentUser: User | null = null;
  isCollapsed = false;
  isMobile = false;
  private subscription: Subscription = new Subscription();

  menuItems: MenuItem[] = [
    {
      label: 'Dashboard',
      icon: 'fas fa-tachometer-alt',
      route: '/dashboard'
    },
    {
      label: 'Employees',
      icon: 'fas fa-users',
      route: '/employees',
      roles: ['HR', 'Admin', 'Manager'],
      children: [
        {
          label: 'Employee List',
          icon: 'fas fa-list',
          route: '/employees',
          roles: ['HR', 'Admin', 'Manager']
        },
        {
          label: 'Org Chart',
          icon: 'fas fa-sitemap',
          route: '/employees/org-chart',
          roles: ['HR', 'Admin', 'Manager']
        }
      ]
    },
    {
      label: 'Attendance',
      icon: 'fas fa-clock',
      route: '/attendance'
    },
    {
      label: 'Projects',
      icon: 'fas fa-project-diagram',
      route: '/projects'
    },
    {
      label: 'Payroll',
      icon: 'fas fa-money-bill-wave',
      route: '/payroll',
      roles: ['HR', 'Admin', 'Finance']
    },
    {
      label: 'Leave Management',
      icon: 'fas fa-calendar-alt',
      route: '/leave'
    },
    {
      label: 'Performance',
      icon: 'fas fa-chart-line',
      route: '/performance',
      roles: ['HR', 'Admin', 'Manager']
    },
    {
      label: 'Reports',
      icon: 'fas fa-chart-bar',
      route: '/reports',
      roles: ['HR', 'Admin', 'Manager']
    },
    {
      label: 'Settings',
      icon: 'fas fa-cog',
      route: '/settings',
      roles: ['Admin', 'SuperAdmin']
    },
    {
      label: 'Navigation Test',
      icon: 'fas fa-route',
      route: '/navigation-test',
      roles: ['Admin', 'SuperAdmin']
    }
  ];

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    this.checkScreenSize();
  }

  ngOnInit(): void {
    this.subscription.add(
      this.authService.currentUser$.subscribe({
        next: user => {
          this.currentUser = user;
          console.log('Sidebar: Current user updated', user);
        },
        error: error => {
          console.error('Sidebar: Error getting current user', error);
          this.currentUser = null;
        }
      })
    );

    // Listen for window resize
    window.addEventListener('resize', () => this.checkScreenSize());
    
    // Initial screen size check
    this.checkScreenSize();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    window.removeEventListener('resize', () => this.checkScreenSize());
  }

  toggleSidebar(): void {
    this.isCollapsed = !this.isCollapsed;
  }

  closeSidebar(): void {
    if (this.isMobile) {
      this.isCollapsed = true;
    }
  }

  hasAccess(item: MenuItem): boolean {
    try {
      if (!item.roles || item.roles.length === 0) {
        return true;
      }
      return this.authService.hasAnyRole(item.roles);
    } catch (error) {
      console.error('Sidebar: Error checking access for menu item', item.label, error);
      return false;
    }
  }

  private checkScreenSize(): void {
    this.isMobile = window.innerWidth < 768;
    if (this.isMobile) {
      this.isCollapsed = true;
    }
  }

  onNavigationClick(item: MenuItem, event: Event): void {
    try {
      console.log('Sidebar: Navigating to', item.route);
      
      // Check access before navigation
      if (!this.hasAccess(item)) {
        event.preventDefault();
        console.warn('Sidebar: Access denied to', item.route);
        return;
      }

      // Close sidebar on mobile after navigation
      if (this.isMobile) {
        setTimeout(() => {
          this.isCollapsed = true;
        }, 100);
      }
    } catch (error) {
      console.error('Sidebar: Navigation error for', item.route, error);
      event.preventDefault();
    }
  }
}