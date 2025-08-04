import { Component, OnInit, OnDestroy, TemplateRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { NgbModal, NgbTooltip, NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { CdkDragDrop, DragDropModule, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';

import { PayrollService } from '../../../services/payroll.service';
import {
  PayslipTemplate,
  PayslipTemplateData,
  PayslipElement,
  PayslipElementType,
  PayslipSection,
  PayslipStyles
} from '../../../models/payroll.models';

@Component({
  selector: 'app-payslip-designer',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    NgbTooltip,
    NgbDropdownModule,
    DragDropModule
  ],
  template: `
    <div class="payslip-designer-container">
      <!-- Header -->
      <div class="page-header d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 class="h3 mb-1">Payslip Designer</h1>
          <p class="text-muted mb-0">Design custom payslip templates with drag-and-drop</p>
        </div>
        <div class="d-flex gap-2">
          <button class="btn btn-outline-secondary" (click)="previewPayslip()">
            <i class="fas fa-eye me-2"></i>Preview
          </button>
          <button class="btn btn-outline-primary" (click)="saveTemplate()">
            <i class="fas fa-save me-2"></i>Save Template
          </button>
          <button class="btn btn-primary" (click)="openTemplateModal()">
            <i class="fas fa-plus me-2"></i>New Template
          </button>
        </div>
      </div>

      <div class="row">
        <!-- Element Palette -->
        <div class="col-md-3">
          <div class="card">
            <div class="card-header">
              <h5 class="card-title mb-0">Elements</h5>
            </div>
            <div class="card-body">
              <div class="element-palette">
                <div class="element-category mb-3">
                  <h6 class="text-muted mb-2">Basic Elements</h6>
                  <div class="element-item" 
                       *ngFor="let element of basicElements"
                       cdkDrag
                       [cdkDragData]="element">
                    <i [class]="element.icon"></i>
                    {{ element.label }}
                  </div>
                </div>

                <div class="element-category mb-3">
                  <h6 class="text-muted mb-2">Payroll Fields</h6>
                  <div class="element-item" 
                       *ngFor="let field of payrollFields"
                       cdkDrag
                       [cdkDragData]="field">
                    <i [class]="field.icon"></i>
                    {{ field.label }}
                  </div>
                </div>

                <div class="element-category">
                  <h6 class="text-muted mb-2">Employee Fields</h6>
                  <div class="element-item" 
                       *ngFor="let field of employeeFields"
                       cdkDrag
                       [cdkDragData]="field">
                    <i [class]="field.icon"></i>
                    {{ field.label }}
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Properties Panel -->
          <div class="card mt-3" *ngIf="selectedElement">
            <div class="card-header">
              <h5 class="card-title mb-0">Properties</h5>
            </div>
            <div class="card-body">
              <form [formGroup]="propertiesForm" (ngSubmit)="updateElementProperties()">
                <div class="mb-3">
                  <label class="form-label">Content</label>
                  <input type="text" class="form-control" formControlName="content">
                </div>

                <div class="row">
                  <div class="col-6">
                    <label class="form-label">Width</label>
                    <input type="number" class="form-control" formControlName="width">
                  </div>
                  <div class="col-6">
                    <label class="form-label">Height</label>
                    <input type="number" class="form-control" formControlName="height">
                  </div>
                </div>

                <div class="row mt-3">
                  <div class="col-6">
                    <label class="form-label">Font Size</label>
                    <input type="number" class="form-control" formControlName="fontSize">
                  </div>
                  <div class="col-6">
                    <label class="form-label">Font Weight</label>
                    <select class="form-select" formControlName="fontWeight">
                      <option value="normal">Normal</option>
                      <option value="bold">Bold</option>
                      <option value="600">Semi-Bold</option>
                    </select>
                  </div>
                </div>

                <div class="mb-3 mt-3">
                  <label class="form-label">Text Align</label>
                  <select class="form-select" formControlName="textAlign">
                    <option value="left">Left</option>
                    <option value="center">Center</option>
                    <option value="right">Right</option>
                  </select>
                </div>

                <div class="mb-3">
                  <label class="form-label">Color</label>
                  <input type="color" class="form-control form-control-color" formControlName="color">
                </div>

                <button type="submit" class="btn btn-primary btn-sm w-100">
                  Update Properties
                </button>
              </form>
            </div>
          </div>
        </div>

        <!-- Design Canvas -->
        <div class="col-md-9">
          <div class="card">
            <div class="card-header d-flex justify-content-between align-items-center">
              <h5 class="card-title mb-0">Payslip Canvas</h5>
              <div class="d-flex gap-2">
                <button class="btn btn-outline-secondary btn-sm" (click)="zoomOut()">
                  <i class="fas fa-search-minus"></i>
                </button>
                <span class="align-self-center">{{ zoomLevel }}%</span>
                <button class="btn btn-outline-secondary btn-sm" (click)="zoomIn()">
                  <i class="fas fa-search-plus"></i>
                </button>
              </div>
            </div>
            <div class="card-body p-0">
              <div class="design-canvas" 
                   [style.transform]="'scale(' + zoomLevel / 100 + ')'"
                   cdkDropList
                   [cdkDropListData]="canvasElements"
                   (cdkDropListDropped)="onElementDrop($event)">
                
                <!-- Canvas Background -->
                <div class="payslip-canvas">
                  <!-- Header Section -->
                  <div class="payslip-section header-section"
                       cdkDropList
                       [cdkDropListData]="templateData.header.elements"
                       (cdkDropListDropped)="onSectionDrop($event, 'header')">
                    <div class="section-label">Header</div>
                    <div class="payslip-element"
                         *ngFor="let element of templateData.header.elements; let i = index"
                         [class.selected]="selectedElement === element"
                         (click)="selectElement(element)"
                         cdkDrag
                         [cdkDragData]="element">
                      <div [innerHTML]="renderElement(element)"></div>
                      <div class="element-controls">
                        <button class="btn btn-sm btn-outline-danger" (click)="removeElement(element, 'header')">
                          <i class="fas fa-times"></i>
                        </button>
                      </div>
                    </div>
                  </div>

                  <!-- Employee Info Section -->
                  <div class="payslip-section employee-section"
                       cdkDropList
                       [cdkDropListData]="templateData.employeeInfo.elements"
                       (cdkDropListDropped)="onSectionDrop($event, 'employeeInfo')">
                    <div class="section-label">Employee Information</div>
                    <div class="payslip-element"
                         *ngFor="let element of templateData.employeeInfo.elements; let i = index"
                         [class.selected]="selectedElement === element"
                         (click)="selectElement(element)"
                         cdkDrag
                         [cdkDragData]="element">
                      <div [innerHTML]="renderElement(element)"></div>
                      <div class="element-controls">
                        <button class="btn btn-sm btn-outline-danger" (click)="removeElement(element, 'employeeInfo')">
                          <i class="fas fa-times"></i>
                        </button>
                      </div>
                    </div>
                  </div>

                  <!-- Earnings Section -->
                  <div class="payslip-section earnings-section"
                       cdkDropList
                       [cdkDropListData]="templateData.earnings.elements"
                       (cdkDropListDropped)="onSectionDrop($event, 'earnings')">
                    <div class="section-label">Earnings</div>
                    <div class="payslip-element"
                         *ngFor="let element of templateData.earnings.elements; let i = index"
                         [class.selected]="selectedElement === element"
                         (click)="selectElement(element)"
                         cdkDrag
                         [cdkDragData]="element">
                      <div [innerHTML]="renderElement(element)"></div>
                      <div class="element-controls">
                        <button class="btn btn-sm btn-outline-danger" (click)="removeElement(element, 'earnings')">
                          <i class="fas fa-times"></i>
                        </button>
                      </div>
                    </div>
                  </div>

                  <!-- Deductions Section -->
                  <div class="payslip-section deductions-section"
                       cdkDropList
                       [cdkDropListData]="templateData.deductions.elements"
                       (cdkDropListDropped)="onSectionDrop($event, 'deductions')">
                    <div class="section-label">Deductions</div>
                    <div class="payslip-element"
                         *ngFor="let element of templateData.deductions.elements; let i = index"
                         [class.selected]="selectedElement === element"
                         (click)="selectElement(element)"
                         cdkDrag
                         [cdkDragData]="element">
                      <div [innerHTML]="renderElement(element)"></div>
                      <div class="element-controls">
                        <button class="btn btn-sm btn-outline-danger" (click)="removeElement(element, 'deductions')">
                          <i class="fas fa-times"></i>
                        </button>
                      </div>
                    </div>
                  </div>

                  <!-- Summary Section -->
                  <div class="payslip-section summary-section"
                       cdkDropList
                       [cdkDropListData]="templateData.summary.elements"
                       (cdkDropListDropped)="onSectionDrop($event, 'summary')">
                    <div class="section-label">Summary</div>
                    <div class="payslip-element"
                         *ngFor="let element of templateData.summary.elements; let i = index"
                         [class.selected]="selectedElement === element"
                         (click)="selectElement(element)"
                         cdkDrag
                         [cdkDragData]="element">
                      <div [innerHTML]="renderElement(element)"></div>
                      <div class="element-controls">
                        <button class="btn btn-sm btn-outline-danger" (click)="removeElement(element, 'summary')">
                          <i class="fas fa-times"></i>
                        </button>
                      </div>
                    </div>
                  </div>

                  <!-- Footer Section -->
                  <div class="payslip-section footer-section"
                       cdkDropList
                       [cdkDropListData]="templateData.footer.elements"
                       (cdkDropListDropped)="onSectionDrop($event, 'footer')">
                    <div class="section-label">Footer</div>
                    <div class="payslip-element"
                         *ngFor="let element of templateData.footer.elements; let i = index"
                         [class.selected]="selectedElement === element"
                         (click)="selectElement(element)"
                         cdkDrag
                         [cdkDragData]="element">
                      <div [innerHTML]="renderElement(element)"></div>
                      <div class="element-controls">
                        <button class="btn btn-sm btn-outline-danger" (click)="removeElement(element, 'footer')">
                          <i class="fas fa-times"></i>
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- New Template Modal -->
    <ng-template #newTemplateModal let-modal>
      <div class="modal-header">
        <h4 class="modal-title">Create New Template</h4>
        <button type="button" class="btn-close" (click)="modal.dismiss()"></button>
      </div>
      <form [formGroup]="templateForm" (ngSubmit)="createTemplate(modal)">
        <div class="modal-body">
          <div class="mb-3">
            <label class="form-label">Template Name *</label>
            <input type="text" class="form-control" formControlName="name" 
                   placeholder="e.g., Standard Payslip Template">
          </div>

          <div class="mb-3">
            <label class="form-label">Description</label>
            <textarea class="form-control" formControlName="description" rows="3"
                      placeholder="Brief description of the template"></textarea>
          </div>

          <div class="mb-3">
            <label class="form-label">Branch</label>
            <select class="form-select" formControlName="branchId">
              <option value="">All Branches</option>
              <option *ngFor="let branch of branches" [value]="branch.id">
                {{ branch.name }}
              </option>
            </select>
          </div>

          <div class="form-check">
            <input class="form-check-input" type="checkbox" formControlName="isDefault" id="isDefault">
            <label class="form-check-label" for="isDefault">
              Set as default template
            </label>
          </div>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" (click)="modal.dismiss()">
            Cancel
          </button>
          <button type="submit" class="btn btn-primary" 
                  [disabled]="templateForm.invalid || creating">
            <span *ngIf="creating" class="spinner-border spinner-border-sm me-2"></span>
            Create Template
          </button>
        </div>
      </form>
    </ng-template>
  `,
  styles: [`
    .payslip-designer-container {
      padding: 1.5rem;
      height: 100vh;
      overflow: hidden;
    }

    .element-palette {
      max-height: 400px;
      overflow-y: auto;
    }

    .element-item {
      padding: 0.75rem;
      margin-bottom: 0.5rem;
      background: var(--bg-secondary);
      border: 1px solid var(--gray-200);
      border-radius: 8px;
      cursor: grab;
      transition: all 0.2s ease;
    }

    .element-item:hover {
      background: var(--primary);
      color: white;
      transform: translateY(-1px);
    }

    .element-item i {
      margin-right: 0.5rem;
    }

    .design-canvas {
      padding: 2rem;
      background: #f8f9fa;
      min-height: 600px;
      transform-origin: top left;
      transition: transform 0.2s ease;
    }

    .payslip-canvas {
      width: 210mm;
      min-height: 297mm;
      background: white;
      margin: 0 auto;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
      position: relative;
    }

    .payslip-section {
      min-height: 80px;
      border: 2px dashed transparent;
      padding: 1rem;
      position: relative;
      transition: all 0.2s ease;
    }

    .payslip-section:hover {
      border-color: var(--primary);
      background: rgba(59, 130, 246, 0.05);
    }

    .payslip-section.cdk-drop-list-dragging {
      border-color: var(--primary);
      background: rgba(59, 130, 246, 0.1);
    }

    .section-label {
      position: absolute;
      top: 0.25rem;
      left: 0.5rem;
      font-size: 0.75rem;
      color: var(--text-muted);
      background: white;
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
      opacity: 0;
      transition: opacity 0.2s ease;
    }

    .payslip-section:hover .section-label {
      opacity: 1;
    }

    .payslip-element {
      position: relative;
      padding: 0.5rem;
      margin: 0.25rem 0;
      border: 1px solid transparent;
      border-radius: 4px;
      cursor: pointer;
      transition: all 0.2s ease;
    }

    .payslip-element:hover {
      border-color: var(--gray-300);
      background: rgba(0, 0, 0, 0.02);
    }

    .payslip-element.selected {
      border-color: var(--primary);
      background: rgba(59, 130, 246, 0.1);
    }

    .element-controls {
      position: absolute;
      top: -8px;
      right: -8px;
      opacity: 0;
      transition: opacity 0.2s ease;
    }

    .payslip-element:hover .element-controls,
    .payslip-element.selected .element-controls {
      opacity: 1;
    }

    .element-controls .btn {
      width: 24px;
      height: 24px;
      padding: 0;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .cdk-drag-preview {
      box-sizing: border-box;
      border-radius: 4px;
      box-shadow: 0 5px 5px -3px rgba(0, 0, 0, 0.2),
                  0 8px 10px 1px rgba(0, 0, 0, 0.14),
                  0 3px 14px 2px rgba(0, 0, 0, 0.12);
    }

    .cdk-drag-placeholder {
      opacity: 0;
    }

    .cdk-drag-animating {
      transition: transform 250ms cubic-bezier(0, 0, 0.2, 1);
    }

    .cdk-drop-list-dragging .payslip-element:not(.cdk-drag-placeholder) {
      transition: transform 250ms cubic-bezier(0, 0, 0.2, 1);
    }

    @media (max-width: 768px) {
      .payslip-designer-container {
        padding: 1rem;
      }
      
      .payslip-canvas {
        width: 100%;
        min-height: auto;
      }
    }
  `]
})
export class PayslipDesignerComponent implements OnInit, OnDestroy {
  @ViewChild('newTemplateModal') newTemplateModal!: TemplateRef<any>;
  
  private destroy$ = new Subject<void>();

  // Data
  templates: PayslipTemplate[] = [];
  branches: any[] = [];
  canvasElements: PayslipElement[] = [];
  selectedElement: PayslipElement | null = null;
  
  // Template Data
  templateData: PayslipTemplateData = {
    header: { elements: [], styles: {} },
    employeeInfo: { elements: [], styles: {} },
    earnings: { elements: [], styles: {} },
    deductions: { elements: [], styles: {} },
    summary: { elements: [], styles: {} },
    footer: { elements: [], styles: {} }
  };
  
  // UI State
  loading = false;
  creating = false;
  zoomLevel = 100;
  
  // Forms
  templateForm: FormGroup;
  propertiesForm: FormGroup;
  
  // Element Definitions
  basicElements = [
    { type: PayslipElementType.Text, label: 'Text', icon: 'fas fa-font' },
    { type: PayslipElementType.Image, label: 'Image', icon: 'fas fa-image' },
    { type: PayslipElementType.Line, label: 'Line', icon: 'fas fa-minus' },
    { type: PayslipElementType.Table, label: 'Table', icon: 'fas fa-table' }
  ];

  payrollFields = [
    { type: PayslipElementType.Field, label: 'Basic Salary', icon: 'fas fa-dollar-sign', field: 'basicSalary' },
    { type: PayslipElementType.Field, label: 'Gross Salary', icon: 'fas fa-money-bill', field: 'grossSalary' },
    { type: PayslipElementType.Field, label: 'Net Salary', icon: 'fas fa-hand-holding-usd', field: 'netSalary' },
    { type: PayslipElementType.Field, label: 'Total Deductions', icon: 'fas fa-minus-circle', field: 'totalDeductions' }
  ];

  employeeFields = [
    { type: PayslipElementType.Field, label: 'Employee Name', icon: 'fas fa-user', field: 'employeeName' },
    { type: PayslipElementType.Field, label: 'Employee ID', icon: 'fas fa-id-badge', field: 'employeeCode' },
    { type: PayslipElementType.Field, label: 'Department', icon: 'fas fa-building', field: 'department' },
    { type: PayslipElementType.Field, label: 'Designation', icon: 'fas fa-briefcase', field: 'designation' }
  ];

  constructor(
    private payrollService: PayrollService,
    private modalService: NgbModal,
    private fb: FormBuilder
  ) {
    this.templateForm = this.fb.group({
      name: ['', [Validators.required]],
      description: [''],
      branchId: [''],
      isDefault: [false]
    });

    this.propertiesForm = this.fb.group({
      content: [''],
      width: [100],
      height: [30],
      fontSize: [14],
      fontWeight: ['normal'],
      textAlign: ['left'],
      color: ['#000000']
    });
  }

  ngOnInit(): void {
    this.loadTemplates();
    this.initializeTemplate();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadTemplates(): void {
    this.payrollService.getPayslipTemplates()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (templates) => {
          this.templates = templates;
        },
        error: (error) => {
          console.error('Error loading templates:', error);
        }
      });
  }

  private initializeTemplate(): void {
    // Initialize with empty template structure
    this.templateData = {
      header: { elements: [], styles: {} },
      employeeInfo: { elements: [], styles: {} },
      earnings: { elements: [], styles: {} },
      deductions: { elements: [], styles: {} },
      summary: { elements: [], styles: {} },
      footer: { elements: [], styles: {} }
    };
  }

  onElementDrop(event: CdkDragDrop<PayslipElement[]>): void {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      const elementData = event.previousContainer.data[event.previousIndex];
      const newElement = this.createElementFromData(elementData);
      transferArrayItem([newElement], event.container.data, 0, event.currentIndex);
    }
  }

  onSectionDrop(event: CdkDragDrop<PayslipElement[]>, sectionName: string): void {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      const elementData = event.item.data;
      const newElement = this.createElementFromData(elementData);
      event.container.data.splice(event.currentIndex, 0, newElement);
    }
  }

  private createElementFromData(data: any): PayslipElement {
    return {
      id: this.generateElementId(),
      type: data.type,
      content: data.field ? `{{${data.field}}}` : data.label,
      position: { x: 0, y: 0 },
      size: { width: 100, height: 30 },
      styles: {
        fontSize: 14,
        fontWeight: 'normal',
        color: '#000000',
        textAlign: 'left'
      }
    };
  }

  private generateElementId(): string {
    return 'element_' + Math.random().toString(36).substr(2, 9);
  }

  selectElement(element: PayslipElement): void {
    this.selectedElement = element;
    this.propertiesForm.patchValue({
      content: element.content,
      width: element.size.width,
      height: element.size.height,
      fontSize: element.styles.fontSize || 14,
      fontWeight: element.styles.fontWeight || 'normal',
      textAlign: element.styles.textAlign || 'left',
      color: element.styles.color || '#000000'
    });
  }

  updateElementProperties(): void {
    if (this.selectedElement) {
      const formValue = this.propertiesForm.value;
      this.selectedElement.content = formValue.content;
      this.selectedElement.size.width = formValue.width;
      this.selectedElement.size.height = formValue.height;
      this.selectedElement.styles = {
        ...this.selectedElement.styles,
        fontSize: formValue.fontSize,
        fontWeight: formValue.fontWeight,
        textAlign: formValue.textAlign,
        color: formValue.color
      };
    }
  }

  removeElement(element: PayslipElement, sectionName: string): void {
    const section = (this.templateData as any)[sectionName];
    const index = section.elements.indexOf(element);
    if (index > -1) {
      section.elements.splice(index, 1);
    }
    if (this.selectedElement === element) {
      this.selectedElement = null;
    }
  }

  renderElement(element: PayslipElement): string {
    const styles = `
      font-size: ${element.styles.fontSize || 14}px;
      font-weight: ${element.styles.fontWeight || 'normal'};
      color: ${element.styles.color || '#000000'};
      text-align: ${element.styles.textAlign || 'left'};
      width: ${element.size.width}px;
      height: ${element.size.height}px;
    `;
    
    return `<div style="${styles}">${element.content}</div>`;
  }

  zoomIn(): void {
    if (this.zoomLevel < 200) {
      this.zoomLevel += 25;
    }
  }

  zoomOut(): void {
    if (this.zoomLevel > 50) {
      this.zoomLevel -= 25;
    }
  }

  openTemplateModal(): void {
    this.modalService.open(this.newTemplateModal, { 
      size: 'lg',
      backdrop: 'static'
    });
  }

  createTemplate(modal: any): void {
    if (this.templateForm.valid) {
      this.creating = true;
      
      const formValue = this.templateForm.value;
      const template: Omit<PayslipTemplate, 'id' | 'createdAt' | 'updatedAt'> = {
        name: formValue.name,
        description: formValue.description,
        templateData: this.templateData,
        isDefault: formValue.isDefault,
        branchId: formValue.branchId || undefined
      };

      this.payrollService.createPayslipTemplate(template)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (newTemplate) => {
            this.templates.unshift(newTemplate);
            this.creating = false;
            modal.close();
            this.templateForm.reset();
          },
          error: (error) => {
            console.error('Error creating template:', error);
            this.creating = false;
          }
        });
    }
  }

  saveTemplate(): void {
    // Implementation for saving current template
    console.log('Saving template with data:', this.templateData);
  }

  previewPayslip(): void {
    // Implementation for previewing payslip
    console.log('Previewing payslip with template data:', this.templateData);
  }
}