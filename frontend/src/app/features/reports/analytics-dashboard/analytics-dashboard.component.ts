import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnalyticsDashboardMainComponent } from './analytics-dashboard-main.component';

@Component({
  selector: 'app-analytics-dashboard',
  standalone: true,
  imports: [CommonModule, AnalyticsDashboardMainComponent],
  template: `
    <app-analytics-dashboard-main></app-analytics-dashboard-main>
  `,
  styles: []
})
export class AnalyticsDashboardComponent implements OnInit {
  constructor() {}

  ngOnInit() {
    // Component initialization handled by the main component
  }
}