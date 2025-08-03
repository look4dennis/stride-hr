import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private loadingMap = new Map<string, boolean>();

  public loading$ = this.loadingSubject.asObservable();

  setLoading(loading: boolean, key: string = 'global'): void {
    if (loading) {
      this.loadingMap.set(key, loading);
    } else {
      this.loadingMap.delete(key);
    }

    // Update global loading state
    this.loadingSubject.next(this.loadingMap.size > 0);
  }

  isLoading(key: string = 'global'): boolean {
    return this.loadingMap.has(key);
  }

  clearAll(): void {
    this.loadingMap.clear();
    this.loadingSubject.next(false);
  }
}