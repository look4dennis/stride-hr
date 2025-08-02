import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ReportService } from '../services/report.service';
import { Report } from '../models/report.models';
import { ReportExportFormat } from '../enums/report.enums';

@Component({
  selector: 'app-report-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="container-fluid">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h2>Reports</h2>
        <div>
          <button class="btn btn-primary" routerLink="/reports/builder">
            <i class="fas fa-plus"></i> Create New Report
          </button>
        </div>
      </div>

      <!-- Search and Filter -->
      <div class="row mb-3">
        <div class="col-md-6">
          <div class="input-group">
            <span class="input-group-text"><i class="fas fa-search"></i></span>
            <input type="text" class="form-control" placeholder="Search reports..." [(ngModel)]="searchTerm" (input)="filterReports()">
          </div>
        </div>
        <div class="col-md-3">
          <select class="form-select" [(ngModel)]="selectedCategory" (change)="filterReports()">
            <option value="">All Categories</option>
            <option value="user">My Reports</option>
            <option value="public">Public Reports</option>
            <option value="shared">Shared with Me</option>
          </select>
        </div>
        <div class="col-md-3">
          <select class="form-select" [(ngModel)]="selectedType" (change)="filterReports()">
            <option value="">All Types</option>
            <option value="Table">Table</option>
            <option value="Chart">Chart</option>
            <option value="Dashboard">Dashboard</option>
          </select>
        </div>
      </div>

      <!-- Reports Tabs -->
      <ul class="nav nav-tabs mb-3">
        <li class="nav-item">
          <button class="nav-link" [class.active]="activeTab === 'user'" (click)="setActiveTab('user')">
            My Reports ({{userReports.length}})
          </button>
        </li>
        <li class="nav-item">
          <button class="nav-link" [class.active]="activeTab === 'public'" (click)="setActiveTab('public')">
            Public Reports ({{publicReports.length}})
          </button>
        </li>
        <li class="nav-item">
          <button class="nav-link" [class.active]="activeTab === 'shared'" (click)="setActiveTab('shared')">
            Shared with Me ({{sharedReports.length}})
          </button>
        </li>
      </ul>

      <!-- Loading State -->
      <div *ngIf="loading" class="text-center py-5">
        <div class="spinner-border" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
      </div>

      <!-- Reports Grid -->
      <div *ngIf="!loading" class="row">
        <div class="col-md-4 mb-3" *ngFor="let report of filteredReports">
          <div class="card h-100">
            <div class="card-header d-flex justify-content-between align-items-center">
              <h6 class="mb-0">{{report.name}}</h6>
              <div class="dropdown">
                <button class="btn btn-sm btn-outline-secondary dropdown-toggle" type="button" data-bs-toggle="dropdown">
                  <i class="fas fa-ellipsis-v"></i>
                </button>
                <ul class="dropdown-menu">
                  <li><a class="dropdown-item" href="#" (click)="executeReport(report)">
                    <i class="fas fa-play"></i> Execute
                  </a></li>
                  <li><a class="dropdown-item" href="#" (click)="editReport(report)" *ngIf="canEdit(report)">
                    <i class="fas fa-edit"></i> Edit
                  </a></li>
                  <li><a class="dropdown-item" href="#" (click)="shareReport(report)" *ngIf="canShare(report)">
                    <i class="fas fa-share"></i> Share
                  </a></li>
                  <li><a class="dropdown-item" href="#" (click)="scheduleReport(report)" *ngIf="canSchedule(report)">
                    <i class="fas fa-clock"></i> Schedule
                  </a></li>
                  <li><hr class="dropdown-divider"></li>
                  <li><a class="dropdown-item" href="#" (click)="exportReport(report, 'Excel')">
                    <i class="fas fa-file-excel"></i> Export Excel
                  </a></li>
                  <li><a class="dropdown-item" href="#" (click)="exportReport(report, 'PDF')">
                    <i class="fas fa-file-pdf"></i> Export PDF
                  </a></li>
                  <li><a class="dropdown-item" href="#" (click)="exportReport(report, 'CSV')">
                    <i class="fas fa-file-csv"></i> Export CSV
                  </a></li>
                  <li><hr class="dropdown-divider" *ngIf="canDelete(report)"></li>
                  <li><a class="dropdown-item text-danger" href="#" (click)="deleteReport(report)" *ngIf="canDelete(report)">
                    <i class="fas fa-trash"></i> Delete
                  </a></li>
                </ul>
              </div>
            </div>
            <div class="card-body">
              <p class="card-text">{{report.description || 'No description available'}}</p>
              <div class="d-flex justify-content-between align-items-center">
                <small class="text-muted">
                  <span class="badge bg-secondary">{{report.type}}</span>
                </small>
                <small class="text-muted">
                  {{report.lastExecuted ? ('Last run: ' + (report.lastExecuted | date:'short')) : 'Never executed'}}
                </small>
              </div>
            </div>
            <div class="card-footer">
              <small class="text-muted">
                Created {{report.createdAt | date:'short'}}
                <span *ngIf="report.updatedAt"> • Updated {{report.updatedAt | date:'short'}}</span>
              </small>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div *ngIf="!loading && filteredReports.length === 0" class="text-center py-5">
        <i class="fas fa-chart-bar fa-3x text-muted mb-3"></i>
        <h5>No reports found</h5>
        <p class="text-muted">
          <span *ngIf="searchTerm || selectedCategory || selectedType">
            Try adjusting your search criteria or 
            <a href="#" (click)="clearFilters()">clear filters</a>.
          </span>
          <span *ngIf="!searchTerm && !selectedCategory && !selectedType">
            Get started by creating your first report.
          </span>
        </p>
        <button class="btn btn-primary" routerLink="/reports/builder" *ngIf="!searchTerm && !selectedCategory && !selectedType">
          <i class="fas fa-plus"></i> Create Your First Report
        </button>
      </div>
    </div>

    <!-- Report Execution Modal -->
    <div class="modal fade" id="executionModal" tabindex="-1">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Report Results: {{selectedReport?.name}}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <div *ngIf="executionLoading" class="text-center py-3">
              <div class="spinner-border" role="status">
                <span class="visually-hidden">Executing report...</span>
              </div>
              <p class="mt-2">Executing report...</p>
            </div>
            
            <div *ngIf="executionResult && !executionLoading">
              <div class="d-flex justify-content-between align-items-center mb-3">
                <div>
                  <small class="text-muted">
                    {{executionResult.totalRecords}} records • 
                    Executed in {{executionResult.executionTime}}ms
                  </small>
                </div>
                <div>
                  <button class="btn btn-sm btn-outline-primary me-2" (click)="exportExecutionResult('Excel')">
                    <i class="fas fa-file-excel"></i> Excel
                  </button>
                  <button class="btn btn-sm btn-outline-primary me-2" (click)="exportExecutionResult('PDF')">
                    <i class="fas fa-file-pdf"></i> PDF
                  </button>
                  <button class="btn btn-sm btn-outline-primary" (click)="exportExecutionResult('CSV')">
                    <i class="fas fa-file-csv"></i> CSV
                  </button>
                </div>
              </div>
              
              <div class="table-responsive" style="max-height: 400px;">
                <table class="table table-striped table-hover">
                  <thead class="sticky-top bg-white">
                    <tr>
                      <th *ngFor="let column of getColumns(executionResult.data)">{{column}}</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let row of executionResult.data | slice:0:100">
                      <td *ngFor="let column of getColumns(executionResult.data)">{{row[column]}}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
              
              <div *ngIf="executionResult.totalRecords > 100" class="text-muted text-center mt-2">
                Showing first 100 of {{executionResult.totalRecords}} records
              </div>
            </div>
            
            <div *ngIf="executionError" class="alert alert-danger">
              <h6>Execution Failed</h6>
              <p>{{executionError}}</p>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card {
      transition: transform 0.2s ease-in-out;
    }
    .card:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 8px rgba(0,0,0,0.1);
    }
    .nav-link {
      cursor: pointer;
    }
    .table-responsive {
      border: 1px solid #dee2e6;
      border-radius: 0.375rem;
    }
  `]
})
export class ReportListComponent implements OnInit {
  userReports: Report[] = [];
  publicReports: Report[] = [];
  sharedReports: Report[] = [];
  filteredReports: Report[] = [];
  
  activeTab: string = 'user';
  loading: boolean = true;
  searchTerm: string = '';
  selectedCategory: string = '';
  selectedType: string = '';
  
  selectedReport: Report | null = null;
  executionResult: any = null;
  executionLoading: boolean = false;
  executionError: string = '';

  constructor(private reportService: ReportService) {}

  ngOnInit() {
    this.loadReports();
  }

  async loadReports() {
    try {
      this.loading = true;
      const reports = await this.reportService.getReports();
      this.userReports = reports.userReports;
      this.publicReports = reports.publicReports;
      this.sharedReports = reports.sharedReports;
      this.filterReports();
    } catch (error) {
      console.error('Failed to load reports:', error);
    } finally {
      this.loading = false;
    }
  }

  setActiveTab(tab: string) {
    this.activeTab = tab;
    this.filterReports();
  }

  filterReports() {
    let reports: Report[] = [];
    
    // Select reports based on active tab
    switch (this.activeTab) {
      case 'user':
        reports = this.userReports;
        break;
      case 'public':
        reports = this.publicReports;
        break;
      case 'shared':
        reports = this.sharedReports;
        break;
    }

    // Apply filters
    this.filteredReports = reports.filter(report => {
      const matchesSearch = !this.searchTerm || 
        report.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        report.description?.toLowerCase().includes(this.searchTerm.toLowerCase());
      
      const matchesType = !this.selectedType || report.type === this.selectedType;
      
      return matchesSearch && matchesType;
    });
  }

  clearFilters() {
    this.searchTerm = '';
    this.selectedCategory = '';
    this.selectedType = '';
    this.filterReports();
  }

  canEdit(report: Report): boolean {
    // User can edit their own reports
    return this.activeTab === 'user';
  }

  canShare(report: Report): boolean {
    // User can share their own reports
    return this.activeTab === 'user';
  }

  canSchedule(report: Report): boolean {
    // User can schedule their own reports
    return this.activeTab === 'user';
  }

  canDelete(report: Report): boolean {
    // User can delete their own reports
    return this.activeTab === 'user';
  }

  async executeReport(report: Report) {
    this.selectedReport = report;
    this.executionResult = null;
    this.executionError = '';
    this.executionLoading = true;

    // Show modal
    const modal = new (window as any).bootstrap.Modal(document.getElementById('executionModal'));
    modal.show();

    try {
      this.executionResult = await this.reportService.executeReport(report.id);
    } catch (error: any) {
      this.executionError = error.message || 'Failed to execute report';
    } finally {
      this.executionLoading = false;
    }
  }

  editReport(report: Report) {
    // Navigate to report builder with report ID
    // This would be implemented with router navigation
    console.log('Edit report:', report.id);
  }

  shareReport(report: Report) {
    // Open share dialog
    console.log('Share report:', report.id);
  }

  scheduleReport(report: Report) {
    // Open schedule dialog
    console.log('Schedule report:', report.id);
  }

  async exportReport(report: Report, format: string) {
    try {
      const exportFormat = ReportExportFormat[format as keyof typeof ReportExportFormat];
      const blob = await this.reportService.exportReport(report.id, exportFormat);
      
      // Download the file
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${report.name}_${new Date().getTime()}.${format.toLowerCase()}`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Failed to export report:', error);
      alert('Failed to export report. Please try again.');
    }
  }

  async deleteReport(report: Report) {
    if (!confirm(`Are you sure you want to delete the report "${report.name}"?`)) {
      return;
    }

    try {
      await this.reportService.deleteReport(report.id);
      await this.loadReports(); // Reload the reports
      alert('Report deleted successfully');
    } catch (error) {
      console.error('Failed to delete report:', error);
      alert('Failed to delete report. Please try again.');
    }
  }

  getColumns(data: any[]): string[] {
    if (!data || data.length === 0) return [];
    return Object.keys(data[0]);
  }

  async exportExecutionResult(format: string) {
    if (!this.executionResult || !this.selectedReport) return;

    try {
      const blob = await this.reportService.exportReportData(
        this.executionResult, 
        format.toLowerCase(), 
        this.selectedReport.name
      );
      
      // Download the file
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${this.selectedReport.name}_${new Date().getTime()}.${format.toLowerCase()}`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Failed to export execution result:', error);
      alert('Failed to export data. Please try again.');
    }
  }
}