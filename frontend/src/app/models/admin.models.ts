// Organization Models
export interface Organization {
  id: number;
  name: string;
  address: string;
  email: string;
  phone: string;
  logo?: string;
  normalWorkingHours: string;
  overtimeRate: number;
  productiveHoursThreshold: number;
  branchIsolationEnabled: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

export interface CreateOrganizationDto {
  name: string;
  address: string;
  email: string;
  phone: string;
  normalWorkingHours: string;
  overtimeRate: number;
  productiveHoursThreshold: number;
  branchIsolationEnabled: boolean;
}

export interface UpdateOrganizationDto {
  name: string;
  address: string;
  email: string;
  phone: string;
  normalWorkingHours: string;
  overtimeRate: number;
  productiveHoursThreshold: number;
  branchIsolationEnabled: boolean;
}

export interface OrganizationConfiguration {
  id: number;
  organizationId: number;
  settings: Record<string, any>;
  createdAt: Date;
  updatedAt?: Date;
}

// Branch Models
export interface Branch {
  id: number;
  organizationId: number;
  name: string;
  country: string;
  currency: string;
  timeZone: string;
  address: string;
  localHolidays: LocalHoliday[];
  complianceSettings: Record<string, any>;
  createdAt: Date;
  updatedAt?: Date;
}

export interface CreateBranchDto {
  organizationId: number;
  name: string;
  country: string;
  currency: string;
  timeZone: string;
  address: string;
  localHolidays: LocalHoliday[];
  complianceSettings: Record<string, any>;
}

export interface UpdateBranchDto {
  name: string;
  country: string;
  currency: string;
  timeZone: string;
  address: string;
  localHolidays: LocalHoliday[];
  complianceSettings: Record<string, any>;
}

export interface LocalHoliday {
  name: string;
  date: Date;
  isRecurring: boolean;
  description?: string;
}

export interface BranchCompliance {
  branchId: number;
  laborLaws: Record<string, any>;
  taxRegulations: Record<string, any>;
  statutoryRequirements: Record<string, any>;
  reportingRequirements: Record<string, any>;
}

// Role and Permission Models
export interface Role {
  id: number;
  name: string;
  description: string;
  hierarchyLevel: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt?: Date;
  permissions?: Permission[];
}

export interface Permission {
  id: number;
  name: string;
  module: string;
  action: string;
  resource: string;
  description?: string;
}

export interface CreateRoleDto {
  name: string;
  description: string;
  hierarchyLevel: number;
  permissionIds: number[];
}

export interface UpdateRoleDto {
  name: string;
  description: string;
  hierarchyLevel: number;
  permissionIds: number[];
}

export interface EmployeeRole {
  id: number;
  employeeId: number;
  roleId: number;
  assignedAt: Date;
  expiryDate?: Date;
  isActive: boolean;
  role: Role;
}

export interface AssignRoleDto {
  employeeId: number;
  expiryDate?: Date;
}

// System Configuration Models
export interface SystemConfiguration {
  id: number;
  key: string;
  value: string;
  description: string;
  category: string;
  dataType: 'string' | 'number' | 'boolean' | 'json';
  isEditable: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

export interface UpdateSystemConfigDto {
  value: string;
}

export interface ConfigurationCategory {
  name: string;
  displayName: string;
  description: string;
  configurations: SystemConfiguration[];
}

// API Response Models
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message: string;
  errors?: string[];
}

// Currency and Country Models
export interface SupportedCountry {
  code: string;
  name: string;
  currency: string;
  timeZones: string[];
}

export interface SupportedCurrency {
  code: string;
  name: string;
  symbol: string;
}

export interface CurrencyConversion {
  amount: number;
  fromCurrency: string;
  toCurrency: string;
  convertedAmount: number;
  exchangeRate: number;
  convertedAt: Date;
}

// File Upload Models
export interface FileUploadResponse {
  success: boolean;
  filePath: string;
  fileName: string;
  fileSize: number;
  message: string;
}