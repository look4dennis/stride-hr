import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of, forkJoin, timer } from 'rxjs';
import { map, catchError, delay, switchMap, take } from 'rxjs/operators';

export interface LoadingTask {
  id: string;
  name: string;
  priority: number; // Lower number = higher priority
  loadFn: () => Observable<any>;
  dependencies?: string[]; // Task IDs that must complete first
  timeout?: number; // Timeout in milliseconds
  retryCount?: number; // Number of retries on failure
}

export interface LoadingState {
  taskId: string;
  status: 'pending' | 'loading' | 'completed' | 'failed' | 'timeout';
  progress: number;
  data?: any;
  error?: any;
  startTime?: number;
  endTime?: number;
}

export interface ProgressiveLoadingConfig {
  maxConcurrentTasks: number;
  defaultTimeout: number;
  defaultRetryCount: number;
  retryDelay: number;
}

@Injectable({
  providedIn: 'root'
})
export class ProgressiveLoadingService {
  private tasks = new Map<string, LoadingTask>();
  private loadingStates = new Map<string, LoadingState>();
  private runningTasks = new Set<string>();

  private loadingStateSubject = new BehaviorSubject<Map<string, LoadingState>>(new Map());
  private overallProgressSubject = new BehaviorSubject<number>(0);

  public loadingStates$ = this.loadingStateSubject.asObservable();
  public overallProgress$ = this.overallProgressSubject.asObservable();

  private config: ProgressiveLoadingConfig = {
    maxConcurrentTasks: 3,
    defaultTimeout: 10000, // 10 seconds
    defaultRetryCount: 2,
    retryDelay: 1000 // 1 second
  };

  constructor() { }

  /**
   * Configure the progressive loading service
   */
  configure(config: Partial<ProgressiveLoadingConfig>): void {
    this.config = { ...this.config, ...config };
  }

  /**
   * Add a loading task
   */
  addTask(task: LoadingTask): void {
    this.tasks.set(task.id, {
      ...task,
      timeout: task.timeout || this.config.defaultTimeout,
      retryCount: task.retryCount || this.config.defaultRetryCount
    });

    this.loadingStates.set(task.id, {
      taskId: task.id,
      status: 'pending',
      progress: 0
    });

    this.updateState();
  }

  /**
   * Add multiple tasks
   */
  addTasks(tasks: LoadingTask[]): void {
    tasks.forEach(task => this.addTask(task));
  }

  /**
   * Start progressive loading
   */
  startLoading(): Observable<Map<string, LoadingState>> {
    return new Observable(observer => {
      this.executeTasksProgressively().subscribe({
        next: (states) => observer.next(states),
        error: (error) => observer.error(error),
        complete: () => observer.complete()
      });
    });
  }

  /**
   * Get loading state for a specific task
   */
  getTaskState(taskId: string): Observable<LoadingState | undefined> {
    return this.loadingStates$.pipe(
      map(states => states.get(taskId))
    );
  }

  /**
   * Get loading states for multiple tasks
   */
  getTaskStates(taskIds: string[]): Observable<LoadingState[]> {
    return this.loadingStates$.pipe(
      map(states => taskIds.map(id => states.get(id)).filter(Boolean) as LoadingState[])
    );
  }

  /**
   * Check if all tasks are completed
   */
  isAllCompleted(): Observable<boolean> {
    return this.loadingStates$.pipe(
      map(states => {
        const stateArray = Array.from(states.values());
        return stateArray.length > 0 && stateArray.every(state =>
          state.status === 'completed' || state.status === 'failed'
        );
      })
    );
  }

  /**
   * Get completed tasks data
   */
  getCompletedData(): Observable<Map<string, any>> {
    return this.loadingStates$.pipe(
      map(states => {
        const completedData = new Map<string, any>();
        states.forEach((state, taskId) => {
          if (state.status === 'completed' && state.data) {
            completedData.set(taskId, state.data);
          }
        });
        return completedData;
      })
    );
  }

  /**
   * Clear all tasks and reset state
   */
  clear(): void {
    this.tasks.clear();
    this.loadingStates.clear();
    this.runningTasks.clear();
    this.updateState();
    this.overallProgressSubject.next(0);
  }

  /**
   * Cancel a specific task
   */
  cancelTask(taskId: string): void {
    if (this.loadingStates.has(taskId)) {
      const state = this.loadingStates.get(taskId)!;
      state.status = 'failed';
      state.error = 'Cancelled by user';
      state.endTime = Date.now();
      this.runningTasks.delete(taskId);
      this.updateState();
    }
  }

  /**
   * Retry a failed task
   */
  retryTask(taskId: string): Observable<LoadingState> {
    const task = this.tasks.get(taskId);
    if (!task) {
      return of({
        taskId,
        status: 'failed',
        progress: 0,
        error: 'Task not found'
      });
    }

    // Reset task state
    this.loadingStates.set(taskId, {
      taskId,
      status: 'pending',
      progress: 0
    });

    this.updateState();

    // Execute the task
    return this.executeTask(task);
  }

  /**
   * Execute tasks progressively based on priority and dependencies
   */
  private executeTasksProgressively(): Observable<Map<string, LoadingState>> {
    return new Observable(observer => {
      const executeNext = () => {
        const readyTasks = this.getReadyTasks();
        const availableSlots = this.config.maxConcurrentTasks - this.runningTasks.size;
        const tasksToExecute = readyTasks.slice(0, availableSlots);

        if (tasksToExecute.length === 0) {
          // Check if all tasks are done
          const allStates = Array.from(this.loadingStates.values());
          const allDone = allStates.every(state =>
            state.status === 'completed' || state.status === 'failed'
          );

          if (allDone) {
            observer.next(this.loadingStates);
            observer.complete();
            return;
          }

          // Wait a bit and try again
          setTimeout(executeNext, 100);
          return;
        }

        // Execute ready tasks
        const taskObservables = tasksToExecute.map(task =>
          this.executeTask(task).pipe(
            catchError(error => of({
              taskId: task.id,
              status: 'failed' as const,
              progress: 0,
              error
            }))
          )
        );

        forkJoin(taskObservables).subscribe({
          next: () => {
            observer.next(this.loadingStates);
            executeNext(); // Continue with next batch
          },
          error: (error) => observer.error(error)
        });
      };

      executeNext();
    });
  }

  /**
   * Get tasks that are ready to execute (dependencies met, not running)
   */
  private getReadyTasks(): LoadingTask[] {
    const readyTasks: LoadingTask[] = [];

    for (const [taskId, task] of this.tasks.entries()) {
      const state = this.loadingStates.get(taskId);

      if (!state || state.status !== 'pending' || this.runningTasks.has(taskId)) {
        continue;
      }

      // Check dependencies
      const dependenciesMet = !task.dependencies || task.dependencies.every(depId => {
        const depState = this.loadingStates.get(depId);
        return depState && depState.status === 'completed';
      });

      if (dependenciesMet) {
        readyTasks.push(task);
      }
    }

    // Sort by priority (lower number = higher priority)
    return readyTasks.sort((a, b) => a.priority - b.priority);
  }

  /**
   * Execute a single task with timeout and retry logic
   */
  private executeTask(task: LoadingTask): Observable<LoadingState> {
    return new Observable(observer => {
      const state = this.loadingStates.get(task.id)!;
      state.status = 'loading';
      state.startTime = Date.now();
      state.progress = 0;

      this.runningTasks.add(task.id);
      this.updateState();

      const executeWithRetry = (attemptsLeft: number): void => {
        const taskExecution = task.loadFn().pipe(
          // Add timeout
          switchMap(data =>
            timer(task.timeout!).pipe(
              take(1),
              switchMap(() => of({ error: 'timeout' })),
              catchError(() => of(data))
            )
          ),
          catchError(error => of({ error }))
        );

        taskExecution.subscribe({
          next: (result) => {
            if (result && 'error' in result) {
              // Handle error or timeout
              if (attemptsLeft > 0 && result.error !== 'timeout') {
                // Retry after delay
                timer(this.config.retryDelay).subscribe(() => {
                  executeWithRetry(attemptsLeft - 1);
                });
                return;
              }

              // Final failure
              state.status = result.error === 'timeout' ? 'timeout' : 'failed';
              state.error = result.error;
              state.progress = 0;
            } else {
              // Success
              state.status = 'completed';
              state.data = result;
              state.progress = 100;
            }

            state.endTime = Date.now();
            this.runningTasks.delete(task.id);
            this.updateState();
            this.updateOverallProgress();

            observer.next(state);
            observer.complete();
          },
          error: (error) => {
            if (attemptsLeft > 0) {
              timer(this.config.retryDelay).subscribe(() => {
                executeWithRetry(attemptsLeft - 1);
              });
              return;
            }

            state.status = 'failed';
            state.error = error;
            state.endTime = Date.now();
            state.progress = 0;

            this.runningTasks.delete(task.id);
            this.updateState();
            this.updateOverallProgress();

            observer.next(state);
            observer.complete();
          }
        });
      };

      executeWithRetry(task.retryCount || 0);
    });
  }

  /**
   * Update the loading state subject
   */
  private updateState(): void {
    this.loadingStateSubject.next(new Map(this.loadingStates));
  }

  /**
   * Update overall progress
   */
  private updateOverallProgress(): void {
    const states = Array.from(this.loadingStates.values());
    if (states.length === 0) {
      this.overallProgressSubject.next(0);
      return;
    }

    const totalProgress = states.reduce((sum, state) => sum + state.progress, 0);
    const overallProgress = totalProgress / states.length;
    this.overallProgressSubject.next(overallProgress);
  }

  /**
   * Create dashboard loading tasks
   */
  createDashboardTasks(userRole: string): LoadingTask[] {
    const baseTasks: LoadingTask[] = [
      {
        id: 'user-profile',
        name: 'User Profile',
        priority: 1,
        loadFn: () => this.loadUserProfile()
      },
      {
        id: 'quick-actions',
        name: 'Quick Actions',
        priority: 2,
        loadFn: () => this.loadQuickActions(),
        dependencies: ['user-profile']
      },
      {
        id: 'recent-activities',
        name: 'Recent Activities',
        priority: 3,
        loadFn: () => this.loadRecentActivities()
      }
    ];

    // Add role-specific tasks
    switch (userRole) {
      case 'Employee':
        baseTasks.push({
          id: 'employee-stats',
          name: 'Employee Statistics',
          priority: 2,
          loadFn: () => this.loadEmployeeStats(),
          dependencies: ['user-profile']
        });
        break;

      case 'Manager':
        baseTasks.push(
          {
            id: 'employee-stats',
            name: 'Employee Statistics',
            priority: 2,
            loadFn: () => this.loadEmployeeStats(),
            dependencies: ['user-profile']
          },
          {
            id: 'manager-stats',
            name: 'Manager Statistics',
            priority: 2,
            loadFn: () => this.loadManagerStats(),
            dependencies: ['user-profile']
          },
          {
            id: 'team-overview',
            name: 'Team Overview',
            priority: 3,
            loadFn: () => this.loadTeamOverview(),
            dependencies: ['manager-stats']
          }
        );
        break;

      case 'HR':
        baseTasks.push(
          {
            id: 'hr-stats',
            name: 'HR Statistics',
            priority: 2,
            loadFn: () => this.loadHRStats(),
            dependencies: ['user-profile']
          },
          {
            id: 'employee-overview',
            name: 'Employee Overview',
            priority: 3,
            loadFn: () => this.loadEmployeeOverview(),
            dependencies: ['hr-stats']
          }
        );
        break;

      case 'Admin':
      case 'SuperAdmin':
        baseTasks.push(
          {
            id: 'admin-stats',
            name: 'Admin Statistics',
            priority: 2,
            loadFn: () => this.loadAdminStats(),
            dependencies: ['user-profile']
          },
          {
            id: 'system-health',
            name: 'System Health',
            priority: 3,
            loadFn: () => this.loadSystemHealth(),
            dependencies: ['admin-stats']
          }
        );
        break;
    }

    return baseTasks;
  }

  // Mock data loading functions (replace with actual API calls)
  private loadUserProfile(): Observable<any> {
    return timer(500).pipe(map(() => ({ name: 'John Doe', role: 'Employee' })));
  }

  private loadQuickActions(): Observable<any> {
    return timer(300).pipe(map(() => [{ id: 1, name: 'Check In' }]));
  }

  private loadRecentActivities(): Observable<any> {
    return timer(800).pipe(map(() => [{ id: 1, message: 'User logged in' }]));
  }

  private loadEmployeeStats(): Observable<any> {
    return timer(600).pipe(map(() => ({ hoursWorked: 8, tasksCompleted: 5 })));
  }

  private loadManagerStats(): Observable<any> {
    return timer(700).pipe(map(() => ({ teamSize: 10, projectsActive: 3 })));
  }

  private loadTeamOverview(): Observable<any> {
    return timer(900).pipe(map(() => ({ presentToday: 8, totalTeam: 10 })));
  }

  private loadHRStats(): Observable<any> {
    return timer(800).pipe(map(() => ({ totalEmployees: 150, newHires: 5 })));
  }

  private loadEmployeeOverview(): Observable<any> {
    return timer(1000).pipe(map(() => ({ active: 140, onLeave: 10 })));
  }

  private loadAdminStats(): Observable<any> {
    return timer(900).pipe(map(() => ({ systemUptime: '99.9%', activeUsers: 98 })));
  }

  private loadSystemHealth(): Observable<any> {
    return timer(1200).pipe(map(() => ({ status: 'Good', alerts: 0 })));
  }
}