export interface Branch {
  id: number;
  organizationId: number;
  name: string;
  country: string;
  currency: string;
  timeZone: string;
  address: string;
  localHolidays: string[];
  complianceSettings: Record<string, any>;
  createdAt: Date;
  updatedAt: Date;
}

export interface Organization {
  id: number;
  name: string;
  address: string;
  email: string;
  phone: string;
  logo?: string;
  website?: string;
  taxId?: string;
  registrationNumber?: string;
  normalWorkingHours: string;
  overtimeRate: number;
  productiveHoursThreshold: number;
  branchIsolationEnabled: boolean;
  configurationSettings: OrganizationConfig;
  createdAt: string;
  updatedAt?: string;
}

export interface OrganizationConfig {
  allowOvertime: boolean;
  requireApprovalForLeave: boolean;
  enableGeolocation: boolean;
  workingDays: string[];
  defaultCurrency: string;
  dateFormat: string;
  timeFormat: string;
}

// Branch DTOs
export interface CreateBranchDto {
  organizationId: number;
  name: string;
  country: string;
  currency: string;
  timeZone: string;
  address: string;
  localHolidays?: string[];
  complianceSettings?: Record<string, any>;
}

export interface UpdateBranchDto {
  name?: string;
  country?: string;
  currency?: string;
  timeZone?: string;
  address?: string;
  localHolidays?: string[];
  complianceSettings?: Record<string, any>;
}

export interface LocalHoliday {
  id: number;
  name: string;
  date: string;
  isRecurring: boolean;
  branchId: number;
}

// Organization DTOs
export interface CreateOrganizationDto {
  name: string;
  address: string;
  email: string;
  phone: string;
  logo?: string;
  website?: string;
  taxId?: string;
  registrationNumber?: string;
  normalWorkingHours: string;
  overtimeRate: number;
  productiveHoursThreshold: number;
  branchIsolationEnabled: boolean;
  configurationSettings: OrganizationConfig;
}

export interface UpdateOrganizationDto {
  name?: string;
  address?: string;
  email?: string;
  phone?: string;
  logo?: string;
  website?: string;
  taxId?: string;
  registrationNumber?: string;
  normalWorkingHours?: string;
  overtimeRate?: number;
  productiveHoursThreshold?: number;
  branchIsolationEnabled?: boolean;
  configurationSettings?: OrganizationConfig;
}

// Role and Permission models
export interface Role {
  id: number;
  name: string;
  description: string;
  permissions: Permission[];
  hierarchyLevel: number;
  isSystemRole: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface Permission {
  id: number;
  name: string;
  description: string;
  module: string;
  action: string;
  resource: string;
}

export interface CreateRoleDto {
  name: string;
  description: string;
  permissionIds: number[];
  hierarchyLevel: number;
}

export interface UpdateRoleDto {
  name?: string;
  description?: string;
  permissionIds?: number[];
  hierarchyLevel?: number;
}

// System Configuration
export interface SystemConfiguration {
  id: number;
  category: string;
  key: string;
  value: string;
  description: string;
  dataType: string;
  isEditable: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface ConfigurationCategory {
  name: string;
  displayName: string;
  description: string;
  configurations: SystemConfiguration[];
}

export interface UpdateSystemConfigDto {
  configurations: { key: string; value: string }[];
}

// API Response wrapper
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: string[];
  timestamp: string;
}

// File upload response
export interface FileUploadResponse {
  fileName: string;
  originalName: string;
  size: number;
  url: string;
  mimeType: string;
}

// Organization Configuration (alias for backward compatibility)
export interface OrganizationConfiguration extends OrganizationConfig {}