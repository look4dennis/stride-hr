import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService, User } from '../../../core/auth/auth.service';

@Component({
    selector: 'app-header',
    imports: [CommonModule, RouterModule],
    template: `
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
      <div class="container-fluid">
        <a class="navbar-brand" routerLink="/">
          <strong>StrideHR</strong>
        </a>
        
        <button 
          class="navbar-toggler" 
          type="button" 
          data-bs-toggle="collapse" 
          data-bs-target="#navbarNav"
          aria-controls="navbarNav" 
          aria-expanded="false" 
          aria-label="Toggle navigation">
          <span class="navbar-toggler-icon"></span>
        </button>
        
        <div class="collapse navbar-collapse" id="navbarNav">
          <ul class="navbar-nav me-auto">
            <li class="nav-item" *ngIf="currentUser">
              <a class="nav-link" routerLink="/dashboard" routerLinkActive="active">
                <i class="fas fa-tachometer-alt me-1"></i>
                Dashboard
              </a>
            </li>
            <li class="nav-item" *ngIf="currentUser">
              <a class="nav-link" routerLink="/employees" routerLinkActive="active">
                <i class="fas fa-users me-1"></i>
                Employees
              </a>
            </li>
            <li class="nav-item" *ngIf="currentUser">
              <a class="nav-link" routerLink="/attendance" routerLinkActive="active">
                <i class="fas fa-clock me-1"></i>
                Attendance
              </a>
            </li>
            <li class="nav-item" *ngIf="currentUser">
              <a class="nav-link" routerLink="/projects" routerLinkActive="active">
                <i class="fas fa-project-diagram me-1"></i>
                Projects
              </a>
            </li>
          </ul>
          
          <ul class="navbar-nav" *ngIf="currentUser">
            <li class="nav-item dropdown">
              <a 
                class="nav-link dropdown-toggle d-flex align-items-center" 
                href="#" 
                id="navbarDropdown" 
                role="button" 
                data-bs-toggle="dropdown" 
                aria-expanded="false">
                <img 
                  [src]="currentUser.profilePhoto || '/assets/images/default-avatar.png'" 
                  alt="Profile" 
                  class="rounded-circle me-2"
                  width="32" 
                  height="32">
                {{ currentUser.firstName }} {{ currentUser.lastName }}
              </a>
              <ul class="dropdown-menu dropdown-menu-end">
                <li>
                  <a class="dropdown-item" routerLink="/profile">
                    <i class="fas fa-user me-2"></i>
                    Profile
                  </a>
                </li>
                <li>
                  <a class="dropdown-item" routerLink="/settings">
                    <i class="fas fa-cog me-2"></i>
                    Settings
                  </a>
                </li>
                <li><hr class="dropdown-divider"></li>
                <li>
                  <a class="dropdown-item" href="#" (click)="logout($event)">
                    <i class="fas fa-sign-out-alt me-2"></i>
                    Logout
                  </a>
                </li>
              </ul>
            </li>
          </ul>
          
          <div *ngIf="!currentUser" class="d-flex">
            <a class="btn btn-outline-light me-2" routerLink="/login">Login</a>
          </div>
        </div>
      </div>
    </nav>
  `,
    styles: [`
    .navbar-brand {
      font-family: var(--font-headings);
      font-weight: 700;
      font-size: 1.5rem;
    }
    
    .bg-primary {
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%) !important;
    }

    .nav-link {
      font-weight: 500;
      transition: color 0.15s ease-in-out;
      min-height: 44px;
      display: flex;
      align-items: center;
    }

    .nav-link:hover {
      color: rgba(255, 255, 255, 0.9) !important;
    }

    .nav-link.active {
      color: white !important;
      font-weight: 600;
    }

    .dropdown-menu {
      border: none;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
      border-radius: 8px;
    }

    .dropdown-item {
      padding: 0.75rem 1rem;
      transition: background-color 0.15s ease-in-out;
      min-height: 44px;
      display: flex;
      align-items: center;
    }

    .dropdown-item:hover {
      background-color: var(--bg-tertiary);
    }

    /* Mobile-specific navbar styles */
    @media (max-width: 991px) {
      .navbar-brand {
        font-size: 1.25rem;
      }
      
      .navbar-nav {
        padding-top: 1rem;
        padding-bottom: 1rem;
      }
      
      .navbar-nav .nav-link {
        padding: 0.75rem 1rem;
        border-bottom: 1px solid rgba(255, 255, 255, 0.1);
        margin-bottom: 0;
      }
      
      .navbar-nav .nav-link:last-child {
        border-bottom: none;
      }
      
      .navbar-nav .dropdown-menu {
        background-color: rgba(255, 255, 255, 0.95);
        backdrop-filter: blur(10px);
        margin-top: 0.5rem;
      }
      
      .navbar-nav .dropdown-item {
        color: var(--text-primary);
      }
      
      .navbar-nav .dropdown-item:hover {
        background-color: var(--bg-secondary);
      }
    }

    /* Touch-friendly navbar toggler */
    .navbar-toggler {
      border: none;
      padding: 0.5rem;
      min-height: 44px;
      min-width: 44px;
    }

    .navbar-toggler:focus {
      box-shadow: none;
    }

    /* Profile image responsive sizing */
    .navbar-nav img {
      transition: all 0.2s ease;
    }

    @media (max-width: 576px) {
      .navbar-nav img {
        width: 28px;
        height: 28px;
      }
      
      .navbar-brand {
        font-size: 1.1rem;
      }
    }
  `]
})
export class HeaderComponent implements OnInit, OnDestroy {
  currentUser: User | null = null;
  private subscription: Subscription = new Subscription();

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.subscription.add(
      this.authService.currentUser$.subscribe(
        user => this.currentUser = user
      )
    );
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  logout(event: Event): void {
    event.preventDefault();
    this.authService.logout();
  }
}