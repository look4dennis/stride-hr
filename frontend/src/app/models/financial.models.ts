export interface FinancialReportRequest {
  branchId?: number;
  startDate: Date;
  endDate: Date;
  currency?: string;
  includeCurrencyConversion?: boolean;
  departments?: string[];
}

export interface FinancialSummaryReport {
  reportTitle: string;
  generatedAt: Date;
  currency: string;
  startDate: Date;
  endDate: Date;
  totalPayrollCost: number;
  totalGrossSalary: number;
  totalNetSalary: number;
  totalDeductions: number;
  totalAllowances: number;
  totalOvertimeCost: number;
  totalEmployees: number;
  averageGrossSalary: number;
  averageNetSalary: number;
  branchSummaries: BranchFinancialSummary[];
  departmentSummaries: DepartmentFinancialSummary[];
  monthlyBreakdown: MonthlyFinancialData[];
}

export interface BranchFinancialSummary {
  branchId: number;
  branchName: string;
  country: string;
  localCurrency: string;
  exchangeRate: number;
  totalPayrollCost: number;
  totalPayrollCostInBaseCurrency: number;
  employeeCount: number;
  averageSalary: number;
  currencyBreakdown: CurrencyBreakdown[];
}

export interface DepartmentFinancialSummary {
  department: string;
  totalCost: number;
  employeeCount: number;
  averageSalary: number;
  percentageOfTotalCost: number;
}

export interface MonthlyFinancialData {
  year: number;
  month: number;
  monthName: string;
  totalCost: number;
  growthPercentage: number;
  employeeCount: number;
}

export interface CurrencyBreakdown {
  currency: string;
  currencySymbol: string;
  amount: number;
  amountInBaseCurrency: number;
  exchangeRate: number;
  employeeCount: number;
}

export interface PayrollCostAnalysisRequest {
  branchId?: number;
  startDate: Date;
  endDate: Date;
  currency?: string;
  departments?: string[];
  includeProjections?: boolean;
}

export interface PayrollCostAnalysisReport {
  reportTitle: string;
  generatedAt: Date;
  currency: string;
  costBreakdown: PayrollCostBreakdown;
  trendData: PayrollTrendData[];
  categoryAnalysis: CostCategoryAnalysis[];
  projection?: PayrollProjection;
}

export interface PayrollCostBreakdown {
  basicSalaryTotal: number;
  allowancesTotal: number;
  overtimeTotal: number;
  deductionsTotal: number;
  grossTotal: number;
  netTotal: number;
  allowanceBreakdown: { [key: string]: number };
  deductionBreakdown: { [key: string]: number };
}

export interface PayrollTrendData {
  date: Date;
  amount: number;
  percentageChange: number;
  category: string;
}

export interface CostCategoryAnalysis {
  category: string;
  amount: number;
  percentage: number;
  varianceFromBudget: number;
  status: string;
}

export interface PayrollProjection {
  projectionDate: Date;
  projectedAmount: number;
  confidenceLevel: number;
  projectionMethod: string;
}

export interface BudgetVarianceRequest {
  branchId?: number;
  startDate: Date;
  endDate: Date;
  currency?: string;
  departments?: string[];
}

export interface BudgetVarianceReport {
  reportTitle: string;
  generatedAt: Date;
  currency: string;
  totalBudget: number;
  totalActual: number;
  totalVariance: number;
  variancePercentage: number;
  varianceItems: BudgetVarianceItem[];
  departmentVariances: DepartmentBudgetVariance[];
}

export interface BudgetVarianceItem {
  category: string;
  budgetedAmount: number;
  actualAmount: number;
  variance: number;
  variancePercentage: number;
  status: string;
  reason: string;
}

export interface DepartmentBudgetVariance {
  department: string;
  budgetedAmount: number;
  actualAmount: number;
  variance: number;
  variancePercentage: number;
  categoryVariances: BudgetVarianceItem[];
}

export interface CurrencyConversionRequest {
  startDate: Date;
  endDate: Date;
  baseCurrency: string;
  targetCurrencies?: string[];
  includeHistoricalRates?: boolean;
}

export interface CurrencyConversionReport {
  reportTitle: string;
  generatedAt: Date;
  baseCurrency: string;
  conversionData: CurrencyConversionData[];
  rateHistory: ExchangeRateHistory[];
  riskAnalysis: CurrencyRiskAnalysis;
}

export interface CurrencyConversionData {
  currency: string;
  currencySymbol: string;
  currentRate: number;
  totalAmountInCurrency: number;
  totalAmountInBaseCurrency: number;
  rateVariation: number;
  trend: string;
}

export interface ExchangeRateHistory {
  date: Date;
  currency: string;
  rate: number;
  change: number;
  percentageChange: number;
}

export interface CurrencyRiskAnalysis {
  totalExposure: number;
  highestRiskCurrency: number;
  highestRiskCurrencyCode: string;
  averageVolatility: number;
  currencyRisks: CurrencyRisk[];
}

export interface CurrencyRisk {
  currency: string;
  exposure: number;
  volatility: number;
  riskLevel: string;
  recommendation: string;
}

export interface DepartmentFinancialRequest {
  branchId?: number;
  startDate: Date;
  endDate: Date;
  currency?: string;
  departments?: string[];
  includeSubDepartments?: boolean;
}

export interface DepartmentWiseFinancialReport {
  reportTitle: string;
  generatedAt: Date;
  currency: string;
  departmentDetails: DepartmentFinancialDetail[];
  comparison: DepartmentComparison;
}

export interface DepartmentFinancialDetail {
  department: string;
  employeeCount: number;
  totalCost: number;
  averageSalary: number;
  medianSalary: number;
  highestSalary: number;
  lowestSalary: number;
  costBreakdown: PayrollCostBreakdown;
  topEarners: EmployeeFinancialSummary[];
}

export interface EmployeeFinancialSummary {
  employeeId: string;
  employeeName: string;
  designation: string;
  grossSalary: number;
  netSalary: number;
}

export interface DepartmentComparison {
  highestCostDepartment: string;
  lowestCostDepartment: string;
  costVariation: number;
  rankings: DepartmentRanking[];
}

export interface DepartmentRanking {
  rank: number;
  department: string;
  totalCost: number;
  percentageOfTotal: number;
}

export interface MonthlyTrendRequest {
  branchId?: number;
  startDate: Date;
  endDate: Date;
  currency?: string;
  trendType: string;
}

export interface MonthlyFinancialTrendReport {
  reportTitle: string;
  generatedAt: Date;
  currency: string;
  trendType: string;
  trendData: MonthlyTrendData[];
  analysis: TrendAnalysis;
}

export interface MonthlyTrendData {
  year: number;
  month: number;
  monthName: string;
  value: number;
  percentageChange: number;
  movingAverage: number;
}

export interface TrendAnalysis {
  overallTrend: string;
  averageGrowthRate: number;
  highestValue: number;
  lowestValue: number;
  highestMonth: string;
  lowestMonth: string;
  insights: string[];
}

export interface FinancialMetric {
  metricName: string;
  value: number;
  unit: string;
  date: Date;
  category: string;
}

export interface CurrencyExchangeRate {
  date: Date;
  baseCurrency: string;
  targetCurrency: string;
  rate: number;
  change: number;
  percentageChange: number;
}

export interface FinancialDashboardData {
  totalPayrollCost: number;
  monthlyGrowth: number;
  currencyExposure: CurrencyBreakdown[];
  topDepartments: DepartmentFinancialSummary[];
  budgetVariance: number;
  exchangeRateAlerts: ExchangeRateAlert[];
}

export interface ExchangeRateAlert {
  currency: string;
  currentRate: number;
  change: number;
  alertType: 'warning' | 'danger' | 'info';
  message: string;
}

export interface CurrencyDisplayOptions {
  showSymbol: boolean;
  decimalPlaces: number;
  thousandsSeparator: string;
  decimalSeparator: string;
}

export interface FinancialReportExportOptions {
  format: 'pdf' | 'excel' | 'csv';
  includeCharts: boolean;
  includeSummary: boolean;
  includeDetails: boolean;
}