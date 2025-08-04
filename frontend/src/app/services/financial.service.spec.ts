import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { FinancialService } from './financial.service';
import {
  FinancialReportRequest,
  FinancialSummaryReport,
  PayrollCostAnalysisRequest,
  PayrollCostAnalysisReport,
  BudgetVarianceRequest,
  BudgetVarianceReport,
  CurrencyConversionRequest,
  CurrencyConversionReport,
  MonthlyTrendRequest,
  MonthlyFinancialTrendReport,
  FinancialMetric,
  CurrencyExchangeRate
} from '../models/financial.models';
import { environment } from '../../environments/environment';

describe('FinancialService', () => {
  let service: FinancialService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/financial-reports`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [FinancialService]
    });
    service = TestBed.inject(FinancialService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('generateFinancialSummary', () => {
    it('should generate financial summary report', () => {
      const mockRequest: FinancialReportRequest = {
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-12-31'),
        currency: 'USD',
        branchId: 1
      };

      const mockResponse: FinancialSummaryReport = {
        reportTitle: 'Financial Summary Report',
        generatedAt: new Date(),
        currency: 'USD',
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-12-31'),
        totalPayrollCost: 1000000,
        totalGrossSalary: 1200000,
        totalNetSalary: 1000000,
        totalDeductions: 200000,
        totalAllowances: 300000,
        totalOvertimeCost: 50000,
        totalEmployees: 100,
        averageGrossSalary: 12000,
        averageNetSalary: 10000,
        branchSummaries: [],
        departmentSummaries: [],
        monthlyBreakdown: []
      };

      service.generateFinancialSummary(mockRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${apiUrl}/summary`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockRequest);
      req.flush(mockResponse);
    });
  });

  describe('generatePayrollCostAnalysis', () => {
    it('should generate payroll cost analysis report', () => {
      const mockRequest: PayrollCostAnalysisRequest = {
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-12-31'),
        currency: 'USD',
        includeProjections: true
      };

      const mockResponse: PayrollCostAnalysisReport = {
        reportTitle: 'Payroll Cost Analysis Report',
        generatedAt: new Date(),
        currency: 'USD',
        costBreakdown: {
          basicSalaryTotal: 800000,
          allowancesTotal: 300000,
          overtimeTotal: 50000,
          deductionsTotal: 200000,
          grossTotal: 1200000,
          netTotal: 1000000,
          allowanceBreakdown: {},
          deductionBreakdown: {}
        },
        trendData: [],
        categoryAnalysis: []
      };

      service.generatePayrollCostAnalysis(mockRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${apiUrl}/payroll-cost-analysis`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockRequest);
      req.flush(mockResponse);
    });
  });

  describe('generateBudgetVariance', () => {
    it('should generate budget variance report', () => {
      const mockRequest: BudgetVarianceRequest = {
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-12-31'),
        currency: 'USD'
      };

      const mockResponse: BudgetVarianceReport = {
        reportTitle: 'Budget Variance Report',
        generatedAt: new Date(),
        currency: 'USD',
        totalBudget: 1100000,
        totalActual: 1000000,
        totalVariance: -100000,
        variancePercentage: -9.09,
        varianceItems: [],
        departmentVariances: []
      };

      service.generateBudgetVariance(mockRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${apiUrl}/budget-variance`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockRequest);
      req.flush(mockResponse);
    });
  });

  describe('generateCurrencyConversion', () => {
    it('should generate currency conversion report', () => {
      const mockRequest: CurrencyConversionRequest = {
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-12-31'),
        baseCurrency: 'USD',
        includeHistoricalRates: true
      };

      const mockResponse: CurrencyConversionReport = {
        reportTitle: 'Currency Conversion Report',
        generatedAt: new Date(),
        baseCurrency: 'USD',
        conversionData: [],
        rateHistory: [],
        riskAnalysis: {
          totalExposure: 1000000,
          highestRiskCurrency: 50000,
          highestRiskCurrencyCode: 'EUR',
          averageVolatility: 0.05,
          currencyRisks: []
        }
      };

      service.generateCurrencyConversion(mockRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${apiUrl}/currency-conversion`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockRequest);
      req.flush(mockResponse);
    });
  });

  describe('generateMonthlyTrend', () => {
    it('should generate monthly trend report', () => {
      const mockRequest: MonthlyTrendRequest = {
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-12-31'),
        currency: 'USD',
        trendType: 'Cost'
      };

      const mockResponse: MonthlyFinancialTrendReport = {
        reportTitle: 'Monthly Financial Trend Report',
        generatedAt: new Date(),
        currency: 'USD',
        trendType: 'Cost',
        trendData: [],
        analysis: {
          overallTrend: 'Increasing',
          averageGrowthRate: 2.5,
          highestValue: 120000,
          lowestValue: 80000,
          highestMonth: 'December 2024',
          lowestMonth: 'January 2024',
          insights: []
        }
      };

      service.generateMonthlyTrend(mockRequest).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${apiUrl}/monthly-trend`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockRequest);
      req.flush(mockResponse);
    });
  });

  describe('getFinancialMetrics', () => {
    it('should get financial metrics', () => {
      const branchId = 1;
      const startDate = new Date('2024-01-01');
      const endDate = new Date('2024-12-31');

      const mockResponse: FinancialMetric[] = [
        {
          metricName: 'Total Payroll Cost',
          value: 1000000,
          unit: 'USD',
          date: new Date(),
          category: 'Payroll'
        }
      ];

      service.getFinancialMetrics(branchId, startDate, endDate).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(
        `${apiUrl}/metrics?branchId=${branchId}&startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('getExchangeRateHistory', () => {
    it('should get exchange rate history', () => {
      const baseCurrency = 'USD';
      const startDate = new Date('2024-01-01');
      const endDate = new Date('2024-12-31');

      const mockResponse: CurrencyExchangeRate[] = [
        {
          date: new Date('2024-01-01'),
          baseCurrency: 'USD',
          targetCurrency: 'EUR',
          rate: 0.85,
          change: 0.01,
          percentageChange: 1.18
        }
      ];

      service.getExchangeRateHistory(baseCurrency, startDate, endDate).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(
        `${apiUrl}/exchange-rate-history?baseCurrency=${baseCurrency}&startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('formatCurrency', () => {
    it('should format currency with symbol', () => {
      const result = service.formatCurrency(1234.56, 'USD', true);
      expect(result).toBe('$1,234.56');
    });

    it('should format currency without symbol', () => {
      const result = service.formatCurrency(1234.56, 'USD', false);
      expect(result).toBe('1,234.56');
    });

    it('should format currency with different currencies', () => {
      expect(service.formatCurrency(1000, 'EUR', true)).toBe('€1,000.00');
      expect(service.formatCurrency(1000, 'GBP', true)).toBe('£1,000.00');
      expect(service.formatCurrency(1000, 'INR', true)).toBe('₹1,000.00');
    });
  });

  describe('convertCurrency', () => {
    it('should convert currency with exchange rate', () => {
      const result = service.convertCurrency(100, 'USD', 'EUR', 0.85);
      expect(result).toBe(85);
    });

    it('should return same amount for same currency', () => {
      const result = service.convertCurrency(100, 'USD', 'USD', 1);
      expect(result).toBe(100);
    });
  });

  describe('getCurrencySymbol', () => {
    it('should return correct currency symbols', () => {
      expect(service.getCurrencySymbol('USD')).toBe('$');
      expect(service.getCurrencySymbol('EUR')).toBe('€');
      expect(service.getCurrencySymbol('GBP')).toBe('£');
      expect(service.getCurrencySymbol('INR')).toBe('₹');
    });

    it('should return currency code for unknown currencies', () => {
      expect(service.getCurrencySymbol('XYZ')).toBe('XYZ');
    });
  });

  describe('getSupportedCurrencies', () => {
    it('should return list of supported currencies', () => {
      const currencies = service.getSupportedCurrencies();
      expect(currencies).toContain('USD');
      expect(currencies).toContain('EUR');
      expect(currencies).toContain('GBP');
      expect(currencies).toContain('INR');
      expect(currencies.length).toBeGreaterThan(10);
    });
  });

  describe('calculatePercentageChange', () => {
    it('should calculate positive percentage change', () => {
      const result = service.calculatePercentageChange(120, 100);
      expect(result).toBe(20);
    });

    it('should calculate negative percentage change', () => {
      const result = service.calculatePercentageChange(80, 100);
      expect(result).toBe(-20);
    });

    it('should return 0 when previous value is 0', () => {
      const result = service.calculatePercentageChange(100, 0);
      expect(result).toBe(0);
    });
  });

  describe('getVarianceStatusClass', () => {
    it('should return success class for low variance', () => {
      expect(service.getVarianceStatusClass(3)).toBe('text-success');
      expect(service.getVarianceStatusClass(-3)).toBe('text-success');
    });

    it('should return warning class for medium variance', () => {
      expect(service.getVarianceStatusClass(10)).toBe('text-warning');
      expect(service.getVarianceStatusClass(-10)).toBe('text-warning');
    });

    it('should return danger class for high variance', () => {
      expect(service.getVarianceStatusClass(20)).toBe('text-danger');
      expect(service.getVarianceStatusClass(-20)).toBe('text-danger');
    });
  });

  describe('getTrendIconClass', () => {
    it('should return correct icon classes for trends', () => {
      expect(service.getTrendIconClass('increasing')).toBe('fas fa-arrow-up text-success');
      expect(service.getTrendIconClass('decreasing')).toBe('fas fa-arrow-down text-danger');
      expect(service.getTrendIconClass('stable')).toBe('fas fa-minus text-warning');
      expect(service.getTrendIconClass('unknown')).toBe('fas fa-question text-muted');
    });
  });

  describe('generateChartData', () => {
    it('should generate chart data correctly', () => {
      const data = [
        { month: 'Jan', cost: 1000 },
        { month: 'Feb', cost: 1200 },
        { month: 'Mar', cost: 1100 }
      ];

      const result = service.generateChartData(data, 'month', 'cost', 'Monthly Cost');

      expect(result.labels).toEqual(['Jan', 'Feb', 'Mar']);
      expect(result.datasets[0].label).toBe('Monthly Cost');
      expect(result.datasets[0].data).toEqual([1000, 1200, 1100]);
    });
  });

  describe('generatePieChartData', () => {
    it('should generate pie chart data correctly', () => {
      const data = {
        'Basic Salary': 800000,
        'Allowances': 200000,
        'Overtime': 50000
      };

      const result = service.generatePieChartData(data);

      expect(result.labels).toEqual(['Basic Salary', 'Allowances', 'Overtime']);
      expect(result.datasets[0].data).toEqual([800000, 200000, 50000]);
      expect(result.datasets[0].backgroundColor).toBeDefined();
    });

    it('should use custom colors when provided', () => {
      const data = { 'Category 1': 100, 'Category 2': 200 };
      const colors = ['#FF0000', '#00FF00'];

      const result = service.generatePieChartData(data, colors);

      expect(result.datasets[0].backgroundColor).toEqual(colors);
    });
  });
});