import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

// Directives
import { VirtualScrollDirective } from './directives/virtual-scroll.directive';
import { LazyImageDirective } from './directives/lazy-image.directive';

// Components
import { ProgressiveDashboardComponent } from './components/progressive-dashboard/progressive-dashboard.component';

@NgModule({
  declarations: [
    // Note: Since we're using standalone components, we don't declare them here
    // but we can still export them for convenience
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    // Import standalone directives and components
    VirtualScrollDirective,
    LazyImageDirective,
    ProgressiveDashboardComponent
  ],
  exports: [
    // Re-export common modules
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    
    // Export our custom directives and components
    VirtualScrollDirective,
    LazyImageDirective,
    ProgressiveDashboardComponent
  ]
})
export class SharedModule { }