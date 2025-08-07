import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable, of, throwError } from 'rxjs';
import { delay } from 'rxjs/operators';
import { BaseComponent } from './base-component';

@Component({
  selector: 'app-example-component',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h5>Base Component Example</h5>
      </div>
      <div class="card-body">
        <div *ngIf="isLoading" class="text-center">
          <div class="spinner-border" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
          <p class="mt-2">Loading data...</p>
        </div>
        
        <div *ngIf="error" class="alert alert-danger">
          <i class="fas fa-exclamation-triangle me-2"></i>
          {{ error }}
        </div>
        
        <div *ngIf="!isLoading && !error">
          <p>Component loaded successfully!</p>
          <div class="d-flex gap-2">
            <button class="btn btn-primary" (click)="loadData()">
              Load Data
            </button>
            <button class="btn btn-warning" (click)="loadDataWithError()">
              Simulate Error
            </button>
            <button class="btn btn-success" (click)="showSuccessMessage()">
              Show Success
            </button>
          </div>
        </div>
      </div>
    </div>
  `
})
export class ExampleComponent extends BaseComponent {
  
  protected initializeComponent(): void {
    console.log('Example component initialized');
  }

  loadData(): void {
    this.executeWithLoading(
      () => this.simulateApiCall(),
      'Loading example data...',
      'Data loaded successfully!'
    ).subscribe({
      next: (data) => {
        console.log('Data received:', data);
      },
      error: (error) => {
        // Error is already handled by base component
        console.log('Error handled by base component');
      }
    });
  }

  loadDataWithError(): void {
    this.executeWithLoading(
      () => this.simulateApiError(),
      'Loading data that will fail...'
    ).subscribe({
      next: (data) => {
        console.log('This should not execute');
      },
      error: (error) => {
        console.log('Error handled by base component');
      }
    });
  }

  showSuccessMessage(): void {
    this.showSuccess('This is a success message from the base component!');
  }

  private simulateApiCall(): Observable<any> {
    return of({ message: 'API call successful', data: [1, 2, 3] }).pipe(
      delay(2000) // Simulate network delay
    );
  }

  private simulateApiError(): Observable<any> {
    return throwError(() => new Error('Simulated API error')).pipe(
      delay(1000)
    );
  }
}