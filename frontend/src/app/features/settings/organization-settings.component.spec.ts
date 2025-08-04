import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { OrganizationSettingsComponent } from './organization-settings.component';
import { OrganizationService } from '../../services/organization.service';
import { Organization, ApiResponse } from '../../models/admin.models';

describe('OrganizationSettingsComponent', () => {
  let component: OrganizationSettingsComponent;
  let fixture: ComponentFixture<OrganizationSettingsComponent>;
  let organizationService: jasmine.SpyObj<OrganizationService>;

  const mockOrganization: Organization = {
    id: 1,
    name: 'Test Organization',
    address: '123 Test St',
    email: 'test@example.com',
    phone: '123-456-7890',
    normalWorkingHours: '08:00',
    overtimeRate: 1.5,
    productiveHoursThreshold: 8,
    branchIsolationEnabled: false,
    createdAt: new Date()
  };

  beforeEach(async () => {
    const spy = jasmine.createSpyObj('OrganizationService', [
      'getOrganization',
      'createOrganization',
      'updateOrganization',
      'uploadLogo',
      'getLogo',
      'deleteLogo'
    ]);

    await TestBed.configureTestingModule({
      imports: [
        OrganizationSettingsComponent,
        ReactiveFormsModule,
        RouterTestingModule,
        HttpClientTestingModule
      ],
      providers: [
        { provide: OrganizationService, useValue: spy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(OrganizationSettingsComponent);
    component = fixture.componentInstance;
    organizationService = TestBed.inject(OrganizationService) as jasmine.SpyObj<OrganizationService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with default values', () => {
    expect(component.organizationForm).toBeDefined();
    expect(component.organizationForm.get('normalWorkingHours')?.value).toBe('08:00');
    expect(component.organizationForm.get('overtimeRate')?.value).toBe(1.5);
    expect(component.organizationForm.get('productiveHoursThreshold')?.value).toBe(8);
    expect(component.organizationForm.get('branchIsolationEnabled')?.value).toBe(false);
  });

  it('should load organization on init', () => {
    const mockResponse: ApiResponse<Organization> = {
      success: true,
      data: mockOrganization,
      message: 'Organization retrieved successfully'
    };

    organizationService.getOrganization.and.returnValue(of(mockResponse));
    organizationService.getLogo.and.returnValue(throwError('No logo found'));

    component.ngOnInit();

    expect(organizationService.getOrganization).toHaveBeenCalledWith(1);
    expect(component.currentOrganization).toEqual(mockOrganization);
  });

  it('should populate form when organization is loaded', () => {
    const mockResponse: ApiResponse<Organization> = {
      success: true,
      data: mockOrganization,
      message: 'Organization retrieved successfully'
    };

    organizationService.getOrganization.and.returnValue(of(mockResponse));
    organizationService.getLogo.and.returnValue(throwError('No logo found'));

    component.ngOnInit();

    expect(component.organizationForm.get('name')?.value).toBe(mockOrganization.name);
    expect(component.organizationForm.get('email')?.value).toBe(mockOrganization.email);
    expect(component.organizationForm.get('phone')?.value).toBe(mockOrganization.phone);
    expect(component.organizationForm.get('address')?.value).toBe(mockOrganization.address);
  });

  it('should validate required fields', () => {
    const nameControl = component.organizationForm.get('name');
    const emailControl = component.organizationForm.get('email');
    const phoneControl = component.organizationForm.get('phone');
    const addressControl = component.organizationForm.get('address');

    // Set empty values
    nameControl?.setValue('');
    emailControl?.setValue('');
    phoneControl?.setValue('');
    addressControl?.setValue('');

    // Mark as touched to trigger validation
    nameControl?.markAsTouched();
    emailControl?.markAsTouched();
    phoneControl?.markAsTouched();
    addressControl?.markAsTouched();

    expect(nameControl?.invalid).toBeTruthy();
    expect(emailControl?.invalid).toBeTruthy();
    expect(phoneControl?.invalid).toBeTruthy();
    expect(addressControl?.invalid).toBeTruthy();
  });

  it('should validate email format', () => {
    const emailControl = component.organizationForm.get('email');
    
    emailControl?.setValue('invalid-email');
    emailControl?.markAsTouched();
    
    expect(emailControl?.invalid).toBeTruthy();
    expect(emailControl?.hasError('email')).toBeTruthy();

    emailControl?.setValue('valid@example.com');
    expect(emailControl?.valid).toBeTruthy();
  });

  it('should validate numeric fields', () => {
    const overtimeRateControl = component.organizationForm.get('overtimeRate');
    const productiveHoursControl = component.organizationForm.get('productiveHoursThreshold');

    // Test minimum values
    overtimeRateControl?.setValue(0.5);
    productiveHoursControl?.setValue(0);

    expect(overtimeRateControl?.invalid).toBeTruthy();
    expect(productiveHoursControl?.invalid).toBeTruthy();

    // Test valid values
    overtimeRateControl?.setValue(1.5);
    productiveHoursControl?.setValue(8);

    expect(overtimeRateControl?.valid).toBeTruthy();
    expect(productiveHoursControl?.valid).toBeTruthy();
  });

  it('should save organization when form is valid', () => {
    component.currentOrganization = mockOrganization;
    
    const mockResponse: ApiResponse<Organization> = {
      success: true,
      data: mockOrganization,
      message: 'Organization updated successfully'
    };

    organizationService.updateOrganization.and.returnValue(of(mockResponse));

    // Set valid form data
    component.organizationForm.patchValue({
      name: 'Updated Organization',
      email: 'updated@example.com',
      phone: '987-654-3210',
      address: '456 Updated St',
      normalWorkingHours: '09:00',
      overtimeRate: 2.0,
      productiveHoursThreshold: 7,
      branchIsolationEnabled: true
    });

    component.saveOrganization();

    expect(organizationService.updateOrganization).toHaveBeenCalled();
    expect(component.isLoading).toBeFalsy();
  });

  it('should not save when form is invalid', () => {
    // Set invalid form data
    component.organizationForm.patchValue({
      name: '',
      email: 'invalid-email',
      phone: '',
      address: ''
    });

    component.saveOrganization();

    expect(organizationService.updateOrganization).not.toHaveBeenCalled();
    expect(organizationService.createOrganization).not.toHaveBeenCalled();
  });

  it('should handle file selection for logo upload', () => {
    component.currentOrganization = mockOrganization;
    
    const mockFile = new File(['test'], 'logo.png', { type: 'image/png' });
    const mockEvent = {
      target: {
        files: [mockFile]
      }
    };

    const mockResponse: ApiResponse<string> = {
      success: true,
      data: '/uploads/logo.png',
      message: 'Logo uploaded successfully'
    };

    organizationService.uploadLogo.and.returnValue(of(mockResponse));
    organizationService.getLogo.and.returnValue(of(new Blob()));

    spyOn(component as any, 'uploadLogo');

    component.onFileSelected(mockEvent);

    expect((component as any).uploadLogo).toHaveBeenCalledWith(mockFile);
  });

  it('should reset form to original values', () => {
    component.currentOrganization = mockOrganization;
    
    // Populate form with organization data
    (component as any).populateForm(mockOrganization);
    
    // Change some values
    component.organizationForm.patchValue({
      name: 'Changed Name',
      email: 'changed@example.com'
    });

    // Reset form
    component.resetForm();

    // Should be back to original values
    expect(component.organizationForm.get('name')?.value).toBe(mockOrganization.name);
    expect(component.organizationForm.get('email')?.value).toBe(mockOrganization.email);
  });

  it('should display organization stats', () => {
    expect(component.organizationStats).toBeDefined();
    expect(component.organizationStats.totalBranches).toBe(3);
    expect(component.organizationStats.totalEmployees).toBe(125);
    expect(component.organizationStats.activeUsers).toBe(98);
  });
});