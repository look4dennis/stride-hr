import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  FinancialReportRequest,
  FinancialSummaryReport,
  PayrollCostAnalysisRequest,
  PayrollCostAnalysisReport,
  BudgetVarianceRequest,
  BudgetVarianceReport,
  CurrencyConversionRequest,
  CurrencyConversionReport,
  DepartmentFinancialRequest,
  DepartmentWiseFinancialReport,
  MonthlyTrendRequest,
  MonthlyFinancialTrendReport,
  FinancialMetric,
  CurrencyExchangeRate,
  FinancialDashboardData
} from '../models/financial.models';

@Injectable({
  providedIn: 'root'
})
export class FinancialService {
  private readonly apiUrl = `${environment.apiUrl}/financial-reports`;

  constructor(private http: HttpClient) {}

  /**
   * Generate financial summary report
   */
  generateFinancialSummary(request: FinancialReportRequest): Observable<FinancialSummaryReport> {
    return this.http.post<FinancialSummaryReport>(`${this.apiUrl}/summary`, request);
  }

  /**
   * Generate payroll cost analysis report
   */
  generatePayrollCostAnalysis(request: PayrollCostAnalysisRequest): Observable<PayrollCostAnalysisReport> {
    return this.http.post<PayrollCostAnalysisReport>(`${this.apiUrl}/payroll-cost-analysis`, request);
  }

  /**
   * Generate budget variance report
   */
  generateBudgetVariance(request: BudgetVarianceRequest): Observable<BudgetVarianceReport> {
    return this.http.post<BudgetVarianceReport>(`${this.apiUrl}/budget-variance`, request);
  }

  /**
   * Generate currency conversion report
   */
  generateCurrencyConversion(request: CurrencyConversionRequest): Observable<CurrencyConversionReport> {
    return this.http.post<CurrencyConversionReport>(`${this.apiUrl}/currency-conversion`, request);
  }

  /**
   * Generate department-wise financial report
   */
  generateDepartmentWiseReport(request: DepartmentFinancialRequest): Observable<DepartmentWiseFinancialReport> {
    return this.http.post<DepartmentWiseFinancialReport>(`${this.apiUrl}/department-wise`, request);
  }

  /**
   * Generate monthly financial trend report
   */
  generateMonthlyTrend(request: MonthlyTrendRequest): Observable<MonthlyFinancialTrendReport> {
    return this.http.post<MonthlyFinancialTrendReport>(`${this.apiUrl}/monthly-trend`, request);
  }

  /**
   * Get financial metrics for dashboard
   */
  getFinancialMetrics(branchId: number, startDate: Date, endDate: Date): Observable<FinancialMetric[]> {
    const params = new HttpParams()
      .set('branchId', branchId.toString())
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());

    return this.http.get<FinancialMetric[]>(`${this.apiUrl}/metrics`, { params });
  }

  /**
   * Get exchange rate history
   */
  getExchangeRateHistory(baseCurrency: string, startDate: Date, endDate: Date): Observable<CurrencyExchangeRate[]> {
    const params = new HttpParams()
      .set('baseCurrency', baseCurrency)
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());

    return this.http.get<CurrencyExchangeRate[]>(`${this.apiUrl}/exchange-rate-history`, { params });
  }

  /**
   * Get financial dashboard data
   */
  getFinancialDashboardData(branchId?: number): Observable<FinancialDashboardData> {
    const params = new HttpParams();
    if (branchId) {
      params.set('branchId', branchId.toString());
    }

    return this.http.get<FinancialDashboardData>(`${this.apiUrl}/dashboard`, { params });
  }

  /**
   * Format currency amount with proper symbol and formatting
   */
  formatCurrency(amount: number, currency: string, showSymbol: boolean = true): string {
    const currencySymbols: { [key: string]: string } = {
      'USD': '$',
      'EUR': '€',
      'GBP': '£',
      'INR': '₹',
      'CAD': 'C$',
      'AUD': 'A$',
      'JPY': '¥',
      'SGD': 'S$',
      'AED': 'د.إ',
      'CHF': 'CHF',
      'CNY': '¥',
      'HKD': 'HK$',
      'NZD': 'NZ$',
      'SEK': 'kr',
      'NOK': 'kr',
      'DKK': 'kr'
    };

    const symbol = showSymbol ? (currencySymbols[currency] || currency) : '';
    const formattedAmount = new Intl.NumberFormat('en-US', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(amount);

    return `${symbol}${formattedAmount}`;
  }

  /**
   * Convert currency amount
   */
  convertCurrency(amount: number, fromCurrency: string, toCurrency: string, exchangeRate: number): number {
    if (fromCurrency === toCurrency) {
      return amount;
    }
    return amount * exchangeRate;
  }

  /**
   * Get currency symbol
   */
  getCurrencySymbol(currency: string): string {
    const currencySymbols: { [key: string]: string } = {
      'USD': '$',
      'EUR': '€',
      'GBP': '£',
      'INR': '₹',
      'CAD': 'C$',
      'AUD': 'A$',
      'JPY': '¥',
      'SGD': 'S$',
      'AED': 'د.إ',
      'CHF': 'CHF',
      'CNY': '¥',
      'HKD': 'HK$',
      'NZD': 'NZ$',
      'SEK': 'kr',
      'NOK': 'kr',
      'DKK': 'kr'
    };

    return currencySymbols[currency] || currency;
  }

  /**
   * Get supported currencies
   */
  getSupportedCurrencies(): string[] {
    return [
      'USD', 'EUR', 'GBP', 'INR', 'CAD', 'AUD', 'JPY', 'SGD', 
      'AED', 'CHF', 'CNY', 'HKD', 'NZD', 'SEK', 'NOK', 'DKK'
    ];
  }

  /**
   * Calculate percentage change
   */
  calculatePercentageChange(current: number, previous: number): number {
    if (previous === 0) return 0;
    return ((current - previous) / previous) * 100;
  }

  /**
   * Get variance status color class
   */
  getVarianceStatusClass(variancePercentage: number): string {
    const absVariance = Math.abs(variancePercentage);
    
    if (absVariance <= 5) return 'text-success';
    if (absVariance <= 15) return 'text-warning';
    return 'text-danger';
  }

  /**
   * Get trend icon class
   */
  getTrendIconClass(trend: string): string {
    switch (trend.toLowerCase()) {
      case 'increasing':
        return 'fas fa-arrow-up text-success';
      case 'decreasing':
        return 'fas fa-arrow-down text-danger';
      case 'stable':
        return 'fas fa-minus text-warning';
      default:
        return 'fas fa-question text-muted';
    }
  }

  /**
   * Export financial report
   */
  exportReport(reportData: any, format: 'pdf' | 'excel' | 'csv', filename: string): void {
    // Implementation would depend on the specific export library used
    // This is a placeholder for the export functionality
    console.log(`Exporting ${filename} as ${format}`, reportData);
  }

  /**
   * Generate chart data for financial trends
   */
  generateChartData(data: any[], xField: string, yField: string, label: string): any {
    return {
      labels: data.map(item => item[xField]),
      datasets: [{
        label: label,
        data: data.map(item => item[yField]),
        borderColor: 'rgb(59, 130, 246)',
        backgroundColor: 'rgba(59, 130, 246, 0.1)',
        tension: 0.4,
        fill: true
      }]
    };
  }

  /**
   * Generate pie chart data for cost breakdown
   */
  generatePieChartData(data: { [key: string]: number }, colors?: string[]): any {
    const defaultColors = [
      '#3B82F6', '#10B981', '#F59E0B', '#EF4444', 
      '#8B5CF6', '#06B6D4', '#84CC16', '#F97316'
    ];

    return {
      labels: Object.keys(data),
      datasets: [{
        data: Object.values(data),
        backgroundColor: colors || defaultColors.slice(0, Object.keys(data).length),
        borderWidth: 2,
        borderColor: '#ffffff'
      }]
    };
  }
}