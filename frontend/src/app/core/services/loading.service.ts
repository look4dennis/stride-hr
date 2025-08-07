import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface LoadingState {
  key: string;
  message?: string;
  progress?: number;
  startTime: number;
}

@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private globalLoadingSubject = new BehaviorSubject<boolean>(false);
  private loadingStatesSubject = new BehaviorSubject<Map<string, LoadingState>>(new Map());
  private componentLoadingSubject = new BehaviorSubject<Map<string, boolean>>(new Map());
  private operationLoadingSubject = new BehaviorSubject<Map<string, boolean>>(new Map());

  // Observable streams
  public loading$ = this.globalLoadingSubject.asObservable();
  public loadingStates$ = this.loadingStatesSubject.asObservable();
  public componentLoading$ = this.componentLoadingSubject.asObservable();
  public operationLoading$ = this.operationLoadingSubject.asObservable();

  // Global loading management
  setGlobalLoading(loading: boolean, message?: string): void {
    this.globalLoadingSubject.next(loading);
    
    if (loading && message) {
      this.setLoading(loading, 'global', message);
    } else if (!loading) {
      this.clearLoading('global');
    }
  }

  isGlobalLoading(): Observable<boolean> {
    return this.globalLoadingSubject.asObservable();
  }

  // General loading state management
  setLoading(loading: boolean, key: string = 'global', message?: string, progress?: number): void {
    const currentStates = this.loadingStatesSubject.value;
    
    if (loading) {
      const loadingState: LoadingState = {
        key,
        message,
        progress,
        startTime: Date.now()
      };
      currentStates.set(key, loadingState);
    } else {
      currentStates.delete(key);
    }

    this.loadingStatesSubject.next(new Map(currentStates));
    
    // Update global loading state based on any active loading operations
    const hasActiveLoading = currentStates.size > 0;
    if (this.globalLoadingSubject.value !== hasActiveLoading) {
      this.globalLoadingSubject.next(hasActiveLoading);
    }
  }

  isLoading(key: string = 'global'): boolean {
    return this.loadingStatesSubject.value.has(key);
  }

  getLoadingState(key: string): LoadingState | undefined {
    return this.loadingStatesSubject.value.get(key);
  }

  // Component-specific loading
  setComponentLoading(componentId: string, loading: boolean): void {
    const currentStates = this.componentLoadingSubject.value;
    
    if (loading) {
      currentStates.set(componentId, true);
    } else {
      currentStates.delete(componentId);
    }
    
    this.componentLoadingSubject.next(new Map(currentStates));
  }

  isComponentLoading(componentId: string): Observable<boolean> {
    return this.componentLoadingSubject.pipe(
      map(states => states.has(componentId))
    );
  }

  // Operation-specific loading
  setOperationLoading(operationId: string, loading: boolean): void {
    const currentStates = this.operationLoadingSubject.value;
    
    if (loading) {
      currentStates.set(operationId, true);
    } else {
      currentStates.delete(operationId);
    }
    
    this.operationLoadingSubject.next(new Map(currentStates));
  }

  isOperationLoading(operationId: string): Observable<boolean> {
    return this.operationLoadingSubject.pipe(
      map(states => states.has(operationId))
    );
  }

  // Utility methods
  clearLoading(key: string): void {
    this.setLoading(false, key);
  }

  clearComponentLoading(componentId: string): void {
    this.setComponentLoading(componentId, false);
  }

  clearOperationLoading(operationId: string): void {
    this.setOperationLoading(operationId, false);
  }

  clearAll(): void {
    this.loadingStatesSubject.next(new Map());
    this.componentLoadingSubject.next(new Map());
    this.operationLoadingSubject.next(new Map());
    this.globalLoadingSubject.next(false);
  }

  // Get all active loading operations
  getActiveLoadingOperations(): Observable<LoadingState[]> {
    return this.loadingStatesSubject.pipe(
      map(states => Array.from(states.values()))
    );
  }

  // Get loading duration for a specific operation
  getLoadingDuration(key: string): number {
    const state = this.getLoadingState(key);
    return state ? Date.now() - state.startTime : 0;
  }

  // Update progress for a loading operation
  updateProgress(key: string, progress: number, message?: string): void {
    const currentStates = this.loadingStatesSubject.value;
    const existingState = currentStates.get(key);
    
    if (existingState) {
      const updatedState: LoadingState = {
        ...existingState,
        progress,
        message: message || existingState.message
      };
      currentStates.set(key, updatedState);
      this.loadingStatesSubject.next(new Map(currentStates));
    }
  }
}