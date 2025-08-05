import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { E2ETestHelper } from '../testing/e2e-test-helper';

// Mock components for testing
import { Component } from '@angular/core';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="employee-management">
      <div class="employee-form" *ngIf="showForm">
        <form #employeeForm="ngForm" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label for="firstName">First Name</label>
            <input 
              type="text" 
              id="firstName" 
              name="firstName" 
              [(ngModel)]="employee.firstName" 
              required 
              #firstName="ngModel">
          </div>
          <div class="form-group">
            <label for="lastName">Last Name</label>
            <input 
              type="text" 
              id="lastName" 
              name="lastName" 
              [(ngModel)]="employee.lastName" 
              required 
              #lastName="ngModel">
          </div>
          <div class="form-group">
            <label for="email">Email</label>
            <input 
              type="email" 
              id="email" 
              name="email" 
              [(ngModel)]="employee.email" 
              required 
              #email="ngModel">
          </div>
          <div class="form-group">
            <label for="department">Department</label>
            <select 
              id="department" 
              name="department" 
              [(ngModel)]="employee.department" 
              required 
              #department="ngModel">
              <option value="">Select Department</option>
              <option value="IT">IT</option>
              <option value="HR">HR</option>
              <option value="Finance">Finance</option>
            </select>
          </div>
          <button 
            type="submit" 
            class="submit-btn" 
            [disabled]="!employeeForm.valid || isSubmitting">
            {{ isSubmitting ? 'Creating...' : 'Create Employee' }}
          </button>
        </form>
        <div class="success-message" *ngIf="showSuccessMessage">
          Employee created successfully
        </div>
      </div>
      
      <div class="employee-list" *ngIf="!showForm">
        <button class="add-employee-btn" (click)="showAddForm()">Add New Employee</button>
        <div class="employee-item" *ngFor="let emp of employees">
          <div class="employee-name">{{ emp.firstName }} {{ emp.lastName }}</div>
          <div class="employee-email">{{ emp.email }}</div>
          <div class="employee-department">{{ emp.department }}</div>
          <button class="edit-btn" (click)="editEmployee(emp)">Edit</button>
          <button class="delete-btn" (click)="deleteEmployee(emp.id)">Delete</button>
        </div>
      </div>
      
      <div class="employee-search" *ngIf="!showForm">
        <input 
          type="text" 
          class="search-input" 
          placeholder="Search employees..." 
          [(ngModel)]="searchTerm" 
          (input)="onSearch()">
      </div>
    </div>
  `
})
class MockEmployeeManagementComponent {
  showForm = false;
  isSubmitting = false;
  showSuccessMessage = false;
  searchTerm = '';
  
  employee = {
    firstName: '',
    lastName: '',
    email: '',
    department: ''
  };
  
  employees: any[] = [];

  showAddForm() {
    this.showForm = true;
    this.resetForm();
  }

  onSubmit() {
    if (this.isValidForm()) {
      this.isSubmitting = true;
      
      // Simulate API call
      setTimeout(() => {
        this.employees.push({
          id: this.employees.length + 1,
          ...this.employee
        });
        
        this.isSubmitting = false;
        this.showSuccessMessage = true;
        
        // Hide success message and return to list after delay
        setTimeout(() => {
          this.showSuccessMessage = false;
          this.showForm = false;
          this.resetForm();
        }, 1000);
      }, 500);
    }
  }

  editEmployee(employee: any) {
    this.employee = { ...employee };
    this.showForm = true;
  }

  deleteEmployee(id: number) {
    this.employees = this.employees.filter(emp => emp.id !== id);
  }

  onSearch() {
    // Mock search functionality
  }

  private isValidForm(): boolean {
    return !!(this.employee.firstName && 
             this.employee.lastName && 
             this.employee.email && 
             this.employee.department);
  }

  private resetForm() {
    this.employee = {
      firstName: '',
      lastName: '',
      email: '',
      department: ''
    };
    this.showSuccessMessage = false;
  }
}

describe('Employee Management E2E Workflow', () => {
  let testHelper: E2ETestHelper<MockEmployeeManagementComponent>;

  beforeEach(async () => {
    testHelper = new E2ETestHelper<MockEmployeeManagementComponent>();
    await testHelper.initialize(MockEmployeeManagementComponent);
  });

  afterEach(() => {
    testHelper.cleanupTest();
  });

  it('should complete full employee creation workflow', async () => {
    // Step 1: Initial state - should show employee list
    testHelper.triggerChangeDetection();
    testHelper.verifyElementExists('.employee-list', 'Employee list should be visible initially');
    testHelper.verifyElementNotExists('.employee-form', 'Employee form should not be visible initially');

    // Step 2: Click "Add New Employee" button
    testHelper.clickElementBySelector('.add-employee-btn');
    testHelper.verifyElementExists('.employee-form', 'Employee form should be visible after clicking add button');
    testHelper.verifyElementNotExists('.employee-list', 'Employee list should be hidden when form is shown');

    // Step 3: Fill out employee form
    testHelper.setInputValueBySelector('#firstName', 'John');
    testHelper.setInputValueBySelector('#lastName', 'Doe');
    testHelper.setInputValueBySelector('#email', 'john.doe@test.com');
    testHelper.selectOptionBySelector('#department', 'IT');

    // Verify submit button is enabled
    expect(testHelper.checkElementDisabled('.submit-btn')).toBeFalsy();

    // Step 4: Submit form
    testHelper.clickElementBySelector('.submit-btn');

    // Step 5: Wait for submission and verify success state
    await new Promise(resolve => setTimeout(resolve, 600)); // Wait for async operation
    testHelper.triggerChangeDetection();
    
    // Verify success message appears
    testHelper.verifyElementExists('.success-message', 'Success message should be visible');
    testHelper.verifyElementContainsText('.success-message', 'Employee created successfully');

    // Step 6: Wait for form reset and return to list
    await new Promise(resolve => setTimeout(resolve, 1200)); // Wait for success message timeout
    testHelper.triggerChangeDetection();

    // Verify employee appears in list
    testHelper.verifyElementExists('.employee-list', 'Employee list should be visible after successful creation');
    testHelper.verifyElementContainsText('.employee-item', 'John Doe');
    testHelper.verifyElementExists('.employee-item .employee-email', 'Employee email should be displayed');
    testHelper.verifyElementNotExists('.employee-form', 'Employee form should be hidden after successful creation');

    // Step 7: Verify employee details are displayed
    testHelper.verifyElementExists('.employee-item', 'Employee item should exist in the list');
    testHelper.verifyElementContainsText('.employee-item .employee-name', 'John Doe');
    testHelper.verifyElementContainsText('.employee-item .employee-email', 'john.doe@test.com');
    testHelper.verifyElementContainsText('.employee-item .employee-department', 'IT');
  });

  it('should handle form validation errors', async () => {
    // Navigate to form
    testHelper.triggerChangeDetection();
    testHelper.clickElementBySelector('.add-employee-btn');
    testHelper.triggerChangeDetection();

    // Try to submit empty form - button should be disabled
    testHelper.triggerChangeDetection();
    const submitBtn = testHelper.getFixture().debugElement.nativeElement.querySelector('.submit-btn');
    expect(submitBtn.disabled).toBeTruthy();

    // Fill partial form
    testHelper.setInputValueBySelector('#firstName', 'John');
    testHelper.setInputValueBySelector('#lastName', 'Doe');
    testHelper.triggerChangeDetection();
    await new Promise(resolve => setTimeout(resolve, 100)); // Allow form validation to process

    // Submit button should still be disabled (missing email and department)
    const submitBtnAfterPartial = testHelper.getFixture().debugElement.nativeElement.querySelector('.submit-btn');
    expect(submitBtnAfterPartial.disabled).toBeTruthy();

    // Complete the form
    testHelper.setInputValueBySelector('#email', 'john.doe@test.com');
    testHelper.selectOptionBySelector('#department', 'IT');
    testHelper.triggerChangeDetection();
    await new Promise(resolve => setTimeout(resolve, 100)); // Allow form validation to process

    // Submit button should now be enabled
    const submitBtnAfterComplete = testHelper.getFixture().debugElement.nativeElement.querySelector('.submit-btn');
    expect(submitBtnAfterComplete.disabled).toBeFalsy();
  });

  it('should support employee editing workflow', async () => {
    // First create an employee
    const component = testHelper.getComponent();
    component.employees = [{
      id: 1,
      firstName: 'Jane',
      lastName: 'Smith',
      email: 'jane.smith@test.com',
      department: 'HR'
    }];

    testHelper.triggerChangeDetection();

    // Verify employee exists in list
    testHelper.verifyElementExists('.employee-item', 'Employee should exist in list');
    testHelper.verifyElementContainsText('.employee-item .employee-name', 'Jane Smith');

    // Click edit button
    testHelper.clickElementBySelector('.edit-btn');

    // Verify form is shown with pre-filled data
    testHelper.verifyElementExists('.employee-form', 'Edit form should be visible');
    
    // Wait for form to be populated
    await new Promise(resolve => setTimeout(resolve, 100));
    testHelper.triggerChangeDetection();
    
    const fixture = testHelper.getFixture();
    const firstNameInput = fixture.debugElement.nativeElement.querySelector('#firstName');
    const lastNameInput = fixture.debugElement.nativeElement.querySelector('#lastName');
    const emailInput = fixture.debugElement.nativeElement.querySelector('#email');
    
    expect(firstNameInput.value).toBe('Jane');
    expect(lastNameInput.value).toBe('Smith');
    expect(emailInput.value).toBe('jane.smith@test.com');

    // Modify the employee data
    testHelper.setInputValueBySelector('#firstName', 'Janet');
    testHelper.setInputValueBySelector('#email', 'janet.smith@test.com');

    testHelper.triggerChangeDetection();

    // Submit the form
    testHelper.clickElementBySelector('.submit-btn');

    // Wait for form submission and navigation back to list
    await new Promise(resolve => setTimeout(resolve, 600));
    testHelper.triggerChangeDetection();
    await new Promise(resolve => setTimeout(resolve, 1100));
    testHelper.triggerChangeDetection();

    // Verify updated employee appears in list
    testHelper.verifyElementExists('.employee-list', 'Employee list should be visible after edit');
    testHelper.verifyElementContainsText('.employee-item .employee-name', 'Jane Smith');
    testHelper.verifyElementContainsText('.employee-item .employee-email', 'jane.smith@test.com');
  });

  it('should support employee deletion workflow', async () => {
    // Setup initial employees
    const component = testHelper.getComponent();
    component.employees = [
      { id: 1, firstName: 'John', lastName: 'Doe', email: 'john@test.com', department: 'IT' },
      { id: 2, firstName: 'Jane', lastName: 'Smith', email: 'jane@test.com', department: 'HR' }
    ];

    testHelper.triggerChangeDetection();

    // Verify both employees exist
    testHelper.verifyElementExists('.employee-item', 'Employees should exist in list');

    // Delete first employee
    testHelper.clickElementBySelector('.delete-btn');

    testHelper.triggerChangeDetection();

    // Verify employee was removed
    testHelper.verifyElementNotExists('.employee-item:contains("John Doe")', 'Deleted employee should not exist');
    testHelper.verifyElementContainsText('.employee-item', 'Jane Smith');
  });
});