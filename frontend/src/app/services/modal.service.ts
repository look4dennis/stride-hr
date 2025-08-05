import { Injectable, TemplateRef, ComponentRef } from '@angular/core';
import { NgbModal, NgbModalRef, NgbModalOptions } from '@ng-bootstrap/ng-bootstrap';
import { Observable, from } from 'rxjs';

export interface ModalConfig extends NgbModalOptions {
    title?: string;
    confirmText?: string;
    cancelText?: string;
    confirmButtonClass?: string;
    cancelButtonClass?: string;
}

@Injectable({
    providedIn: 'root'
})
export class ModalService {
    constructor(private ngbModal: NgbModal) { }

    /**
     * Open a modal with a template
     */
    openTemplate<T = any>(
        template: TemplateRef<any>,
        config: ModalConfig = {}
    ): NgbModalRef {
        const defaultConfig: NgbModalOptions = {
            backdrop: 'static',
            keyboard: false,
            centered: true,
            ...config
        };

        return this.ngbModal.open(template, defaultConfig);
    }

    /**
     * Open a modal with a component
     */
    openComponent<T = any>(
        component: any,
        config: ModalConfig = {}
    ): NgbModalRef {
        const defaultConfig: NgbModalOptions = {
            backdrop: 'static',
            keyboard: false,
            centered: true,
            ...config
        };

        return this.ngbModal.open(component, defaultConfig);
    }

    /**
     * Show confirmation dialog
     */
    confirm(
        message: string,
        title: string = 'Confirm Action',
        config: ModalConfig = {}
    ): Observable<boolean> {
        const modalRef = this.ngbModal.open(ConfirmationModalComponent, {
            backdrop: 'static',
            keyboard: false,
            centered: true,
            ...config
        });

        modalRef.componentInstance.title = title;
        modalRef.componentInstance.message = message;
        modalRef.componentInstance.confirmText = config.confirmText || 'Confirm';
        modalRef.componentInstance.cancelText = config.cancelText || 'Cancel';
        modalRef.componentInstance.confirmButtonClass = config.confirmButtonClass || 'btn-primary';
        modalRef.componentInstance.cancelButtonClass = config.cancelButtonClass || 'btn-secondary';

        return from(modalRef.result.then(
            (result) => result === true,
            () => false
        ));
    }

    /**
     * Show alert dialog
     */
    alert(
        message: string,
        title: string = 'Alert',
        config: ModalConfig = {}
    ): Observable<void> {
        const modalRef = this.ngbModal.open(AlertModalComponent, {
            backdrop: 'static',
            keyboard: false,
            centered: true,
            ...config
        });

        modalRef.componentInstance.title = title;
        modalRef.componentInstance.message = message;

        return from(modalRef.result.then(
            () => { },
            () => { }
        ));
    }

    /**
     * Close all open modals
     */
    closeAll(): void {
        this.ngbModal.dismissAll();
    }

    /**
     * Check if any modal is open
     */
    hasOpenModals(): boolean {
        return this.ngbModal.hasOpenModals();
    }
}

// Confirmation Modal Component
import { Component, Input } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-confirmation-modal',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="modal-header">
      <h4 class="modal-title">{{ title }}</h4>
      <button type="button" class="btn-close" aria-label="Close" (click)="activeModal.dismiss()"></button>
    </div>
    <div class="modal-body">
      <p>{{ message }}</p>
    </div>
    <div class="modal-footer">
      <button type="button" [class]="'btn ' + cancelButtonClass" (click)="activeModal.dismiss()">
        {{ cancelText }}
      </button>
      <button type="button" [class]="'btn ' + confirmButtonClass" (click)="activeModal.close(true)">
        {{ confirmText }}
      </button>
    </div>
  `
})
export class ConfirmationModalComponent {
    @Input() title = 'Confirm Action';
    @Input() message = 'Are you sure?';
    @Input() confirmText = 'Confirm';
    @Input() cancelText = 'Cancel';
    @Input() confirmButtonClass = 'btn-primary';
    @Input() cancelButtonClass = 'btn-secondary';

    constructor(public activeModal: NgbActiveModal) { }
}

// Alert Modal Component
@Component({
    selector: 'app-alert-modal',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="modal-header">
      <h4 class="modal-title">{{ title }}</h4>
      <button type="button" class="btn-close" aria-label="Close" (click)="activeModal.close()"></button>
    </div>
    <div class="modal-body">
      <p>{{ message }}</p>
    </div>
    <div class="modal-footer">
      <button type="button" class="btn btn-primary" (click)="activeModal.close()">
        OK
      </button>
    </div>
  `
})
export class AlertModalComponent {
    @Input() title = 'Alert';
    @Input() message = '';

    constructor(public activeModal: NgbActiveModal) { }
}