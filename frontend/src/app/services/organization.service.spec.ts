import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { OrganizationService } from './organization.service';
import { Organization, CreateOrganizationDto, UpdateOrganizationDto, ApiResponse } from '../models/admin.models';
import { environment } from '../../environments/environment';

describe('OrganizationService', () => {
  let service: OrganizationService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/organization`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [OrganizationService]
    });
    service = TestBed.inject(OrganizationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAllOrganizations', () => {
    it('should get all organizations', () => {
      const mockResponse: ApiResponse<Organization[]> = {
        success: true,
        data: [
          {
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
          }
        ],
        message: 'Organizations retrieved successfully'
      };

      service.getAllOrganizations().subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.data?.length).toBe(1);
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('getOrganization', () => {
    it('should get organization by id', () => {
      const organizationId = 1;
      const mockResponse: ApiResponse<Organization> = {
        success: true,
        data: {
          id: organizationId,
          name: 'Test Organization',
          address: '123 Test St',
          email: 'test@example.com',
          phone: '123-456-7890',
          normalWorkingHours: '08:00',
          overtimeRate: 1.5,
          productiveHoursThreshold: 8,
          branchIsolationEnabled: false,
          createdAt: new Date()
        },
        message: 'Organization retrieved successfully'
      };

      service.getOrganization(organizationId).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.data?.id).toBe(organizationId);
      });

      const req = httpMock.expectOne(`${apiUrl}/${organizationId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('createOrganization', () => {
    it('should create organization', () => {
      const createDto: CreateOrganizationDto = {
        name: 'New Organization',
        address: '456 New St',
        email: 'new@example.com',
        phone: '987-654-3210',
        normalWorkingHours: '09:00',
        overtimeRate: 2.0,
        productiveHoursThreshold: 7,
        branchIsolationEnabled: true
      };

      const mockResponse: ApiResponse<Organization> = {
        success: true,
        data: {
          id: 2,
          ...createDto,
          createdAt: new Date()
        },
        message: 'Organization created successfully'
      };

      service.createOrganization(createDto).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.data?.name).toBe(createDto.name);
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(createDto);
      req.flush(mockResponse);
    });
  });

  describe('updateOrganization', () => {
    it('should update organization', () => {
      const organizationId = 1;
      const updateDto: UpdateOrganizationDto = {
        name: 'Updated Organization',
        address: '789 Updated St',
        email: 'updated@example.com',
        phone: '555-123-4567',
        normalWorkingHours: '08:30',
        overtimeRate: 1.75,
        productiveHoursThreshold: 8,
        branchIsolationEnabled: true
      };

      const mockResponse: ApiResponse<Organization> = {
        success: true,
        data: {
          id: organizationId,
          ...updateDto,
          createdAt: new Date(),
          updatedAt: new Date()
        },
        message: 'Organization updated successfully'
      };

      service.updateOrganization(organizationId, updateDto).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.data?.name).toBe(updateDto.name);
      });

      const req = httpMock.expectOne(`${apiUrl}/${organizationId}`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateDto);
      req.flush(mockResponse);
    });
  });

  describe('deleteOrganization', () => {
    it('should delete organization', () => {
      const organizationId = 1;
      const mockResponse: ApiResponse<void> = {
        success: true,
        message: 'Organization deleted successfully'
      };

      service.deleteOrganization(organizationId).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${apiUrl}/${organizationId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(mockResponse);
    });
  });

  describe('uploadLogo', () => {
    it('should upload logo', () => {
      const organizationId = 1;
      const file = new File(['test'], 'logo.png', { type: 'image/png' });
      const mockResponse: ApiResponse<string> = {
        success: true,
        data: '/uploads/logos/logo.png',
        message: 'Logo uploaded successfully'
      };

      service.uploadLogo(organizationId, file).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${apiUrl}/${organizationId}/logo`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body instanceof FormData).toBeTruthy();
      req.flush(mockResponse);
    });
  });

  describe('validateOrganizationData', () => {
    it('should return no errors for valid data', () => {
      const validDto: CreateOrganizationDto = {
        name: 'Valid Organization',
        address: '123 Valid St',
        email: 'valid@example.com',
        phone: '123-456-7890',
        normalWorkingHours: '08:00',
        overtimeRate: 1.5,
        productiveHoursThreshold: 8,
        branchIsolationEnabled: false
      };

      const errors = service.validateOrganizationData(validDto);
      expect(errors).toEqual([]);
    });

    it('should return errors for invalid data', () => {
      const invalidDto: CreateOrganizationDto = {
        name: '',
        address: '',
        email: 'invalid-email',
        phone: '',
        normalWorkingHours: '08:00',
        overtimeRate: -1,
        productiveHoursThreshold: -5,
        branchIsolationEnabled: false
      };

      const errors = service.validateOrganizationData(invalidDto);
      expect(errors.length).toBeGreaterThan(0);
      expect(errors).toContain('Organization name is required');
      expect(errors).toContain('Invalid email format');
      expect(errors).toContain('Phone number is required');
      expect(errors).toContain('Address is required');
      expect(errors).toContain('Overtime rate cannot be negative');
      expect(errors).toContain('Productive hours threshold cannot be negative');
    });
  });
});