import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { FormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { of, throwError } from 'rxjs';

import { FinancialAnalyticsComponent } from './financial-analytics.component';
import { FinancialService } from '../../../services/financial.service';
import {
  FinancialSummaryReport,
  PayrollCostAnalysisReport,
  BudgetVarianceReport,
  CurrencyConversionReport,
  MonthlyFinancialTrendReport,
  FinancialDashboardData
} from '../../../models/financial.models';

describe('FinancialAnalyticsComponent', () => {
  let component: FinancialAnalyticsComponent;
  let fixture: ComponentFixture<FinancialAnalyticsComponent>;
  let financialService: jasmine.SpyObj<FinancialService>;

  const mockDashboardData: FinancialDashboardData = {
    totalPayrollCost: 1000000,
    monthlyGrowth: 2.5,
    currencyExposure: [],
    topDepartments: [],
    budgetVariance: -5.2,
    exchangeRateAlerts: []
  };

  const mockFinancialSummary: FinancialSummaryReport = {
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
    branchSummaries: [
      {
        branchId: 1,
        branchName: 'Main Branch',
        country: 'USA',
        localCurrency: 'USD',
        exchangeRate: 1,
        totalPayrollCost: 500000,
        totalPayrollCostInBaseCurrency: 500000,
        employeeCount: 50,
        averageSalary: 10000,
        currencyBreakdown: []
      }
    ],
    departmentSummaries: [
      {
        department: 'IT',
        totalCost: 400000,
        employeeCount: 40,
        averageSalary: 10000,
        percentageOfTotalCost: 40
      }
    ],
    monthlyBreakdown: []
  };

  const mockCostAnalysis: PayrollCostAnalysisReport = {
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
      allowanceBreakdown: {
        'House Rent Allowance': 150000,
        'Transport Allowance': 100000,
        'Medical Allowance': 50000
      },
      deductionBreakdown: {
        'Tax Deduction': 120000,
        'Provident Fund': 80000
      }
    },
    trendData: [],
    categoryAnalysis: [
      {
        category: 'Basic Salary',
        amount: 800000,
        percentage: 66.7,
        varianceFromBudget: -10000,
        status: 'On Track'
      }
    ]
  };

  const mockBudgetVariance: BudgetVarianceReport = {
    reportTitle: 'Budget Variance Report',
    generatedAt: new Date(),
    currency: 'USD',
    totalBudget: 1100000,
    totalActual: 1000000,
    totalVariance: -100000,
    variancePercentage: -9.09,
    varianceItems: [
      {
        category: 'Salary',
        budgetedAmount: 900000,
        actualAmount: 800000,
        variance: -100000,
        variancePercentage: -11.11,
        status: 'Under Budget',
        reason: 'Lower than expected costs'
      }
    ],
    departmentVariances: []
  };

  const mockCurrencyConversion: CurrencyConversionReport = {
    reportTitle: 'Currency Conversion Report',
    generatedAt: new Date(),
    baseCurrency: 'USD',
    conversionData: [
      {
        currency: 'EUR',
        currencySymbol: '€',
        currentRate: 0.85,
        totalAmountInCurrency: 85000,
        totalAmountInBaseCurrency: 100000,
        rateVariation: 2.5,
        trend: 'Increasing'
      }
    ],
    rateHistory: [],
    riskAnalysis: {
      totalExposure: 1000000,
      highestRiskCurrency: 50000,
      highestRiskCurrencyCode: 'EUR',
      averageVolatility: 0.05,
      currencyRisks: [
        {
          currency: 'EUR',
          exposure: 50000,
          volatility: 0.05,
          riskLevel: 'Medium',
          recommendation: 'Monitor closely'
        }
      ]
    }
  };

  const mockMonthlyTrends: MonthlyFinancialTrendReport = {
    reportTitle: 'Monthly Financial Trend Report',
    generatedAt: new Date(),
    currency: 'USD',
    trendType: 'Cost',
    trendData: [
      {
        year: 2024,
        month: 1,
        monthName: 'January',
        value: 80000,
        percentageChange: 0,
        movingAverage: 80000
      },
      {
        year: 2024,
        month: 2,
        monthName: 'February',
        value: 85000,
        percentageChange: 6.25,
        movingAverage: 82500
      }
    ],
    analysis: {
      overallTrend: 'Increasing',
      averageGrowthRate: 3.2,
      highestValue: 120000,
      lowestValue: 80000,
      highestMonth: 'December 2024',
      lowestMonth: 'January 2024',
      insights: ['Payroll costs are growing steadily']
    }
  };

  beforeEach(async () => {
    const financialServiceSpy = jasmine.createSpyObj('FinancialService', [
      'getFinancialDashboardData',
      'generateFinancialSummary',
      'generatePayrollCostAnalysis',
      'generateBudgetVariance',
      'generateCurrencyConversion',
      'generateMonthlyTrend',
      'getSupportedCurrencies',
      'formatCurrency',
      'getCurrencySymbol',
      'getVarianceStatusClass',
      'getTrendIconClass',
      'exportReport'
    ]);

    await TestBed.configureTestingModule({
      imports: [
        FinancialAnalyticsComponent,
        HttpClientTestingModule,
        FormsModule,
        NgbModule
      ],
      providers: [
        { provide: FinancialService, useValue: financialServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(FinancialAnalyticsComponent);
    component = fixture.componentInstance;
    financialService = TestBed.inject(FinancialService) as jasmine.SpyObj<FinancialService>;

    // Setup default spy returns
    financialService.getFinancialDashboardData.and.returnValue(of(mockDashboardData));
    financialService.getSupportedCurrencies.and.returnValue(['USD', 'EUR', 'GBP', 'INR']);
    financialService.formatCurrency.and.returnValue('$1,000.00');
    financialService.getCurrencySymbol.and.returnValue('$');
    financialService.getVarianceStatusClass.and.returnValue('text-success');
    financialService.getTrendIconClass.and.returnValue('fas fa-arrow-up text-success');
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with default filters', () => {
    expect(component.filters.currency).toBe('USD');
    expect(component.filters.branchId).toBeNull();
    expect(component.filters.departments).toEqual([]);
  });

  it('should load initial data on init', () => {
    component.ngOnInit();

    expect(financialService.getFinancialDashboardData).toHaveBeenCalled();
    expect(component.supportedCurrencies).toEqual(['USD', 'EUR', 'GBP', 'INR']);
  });

  it('should handle dashboard data loading success', () => {
    component.ngOnInit();

    expect(component.dashboardData).toEqual(mockDashboardData);
    expect(component.loading).toBeFalse();
  });

  it('should handle dashboard data loading error', () => {
    financialService.getFinancialDashboardData.and.returnValue(throwError('Error'));
    spyOn(console, 'error');

    component.ngOnInit();

    expect(component.loading).toBeFalse();
    expect(console.error).toHaveBeenCalledWith('Error loading dashboard data:', 'Error');
  });

  it('should generate all reports when generateReports is called', () => {
    // Setup spy returns for all report generation methods
    financialService.generateFinancialSummary.and.returnValue(of(mockFinancialSummary));
    financialService.generatePayrollCostAnalysis.and.returnValue(of(mockCostAnalysis));
    financialService.generateBudgetVariance.and.returnValue(of(mockBudgetVariance));
    financialService.generateCurrencyConversion.and.returnValue(of(mockCurrencyConversion));
    financialService.generateMonthlyTrend.and.returnValue(of(mockMonthlyTrends));

    component.generateReports();

    expect(component.loading).toBeTrue();
    expect(financialService.generateFinancialSummary).toHaveBeenCalled();
    expect(financialService.generatePayrollCostAnalysis).toHaveBeenCalled();
    expect(financialService.generateBudgetVariance).toHaveBeenCalled();
    expect(financialService.generateCurrencyConversion).toHaveBeenCalled();
    expect(financialService.generateMonthlyTrend).toHaveBeenCalled();
  });

  it('should handle report generation success', () => {
    financialService.generateFinancialSummary.and.returnValue(of(mockFinancialSummary));
    financialService.generatePayrollCostAnalysis.and.returnValue(of(mockCostAnalysis));
    financialService.generateBudgetVariance.and.returnValue(of(mockBudgetVariance));
    financialService.generateCurrencyConversion.and.returnValue(of(mockCurrencyConversion));
    financialService.generateMonthlyTrend.and.returnValue(of(mockMonthlyTrends));

    component.generateReports();

    expect(component.financialSummary).toEqual(mockFinancialSummary);
    expect(component.costAnalysis).toEqual(mockCostAnalysis);
    expect(component.budgetVariance).toEqual(mockBudgetVariance);
    expect(component.currencyConversion).toEqual(mockCurrencyConversion);
    expect(component.monthlyTrends).toEqual(mockMonthlyTrends);
    expect(component.loading).toBeFalse();
  });

  it('should handle report generation error', () => {
    financialService.generateFinancialSummary.and.returnValue(throwError('Error'));
    financialService.generatePayrollCostAnalysis.and.returnValue(of(mockCostAnalysis));
    financialService.generateBudgetVariance.and.returnValue(of(mockBudgetVariance));
    financialService.generateCurrencyConversion.and.returnValue(of(mockCurrencyConversion));
    financialService.generateMonthlyTrend.and.returnValue(of(mockMonthlyTrends));
    spyOn(console, 'error');

    component.generateReports();

    expect(component.loading).toBeFalse();
    expect(console.error).toHaveBeenCalledWith('Error generating reports:', 'Error');
  });

  it('should refresh data when refreshData is called', () => {
    component.financialSummary = mockFinancialSummary;
    
    component.refreshData();

    expect(financialService.getFinancialDashboardData).toHaveBeenCalled();
  });

  it('should export report with correct parameters', () => {
    component.financialSummary = mockFinancialSummary;
    component.costAnalysis = mockCostAnalysis;
    component.budgetVariance = mockBudgetVariance;
    component.currencyConversion = mockCurrencyConversion;
    component.monthlyTrends = mockMonthlyTrends;

    component.exportReport('pdf');

    expect(financialService.exportReport).toHaveBeenCalledWith(
      jasmine.objectContaining({
        summary: mockFinancialSummary,
        costAnalysis: mockCostAnalysis,
        budgetVariance: mockBudgetVariance,
        currencyConversion: mockCurrencyConversion,
        monthlyTrends: mockMonthlyTrends
      }),
      'pdf',
      jasmine.stringMatching(/financial-report-\d{4}-\d{2}-\d{2}/)
    );
  });

  it('should format currency correctly', () => {
    const result = component.formatCurrency(1000, 'USD');
    
    expect(financialService.formatCurrency).toHaveBeenCalledWith(1000, 'USD');
    expect(result).toBe('$1,000.00');
  });

  it('should get currency symbol correctly', () => {
    const result = component.getCurrencySymbol('USD');
    
    expect(financialService.getCurrencySymbol).toHaveBeenCalledWith('USD');
    expect(result).toBe('$');
  });

  it('should get variance class correctly', () => {
    const result = component.getVarianceClass(5);
    
    expect(financialService.getVarianceStatusClass).toHaveBeenCalledWith(5);
    expect(result).toBe('text-success');
  });

  it('should get trend icon for string trend', () => {
    const result = component.getTrendIcon('increasing');
    
    expect(financialService.getTrendIconClass).toHaveBeenCalledWith('increasing');
    expect(result).toBe('fas fa-arrow-up text-success');
  });

  it('should get trend icon for numeric trend', () => {
    expect(component.getTrendIcon(5)).toBe('fas fa-arrow-up text-success');
    expect(component.getTrendIcon(-5)).toBe('fas fa-arrow-down text-danger');
    expect(component.getTrendIcon(0)).toBe('fas fa-minus text-warning');
  });

  it('should get correct status badge class', () => {
    expect(component.getStatusBadgeClass('On Track')).toBe('bg-success');
    expect(component.getStatusBadgeClass('Over Budget')).toBe('bg-warning');
    expect(component.getStatusBadgeClass('Under Budget')).toBe('bg-info');
    expect(component.getStatusBadgeClass('Significantly Over Budget')).toBe('bg-danger');
    expect(component.getStatusBadgeClass('Unknown')).toBe('bg-secondary');
  });

  it('should get correct risk badge class', () => {
    expect(component.getRiskBadgeClass('Low')).toBe('bg-success');
    expect(component.getRiskBadgeClass('Medium')).toBe('bg-warning');
    expect(component.getRiskBadgeClass('High')).toBe('bg-danger');
    expect(component.getRiskBadgeClass('Unknown')).toBe('bg-secondary');
  });

  it('should update filters correctly', () => {
    const newStartDate = '2024-06-01';
    const newEndDate = '2024-12-31';
    const newCurrency = 'EUR';
    const newBranchId = 2;
    const newDepartments = ['IT', 'HR'];

    component.filters.startDate = newStartDate;
    component.filters.endDate = newEndDate;
    component.filters.currency = newCurrency;
    component.filters.branchId = newBranchId;
    component.filters.departments = newDepartments;

    expect(component.filters.startDate).toBe(newStartDate);
    expect(component.filters.endDate).toBe(newEndDate);
    expect(component.filters.currency).toBe(newCurrency);
    expect(component.filters.branchId).toBe(newBranchId);
    expect(component.filters.departments).toEqual(newDepartments);
  });

  it('should cleanup subscriptions on destroy', () => {
    spyOn(component['destroy$'], 'next');
    spyOn(component['destroy$'], 'complete');

    component.ngOnDestroy();

    expect(component['destroy$'].next).toHaveBeenCalled();
    expect(component['destroy$'].complete).toHaveBeenCalled();
  });

  it('should display loading state correctly', () => {
    component.loading = true;
    fixture.detectChanges();

    const loadingElement = fixture.nativeElement.querySelector('.spinner-border');
    expect(loadingElement).toBeTruthy();
  });

  it('should display dashboard cards when data is available', () => {
    component.loading = false;
    component.dashboardData = mockDashboardData;
    fixture.detectChanges();

    const cards = fixture.nativeElement.querySelectorAll('.card.bg-primary, .card.bg-success, .card.bg-info, .card.bg-warning');
    expect(cards.length).toBe(4);
  });

  it('should display financial summary data in table', () => {
    component.loading = false;
    component.financialSummary = mockFinancialSummary;
    fixture.detectChanges();

    // Check if branch summary table is rendered
    const table = fixture.nativeElement.querySelector('table');
    expect(table).toBeTruthy();
  });

  it('should handle empty data gracefully', () => {
    component.loading = false;
    component.financialSummary = null;
    component.costAnalysis = null;
    component.budgetVariance = null;
    component.currencyConversion = null;
    component.monthlyTrends = null;
    fixture.detectChanges();

    // Should not throw errors and should display tabs
    const tabset = fixture.nativeElement.querySelector('ngb-tabset');
    expect(tabset).toBeTruthy();
  });
});