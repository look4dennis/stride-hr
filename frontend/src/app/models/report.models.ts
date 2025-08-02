import { ReportType, ChartType, ReportExportFormat, ReportPermission } from '../enums/report.enums';

export interface ReportBuilderConfiguration {
  dataSource: string;
  columns: ReportColumn[];
  filters: ReportFilter[];
  groupings: ReportGrouping[];
  sortings: ReportSorting[];
  chartConfiguration?: ReportChartConfiguration;
  pagination?: ReportPagination;
}

export interface ReportColumn {
  name: string;
  displayName: string;
  dataType: string;
  isVisible: boolean;
  order: number;
  format?: string;
  aggregateFunction?: string;
  width?: number;
  alignment?: string;
}

export interface ReportFilter {
  column: string;
  operator: string;
  value: any;
  logicalOperator?: string;
  order: number;
}

export interface ReportGrouping {
  column: string;
  aggregateFunction?: string;
  order: number;
}

export interface ReportSorting {
  column: string;
  direction: string;
  order: number;
}

export interface ReportChartConfiguration {
  type: ChartType;
  title: string;
  xAxisColumn: string;
  yAxisColumn: string;
  seriesColumn?: string;
  options: { [key: string]: any };
  colors: string[];
}

export interface ReportPagination {
  pageSize: number;
  enablePaging: boolean;
}

export interface ReportExecutionResult {
  success: boolean;
  errorMessage?: string;
  data: { [key: string]: any }[];
  totalRecords: number;
  executionTime: number;
  metadata: { [key: string]: any };
}

export interface ReportDataSource {
  name: string;
  displayName: string;
  description: string;
  connectionString: string;
  query: string;
  columns: ReportDataSourceColumn[];
}

export interface ReportDataSourceColumn {
  name: string;
  displayName: string;
  dataType: string;
  isFilterable: boolean;
  isSortable: boolean;
  isGroupable: boolean;
  possibleValues?: string[];
}

export interface Report {
  id: number;
  name: string;
  description: string;
  type: ReportType;
  dataSource: string;
  configuration: string;
  filters: string;
  columns: string;
  chartConfiguration: string;
  isPublic: boolean;
  isScheduled: boolean;
  scheduleCron?: string;
  lastExecuted?: Date;
  status: string;
  createdBy: number;
  branchId?: number;
  createdAt: Date;
  updatedAt?: Date;
}

export interface CreateReportRequest {
  name: string;
  description: string;
  type: ReportType;
  configuration: ReportBuilderConfiguration;
}

export interface UpdateReportRequest {
  name: string;
  description: string;
  configuration: ReportBuilderConfiguration;
}

export interface ShareReportRequest {
  sharedWithUserId: number;
  permission: ReportPermission;
  expiresAt?: Date;
}

export interface ReportExportRequest {
  reportId: number;
  format: ReportExportFormat;
  parameters?: { [key: string]: any };
  fileName?: string;
  includeCharts: boolean;
}

export interface ReportScheduleRequest {
  reportId: number;
  name: string;
  cronExpression: string;
  recipients: string[];
  exportFormat: ReportExportFormat;
  emailSubject?: string;
  emailBody?: string;
  parameters?: { [key: string]: any };
}

export interface ReportSchedule {
  id: number;
  reportId: number;
  name: string;
  cronExpression: string;
  isActive: boolean;
  nextRunTime?: Date;
  lastRunTime?: Date;
  parameters?: string;
  recipients: string;
  exportFormat: ReportExportFormat;
  emailSubject?: string;
  emailBody?: string;
  createdBy: number;
  createdAt: Date;
  updatedAt?: Date;
}