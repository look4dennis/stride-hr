export enum ReportType {
  Table = 'Table',
  Chart = 'Chart',
  Dashboard = 'Dashboard',
  Summary = 'Summary',
  Detailed = 'Detailed',
  Analytical = 'Analytical',
  Compliance = 'Compliance',
  Custom = 'Custom'
}

export enum ReportStatus {
  Draft = 'Draft',
  Active = 'Active',
  Inactive = 'Inactive',
  Archived = 'Archived',
  Error = 'Error'
}

export enum ReportExecutionStatus {
  Pending = 'Pending',
  Running = 'Running',
  Completed = 'Completed',
  Failed = 'Failed',
  Cancelled = 'Cancelled',
  Timeout = 'Timeout'
}

export enum ReportExportFormat {
  PDF = 'PDF',
  Excel = 'Excel',
  CSV = 'CSV',
  JSON = 'JSON',
  XML = 'XML',
  Word = 'Word',
  PowerPoint = 'PowerPoint',
  HTML = 'HTML'
}

export enum ReportPermission {
  View = 'View',
  Edit = 'Edit',
  Execute = 'Execute',
  Share = 'Share',
  Delete = 'Delete',
  FullControl = 'FullControl'
}

export enum ChartType {
  Bar = 'Bar',
  Line = 'Line',
  Pie = 'Pie',
  Doughnut = 'Doughnut',
  Area = 'Area',
  Scatter = 'Scatter',
  Bubble = 'Bubble',
  Radar = 'Radar',
  PolarArea = 'PolarArea',
  Histogram = 'Histogram',
  Heatmap = 'Heatmap',
  Gauge = 'Gauge',
  Funnel = 'Funnel',
  Waterfall = 'Waterfall'
}