import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

/**
 * Base class for end-to-end component tests
 * Provides common setup and utilities for testing complete user workflows
 */
export abstract class E2ETestBase<T> {
  protected component!: T;
  protected fixture!: ComponentFixture<T>;
  protected httpMock!: HttpTestingController;

  protected async setupComponent(componentType: any, additionalImports: any[] = [], additionalProviders: any[] = [], isStandalone: boolean = false): Promise<void> {
    const config: any = {
      imports: [
        BrowserAnimationsModule,
        NgbModule,
        FormsModule,
        ReactiveFormsModule,
        ...additionalImports
      ],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        ...additionalProviders
      ]
    };

    // Handle standalone vs non-standalone components
    if (isStandalone) {
      config.imports.push(componentType);
    } else {
      config.declarations = [componentType];
    }

    await TestBed.configureTestingModule(config).compileComponents();

    this.fixture = TestBed.createComponent(componentType);
    this.component = this.fixture.componentInstance;
    this.httpMock = TestBed.inject(HttpTestingController);
  }

  protected detectChanges(): void {
    this.fixture.detectChanges();
  }

  protected async waitForAsync(fn: () => void): Promise<void> {
    return new Promise((resolve) => {
      fn();
      this.fixture.whenStable().then(() => {
        this.detectChanges();
        resolve();
      });
    });
  }

  protected mockHttpResponse(url: string, method: string, responseData: any, statusCode: number = 200): void {
    const req = this.httpMock.expectOne(req => req.url.includes(url) && req.method === method);
    if (statusCode >= 200 && statusCode < 300) {
      req.flush(responseData);
    } else {
      req.flush(responseData, { status: statusCode, statusText: 'Error' });
    }
  }

  protected mockHttpError(url: string, method: string, errorMessage: string, statusCode: number = 500): void {
    const req = this.httpMock.expectOne(req => req.url.includes(url) && req.method === method);
    req.flush({ message: errorMessage }, { status: statusCode, statusText: 'Error' });
  }

  protected clickElement(selector: string): void {
    const element = this.fixture.debugElement.nativeElement.querySelector(selector);
    if (element) {
      element.click();
      this.detectChanges();
    } else {
      throw new Error(`Element with selector '${selector}' not found`);
    }
  }

  protected setInputValue(selector: string, value: string): void {
    const input = this.fixture.debugElement.nativeElement.querySelector(selector);
    if (input) {
      input.value = value;
      input.dispatchEvent(new Event('input'));
      this.detectChanges();
    } else {
      throw new Error(`Input with selector '${selector}' not found`);
    }
  }

  protected selectOption(selector: string, value: string): void {
    const select = this.fixture.debugElement.nativeElement.querySelector(selector);
    if (select) {
      select.value = value;
      select.dispatchEvent(new Event('change'));
      this.detectChanges();
    } else {
      throw new Error(`Select with selector '${selector}' not found`);
    }
  }

  protected getElementText(selector: string): string {
    const element = this.fixture.debugElement.nativeElement.querySelector(selector);
    return element ? element.textContent.trim() : '';
  }

  protected isElementVisible(selector: string): boolean {
    const element = this.fixture.debugElement.nativeElement.querySelector(selector);
    return element && element.offsetParent !== null;
  }

  protected isElementDisabled(selector: string): boolean {
    const element = this.fixture.debugElement.nativeElement.querySelector(selector);
    return element ? element.disabled : false;
  }

  protected waitForElement(selector: string, timeout: number = 5000): Promise<Element> {
    return new Promise((resolve, reject) => {
      const startTime = Date.now();
      
      const checkElement = () => {
        const element = this.fixture.debugElement.nativeElement.querySelector(selector);
        if (element) {
          resolve(element);
        } else if (Date.now() - startTime > timeout) {
          reject(new Error(`Element '${selector}' not found within ${timeout}ms`));
        } else {
          setTimeout(checkElement, 100);
        }
      };
      
      checkElement();
    });
  }

  protected simulateFormSubmission(formSelector: string): void {
    const form = this.fixture.debugElement.nativeElement.querySelector(formSelector);
    if (form) {
      form.dispatchEvent(new Event('submit'));
      this.detectChanges();
    } else {
      throw new Error(`Form with selector '${formSelector}' not found`);
    }
  }

  protected verifyNoOutstandingRequests(): void {
    this.httpMock.verify();
  }

  protected cleanup(): void {
    try {
      if (this.httpMock) {
        this.httpMock.verify();
      }
    } catch (error) {
      // Ignore verification errors during cleanup
      console.warn('HTTP verification failed during cleanup:', error);
    }
  }

  // Mock data generators
  protected generateMockEmployee(id: number = 1): any {
    return {
      id: id,
      employeeId: `EMP${id.toString().padStart(3, '0')}`,
      firstName: `Employee${id}`,
      lastName: 'Test',
      email: `employee${id}@test.com`,
      phone: '123-456-7890',
      dateOfBirth: '1990-01-01',
      address: `Address ${id}`,
      joiningDate: '2023-01-01',
      designation: 'Test Employee',
      department: 'IT',
      branchId: 1,
      status: 'Active',
      createdAt: '2023-01-01T00:00:00Z'
    };
  }

  protected generateMockAttendanceRecord(employeeId: number = 1): any {
    return {
      id: 1,
      employeeId: employeeId,
      date: new Date().toISOString().split('T')[0],
      checkInTime: '09:00:00',
      checkOutTime: '17:00:00',
      totalWorkingHours: '08:00:00',
      status: 'Present',
      location: 'Office'
    };
  }

  protected generateMockProject(id: number = 1): any {
    return {
      id: id,
      name: `Test Project ${id}`,
      description: `Description for test project ${id}`,
      startDate: '2023-01-01',
      endDate: '2023-12-31',
      estimatedHours: 1000,
      budget: 100000,
      status: 'Active',
      priority: 'Medium',
      createdAt: '2023-01-01T00:00:00Z'
    };
  }

  protected generateMockPayrollRecord(employeeId: number = 1): any {
    return {
      id: 1,
      employeeId: employeeId,
      payrollPeriod: {
        startDate: '2023-01-01',
        endDate: '2023-01-31',
        month: 1,
        year: 2023
      },
      basicSalary: 50000,
      grossSalary: 55000,
      deductions: 10000,
      netSalary: 45000,
      currency: 'USD',
      status: 'Calculated'
    };
  }

  protected generateMockLeaveRequest(employeeId: number = 1): any {
    return {
      id: 1,
      employeeId: employeeId,
      leaveType: 'Annual',
      startDate: '2023-06-01',
      endDate: '2023-06-05',
      totalDays: 5,
      reason: 'Vacation',
      status: 'Pending',
      requestedAt: '2023-05-01T00:00:00Z'
    };
  }

  // API response wrapper
  protected wrapApiResponse(data: any, success: boolean = true, message: string = ''): any {
    return {
      success: success,
      message: message,
      data: data,
      errors: []
    };
  }

  // Common assertions
  protected assertElementExists(selector: string, message?: string): void {
    const element = this.fixture.debugElement.nativeElement.querySelector(selector);
    if (!element) {
      throw new Error(message || `Element '${selector}' should exist`);
    }
  }

  protected assertElementNotExists(selector: string, message?: string): void {
    // Handle :contains() pseudo-selector
    if (selector.includes(':contains(')) {
      const match = selector.match(/(.+):contains\("([^"]+)"\)/);
      if (match) {
        const baseSelector = match[1];
        const textContent = match[2];
        const elements = this.fixture.debugElement.nativeElement.querySelectorAll(baseSelector);
        const foundElement = Array.from(elements).find((el: any) => el.textContent.includes(textContent));
        if (foundElement) {
          throw new Error(message || `Element '${selector}' should not exist`);
        }
        return;
      }
    }
    
    const element = this.fixture.debugElement.nativeElement.querySelector(selector);
    if (element) {
      throw new Error(message || `Element '${selector}' should not exist`);
    }
  }

  protected assertElementText(selector: string, expectedText: string, message?: string): void {
    const actualText = this.getElementText(selector);
    if (actualText !== expectedText) {
      throw new Error(message || `Expected text '${expectedText}' but got '${actualText}'`);
    }
  }

  protected assertElementContainsText(selector: string, expectedText: string, message?: string): void {
    const actualText = this.getElementText(selector);
    if (!actualText.includes(expectedText)) {
      throw new Error(message || `Expected text to contain '${expectedText}' but got '${actualText}'`);
    }
  }

  protected assertFormValid(formSelector: string, message?: string): void {
    const form = this.fixture.debugElement.nativeElement.querySelector(formSelector);
    if (!form || !form.checkValidity()) {
      throw new Error(message || `Form '${formSelector}' should be valid`);
    }
  }

  protected assertFormInvalid(formSelector: string, message?: string): void {
    const form = this.fixture.debugElement.nativeElement.querySelector(formSelector);
    if (form && form.checkValidity()) {
      throw new Error(message || `Form '${formSelector}' should be invalid`);
    }
  }
}