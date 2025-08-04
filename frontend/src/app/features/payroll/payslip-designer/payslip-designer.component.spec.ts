import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { of, throwError } from 'rxjs';

import { PayslipDesignerComponent } from './payslip-designer.component';
import { PayrollService } from '../../../services/payroll.service';
import { PayslipTemplate, PayslipElement, PayslipElementType } from '../../../models/payroll.models';

describe('PayslipDesignerComponent', () => {
  let component: PayslipDesignerComponent;
  let fixture: ComponentFixture<PayslipDesignerComponent>;
  let mockPayrollService: jasmine.SpyObj<PayrollService>;
  let mockModalService: jasmine.SpyObj<NgbModal>;

  const mockPayslipTemplate: PayslipTemplate = {
    id: 1,
    name: 'Standard Template',
    description: 'Standard payslip template',
    templateData: {
      header: { elements: [], styles: {} },
      employeeInfo: { elements: [], styles: {} },
      earnings: { elements: [], styles: {} },
      deductions: { elements: [], styles: {} },
      summary: { elements: [], styles: {} },
      footer: { elements: [], styles: {} }
    },
    isDefault: true,
    branchId: 1,
    createdAt: new Date('2024-01-01'),
    updatedAt: new Date('2024-01-01')
  };

  const mockElement: PayslipElement = {
    id: 'element_1',
    type: PayslipElementType.Text,
    content: 'Sample Text',
    position: { x: 0, y: 0 },
    size: { width: 100, height: 30 },
    styles: {
      fontSize: 14,
      fontWeight: 'normal',
      color: '#000000',
      textAlign: 'left'
    }
  };

  beforeEach(async () => {
    const payrollServiceSpy = jasmine.createSpyObj('PayrollService', [
      'getPayslipTemplates',
      'createPayslipTemplate',
      'updatePayslipTemplate',
      'deletePayslipTemplate'
    ]);

    const modalServiceSpy = jasmine.createSpyObj('NgbModal', ['open']);

    await TestBed.configureTestingModule({
      imports: [PayslipDesignerComponent, ReactiveFormsModule],
      providers: [
        FormBuilder,
        { provide: PayrollService, useValue: payrollServiceSpy },
        { provide: NgbModal, useValue: modalServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PayslipDesignerComponent);
    component = fixture.componentInstance;
    mockPayrollService = TestBed.inject(PayrollService) as jasmine.SpyObj<PayrollService>;
    mockModalService = TestBed.inject(NgbModal) as jasmine.SpyObj<NgbModal>;

    // Setup default mock returns
    mockPayrollService.getPayslipTemplates.and.returnValue(of([mockPayslipTemplate]));
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load templates on init', () => {
    component.ngOnInit();

    expect(mockPayrollService.getPayslipTemplates).toHaveBeenCalled();
    expect(component.templates).toEqual([mockPayslipTemplate]);
  });

  it('should handle template loading error gracefully', () => {
    mockPayrollService.getPayslipTemplates.and.returnValue(throwError('API Error'));
    spyOn(console, 'error');

    component.ngOnInit();

    expect(console.error).toHaveBeenCalledWith('Error loading templates:', 'API Error');
  });

  it('should initialize template data structure', () => {
    component.ngOnInit();

    expect(component.templateData.header).toBeDefined();
    expect(component.templateData.employeeInfo).toBeDefined();
    expect(component.templateData.earnings).toBeDefined();
    expect(component.templateData.deductions).toBeDefined();
    expect(component.templateData.summary).toBeDefined();
    expect(component.templateData.footer).toBeDefined();
  });

  it('should generate unique element ID', () => {
    const id1 = (component as any).generateElementId();
    const id2 = (component as any).generateElementId();

    expect(id1).toMatch(/^element_[a-z0-9]{9}$/);
    expect(id2).toMatch(/^element_[a-z0-9]{9}$/);
    expect(id1).not.toBe(id2);
  });

  it('should create element from data', () => {
    const elementData = {
      type: PayslipElementType.Text,
      label: 'Test Label',
      field: 'testField'
    };

    const element = (component as any).createElementFromData(elementData);

    expect(element.type).toBe(PayslipElementType.Text);
    expect(element.content).toBe('{{testField}}');
    expect(element.size).toEqual({ width: 100, height: 30 });
    expect(element.styles.fontSize).toBe(14);
  });

  it('should create element without field', () => {
    const elementData = {
      type: PayslipElementType.Text,
      label: 'Test Label'
    };

    const element = (component as any).createElementFromData(elementData);

    expect(element.content).toBe('Test Label');
  });

  it('should select element and update properties form', () => {
    component.selectElement(mockElement);

    expect(component.selectedElement).toBe(mockElement);
    expect(component.propertiesForm.get('content')?.value).toBe(mockElement.content);
    expect(component.propertiesForm.get('width')?.value).toBe(mockElement.size.width);
    expect(component.propertiesForm.get('height')?.value).toBe(mockElement.size.height);
  });

  it('should update element properties', () => {
    component.selectedElement = mockElement;
    component.propertiesForm.patchValue({
      content: 'Updated Content',
      width: 150,
      height: 40,
      fontSize: 16,
      fontWeight: 'bold',
      textAlign: 'center',
      color: '#ff0000'
    });

    component.updateElementProperties();

    expect(component.selectedElement.content).toBe('Updated Content');
    expect(component.selectedElement.size.width).toBe(150);
    expect(component.selectedElement.size.height).toBe(40);
    expect(component.selectedElement.styles.fontSize).toBe(16);
    expect(component.selectedElement.styles.fontWeight).toBe('bold');
    expect(component.selectedElement.styles.textAlign).toBe('center');
    expect(component.selectedElement.styles.color).toBe('#ff0000');
  });

  it('should not update properties without selected element', () => {
    component.selectedElement = null;
    const originalElement = { ...mockElement };

    component.updateElementProperties();

    expect(mockElement).toEqual(originalElement);
  });

  it('should remove element from section', () => {
    component.templateData.header.elements = [mockElement];
    component.selectedElement = mockElement;

    component.removeElement(mockElement, 'header');

    expect(component.templateData.header.elements).not.toContain(mockElement);
    expect(component.selectedElement).toBeNull();
  });

  it('should not remove element if not found in section', () => {
    component.templateData.header.elements = [];

    component.removeElement(mockElement, 'header');

    expect(component.templateData.header.elements.length).toBe(0);
  });

  it('should render element with correct styles', () => {
    const element = {
      ...mockElement,
      content: 'Test Content',
      size: { width: 200, height: 50 },
      styles: {
        fontSize: 18,
        fontWeight: 'bold',
        color: '#ff0000',
        textAlign: 'center'
      }
    };

    const rendered = component.renderElement(element);

    expect(rendered).toContain('Test Content');
    expect(rendered).toContain('font-size: 18px');
    expect(rendered).toContain('font-weight: bold');
    expect(rendered).toContain('color: #ff0000');
    expect(rendered).toContain('text-align: center');
    expect(rendered).toContain('width: 200px');
    expect(rendered).toContain('height: 50px');
  });

  it('should zoom in correctly', () => {
    component.zoomLevel = 100;

    component.zoomIn();

    expect(component.zoomLevel).toBe(125);
  });

  it('should not zoom in beyond maximum', () => {
    component.zoomLevel = 200;

    component.zoomIn();

    expect(component.zoomLevel).toBe(200);
  });

  it('should zoom out correctly', () => {
    component.zoomLevel = 100;

    component.zoomOut();

    expect(component.zoomLevel).toBe(75);
  });

  it('should not zoom out beyond minimum', () => {
    component.zoomLevel = 50;

    component.zoomOut();

    expect(component.zoomLevel).toBe(50);
  });

  it('should open template modal', () => {
    const mockModalRef = { close: jasmine.createSpy(), dismiss: jasmine.createSpy() };
    mockModalService.open.and.returnValue(mockModalRef as any);

    component.openTemplateModal();

    expect(mockModalService.open).toHaveBeenCalled();
  });

  it('should create template successfully', () => {
    component.templateForm.patchValue({
      name: 'New Template',
      description: 'Test description',
      branchId: 1,
      isDefault: false
    });

    const mockModal = { close: jasmine.createSpy() };
    mockPayrollService.createPayslipTemplate.and.returnValue(of(mockPayslipTemplate));

    component.createTemplate(mockModal);

    expect(mockPayrollService.createPayslipTemplate).toHaveBeenCalled();
    expect(mockModal.close).toHaveBeenCalled();
    expect(component.templates).toContain(mockPayslipTemplate);
  });

  it('should not create template with invalid form', () => {
    const mockModal = { close: jasmine.createSpy() };
    component.templateForm.patchValue({ name: '' }); // Invalid form

    component.createTemplate(mockModal);

    expect(mockPayrollService.createPayslipTemplate).not.toHaveBeenCalled();
    expect(mockModal.close).not.toHaveBeenCalled();
  });

  it('should handle template creation error', () => {
    component.templateForm.patchValue({
      name: 'New Template',
      description: 'Test description',
      branchId: 1,
      isDefault: false
    });

    const mockModal = { close: jasmine.createSpy() };
    mockPayrollService.createPayslipTemplate.and.returnValue(throwError('API Error'));
    spyOn(console, 'error');

    component.createTemplate(mockModal);

    expect(console.error).toHaveBeenCalledWith('Error creating template:', 'API Error');
    expect(component.creating).toBeFalse();
  });

  it('should handle element drop from palette', () => {
    const mockEvent = {
      previousContainer: { data: [{ type: PayslipElementType.Text, label: 'Text' }] },
      container: { data: [] },
      previousIndex: 0,
      currentIndex: 0,
      item: { data: { type: PayslipElementType.Text, label: 'Text' } }
    } as any;

    spyOn(component as any, 'createElementFromData').and.returnValue(mockElement);

    component.onElementDrop(mockEvent);

    expect((component as any).createElementFromData).toHaveBeenCalled();
  });

  it('should handle section drop', () => {
    const mockEvent = {
      previousContainer: { data: [{ type: PayslipElementType.Text, label: 'Text' }] },
      container: { data: [] },
      previousIndex: 0,
      currentIndex: 0,
      item: { data: { type: PayslipElementType.Text, label: 'Text' } }
    } as any;

    spyOn(component as any, 'createElementFromData').and.returnValue(mockElement);

    component.onSectionDrop(mockEvent, 'header');

    expect((component as any).createElementFromData).toHaveBeenCalled();
    expect(mockEvent.container.data).toContain(mockElement);
  });

  it('should validate template form', () => {
    expect(component.templateForm.valid).toBeFalse();

    component.templateForm.patchValue({
      name: 'Test Template',
      description: 'Test description',
      branchId: 1,
      isDefault: false
    });

    expect(component.templateForm.valid).toBeTrue();
  });

  it('should validate properties form', () => {
    expect(component.propertiesForm.valid).toBeTrue(); // All fields are optional

    component.propertiesForm.patchValue({
      content: 'Test Content',
      width: 100,
      height: 30,
      fontSize: 14,
      fontWeight: 'normal',
      textAlign: 'left',
      color: '#000000'
    });

    expect(component.propertiesForm.valid).toBeTrue();
  });

  it('should handle save template action', () => {
    spyOn(console, 'log');

    component.saveTemplate();

    expect(console.log).toHaveBeenCalledWith('Saving template with data:', component.templateData);
  });

  it('should handle preview payslip action', () => {
    spyOn(console, 'log');

    component.previewPayslip();

    expect(console.log).toHaveBeenCalledWith('Previewing payslip with template data:', component.templateData);
  });

  it('should have correct basic elements', () => {
    expect(component.basicElements).toEqual([
      { type: PayslipElementType.Text, label: 'Text', icon: 'fas fa-font' },
      { type: PayslipElementType.Image, label: 'Image', icon: 'fas fa-image' },
      { type: PayslipElementType.Line, label: 'Line', icon: 'fas fa-minus' },
      { type: PayslipElementType.Table, label: 'Table', icon: 'fas fa-table' }
    ]);
  });

  it('should have correct payroll fields', () => {
    expect(component.payrollFields).toContain(
      jasmine.objectContaining({ field: 'basicSalary', label: 'Basic Salary' })
    );
    expect(component.payrollFields).toContain(
      jasmine.objectContaining({ field: 'grossSalary', label: 'Gross Salary' })
    );
    expect(component.payrollFields).toContain(
      jasmine.objectContaining({ field: 'netSalary', label: 'Net Salary' })
    );
  });

  it('should have correct employee fields', () => {
    expect(component.employeeFields).toContain(
      jasmine.objectContaining({ field: 'employeeName', label: 'Employee Name' })
    );
    expect(component.employeeFields).toContain(
      jasmine.objectContaining({ field: 'employeeCode', label: 'Employee ID' })
    );
    expect(component.employeeFields).toContain(
      jasmine.objectContaining({ field: 'department', label: 'Department' })
    );
  });
});