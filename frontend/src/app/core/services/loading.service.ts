import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private globalLoadingSubject = new BehaviorSubject<boolean>(false);
  private componentLoadingSubject = new BehaviorSubject<Map<string, boolean>>(new Map());
  private operationLoadingSubject = new BehaviorSubject<Map<string, boolean>>(new Map());

  public globalLoading$ = this.globalLoadingSubject.asObservable();
  public componentLoading$ = this.componentLoadingSubject.asObservable();
  public operationLoading$ = this.operationLoadingSubject.asObservable();

  constructor() {}

  // Global loading
  setGlobalLoading(loading: boolean): void {
    this.globalLoadingSubject.next(loading);
  }

  isGlobalLoading(): Observable<boolean> {
    return this.globalLoading$;
  }

  // Component-specific loading
  setComponentLoading(componentId: string, loading: boolean): void {
    const currentMap = new Map(this.componentLoadingSubject.value);
    if (loading) {
      currentMap.set(componentId, true);
    } else {
      currentMap.delete(componentId);
    }
    this.componentLoadingSubject.next(currentMap);
  }

  isComponentLoading(componentId: string): Observable<boolean> {
    return new Observable(observer => {
      this.componentLoading$.subscribe(loadingMap => {
        observer.next(loadingMap.has(componentId) && loadingMap.get(componentId) === true);
      });
    });
  }

  // Operation-specific loading
  setOperationLoading(operationId: string, loading: boolean): void {
    const currentMap = new Map(this.operationLoadingSubject.value);
    if (loading) {
      currentMap.set(operationId, true);
    } else {
      currentMap.delete(operationId);
    }
    this.operationLoadingSubject.next(currentMap);
  }

  isOperationLoading(operationId: string): Observable<boolean> {
    return new Observable(observer => {
      this.operationLoading$.subscribe(loadingMap => {
        observer.next(loadingMap.has(operationId) && loadingMap.get(operationId) === true);
      });
    });
  }

  // Utility methods
  setLoading(loading: boolean, componentId?: string, message?: string): void {
    if (componentId) {
      this.setComponentLoading(componentId, loading);
    } else {
      this.setGlobalLoading(loading);
    }
  }

  clearLoading(componentId?: string): void {
    if (componentId) {
      this.setComponentLoading(componentId, false);
    } else {
      this.setGlobalLoading(false);
    }
  }

  clearComponentLoading(componentId: string): void {
    this.setComponentLoading(componentId, false);
  }

  showLoading(id?: string): void {
    if (id) {
      this.setComponentLoading(id, true);
    } else {
      this.setGlobalLoading(true);
    }
  }

  hideLoading(id?: string): void {
    if (id) {
      this.setComponentLoading(id, false);
    } else {
      this.setGlobalLoading(false);
    }
  }

  // Clear all loading states
  clearAll(): void {
    this.setGlobalLoading(false);
    this.componentLoadingSubject.next(new Map());
    this.operationLoadingSubject.next(new Map());
  }
}