using Microsoft.Extensions.Logging;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Financial;
using System.Globalization;

namespace StrideHR.Infrastructure.Services;

public class FinancialReportingService : IFinancialReportingService
{
    private readonly IPayrollRepository _payrollRepository;
    private readonly IBudgetRepository _budgetRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<Branch> _branchRepository;
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<FinancialReportingService> _logger;

    public FinancialReportingService(
        IPayrollRepository payrollRepository,
        IBudgetRepository budgetRepository,
        IRepository<Employee> employeeRepository,
        IRepository<Branch> branchRepository,
        ICurrencyService currencyService,
        ILogger<FinancialReportingService> logger)
    {
        _payrollRepository = payrollRepository;
        _budgetRepository = budgetRepository;
        _employeeRepository = employeeRepository;
        _branchRepository = branchRepository;
        _currencyService = currencyService;
        _logger = logger;
    }

    public async Task<FinancialSummaryReport> GenerateFinancialSummaryAsync(FinancialReportRequest request)
    {
        try
        {
            _logger.LogInformation("Generating financial summary report for period {StartDate} to {EndDate}", 
                request.StartDate, request.EndDate);

            var report = new FinancialSummaryReport
            {
                ReportTitle = "Financial Summary Report",
                GeneratedAt = DateTime.UtcNow,
                Currency = request.Currency ?? "USD",
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            // Get payroll records for the period
            var payrollRecords = await GetPayrollRecordsForPeriodAsync(request);

            // Calculate overall totals
            await CalculateOverallTotalsAsync(report, payrollRecords);

            // Generate branch summaries
            report.BranchSummaries = await GenerateBranchSummariesAsync(payrollRecords, report.Currency);

            // Generate department summaries
            report.DepartmentSummaries = await GenerateDepartmentSummariesAsync(payrollRecords);

            // Generate monthly breakdown
            report.MonthlyBreakdown = await GenerateMonthlyBreakdownAsync(payrollRecords);

            _logger.LogInformation("Financial summary report generated successfully");
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating financial summary report");
            throw;
        }
    }

    public async Task<PayrollCostAnalysisReport> GeneratePayrollCostAnalysisAsync(PayrollCostAnalysisRequest request)
    {
        try
        {
            _logger.LogInformation("Generating payroll cost analysis report");

            var report = new PayrollCostAnalysisReport
            {
                ReportTitle = "Payroll Cost Analysis Report",
                GeneratedAt = DateTime.UtcNow,
                Currency = request.Currency ?? "USD"
            };

            var payrollRecords = await GetPayrollRecordsForPeriodAsync(
                new FinancialReportRequest
                {
                    BranchId = request.BranchId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Currency = request.Currency,
                    Departments = request.Departments
                });

            // Generate cost breakdown
            report.CostBreakdown = await GeneratePayrollCostBreakdownAsync(payrollRecords);

            // Generate trend data
            report.TrendData = await GeneratePayrollTrendDataAsync(payrollRecords);

            // Generate category analysis
            report.CategoryAnalysis = await GenerateCostCategoryAnalysisAsync(payrollRecords);

            // Generate projection if requested
            if (request.IncludeProjections)
            {
                report.Projection = await GeneratePayrollProjectionAsync(payrollRecords);
            }

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating payroll cost analysis report");
            throw;
        }
    }

    public async Task<BudgetVarianceReport> GenerateBudgetVarianceReportAsync(BudgetVarianceRequest request)
    {
        try
        {
            _logger.LogInformation("Generating budget variance report");

            var report = new BudgetVarianceReport
            {
                ReportTitle = "Budget Variance Report",
                GeneratedAt = DateTime.UtcNow,
                Currency = request.Currency ?? "USD"
            };

            // Get budget data
            var budgets = await GetBudgetsForPeriodAsync(request);
            var payrollRecords = await GetPayrollRecordsForPeriodAsync(
                new FinancialReportRequest
                {
                    BranchId = request.BranchId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Currency = request.Currency,
                    Departments = request.Departments
                });

            // Calculate totals
            report.TotalBudget = budgets.Sum(b => b.BudgetedAmount);
            report.TotalActual = payrollRecords.Sum(p => p.NetSalary);
            report.TotalVariance = report.TotalActual - report.TotalBudget;
            report.VariancePercentage = report.TotalBudget != 0 
                ? (report.TotalVariance / report.TotalBudget) * 100 
                : 0;

            // Generate variance items
            report.VarianceItems = await GenerateBudgetVarianceItemsAsync(budgets, payrollRecords);

            // Generate department variances
            report.DepartmentVariances = await GenerateDepartmentBudgetVariancesAsync(budgets, payrollRecords);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating budget variance report");
            throw;
        }
    }

    public async Task<CurrencyConversionReport> GenerateCurrencyConversionReportAsync(CurrencyConversionRequest request)
    {
        try
        {
            _logger.LogInformation("Generating currency conversion report");

            var report = new CurrencyConversionReport
            {
                ReportTitle = "Currency Conversion Report",
                GeneratedAt = DateTime.UtcNow,
                BaseCurrency = request.BaseCurrency
            };

            // Get all payroll records with different currencies
            var payrollRecords = await _payrollRepository.GetAllAsync();
            var relevantRecords = payrollRecords.Where(p => 
                p.PayrollPeriodStart >= request.StartDate && 
                p.PayrollPeriodEnd <= request.EndDate).ToList();

            // Generate conversion data
            report.ConversionData = await GenerateCurrencyConversionDataAsync(relevantRecords, request.BaseCurrency);

            // Generate rate history if requested
            if (request.IncludeHistoricalRates)
            {
                report.RateHistory = await GenerateExchangeRateHistoryAsync(request);
            }

            // Generate risk analysis
            report.RiskAnalysis = await GenerateCurrencyRiskAnalysisAsync(relevantRecords, request.BaseCurrency);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating currency conversion report");
            throw;
        }
    }

    public async Task<DepartmentWiseFinancialReport> GenerateDepartmentWiseReportAsync(DepartmentFinancialRequest request)
    {
        try
        {
            _logger.LogInformation("Generating department-wise financial report");

            var report = new DepartmentWiseFinancialReport
            {
                ReportTitle = "Department-wise Financial Report",
                GeneratedAt = DateTime.UtcNow,
                Currency = request.Currency ?? "USD"
            };

            var payrollRecords = await GetPayrollRecordsForPeriodAsync(
                new FinancialReportRequest
                {
                    BranchId = request.BranchId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Currency = request.Currency,
                    Departments = request.Departments
                });

            // Generate department details
            report.DepartmentDetails = await GenerateDepartmentFinancialDetailsAsync(payrollRecords);

            // Generate comparison
            report.Comparison = await GenerateDepartmentComparisonAsync(report.DepartmentDetails);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating department-wise financial report");
            throw;
        }
    }

    public async Task<MonthlyFinancialTrendReport> GenerateMonthlyTrendReportAsync(MonthlyTrendRequest request)
    {
        try
        {
            _logger.LogInformation("Generating monthly financial trend report");

            var report = new MonthlyFinancialTrendReport
            {
                ReportTitle = "Monthly Financial Trend Report",
                GeneratedAt = DateTime.UtcNow,
                Currency = request.Currency ?? "USD",
                TrendType = request.TrendType
            };

            var payrollRecords = await GetPayrollRecordsForPeriodAsync(
                new FinancialReportRequest
                {
                    BranchId = request.BranchId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Currency = request.Currency
                });

            // Generate trend data
            report.TrendData = await GenerateMonthlyTrendDataAsync(payrollRecords, request.TrendType);

            // Generate analysis
            report.Analysis = await GenerateTrendAnalysisAsync(report.TrendData);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating monthly trend report");
            throw;
        }
    }

    public async Task<List<FinancialMetric>> GetFinancialMetricsAsync(int branchId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var metrics = new List<FinancialMetric>();
            
            var payrollRecords = await _payrollRepository.GetByBranchAndPeriodAsync(branchId, startDate.Year, startDate.Month);
            
            if (payrollRecords.Any())
            {
                metrics.Add(new FinancialMetric
                {
                    MetricName = "Total Payroll Cost",
                    Value = payrollRecords.Sum(p => p.NetSalary),
                    Unit = payrollRecords.First().Currency,
                    Date = DateTime.UtcNow,
                    Category = "Payroll"
                });

                metrics.Add(new FinancialMetric
                {
                    MetricName = "Average Salary",
                    Value = payrollRecords.Average(p => p.NetSalary),
                    Unit = payrollRecords.First().Currency,
                    Date = DateTime.UtcNow,
                    Category = "Payroll"
                });

                metrics.Add(new FinancialMetric
                {
                    MetricName = "Employee Count",
                    Value = payrollRecords.Count,
                    Unit = "Count",
                    Date = DateTime.UtcNow,
                    Category = "Headcount"
                });
            }

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting financial metrics");
            throw;
        }
    }

    public async Task<List<CurrencyExchangeRate>> GetExchangeRateHistoryAsync(string baseCurrency, DateTime startDate, DateTime endDate)
    {
        try
        {
            var rates = new List<CurrencyExchangeRate>();
            var supportedCurrencies = await _currencyService.GetSupportedCurrenciesAsync();

            foreach (var currency in supportedCurrencies.Where(c => c != baseCurrency))
            {
                // In a real implementation, this would fetch historical data from an external API
                // For now, we'll generate sample data
                var currentDate = startDate;
                while (currentDate <= endDate)
                {
                    var rate = await _currencyService.GetExchangeRateAsync(baseCurrency, currency);
                    rates.Add(new CurrencyExchangeRate
                    {
                        Date = currentDate,
                        BaseCurrency = baseCurrency,
                        TargetCurrency = currency,
                        Rate = rate,
                        Change = 0, // Would be calculated from previous day
                        PercentageChange = 0
                    });

                    currentDate = currentDate.AddDays(1);
                }
            }

            return rates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exchange rate history");
            throw;
        }
    }

    // Private helper methods
    private async Task<List<PayrollRecord>> GetPayrollRecordsForPeriodAsync(FinancialReportRequest request)
    {
        var allRecords = await _payrollRepository.GetAllAsync();
        var filteredRecords = allRecords.Where(p => 
            p.PayrollPeriodStart >= request.StartDate && 
            p.PayrollPeriodEnd <= request.EndDate);

        if (request.BranchId.HasValue)
        {
            filteredRecords = filteredRecords.Where(p => p.Employee.BranchId == request.BranchId.Value);
        }

        if (request.Departments?.Any() == true)
        {
            filteredRecords = filteredRecords.Where(p => request.Departments.Contains(p.Employee.Department));
        }

        return filteredRecords.ToList();
    }

    private async Task<List<Budget>> GetBudgetsForPeriodAsync(BudgetVarianceRequest request)
    {
        var allBudgets = await _budgetRepository.GetAllAsync();
        var filteredBudgets = allBudgets.Where(b => 
            new DateTime(b.Year, b.Month, 1) >= request.StartDate && 
            new DateTime(b.Year, b.Month, 1) <= request.EndDate);

        if (request.BranchId.HasValue)
        {
            filteredBudgets = filteredBudgets.Where(b => b.BranchId == request.BranchId.Value);
        }

        if (request.Departments?.Any() == true)
        {
            filteredBudgets = filteredBudgets.Where(b => request.Departments.Contains(b.Department));
        }

        return filteredBudgets.ToList();
    }

    private async Task CalculateOverallTotalsAsync(FinancialSummaryReport report, List<PayrollRecord> payrollRecords)
    {
        if (!payrollRecords.Any()) return;

        var baseCurrency = report.Currency;
        
        foreach (var record in payrollRecords)
        {
            var grossSalary = record.Currency == baseCurrency 
                ? record.GrossSalary 
                : await _currencyService.ConvertCurrencyAsync(record.GrossSalary, record.Currency, baseCurrency);

            var netSalary = record.Currency == baseCurrency 
                ? record.NetSalary 
                : await _currencyService.ConvertCurrencyAsync(record.NetSalary, record.Currency, baseCurrency);

            var deductions = record.Currency == baseCurrency 
                ? record.TotalDeductions 
                : await _currencyService.ConvertCurrencyAsync(record.TotalDeductions, record.Currency, baseCurrency);

            var allowances = record.Currency == baseCurrency 
                ? record.TotalAllowances 
                : await _currencyService.ConvertCurrencyAsync(record.TotalAllowances, record.Currency, baseCurrency);

            var overtimeCost = record.Currency == baseCurrency 
                ? record.OvertimeAmount 
                : await _currencyService.ConvertCurrencyAsync(record.OvertimeAmount, record.Currency, baseCurrency);

            report.TotalGrossSalary += grossSalary;
            report.TotalNetSalary += netSalary;
            report.TotalDeductions += deductions;
            report.TotalAllowances += allowances;
            report.TotalOvertimeCost += overtimeCost;
        }

        report.TotalPayrollCost = report.TotalNetSalary;
        report.TotalEmployees = payrollRecords.Select(p => p.EmployeeId).Distinct().Count();
        report.AverageGrossSalary = report.TotalEmployees > 0 ? report.TotalGrossSalary / report.TotalEmployees : 0;
        report.AverageNetSalary = report.TotalEmployees > 0 ? report.TotalNetSalary / report.TotalEmployees : 0;
    }

    private async Task<List<BranchFinancialSummary>> GenerateBranchSummariesAsync(List<PayrollRecord> payrollRecords, string baseCurrency)
    {
        var branchSummaries = new List<BranchFinancialSummary>();
        var branchGroups = payrollRecords.GroupBy(p => p.Employee.BranchId);

        foreach (var branchGroup in branchGroups)
        {
            var branch = branchGroup.First().Employee.Branch;
            var records = branchGroup.ToList();

            var summary = new BranchFinancialSummary
            {
                BranchId = branch.Id,
                BranchName = branch.Name,
                Country = branch.Country,
                LocalCurrency = branch.Currency,
                ExchangeRate = await _currencyService.GetExchangeRateAsync(branch.Currency, baseCurrency),
                EmployeeCount = records.Select(r => r.EmployeeId).Distinct().Count()
            };

            // Calculate totals in local currency
            summary.TotalPayrollCost = records.Sum(r => r.NetSalary);
            
            // Convert to base currency
            summary.TotalPayrollCostInBaseCurrency = branch.Currency == baseCurrency 
                ? summary.TotalPayrollCost 
                : await _currencyService.ConvertCurrencyAsync(summary.TotalPayrollCost, branch.Currency, baseCurrency);

            summary.AverageSalary = summary.EmployeeCount > 0 ? summary.TotalPayrollCost / summary.EmployeeCount : 0;

            // Generate currency breakdown
            var currencyGroups = records.GroupBy(r => r.Currency);
            foreach (var currencyGroup in currencyGroups)
            {
                var currencyRecords = currencyGroup.ToList();
                var currencyCode = currencyGroup.Key;
                
                summary.CurrencyBreakdown.Add(new CurrencyBreakdown
                {
                    Currency = currencyCode,
                    CurrencySymbol = await _currencyService.GetCurrencySymbolAsync(currencyCode),
                    Amount = currencyRecords.Sum(r => r.NetSalary),
                    AmountInBaseCurrency = currencyCode == baseCurrency 
                        ? currencyRecords.Sum(r => r.NetSalary)
                        : await _currencyService.ConvertCurrencyAsync(currencyRecords.Sum(r => r.NetSalary), currencyCode, baseCurrency),
                    ExchangeRate = await _currencyService.GetExchangeRateAsync(currencyCode, baseCurrency),
                    EmployeeCount = currencyRecords.Select(r => r.EmployeeId).Distinct().Count()
                });
            }

            branchSummaries.Add(summary);
        }

        return branchSummaries;
    }

    private async Task<List<DepartmentFinancialSummary>> GenerateDepartmentSummariesAsync(List<PayrollRecord> payrollRecords)
    {
        var departmentSummaries = new List<DepartmentFinancialSummary>();
        var departmentGroups = payrollRecords.GroupBy(p => p.Employee.Department);
        var totalCost = payrollRecords.Sum(p => p.NetSalary);

        foreach (var departmentGroup in departmentGroups)
        {
            var records = departmentGroup.ToList();
            var departmentCost = records.Sum(r => r.NetSalary);

            departmentSummaries.Add(new DepartmentFinancialSummary
            {
                Department = departmentGroup.Key,
                TotalCost = departmentCost,
                EmployeeCount = records.Select(r => r.EmployeeId).Distinct().Count(),
                AverageSalary = records.Count > 0 ? departmentCost / records.Count : 0,
                PercentageOfTotalCost = totalCost > 0 ? (departmentCost / totalCost) * 100 : 0
            });
        }

        return departmentSummaries.OrderByDescending(d => d.TotalCost).ToList();
    }

    private async Task<List<MonthlyFinancialData>> GenerateMonthlyBreakdownAsync(List<PayrollRecord> payrollRecords)
    {
        var monthlyData = new List<MonthlyFinancialData>();
        var monthlyGroups = payrollRecords.GroupBy(p => new { p.PayrollYear, p.PayrollMonth });

        MonthlyFinancialData? previousMonth = null;

        foreach (var monthGroup in monthlyGroups.OrderBy(g => g.Key.PayrollYear).ThenBy(g => g.Key.PayrollMonth))
        {
            var records = monthGroup.ToList();
            var totalCost = records.Sum(r => r.NetSalary);

            var monthData = new MonthlyFinancialData
            {
                Year = monthGroup.Key.PayrollYear,
                Month = monthGroup.Key.PayrollMonth,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthGroup.Key.PayrollMonth),
                TotalCost = totalCost,
                EmployeeCount = records.Select(r => r.EmployeeId).Distinct().Count()
            };

            // Calculate growth percentage
            if (previousMonth != null && previousMonth.TotalCost > 0)
            {
                monthData.GrowthPercentage = ((totalCost - previousMonth.TotalCost) / previousMonth.TotalCost) * 100;
            }

            monthlyData.Add(monthData);
            previousMonth = monthData;
        }

        return monthlyData;
    }

    private async Task<PayrollCostBreakdown> GeneratePayrollCostBreakdownAsync(List<PayrollRecord> payrollRecords)
    {
        var breakdown = new PayrollCostBreakdown
        {
            BasicSalaryTotal = payrollRecords.Sum(p => p.BasicSalary),
            AllowancesTotal = payrollRecords.Sum(p => p.TotalAllowances),
            OvertimeTotal = payrollRecords.Sum(p => p.OvertimeAmount),
            DeductionsTotal = payrollRecords.Sum(p => p.TotalDeductions),
            GrossTotal = payrollRecords.Sum(p => p.GrossSalary),
            NetTotal = payrollRecords.Sum(p => p.NetSalary)
        };

        // Generate allowance breakdown
        breakdown.AllowanceBreakdown = new Dictionary<string, decimal>
        {
            ["House Rent Allowance"] = payrollRecords.Sum(p => p.HouseRentAllowance),
            ["Transport Allowance"] = payrollRecords.Sum(p => p.TransportAllowance),
            ["Medical Allowance"] = payrollRecords.Sum(p => p.MedicalAllowance),
            ["Food Allowance"] = payrollRecords.Sum(p => p.FoodAllowance),
            ["Other Allowances"] = payrollRecords.Sum(p => p.OtherAllowances)
        };

        // Generate deduction breakdown
        breakdown.DeductionBreakdown = new Dictionary<string, decimal>
        {
            ["Tax Deduction"] = payrollRecords.Sum(p => p.TaxDeduction),
            ["Provident Fund"] = payrollRecords.Sum(p => p.ProvidentFund),
            ["Employee State Insurance"] = payrollRecords.Sum(p => p.EmployeeStateInsurance),
            ["Professional Tax"] = payrollRecords.Sum(p => p.ProfessionalTax),
            ["Loan Deduction"] = payrollRecords.Sum(p => p.LoanDeduction),
            ["Advance Deduction"] = payrollRecords.Sum(p => p.AdvanceDeduction),
            ["Other Deductions"] = payrollRecords.Sum(p => p.OtherDeductions)
        };

        return breakdown;
    }

    private async Task<List<PayrollTrendData>> GeneratePayrollTrendDataAsync(List<PayrollRecord> payrollRecords)
    {
        var trendData = new List<PayrollTrendData>();
        var monthlyGroups = payrollRecords
            .GroupBy(p => new DateTime(p.PayrollYear, p.PayrollMonth, 1))
            .OrderBy(g => g.Key);

        PayrollTrendData? previousData = null;

        foreach (var monthGroup in monthlyGroups)
        {
            var records = monthGroup.ToList();
            var totalAmount = records.Sum(r => r.NetSalary);

            var trendItem = new PayrollTrendData
            {
                Date = monthGroup.Key,
                Amount = totalAmount,
                Category = "Total Payroll"
            };

            if (previousData != null && previousData.Amount > 0)
            {
                trendItem.PercentageChange = ((totalAmount - previousData.Amount) / previousData.Amount) * 100;
            }

            trendData.Add(trendItem);
            previousData = trendItem;
        }

        return trendData;
    }

    private async Task<List<CostCategoryAnalysis>> GenerateCostCategoryAnalysisAsync(List<PayrollRecord> payrollRecords)
    {
        var analysis = new List<CostCategoryAnalysis>();
        var totalCost = payrollRecords.Sum(p => p.NetSalary);

        var categories = new Dictionary<string, decimal>
        {
            ["Basic Salary"] = payrollRecords.Sum(p => p.BasicSalary),
            ["Allowances"] = payrollRecords.Sum(p => p.TotalAllowances),
            ["Overtime"] = payrollRecords.Sum(p => p.OvertimeAmount),
            ["Deductions"] = payrollRecords.Sum(p => p.TotalDeductions)
        };

        foreach (var category in categories)
        {
            analysis.Add(new CostCategoryAnalysis
            {
                Category = category.Key,
                Amount = category.Value,
                Percentage = totalCost > 0 ? (category.Value / totalCost) * 100 : 0,
                VarianceFromBudget = 0, // Would need budget data to calculate
                Status = "On Track" // Would be determined based on budget comparison
            });
        }

        return analysis;
    }

    private async Task<PayrollProjection?> GeneratePayrollProjectionAsync(List<PayrollRecord> payrollRecords)
    {
        if (payrollRecords.Count < 3) return null; // Need at least 3 months for projection

        var monthlyTotals = payrollRecords
            .GroupBy(p => new DateTime(p.PayrollYear, p.PayrollMonth, 1))
            .OrderBy(g => g.Key)
            .Select(g => g.Sum(p => p.NetSalary))
            .ToList();

        // Simple linear projection based on average growth
        var averageGrowth = 0m;
        for (int i = 1; i < monthlyTotals.Count; i++)
        {
            if (monthlyTotals[i - 1] > 0)
            {
                averageGrowth += (monthlyTotals[i] - monthlyTotals[i - 1]) / monthlyTotals[i - 1];
            }
        }
        averageGrowth /= (monthlyTotals.Count - 1);

        var lastAmount = monthlyTotals.Last();
        var projectedAmount = lastAmount * (1 + averageGrowth);

        return new PayrollProjection
        {
            ProjectionDate = DateTime.UtcNow.AddMonths(1),
            ProjectedAmount = projectedAmount,
            ConfidenceLevel = 0.75m, // 75% confidence
            ProjectionMethod = "Linear Trend Analysis"
        };
    }

    private async Task<List<BudgetVarianceItem>> GenerateBudgetVarianceItemsAsync(List<Budget> budgets, List<PayrollRecord> payrollRecords)
    {
        var varianceItems = new List<BudgetVarianceItem>();
        var categoryGroups = budgets.GroupBy(b => b.Category);

        foreach (var categoryGroup in categoryGroups)
        {
            var categoryBudgets = categoryGroup.ToList();
            var budgetedAmount = categoryBudgets.Sum(b => b.BudgetedAmount);
            var actualAmount = GetActualAmountForCategory(categoryGroup.Key, payrollRecords);
            var variance = actualAmount - budgetedAmount;
            var variancePercentage = budgetedAmount != 0 ? (variance / budgetedAmount) * 100 : 0;

            varianceItems.Add(new BudgetVarianceItem
            {
                Category = categoryGroup.Key,
                BudgetedAmount = budgetedAmount,
                ActualAmount = actualAmount,
                Variance = variance,
                VariancePercentage = variancePercentage,
                Status = GetVarianceStatus(variancePercentage),
                Reason = GetVarianceReason(variancePercentage)
            });
        }

        return varianceItems;
    }

    private async Task<List<DepartmentBudgetVariance>> GenerateDepartmentBudgetVariancesAsync(List<Budget> budgets, List<PayrollRecord> payrollRecords)
    {
        var departmentVariances = new List<DepartmentBudgetVariance>();
        var departmentGroups = budgets.GroupBy(b => b.Department);

        foreach (var departmentGroup in departmentGroups)
        {
            var departmentBudgets = departmentGroup.ToList();
            var departmentPayroll = payrollRecords.Where(p => p.Employee.Department == departmentGroup.Key).ToList();
            
            var budgetedAmount = departmentBudgets.Sum(b => b.BudgetedAmount);
            var actualAmount = departmentPayroll.Sum(p => p.NetSalary);
            var variance = actualAmount - budgetedAmount;
            var variancePercentage = budgetedAmount != 0 ? (variance / budgetedAmount) * 100 : 0;

            var departmentVariance = new DepartmentBudgetVariance
            {
                Department = departmentGroup.Key,
                BudgetedAmount = budgetedAmount,
                ActualAmount = actualAmount,
                Variance = variance,
                VariancePercentage = variancePercentage
            };

            // Generate category variances for this department
            var categoryGroups = departmentBudgets.GroupBy(b => b.Category);
            foreach (var categoryGroup in categoryGroups)
            {
                var categoryBudgetedAmount = categoryGroup.Sum(b => b.BudgetedAmount);
                var categoryActualAmount = GetActualAmountForCategory(categoryGroup.Key, departmentPayroll);
                var categoryVariance = categoryActualAmount - categoryBudgetedAmount;
                var categoryVariancePercentage = categoryBudgetedAmount != 0 ? (categoryVariance / categoryBudgetedAmount) * 100 : 0;

                departmentVariance.CategoryVariances.Add(new BudgetVarianceItem
                {
                    Category = categoryGroup.Key,
                    BudgetedAmount = categoryBudgetedAmount,
                    ActualAmount = categoryActualAmount,
                    Variance = categoryVariance,
                    VariancePercentage = categoryVariancePercentage,
                    Status = GetVarianceStatus(categoryVariancePercentage),
                    Reason = GetVarianceReason(categoryVariancePercentage)
                });
            }

            departmentVariances.Add(departmentVariance);
        }

        return departmentVariances;
    }

    private async Task<List<CurrencyConversionData>> GenerateCurrencyConversionDataAsync(List<PayrollRecord> payrollRecords, string baseCurrency)
    {
        var conversionData = new List<CurrencyConversionData>();
        var currencyGroups = payrollRecords.GroupBy(p => p.Currency);

        foreach (var currencyGroup in currencyGroups)
        {
            var currency = currencyGroup.Key;
            var records = currencyGroup.ToList();
            var totalAmount = records.Sum(r => r.NetSalary);
            var currentRate = await _currencyService.GetExchangeRateAsync(currency, baseCurrency);
            var totalInBaseCurrency = currency == baseCurrency 
                ? totalAmount 
                : await _currencyService.ConvertCurrencyAsync(totalAmount, currency, baseCurrency);

            conversionData.Add(new CurrencyConversionData
            {
                Currency = currency,
                CurrencySymbol = await _currencyService.GetCurrencySymbolAsync(currency),
                CurrentRate = currentRate,
                TotalAmountInCurrency = totalAmount,
                TotalAmountInBaseCurrency = totalInBaseCurrency,
                RateVariation = 0, // Would need historical data to calculate
                Trend = "Stable" // Would be determined from historical data
            });
        }

        return conversionData;
    }

    private async Task<List<ExchangeRateHistory>> GenerateExchangeRateHistoryAsync(CurrencyConversionRequest request)
    {
        var history = new List<ExchangeRateHistory>();
        var targetCurrencies = request.TargetCurrencies ?? await _currencyService.GetSupportedCurrenciesAsync();

        foreach (var currency in targetCurrencies.Where(c => c != request.BaseCurrency))
        {
            // In a real implementation, this would fetch historical data
            // For now, we'll generate sample data
            var currentDate = request.StartDate;
            while (currentDate <= request.EndDate)
            {
                var rate = await _currencyService.GetExchangeRateAsync(request.BaseCurrency, currency);
                history.Add(new ExchangeRateHistory
                {
                    Date = currentDate,
                    Currency = currency,
                    Rate = rate,
                    Change = 0, // Would be calculated from previous day
                    PercentageChange = 0
                });

                currentDate = currentDate.AddDays(1);
            }
        }

        return history;
    }

    private async Task<CurrencyRiskAnalysis> GenerateCurrencyRiskAnalysisAsync(List<PayrollRecord> payrollRecords, string baseCurrency)
    {
        var analysis = new CurrencyRiskAnalysis();
        var currencyGroups = payrollRecords.GroupBy(p => p.Currency);
        var totalExposure = payrollRecords.Sum(p => p.NetSalary);

        analysis.TotalExposure = totalExposure;

        foreach (var currencyGroup in currencyGroups.Where(g => g.Key != baseCurrency))
        {
            var currency = currencyGroup.Key;
            var exposure = currencyGroup.Sum(r => r.NetSalary);
            var volatility = 0.05m; // 5% - would be calculated from historical data
            
            var riskLevel = volatility switch
            {
                < 0.03m => "Low",
                < 0.07m => "Medium",
                _ => "High"
            };

            analysis.CurrencyRisks.Add(new CurrencyRisk
            {
                Currency = currency,
                Exposure = exposure,
                Volatility = volatility,
                RiskLevel = riskLevel,
                Recommendation = GetRiskRecommendation(riskLevel, exposure, totalExposure)
            });
        }

        // Find highest risk currency
        var highestRisk = analysis.CurrencyRisks.OrderByDescending(r => r.Exposure * r.Volatility).FirstOrDefault();
        if (highestRisk != null)
        {
            analysis.HighestRiskCurrencyCode = highestRisk.Currency;
            analysis.HighestRiskCurrency = highestRisk.Exposure;
        }

        analysis.AverageVolatility = analysis.CurrencyRisks.Any() 
            ? analysis.CurrencyRisks.Average(r => r.Volatility) 
            : 0;

        return analysis;
    }

    private async Task<List<DepartmentFinancialDetail>> GenerateDepartmentFinancialDetailsAsync(List<PayrollRecord> payrollRecords)
    {
        var details = new List<DepartmentFinancialDetail>();
        var departmentGroups = payrollRecords.GroupBy(p => p.Employee.Department);

        foreach (var departmentGroup in departmentGroups)
        {
            var records = departmentGroup.ToList();
            var salaries = records.Select(r => r.NetSalary).OrderBy(s => s).ToList();

            var detail = new DepartmentFinancialDetail
            {
                Department = departmentGroup.Key,
                EmployeeCount = records.Select(r => r.EmployeeId).Distinct().Count(),
                TotalCost = records.Sum(r => r.NetSalary),
                AverageSalary = records.Average(r => r.NetSalary),
                MedianSalary = GetMedian(salaries),
                HighestSalary = salaries.LastOrDefault(),
                LowestSalary = salaries.FirstOrDefault(),
                CostBreakdown = await GeneratePayrollCostBreakdownAsync(records)
            };

            // Get top earners
            detail.TopEarners = records
                .OrderByDescending(r => r.NetSalary)
                .Take(5)
                .Select(r => new EmployeeFinancialSummary
                {
                    EmployeeId = r.Employee.EmployeeId,
                    EmployeeName = r.Employee.FullName,
                    Designation = r.Employee.Designation,
                    GrossSalary = r.GrossSalary,
                    NetSalary = r.NetSalary
                })
                .ToList();

            details.Add(detail);
        }

        return details;
    }

    private async Task<DepartmentComparison> GenerateDepartmentComparisonAsync(List<DepartmentFinancialDetail> departmentDetails)
    {
        var comparison = new DepartmentComparison();
        
        if (departmentDetails.Any())
        {
            var highestCost = departmentDetails.OrderByDescending(d => d.TotalCost).First();
            var lowestCost = departmentDetails.OrderBy(d => d.TotalCost).First();
            
            comparison.HighestCostDepartment = highestCost.Department;
            comparison.LowestCostDepartment = lowestCost.Department;
            comparison.CostVariation = highestCost.TotalCost - lowestCost.TotalCost;

            var totalCost = departmentDetails.Sum(d => d.TotalCost);
            comparison.Rankings = departmentDetails
                .OrderByDescending(d => d.TotalCost)
                .Select((d, index) => new DepartmentRanking
                {
                    Rank = index + 1,
                    Department = d.Department,
                    TotalCost = d.TotalCost,
                    PercentageOfTotal = totalCost > 0 ? (d.TotalCost / totalCost) * 100 : 0
                })
                .ToList();
        }

        return comparison;
    }

    private async Task<List<MonthlyTrendData>> GenerateMonthlyTrendDataAsync(List<PayrollRecord> payrollRecords, string trendType)
    {
        var trendData = new List<MonthlyTrendData>();
        var monthlyGroups = payrollRecords
            .GroupBy(p => new { p.PayrollYear, p.PayrollMonth })
            .OrderBy(g => g.Key.PayrollYear)
            .ThenBy(g => g.Key.PayrollMonth);

        var previousValue = 0m;
        var values = new List<decimal>();

        foreach (var monthGroup in monthlyGroups)
        {
            var records = monthGroup.ToList();
            var value = trendType switch
            {
                "Cost" => records.Sum(r => r.NetSalary),
                "Headcount" => records.Select(r => r.EmployeeId).Distinct().Count(),
                "Average Salary" => records.Average(r => r.NetSalary),
                _ => records.Sum(r => r.NetSalary)
            };

            values.Add(value);

            var percentageChange = previousValue > 0 ? ((value - previousValue) / previousValue) * 100 : 0;
            var movingAverage = values.Count >= 3 ? values.TakeLast(3).Average() : values.Average();

            trendData.Add(new MonthlyTrendData
            {
                Year = monthGroup.Key.PayrollYear,
                Month = monthGroup.Key.PayrollMonth,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthGroup.Key.PayrollMonth),
                Value = value,
                PercentageChange = percentageChange,
                MovingAverage = movingAverage
            });

            previousValue = value;
        }

        return trendData;
    }

    private async Task<TrendAnalysis> GenerateTrendAnalysisAsync(List<MonthlyTrendData> trendData)
    {
        var analysis = new TrendAnalysis();

        if (trendData.Any())
        {
            var values = trendData.Select(t => t.Value).ToList();
            var changes = trendData.Where(t => t.PercentageChange != 0).Select(t => t.PercentageChange).ToList();

            analysis.HighestValue = values.Max();
            analysis.LowestValue = values.Min();
            analysis.AverageGrowthRate = changes.Any() ? changes.Average() : 0;

            var highestMonth = trendData.First(t => t.Value == analysis.HighestValue);
            var lowestMonth = trendData.First(t => t.Value == analysis.LowestValue);

            analysis.HighestMonth = $"{highestMonth.MonthName} {highestMonth.Year}";
            analysis.LowestMonth = $"{lowestMonth.MonthName} {lowestMonth.Year}";

            // Determine overall trend
            analysis.OverallTrend = analysis.AverageGrowthRate switch
            {
                > 2 => "Increasing",
                < -2 => "Decreasing",
                _ => "Stable"
            };

            // Generate insights
            analysis.Insights = GenerateInsights(trendData, analysis);
        }

        return analysis;
    }

    // Helper methods
    private decimal GetActualAmountForCategory(string category, List<PayrollRecord> payrollRecords)
    {
        return category.ToLower() switch
        {
            "salary" or "basic salary" => payrollRecords.Sum(p => p.BasicSalary),
            "allowances" => payrollRecords.Sum(p => p.TotalAllowances),
            "overtime" => payrollRecords.Sum(p => p.OvertimeAmount),
            "deductions" => payrollRecords.Sum(p => p.TotalDeductions),
            _ => payrollRecords.Sum(p => p.NetSalary)
        };
    }

    private string GetVarianceStatus(decimal variancePercentage)
    {
        return Math.Abs(variancePercentage) switch
        {
            <= 5 => "On Track",
            <= 15 => variancePercentage > 0 ? "Over Budget" : "Under Budget",
            _ => variancePercentage > 0 ? "Significantly Over Budget" : "Significantly Under Budget"
        };
    }

    private string GetVarianceReason(decimal variancePercentage)
    {
        return Math.Abs(variancePercentage) switch
        {
            <= 5 => "Within acceptable variance range",
            <= 15 => variancePercentage > 0 ? "Higher than expected costs" : "Lower than expected costs",
            _ => variancePercentage > 0 ? "Significant cost overrun - requires investigation" : "Significant cost savings - review budget accuracy"
        };
    }

    private string GetRiskRecommendation(string riskLevel, decimal exposure, decimal totalExposure)
    {
        var exposurePercentage = totalExposure > 0 ? (exposure / totalExposure) * 100 : 0;

        return riskLevel switch
        {
            "High" when exposurePercentage > 20 => "Consider hedging strategies to mitigate currency risk",
            "High" => "Monitor exchange rate fluctuations closely",
            "Medium" when exposurePercentage > 30 => "Consider partial hedging for large exposures",
            "Medium" => "Regular monitoring recommended",
            _ => "Low risk - continue monitoring"
        };
    }

    private decimal GetMedian(List<decimal> values)
    {
        if (!values.Any()) return 0;
        
        var sortedValues = values.OrderBy(v => v).ToList();
        var count = sortedValues.Count;
        
        if (count % 2 == 0)
        {
            return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2;
        }
        else
        {
            return sortedValues[count / 2];
        }
    }

    private List<string> GenerateInsights(List<MonthlyTrendData> trendData, TrendAnalysis analysis)
    {
        var insights = new List<string>();

        if (analysis.AverageGrowthRate > 5)
        {
            insights.Add("Payroll costs are growing rapidly - consider budget adjustments");
        }
        else if (analysis.AverageGrowthRate < -5)
        {
            insights.Add("Payroll costs are declining - may indicate workforce reduction");
        }

        var volatility = trendData.Select(t => t.PercentageChange).Where(c => c != 0).ToList();
        if (volatility.Any() && volatility.Max() - volatility.Min() > 20)
        {
            insights.Add("High volatility detected in payroll costs - investigate seasonal patterns");
        }

        if (trendData.Count >= 6)
        {
            var recentTrend = trendData.TakeLast(3).Average(t => t.PercentageChange);
            var earlierTrend = trendData.Take(3).Average(t => t.PercentageChange);
            
            if (Math.Abs(recentTrend - earlierTrend) > 5)
            {
                insights.Add("Trend pattern has changed significantly in recent months");
            }
        }

        return insights;
    }
}