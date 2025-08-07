import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  SystemConfiguration,
  UpdateSystemConfigDto,
  ConfigurationCategory,
  ApiResponse
} from '../models/admin.models';

@Injectable({
  providedIn: 'root'
})
export class SystemConfigService {
  private readonly apiUrl = `${environment.apiUrl}/system-config`;

  constructor(private http: HttpClient) { }

  // System Configuration Operations
  getAllConfigurations(): Observable<SystemConfiguration[]> {
    // For now, return mock data since the backend endpoint might not exist yet
    return of(this.getMockConfigurations());
  }

  getConfigurationsByCategory(): Observable<ConfigurationCategory[]> {
    return of(this.getMockConfigurationCategories());
  }

  getConfiguration(key: string): Observable<SystemConfiguration | null> {
    const configs = this.getMockConfigurations();
    const config = configs.find(c => c.key === key);
    return of(config || null);
  }

  updateConfiguration(key: string, dto: UpdateSystemConfigDto): Observable<ApiResponse<void>> {
    // This would be a real API call in production
    return of({
      success: true,
      message: 'Configuration updated successfully',
      timestamp: new Date().toISOString()
    });
  }

  resetConfiguration(key: string): Observable<ApiResponse<void>> {
    return of({
      success: true,
      message: 'Configuration reset to default value',
      timestamp: new Date().toISOString()
    });
  }

  // Utility Methods
  validateConfigurationValue(config: SystemConfiguration, value: string): string[] {
    const errors: string[] = [];

    if (!value && config.key !== 'optional_settings') {
      errors.push('Value is required');
    }

    switch (config.dataType) {
      case 'number':
        if (isNaN(Number(value))) {
          errors.push('Value must be a valid number');
        }
        break;
      case 'boolean':
        if (value !== 'true' && value !== 'false') {
          errors.push('Value must be true or false');
        }
        break;
      case 'json':
        try {
          JSON.parse(value);
        } catch {
          errors.push('Value must be valid JSON');
        }
        break;
    }

    return errors;
  }

  private getMockConfigurations(): SystemConfiguration[] {
    return [
      // General Settings
      {
        id: 1,
        key: 'app.name',
        value: 'StrideHR',
        description: 'Application name displayed in the interface',
        category: 'general',
        dataType: 'string',
        isEditable: true,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },
      {
        id: 2,
        key: 'app.version',
        value: '1.0.0',
        description: 'Current application version',
        category: 'general',
        dataType: 'string',
        isEditable: false,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },
      {
        id: 3,
        key: 'app.maintenance_mode',
        value: 'false',
        description: 'Enable maintenance mode to restrict access',
        category: 'general',
        dataType: 'boolean',
        isEditable: true,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },

      // Security Settings
      {
        id: 4,
        key: 'security.session_timeout',
        value: '30',
        description: 'Session timeout in minutes',
        category: 'security',
        dataType: 'number',
        isEditable: true,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },
      {
        id: 5,
        key: 'security.password_min_length',
        value: '8',
        description: 'Minimum password length',
        category: 'security',
        dataType: 'number',
        isEditable: true,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },
      {
        id: 6,
        key: 'security.require_2fa',
        value: 'false',
        description: 'Require two-factor authentication for all users',
        category: 'security',
        dataType: 'boolean',
        isEditable: true,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },

      // Email Settings
      {
        id: 7,
        key: 'email.smtp_host',
        value: 'smtp.gmail.com',
        description: 'SMTP server hostname',
        category: 'email',
        dataType: 'string',
        isEditable: true,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },
      {
        id: 8,
        key: 'email.smtp_port',
        value: '587',
        description: 'SMTP server port',
        category: 'email',
        dataType: 'number',
        isEditable: true,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },
      {
        id: 9,
        key: 'email.from_address',
        value: 'noreply@stridehr.com',
        description: 'Default from email address',
        category: 'email',
        dataType: 'string',
        isEditable: true,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },

      // Attendance Settings
      {
        id: 10,
        key: 'attendance.auto_checkout_hours',
        value: '12',
        description: 'Automatically checkout employees after specified hours',
        category: 'attendance',
        dataType: 'number',
        isEditable: true,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },
      {
        id: 11,
        key: 'attendance.late_threshold_minutes',
        value: '15',
        description: 'Minutes after which arrival is considered late',
        category: 'attendance',
        dataType: 'number',
        isEditable: true,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },

      // Payroll Settings
      {
        id: 12,
        key: 'payroll.default_currency',
        value: 'USD',
        description: 'Default currency for payroll calculations',
        category: 'payroll',
        dataType: 'string',
        isEditable: true,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },
      {
        id: 13,
        key: 'payroll.overtime_multiplier',
        value: '1.5',
        description: 'Overtime pay multiplier',
        category: 'payroll',
        dataType: 'number',
        isEditable: true,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      }
    ];
  }

  private getMockConfigurationCategories(): ConfigurationCategory[] {
    const configs = this.getMockConfigurations();
    const categories = [...new Set(configs.map(c => c.category))];

    return categories.map(category => ({
      name: category,
      displayName: this.getCategoryDisplayName(category),
      description: this.getCategoryDescription(category),
      configurations: configs.filter(c => c.category === category)
    }));
  }

  private getCategoryDisplayName(category: string): string {
    const displayNames: Record<string, string> = {
      general: 'General Settings',
      security: 'Security Settings',
      email: 'Email Configuration',
      attendance: 'Attendance Settings',
      payroll: 'Payroll Settings'
    };
    return displayNames[category] || category;
  }

  private getCategoryDescription(category: string): string {
    const descriptions: Record<string, string> = {
      general: 'Basic application settings and configuration',
      security: 'Security policies and authentication settings',
      email: 'Email server and notification settings',
      attendance: 'Attendance tracking and time management settings',
      payroll: 'Payroll processing and calculation settings'
    };
    return descriptions[category] || '';
  }
}