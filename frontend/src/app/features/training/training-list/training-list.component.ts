import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-training-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="training-list">
      <h2>Training Programs</h2>
      <p>Training list content will be implemented here.</p>
    </div>
  `,
  styles: [`
    .training-list {
      padding: 20px;
    }
  `]
})
export class TrainingListComponent {
}