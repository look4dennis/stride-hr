import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RoleService } from './role.service';
import { Role, CreateRoleDto, UpdateRoleDto } from '../models/admin.models';
import { environment } from '../../environments/environment';

describe('RoleService', () => {
  let service: RoleService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/role`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [RoleService]
    });
    service = TestBed.inject(RoleService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getAllRoles', () => {
    it('should get all roles', () => {
      const mockResponse = {
        success: true,
        data: {
          roles: [
            {
              id: 1,
              name: 'Admin',
              description: 'Administrator role',
              hierarchyLevel: 1,
              isActive: true,
              createdAt: new Date()
            }
          ]
        }
      };

      service.getAllRoles().subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.data.roles.length).toBe(1);
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('getRole', () => {
    it('should get role by id', () => {
      const roleId = 1;
      const mockResponse = {
        success: true,
        data: {
          role: {
            id: roleId,
            name: 'Admin',
            description: 'Administrator role',
            hierarchyLevel: 1,
            isActive: true,
            createdAt: new Date(),
            permissions: [
              {
                id: 1,
                name: 'User.Create',
                module: 'User',
                action: 'Create',
                resource: 'User'
              }
            ]
          }
        }
      };

      service.getRole(roleId).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.data.role.id).toBe(roleId);
      });

      const req = httpMock.expectOne(`${apiUrl}/${roleId}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('createRole', () => {
    it('should create role', () => {
      const createDto: CreateRoleDto = {
        name: 'Manager',
        description: 'Manager role',
        hierarchyLevel: 2,
        permissionIds: [1, 2, 3]
      };

      const mockResponse = {
        success: true,
        message: 'Role created successfully',
        data: {
          role: {
            id: 2,
            name: createDto.name,
            description: createDto.description,
            hierarchyLevel: createDto.hierarchyLevel,
            isActive: true,
            createdAt: new Date()
          }
        }
      };

      service.createRole(createDto).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.data.role.name).toBe(createDto.name);
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(createDto);
      req.flush(mockResponse);
    });
  });

  describe('updateRole', () => {
    it('should update role', () => {
      const roleId = 1;
      const updateDto: UpdateRoleDto = {
        name: 'Updated Manager',
        description: 'Updated manager role',
        hierarchyLevel: 3,
        permissionIds: [1, 2, 4]
      };

      const mockResponse = {
        success: true,
        message: 'Role updated successfully'
      };

      service.updateRole(roleId, updateDto).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${apiUrl}/${roleId}`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateDto);
      req.flush(mockResponse);
    });
  });

  describe('deleteRole', () => {
    it('should delete role', () => {
      const roleId = 1;
      const mockResponse = {
        success: true,
        message: 'Role deleted successfully'
      };

      service.deleteRole(roleId).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      const req = httpMock.expectOne(`${apiUrl}/${roleId}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(mockResponse);
    });
  });

  describe('checkPermission', () => {
    it('should check user permission', () => {
      const permission = 'User.Create';
      const mockResponse = {
        success: true,
        data: {
          hasPermission: true,
          permission: permission
        }
      };

      service.checkPermission(permission).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(response.data.hasPermission).toBe(true);
      });

      const req = httpMock.expectOne(`${apiUrl}/check-permission/${permission}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockResponse);
    });
  });

  describe('validateRoleData', () => {
    it('should return no errors for valid data', () => {
      const validDto: CreateRoleDto = {
        name: 'Valid Role',
        description: 'Valid role description',
        hierarchyLevel: 3,
        permissionIds: [1, 2, 3]
      };

      const errors = service.validateRoleData(validDto);
      expect(errors).toEqual([]);
    });

    it('should return errors for invalid data', () => {
      const invalidDto: CreateRoleDto = {
        name: '',
        description: '',
        hierarchyLevel: 15,
        permissionIds: []
      };

      const errors = service.validateRoleData(invalidDto);
      expect(errors.length).toBeGreaterThan(0);
      expect(errors).toContain('Role name is required');
      expect(errors).toContain('Role description is required');
      expect(errors).toContain('Hierarchy level must be between 1 and 10');
      expect(errors).toContain('At least one permission must be selected');
    });
  });

  describe('getHierarchyLevels', () => {
    it('should return hierarchy levels', () => {
      const levels = service.getHierarchyLevels();
      expect(levels).toBeDefined();
      expect(levels.length).toBe(10);
      expect(levels[0]).toEqual({ value: 1, label: 'Level 1 - Executive' });
      expect(levels[9]).toEqual({ value: 10, label: 'Level 10 - Temporary' });
    });
  });

  describe('getAllPermissions', () => {
    it('should return mock permissions', () => {
      service.getAllPermissions().subscribe(permissions => {
        expect(permissions).toBeDefined();
        expect(permissions.length).toBeGreaterThan(0);
        expect(permissions[0].id).toBeDefined();
        expect(permissions[0].name).toBeDefined();
        expect(permissions[0].module).toBeDefined();
        expect(permissions[0].action).toBeDefined();
        expect(permissions[0].resource).toBeDefined();
      });
    });
  });
});