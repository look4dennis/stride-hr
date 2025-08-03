import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { EmployeeService } from '../../../services/employee.service';
import { Employee, OrganizationalChart } from '../../../models/employee.models';
import { NotificationService } from '../../../core/services/notification.service';

// Org Node Component for Tree View
@Component({
  selector: 'app-org-node',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="org-node">
      <div class="employee-card" (click)="onEmployeeClick()">
        <img [src]="getProfilePhoto()" 
             [alt]="node.employee.firstName + ' ' + node.employee.lastName"
             class="employee-photo">
        <div class="employee-info">
          <div class="employee-name">{{ node.employee.firstName }} {{ node.employee.lastName }}</div>
          <div class="employee-title">{{ node.employee.designation }}</div>
          <div class="employee-department">{{ node.employee.department }}</div>
        </div>
      </div>
      
      <div class="children" *ngIf="node.children && node.children.length > 0">
        <div class="connection-line"></div>
        <div class="child-nodes">
          <div class="child-node" *ngFor="let child of node.children">
            <div class="child-connection"></div>
            <app-org-node [node]="child" (employeeClick)="onChildEmployeeClick($event)"></app-org-node>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .org-node {
      display: flex;
      flex-direction: column;
      align-items: center;
      position: relative;
    }

    .employee-card {
      background: white;
      border: 2px solid #e9ecef;
      border-radius: 12px;
      padding: 1rem;
      text-align: center;
      cursor: pointer;
      transition: all 0.3s ease;
      min-width: 200px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

    .employee-card:hover {
      border-color: #007bff;
      transform: translateY(-2px);
      box-shadow: 0 4px 16px rgba(0, 123, 255, 0.2);
    }

    .employee-photo {
      width: 60px;
      height: 60px;
      border-radius: 50%;
      object-fit: cover;
      margin-bottom: 0.75rem;
      border: 3px solid #f8f9fa;
    }

    .employee-name {
      font-weight: 600;
      color: #2c3e50;
      margin-bottom: 0.25rem;
      font-size: 0.95rem;
    }

    .employee-title {
      color: #495057;
      font-size: 0.85rem;
      margin-bottom: 0.25rem;
      font-weight: 500;
    }

    .employee-department {
      color: #6c757d;
      font-size: 0.8rem;
    }

    .children {
      margin-top: 2rem;
      position: relative;
    }

    .connection-line {
      width: 2px;
      height: 30px;
      background-color: #dee2e6;
      margin: 0 auto;
    }

    .child-nodes {
      display: flex;
      gap: 2rem;
      position: relative;
    }

    .child-nodes::before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      height: 2px;
      background-color: #dee2e6;
    }

    .child-node {
      position: relative;
    }

    .child-connection {
      width: 2px;
      height: 30px;
      background-color: #dee2e6;
      margin: 0 auto;
      margin-bottom: 0;
    }
  `]
})
export class OrgNodeComponent {
  @Input() node!: OrganizationalChart;
  @Output() employeeClick = new EventEmitter<Employee>();

  onEmployeeClick(): void {
    this.employeeClick.emit(this.node.employee);
  }

  onChildEmployeeClick(employee: Employee): void {
    this.employeeClick.emit(employee);
  }

  getProfilePhoto(): string {
    return this.node.employee.profilePhoto || '/assets/images/avatars/default-avatar.png';
  }
}

@Component({
  selector: 'app-org-chart',
  standalone: true,
  imports: [CommonModule, FormsModule, OrgNodeComponent],
  template: `
    <div class="page-header d-flex justify-content-between align-items-center">
      <div>
        <h1>Organizational Chart</h1>
        <p class="text-muted">Visualize your organization's structure</p>
      </div>
      <div class="d-flex gap-2">
        <select class="form-select" [(ngModel)]="selectedBranch" (change)="loadOrgChart()">
          <option value="">All Branches</option>
          <option value="1">Main Office</option>
          <option value="2">Branch Office</option>
        </select>
        <div class="btn-group" role="group">
          <input type="radio" class="btn-check" name="viewType" id="treeView" 
                 [checked]="viewType === 'tree'" (change)="setViewType('tree')">
          <label class="btn btn-outline-primary" for="treeView">
            <i class="fas fa-sitemap me-1"></i>Tree
          </label>
          <input type="radio" class="btn-check" name="viewType" id="listView" 
                 [checked]="viewType === 'list'" (change)="setViewType('list')">
          <label class="btn btn-outline-primary" for="listView">
            <i class="fas fa-list me-1"></i>List
          </label>
        </div>
      </div>
    </div>

    <!-- Tree View -->
    <div class="org-chart-container" *ngIf="viewType === 'tree' && !loading">
      <div class="org-tree" *ngIf="orgChart.length > 0">
        <div class="tree-node" *ngFor="let node of orgChart">
          <app-org-node [node]="node" (employeeClick)="onEmployeeClick($event)"></app-org-node>
        </div>
      </div>
      
      <div class="text-center py-5" *ngIf="orgChart.length === 0">
        <i class="fas fa-sitemap text-muted mb-3" style="font-size: 3rem;"></i>
        <h5>No organizational structure found</h5>
        <p class="text-muted">Add employees and set reporting relationships to build the org chart.</p>
      </div>
    </div>

    <!-- List View -->
    <div class="card" *ngIf="viewType === 'list' && !loading">
      <div class="card-body">
        <div class="org-list">
          <div class="list-item" *ngFor="let item of flatOrgList" 
               [style.margin-left.px]="item.level * 30">
            <div class="employee-info" (click)="onEmployeeClick(item.employee)">
              <img [src]="getProfilePhoto(item.employee)" 
                   [alt]="item.employee.firstName + ' ' + item.employee.lastName"
                   class="employee-avatar">
              <div class="employee-details">
                <div class="employee-name">
                  {{ item.employee.firstName }} {{ item.employee.lastName }}
                </div>
                <div class="employee-title">{{ item.employee.designation }}</div>
                <div class="employee-department">{{ item.employee.department }}</div>
              </div>
              <div class="employee-actions">
                <span class="level-indicator">Level {{ item.level + 1 }}</span>
                <i class="fas fa-chevron-right"></i>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div class="text-center py-5" *ngIf="loading">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
      <p class="mt-2 text-muted">Loading organizational chart...</p>
    </div>
  `,
  styles: [`
    .org-chart-container {
      overflow-x: auto;
      padding: 2rem;
      min-height: 400px;
    }

    .org-tree {
      display: flex;
      justify-content: center;
      align-items: flex-start;
      gap: 2rem;
    }

    .org-list {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .list-item {
      border: 1px solid #e9ecef;
      border-radius: 8px;
      transition: all 0.3s ease;
    }

    .list-item:hover {
      border-color: #007bff;
      box-shadow: 0 2px 8px rgba(0, 123, 255, 0.1);
    }

    .employee-info {
      display: flex;
      align-items: center;
      padding: 1rem;
      cursor: pointer;
    }

    .employee-avatar {
      width: 50px;
      height: 50px;
      border-radius: 50%;
      object-fit: cover;
      margin-right: 1rem;
      flex-shrink: 0;
    }

    .employee-details {
      flex-grow: 1;
    }

    .employee-name {
      font-weight: 600;
      color: #2c3e50;
      margin-bottom: 0.25rem;
    }

    .employee-title {
      color: #495057;
      font-size: 0.875rem;
      margin-bottom: 0.25rem;
    }

    .employee-department {
      color: #6c757d;
      font-size: 0.875rem;
    }

    .employee-actions {
      display: flex;
      align-items: center;
      gap: 1rem;
      flex-shrink: 0;
    }

    .level-indicator {
      background-color: #e9ecef;
      color: #495057;
      padding: 0.25rem 0.5rem;
      border-radius: 12px;
      font-size: 0.75rem;
      font-weight: 500;
    }

    .btn-group .btn-check:checked + .btn {
      background-color: #007bff;
      border-color: #007bff;
      color: white;
    }
  `]
})
export class OrgChartComponent implements OnInit {
  orgChart: OrganizationalChart[] = [];
  flatOrgList: OrganizationalChart[] = [];
  viewType: 'tree' | 'list' = 'tree';
  selectedBranch: string = '';
  loading = false;

  constructor(
    private employeeService: EmployeeService,
    private router: Router,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadOrgChart();
  }

  loadOrgChart(): void {
    this.loading = true;
    
    // Mock organizational chart data
    setTimeout(() => {
      this.orgChart = this.getMockOrgChart();
      this.flatOrgList = this.flattenOrgChart(this.orgChart);
      this.loading = false;
    }, 500);

    // Uncomment for production
    // const branchId = this.selectedBranch ? parseInt(this.selectedBranch) : undefined;
    // this.employeeService.getOrganizationalChart(branchId).subscribe({
    //   next: (chart) => {
    //     this.orgChart = chart;
    //     this.flatOrgList = this.flattenOrgChart(chart);
    //     this.loading = false;
    //   },
    //   error: (error) => {
    //     this.notificationService.showError('Failed to load organizational chart');
    //     this.loading = false;
    //   }
    // });
  }

  setViewType(type: 'tree' | 'list'): void {
    this.viewType = type;
  }

  onEmployeeClick(employee: Employee): void {
    this.router.navigate(['/employees', employee.id]);
  }

  getProfilePhoto(employee: Employee): string {
    return employee.profilePhoto || '/assets/images/avatars/default-avatar.png';
  }

  private flattenOrgChart(chart: OrganizationalChart[]): OrganizationalChart[] {
    const flattened: OrganizationalChart[] = [];
    
    const flatten = (nodes: OrganizationalChart[]) => {
      nodes.forEach(node => {
        flattened.push(node);
        if (node.children && node.children.length > 0) {
          flatten(node.children);
        }
      });
    };
    
    flatten(chart);
    return flattened;
  }

  private getMockOrgChart(): OrganizationalChart[] {
    const ceo: Employee = {
      id: 1,
      employeeId: 'CEO001',
      branchId: 1,
      firstName: 'Alice',
      lastName: 'Johnson',
      email: 'alice.johnson@company.com',
      phone: '+1-555-0001',
      profilePhoto: '/assets/images/avatars/alice-johnson.jpg',
      dateOfBirth: '1975-03-20',
      joiningDate: '2015-01-01',
      designation: 'Chief Executive Officer',
      department: 'Executive',
      basicSalary: 200000,
      status: 'Active' as any,
      createdAt: '2015-01-01T00:00:00Z'
    };

    const hrManager: Employee = {
      id: 2,
      employeeId: 'HR001',
      branchId: 1,
      firstName: 'Sarah',
      lastName: 'Wilson',
      email: 'sarah.wilson@company.com',
      phone: '+1-555-0002',
      profilePhoto: '/assets/images/avatars/sarah-wilson.jpg',
      dateOfBirth: '1988-04-18',
      joiningDate: '2019-09-15',
      designation: 'HR Manager',
      department: 'Human Resources',
      basicSalary: 85000,
      status: 'Active' as any,
      reportingManagerId: 1,
      createdAt: '2019-09-15T00:00:00Z'
    };

    const devManager: Employee = {
      id: 3,
      employeeId: 'DEV001',
      branchId: 1,
      firstName: 'Jane',
      lastName: 'Smith',
      email: 'jane.smith@company.com',
      phone: '+1-555-0003',
      profilePhoto: '/assets/images/avatars/jane-smith.jpg',
      dateOfBirth: '1985-08-22',
      joiningDate: '2018-03-10',
      designation: 'Development Manager',
      department: 'Development',
      basicSalary: 95000,
      status: 'Active' as any,
      reportingManagerId: 1,
      createdAt: '2018-03-10T00:00:00Z'
    };

    const seniorDev: Employee = {
      id: 4,
      employeeId: 'DEV002',
      branchId: 1,
      firstName: 'John',
      lastName: 'Doe',
      email: 'john.doe@company.com',
      phone: '+1-555-0004',
      profilePhoto: '/assets/images/avatars/john-doe.jpg',
      dateOfBirth: '1990-05-15',
      joiningDate: '2020-01-15',
      designation: 'Senior Developer',
      department: 'Development',
      basicSalary: 75000,
      status: 'Active' as any,
      reportingManagerId: 3,
      createdAt: '2020-01-15T00:00:00Z'
    };

    const juniorDev: Employee = {
      id: 5,
      employeeId: 'DEV003',
      branchId: 1,
      firstName: 'Mike',
      lastName: 'Johnson',
      email: 'mike.johnson@company.com',
      phone: '+1-555-0005',
      profilePhoto: '/assets/images/avatars/mike-johnson.jpg',
      dateOfBirth: '1992-12-03',
      joiningDate: '2021-06-01',
      designation: 'Junior Developer',
      department: 'Development',
      basicSalary: 55000,
      status: 'Active' as any,
      reportingManagerId: 3,
      createdAt: '2021-06-01T00:00:00Z'
    };

    return [
      {
        employee: ceo,
        level: 0,
        children: [
          {
            employee: hrManager,
            level: 1,
            children: []
          },
          {
            employee: devManager,
            level: 1,
            children: [
              {
                employee: seniorDev,
                level: 2,
                children: []
              },
              {
                employee: juniorDev,
                level: 2,
                children: []
              }
            ]
          }
        ]
      }
    ];
  }
}

