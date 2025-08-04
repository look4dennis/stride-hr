import { Component, OnInit, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Chart, ChartConfiguration, ChartType, registerables } from 'chart.js';
import { ReportService } from '../../../services/report.service';
import { 
  ReportBuilderConfiguration, 
  ReportColumn, 
  ReportFilter, 
  ReportChartConfiguration, 
  ReportDataSource,
  ReportExecutionResult,
  CreateReportRequest
} from '../../../models/report.models';
import { ReportType, ChartType as AppChartType, ReportExportFormat } from '../../../enums/report.enums';

Chart.register(...registerables);

interface ReportTemplate {
  id: string;
  name: string;
  description: string;
  category: string;
  configuration: ReportBuilderConfiguration;
  preview: string;
}

@Component({
  selector: 'app-report-builder',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div class="report-builder">
      <!-- Header -->
      <div class="builder-header">
        <div class="d-flex justify-content-between align-items-center mb-4">
          <div>
            <h1 class="builder-title">
              <i class="fas fa-tools me-2"></i>
              Report Builder
            </h1>
            <p class="text-muted">Create custom reports with drag-and-drop interface</p>
          </div>
          <div class="builder-actions">
            <button class="btn btn-outline-secondary me-2" (click)="loadTemplate()">
              <i class="fas fa-file-import"></i> Load Template
            </button>
            <button class="btn btn-outline-secondary me-2" (click)="saveAsTemplate()">
              <i class="fas fa-save"></i> Save as Template
            </button>
            <button class="btn btn-success" (click)="saveReport()" [disabled]="!canSave()">
              <i class="fas fa-check"></i> Save Report
            </button>
          </div>
        </div>

        <!-- Progress Steps -->
        <div class="progress-steps mb-4">
          <div class="step" [class.active]="currentStep >= 1" [class.completed]="currentStep > 1">
            <div class="step-number">1</div>
            <div class="step-label">Data Source</div>
          </div>
          <div class="step" [class.active]="currentStep >= 2" [class.completed]="currentStep > 2">
            <div class="step-number">2</div>
            <div class="step-label">Columns & Filters</div>
          </div>
          <div class="step" [class.active]="currentStep >= 3" [class.completed]="currentStep > 3">
            <div class="step-number">3</div>
            <div class="step-label">Visualization</div>
          </div>
          <div class="step" [class.active]="currentStep >= 4" [class.completed]="currentStep > 4">
            <div class="step-number">4</div>
            <div class="step-label">Preview & Save</div>
          </div>
        </div>
      </div>

      <div class="row">
        <!-- Sidebar -->
        <div class="col-md-3">
          <div class="builder-sidebar">
            <!-- Step 1: Data Source Selection -->
            <div class="sidebar-section" *ngIf="currentStep === 1">
              <h5 class="section-title">
                <i class="fas fa-database me-2"></i>
                Select Data Source
              </h5>
              
              <div class="data-source-list">
                <div class="data-source-item" 
                     *ngFor="let ds of dataSources"
                     [class.selected]="selectedDataSource === ds.name"
                     (click)="selectDataSource(ds.name)">
                  <div class="ds-icon">
                    <i [class]="getDataSourceIcon(ds.name)"></i>
                  </div>
                  <div class="ds-info">
                    <div class="ds-name">{{ds.displayName}}</div>
                    <div class="ds-description">{{ds.description}}</div>
                    <div class="ds-columns">{{ds.columns.length}} columns</div>
                  </div>
                </div>
              </div>

              <div class="step-actions mt-3">
                <button class="btn btn-primary w-100" 
                        [disabled]="!selectedDataSource"
                        (click)="nextStep()">
                  Next: Configure Columns
                </button>
              </div>
            </div>

            <!-- Step 2: Columns and Filters -->
            <div class="sidebar-section" *ngIf="currentStep === 2">
              <h5 class="section-title">
                <i class="fas fa-columns me-2"></i>
                Available Columns
              </h5>
              
              <div class="search-box mb-3">
                <input type="text" 
                       class="form-control form-control-sm" 
                       placeholder="Search columns..."
                       [(ngModel)]="columnSearchTerm"
                       (input)="filterColumns()">
              </div>

              <div class="columns-list">
                <div class="column-item" 
                     *ngFor="let column of filteredColumns"
                     draggable="true"
                     (dragstart)="onDragStart($event, column)">
                  <div class="column-type">
                    <i [class]="getColumnTypeIcon(column.dataType)"></i>
                  </div>
                  <div class="column-info">
                    <div class="column-name">{{column.displayName}}</div>
                    <div class="column-type-text">{{column.dataType}}</div>
                  </div>
                  <div class="column-actions">
                    <button class="btn btn-sm btn-outline-primary" 
                            (click)="addColumn(column)"
                            [disabled]="isColumnSelected(column.name)">
                      <i class="fas fa-plus"></i>
                    </button>
                  </div>
                </div>
              </div>

              <div class="step-actions mt-3">
                <button class="btn btn-outline-secondary me-2" (click)="previousStep()">
                  <i class="fas fa-arrow-left"></i> Back
                </button>
                <button class="btn btn-primary" 
                        [disabled]="selectedColumns.length === 0"
                        (click)="nextStep()">
                  Next: Visualization
                </button>
              </div>
            </div>

            <!-- Step 3: Visualization -->
            <div class="sidebar-section" *ngIf="currentStep === 3">
              <h5 class="section-title">
                <i class="fas fa-chart-bar me-2"></i>
                Visualization Options
              </h5>

              <div class="visualization-types">
                <div class="viz-type" 
                     *ngFor="let type of visualizationTypes"
                     [class.selected]="selectedVisualization === type.type"
                     (click)="selectVisualization(type.type)">
                  <div class="viz-icon">
                    <i [class]="type.icon"></i>
                  </div>
                  <div class="viz-name">{{type.name}}</div>
                </div>
              </div>

              <!-- Chart Configuration -->
              <div class="chart-config mt-4" *ngIf="selectedVisualization !== 'table'">
                <h6>Chart Configuration</h6>
                
                <div class="mb-3">
                  <label class="form-label">Chart Type</label>
                  <select class="form-select form-select-sm" [(ngModel)]="chartConfig.type">
                    <option *ngFor="let type of chartTypes" [value]="type">{{type}}</option>
                  </select>
                </div>

                <div class="mb-3">
                  <label class="form-label">X-Axis</label>
                  <select class="form-select form-select-sm" [(ngModel)]="chartConfig.xAxisColumn">
                    <option value="">Select Column</option>
                    <option *ngFor="let col of selectedColumns" [value]="col.name">{{col.displayName}}</option>
                  </select>
                </div>

                <div class="mb-3">
                  <label class="form-label">Y-Axis</label>
                  <select class="form-select form-select-sm" [(ngModel)]="chartConfig.yAxisColumn">
                    <option value="">Select Column</option>
                    <option *ngFor="let col of selectedColumns" [value]="col.name">{{col.displayName}}</option>
                  </select>
                </div>

                <div class="mb-3" *ngIf="chartConfig.type === 'Bar' || chartConfig.type === 'Line'">
                  <label class="form-label">Series (Optional)</label>
                  <select class="form-select form-select-sm" [(ngModel)]="chartConfig.seriesColumn">
                    <option value="">None</option>
                    <option *ngFor="let col of selectedColumns" [value]="col.name">{{col.displayName}}</option>
                  </select>
                </div>

                <div class="mb-3">
                  <label class="form-label">Chart Title</label>
                  <input type="text" 
                         class="form-control form-control-sm" 
                         [(ngModel)]="chartConfig.title"
                         placeholder="Enter chart title">
                </div>
              </div>

              <div class="step-actions mt-3">
                <button class="btn btn-outline-secondary me-2" (click)="previousStep()">
                  <i class="fas fa-arrow-left"></i> Back
                </button>
                <button class="btn btn-primary" (click)="nextStep()">
                  Next: Preview
                </button>
              </div>
            </div>

            <!-- Step 4: Preview & Save -->
            <div class="sidebar-section" *ngIf="currentStep === 4">
              <h5 class="section-title">
                <i class="fas fa-eye me-2"></i>
                Preview & Save
              </h5>

              <div class="preview-actions">
                <button class="btn btn-outline-primary w-100 mb-2" 
                        (click)="previewReport()"
                        [disabled]="!canPreview()">
                  <i class="fas fa-eye"></i> Generate Preview
                </button>

                <div class="export-options">
                  <h6>Export Options</h6>
                  <div class="btn-group w-100 mb-2">
                    <button class="btn btn-outline-secondary btn-sm" 
                            (click)="exportReport('PDF')">
                      <i class="fas fa-file-pdf"></i> PDF
                    </button>
                    <button class="btn btn-outline-secondary btn-sm" 
                            (click)="exportReport('Excel')">
                      <i class="fas fa-file-excel"></i> Excel
                    </button>
                    <button class="btn btn-outline-secondary btn-sm" 
                            (click)="exportReport('CSV')">
                      <i class="fas fa-file-csv"></i> CSV
                    </button>
                  </div>
                </div>

                <div class="schedule-options">
                  <h6>Schedule Report</h6>
                  <button class="btn btn-outline-info w-100 mb-2" (click)="scheduleReport()">
                    <i class="fas fa-clock"></i> Schedule Delivery
                  </button>
                </div>
              </div>

              <div class="step-actions mt-3">
                <button class="btn btn-outline-secondary me-2" (click)="previousStep()">
                  <i class="fas fa-arrow-left"></i> Back
                </button>
                <button class="btn btn-success" 
                        (click)="saveReport()"
                        [disabled]="!canSave()">
                  <i class="fas fa-save"></i> Save Report
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Main Content Area -->
        <div class="col-md-9">
          <div class="builder-content">
            <!-- Step 2: Column Configuration -->
            <div *ngIf="currentStep === 2" class="configuration-area">
              <div class="row">
                <!-- Selected Columns -->
                <div class="col-md-6">
                  <div class="config-card">
                    <div class="config-header">
                      <h6>Selected Columns</h6>
                      <span class="badge bg-primary">{{selectedColumns.length}}</span>
                    </div>
                    <div class="config-body drop-zone" 
                         (drop)="onDrop($event, 'columns')" 
                         (dragover)="onDragOver($event)">
                      <div class="selected-column" 
                           *ngFor="let column of selectedColumns; let i = index">
                        <div class="column-header">
                          <div class="column-info">
                            <i [class]="getColumnTypeIcon(column.dataType)"></i>
                            <span class="column-name">{{column.displayName}}</span>
                            <span class="column-type">{{column.dataType}}</span>
                          </div>
                          <div class="column-controls">
                            <button class="btn btn-sm btn-outline-secondary" 
                                    (click)="moveColumn(i, -1)" 
                                    [disabled]="i === 0">
                              <i class="fas fa-arrow-up"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-secondary" 
                                    (click)="moveColumn(i, 1)" 
                                    [disabled]="i === selectedColumns.length - 1">
                              <i class="fas fa-arrow-down"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-danger" 
                                    (click)="removeColumn(i)">
                              <i class="fas fa-times"></i>
                            </button>
                          </div>
                        </div>
                        <div class="column-options">
                          <div class="row">
                            <div class="col-md-6">
                              <label class="form-label">Display Name</label>
                              <input type="text" 
                                     class="form-control form-control-sm" 
                                     [(ngModel)]="column.displayName">
                            </div>
                            <div class="col-md-6">
                              <label class="form-label">Format</label>
                              <select class="form-select form-select-sm" [(ngModel)]="column.format">
                                <option value="">Default</option>
                                <option value="currency">Currency</option>
                                <option value="percentage">Percentage</option>
                                <option value="date">Date</option>
                                <option value="datetime">Date Time</option>
                              </select>
                            </div>
                          </div>
                          <div class="row mt-2">
                            <div class="col-md-6">
                              <label class="form-label">Aggregate</label>
                              <select class="form-select form-select-sm" [(ngModel)]="column.aggregateFunction">
                                <option value="">None</option>
                                <option value="SUM">Sum</option>
                                <option value="AVG">Average</option>
                                <option value="COUNT">Count</option>
                                <option value="MIN">Minimum</option>
                                <option value="MAX">Maximum</option>
                              </select>
                            </div>
                            <div class="col-md-6">
                              <label class="form-label">Width</label>
                              <input type="number" 
                                     class="form-control form-control-sm" 
                                     [(ngModel)]="column.width"
                                     placeholder="Auto">
                            </div>
                          </div>
                        </div>
                      </div>
                      <div class="drop-placeholder" *ngIf="selectedColumns.length === 0">
                        <i class="fas fa-columns"></i>
                        <p>Drag columns here or click the + button to add them</p>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- Filters -->
                <div class="col-md-6">
                  <div class="config-card">
                    <div class="config-header">
                      <h6>Filters</h6>
                      <button class="btn btn-sm btn-outline-primary" (click)="addFilter()">
                        <i class="fas fa-plus"></i> Add Filter
                      </button>
                    </div>
                    <div class="config-body">
                      <div class="filter-item" *ngFor="let filter of filters; let i = index">
                        <div class="filter-header">
                          <span class="filter-number">{{i + 1}}</span>
                          <button class="btn btn-sm btn-outline-danger" (click)="removeFilter(i)">
                            <i class="fas fa-times"></i>
                          </button>
                        </div>
                        <div class="filter-config">
                          <div class="mb-2">
                            <label class="form-label">Column</label>
                            <select class="form-select form-select-sm" [(ngModel)]="filter.column">
                              <option value="">Select Column</option>
                              <option *ngFor="let col of availableColumns" [value]="col.name">
                                {{col.displayName}}
                              </option>
                            </select>
                          </div>
                          <div class="mb-2">
                            <label class="form-label">Operator</label>
                            <select class="form-select form-select-sm" [(ngModel)]="filter.operator">
                              <option value="=">=</option>
                              <option value="!=">!=</option>
                              <option value=">">></option>
                              <option value="<"><</option>
                              <option value=">=">>=</option>
                              <option value="<="><=</option>
                              <option value="LIKE">Contains</option>
                              <option value="NOT LIKE">Does not contain</option>
                              <option value="IN">In list</option>
                              <option value="NOT IN">Not in list</option>
                            </select>
                          </div>
                          <div class="mb-2">
                            <label class="form-label">Value</label>
                            <input type="text" 
                                   class="form-control form-control-sm" 
                                   [(ngModel)]="filter.value" 
                                   placeholder="Enter value">
                          </div>
                          <div class="mb-2" *ngIf="i > 0">
                            <label class="form-label">Logic</label>
                            <select class="form-select form-select-sm" [(ngModel)]="filter.logicalOperator">
                              <option value="AND">AND</option>
                              <option value="OR">OR</option>
                            </select>
                          </div>
                        </div>
                      </div>
                      <div class="no-filters" *ngIf="filters.length === 0">
                        <i class="fas fa-filter"></i>
                        <p>No filters applied</p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Step 3: Visualization Preview -->
            <div *ngIf="currentStep === 3" class="visualization-preview">
              <div class="preview-card">
                <div class="preview-header">
                  <h6>Visualization Preview</h6>
                  <div class="preview-controls">
                    <button class="btn btn-sm btn-outline-secondary" (click)="refreshPreview()">
                      <i class="fas fa-sync-alt"></i> Refresh
                    </button>
                  </div>
                </div>
                <div class="preview-body">
                  <div *ngIf="selectedVisualization === 'table'" class="table-preview">
                    <div class="table-responsive">
                      <table class="table table-striped table-hover">
                        <thead>
                          <tr>
                            <th *ngFor="let column of selectedColumns">{{column.displayName}}</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr *ngFor="let row of sampleData">
                            <td *ngFor="let column of selectedColumns">
                              {{formatCellValue(row[column.name], column.format)}}
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                  </div>
                  <div *ngIf="selectedVisualization === 'chart'" class="chart-preview">
                    <canvas #previewChart></canvas>
                  </div>
                </div>
              </div>
            </div>

            <!-- Step 4: Final Preview -->
            <div *ngIf="currentStep === 4" class="final-preview">
              <div class="preview-card">
                <div class="preview-header">
                  <h6>Report Preview</h6>
                  <div class="preview-info">
                    <span class="badge bg-info me-2">{{selectedColumns.length}} columns</span>
                    <span class="badge bg-secondary me-2">{{filters.length}} filters</span>
                    <span class="badge bg-success">{{selectedVisualization}}</span>
                  </div>
                </div>
                <div class="preview-body">
                  <div class="loading-state" *ngIf="isLoadingPreview">
                    <div class="spinner-border text-primary"></div>
                    <p>Generating preview...</p>
                  </div>
                  
                  <div *ngIf="previewData && !isLoadingPreview">
                    <!-- Chart Preview -->
                    <div *ngIf="selectedVisualization === 'chart' && chartData" class="mb-4">
                      <canvas #finalChart></canvas>
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
                            <td *ngFor="let column of selectedColumns">
                              {{formatCellValue(row[column.name], column.format)}}
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                    
                    <div class="preview-footer">
                      <small class="text-muted">
                        Showing {{Math.min(10, previewData.totalRecords)}} of {{previewData.totalRecords}} records
                        ({{previewData.executionTime.toFixed(2)}}ms)
                      </small>
                    </div>
                  </div>
                </div>
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
                <label class="form-label">Report Name *</label>
                <input type="text" class="form-control" formControlName="name" required>
                <div class="form-text">Choose a descriptive name for your report</div>
              </div>
              <div class="mb-3">
                <label class="form-label">Description</label>
                <textarea class="form-control" formControlName="description" rows="3" 
                          placeholder="Describe what this report shows..."></textarea>
              </div>
              <div class="mb-3">
                <label class="form-label">Report Type</label>
                <select class="form-select" formControlName="type">
                  <option value="Table">Table Report</option>
                  <option value="Chart">Chart Report</option>
                  <option value="Dashboard">Dashboard</option>
                  <option value="Analytical">Analytical Report</option>
                </select>
              </div>
              <div class="mb-3">
                <div class="form-check">
                  <input class="form-check-input" type="checkbox" formControlName="isPublic" id="isPublic">
                  <label class="form-check-label" for="isPublic">
                    Make this report public (visible to all users)
                  </label>
                </div>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
              <button type="submit" class="btn btn-primary" [disabled]="!saveReportForm.valid">
                <i class="fas fa-save"></i> Save Report
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Templates Modal -->
    <div class="modal fade" id="templatesModal" tabindex="-1">
      <div class="modal-dialog modal-lg">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Report Templates</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body">
            <div class="row">
              <div class="col-md-4" *ngFor="let template of reportTemplates">
                <div class="template-card" (click)="loadReportTemplate(template)">
                  <div class="template-preview">
                    <img [src]="template.preview" [alt]="template.name" class="img-fluid">
                  </div>
                  <div class="template-info">
                    <h6>{{template.name}}</h6>
                    <p class="text-muted">{{template.description}}</p>
                    <span class="badge bg-secondary">{{template.category}}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .report-builder {
      padding: 1.5rem;
      background-color: #f8f9fa;
      min-height: 100vh;
    }

    .builder-title {
      font-size: 2rem;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    .progress-steps {
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 2rem;
    }

    .step {
      display: flex;
      flex-direction: column;
      align-items: center;
      position: relative;
    }

    .step::after {
      content: '';
      position: absolute;
      top: 20px;
      left: 100%;
      width: 2rem;
      height: 2px;
      background-color: #dee2e6;
      z-index: 1;
    }

    .step:last-child::after {
      display: none;
    }

    .step.completed::after {
      background-color: #10b981;
    }

    .step-number {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      background-color: #dee2e6;
      color: #6c757d;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 600;
      margin-bottom: 0.5rem;
      position: relative;
      z-index: 2;
    }

    .step.active .step-number {
      background-color: #3b82f6;
      color: white;
    }

    .step.completed .step-number {
      background-color: #10b981;
      color: white;
    }

    .step-label {
      font-size: 0.875rem;
      color: #6c757d;
      text-align: center;
    }

    .step.active .step-label {
      color: #3b82f6;
      font-weight: 600;
    }

    .builder-sidebar {
      background: white;
      border-radius: 12px;
      padding: 1.5rem;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      border: 1px solid #e9ecef;
      height: fit-content;
      position: sticky;
      top: 1rem;
    }

    .section-title {
      font-size: 1.125rem;
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 1rem;
      padding-bottom: 0.5rem;
      border-bottom: 2px solid #e9ecef;
    }

    .data-source-list {
      max-height: 400px;
      overflow-y: auto;
    }

    .data-source-item {
      display: flex;
      align-items: center;
      padding: 1rem;
      border: 1px solid #e9ecef;
      border-radius: 8px;
      margin-bottom: 0.75rem;
      cursor: pointer;
      transition: all 0.2s ease;
    }

    .data-source-item:hover {
      border-color: #3b82f6;
      box-shadow: 0 2px 8px rgba(59, 130, 246, 0.1);
    }

    .data-source-item.selected {
      border-color: #3b82f6;
      background-color: rgba(59, 130, 246, 0.05);
    }

    .ds-icon {
      width: 40px;
      height: 40px;
      border-radius: 8px;
      background-color: #3b82f6;
      color: white;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-right: 0.75rem;
      font-size: 1.125rem;
    }

    .ds-name {
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 0.25rem;
    }

    .ds-description {
      font-size: 0.875rem;
      color: var(--text-secondary);
      margin-bottom: 0.25rem;
    }

    .ds-columns {
      font-size: 0.75rem;
      color: var(--text-muted);
    }

    .columns-list {
      max-height: 300px;
      overflow-y: auto;
    }

    .column-item {
      display: flex;
      align-items: center;
      padding: 0.75rem;
      border: 1px solid #e9ecef;
      border-radius: 6px;
      margin-bottom: 0.5rem;
      cursor: grab;
      transition: all 0.2s ease;
    }

    .column-item:hover {
      border-color: #3b82f6;
      background-color: rgba(59, 130, 246, 0.05);
    }

    .column-item:active {
      cursor: grabbing;
    }

    .column-type {
      width: 24px;
      height: 24px;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-right: 0.5rem;
      color: #6c757d;
    }

    .column-info {
      flex-grow: 1;
    }

    .column-name {
      font-weight: 500;
      color: var(--text-primary);
      font-size: 0.875rem;
    }

    .column-type-text {
      font-size: 0.75rem;
      color: var(--text-secondary);
    }

    .visualization-types {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 0.75rem;
      margin-bottom: 1rem;
    }

    .viz-type {
      padding: 1rem;
      border: 1px solid #e9ecef;
      border-radius: 8px;
      text-align: center;
      cursor: pointer;
      transition: all 0.2s ease;
    }

    .viz-type:hover {
      border-color: #3b82f6;
      background-color: rgba(59, 130, 246, 0.05);
    }

    .viz-type.selected {
      border-color: #3b82f6;
      background-color: rgba(59, 130, 246, 0.1);
    }

    .viz-icon {
      font-size: 1.5rem;
      color: #3b82f6;
      margin-bottom: 0.5rem;
    }

    .viz-name {
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--text-primary);
    }

    .builder-content {
      background: white;
      border-radius: 12px;
      padding: 1.5rem;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      border: 1px solid #e9ecef;
      min-height: 600px;
    }

    .config-card {
      border: 1px solid #e9ecef;
      border-radius: 8px;
      margin-bottom: 1rem;
    }

    .config-header {
      padding: 1rem 1.25rem;
      background-color: #f8f9fa;
      border-bottom: 1px solid #e9ecef;
      display: flex;
      justify-content: between;
      align-items: center;
    }

    .config-header h6 {
      margin: 0;
      font-weight: 600;
      color: var(--text-primary);
      flex-grow: 1;
    }

    .config-body {
      padding: 1.25rem;
      min-height: 300px;
    }

    .drop-zone {
      border: 2px dashed #dee2e6;
      border-radius: 8px;
      transition: all 0.2s ease;
    }

    .drop-zone:hover,
    .drop-zone.drag-over {
      border-color: #3b82f6;
      background-color: rgba(59, 130, 246, 0.05);
    }

    .selected-column {
      border: 1px solid #e9ecef;
      border-radius: 8px;
      padding: 1rem;
      margin-bottom: 1rem;
      background-color: #f8f9fa;
    }

    .column-header {
      display: flex;
      justify-content: between;
      align-items: center;
      margin-bottom: 1rem;
    }

    .column-header .column-info {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .column-header .column-name {
      font-weight: 600;
      color: var(--text-primary);
    }

    .column-header .column-type {
      font-size: 0.75rem;
      color: var(--text-secondary);
      background-color: #e9ecef;
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
    }

    .column-controls {
      display: flex;
      gap: 0.25rem;
    }

    .drop-placeholder,
    .no-filters {
      text-align: center;
      padding: 2rem;
      color: var(--text-muted);
    }

    .drop-placeholder i,
    .no-filters i {
      font-size: 2rem;
      margin-bottom: 1rem;
      display: block;
    }

    .filter-item {
      border: 1px solid #e9ecef;
      border-radius: 8px;
      padding: 1rem;
      margin-bottom: 1rem;
      background-color: #f8f9fa;
    }

    .filter-header {
      display: flex;
      justify-content: between;
      align-items: center;
      margin-bottom: 1rem;
    }

    .filter-number {
      width: 24px;
      height: 24px;
      border-radius: 50%;
      background-color: #3b82f6;
      color: white;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.75rem;
      font-weight: 600;
    }

    .preview-card {
      border: 1px solid #e9ecef;
      border-radius: 12px;
      overflow: hidden;
    }

    .preview-header {
      padding: 1.25rem 1.5rem;
      background-color: #f8f9fa;
      border-bottom: 1px solid #e9ecef;
      display: flex;
      justify-content: between;
      align-items: center;
    }

    .preview-header h6 {
      margin: 0;
      font-weight: 600;
      color: var(--text-primary);
    }

    .preview-body {
      padding: 1.5rem;
    }

    .loading-state {
      text-align: center;
      padding: 3rem;
      color: var(--text-muted);
    }

    .loading-state .spinner-border {
      margin-bottom: 1rem;
    }

    .preview-footer {
      padding-top: 1rem;
      border-top: 1px solid #e9ecef;
      text-align: center;
    }

    .template-card {
      border: 1px solid #e9ecef;
      border-radius: 8px;
      overflow: hidden;
      cursor: pointer;
      transition: all 0.2s ease;
      margin-bottom: 1rem;
    }

    .template-card:hover {
      border-color: #3b82f6;
      box-shadow: 0 4px 12px rgba(59, 130, 246, 0.15);
    }

    .template-preview {
      height: 120px;
      background-color: #f8f9fa;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .template-info {
      padding: 1rem;
    }

    .template-info h6 {
      margin-bottom: 0.5rem;
      font-weight: 600;
    }

    .template-info p {
      font-size: 0.875rem;
      margin-bottom: 0.5rem;
    }
  `]
})
export class ReportBuilderComponent implements OnInit, AfterViewInit {
  @ViewChild('previewChart') previewChart!: ElementRef<HTMLCanvasElement>;
  @ViewChild('finalChart') finalChart!: ElementRef<HTMLCanvasElement>;

  currentStep: number = 1;
  dataSources: ReportDataSource[] = [];
  selectedDataSource: string = '';
  availableColumns: any[] = [];
  filteredColumns: any[] = [];
  selectedColumns: ReportColumn[] = [];
  filters: ReportFilter[] = [];
  columnSearchTerm: string = '';
  
  selectedVisualization: string = 'table';
  visualizationTypes = [
    { type: 'table', name: 'Table', icon: 'fas fa-table' },
    { type: 'chart', name: 'Chart', icon: 'fas fa-chart-bar' }
  ];
  
  chartTypes: string[] = ['Bar', 'Line', 'Pie', 'Doughnut', 'Area'];
  chartConfig: ReportChartConfiguration = {
    type: AppChartType.Bar,
    title: '',
    xAxisColumn: '',
    yAxisColumn: '',
    seriesColumn: '',
    options: {},
    colors: ['#3b82f6', '#ef4444', '#10b981', '#f59e0b', '#8b5cf6']
  };

  previewData: ReportExecutionResult | null = null;
  chartData: any = null;
  isLoadingPreview: boolean = false;
  sampleData: any[] = [];

  saveReportForm: FormGroup;
  reportTemplates: ReportTemplate[] = [];

  private charts: { [key: string]: Chart } = {};
  Math = Math;

  constructor(
    private reportService: ReportService,
    private fb: FormBuilder
  ) {
    this.saveReportForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      type: [ReportType.Table, Validators.required],
      isPublic: [false]
    });
  }

  ngOnInit() {
    this.loadDataSources();
    this.loadReportTemplates();
    this.generateSampleData();
  }

  ngAfterViewInit() {
    // Charts will be initialized when needed
  }

  async loadDataSources() {
    try {
      this.dataSources = await this.reportService.getDataSources();
    } catch (error) {
      console.error('Failed to load data sources:', error);
    }
  }

  loadReportTemplates() {
    // Sample templates - in production, these would come from the backend
    this.reportTemplates = [
      {
        id: '1',
        name: 'Employee Attendance Report',
        description: 'Monthly attendance summary by department',
        category: 'Attendance',
        preview: '/assets/images/template-attendance.png',
        configuration: {
          dataSource: 'attendance',
          columns: [],
          filters: [],
          groupings: [],
          sortings: []
        }
      },
      {
        id: '2',
        name: 'Performance Dashboard',
        description: 'Employee performance metrics and trends',
        category: 'Performance',
        preview: '/assets/images/template-performance.png',
        configuration: {
          dataSource: 'performance',
          columns: [],
          filters: [],
          groupings: [],
          sortings: []
        }
      }
    ];
  }

  generateSampleData() {
    this.sampleData = [
      { id: 1, name: 'John Doe', department: 'Engineering', salary: 75000, joinDate: '2023-01-15' },
      { id: 2, name: 'Jane Smith', department: 'Marketing', salary: 65000, joinDate: '2023-02-20' },
      { id: 3, name: 'Mike Johnson', department: 'Sales', salary: 70000, joinDate: '2023-03-10' },
      { id: 4, name: 'Sarah Wilson', department: 'HR', salary: 60000, joinDate: '2023-04-05' },
      { id: 5, name: 'David Brown', department: 'Engineering', salary: 80000, joinDate: '2023-05-12' }
    ];
  }

  selectDataSource(dataSourceName: string) {
    this.selectedDataSource = dataSourceName;
    this.loadDataSourceSchema();
  }

  async loadDataSourceSchema() {
    if (!this.selectedDataSource) return;

    try {
      const schema = await this.reportService.getDataSourceSchema(this.selectedDataSource);
      this.availableColumns = schema.columns;
      this.filteredColumns = [...this.availableColumns];
    } catch (error) {
      console.error('Failed to load data source schema:', error);
    }
  }

  filterColumns() {
    if (!this.columnSearchTerm) {
      this.filteredColumns = [...this.availableColumns];
      return;
    }

    const term = this.columnSearchTerm.toLowerCase();
    this.filteredColumns = this.availableColumns.filter(col => 
      col.displayName.toLowerCase().includes(term) ||
      col.name.toLowerCase().includes(term) ||
      col.dataType.toLowerCase().includes(term)
    );
  }

  getDataSourceIcon(dataSourceName: string): string {
    const icons: { [key: string]: string } = {
      'employees': 'fas fa-users',
      'attendance': 'fas fa-clock',
      'payroll': 'fas fa-money-bill-wave',
      'projects': 'fas fa-tasks',
      'performance': 'fas fa-chart-line'
    };
    return icons[dataSourceName] || 'fas fa-database';
  }

  getColumnTypeIcon(dataType: string): string {
    const icons: { [key: string]: string } = {
      'string': 'fas fa-font',
      'int': 'fas fa-hashtag',
      'decimal': 'fas fa-calculator',
      'datetime': 'fas fa-calendar',
      'date': 'fas fa-calendar-day',
      'boolean': 'fas fa-toggle-on',
      'timespan': 'fas fa-stopwatch'
    };
    return icons[dataType] || 'fas fa-question-circle';
  }

  onDragStart(event: DragEvent, column: any) {
    event.dataTransfer?.setData('text/json', JSON.stringify(column));
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    (event.currentTarget as HTMLElement)?.classList.add('drag-over');
  }

  onDrop(event: DragEvent, target: string) {
    event.preventDefault();
    (event.currentTarget as HTMLElement)?.classList.remove('drag-over');
    
    const columnData = event.dataTransfer?.getData('text/json');
    if (!columnData) return;

    const column = JSON.parse(columnData);
    
    if (target === 'columns') {
      this.addColumn(column);
    }
  }

  addColumn(column: any) {
    if (this.isColumnSelected(column.name)) return;

    this.selectedColumns.push({
      name: column.name,
      displayName: column.displayName,
      dataType: column.dataType,
      isVisible: true,
      order: this.selectedColumns.length,
      format: '',
      aggregateFunction: '',
      width: undefined,
      alignment: 'left'
    });
  }

  isColumnSelected(columnName: string): boolean {
    return this.selectedColumns.some(col => col.name === columnName);
  }

  removeColumn(index: number) {
    this.selectedColumns.splice(index, 1);
    this.selectedColumns.forEach((col, i) => col.order = i);
  }

  moveColumn(index: number, direction: number) {
    const newIndex = index + direction;
    if (newIndex >= 0 && newIndex < this.selectedColumns.length) {
      const temp = this.selectedColumns[index];
      this.selectedColumns[index] = this.selectedColumns[newIndex];
      this.selectedColumns[newIndex] = temp;
      
      this.selectedColumns.forEach((col, i) => col.order = i);
    }
  }

  addFilter() {
    this.filters.push({
      column: '',
      operator: '=',
      value: '',
      logicalOperator: this.filters.length > 0 ? 'AND' : '',
      order: this.filters.length
    });
  }

  removeFilter(index: number) {
    this.filters.splice(index, 1);
    this.filters.forEach((filter, i) => filter.order = i);
  }

  selectVisualization(type: string) {
    this.selectedVisualization = type;
  }

  nextStep() {
    if (this.currentStep < 4) {
      this.currentStep++;
    }
  }

  previousStep() {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }

  canPreview(): boolean {
    return this.selectedDataSource !== '' && this.selectedColumns.length > 0;
  }

  canSave(): boolean {
    return this.canPreview();
  }

  async previewReport() {
    if (!this.canPreview()) return;

    this.isLoadingPreview = true;

    const configuration: ReportBuilderConfiguration = {
      dataSource: this.selectedDataSource,
      columns: this.selectedColumns,
      filters: this.filters.filter(f => f.column && f.value),
      groupings: [],
      sortings: [],
      chartConfiguration: this.selectedVisualization === 'chart' ? this.chartConfig : undefined,
      pagination: { pageSize: 100, enablePaging: true }
    };

    try {
      this.previewData = await this.reportService.previewReport(configuration);
      
      if (this.selectedVisualization === 'chart' && this.chartConfig.xAxisColumn && this.chartConfig.yAxisColumn) {
        this.chartData = await this.reportService.generateChartData(this.previewData, this.chartConfig);
        setTimeout(() => this.renderFinalChart(), 100);
      }
    } catch (error) {
      console.error('Failed to preview report:', error);
    } finally {
      this.isLoadingPreview = false;
    }
  }

  refreshPreview() {
    this.renderPreviewChart();
  }

  renderPreviewChart() {
    if (!this.previewChart || this.selectedVisualization !== 'chart') return;

    const ctx = this.previewChart.nativeElement.getContext('2d');
    if (!ctx) return;

    if (this.charts['preview']) {
      this.charts['preview'].destroy();
    }

    // Sample chart data for preview
    this.charts['preview'] = new Chart(ctx, {
      type: this.chartConfig.type.toLowerCase() as ChartType,
      data: {
        labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May'],
        datasets: [{
          label: this.chartConfig.yAxisColumn || 'Data',
          data: [12, 19, 3, 5, 2],
          backgroundColor: this.chartConfig.colors?.[0] || '#3b82f6',
          borderColor: this.chartConfig.colors?.[0] || '#3b82f6',
          borderWidth: 2
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          title: {
            display: !!this.chartConfig.title,
            text: this.chartConfig.title
          }
        }
      }
    });
  }

  renderFinalChart() {
    if (!this.finalChart || !this.chartData) return;

    const ctx = this.finalChart.nativeElement.getContext('2d');
    if (!ctx) return;

    if (this.charts['final']) {
      this.charts['final'].destroy();
    }

    this.charts['final'] = new Chart(ctx, {
      type: this.chartConfig.type.toLowerCase() as ChartType,
      data: this.chartData,
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          title: {
            display: !!this.chartConfig.title,
            text: this.chartConfig.title
          }
        }
      }
    });
  }

  formatCellValue(value: any, format?: string): string {
    if (value == null) return '';

    switch (format) {
      case 'currency':
        return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
      case 'percentage':
        return `${(value * 100).toFixed(2)}%`;
      case 'date':
        return new Date(value).toLocaleDateString();
      case 'datetime':
        return new Date(value).toLocaleString();
      default:
        return value.toString();
    }
  }

  saveReport() {
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
      chartConfiguration: this.selectedVisualization === 'chart' ? this.chartConfig : undefined,
      pagination: { pageSize: 50, enablePaging: true }
    };

    const formValue = this.saveReportForm.value;

    try {
      const request: CreateReportRequest = {
        name: formValue.name,
        description: formValue.description,
        type: formValue.type,
        configuration: configuration
      };

      await this.reportService.createReport(request);

      const modal = (window as any).bootstrap.Modal.getInstance(document.getElementById('saveReportModal'));
      modal.hide();
      
      this.saveReportForm.reset();
      alert('Report saved successfully!');
      
      // Reset builder
      this.resetBuilder();
    } catch (error) {
      console.error('Failed to save report:', error);
      alert('Failed to save report. Please try again.');
    }
  }

  async exportReport(format: ReportExportFormat) {
    if (!this.canPreview()) return;

    const configuration: ReportBuilderConfiguration = {
      dataSource: this.selectedDataSource,
      columns: this.selectedColumns,
      filters: this.filters.filter(f => f.column && f.value),
      groupings: [],
      sortings: [],
      chartConfiguration: this.selectedVisualization === 'chart' ? this.chartConfig : undefined,
      pagination: { pageSize: 1000, enablePaging: true }
    };

    try {
      const reportData = await this.reportService.previewReport(configuration);
      const blob = await this.reportService.exportReportData(reportData, format.toLowerCase(), 'Custom Report');
      
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `report_${new Date().getTime()}.${format.toLowerCase()}`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Failed to export report:', error);
      alert('Failed to export report. Please try again.');
    }
  }

  scheduleReport() {
    // Implement report scheduling
    console.log('Schedule report functionality');
  }

  loadTemplate() {
    const modal = new (window as any).bootstrap.Modal(document.getElementById('templatesModal'));
    modal.show();
  }

  loadReportTemplate(template: ReportTemplate) {
    // Load template configuration
    this.selectedDataSource = template.configuration.dataSource;
    this.selectedColumns = [...template.configuration.columns];
    this.filters = [...template.configuration.filters];
    
    // Close modal
    const modal = (window as any).bootstrap.Modal.getInstance(document.getElementById('templatesModal'));
    modal.hide();
    
    // Load data source schema
    this.loadDataSourceSchema();
    
    // Move to step 2
    this.currentStep = 2;
  }

  saveAsTemplate() {
    // Implement save as template functionality
    console.log('Save as template functionality');
  }

  resetBuilder() {
    this.currentStep = 1;
    this.selectedDataSource = '';
    this.availableColumns = [];
    this.selectedColumns = [];
    this.filters = [];
    this.previewData = null;
    this.chartData = null;
    this.selectedVisualization = 'table';
    this.chartConfig = {
      type: AppChartType.Bar,
      title: '',
      xAxisColumn: '',
      yAxisColumn: '',
      seriesColumn: '',
      options: {},
      colors: ['#3b82f6', '#ef4444', '#10b981', '#f59e0b', '#8b5cf6']
    };
  }
}