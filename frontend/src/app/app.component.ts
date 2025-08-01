import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container-fluid">
      <div class="row">
        <div class="col-12">
          <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
            <div class="container">
              <a class="navbar-brand" href="#">
                <strong>StrideHR</strong>
              </a>
            </div>
          </nav>
        </div>
      </div>
      <div class="row mt-4">
        <div class="col-12">
          <div class="container">
            <div class="card">
              <div class="card-body text-center">
                <h1 class="card-title">Welcome to StrideHR</h1>
                <p class="card-text">Enterprise Human Resource Management System</p>
                <p class="text-muted">Application is ready for development</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class AppComponent {
  title = 'StrideHR';
}