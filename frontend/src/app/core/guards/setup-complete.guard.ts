import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { SetupWizardService } from '../services/setup-wizard.service';

@Injectable({
  providedIn: 'root'
})
export class SetupCompleteGuard implements CanActivate {
  constructor(
    private setupWizardService: SetupWizardService,
    private router: Router
  ) {}

  canActivate(): Observable<boolean | UrlTree> {
    return this.setupWizardService.checkSetupStatus().pipe(
      map(status => {
        if (!status.isSetupComplete) {
          // Setup is not complete, allow access to setup wizard
          return true;
        } else {
          // Setup is complete, redirect to dashboard
          return this.router.createUrlTree(['/dashboard']);
        }
      }),
      catchError(error => {
        console.error('Error checking setup status in setup complete guard:', error);
        // If we can't check setup status, allow access to setup wizard
        return of(true);
      })
    );
  }
}