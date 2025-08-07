import { Directive, Input, OnInit, OnDestroy } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Directive({
  selector: '[appFormValidation]',
  standalone: true
})
export class FormValidationDirective implements OnInit, OnDestroy {
  @Input('appFormValidation') form!: FormGroup;
  
  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    if (this.form) {
      // Add any form-level validation logic here
      this.form.statusChanges
        .pipe(takeUntil(this.destroy$))
        .subscribe(status => {
          // Handle form status changes if needed
        });
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}