import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, firstValueFrom } from 'rxjs';
import { 
  Report, 
  CreateReportRequest, 
  UpdateReportRequest, 
  ShareReportRequest,
  ReportExportRequest,
  ReportScheduleRequest,
  ReportSchedule,
  ReportBuilderConfiguration,
  ReportExecutionResult,
  ReportDataSource,
  ReportChartConfiguration
} from '../models/report.models';
import { ReportExportFormat } from '../enums/report.enums';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private readonly apiUrl = '/api/reports';
  private readonly schedulesApiUrl = '/api/reportschedules';

  constructor(private http: HttpClient) {}

  // Report CRUD operations
  async getReports(): Promise<{ userReports: Report[], publicReports: Report[], sharedReports: Report[] }> {
    return firstValueFrom(this.http.get<any>(this.apiUrl));
  }

  async getReport(id: number): Promise<Report> {
    return firstValueFrom(this.http.get<Report>(`${this.apiUrl}/${id}`));
  }

  async createReport(request: CreateReportRequest): Promise<Report> {
    return firstValueFrom(this.http.post<Report>(this.apiUrl, request));
  }

  async updateReport(id: number, request: UpdateReportRequest): Promise<Report> {
    return firstValueFrom(this.http.put<Report>(`${this.apiUrl}/${id}`, request));
  }

  async deleteReport(id: number): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${this.apiUrl}/${id}`));
  }

  // Report execution
  async executeReport(id: number, parameters?: { [key: string]: any }): Promise<ReportExecutionResult> {
    return firstValueFrom(this.http.post<ReportExecutionResult>(`${this.apiUrl}/${id}/execute`, parameters || {}));
  }

  async previewReport(configuration: ReportBuilderConfiguration): Promise<ReportExecutionResult> {
    return firstValueFrom(this.http.post<ReportExecutionResult>(`${this.apiUrl}/preview`, configuration));
  }

  // Report sharing
  async shareReport(id: number, request: ShareReportRequest): Promise<{ message: string }> {
    return firstValueFrom(this.http.post<{ message: string }>(`${this.apiUrl}/${id}/share`, request));
  }

  async revokeReportShare(reportId: number, userId: number): Promise<{ message: string }> {
    return firstValueFrom(this.http.delete<{ message: string }>(`${this.apiUrl}/${reportId}/share/${userId}`));
  }

  // Report export
  async exportReport(id: number, format: ReportExportFormat, parameters?: { [key: string]: any }, fileName?: string): Promise<Blob> {
    const request: ReportExportRequest = {
      reportId: id,
      format: format,
      parameters: parameters,
      fileName: fileName,
      includeCharts: true
    };

    const response = await firstValueFrom(
      this.http.post(`${this.apiUrl}/${id}/export`, request, { 
        responseType: 'blob',
        observe: 'response'
      })
    );

    return response.body!;
  }

  async exportReportData(data: ReportExecutionResult, format: string, reportName: string): Promise<Blob> {
    // This would typically call a backend endpoint to export the data
    // For now, we'll create a simple CSV export on the client side
    if (format === 'csv') {
      return this.exportToCsv(data, reportName);
    }
    
    // For other formats, you would call the backend
    throw new Error(`Export format ${format} not implemented`);
  }

  private exportToCsv(data: ReportExecutionResult, reportName: string): Blob {
    if (!data.data || data.data.length === 0) {
      return new Blob(['No data to export'], { type: 'text/csv' });
    }

    const headers = Object.keys(data.data[0]);
    const csvContent = [
      headers.join(','),
      ...data.data.map(row => 
        headers.map(header => {
          const value = row[header]?.toString() || '';
          return `"${value.replace(/"/g, '""')}"`;
        }).join(',')
      )
    ].join('\n');

    return new Blob([csvContent], { type: 'text/csv' });
  }

  // Data sources
  async getDataSources(): Promise<ReportDataSource[]> {
    return firstValueFrom(this.http.get<ReportDataSource[]>(`${this.apiUrl}/data-sources`));
  }

  async getDataSourceSchema(name: string): Promise<ReportDataSource> {
    return firstValueFrom(this.http.get<ReportDataSource>(`${this.apiUrl}/data-sources/${name}/schema`));
  }

  // Chart and visualization
  async generateChartData(data: ReportExecutionResult, chartConfig: ReportChartConfiguration): Promise<any> {
    return firstValueFrom(this.http.post<any>(`${this.apiUrl}/chart-data`, { data, chartConfiguration: chartConfig }));
  }

  async getSupportedChartTypes(): Promise<string[]> {
    return firstValueFrom(this.http.get<string[]>(`${this.apiUrl}/chart-types`));
  }

  async suggestChartConfiguration(columns: any[], sampleData: any[]): Promise<ReportChartConfiguration> {
    return firstValueFrom(this.http.post<ReportChartConfiguration>(`${this.apiUrl}/suggest-chart`, { columns, sampleData }));
  }

  // Report scheduling
  async getSchedules(): Promise<ReportSchedule[]> {
    return firstValueFrom(this.http.get<ReportSchedule[]>(this.schedulesApiUrl));
  }

  async getSchedule(id: number): Promise<ReportSchedule> {
    return firstValueFrom(this.http.get<ReportSchedule>(`${this.schedulesApiUrl}/${id}`));
  }

  async getReportSchedules(reportId: number): Promise<ReportSchedule[]> {
    return firstValueFrom(this.http.get<ReportSchedule[]>(`${this.schedulesApiUrl}/report/${reportId}`));
  }

  async createSchedule(request: ReportScheduleRequest): Promise<ReportSchedule> {
    return firstValueFrom(this.http.post<ReportSchedule>(this.schedulesApiUrl, request));
  }

  async updateSchedule(id: number, request: ReportScheduleRequest): Promise<ReportSchedule> {
    return firstValueFrom(this.http.put<ReportSchedule>(`${this.schedulesApiUrl}/${id}`, request));
  }

  async deleteSchedule(id: number): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${this.schedulesApiUrl}/${id}`));
  }

  async activateSchedule(id: number): Promise<{ message: string }> {
    return firstValueFrom(this.http.post<{ message: string }>(`${this.schedulesApiUrl}/${id}/activate`, {}));
  }

  async deactivateSchedule(id: number): Promise<{ message: string }> {
    return firstValueFrom(this.http.post<{ message: string }>(`${this.schedulesApiUrl}/${id}/deactivate`, {}));
  }

  async validateCronExpression(cronExpression: string): Promise<{ isValid: boolean, nextRunTime?: Date }> {
    return firstValueFrom(this.http.post<{ isValid: boolean, nextRunTime?: Date }>(`${this.schedulesApiUrl}/validate-cron`, { cronExpression }));
  }
}