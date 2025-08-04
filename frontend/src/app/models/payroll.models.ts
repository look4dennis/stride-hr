export interface PayrollRecord {
  id: number;
  employeeId: number;
  employeeName: string;
  employeeCode: string;
  department: string;
  designation: string;
  period: PayrollPeriod;
  basicSalary: number;
  allowances: PayrollAllowance[];
  deductions: PayrollDeduction[];
  grossSalary: number;
  totalDeductions: number;
  netSalary: number;
  currency: string;
  status: PayrollStatus;
  calculatedAt: Date;
  approvedAt?: Date;
  approvedBy?: string;
  releasedAt?: Date;
  releasedBy?: string;
  payslipUrl?: string;
}

export interface PayrollPeriod {
  month: number;
  year: number;
  startDate: Date;
  endDate: Date;
  workingDays: number;
  actualWorkingDays: number;
}

export interface PayrollAllowance {
  id: number;
  name: string;
  type: AllowanceType;
  amount: number;
  isPercentage: boolean;
  isTaxable: boolean;
}

export interface PayrollDeduction {
  id: number;
  name: string;
  type: DeductionType;
  amount: number;
  isPercentage: boolean;
  isStatutory: boolean;
}

export interface PayrollFormula {
  id: number;
  name: string;
  description: string;
  formula: string;
  type: FormulaType;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface PayslipTemplate {
  id: number;
  name: string;
  description: string;
  templateData: PayslipTemplateData;
  isDefault: boolean;
  branchId?: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface PayslipTemplateData {
  header: PayslipSection;
  employeeInfo: PayslipSection;
  earnings: PayslipSection;
  deductions: PayslipSection;
  summary: PayslipSection;
  footer: PayslipSection;
}

export interface PayslipSection {
  elements: PayslipElement[];
  styles: PayslipStyles;
}

export interface PayslipElement {
  id: string;
  type: PayslipElementType;
  content: string;
  position: { x: number; y: number };
  size: { width: number; height: number };
  styles: PayslipStyles;
}

export interface PayslipStyles {
  fontSize?: number;
  fontWeight?: string;
  color?: string;
  backgroundColor?: string;
  textAlign?: string;
  border?: string;
  padding?: string;
  margin?: string;
}

export interface PayrollBatch {
  id: number;
  name: string;
  period: PayrollPeriod;
  branchId: number;
  branchName: string;
  totalEmployees: number;
  processedEmployees: number;
  totalAmount: number;
  currency: string;
  status: PayrollBatchStatus;
  createdAt: Date;
  processedAt?: Date;
  approvedAt?: Date;
  releasedAt?: Date;
  createdBy: string;
  approvedBy?: string;
  releasedBy?: string;
}

export interface PayrollApprovalWorkflow {
  id: number;
  payrollBatchId: number;
  level: number;
  approverRole: string;
  approverId?: number;
  approverName?: string;
  status: ApprovalStatus;
  comments?: string;
  approvedAt?: Date;
}

export interface PayrollReport {
  id: number;
  name: string;
  type: PayrollReportType;
  period: PayrollPeriod;
  branchId?: number;
  data: any;
  generatedAt: Date;
  generatedBy: string;
}

export interface CurrencyExchangeRate {
  id: number;
  fromCurrency: string;
  toCurrency: string;
  rate: number;
  effectiveDate: Date;
  isActive: boolean;
}

// Enums
export enum PayrollStatus {
  Draft = 'Draft',
  Calculated = 'Calculated',
  PendingHRApproval = 'PendingHRApproval',
  PendingFinanceApproval = 'PendingFinanceApproval',
  Approved = 'Approved',
  Released = 'Released',
  Rejected = 'Rejected'
}

export enum PayrollBatchStatus {
  Draft = 'Draft',
  Processing = 'Processing',
  Calculated = 'Calculated',
  PendingApproval = 'PendingApproval',
  Approved = 'Approved',
  Released = 'Released',
  Failed = 'Failed'
}

export enum AllowanceType {
  Basic = 'Basic',
  HRA = 'HRA',
  Transport = 'Transport',
  Medical = 'Medical',
  Bonus = 'Bonus',
  Overtime = 'Overtime',
  ShiftAllowance = 'ShiftAllowance',
  Custom = 'Custom'
}

export enum DeductionType {
  PF = 'PF',
  ESI = 'ESI',
  Tax = 'Tax',
  LoanDeduction = 'LoanDeduction',
  AdvanceDeduction = 'AdvanceDeduction',
  LateDeduction = 'LateDeduction',
  Custom = 'Custom'
}

export enum FormulaType {
  Allowance = 'Allowance',
  Deduction = 'Deduction',
  Tax = 'Tax',
  Overtime = 'Overtime'
}

export enum PayslipElementType {
  Text = 'Text',
  Field = 'Field',
  Image = 'Image',
  Table = 'Table',
  Line = 'Line'
}

export enum ApprovalStatus {
  Pending = 'Pending',
  Approved = 'Approved',
  Rejected = 'Rejected'
}

export enum PayrollReportType {
  PayrollSummary = 'PayrollSummary',
  TaxReport = 'TaxReport',
  StatutoryReport = 'StatutoryReport',
  DepartmentWise = 'DepartmentWise',
  BranchWise = 'BranchWise'
}

// DTOs
export interface CreatePayrollBatchDto {
  name: string;
  period: PayrollPeriod;
  branchId: number;
  employeeIds?: number[];
}

export interface ProcessPayrollDto {
  batchId: number;
  customValues?: { [employeeId: number]: { [field: string]: number } };
}

export interface ApprovePayrollDto {
  batchId: number;
  comments?: string;
}

export interface PayslipGenerationDto {
  payrollRecordIds: number[];
  templateId: number;
  includeAttachments?: boolean;
}

export interface PayrollCalculationDto {
  employeeId: number;
  period: PayrollPeriod;
  customAllowances?: PayrollAllowance[];
  customDeductions?: PayrollDeduction[];
  customValues?: { [field: string]: number };
}