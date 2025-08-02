import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReportService } from '../services/report.service';
import { ReportBuilderConfiguration, ReportColumn, ReportFilter, ReportChartConfiguration, ReportDataSource } from '../models/report.models';
import { ReportType, ChartType } from '../enums/report.enums';

@Component({
  selector: 'app-report-builder',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div class="container-fluid">
      <div class="row">
        <!-- Sidebar -->
        <div class="col-md-3 bg-light p-3">
          <h5>Report Builder</h5>
          
          <!-- Data Source Selection -->
          <div class="mb-3">
            <label class="form-label">Data Source</label>
            <select class="form-select" [(ngModel)]="selectedDataSource" (change)="onDataSourceChange()">
              <option value="">Select Data Source</option>
              <option *ngFor="let ds of dataSources" [value]="ds.name">{{ds.displayName}}</option>
            </select>
          </div>

          <!-- Available Columns -->
          <div class="mb-3" *ngIf="availableColumns.length > 0">
            <h6>Available Columns</h6>
            <div class="list-group">
              <div *ngFor="let column of availableColumns" 
                   class="list-group-item list-group-item-action"
                   draggable="true"
                   (dragstart)="onDragStart($event, column)">
                <small class="text-muted">{{column.dataType}}</small><br>
                <strong>{{column.displayName}}</strong>
              </div>
            </div>
          </div>

          <!-- Chart Configuration -->
          <div class="mb-3" *ngIf="showChartConfig">
            <h6>Chart Configuration</h6>
            <div class="mb-2">
              <label class="form-label">Chart Type</label>
              <select class="form-select" [(ngModel)]="chartConfig.type">
                <option *ngFor="let type of chartTypes" [value]="type">{{type}}</option>
              </select>
            </div>
            <div class="mb-2">
              <label class="form-label">X-Axis</label>
              <select class="form-select" [(ngModel)]="chartConfig.xAxisColumn">
                <option value="">Select Column</option>
                <option *ngFor="let col of selectedColumns" [value]="col.name">{{col.displayName}}</option>
              </select>
            </div>
            <div class="mb-2">
              <label class="form-label">Y-Axis</label>
              <select class="form-select" [(ngModel)]="chartConfig.yAxisColumn">
                <option value="">Select Column</option>
                <option *ngFor="let col of selectedColumns" [value]="col.name">{{col.displayName}}</option>
              </select>
            </div>
          </div>
        </div>

        <!-- Main Content -->
        <div class="col-md-9">
          <!-- Toolbar -->
          <div class="d-flex justify-content-between align-items-center mb-3">
            <div>
              <button class="btn btn-primary me-2" (click)="previewReport()" [disabled]="!canPreview()">
                <i class="fas fa-eye"></i> Preview
              </button>
              <button class="btn btn-success me-2" (click)="saveReport()" [disabled]="!canSave()">
                <i class="fas fa-save"></i> Save
              </button>
              <button class="btn btn-info me-2" (click)="toggleChartConfig()">
                <i class="fas fa-chart-bar"></i> Add Chart
              </button>
            </div>
            <div>
              <button class="btn btn-outline-secondary me-2" (click)="exportReport('excel')">
                <i class="fas fa-file-excel"></i> Excel
              </button>
              <button class="btn btn-outline-secondary me-2" (click)="exportReport('pdf')">
                <i class="fas fa-file-pdf"></i> PDF
              </button>
              <button class="btn btn-outline-secondary" (click)="exportReport('csv')">
                <i class="fas fa-file-csv"></i> CSV
              </button>
            </div>
          </div>

          <!-- Report Configuration -->
          <div class="row">
            <!-- Selected Columns -->
            <div class="col-md-6">
              <div class="card">
                <div class="card-header">
                  <h6 class="mb-0">Selected Columns</h6>
                </div>
                <div class="card-body" 
                     (drop)="onDrop($event, 'columns')" 
                     (dragover)="onDragOver($event)"
                     style="min-height: 200px;">
                  <div *ngFor="let column of selectedColumns; let i = index" 
                       class="d-flex justify-content-between align-items-center mb-2 p-2 border rounded">
                    <div>
                      <strong>{{column.displayName}}</strong>
                      <small class="text-muted d-block">{{column.dataType}}</small>
                    </div>
                    <div>
                      <button class="btn btn-sm btn-outline-secondary me-1" (click)="moveColumn(i, -1)" [disabled]="i === 0">
                        <i class="fas fa-arrow-up"></i>
                      </button>
                      <button class="btn btn-sm btn-outline-secondary me-1" (click)="moveColumn(i, 1)" [disabled]="i === selectedColumns.length - 1">
                        <i class="fas fa-arrow-down"></i>
                      </button>
                      <button class="btn btn-sm btn-outline-danger" (click)="removeColumn(i)">
                        <i class="fas fa-times"></i>
                      </button>
                    </div>
                  </div>
                  <div *ngIf="selectedColumns.length === 0" class="text-center text-muted">
                    Drag columns here to add them to your report
                  </div>
                </div>
              </div>
            </div>

            <!-- Filters -->
            <div class="col-md-6">
              <div class="card">
                <div class="card-header d-flex justify-content-between align-items-center">
                  <h6 class="mb-0">Filters</h6>
                  <button class="btn btn-sm btn-outline-primary" (click)="addFilter()">
                    <i class="fas fa-plus"></i> Add Filter
                  </button>
                </div>
                <div class="card-body" style="min-height: 200px;">
                  <div *ngFor="let filter of filters; let i = index" class="mb-3 p-2 border rounded">
                    <div class="row">
                      <div class="col-md-4">
                        <select class="form-select form-select-sm" [(ngModel)]="filter.column">
                          <option value="">Select Column</option>
                          <option *ngFor="let col of availableColumns" [value]="col.name">{{col.displayName}}</option>
                        </select>
                      </div>
                      <div class="col-md-3">
                        <select class="form-select form-select-sm" [(ngModel)]="filter.operator">
                          <option value="=">=</option>
                          <option value="!=">!=</option>
                          <option value=">">></option>
                          <option value="<"><</option>
                          <option value=">=">>=</option>
                          <option value="<="><=</option>
                          <option value="LIKE">Contains</option>
                        </select>
                      </div>
                      <div class="col-md-4">
                        <input type="text" class="form-control form-control-sm" [(ngModel)]="filter.value" placeholder="Value">
                      </div>
                      <div class="col-md-1">
                        <button class="btn btn-sm btn-outline-danger" (click)="removeFilter(i)">
                          <i class="fas fa-times"></i>
                        </button>
                      </div>
                    </div>
                  </div>
                  <div *ngIf="filters.length === 0" class="text-center text-muted">
                    No filters applied
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Preview Area -->
          <div class="card mt-3" *ngIf="previewData">
            <div class="card-header">
              <h6 class="mb-0">Preview</h6>
            </div>
            <div class="card-body">
              <!-- Chart Preview -->
              <div *ngIf="showChartConfig && chartData" class="mb-3">
                <canvas #chartCanvas></canvas>
              </div>

              <!-- Table Preview -->
              <div class="table-responsive">
                <table class="table table-striped table-hover">
                  <thead>
                    <tr>
                      <th *ngFor="let column of selectedColumns">{{column.displayName}}</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let row of previewData.data | slice:0:10">
                      <td *ngFor="let column of selectedColumns">{{row[column.name]}}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <div class="text-muted">
                Showing {{Math.min(10, previewData.totalRecords)}} of {{previewData.totalRecords}} records
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Save Report Modal -->
    <div class="modal fade" id="saveReportModal" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Save Report</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <form [formGroup]="saveReportForm" (ngSubmit)="onSaveReport()">
            <div class="modal-body">
              <div class="mb-3">
                <label class="form-label">Report Name</label>
                <input type="text" class="form-control" formControlName="name" required>
              </div>
              <div class="mb-3">
                <label class="form-label">Description</label>
                <textarea class="form-control" formControlName="description" rows="3"></textarea>
              </div>
              <div class="mb-3">
                <label class="form-label">Report Type</label>
                <select class="form-select" formControlName="type">
                  <option value="Table">Table</option>
                  <option value="Chart">Chart</option>
                  <option value="Dashboard">Dashboard</option>
                </select>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
              <button type="submit" class="btn btn-primary" [disabled]="!saveReportForm.valid">Save Report</button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .list-group-item {
      cursor: grab;
    }
    .list-group-item:active {
      cursor: grabbing;
    }
    .card-body[style*="min-height"] {
      border: 2px dashed #dee2e6;
    }
    .card-body[style*="min-height"]:hover {
      border-color: #0d6efd;
      background-color: #f8f9fa;
    }
  `]
})
export class ReportBuilderComponent implements OnInit {
  dataSources: ReportDataSource[] = [];
  selectedDataSource: string = '';
  availableColumns: any[] = [];
  selectedColumns: ReportColumn[] = [];
  filters: ReportFilter[] = [];
  previewData: any = null;
  chartData: any = null;
  showChartConfig: boolean = false;
  chartTypes: string[] = ['Bar', 'Line', 'Pie', 'Doughnut', 'Area'];
  
  chartConfig: ReportChartConfiguration = {
    type: ChartType.Bar,
    title: '',
    xAxisColumn: '',
    yAxisColumn: '',
    seriesColumn: '',
    options: {},
    colors: ['#3b82f6', '#ef4444', '#10b981', '#f59e0b', '#8b5cf6']
  };

  saveReportForm: FormGroup;
  Math = Math;

  constructor(
    private reportService: ReportService,
    private fb: FormBuilder
  ) {
    this.saveReportForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      type: [ReportType.Table, Validators.required]
    });
  }

  ngOnInit() {
    this.loadDataSources();
  }

  async loadDataSources() {
    try {
      this.dataSources = await this.reportService.getDataSources();
    } catch (error) {
      console.error('Failed to load data sources:', error);
    }
  }

  async onDataSourceChange() {
    if (!this.selectedDataSource) {
      this.availableColumns = [];
      return;
    }

    try {
      const schema = await this.reportService.getDataSourceSchema(this.selectedDataSource);
      this.availableColumns = schema.columns;
    } catch (error) {
      console.error('Failed to load data source schema:', error);
    }
  }

  onDragStart(event: DragEvent, column: any) {
    event.dataTransfer?.setData('text/json', JSON.stringify(column));
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
  }

  onDrop(event: DragEvent, target: string) {
    event.preventDefault();
    const columnData = event.dataTransfer?.getData('text/json');
    if (!columnData) return;

    const column = JSON.parse(columnData);
    
    if (target === 'columns') {
      if (!this.selectedColumns.find(c => c.name === column.name)) {
        this.selectedColumns.push({
          name: column.name,
          displayName: column.displayName,
          dataType: column.dataType,
          isVisible: true,
          order: this.selectedColumns.length
        });
      }
    }
  }

  moveColumn(index: number, direction: number) {
    const newIndex = index + direction;
    if (newIndex >= 0 && newIndex < this.selectedColumns.length) {
      const temp = this.selectedColumns[index];
      this.selectedColumns[index] = this.selectedColumns[newIndex];
      this.selectedColumns[newIndex] = temp;
      
      // Update order
      this.selectedColumns.forEach((col, i) => col.order = i);
    }
  }

  removeColumn(index: number) {
    this.selectedColumns.splice(index, 1);
    this.selectedColumns.forEach((col, i) => col.order = i);
  }

  addFilter() {
    this.filters.push({
      column: '',
      operator: '=',
      value: '',
      logicalOperator: 'AND',
      order: this.filters.length
    });
  }

  removeFilter(index: number) {
    this.filters.splice(index, 1);
    this.filters.forEach((filter, i) => filter.order = i);
  }

  toggleChartConfig() {
    this.showChartConfig = !this.showChartConfig;
  }

  canPreview(): boolean {
    return this.selectedDataSource !== '' && this.selectedColumns.length > 0;
  }

  canSave(): boolean {
    return this.canPreview();
  }

  async previewReport() {
    if (!this.canPreview()) return;

    const configuration: ReportBuilderConfiguration = {
      dataSource: this.selectedDataSource,
      columns: this.selectedColumns,
      filters: this.filters.filter(f => f.column && f.value),
      groupings: [],
      sortings: [],
      chartConfiguration: this.showChartConfig ? this.chartConfig : undefined,
      pagination: { pageSize: 100, enablePaging: true }
    };

    try {
      this.previewData = await this.reportService.previewReport(configuration);
      
      if (this.showChartConfig && this.chartConfig.xAxisColumn && this.chartConfig.yAxisColumn) {
        this.chartData = await this.reportService.generateChartData(this.previewData, this.chartConfig);
        // Here you would render the chart using Chart.js or similar library
      }
    } catch (error) {
      console.error('Failed to preview report:', error);
    }
  }

  saveReport() {
    // Show the save modal
    const modal = new (window as any).bootstrap.Modal(document.getElementById('saveReportModal'));
    modal.show();
  }

  async onSaveReport() {
    if (!this.saveReportForm.valid || !this.canSave()) return;

    const configuration: ReportBuilderConfiguration = {
      dataSource: this.selectedDataSource,
      columns: this.selectedColumns,
      filters: this.filters.filter(f => f.column && f.value),
      groupings: [],
      sortings: [],
      chartConfiguration: this.showChartConfig ? this.chartConfig : undefined,
      pagination: { pageSize: 50, enablePaging: true }
    };

    const formValue = this.saveReportForm.value;

    try {
      await this.reportService.createReport({
        name: formValue.name,
        description: formValue.description,
        type: formValue.type,
        configuration: configuration
      });

      // Close modal and show success message
      const modal = (window as any).bootstrap.Modal.getInstance(document.getElementById('saveReportModal'));
      modal.hide();
      
      // Reset form
      this.saveReportForm.reset();
      
      alert('Report saved successfully!');
    } catch (error) {
      console.error('Failed to save report:', error);
      alert('Failed to save report. Please try again.');
    }
  }

  async exportReport(format: string) {
    if (!this.canPreview()) return;

    const configuration: ReportBuilderConfiguration = {
      dataSource: this.selectedDataSource,
      columns: this.selectedColumns,
      filters: this.filters.filter(f => f.column && f.value),
      groupings: [],
      sortings: [],
      chartConfiguration: this.showChartConfig ? this.chartConfig : undefined,
      pagination: { pageSize: 1000, enablePaging: true }
    };

    try {
      // First create a temporary report or use preview data
      const reportData = await this.reportService.previewReport(configuration);
      
      // Then export the data
      const blob = await this.reportService.exportReportData(reportData, format, 'Custom Report');
      
      // Download the file
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `report_${new Date().getTime()}.${format}`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Failed to export report:', error);
      alert('Failed to export report. Please try again.');
    }
  }
}