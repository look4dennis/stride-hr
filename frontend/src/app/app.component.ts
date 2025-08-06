import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';

@Component({
    selector: 'app-root',
    imports: [
      CommonModule, 
      RouterOutlet
    ],
    template: `
    <!-- Main Application -->
    <router-outlet></router-outlet>
  `,
    styles: []
})
export class AppComponent {
  title = 'StrideHR';
}