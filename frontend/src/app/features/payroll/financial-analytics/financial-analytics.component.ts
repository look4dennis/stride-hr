import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { Subject, takeUntil, forkJoin } from 'rxjs';
import { FinancialService } from '../../../services/financial.service';
import {
  FinancialSummaryReport,
  PayrollCostAnalysisReport,
  BudgetVarianceReport,
  CurrencyConversionReport,
  MonthlyFinancialTrendReport,
  FinancialReportRequest,
  PayrollCostAnalysisRequest,
  BudgetVarianceRequest,
  CurrencyConversionRequest,
  MonthlyTrendRequest,
  FinancialDashboardData
} from '../../../models/financial.models';

@Component({
  selector: 'app-financial-analytics',
  standalone: true,
  imports: [CommonModule, FormsModule, NgbModule],
  template: `
    <div class="container-fluid">
      <!-- Header -->
      <div class="row mb-4">
        <div class="col-12">
          <div class="d-flex justify-content-between align-items-center">
            <div>
              <h2 class="mb-1">Financial Analytics Dashboard</h2>
              <p class="text-muted mb-0">Comprehensive financial reporting and currency management</p>
            </div>
            <div class="d-flex gap-2">
              <button class="btn btn-outline-primary" (click)="refreshData()">
                <i class="fas fa-sync-alt me-2"></i>Refresh
              </button>
              <div class="dropdown">
                <button class="btn btn-primary dropdown-toggle" type="button" data-bs-toggle="dropdown">
                  <i class="fas fa-download me-2"></i>Export
                </button>
                <ul class="dropdown-menu">
                  <li><a class="dropdown-item" href="#" (click)="exportReport('pdf')">
                    <i class="fas fa-file-pdf me-2"></i>PDF Report
                  </a></li>
                  <li><a class="dropdown-item" href="#" (click)="exportReport('excel')">
                    <i class="fas fa-file-excel me-2"></i>Excel Report
                  </a></li>
                  <li><a class="dropdown-item" href="#" (click)="exportReport('csv')">
                    <i class="fas fa-file-csv me-2"></i>CSV Data
                  </a></li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Filters -->
      <div class="row mb-4">
        <div class="col-12">
          <div class="card">
            <div class="card-body">
              <div class="row g-3">
                <div class="col-md-3">
                  <label class="form-label">Date Range</label>
                  <div class="input-group">
                    <input type="date" class="form-control" [(ngModel)]="filters.startDate" (change)="onFilterChange()">
                    <span class="input-group-text">to</span>
                    <input type="date" class="form-control" [(ngModel)]="filters.endDate" (change)="onFilterChange()">
                  </div>
                </div>
                <div class="col-md-2">
                  <label class="form-label">Currency</label>
                  <select class="form-select" [(ngModel)]="filters.currency" (change)="onFilterChange()">
                    <option value="">All Currencies</option>
                    <option *ngFor="let currency of supportedCurrencies" [value]="currency">
                      {{currency}} ({{getCurrencySymbol(currency)}})
                    </option>
                  </select>
                </div>
                <div class="col-md-2">
                  <label class="form-label">Branch</label>
                  <select class="form-select" [(ngModel)]="filters.branchId" (change)="onFilterChange()">
                    <option value="">All Branches</option>
                    <option *ngFor="let branch of branches" [value]="branch.id">{{branch.name}}</option>
                  </select>
                </div>
                <div class="col-md-3">
                  <label class="form-label">Departments</label>
                  <select class="form-select" multiple [(ngModel)]="filters.departments" (change)="onFilterChange()">
                    <option *ngFor="let dept of departments" [value]="dept">{{dept}}</option>
                  </select>
                </div>
                <div class="col-md-2">
                  <label class="form-label">&nbsp;</label>
                  <button class="btn btn-primary w-100" (click)="generateReports()">
                    <i class="fas fa-chart-line me-2"></i>Generate Reports
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div *ngIf="loading" class="row">
        <div class="col-12">
          <div class="text-center py-5">
            <div class="spinner-border text-primary" role="status">
              <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-3 text-muted">Generating financial reports...</p>
          </div>
        </div>
      </div>

      <!-- Dashboard Cards -->
      <div *ngIf="!loading && dashboardData" class="row mb-4">
        <div class="col-md-3">
          <div class="card bg-primary text-white">
            <div class="card-body">
              <div class="d-flex justify-content-between">
                <div>
                  <h6 class="card-title mb-1">Total Payroll Cost</h6>
                  <h3 class="mb-0">{{formatCurrency(dashboardData.totalPayrollCost, filters.currency || 'USD')}}</h3>
                </div>
                <div class="align-self-center">
                  <i class="fas fa-dollar-sign fa-2x opacity-75"></i>
                </div>
              </div>
              <div class="mt-2">
                <small class="opacity-75">
                  <i [class]="getTrendIcon(dashboardData.monthlyGrowth)"></i>
                  {{dashboardData.monthlyGrowth | number:'1.1-1'}}% from last month
                </small>
              </div>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card bg-success text-white">
            <div class="card-body">
              <div class="d-flex justify-content-between">
                <div>
                  <h6 class="card-title mb-1">Budget Variance</h6>
                  <h3 class="mb-0">{{dashboardData.budgetVariance | number:'1.1-1'}}%</h3>
                </div>
                <div class="align-self-center">
                  <i class="fas fa-chart-pie fa-2x opacity-75"></i>
                </div>
              </div>
              <div class="mt-2">
                <small class="opacity-75">
                  {{dashboardData.budgetVariance > 0 ? 'Over' : 'Under'}} budget
                </small>
              </div>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card bg-info text-white">
            <div class="card-body">
              <div class="d-flex justify-content-between">
                <div>
                  <h6 class="card-title mb-1">Currency Exposure</h6>
                  <h3 class="mb-0">{{dashboardData.currencyExposure?.length || 0}}</h3>
                </div>
                <div class="align-self-center">
                  <i class="fas fa-exchange-alt fa-2x opacity-75"></i>
                </div>
              </div>
              <div class="mt-2">
                <small class="opacity-75">Active currencies</small>
              </div>
            </div>
          </div>
        </div>
        <div class="col-md-3">
          <div class="card bg-warning text-white">
            <div class="card-body">
              <div class="d-flex justify-content-between">
                <div>
                  <h6 class="card-title mb-1">Exchange Rate Alerts</h6>
                  <h3 class="mb-0">{{dashboardData.exchangeRateAlerts?.length || 0}}</h3>
                </div>
                <div class="align-self-center">
                  <i class="fas fa-exclamation-triangle fa-2x opacity-75"></i>
                </div>
              </div>
              <div class="mt-2">
                <small class="opacity-75">Requires attention</small>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Reports Tabs -->
      <div *ngIf="!loading" class="row">
        <div class="col-12">
          <ngb-tabset>
            <!-- Financial Summary Tab -->
            <ngb-tab id="summary">
              <ng-template ngbTabTitle>
                <i class="fas fa-chart-bar me-2"></i>Financial Summary
              </ng-template>
              <ng-template ngbTabContent>
                <div *ngIf="financialSummary" class="mt-3">
                  <!-- Summary Cards -->
                  <div class="row mb-4">
                    <div class="col-md-2">
                      <div class="card text-center">
                        <div class="card-body">
                          <h6 class="card-title text-muted">Total Employees</h6>
                          <h4 class="text-primary">{{financialSummary.totalEmployees}}</h4>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-2">
                      <div class="card text-center">
                        <div class="card-body">
                          <h6 class="card-title text-muted">Gross Salary</h6>
                          <h4 class="text-success">{{formatCurrency(financialSummary.totalGrossSalary, financialSummary.currency)}}</h4>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-2">
                      <div class="card text-center">
                        <div class="card-body">
                          <h6 class="card-title text-muted">Net Salary</h6>
                          <h4 class="text-info">{{formatCurrency(financialSummary.totalNetSalary, financialSummary.currency)}}</h4>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-2">
                      <div class="card text-center">
                        <div class="card-body">
                          <h6 class="card-title text-muted">Deductions</h6>
                          <h4 class="text-warning">{{formatCurrency(financialSummary.totalDeductions, financialSummary.currency)}}</h4>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-2">
                      <div class="card text-center">
                        <div class="card-body">
                          <h6 class="card-title text-muted">Allowances</h6>
                          <h4 class="text-primary">{{formatCurrency(financialSummary.totalAllowances, financialSummary.currency)}}</h4>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-2">
                      <div class="card text-center">
                        <div class="card-body">
                          <h6 class="card-title text-muted">Overtime</h6>
                          <h4 class="text-secondary">{{formatCurrency(financialSummary.totalOvertimeCost, financialSummary.currency)}}</h4>
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- Charts Row -->
                  <div class="row mb-4">
                    <div class="col-md-8">
                      <div class="card">
                        <div class="card-header">
                          <h5 class="card-title mb-0">Monthly Financial Trend</h5>
                        </div>
                        <div class="card-body">
                          <canvas #monthlyTrendChart></canvas>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-4">
                      <div class="card">
                        <div class="card-header">
                          <h5 class="card-title mb-0">Department Breakdown</h5>
                        </div>
                        <div class="card-body">
                          <canvas #departmentChart></canvas>
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- Branch Summary Table -->
                  <div class="card">
                    <div class="card-header">
                      <h5 class="card-title mb-0">Branch-wise Summary</h5>
                    </div>
                    <div class="card-body">
                      <div class="table-responsive">
                        <table class="table table-hover">
                          <thead>
                            <tr>
                              <th>Branch</th>
                              <th>Country</th>
                              <th>Local Currency</th>
                              <th>Employee Count</th>
                              <th>Total Cost (Local)</th>
                              <th>Total Cost ({{financialSummary.currency}})</th>
                              <th>Average Salary</th>
                              <th>Exchange Rate</th>
                            </tr>
                          </thead>
                          <tbody>
                            <tr *ngFor="let branch of financialSummary.branchSummaries">
                              <td>{{branch.branchName}}</td>
                              <td>{{branch.country}}</td>
                              <td>{{branch.localCurrency}}</td>
                              <td>{{branch.employeeCount}}</td>
                              <td>{{formatCurrency(branch.totalPayrollCost, branch.localCurrency)}}</td>
                              <td>{{formatCurrency(branch.totalPayrollCostInBaseCurrency, financialSummary.currency)}}</td>
                              <td>{{formatCurrency(branch.averageSalary, branch.localCurrency)}}</td>
                              <td>{{branch.exchangeRate | number:'1.4-4'}}</td>
                            </tr>
                          </tbody>
                        </table>
                      </div>
                    </div>
                  </div>
                </div>
              </ng-template>
            </ngb-tab>

            <!-- Cost Analysis Tab -->
            <ngb-tab id="cost-analysis">
              <ng-template ngbTabTitle>
                <i class="fas fa-calculator me-2"></i>Cost Analysis
              </ng-template>
              <ng-template ngbTabContent>
                <div *ngIf="costAnalysis" class="mt-3">
                  <!-- Cost Breakdown Cards -->
                  <div class="row mb-4">
                    <div class="col-md-6">
                      <div class="card">
                        <div class="card-header">
                          <h5 class="card-title mb-0">Payroll Cost Breakdown</h5>
                        </div>
                        <div class="card-body">
                          <canvas #costBreakdownChart></canvas>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-6">
                      <div class="card">
                        <div class="card-header">
                          <h5 class="card-title mb-0">Cost Trend Analysis</h5>
                        </div>
                        <div class="card-body">
                          <canvas #costTrendChart></canvas>
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- Category Analysis Table -->
                  <div class="card">
                    <div class="card-header">
                      <h5 class="card-title mb-0">Category Analysis</h5>
                    </div>
                    <div class="card-body">
                      <div class="table-responsive">
                        <table class="table table-hover">
                          <thead>
                            <tr>
                              <th>Category</th>
                              <th>Amount</th>
                              <th>Percentage</th>
                              <th>Budget Variance</th>
                              <th>Status</th>
                            </tr>
                          </thead>
                          <tbody>
                            <tr *ngFor="let category of costAnalysis.categoryAnalysis">
                              <td>{{category.category}}</td>
                              <td>{{formatCurrency(category.amount, costAnalysis.currency)}}</td>
                              <td>{{category.percentage | number:'1.1-1'}}%</td>
                              <td [class]="getVarianceClass(category.varianceFromBudget)">
                                {{formatCurrency(category.varianceFromBudget, costAnalysis.currency)}}
                              </td>
                              <td>
                                <span class="badge" [class]="getStatusBadgeClass(category.status)">
                                  {{category.status}}
                                </span>
                              </td>
                            </tr>
                          </tbody>
                        </table>
                      </div>
                    </div>
                  </div>
                </div>
              </ng-template>
            </ngb-tab>

            <!-- Budget Variance Tab -->
            <ngb-tab id="budget-variance">
              <ng-template ngbTabTitle>
                <i class="fas fa-balance-scale me-2"></i>Budget Variance
              </ng-template>
              <ng-template ngbTabContent>
                <div *ngIf="budgetVariance" class="mt-3">
                  <!-- Variance Summary -->
                  <div class="row mb-4">
                    <div class="col-md-3">
                      <div class="card text-center">
                        <div class="card-body">
                          <h6 class="card-title text-muted">Total Budget</h6>
                          <h4 class="text-primary">{{formatCurrency(budgetVariance.totalBudget, budgetVariance.currency)}}</h4>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-3">
                      <div class="card text-center">
                        <div class="card-body">
                          <h6 class="card-title text-muted">Total Actual</h6>
                          <h4 class="text-info">{{formatCurrency(budgetVariance.totalActual, budgetVariance.currency)}}</h4>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-3">
                      <div class="card text-center">
                        <div class="card-body">
                          <h6 class="card-title text-muted">Variance</h6>
                          <h4 [class]="getVarianceClass(budgetVariance.totalVariance)">
                            {{formatCurrency(budgetVariance.totalVariance, budgetVariance.currency)}}
                          </h4>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-3">
                      <div class="card text-center">
                        <div class="card-body">
                          <h6 class="card-title text-muted">Variance %</h6>
                          <h4 [class]="getVarianceClass(budgetVariance.variancePercentage)">
                            {{budgetVariance.variancePercentage | number:'1.1-1'}}%
                          </h4>
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- Variance Items Table -->
                  <div class="card">
                    <div class="card-header">
                      <h5 class="card-title mb-0">Budget Variance by Category</h5>
                    </div>
                    <div class="card-body">
                      <div class="table-responsive">
                        <table class="table table-hover">
                          <thead>
                            <tr>
                              <th>Category</th>
                              <th>Budgeted</th>
                              <th>Actual</th>
                              <th>Variance</th>
                              <th>Variance %</th>
                              <th>Status</th>
                              <th>Reason</th>
                            </tr>
                          </thead>
                          <tbody>
                            <tr *ngFor="let item of budgetVariance.varianceItems">
                              <td>{{item.category}}</td>
                              <td>{{formatCurrency(item.budgetedAmount, budgetVariance.currency)}}</td>
                              <td>{{formatCurrency(item.actualAmount, budgetVariance.currency)}}</td>
                              <td [class]="getVarianceClass(item.variance)">
                                {{formatCurrency(item.variance, budgetVariance.currency)}}
                              </td>
                              <td [class]="getVarianceClass(item.variancePercentage)">
                                {{item.variancePercentage | number:'1.1-1'}}%
                              </td>
                              <td>
                                <span class="badge" [class]="getStatusBadgeClass(item.status)">
                                  {{item.status}}
                                </span>
                              </td>
                              <td>{{item.reason}}</td>
                            </tr>
                          </tbody>
                        </table>
                      </div>
                    </div>
                  </div>
                </div>
              </ng-template>
            </ngb-tab>

            <!-- Currency Management Tab -->
            <ngb-tab id="currency">
              <ng-template ngbTabTitle>
                <i class="fas fa-exchange-alt me-2"></i>Currency Management
              </ng-template>
              <ng-template ngbTabContent>
                <div *ngIf="currencyConversion" class="mt-3">
                  <!-- Currency Overview -->
                  <div class="row mb-4">
                    <div class="col-md-8">
                      <div class="card">
                        <div class="card-header">
                          <h5 class="card-title mb-0">Currency Conversion Overview</h5>
                        </div>
                        <div class="card-body">
                          <div class="table-responsive">
                            <table class="table table-hover">
                              <thead>
                                <tr>
                                  <th>Currency</th>
                                  <th>Current Rate</th>
                                  <th>Total Amount</th>
                                  <th>Amount in {{currencyConversion.baseCurrency}}</th>
                                  <th>Rate Variation</th>
                                  <th>Trend</th>
                                </tr>
                              </thead>
                              <tbody>
                                <tr *ngFor="let data of currencyConversion.conversionData">
                                  <td>
                                    <strong>{{data.currency}}</strong>
                                    <small class="text-muted ms-1">({{data.currencySymbol}})</small>
                                  </td>
                                  <td>{{data.currentRate | number:'1.4-4'}}</td>
                                  <td>{{formatCurrency(data.totalAmountInCurrency, data.currency)}}</td>
                                  <td>{{formatCurrency(data.totalAmountInBaseCurrency, currencyConversion.baseCurrency)}}</td>
                                  <td [class]="getVarianceClass(data.rateVariation)">
                                    {{data.rateVariation | number:'1.2-2'}}%
                                  </td>
                                  <td>
                                    <i [class]="getTrendIcon(data.trend)"></i>
                                    {{data.trend}}
                                  </td>
                                </tr>
                              </tbody>
                            </table>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-4">
                      <div class="card">
                        <div class="card-header">
                          <h5 class="card-title mb-0">Currency Risk Analysis</h5>
                        </div>
                        <div class="card-body">
                          <div class="mb-3">
                            <small class="text-muted">Total Exposure</small>
                            <h5>{{formatCurrency(currencyConversion.riskAnalysis.totalExposure, currencyConversion.baseCurrency)}}</h5>
                          </div>
                          <div class="mb-3">
                            <small class="text-muted">Highest Risk Currency</small>
                            <h6>{{currencyConversion.riskAnalysis.highestRiskCurrencyCode}}</h6>
                          </div>
                          <div class="mb-3">
                            <small class="text-muted">Average Volatility</small>
                            <h6>{{currencyConversion.riskAnalysis.averageVolatility | number:'1.2-2'}}%</h6>
                          </div>
                          
                          <hr>
                          
                          <div *ngFor="let risk of currencyConversion.riskAnalysis.currencyRisks" class="mb-2">
                            <div class="d-flex justify-content-between align-items-center">
                              <span>{{risk.currency}}</span>
                              <span class="badge" [class]="getRiskBadgeClass(risk.riskLevel)">
                                {{risk.riskLevel}}
                              </span>
                            </div>
                            <small class="text-muted">{{risk.recommendation}}</small>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- Exchange Rate History Chart -->
                  <div class="card">
                    <div class="card-header">
                      <h5 class="card-title mb-0">Exchange Rate History</h5>
                    </div>
                    <div class="card-body">
                      <canvas #exchangeRateChart></canvas>
                    </div>
                  </div>
                </div>
              </ng-template>
            </ngb-tab>

            <!-- Monthly Trends Tab -->
            <ngb-tab id="trends">
              <ng-template ngbTabTitle>
                <i class="fas fa-chart-line me-2"></i>Monthly Trends
              </ng-template>
              <ng-template ngbTabContent>
                <div *ngIf="monthlyTrends" class="mt-3">
                  <!-- Trend Analysis Summary -->
                  <div class="row mb-4">
                    <div class="col-md-3">
                      <div class="card text-center">
                        <div class="card-body">
                          <h6 class="card-title text-muted">Overall Trend</h6>
                          <h4>
                            <i [class]="getTrendIcon(monthlyTrends.analysis.overallTrend)"></i>
                            {{monthlyTrends.analysis.overallTrend}}
                          </h4>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-3">
                      <div class="card text-center">
                        <div class="card-body">
                          <h6 class="card-title text-muted">Average Growth</h6>
                          <h4 [class]="getVarianceClass(monthlyTrends.analysis.averageGrowthRate)">
                            {{monthlyTrends.analysis.averageGrowthRate | number:'1.1-1'}}%
                          </h4>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-3">
                      <div class="card text-center">
                        <div class="card-body">
                          <h6 class="card-title text-muted">Highest Month</h6>
                          <h6>{{monthlyTrends.analysis.highestMonth}}</h6>
                          <small class="text-muted">{{formatCurrency(monthlyTrends.analysis.highestValue, monthlyTrends.currency)}}</small>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-3">
                      <div class="card text-center">
                        <div class="card-body">
                          <h6 class="card-title text-muted">Lowest Month</h6>
                          <h6>{{monthlyTrends.analysis.lowestMonth}}</h6>
                          <small class="text-muted">{{formatCurrency(monthlyTrends.analysis.lowestValue, monthlyTrends.currency)}}</small>
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- Trend Chart -->
                  <div class="card mb-4">
                    <div class="card-header">
                      <h5 class="card-title mb-0">{{monthlyTrends.trendType}} Trend Analysis</h5>
                    </div>
                    <div class="card-body">
                      <canvas #trendAnalysisChart></canvas>
                    </div>
                  </div>

                  <!-- Insights -->
                  <div class="card">
                    <div class="card-header">
                      <h5 class="card-title mb-0">Key Insights</h5>
                    </div>
                    <div class="card-body">
                      <div *ngIf="monthlyTrends.analysis.insights.length === 0" class="text-muted">
                        No specific insights available for the current data set.
                      </div>
                      <div *ngFor="let insight of monthlyTrends.analysis.insights" class="alert alert-info">
                        <i class="fas fa-lightbulb me-2"></i>{{insight}}
                      </div>
                    </div>
                  </div>
                </div>
              </ng-template>
            </ngb-tab>
          </ngb-tabset>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card {
      border: none;
      box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
      transition: box-shadow 0.15s ease-in-out;
    }

    .card:hover {
      box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
    }

    .table th {
      border-top: none;
      font-weight: 600;
      color: #495057;
      background-color: #f8f9fa;
    }

    .badge {
      font-size: 0.75em;
    }

    .text-success { color: #28a745 !important; }
    .text-danger { color: #dc3545 !important; }
    .text-warning { color: #ffc107 !important; }
    .text-info { color: #17a2b8 !important; }

    .bg-success { background-color: #28a745 !important; }
    .bg-danger { background-color: #dc3545 !important; }
    .bg-warning { background-color: #ffc107 !important; }
    .bg-info { background-color: #17a2b8 !important; }

    canvas {
      max-height: 400px;
    }

    .spinner-border {
      width: 3rem;
      height: 3rem;
    }
  `]
})
export class FinancialAnalyticsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  loading = false;
  
  // Data properties
  dashboardData: FinancialDashboardData | null = null;
  financialSummary: FinancialSummaryReport | null = null;
  costAnalysis: PayrollCostAnalysisReport | null = null;
  budgetVariance: BudgetVarianceReport | null = null;
  currencyConversion: CurrencyConversionReport | null = null;
  monthlyTrends: MonthlyFinancialTrendReport | null = null;

  // Filter properties
  filters = {
    startDate: new Date(new Date().getFullYear(), new Date().getMonth() - 6, 1).toISOString().split('T')[0],
    endDate: new Date().toISOString().split('T')[0],
    currency: 'USD',
    branchId: null as number | null,
    departments: [] as string[]
  };

  // Options
  supportedCurrencies: string[] = [];
  branches: any[] = [];
  departments: string[] = ['HR', 'IT', 'Finance', 'Marketing', 'Operations', 'Sales'];

  constructor(private financialService: FinancialService) {}

  ngOnInit(): void {
    this.supportedCurrencies = this.financialService.getSupportedCurrencies();
    this.loadInitialData();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadInitialData(): void {
    this.loading = true;
    
    // Load dashboard data
    this.financialService.getFinancialDashboardData()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.dashboardData = data;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading dashboard data:', error);
          this.loading = false;
        }
      });
  }

  onFilterChange(): void {
    // Auto-generate reports when filters change
    // You might want to debounce this in a real application
  }

  generateReports(): void {
    this.loading = true;

    const startDate = new Date(this.filters.startDate);
    const endDate = new Date(this.filters.endDate);

    // Prepare requests
    const financialRequest: FinancialReportRequest = {
      startDate,
      endDate,
      currency: this.filters.currency || undefined,
      branchId: this.filters.branchId || undefined,
      departments: this.filters.departments.length > 0 ? this.filters.departments : undefined,
      includeCurrencyConversion: true
    };

    const costAnalysisRequest: PayrollCostAnalysisRequest = {
      startDate,
      endDate,
      currency: this.filters.currency || undefined,
      branchId: this.filters.branchId || undefined,
      departments: this.filters.departments.length > 0 ? this.filters.departments : undefined,
      includeProjections: true
    };

    const budgetVarianceRequest: BudgetVarianceRequest = {
      startDate,
      endDate,
      currency: this.filters.currency || undefined,
      branchId: this.filters.branchId || undefined,
      departments: this.filters.departments.length > 0 ? this.filters.departments : undefined
    };

    const currencyRequest: CurrencyConversionRequest = {
      startDate,
      endDate,
      baseCurrency: this.filters.currency || 'USD',
      includeHistoricalRates: true
    };

    const trendRequest: MonthlyTrendRequest = {
      startDate,
      endDate,
      currency: this.filters.currency || undefined,
      branchId: this.filters.branchId || undefined,
      trendType: 'Cost'
    };

    // Execute all requests in parallel
    forkJoin({
      summary: this.financialService.generateFinancialSummary(financialRequest),
      costAnalysis: this.financialService.generatePayrollCostAnalysis(costAnalysisRequest),
      budgetVariance: this.financialService.generateBudgetVariance(budgetVarianceRequest),
      currencyConversion: this.financialService.generateCurrencyConversion(currencyRequest),
      monthlyTrends: this.financialService.generateMonthlyTrend(trendRequest)
    })
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: (results) => {
        this.financialSummary = results.summary;
        this.costAnalysis = results.costAnalysis;
        this.budgetVariance = results.budgetVariance;
        this.currencyConversion = results.currencyConversion;
        this.monthlyTrends = results.monthlyTrends;
        
        this.loading = false;
        
        // Generate charts after data is loaded
        setTimeout(() => this.generateCharts(), 100);
      },
      error: (error) => {
        console.error('Error generating reports:', error);
        this.loading = false;
      }
    });
  }

  refreshData(): void {
    this.loadInitialData();
    if (this.financialSummary) {
      this.generateReports();
    }
  }

  exportReport(format: 'pdf' | 'excel' | 'csv'): void {
    const reportData = {
      summary: this.financialSummary,
      costAnalysis: this.costAnalysis,
      budgetVariance: this.budgetVariance,
      currencyConversion: this.currencyConversion,
      monthlyTrends: this.monthlyTrends
    };

    this.financialService.exportReport(reportData, format, `financial-report-${new Date().toISOString().split('T')[0]}`);
  }

  // Chart generation methods
  generateCharts(): void {
    if (this.financialSummary) {
      this.generateMonthlyTrendChart();
      this.generateDepartmentChart();
    }
    
    if (this.costAnalysis) {
      this.generateCostBreakdownChart();
      this.generateCostTrendChart();
    }
    
    if (this.currencyConversion) {
      this.generateExchangeRateChart();
    }
    
    if (this.monthlyTrends) {
      this.generateTrendAnalysisChart();
    }
  }

  generateMonthlyTrendChart(): void {
    // Implementation for monthly trend chart
    // This would use Chart.js to create the chart
  }

  generateDepartmentChart(): void {
    // Implementation for department pie chart
  }

  generateCostBreakdownChart(): void {
    // Implementation for cost breakdown chart
  }

  generateCostTrendChart(): void {
    // Implementation for cost trend chart
  }

  generateExchangeRateChart(): void {
    // Implementation for exchange rate chart
  }

  generateTrendAnalysisChart(): void {
    // Implementation for trend analysis chart
  }

  // Utility methods
  formatCurrency(amount: number, currency: string): string {
    return this.financialService.formatCurrency(amount, currency);
  }

  getCurrencySymbol(currency: string): string {
    return this.financialService.getCurrencySymbol(currency);
  }

  getVarianceClass(variance: number): string {
    return this.financialService.getVarianceStatusClass(variance);
  }

  getTrendIcon(trend: string | number): string {
    if (typeof trend === 'number') {
      if (trend > 0) return 'fas fa-arrow-up text-success';
      if (trend < 0) return 'fas fa-arrow-down text-danger';
      return 'fas fa-minus text-warning';
    }
    return this.financialService.getTrendIconClass(trend);
  }

  getStatusBadgeClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'on track':
        return 'bg-success';
      case 'over budget':
        return 'bg-warning';
      case 'under budget':
        return 'bg-info';
      case 'significantly over budget':
        return 'bg-danger';
      default:
        return 'bg-secondary';
    }
  }

  getRiskBadgeClass(riskLevel: string): string {
    switch (riskLevel.toLowerCase()) {
      case 'low':
        return 'bg-success';
      case 'medium':
        return 'bg-warning';
      case 'high':
        return 'bg-danger';
      default:
        return 'bg-secondary';
    }
  }
}