import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SystemConfigService } from '../../services/system-config.service';
import { SystemConfiguration, ConfigurationCategory, UpdateSystemConfigDto } from '../../models/admin.models';

@Component({
  selector: 'app-system-config',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="system-config-container">
      <!-- Header -->
      <div class="page-header">
        <div class="d-flex justify-content-between align-items-center">
          <div>
            <h1>System Configuration</h1>
            <p class="text-muted">Configure system-wide settings and preferences</p>
          </div>
          <div class="d-flex gap-2">
            <button class="btn btn-success" (click)="saveAllChanges()" [disabled]="!hasUnsavedChanges || isSaving">
              <span *ngIf="isSaving" class="spinner-border spinner-border-sm me-2"></span>
              <i class="fas fa-save me-2" *ngIf="!isSaving"></i>
              Save All Changes
            </button>
            <button class="btn btn-outline-secondary" routerLink="/settings">
              <i class="fas fa-arrow-left me-2"></i>Back to Settings
            </button>
          </div>
        </div>
      </div>

      <!-- Configuration Categories -->
      <div class="row">
        <!-- Category Navigation -->
        <div class="col-lg-3">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">Categories</h5>
            </div>
            <div class="card-body p-0">
              <div class="list-group list-group-flush">
                <button 
                  type="button"
                  class="list-group-item list-group-item-action"
                  [class.active]="selectedCategory === category.name"
                  *ngFor="let category of configurationCategories"
                  (click)="selectCategory(category.name)">
                  <div class="d-flex justify-content-between align-items-center">
                    <div>
                      <div class="fw-semibold">{{ category.displayName }}</div>
                      <small class="text-muted">{{ category.configurations.length }} settings</small>
                    </div>
                    <i class="fas fa-chevron-right text-muted"></i>
                  </div>
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Configuration Settings -->
        <div class="col-lg-9">
          <div class="card" *ngIf="selectedCategoryData">
            <div class="card-header">
              <div class="d-flex justify-content-between align-items-center">
                <div>
                  <h5 class="card-title mb-0">{{ selectedCategoryData.displayName }}</h5>
                  <small class="text-muted">{{ selectedCategoryData.description }}</small>
                </div>
                <button class="btn btn-outline-warning btn-sm" (click)="resetCategoryToDefaults()">
                  <i class="fas fa-undo me-1"></i>Reset to Defaults
                </button>
              </div>
            </div>
            <div class="card-body">
              <div class="configuration-list">
                <div class="configuration-item" *ngFor="let config of selectedCategoryData.configurations">
                  <div class="row align-items-center">
                    <div class="col-md-4">
                      <label class="form-label mb-0">
                        {{ getConfigDisplayName(config.key) }}
                        <span class="text-danger" *ngIf="!config.isEditable">*</span>
                      </label>
                      <div class="form-text">{{ config.description }}</div>
                    </div>
                    <div class="col-md-6">
                      <!-- String Input -->
                      <input 
                        *ngIf="config.dataType === 'string'"
                        type="text" 
                        class="form-control"
                        [value]="getConfigValue(config.key)"
                        (input)="updateConfigValue(config.key, $event)"
                        [disabled]="!config.isEditable">

                      <!-- Number Input -->
                      <input 
                        *ngIf="config.dataType === 'number'"
                        type="number" 
                        class="form-control"
                        [value]="getConfigValue(config.key)"
                        (input)="updateConfigValue(config.key, $event)"
                        [disabled]="!config.isEditable">

                      <!-- Boolean Input -->
                      <div *ngIf="config.dataType === 'boolean'" class="form-check form-switch">
                        <input 
                          class="form-check-input" 
                          type="checkbox" 
                          [id]="'config-' + config.key"
                          [checked]="getConfigValue(config.key) === 'true'"
                          (change)="updateConfigBooleanValue(config.key, $event)"
                          [disabled]="!config.isEditable">
                        <label class="form-check-label" [for]="'config-' + config.key">
                          {{ getConfigValue(config.key) === 'true' ? 'Enabled' : 'Disabled' }}
                        </label>
                      </div>

                      <!-- JSON Input -->
                      <textarea 
                        *ngIf="config.dataType === 'json'"
                        class="form-control" 
                        rows="4"
                        [value]="getConfigValue(config.key)"
                        (input)="updateConfigValue(config.key, $event)"
                        [disabled]="!config.isEditable"
                        placeholder="Enter valid JSON"></textarea>
                    </div>
                    <div class="col-md-2">
                      <div class="d-flex gap-1">
                        <button 
                          class="btn btn-outline-info btn-sm"
                          (click)="showConfigDetails(config)"
                          title="View Details">
                          <i class="fas fa-info"></i>
                        </button>
                        <button 
                          class="btn btn-outline-warning btn-sm"
                          (click)="resetConfigToDefault(config)"
                          [disabled]="!config.isEditable"
                          title="Reset to Default">
                          <i class="fas fa-undo"></i>
                        </button>
                      </div>
                    </div>
                  </div>
                  
                  <!-- Validation Errors -->
                  <div class="row mt-2" *ngIf="getConfigErrors(config.key).length > 0">
                    <div class="col-md-8 offset-md-4">
                      <div class="alert alert-danger alert-sm mb-0">
                        <ul class="mb-0">
                          <li *ngFor="let error of getConfigErrors(config.key)">{{ error }}</li>
                        </ul>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- No Category Selected -->
          <div class="card" *ngIf="!selectedCategoryData">
            <div class="card-body text-center py-5">
              <i class="fas fa-cogs text-muted mb-3" style="font-size: 3rem;"></i>
              <h3>Select a Configuration Category</h3>
              <p class="text-muted">Choose a category from the left panel to view and edit settings.</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Configuration Details Modal -->
      <div class="modal fade" [class.show]="showDetailsModal" [style.display]="showDetailsModal ? 'block' : 'none'" tabindex="-1">
        <div class="modal-dialog">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">Configuration Details</h5>
              <button type="button" class="btn-close" (click)="closeDetailsModal()"></button>
            </div>
            <div class="modal-body" *ngIf="selectedConfig">
              <div class="row g-3">
                <div class="col-12">
                  <strong>Configuration Key:</strong>
                  <p class="font-monospace">{{ selectedConfig.key }}</p>
                </div>
                <div class="col-12">
                  <strong>Description:</strong>
                  <p>{{ selectedConfig.description }}</p>
                </div>
                <div class="col-md-6">
                  <strong>Data Type:</strong>
                  <p>{{ selectedConfig.dataType }}</p>
                </div>
                <div class="col-md-6">
                  <strong>Editable:</strong>
                  <p>{{ selectedConfig.isEditable ? 'Yes' : 'No' }}</p>
                </div>
                <div class="col-12">
                  <strong>Current Value:</strong>
                  <pre class="bg-light p-2 rounded">{{ selectedConfig.value }}</pre>
                </div>
                <div class="col-md-6">
                  <strong>Created:</strong>
                  <p>{{ selectedConfig.createdAt | date:'medium' }}</p>
                </div>
                <div class="col-md-6">
                  <strong>Last Updated:</strong>
                  <p>{{ selectedConfig.updatedAt | date:'medium' }}</p>
                </div>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" (click)="closeDetailsModal()">Close</button>
            </div>
          </div>
        </div>
      </div>

      <!-- Modal Backdrop -->
      <div class="modal-backdrop fade" [class.show]="showDetailsModal" *ngIf="showDetailsModal"></div>
    </div>
  `,
  styles: [`
    .system-config-container {
      padding: 2rem;
    }

    .page-header {
      margin-bottom: 2rem;
    }

    .page-header h1 {
      font-size: 2rem;
      font-weight: 700;
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    .card {
      border: 1px solid var(--gray-200);
      border-radius: 12px;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    }

    .card-header {
      background: linear-gradient(135deg, var(--bg-secondary) 0%, var(--bg-tertiary) 100%);
      border-bottom: 1px solid var(--gray-200);
      padding: 1.25rem 1.5rem;
      border-radius: 12px 12px 0 0;
    }

    .card-title {
      font-weight: 600;
      color: var(--text-primary);
    }

    .list-group-item {
      border: none;
      padding: 1rem 1.5rem;
      transition: all 0.15s ease-in-out;
    }

    .list-group-item:hover {
      background-color: var(--bg-tertiary);
    }

    .list-group-item.active {
      background-color: var(--primary);
      color: white;
    }

    .list-group-item.active .text-muted {
      color: rgba(255, 255, 255, 0.7) !important;
    }

    .configuration-item {
      padding: 1.5rem 0;
      border-bottom: 1px solid var(--gray-100);
    }

    .configuration-item:last-child {
      border-bottom: none;
    }

    .form-label {
      font-weight: 500;
      color: var(--text-primary);
    }

    .form-control, .form-select {
      border: 2px solid var(--gray-200);
      border-radius: 8px;
      padding: 0.75rem 1rem;
      transition: all 0.15s ease-in-out;
    }

    .form-control:focus, .form-select:focus {
      border-color: var(--primary);
      box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
    }

    .form-control:disabled {
      background-color: var(--gray-100);
      opacity: 0.7;
    }

    .form-check-input {
      width: 3rem;
      height: 1.5rem;
    }

    .btn {
      border-radius: 8px;
      font-weight: 500;
      padding: 0.625rem 1.25rem;
      transition: all 0.15s ease-in-out;
    }

    .btn-primary {
      background: linear-gradient(135deg, var(--primary) 0%, var(--primary-dark) 100%);
      border: none;
      color: white;
    }

    .btn-primary:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 8px rgba(59, 130, 246, 0.4);
    }

    .btn-success {
      background: linear-gradient(135deg, var(--success) 0%, #059669 100%);
      border: none;
      color: white;
    }

    .btn-success:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 8px rgba(16, 185, 129, 0.4);
    }

    .alert-sm {
      padding: 0.5rem 0.75rem;
      font-size: 0.875rem;
    }

    .modal.show {
      background-color: rgba(0, 0, 0, 0.5);
    }

    .modal-content {
      border-radius: 12px;
      border: none;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
    }

    .modal-header {
      border-bottom: 1px solid var(--gray-200);
      padding: 1.5rem;
    }

    .modal-title {
      font-weight: 600;
      color: var(--text-primary);
    }

    pre {
      font-size: 0.875rem;
      max-height: 200px;
      overflow-y: auto;
    }
  `]
})
export class SystemConfigComponent implements OnInit {
  configurationCategories: ConfigurationCategory[] = [];
  selectedCategory = '';
  selectedCategoryData: ConfigurationCategory | null = null;
  selectedConfig: SystemConfiguration | null = null;
  showDetailsModal = false;
  hasUnsavedChanges = false;
  isSaving = false;

  // Track configuration changes
  configChanges: Record<string, string> = {};
  configErrors: Record<string, string[]> = {};

  constructor(
    private fb: FormBuilder,
    private systemConfigService: SystemConfigService
  ) {}

  ngOnInit(): void {
    this.loadConfigurations();
  }

  private loadConfigurations(): void {
    this.systemConfigService.getConfigurationsByCategory().subscribe({
      next: (categories) => {
        this.configurationCategories = categories;
        if (categories.length > 0) {
          this.selectCategory(categories[0].name);
        }
      },
      error: (error) => {
        console.error('Error loading configurations:', error);
      }
    });
  }

  selectCategory(categoryName: string): void {
    this.selectedCategory = categoryName;
    this.selectedCategoryData = this.configurationCategories.find(c => c.name === categoryName) || null;
  }

  getConfigDisplayName(key: string): string {
    // Convert key like 'app.name' to 'App Name'
    return key.split('.').map(part => 
      part.charAt(0).toUpperCase() + part.slice(1).replace(/_/g, ' ')
    ).join(' ');
  }

  getConfigValue(key: string): string {
    // Return changed value if exists, otherwise original value
    if (this.configChanges.hasOwnProperty(key)) {
      return this.configChanges[key];
    }
    
    const config = this.findConfigByKey(key);
    return config?.value || '';
  }

  updateConfigValue(key: string, event: any): void {
    const value = event.target.value;
    this.configChanges[key] = value;
    this.hasUnsavedChanges = true;
    this.validateConfig(key, value);
  }

  updateConfigBooleanValue(key: string, event: any): void {
    const value = event.target.checked ? 'true' : 'false';
    this.configChanges[key] = value;
    this.hasUnsavedChanges = true;
    this.validateConfig(key, value);
  }

  private validateConfig(key: string, value: string): void {
    const config = this.findConfigByKey(key);
    if (!config) return;

    const errors = this.systemConfigService.validateConfigurationValue(config, value);
    if (errors.length > 0) {
      this.configErrors[key] = errors;
    } else {
      delete this.configErrors[key];
    }
  }

  getConfigErrors(key: string): string[] {
    return this.configErrors[key] || [];
  }

  private findConfigByKey(key: string): SystemConfiguration | null {
    for (const category of this.configurationCategories) {
      const config = category.configurations.find(c => c.key === key);
      if (config) return config;
    }
    return null;
  }

  showConfigDetails(config: SystemConfiguration): void {
    this.selectedConfig = config;
    this.showDetailsModal = true;
  }

  closeDetailsModal(): void {
    this.showDetailsModal = false;
    this.selectedConfig = null;
  }

  resetConfigToDefault(config: SystemConfiguration): void {
    if (confirm(`Reset "${this.getConfigDisplayName(config.key)}" to its default value?`)) {
      this.systemConfigService.resetConfiguration(config.key).subscribe({
        next: (response) => {
          if (response.success) {
            console.log('Configuration reset successfully');
            // Remove from changes and reload
            delete this.configChanges[config.key];
            delete this.configErrors[config.key];
            this.loadConfigurations();
          }
        },
        error: (error) => {
          console.error('Error resetting configuration:', error);
        }
      });
    }
  }

  resetCategoryToDefaults(): void {
    if (!this.selectedCategoryData) return;

    const categoryName = this.selectedCategoryData.displayName;
    if (confirm(`Reset all settings in "${categoryName}" to their default values?`)) {
      // Reset all configurations in the category
      this.selectedCategoryData.configurations.forEach(config => {
        if (config.isEditable) {
          this.resetConfigToDefault(config);
        }
      });
    }
  }

  saveAllChanges(): void {
    if (Object.keys(this.configErrors).length > 0) {
      alert('Please fix validation errors before saving.');
      return;
    }

    this.isSaving = true;
    const savePromises: Promise<any>[] = [];

    // Save each changed configuration
    Object.keys(this.configChanges).forEach(key => {
      const value = this.configChanges[key];
      const promise = this.systemConfigService.updateConfiguration(key, { configurations: [{ key, value }] }).toPromise();
      savePromises.push(promise);
    });

    Promise.all(savePromises).then(() => {
      console.log('All configurations saved successfully');
      this.configChanges = {};
      this.hasUnsavedChanges = false;
      this.isSaving = false;
      this.loadConfigurations(); // Reload to get updated timestamps
    }).catch(error => {
      console.error('Error saving configurations:', error);
      this.isSaving = false;
    });
  }
}