import { Component, OnInit, ViewChild, ElementRef, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Chart, ChartConfiguration, ChartType, registerables } from 'chart.js';
import { ReportExecutionResult, ReportChartConfiguration } from '../../../models/report.models';
import { ChartType as AppChartType } from '../../../enums/report.enums';

Chart.register(...registerables);

interface ChartDataset {
    label: string;
    data: number[];
    backgroundColor?: string | string[];
    borderColor?: string | string[];
    borderWidth?: number;
}

interface ChartData {
    labels: string[];
    datasets: ChartDataset[];
}

@Component({
    selector: 'app-data-visualization',
    standalone: true,
    imports: [CommonModule, FormsModule],
    template: `
    <div class="data-visualization">
      <!-- Visualization Controls -->
      <div class="visualization-controls mb-3" *ngIf="showControls">
        <div class="row align-items-center">
          <div class="col-md-3">
            <label class="form-label">Chart Type</label>
            <select class="form-select form-select-sm" 
                    [(ngModel)]="selectedChartType" 
                    (change)="onChartTypeChange()">
              <option value="bar">Bar Chart</option>
              <option value="line">Line Chart</option>
              <option value="pie">Pie Chart</option>
              <option value="doughnut">Doughnut Chart</option>
              <option value="radar">Radar Chart</option>
              <option value="polarArea">Polar Area Chart</option>
              <option value="scatter">Scatter Plot</option>
            </select>
          </div>
          <div class="col-md-3">
            <label class="form-label">X-Axis Column</label>
            <select class="form-select form-select-sm" 
                    [(ngModel)]="xAxisColumn" 
                    (change)="onAxisChange()">
              <option value="">Select Column</option>
              <option *ngFor="let col of availableColumns" [value]="col">{{col}}</option>
            </select>
          </div>
          <div class="col-md-3">
            <label class="form-label">Y-Axis Column</label>
            <select class="form-select form-select-sm" 
                    [(ngModel)]="yAxisColumn" 
                    (change)="onAxisChange()">
              <option value="">Select Column</option>
              <option *ngFor="let col of availableColumns" [value]="col">{{col}}</option>
            </select>
          </div>
          <div class="col-md-3">
            <label class="form-label">Series Column (Optional)</label>
            <select class="form-select form-select-sm" 
                    [(ngModel)]="seriesColumn" 
                    (change)="onAxisChange()">
              <option value="">None</option>
              <option *ngFor="let col of availableColumns" [value]="col">{{col}}</option>
            </select>
          </div>
        </div>
        <div class="row mt-2">
          <div class="col-md-6">
            <label class="form-label">Chart Title</label>
            <input type="text" 
                   class="form-control form-control-sm" 
                   [(ngModel)]="chartTitle" 
                   (input)="onTitleChange()"
                   placeholder="Enter chart title">
          </div>
          <div class="col-md-3">
            <label class="form-label">Animation</label>
            <div class="form-check form-switch">
              <input class="form-check-input" 
                     type="checkbox" 
                     [(ngModel)]="enableAnimation" 
                     (change)="onAnimationChange()">
              <label class="form-check-label">Enable Animation</label>
            </div>
          </div>
          <div class="col-md-3">
            <label class="form-label">Legend</label>
            <div class="form-check form-switch">
              <input class="form-check-input" 
                     type="checkbox" 
                     [(ngModel)]="showLegend" 
                     (change)="onLegendChange()">
              <label class="form-check-label">Show Legend</label>
            </div>
          </div>
        </div>
      </div>

      <!-- Chart Container -->
      <div class="chart-container" [style.height]="chartHeight">
        <div class="loading-state" *ngIf="isLoading">
          <div class="spinner-border text-primary"></div>
          <p>Generating visualization...</p>
        </div>
        
        <div class="error-state" *ngIf="error && !isLoading">
          <div class="alert alert-danger">
            <i class="fas fa-exclamation-triangle me-2"></i>
            {{error}}
          </div>
        </div>

        <div class="no-data-state" *ngIf="!data && !isLoading && !error">
          <div class="text-center text-muted">
            <i class="fas fa-chart-bar fa-3x mb-3"></i>
            <h5>No Data Available</h5>
            <p>Please provide data to generate visualization</p>
          </div>
        </div>

        <canvas #chartCanvas 
                *ngIf="data && !isLoading && !error"
                [style.display]="chart ? 'block' : 'none'">
        </canvas>
      </div>

      <!-- Chart Statistics -->
      <div class="chart-statistics mt-3" *ngIf="data && !isLoading && !error && showStatistics">
        <div class="row">
          <div class="col-md-3">
            <div class="stat-card">
              <div class="stat-value">{{dataPoints}}</div>
              <div class="stat-label">Data Points</div>
            </div>
          </div>
          <div class="col-md-3">
            <div class="stat-card">
              <div class="stat-value">{{uniqueCategories}}</div>
              <div class="stat-label">Categories</div>
            </div>
          </div>
          <div class="col-md-3">
            <div class="stat-card">
              <div class="stat-value">{{maxValue | number:'1.0-2'}}</div>
              <div class="stat-label">Max Value</div>
            </div>
          </div>
          <div class="col-md-3">
            <div class="stat-card">
              <div class="stat-value">{{minValue | number:'1.0-2'}}</div>
              <div class="stat-label">Min Value</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Export Options -->
      <div class="export-options mt-3" *ngIf="data && !isLoading && !error && showExportOptions">
        <div class="btn-group">
          <button class="btn btn-outline-secondary btn-sm" (click)="exportChart('png')">
            <i class="fas fa-image me-1"></i>PNG
          </button>
          <button class="btn btn-outline-secondary btn-sm" (click)="exportChart('jpg')">
            <i class="fas fa-image me-1"></i>JPG
          </button>
          <button class="btn btn-outline-secondary btn-sm" (click)="exportChart('pdf')">
            <i class="fas fa-file-pdf me-1"></i>PDF
          </button>
          <button class="btn btn-outline-secondary btn-sm" (click)="exportChart('svg')">
            <i class="fas fa-vector-square me-1"></i>SVG
          </button>
        </div>
      </div>
    </div>
  `,
    styles: [`
    .data-visualization {
      background: white;
      border-radius: 8px;
      padding: 1.5rem;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }

    .visualization-controls {
      background: #f8f9fa;
      padding: 1rem;
      border-radius: 6px;
      border: 1px solid #e9ecef;
    }

    .chart-container {
      position: relative;
      min-height: 400px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .loading-state, .error-state, .no-data-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 100%;
      text-align: center;
    }

    .loading-state .spinner-border {
      margin-bottom: 1rem;
    }

    .no-data-state i {
      color: #6c757d;
    }

    .chart-statistics {
      background: #f8f9fa;
      padding: 1rem;
      border-radius: 6px;
      border: 1px solid #e9ecef;
    }

    .stat-card {
      text-align: center;
      padding: 0.75rem;
      background: white;
      border-radius: 4px;
      border: 1px solid #dee2e6;
    }

    .stat-value {
      font-size: 1.5rem;
      font-weight: 700;
      color: var(--primary);
      margin-bottom: 0.25rem;
    }

    .stat-label {
      font-size: 0.875rem;
      color: var(--text-secondary);
      font-weight: 500;
    }

    .export-options {
      text-align: center;
    }

    canvas {
      max-width: 100%;
      height: auto !important;
    }
  `]
})
export class DataVisualizationComponent implements OnInit, OnChanges {
    @ViewChild('chartCanvas') chartCanvas!: ElementRef<HTMLCanvasElement>;

    @Input() data: ReportExecutionResult | null = null;
    @Input() chartConfiguration: ReportChartConfiguration | null = null;
    @Input() showControls: boolean = true;
    @Input() showStatistics: boolean = true;
    @Input() showExportOptions: boolean = true;
    @Input() chartHeight: string = '400px';
    @Input() responsive: boolean = true;

    @Output() configurationChange = new EventEmitter<ReportChartConfiguration>();
    @Output() chartReady = new EventEmitter<Chart>();
    @Output() chartError = new EventEmitter<string>();

    chart: Chart | null = null;
    isLoading: boolean = false;
    error: string | null = null;

    // Chart configuration
    selectedChartType: string = 'bar';
    xAxisColumn: string = '';
    yAxisColumn: string = '';
    seriesColumn: string = '';
    chartTitle: string = '';
    enableAnimation: boolean = true;
    showLegend: boolean = true;

    // Data statistics
    dataPoints: number = 0;
    uniqueCategories: number = 0;
    maxValue: number = 0;
    minValue: number = 0;

    // Available columns from data
    availableColumns: string[] = [];

    ngOnInit() {
        this.initializeConfiguration();
    }

    ngOnChanges(changes: SimpleChanges) {
        if (changes['data'] && this.data) {
            this.updateAvailableColumns();
            this.calculateStatistics();
            this.generateChart();
        }

        if (changes['chartConfiguration'] && this.chartConfiguration) {
            this.applyConfiguration();
            this.generateChart();
        }
    }

    private initializeConfiguration() {
        if (this.chartConfiguration) {
            this.applyConfiguration();
        }
    }

    private applyConfiguration() {
        if (!this.chartConfiguration) return;

        this.selectedChartType = this.chartConfiguration.type || 'bar';
        this.xAxisColumn = this.chartConfiguration.xAxisColumn || '';
        this.yAxisColumn = this.chartConfiguration.yAxisColumn || '';
        this.seriesColumn = this.chartConfiguration.seriesColumn || '';
        this.chartTitle = this.chartConfiguration.title || '';
        this.enableAnimation = this.chartConfiguration.enableAnimation !== false;
        this.showLegend = this.chartConfiguration.showLegend !== false;
    }

    private updateAvailableColumns() {
        if (!this.data || !this.data.data || this.data.data.length === 0) {
            this.availableColumns = [];
            return;
        }

        this.availableColumns = Object.keys(this.data.data[0]);
    }

    private calculateStatistics() {
        if (!this.data || !this.data.data) {
            this.dataPoints = 0;
            this.uniqueCategories = 0;
            this.maxValue = 0;
            this.minValue = 0;
            return;
        }

        this.dataPoints = this.data.data.length;

        if (this.xAxisColumn) {
            const uniqueValues = new Set(this.data.data.map(row => row[this.xAxisColumn]));
            this.uniqueCategories = uniqueValues.size;
        }

        if (this.yAxisColumn) {
            const values = this.data.data
                .map(row => parseFloat(row[this.yAxisColumn]) || 0)
                .filter(val => !isNaN(val));

            this.maxValue = values.length > 0 ? Math.max(...values) : 0;
            this.minValue = values.length > 0 ? Math.min(...values) : 0;
        }
    }

    private async generateChart() {
        if (!this.data || !this.data.data || !this.xAxisColumn || !this.yAxisColumn) {
            return;
        }

        this.isLoading = true;
        this.error = null;

        try {
            await this.createChart();
            this.chartReady.emit(this.chart!);
        } catch (error) {
            this.error = 'Failed to generate chart: ' + (error as Error).message;
            this.chartError.emit(this.error);
        } finally {
            this.isLoading = false;
        }
    }

    private async createChart() {
        if (this.chart) {
            this.chart.destroy();
        }

        if (!this.chartCanvas) {
            await new Promise(resolve => setTimeout(resolve, 100));
        }

        const ctx = this.chartCanvas.nativeElement.getContext('2d');
        if (!ctx) {
            throw new Error('Could not get canvas context');
        }

        const chartData = this.prepareChartData();
        const chartConfig = this.createChartConfiguration(chartData);

        this.chart = new Chart(ctx, chartConfig);
    }

    private prepareChartData(): ChartData {
        if (!this.data || !this.data.data) {
            return { labels: [], datasets: [] };
        }

        const labels: string[] = [];
        const dataMap = new Map<string, number>();

        // Process data based on chart type
        if (this.selectedChartType === 'pie' || this.selectedChartType === 'doughnut') {
            // For pie/doughnut charts, aggregate data by category
            this.data.data.forEach(row => {
                const label = String(row[this.xAxisColumn] || 'Unknown');
                const value = parseFloat(row[this.yAxisColumn]) || 0;

                if (dataMap.has(label)) {
                    dataMap.set(label, dataMap.get(label)! + value);
                } else {
                    dataMap.set(label, value);
                }
            });

            labels.push(...dataMap.keys());
            const data = Array.from(dataMap.values());

            return {
                labels,
                datasets: [{
                    label: this.yAxisColumn,
                    data,
                    backgroundColor: this.generateColors(labels.length),
                    borderColor: this.generateColors(labels.length, 0.8),
                    borderWidth: 2
                }]
            };
        } else {
            // For other chart types
            if (this.seriesColumn) {
                // Group by series
                const seriesMap = new Map<string, Map<string, number>>();

                this.data.data.forEach(row => {
                    const xValue = String(row[this.xAxisColumn] || 'Unknown');
                    const yValue = parseFloat(row[this.yAxisColumn]) || 0;
                    const series = String(row[this.seriesColumn] || 'Default');

                    if (!seriesMap.has(series)) {
                        seriesMap.set(series, new Map());
                    }

                    seriesMap.get(series)!.set(xValue, yValue);

                    if (!labels.includes(xValue)) {
                        labels.push(xValue);
                    }
                });

                const datasets: ChartDataset[] = [];
                const colors = this.generateColors(seriesMap.size);
                let colorIndex = 0;

                seriesMap.forEach((seriesData, seriesName) => {
                    const data = labels.map(label => seriesData.get(label) || 0);

                    datasets.push({
                        label: seriesName,
                        data,
                        backgroundColor: colors[colorIndex],
                        borderColor: colors[colorIndex],
                        borderWidth: 2
                    });

                    colorIndex++;
                });

                return { labels, datasets };
            } else {
                // Single series
                this.data.data.forEach(row => {
                    const label = String(row[this.xAxisColumn] || 'Unknown');
                    const value = parseFloat(row[this.yAxisColumn]) || 0;

                    if (dataMap.has(label)) {
                        dataMap.set(label, dataMap.get(label)! + value);
                    } else {
                        dataMap.set(label, value);
                    }
                });

                labels.push(...dataMap.keys());
                const data = Array.from(dataMap.values());

                return {
                    labels,
                    datasets: [{
                        label: this.yAxisColumn,
                        data,
                        backgroundColor: this.generateColors(1)[0],
                        borderColor: this.generateColors(1, 0.8)[0],
                        borderWidth: 2
                    }]
                };
            }
        }
    }

    private createChartConfiguration(chartData: ChartData): ChartConfiguration {
        return {
            type: this.selectedChartType as ChartType,
            data: chartData,
            options: {
                responsive: this.responsive,
                maintainAspectRatio: false,
                animation: {
                    duration: this.enableAnimation ? 1000 : 0
                },
                plugins: {
                    title: {
                        display: !!this.chartTitle,
                        text: this.chartTitle,
                        font: {
                            size: 16,
                            weight: 'bold'
                        }
                    },
                    legend: {
                        display: this.showLegend,
                        position: 'top'
                    },
                    tooltip: {
                        mode: 'index',
                        intersect: false
                    }
                },
                scales: this.getScalesConfiguration(),
                interaction: {
                    mode: 'nearest',
                    axis: 'x',
                    intersect: false
                }
            }
        };
    }

    private getScalesConfiguration(): any {
        if (this.selectedChartType === 'pie' ||
            this.selectedChartType === 'doughnut' ||
            this.selectedChartType === 'polarArea') {
            return {};
        }

        return {
            x: {
                display: true,
                title: {
                    display: true,
                    text: this.xAxisColumn
                }
            },
            y: {
                display: true,
                title: {
                    display: true,
                    text: this.yAxisColumn
                },
                beginAtZero: true
            }
        };
    }

    private generateColors(count: number, alpha: number = 0.6): string[] {
        const colors = [
            `rgba(59, 130, 246, ${alpha})`,   // Blue
            `rgba(16, 185, 129, ${alpha})`,   // Green
            `rgba(245, 158, 11, ${alpha})`,   // Yellow
            `rgba(239, 68, 68, ${alpha})`,    // Red
            `rgba(139, 92, 246, ${alpha})`,   // Purple
            `rgba(6, 182, 212, ${alpha})`,    // Cyan
            `rgba(236, 72, 153, ${alpha})`,   // Pink
            `rgba(34, 197, 94, ${alpha})`,    // Emerald
            `rgba(251, 146, 60, ${alpha})`,   // Orange
            `rgba(168, 85, 247, ${alpha})`    // Violet
        ];

        const result: string[] = [];
        for (let i = 0; i < count; i++) {
            result.push(colors[i % colors.length]);
        }

        return result;
    }

    // Event handlers
    onChartTypeChange() {
        this.emitConfigurationChange();
        this.generateChart();
    }

    onAxisChange() {
        this.calculateStatistics();
        this.emitConfigurationChange();
        this.generateChart();
    }

    onTitleChange() {
        this.emitConfigurationChange();
        if (this.chart) {
            this.chart.options.plugins!.title!.text = this.chartTitle;
            this.chart.update();
        }
    }

    onAnimationChange() {
        this.emitConfigurationChange();
        this.generateChart();
    }

    onLegendChange() {
        this.emitConfigurationChange();
        if (this.chart) {
            this.chart.options.plugins!.legend!.display = this.showLegend;
            this.chart.update();
        }
    }

    private emitConfigurationChange() {
        const config: ReportChartConfiguration = {
            type: this.selectedChartType,
            xAxisColumn: this.xAxisColumn,
            yAxisColumn: this.yAxisColumn,
            seriesColumn: this.seriesColumn,
            title: this.chartTitle,
            enableAnimation: this.enableAnimation,
            showLegend: this.showLegend
        };

        this.configurationChange.emit(config);
    }

    // Export functionality
    exportChart(format: string) {
        if (!this.chart) return;

        try {
            let dataUrl: string;

            switch (format) {
                case 'png':
                    dataUrl = this.chart.toBase64Image('image/png', 1);
                    this.downloadImage(dataUrl, `chart.png`);
                    break;
                case 'jpg':
                    dataUrl = this.chart.toBase64Image('image/jpeg', 0.9);
                    this.downloadImage(dataUrl, `chart.jpg`);
                    break;
                case 'svg':
                    // SVG export would require additional library
                    console.warn('SVG export not implemented');
                    break;
                case 'pdf':
                    // PDF export would require additional library
                    console.warn('PDF export not implemented');
                    break;
            }
        } catch (error) {
            console.error('Export failed:', error);
        }
    }

    private downloadImage(dataUrl: string, filename: string) {
        const link = document.createElement('a');
        link.download = filename;
        link.href = dataUrl;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }

    // Public methods
    refreshChart() {
        this.generateChart();
    }

    updateData(newData: ReportExecutionResult) {
        this.data = newData;
        this.updateAvailableColumns();
        this.calculateStatistics();
        this.generateChart();
    }

    updateConfiguration(config: ReportChartConfiguration) {
        this.chartConfiguration = config;
        this.applyConfiguration();
        this.generateChart();
    }
}