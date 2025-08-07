import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { SetupWizardComponent } from './setup-wizard.component';
import { SetupWizardService } from '../../core/services/setup-wizard.service';
import { SetupWizardGuard } from '../../core/guards/setup-wizard.guard';
import { SetupCompleteGuard } from '../../core/guards/setup-complete.guard';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    SetupWizardComponent // Import as standalone component
  ],
  providers: [
    SetupWizardService,
    SetupWizardGuard,
    SetupCompleteGuard
  ],
  exports: [
    SetupWizardComponent
  ]
})
export class SetupWizardModule { }