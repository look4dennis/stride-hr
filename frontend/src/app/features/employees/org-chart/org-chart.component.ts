import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { EnhancedEmployeeService } from '../../../services/enhanced-employee.service';
import { BranchService } from '../../../services/branch.service';
import { OrganizationalChart, Employee } from '../../../models/employee.models';
import { Branch } from '../../../models/admin.models';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-org-chart',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-header d-flex justify-content-between align-items-center">
      <div>
        <h1>Organizational Chart</h1>
        <p class="text-muted">View your organization's hierarchy</p>
      </div>
      <div class="d-flex gap-2">
        <select class="form-select" [(ngModel)]="selectedBranchId" (change)="onBranchChange()">
          <option value="">All Branches</option>
          <option *ngFor="let branch of branches" [value]="branch.id">
            {{ branch.name }}
          </option>
        </select>
        <button class="btn btn-outline-primary" (click)="refreshChart()">
          <i class="fas fa-sync-alt me-2"></i>Refresh
        </button>
      </div>
    </div>

    <div class="card">
      <div class="card-body">
        <div class="org-chart-container" *ngIf="!loading && orgChart.length > 0">
          <div class="org-chart">
            <div class="org-level" *ngFor="let level of getLevels(); let levelIndex = index">
              <div class="level-title" *ngIf="levelIndex > 0">
                Level {{ levelIndex + 1 }}
              </div>
              <div class="employee-nodes">
                <div class="employee-node" 
                     *ngFor="let node of getEmployeesAtLevel(levelIndex)"
                     [class.has-children]="node.children.length > 0"
                     (click)="viewEmployeeProfile(node.employee.id)">
                  
                  <div class="employee-card">
                    <div class="employee-avatar">
                      <img [src]="getProfilePhoto(node.employee)" 
                           [alt]="node.employee.firstName + ' ' + node.employee.lastName"
                           class="avatar-img">
                    </div>
                    
                    <div class="employee-info">
                      <h6 class="employee-name">
                        {{ node.employee.firstName }} {{ node.employee.lastName }}
                      </h6>
                      <p class="employee-designation">{{ node.employee.designation }}</p>
                      <p class="employee-department">{{ node.employee.department }}</p>
                      <p class="employee-id">{{ node.employee.employeeId }}</p>
                    </div>
                    
                    <div class="employee-stats" *ngIf="node.children.length > 0">
                      <span class="subordinates-count">
                        <i class="fas fa-users me-1"></i>{{ node.children.length }}
                      </span>
                    </div>
                  </div>
                  
                  <!-- Connection lines -->
                  <div class="connection-line" *ngIf="levelIndex > 0"></div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Empty State -->
        <div class="text-center py-5" *ngIf="!loading && orgChart.length === 0">
          <i class="fas fa-sitemap text-muted mb-3" style="font-size: 3rem;"></i>
          <h5>No organizational data found</h5>
          <p class="text-muted">
            {{ selectedBranchId ? 'No employees found for the selected branch.' : 'No employees found in the organization.' }}
          </p>
          <button class="btn btn-primary" (click)="navigateToEmployeeList()">
            <i class="fas fa-plus me-2"></i>Add Employees
          </button>
        </div>

        <!-- Loading State -->
        <div class="text-center py-5" *ngIf="loading">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
          <p class="mt-2 text-muted">Loading organizational chart...</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page-header {
      margin-bottom: 2rem;
    }
    
    .page-header h1 {
      font-size: 2rem;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    .org-chart-container {
      overflow-x: auto;
      padding: 2rem;
    }

    .org-chart {
      min-width: 800px;
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .org-level {
      margin-bottom: 3rem;
      width: 100%;
    }

    .level-title {
      text-align: center;
      font-weight: 600;
      color: #6c757d;
      margin-bottom: 1rem;
      font-size: 0.875rem;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .employee-nodes {
      display: flex;
      justify-content: center;
      flex-wrap: wrap;
      gap: 2rem;
    }

    .employee-node {
      position: relative;
      cursor: pointer;
      transition: transform 0.2s ease;
    }

    .employee-node:hover {
      transform: translateY(-2px);
    }

    .employee-card {
      background: white;
      border-radius: 12px;
      padding: 1.5rem;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
      border: 2px solid #e9ecef;
      text-align: center;
      min-width: 200px;
      transition: all 0.3s ease;
    }

    .employee-node:hover .employee-card {
      border-color: #007bff;
      box-shadow: 0 6px 20px rgba(0, 123, 255, 0.15);
    }

    .employee-avatar {
      margin-bottom: 1rem;
    }

    .avatar-img {
      width: 60px;
      height: 60px;
      border-radius: 50%;
      object-fit: cover;
      border: 3px solid #f8f9fa;
    }

    .employee-info {
      margin-bottom: 1rem;
    }

    .employee-name {
      font-weight: 600;
      color: #2c3e50;
      margin-bottom: 0.25rem;
      font-size: 1rem;
    }

    .employee-designation {
      font-weight: 500;
      color: #495057;
      margin-bottom: 0.25rem;
      font-size: 0.875rem;
    }

    .employee-department {
      color: #6c757d;
      margin-bottom: 0.25rem;
      font-size: 0.8rem;
    }

    .employee-id {
      color: #6c757d;
      margin-bottom: 0;
      font-size: 0.75rem;
      font-family: monospace;
    }

    .employee-stats {
      border-top: 1px solid #e9ecef;
      padding-top: 0.75rem;
    }

    .subordinates-count {
      background: #e3f2fd;
      color: #1976d2;
      padding: 0.25rem 0.5rem;
      border-radius: 12px;
      font-size: 0.75rem;
      font-weight: 500;
    }

    .connection-line {
      position: absolute;
      top: -1.5rem;
      left: 50%;
      transform: translateX(-50%);
      width: 2px;
      height: 1.5rem;
      background: #dee2e6;
    }

    .connection-line::before {
      content: '';
      position: absolute;
      top: -2px;
      left: -4px;
      width: 10px;
      height: 2px;
      background: #dee2e6;
    }

    /* CEO/Top level styling */
    .org-level:first-child .employee-card {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border-color: #667eea;
    }

    .org-level:first-child .employee-name,
    .org-level:first-child .employee-designation,
    .org-level:first-child .employee-department,
    .org-level:first-child .employee-id {
      color: white;
    }

    .org-level:first-child .subordinates-count {
      background: rgba(255, 255, 255, 0.2);
      color: white;
    }

    /* Manager level styling */
    .org-level:nth-child(2) .employee-card {
      border-color: #28a745;
    }

    .org-level:nth-child(2) .employee-card:hover {
      border-color: #28a745;
      box-shadow: 0 6px 20px rgba(40, 167, 69, 0.15);
    }

    /* Responsive design */
    @media (max-width: 768px) {
      .page-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 1rem;
      }
      
      .page-header .d-flex {
        width: 100%;
        flex-direction: column;
        gap: 0.5rem;
      }
      
      .org-chart-container {
        padding: 1rem;
      }
      
      .org-chart {
        min-width: auto;
      }
      
      .employee-nodes {
        flex-direction: column;
        align-items: center;
        gap: 1rem;
      }
      
      .employee-card {
        min-width: 250px;
        padding: 1rem;
      }
      
      .connection-line {
        display: none;
      }
    }

    @media (max-width: 576px) {
      .employee-card {
        min-width: 200px;
        padding: 0.75rem;
      }
      
      .avatar-img {
        width: 50px;
        height: 50px;
      }
      
      .employee-name {
        font-size: 0.9rem;
      }
      
      .employee-designation {
        font-size: 0.8rem;
      }
    }

    /* Touch-friendly improvements */
    .employee-node {
      -webkit-tap-highlight-color: rgba(0, 0, 0, 0.05);
      touch-action: manipulation;
    }

    .employee-node:active {
      transform: translateY(-1px) scale(0.98);
    }
  `]
})
export class OrgChartComponent implements OnInit {
  orgChart: OrganizationalChart[] = [];
  branches: Branch[] = [];
  selectedBranchId: number | string = '';
  loading = false;

  constructor(
    private employeeService: EnhancedEmployeeService,
    private branchService: BranchService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadBranches();
    this.loadOrgChart();
  }

  loadBranches(): void {
    this.branchService.getAllBranches().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.branches = response.data;
        }
      },
      error: (error) => {
        console.error('Failed to load branches:', error);
      }
    });
  }

  loadOrgChart(): void {
    this.loading = true;
    
    const branchId = this.selectedBranchId ? parseInt(this.selectedBranchId.toString()) : undefined;
    
    this.employeeService.getOrganizationalChart(branchId).subscribe({
      next: (chart) => {
        this.orgChart = chart;
        this.loading = false;
      },
      error: (error) => {
        // Create mock organizational chart for development
        this.createMockOrgChart();
        this.loading = false;
      }
    });
  }

  onBranchChange(): void {
    this.loadOrgChart();
  }

  refreshChart(): void {
    this.loadOrgChart();
  }

  getLevels(): number[] {
    if (this.orgChart.length === 0) return [];
    
    const maxLevel = Math.max(...this.orgChart.map(node => node.level));
    return Array.from({ length: maxLevel + 1 }, (_, i) => i);
  }

  getEmployeesAtLevel(level: number): OrganizationalChart[] {
    return this.orgChart.filter(node => node.level === level);
  }

  getProfilePhoto(employee: Employee): string {
    return employee.profilePhoto || '/assets/images/avatars/default-avatar.png';
  }

  viewEmployeeProfile(employeeId: number): void {
    this.router.navigate(['/employees', employeeId]);
  }

  navigateToEmployeeList(): void {
    this.router.navigate(['/employees']);
  }

  private createMockOrgChart(): void {
    // Create a mock organizational chart for development
    const mockEmployees: Employee[] = [
      {
        id: 1,
        employeeId: 'CEO001',
        branchId: 1,
        firstName: 'John',
        lastName: 'Smith',
        email: 'john.smith@company.com',
        phone: '+1-555-0001',
        dateOfBirth: '1975-03-15',
        joiningDate: '2015-01-01',
        designation: 'Chief Executive Officer',
        department: 'Executive',
        basicSalary: 200000,
        status: 'Active' as any,
        createdAt: '2015-01-01T00:00:00Z'
      },
      {
        id: 2,
        employeeId: 'MGR001',
        branchId: 1,
        firstName: 'Jane',
        lastName: 'Doe',
        email: 'jane.doe@company.com',
        phone: '+1-555-0002',
        dateOfBirth: '1980-07-22',
        joiningDate: '2018-03-10',
        designation: 'Development Manager',
        department: 'Development',
        basicSalary: 120000,
        status: 'Active' as any,
        reportingManagerId: 1,
        createdAt: '2018-03-10T00:00:00Z'
      },
      {
        id: 3,
        employeeId: 'DEV001',
        branchId: 1,
        firstName: 'Mike',
        lastName: 'Johnson',
        email: 'mike.johnson@company.com',
        phone: '+1-555-0003',
        dateOfBirth: '1990-12-03',
        joiningDate: '2021-06-01',
        designation: 'Senior Developer',
        department: 'Development',
        basicSalary: 85000,
        status: 'Active' as any,
        reportingManagerId: 2,
        createdAt: '2021-06-01T00:00:00Z'
      }
    ];

    this.orgChart = [
      {
        employee: mockEmployees[0],
        children: [
          {
            employee: mockEmployees[1],
            children: [
              {
                employee: mockEmployees[2],
                children: [],
                level: 2
              }
            ],
            level: 1
          }
        ],
        level: 0
      },
      {
        employee: mockEmployees[1],
        children: [
          {
            employee: mockEmployees[2],
            children: [],
            level: 2
          }
        ],
        level: 1
      },
      {
        employee: mockEmployees[2],
        children: [],
        level: 2
      }
    ];
  }
}