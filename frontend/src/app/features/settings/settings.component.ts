import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminSettingsComponent } from './admin-settings.component';

@Component({
    selector: 'app-settings',
    imports: [CommonModule, AdminSettingsComponent],
    template: `
    <app-admin-settings></app-admin-settings>
  `,
    styles: []
})
export class SettingsComponent {}