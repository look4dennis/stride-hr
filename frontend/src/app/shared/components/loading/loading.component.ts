import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { LoadingService } from '../../../core/services/loading.service';

@Component({
    selector: 'app-loading',
    imports: [CommonModule],
    template: `
    <div *ngIf="isLoading" class="loading-overlay">
      <div class="loading-spinner">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
        <div class="loading-text mt-3">
          <strong>Loading...</strong>
        </div>
      </div>
    </div>
  `,
    styles: [`
    .loading-overlay {
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background-color: rgba(255, 255, 255, 0.8);
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 9999;
      backdrop-filter: blur(2px);
    }

    .loading-spinner {
      text-align: center;
      padding: 2rem;
      background: white;
      border-radius: 12px;
      box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
    }

    .spinner-border {
      width: 3rem;
      height: 3rem;
    }

    .loading-text {
      color: var(--text-primary);
      font-family: var(--font-primary);
    }
  `]
})
export class LoadingComponent implements OnInit, OnDestroy {
  isLoading = false;
  private subscription: Subscription = new Subscription();

  constructor(private loadingService: LoadingService) {}

  ngOnInit(): void {
    this.subscription.add(
      this.loadingService.loading$.subscribe(
        loading => this.isLoading = loading
      )
    );
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}