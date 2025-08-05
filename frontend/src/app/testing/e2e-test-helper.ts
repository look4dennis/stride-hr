import { E2ETestBase } from './e2e-test-base';

/**
 * Concrete implementation of E2ETestBase for use in tests
 * This class can be instantiated directly in test files
 */
export class E2ETestHelper<T> extends E2ETestBase<T> {
  
  /**
   * Initialize the test helper with a component
   */
  async initialize(componentType: any, additionalImports: any[] = [], additionalProviders: any[] = [], isStandalone: boolean = true): Promise<void> {
    await this.setupComponent(componentType, additionalImports, additionalProviders, isStandalone);
  }

  /**
   * Get the component instance
   */
  getComponent(): T {
    return this.component;
  }

  /**
   * Get the fixture
   */
  getFixture() {
    return this.fixture;
  }

  /**
   * Get the HTTP mock controller
   */
  getHttpMock() {
    return this.httpMock;
  }

  /**
   * Public wrapper for detectChanges
   */
  triggerChangeDetection(): void {
    this.detectChanges();
  }

  /**
   * Public wrapper for clickElement
   */
  clickElementBySelector(selector: string): void {
    this.clickElement(selector);
  }

  /**
   * Public wrapper for setInputValue
   */
  setInputValueBySelector(selector: string, value: string): void {
    this.setInputValue(selector, value);
  }

  /**
   * Public wrapper for selectOption
   */
  selectOptionBySelector(selector: string, value: string): void {
    this.selectOption(selector, value);
  }

  /**
   * Public wrapper for simulateFormSubmission
   */
  submitForm(formSelector: string): void {
    this.simulateFormSubmission(formSelector);
  }

  /**
   * Public wrapper for assertElementExists
   */
  verifyElementExists(selector: string, message?: string): void {
    this.assertElementExists(selector, message);
  }

  /**
   * Public wrapper for assertElementNotExists
   */
  verifyElementNotExists(selector: string, message?: string): void {
    this.assertElementNotExists(selector, message);
  }

  /**
   * Public wrapper for assertElementContainsText
   */
  verifyElementContainsText(selector: string, expectedText: string, message?: string): void {
    this.assertElementContainsText(selector, expectedText, message);
  }

  /**
   * Public wrapper for isElementDisabled
   */
  checkElementDisabled(selector: string): boolean {
    return this.isElementDisabled(selector);
  }

  /**
   * Public wrapper for mockHttpResponse
   */
  mockApiResponse(url: string, method: string, responseData: any, statusCode: number = 200): void {
    this.mockHttpResponse(url, method, responseData, statusCode);
  }

  /**
   * Public wrapper for cleanup
   */
  cleanupTest(): void {
    this.cleanup();
  }
}