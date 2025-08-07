import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

// Components
import { EmployeeListComponent } from './employee-list/employee-list.component';
import { EmployeeCreateComponent } from './employee-create/employee-create.component';
import { EmployeeProfileComponent } from './employee-profile/employee-profile.component';
import { EmployeeOnboardingComponent } from './employee-onboarding/employee-onboarding.component';
import { EmployeeExitComponent } from './employee-exit/employee-exit.component';
import { OrgChartComponent } from './org-chart/org-chart.component';

// Shared components and directives
import { VirtualScrollDirective } from '../../shared/directives/virtual-scroll.directive';
import { LazyImageDirective } from '../../shared/directives/lazy-image.directive';

// Guards
import { RoleGuard } from '../../core/guards/role.guard';

const routes: Routes = [
  {
    path: '',
    component: EmployeeListComponent,
    data: { roles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] }
  },
  {
    path: 'add',
    component: EmployeeCreateComponent,
    data: { roles: ['HR', 'Admin', 'SuperAdmin'] }
  },
  {
    path: 'org-chart',
    component: OrgChartComponent,
    data: { roles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] }
  },
  {
    path: ':id',
    component: EmployeeProfileComponent,
    data: { roles: ['HR', 'Admin', 'Manager', 'SuperAdmin'] }
  },
  {
    path: ':id/edit',
    component: EmployeeProfileComponent,
    data: { roles: ['HR', 'Admin', 'SuperAdmin'] }
  },
  {
    path: ':id/onboarding',
    component: EmployeeOnboardingComponent,
    data: { roles: ['HR', 'Admin', 'SuperAdmin'] }
  },
  {
    path: ':id/exit',
    component: EmployeeExitComponent,
    data: { roles: ['HR', 'Admin', 'SuperAdmin'] }
  }
];

@NgModule({
  declarations: [
    // Components are now standalone, so we don't declare them here
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild(routes),
    VirtualScrollDirective,
    LazyImageDirective
  ],
  providers: [
    RoleGuard
  ]
})
export class EmployeesModule { }