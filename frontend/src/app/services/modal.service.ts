import { Injectable, TemplateRef, ComponentRef, ViewContainerRef, ComponentFactoryResolver, ApplicationRef, Injector } from '@angular/core';
import { Observable, Subject, BehaviorSubject } from 'rxjs';
import { ModalComponent, ModalConfig } from '../shared/components/modal/modal.component';

export interface EnhancedModalConfig extends ModalConfig {
    title?: string;
    confirmText?: string;
    cancelText?: string;
    confirmButtonClass?: string;
    cancelButtonClass?: string;
}

export interface ModalRef {
    id: string;
    component: ComponentRef<ModalComponent>;
    result: Observable<any>;
    close: (result?: any) => void;
    dismiss: (reason?: any) => void;
}

@Injectable({
    providedIn: 'root'
})
export class ModalService {
    private modals = new Map<string, ModalRef>();
    private modalContainer?: ViewContainerRef;

    constructor(
        private appRef: ApplicationRef,
        private injector: Injector
    ) {}

    /**
     * Set the container where modals will be rendered
     */
    setContainer(container: ViewContainerRef): void {
        this.modalContainer = container;
    }

    /**
     * Open a modal with enhanced configuration
     */
    open(config: EnhancedModalConfig = {}): ModalRef {
        const modalId = this.generateId();
        const resultSubject = new Subject<any>();

        // Create modal component
        const componentRef = this.createModalComponent(config);
        
        // Set up modal properties
        componentRef.instance.isVisible = true;
        componentRef.instance.title = config.title || '';
        componentRef.instance.config = config;
        componentRef.instance.modalId = modalId;

        // Handle modal events
        componentRef.instance.modalClose.subscribe((result) => {
            resultSubject.next(result);
            resultSubject.complete();
            this.destroyModal(modalId);
        });

        componentRef.instance.modalDismiss.subscribe((reason) => {
            resultSubject.error(reason);
            this.destroyModal(modalId);
        });

        const modalRef: ModalRef = {
            id: modalId,
            component: componentRef,
            result: resultSubject.asObservable(),
            close: (result?: any) => {
                componentRef.instance.close();
            },
            dismiss: (reason?: any) => {
                componentRef.instance.dismiss();
            }
        };

        this.modals.set(modalId, modalRef);
        return modalRef;
    }

    /**
     * Show confirmation dialog
     */
    confirm(
        message: string,
        title: string = 'Confirm Action',
        config: EnhancedModalConfig = {}
    ): Observable<boolean> {
        const confirmConfig: EnhancedModalConfig = {
            title,
            size: 'md',
            centered: true,
            backdrop: 'static',
            ...config
        };

        const modalRef = this.open(confirmConfig);
        
        // Set confirmation content
        // This would need to be handled by creating a confirmation component
        // For now, we'll return the basic modal result
        return new Observable<boolean>(observer => {
            modalRef.result.subscribe({
                next: (result) => observer.next(result === true),
                error: () => observer.next(false),
                complete: () => observer.complete()
            });
        });
    }

    /**
     * Show alert dialog
     */
    alert(
        message: string,
        title: string = 'Alert',
        config: EnhancedModalConfig = {}
    ): Observable<void> {
        const alertConfig: EnhancedModalConfig = {
            title,
            size: 'md',
            centered: true,
            backdrop: 'static',
            showCloseButton: true,
            ...config
        };

        const modalRef = this.open(alertConfig);
        
        return new Observable<void>(observer => {
            modalRef.result.subscribe({
                next: () => observer.next(),
                error: () => observer.next(),
                complete: () => observer.complete()
            });
        });
    }

    /**
     * Close all open modals
     */
    closeAll(): void {
        this.modals.forEach(modal => {
            modal.dismiss('closeAll');
        });
        this.modals.clear();
    }

    /**
     * Check if any modal is open
     */
    hasOpenModals(): boolean {
        return this.modals.size > 0;
    }

    /**
     * Get modal by ID
     */
    getModal(id: string): ModalRef | undefined {
        return this.modals.get(id);
    }

    /**
     * Open a modal with a template
     */
    openTemplate(template: TemplateRef<any>, config: EnhancedModalConfig = {}): ModalRef {
        const modalRef = this.open(config);
        
        // For now, just return the modal ref
        // Template content would need to be handled differently
        // This is a placeholder implementation
        
        return modalRef;
    }

    private createModalComponent(config: EnhancedModalConfig): ComponentRef<ModalComponent> {
        // Create component factory
        const componentFactory = this.injector.get(ComponentFactoryResolver).resolveComponentFactory(ModalComponent);
        
        // Create component
        const componentRef = componentFactory.create(this.injector);
        
        // Attach to application
        this.appRef.attachView(componentRef.hostView);
        
        // Append to DOM
        const domElem = (componentRef.hostView as any).rootNodes[0] as HTMLElement;
        if (this.modalContainer) {
            this.modalContainer.element.nativeElement.appendChild(domElem);
        } else {
            document.body.appendChild(domElem);
        }
        
        return componentRef;
    }

    private destroyModal(id: string): void {
        const modal = this.modals.get(id);
        if (modal) {
            // Detach from application
            this.appRef.detachView(modal.component.hostView);
            
            // Destroy component
            modal.component.destroy();
            
            // Remove from map
            this.modals.delete(id);
        }
    }

    private generateId(): string {
        return 'modal-' + Math.random().toString(36).substr(2, 9);
    }
}

// Legacy modal components removed - using new ModalComponent instead