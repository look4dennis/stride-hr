import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { SetupWizardService } from '../services/setup-wizard.service';

@Injectable({
  providedIn: 'root'
})
export class SetupWizardGuard implements CanActivate {
  constructor(
    private setupWizardService: SetupWizardService,
    private router: Router
  ) {}

  canActivate(): Observable<boolean | UrlTree> {
    return this.setupWizardService.checkSetupStatus().pipe(
      map(status => {
        if (status.isSetupComplete) {
          // Setup is complete, allow access to the route
          return true;
        } else {
          // Setup is not complete, redirect to setup wizard
          return this.router.createUrlTree(['/setup-wizard']);
        }
      }),
      catchError(error => {
        console.error('Error checking setup status in guard:', error);
        // If we can't check setup status, redirect to setup wizard to be safe
        return of(this.router.createUrlTree(['/setup-wizard']));
      })
    );
  }
}