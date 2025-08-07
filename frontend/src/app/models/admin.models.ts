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