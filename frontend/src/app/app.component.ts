import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  template: `
    <div class="container-fluid">
      <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
        <div class="container">
          <a class="navbar-brand" href="#">
            <strong>StrideHR</strong>
          </a>
        </div>
      </nav>
      
      <main class="py-4">
        <div class="container">
          <div class="row">
            <div class="col-12">
              <div class="card">
                <div class="card-body text-center">
                  <h1 class="card-title">Welcome to {{ title }}</h1>
                  <p class="card-text">Human Resource Management System</p>
                  <p class="text-muted">Application is ready for development</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
    
    <router-outlet></router-outlet>
  `,
  styles: [`
    .navbar-brand {
      font-family: var(--font-headings);
      font-weight: 700;
    }
    
    .bg-primary {
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%) !important;
    }
  `]
})
export class AppComponent {
  title = 'StrideHR';
}