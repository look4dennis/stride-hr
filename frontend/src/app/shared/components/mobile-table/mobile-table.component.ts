import { Component, Input, Output, EventEmitter, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface MobileTableColumn {
  key: string;
  label: string;
  sortable?: boolean;
  width?: string;
  align?: 'left' | 'center' | 'right';
  template?: TemplateRef<any>;
}

export interface MobileTableAction {
  label: string;
  icon?: string;
  color?: 'primary' | 'secondary' | 'success' | 'warning' | 'danger' | 'info';
  action: (item: any) => void;
  visible?: (item: any) => boolean;
}

@Component({
  selector: 'app-mobile-table',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="mobile-table-container">
      <!-- Desktop Table View -->
      <div class="table-responsive d-none d-md-block">
        <table class="table table-hover">
          <thead>
            <tr>
              <th 
                *ngFor="let column of columns"
                [style.width]="column.width"
                [class.text-center]="column.align === 'center'"
                [class.text-end]="column.align === 'right'"
                [class.sortable]="column.sortable"
                (click)="column.sortable && sort(column.key)">
                {{ column.label }}
                <i 
                  *ngIf="column.sortable" 
                  class="fas fa-sort ms-1"
                  [class.fa-sort-up]="sortColumn === column.key && sortDirection === 'asc'"
                  [class.fa-sort-down]="sortColumn === column.key && sortDirection === 'desc'">
                </i>
              </th>
              <th *ngIf="actions.length > 0" class="text-center">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of data; trackBy: trackByFn">
              <td 
                *ngFor="let column of columns"
                [class.text-center]="column.align === 'center'"
                [class.text-end]="column.align === 'right'">
                <ng-container *ngIf="column.template; else defaultCell">
                  <ng-container 
                    *ngTemplateOutlet="column.template; context: { $implicit: item, column: column }">
                  </ng-container>
                </ng-container>
                <ng-template #defaultCell>
                  {{ getNestedValue(item, column.key) }}
                </ng-template>
              </td>
              <td *ngIf="actions.length > 0" class="text-center">
                <div class="btn-group" role="group">
                  <button 
                    *ngFor="let action of getVisibleActions(item)"
                    type="button"
                    class="btn btn-sm"
                    [class]="'btn-outline-' + (action.color || 'primary')"
                    (click)="action.action(item)"
                    [title]="action.label">
                    <i *ngIf="action.icon" [class]="action.icon"></i>
                    <span *ngIf="!action.icon">{{ action.label }}</span>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Mobile Card View -->
      <div class="mobile-cards d-md-none">
        <div 
          class="mobile-card" 
          *ngFor="let item of data; trackBy: trackByFn"
          (click)="onCardClick(item)">
          <div class="card-content">
            <div 
              class="card-field" 
              *ngFor="let column of columns; let first = first"
              [class.primary-field]="first">
              <div class="field-label">{{ column.label }}</div>
              <div class="field-value">
                <ng-container *ngIf="column.template; else defaultMobileCell">
                  <ng-container 
                    *ngTemplateOutlet="column.template; context: { $implicit: item, column: column }">
                  </ng-container>
                </ng-container>
                <ng-template #defaultMobileCell>
                  {{ getNestedValue(item, column.key) }}
                </ng-template>
              </div>
            </div>
          </div>
          
          <div class="card-actions" *ngIf="actions.length > 0" (click)="$event.stopPropagation()">
            <div class="dropdown">
              <button 
                class="btn btn-outline-secondary btn-sm dropdown-toggle" 
                type="button" 
                [id]="'actions-' + getItemId(item)"
                data-bs-toggle="dropdown" 
                aria-expanded="false">
                <i class="fas fa-ellipsis-v"></i>
              </button>
              <ul class="dropdown-menu" [attr.aria-labelledby]="'actions-' + getItemId(item)">
                <li *ngFor="let action of getVisibleActions(item)">
                  <a 
                    class="dropdown-item" 
                    href="#" 
                    (click)="action.action(item); $event.preventDefault()">
                    <i *ngIf="action.icon" [class]="action.icon + ' me-2'"></i>
                    {{ action.label }}
                  </a>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div class="empty-state text-center py-5" *ngIf="data.length === 0">
        <i class="fas fa-inbox text-muted mb-3" style="font-size: 3rem;"></i>
        <h5 class="text-muted">{{ emptyMessage }}</h5>
        <p class="text-muted">{{ emptySubMessage }}</p>
      </div>

      <!-- Loading State -->
      <div class="loading-state text-center py-5" *ngIf="loading">
        <div class="spinner-border text-primary mb-3" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
        <p class="text-muted">{{ loadingMessage }}</p>
      </div>
    </div>
  `,
  styles: [`
    .mobile-table-container {
      width: 100%;
    }

    .table {
      margin-bottom: 0;
    }

    .sortable {
      cursor: pointer;
      user-select: none;
      transition: background-color 0.15s ease;
    }

    .sortable:hover {
      background-color: var(--gray-50);
    }

    .mobile-cards {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .mobile-card {
      background: white;
      border: 1px solid var(--gray-200);
      border-radius: 12px;
      padding: 1rem;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
      transition: all 0.2s ease;
      cursor: pointer;
      -webkit-tap-highlight-color: rgba(0, 0, 0, 0.05);
      touch-action: manipulation;
    }

    .mobile-card:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
    }

    .mobile-card:active {
      transform: translateY(0) scale(0.98);
    }

    .card-content {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .card-field {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 1rem;
    }

    .card-field.primary-field {
      border-bottom: 1px solid var(--gray-100);
      padding-bottom: 0.75rem;
      margin-bottom: 0.25rem;
    }

    .card-field.primary-field .field-value {
      font-weight: 600;
      font-size: 1.1rem;
      color: var(--text-primary);
    }

    .field-label {
      font-weight: 500;
      color: var(--text-secondary);
      font-size: 0.875rem;
      min-width: 80px;
      flex-shrink: 0;
    }

    .field-value {
      color: var(--text-primary);
      font-size: 0.9rem;
      text-align: right;
      word-break: break-word;
    }

    .card-actions {
      margin-top: 0.75rem;
      padding-top: 0.75rem;
      border-top: 1px solid var(--gray-100);
      display: flex;
      justify-content: flex-end;
    }

    .dropdown-menu {
      border: none;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
      border-radius: 8px;
      min-width: 160px;
    }

    .dropdown-item {
      padding: 0.75rem 1rem;
      font-size: 0.9rem;
      transition: all 0.15s ease;
      display: flex;
      align-items: center;
    }

    .dropdown-item:hover {
      background-color: var(--gray-50);
      transform: translateX(2px);
    }

    .empty-state,
    .loading-state {
      min-height: 200px;
      display: flex;
      flex-direction: column;
      justify-content: center;
      align-items: center;
    }

    /* Enhanced mobile responsiveness */
    @media (max-width: 576px) {
      .mobile-card {
        padding: 0.75rem;
      }
      
      .card-field {
        flex-direction: column;
        align-items: flex-start;
        gap: 0.25rem;
      }
      
      .field-label {
        min-width: auto;
        font-size: 0.8rem;
      }
      
      .field-value {
        text-align: left;
        font-size: 0.875rem;
      }
      
      .card-field.primary-field .field-value {
        font-size: 1rem;
      }
    }

    /* Improved table responsiveness */
    @media (max-width: 991px) {
      .table-responsive {
        font-size: 0.875rem;
      }
      
      .table th,
      .table td {
        padding: 0.5rem 0.25rem;
        white-space: nowrap;
      }
      
      .btn-group .btn {
        padding: 0.25rem 0.5rem;
        font-size: 0.8rem;
      }
    }

    /* Accessibility improvements */
    .mobile-card:focus {
      outline: 2px solid var(--primary);
      outline-offset: 2px;
    }

    .sortable:focus {
      outline: 2px solid var(--primary);
      outline-offset: -2px;
    }
  `]
})
export class MobileTableComponent {
  @Input() data: any[] = [];
  @Input() columns: MobileTableColumn[] = [];
  @Input() actions: MobileTableAction[] = [];
  @Input() loading: boolean = false;
  @Input() emptyMessage: string = 'No data available';
  @Input() emptySubMessage: string = 'There are no items to display at this time.';
  @Input() loadingMessage: string = 'Loading data...';
  @Input() sortColumn: string = '';
  @Input() sortDirection: 'asc' | 'desc' = 'asc';

  @Output() sortChange = new EventEmitter<{ column: string; direction: 'asc' | 'desc' }>();
  @Output() cardClick = new EventEmitter<any>();

  sort(column: string): void {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    
    this.sortChange.emit({
      column: this.sortColumn,
      direction: this.sortDirection
    });
  }

  onCardClick(item: any): void {
    this.cardClick.emit(item);
  }

  getVisibleActions(item: any): MobileTableAction[] {
    return this.actions.filter(action => 
      !action.visible || action.visible(item)
    );
  }

  getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((current, key) => current?.[key], obj);
  }

  getItemId(item: any): string {
    return item.id || item._id || Math.random().toString(36).substr(2, 9);
  }

  trackByFn(index: number, item: any): any {
    return this.getItemId(item);
  }
}