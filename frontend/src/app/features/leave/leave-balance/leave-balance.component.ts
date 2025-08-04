import { Component, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { LeaveService } from '../../../services/leave.service';
import { LeaveBalance, LeaveType } from '../../../models/leave.models';

@Component({
  selector: 'app-leave-balance',
  standalone: true,
  imports: [CommonModule, NgbModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h5 class="card-title mb-0">
          <i class="fas fa-chart-pie me-2"></i>
          Leave Balance {{ currentYear }}
        </h5>
      </div>
      <div class="card-body">
        <div class="row" *ngIf="leaveBalances.length > 0; else noBalances">
          <div class="col-lg-6 col-xl-4 mb-4" *ngFor="let balance of leaveBalances">
            <div class="balance-card">
              <div class="balance-header">
                <div class="d-flex justify-content-between align-items-center">
                  <h6 class="balance-title">{{ balance.leaveTypeName }}</h6>
                  <span class="badge" [style.background-color]="getLeaveTypeColor(balance.leaveType)">
                    {{ getLeaveTypeText(balance.leaveType) }}
                  </span>
                </div>
              </div>
              
              <div class="balance-body">
                <!-- Progress Bar -->
                <div class="progress mb-3" style="height: 8px;">
                  <div 
                    class="progress-bar" 
                    role="progressbar" 
                    [style.width.%]="getUsagePercentage(balance)"
                    [style.background-color]="getProgressColor(balance)">
                  </div>
                </div>
                
                <!-- Balance Details -->
                <div class="row text-center">
                  <div class="col-4">
                    <div class="balance-stat">
                      <div class="stat-value text-primary">{{ balance.allocatedDays }}</div>
                      <div class="stat-label">Allocated</div>
                    </div>
                  </div>
                  <div class="col-4">
                    <div class="balance-stat">
                      <div class="stat-value text-danger">{{ balance.usedDays }}</div>
                      <div class="stat-label">Used</div>
                    </div>
                  </div>
                  <div class="col-4">
                    <div class="balance-stat">
                      <div class="stat-value text-success">{{ balance.remainingDays }}</div>
                      <div class="stat-label">Remaining</div>
                    </div>
                  </div>
                </div>
                
                <!-- Additional Info -->
                <div class="row mt-3" *ngIf="balance.carriedForwardDays > 0 || balance.encashedDays > 0">
                  <div class="col-6" *ngIf="balance.carriedForwardDays > 0">
                    <small class="text-muted">
                      <i class="fas fa-arrow-right me-1"></i>
                      Carried Forward: {{ balance.carriedForwardDays }}
                    </small>
                  </div>
                  <div class="col-6" *ngIf="balance.encashedDays > 0">
                    <small class="text-muted">
                      <i class="fas fa-money-bill me-1"></i>
                      Encashed: {{ balance.encashedDays }}
                    </small>
                  </div>
                </div>
                
                <!-- Warning for Low Balance -->
                <div class="alert alert-warning mt-3 mb-0" *ngIf="isLowBalance(balance)">
                  <i class="fas fa-exclamation-triangle me-2"></i>
                  <small>Low balance remaining!</small>
                </div>
              </div>
            </div>
          </div>
        </div>

        <ng-template #noBalances>
          <div class="text-center py-5">
            <i class="fas fa-calendar-times text-muted mb-3" style="font-size: 3rem;"></i>
            <h5 class="text-muted">No Leave Balances Found</h5>
            <p class="text-muted">Leave balances will appear here once they are allocated.</p>
          </div>
        </ng-template>

        <!-- Summary Card -->
        <div class="row mt-4" *ngIf="leaveBalances.length > 0">
          <div class="col-12">
            <div class="card bg-light">
              <div class="card-body">
                <h6 class="card-title">
                  <i class="fas fa-chart-bar me-2"></i>
                  Summary
                </h6>
                <div class="row text-center">
                  <div class="col-md-3">
                    <div class="summary-stat">
                      <div class="stat-value text-primary">{{ getTotalAllocated() }}</div>
                      <div class="stat-label">Total Allocated</div>
                    </div>
                  </div>
                  <div class="col-md-3">
                    <div class="summary-stat">
                      <div class="stat-value text-danger">{{ getTotalUsed() }}</div>
                      <div class="stat-label">Total Used</div>
                    </div>
                  </div>
                  <div class="col-md-3">
                    <div class="summary-stat">
                      <div class="stat-value text-success">{{ getTotalRemaining() }}</div>
                      <div class="stat-label">Total Remaining</div>
                    </div>
                  </div>
                  <div class="col-md-3">
                    <div class="summary-stat">
                      <div class="stat-value text-info">{{ getUsagePercentageTotal() }}%</div>
                      <div class="stat-label">Usage Rate</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Action Buttons -->
        <div class="d-flex justify-content-center mt-4" *ngIf="leaveBalances.length > 0">
          <button class="btn btn-outline-primary me-2" (click)="refreshBalances()">
            <i class="fas fa-sync-alt me-2"></i>
            Refresh
          </button>
          <button class="btn btn-outline-success" (click)="downloadReport()">
            <i class="fas fa-download me-2"></i>
            Download Report
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card {
      border-radius: 12px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .card-header {
      background: linear-gradient(135deg, var(--bs-primary) 0%, #0056b3 100%);
      color: white;
      border-radius: 12px 12px 0 0;
    }

    .balance-card {
      background: white;
      border: 1px solid #e9ecef;
      border-radius: 12px;
      padding: 1.5rem;
      height: 100%;
      transition: all 0.3s ease;
    }

    .balance-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    }

    .balance-header {
      margin-bottom: 1rem;
    }

    .balance-title {
      font-weight: 600;
      color: var(--bs-gray-800);
      margin: 0;
    }

    .badge {
      font-size: 0.7rem;
      padding: 0.25rem 0.5rem;
      border-radius: 6px;
    }

    .progress {
      border-radius: 10px;
      background-color: #f8f9fa;
    }

    .progress-bar {
      border-radius: 10px;
      transition: width 0.6s ease;
    }

    .balance-stat {
      padding: 0.5rem 0;
    }

    .stat-value {
      font-size: 1.5rem;
      font-weight: 700;
      line-height: 1;
    }

    .stat-label {
      font-size: 0.75rem;
      color: var(--bs-gray-600);
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin-top: 0.25rem;
    }

    .summary-stat {
      padding: 1rem 0;
    }

    .summary-stat .stat-value {
      font-size: 2rem;
    }

    .alert {
      border-radius: 8px;
      border: none;
      padding: 0.5rem 0.75rem;
    }

    .alert-warning {
      background-color: #fff3cd;
      color: #856404;
    }

    .btn {
      border-radius: 8px;
      font-weight: 500;
      padding: 0.625rem 1.25rem;
    }

    .bg-light {
      background-color: #f8f9fa !important;
    }

    @media (max-width: 768px) {
      .balance-card {
        margin-bottom: 1rem;
      }
      
      .stat-value {
        font-size: 1.25rem;
      }
      
      .summary-stat .stat-value {
        font-size: 1.5rem;
      }
    }
  `]
})
export class LeaveBalanceComponent implements OnInit {
  @Input() employeeId?: number;
  
  leaveBalances: LeaveBalance[] = [];
  currentYear = new Date().getFullYear();
  isLoading = false;

  constructor(private leaveService: LeaveService) {}

  ngOnInit(): void {
    this.loadLeaveBalances();
  }

  private loadLeaveBalances(): void {
    this.isLoading = true;
    
    const balanceObservable = this.employeeId 
      ? this.leaveService.getEmployeeLeaveBalances(this.employeeId)
      : this.leaveService.getMyLeaveBalances();

    balanceObservable.subscribe({
      next: (balances) => {
        this.leaveBalances = balances.filter(b => b.year === this.currentYear);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading leave balances:', error);
        this.isLoading = false;
      }
    });
  }

  getLeaveTypeColor(leaveType: LeaveType): string {
    return this.leaveService.getLeaveTypeColor(leaveType);
  }

  getLeaveTypeText(leaveType: LeaveType): string {
    return this.leaveService.getLeaveTypeText(leaveType);
  }

  getUsagePercentage(balance: LeaveBalance): number {
    if (balance.allocatedDays === 0) return 0;
    return Math.round((balance.usedDays / balance.allocatedDays) * 100);
  }

  getProgressColor(balance: LeaveBalance): string {
    const percentage = this.getUsagePercentage(balance);
    if (percentage >= 90) return '#dc3545'; // Red
    if (percentage >= 70) return '#fd7e14'; // Orange
    if (percentage >= 50) return '#ffc107'; // Yellow
    return '#28a745'; // Green
  }

  isLowBalance(balance: LeaveBalance): boolean {
    return balance.remainingDays <= 2 && balance.remainingDays > 0;
  }

  getTotalAllocated(): number {
    return this.leaveBalances.reduce((total, balance) => total + balance.allocatedDays, 0);
  }

  getTotalUsed(): number {
    return this.leaveBalances.reduce((total, balance) => total + balance.usedDays, 0);
  }

  getTotalRemaining(): number {
    return this.leaveBalances.reduce((total, balance) => total + balance.remainingDays, 0);
  }

  getUsagePercentageTotal(): number {
    const totalAllocated = this.getTotalAllocated();
    const totalUsed = this.getTotalUsed();
    if (totalAllocated === 0) return 0;
    return Math.round((totalUsed / totalAllocated) * 100);
  }

  refreshBalances(): void {
    this.loadLeaveBalances();
  }

  downloadReport(): void {
    // Implement download functionality
    const reportData = {
      year: this.currentYear,
      balances: this.leaveBalances,
      summary: {
        totalAllocated: this.getTotalAllocated(),
        totalUsed: this.getTotalUsed(),
        totalRemaining: this.getTotalRemaining(),
        usagePercentage: this.getUsagePercentageTotal()
      }
    };

    const dataStr = JSON.stringify(reportData, null, 2);
    const dataBlob = new Blob([dataStr], { type: 'application/json' });
    const url = URL.createObjectURL(dataBlob);
    
    const link = document.createElement('a');
    link.href = url;
    link.download = `leave-balance-report-${this.currentYear}.json`;
    link.click();
    
    URL.revokeObjectURL(url);
  }
}