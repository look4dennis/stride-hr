# Comprehensive Error Handling System

This document describes the comprehensive error handling system implemented for the StrideHR application. The system provides robust error handling, user-friendly error messages, retry mechanisms, offline support, and graceful failure handling.

## Overview

The error handling system consists of several interconnected components:

1. **ErrorHandlingService** - Global error handler and comprehensive error management
2. **ConnectionService** - Offline detection and connection monitoring
3. **Error Interceptor** - HTTP error interception and handling
4. **Error Boundary Components** - Graceful failure handling for UI components
5. **Validation Error Components** - Form validation error display
6. **Base Components** - Enhanced base classes with error handling support

## Components

### 1. ErrorHandlingService

The main service that implements Angular's `ErrorHandler` interface and provides comprehensive error management.

**Features:**
- Global error handling for unhandled JavaScript errors
- HTTP error handling with specific logic for different status codes
- Retry mechanisms with exponential backoff
- Error logging and monitoring
- User-friendly error messages
- Database connection failure handling
- Form validation error processing

**Usage:**
```typescript
// Inject the service
private errorHandlingService = inject(ErrorHandlingService);

// Handle HTTP errors
this.errorHandlingService.handleHttpError(error, context);

// Retry operations
this.errorHandlingService.retryOperation(operation, config);

// Handle validation errors
const validationErrors = this.errorHandlingService.handleValidationErrors(errors);
```

### 2. ConnectionService

Monitors network connectivity and provides offline support.

**Features:**
- Browser online/offline detection
- Server connectivity monitoring
- Offline action queuing
- Connection quality assessment
- Automatic retry of queued actions when connection is restored

**Usage:**
```typescript
// Check connection status
const isOnline = this.connectionService.isOnline();

// Queue actions for offline processing
this.connectionService.queueOfflineAction('POST', '/api/data', data);

// Subscribe to connection status
this.connectionService.connectionStatus$.subscribe(status => {
  console.log('Connection status:', status);
});
```

### 3. Error Boundary Component

Provides graceful failure handling for UI components.

**Features:**
- Catches component-level errors
- Displays user-friendly error messages
- Provides retry functionality
- Supports custom error messages and configurations
- Responsive design

**Usage:**
```html
<app-error-boundary 
  [config]="{ showRetryButton: true, customMessage: 'Failed to load data' }"
  [retryCallback]="retryOperation">
  <!-- Your component content here -->
  <my-component></my-component>
</app-error-boundary>
```

### 4. Validation Errors Component

Displays form validation errors in a user-friendly format.

**Features:**
- Field-specific error display
- Form-level error summary
- Server validation error handling
- Customizable field display names
- Responsive design

**Usage:**
```html
<!-- Field-specific errors -->
<app-validation-errors 
  [form]="myForm" 
  fieldName="email"
  [fieldDisplayNames]="{ email: 'Email Address' }">
</app-validation-errors>

<!-- Form-level errors -->
<app-validation-errors 
  [form]="myForm" 
  [showSummary]="true">
</app-validation-errors>
```

### 5. Connection Status Component

Displays current connection status and offline indicators.

**Features:**
- Real-time connection status display
- Offline action count indicator
- Manual connection testing
- Connection quality information

**Usage:**
```html
<app-connection-status></app-connection-status>
```

### 6. Global Error Display Component

Displays system-wide error notifications and status.

**Features:**
- Critical error banners
- Offline status notifications
- Error summary for development
- Dismissible notifications

**Usage:**
```html
<!-- Add to your main app layout -->
<app-global-error-display></app-global-error-display>
```

## Base Components

### BaseComponent

Enhanced base component with comprehensive error handling support.

**Features:**
- Integrated error handling
- Loading state management
- Connection status awareness
- Retry operation support
- Offline-aware operations

**Usage:**
```typescript
export class MyComponent extends BaseComponent {
  protected initializeComponent(): void {
    // Component initialization logic
  }

  loadData(): void {
    this.executeOperation(
      () => this.dataService.getData(),
      {
        loadingMessage: 'Loading data...',
        successMessage: 'Data loaded successfully',
        enableRetry: true,
        maxRetries: 3
      }
    ).subscribe(data => {
      // Handle successful data loading
    });
  }
}
```

### BaseFormComponent

Enhanced form component with validation error handling.

**Features:**
- Comprehensive form validation
- Server error handling
- Offline form submission queuing
- Validation error display
- Form state management

**Usage:**
```typescript
export class MyFormComponent extends BaseFormComponent<MyFormData> {
  protected createForm(): FormGroup {
    return this.formBuilder.group({
      name: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]]
    });
  }

  protected submitForm(data: MyFormData): Observable<FormSubmissionResult> {
    return this.myService.submitData(data);
  }
}
```

## Configuration

### Error Handling Configuration

The error handling system can be configured through various options:

```typescript
// Retry configuration
const retryConfig: RetryConfig = {
  maxRetries: 3,
  delayMs: 1000,
  exponentialBackoff: true,
  retryCondition: (error) => error.status >= 500
};

// Error boundary configuration
const errorBoundaryConfig: ErrorBoundaryConfig = {
  showRetryButton: true,
  showReloadButton: false,
  showNavigationButton: true,
  fallbackRoute: '/dashboard',
  customMessage: 'Custom error message',
  logError: true
};

// Form validation configuration
const validationConfig: FormValidationConfig = {
  showInlineErrors: true,
  showSummaryErrors: true,
  validateOnSubmit: true,
  validateOnChange: false,
  fieldDisplayNames: {
    firstName: 'First Name',
    lastName: 'Last Name'
  }
};
```

## Error Types and Handling

### HTTP Errors

The system handles different HTTP error status codes with specific logic:

- **0**: Network connectivity issues - Shows retry options
- **400**: Bad request - Displays validation errors
- **401**: Unauthorized - Redirects to login
- **403**: Forbidden - Shows permission error
- **404**: Not found - Shows resource not found message
- **409**: Conflict - Shows conflict resolution message
- **422**: Validation error - Displays field-specific errors
- **429**: Rate limiting - Shows retry after delay
- **500+**: Server errors - Shows retry options

### JavaScript Errors

Unhandled JavaScript errors are caught by the global error handler and:
- Logged for debugging
- Displayed with user-friendly messages
- Optionally trigger page reload for critical errors

### Validation Errors

Form validation errors are handled at multiple levels:
- Field-level validation with real-time feedback
- Form-level validation on submission
- Server-side validation error display
- Custom validation rules support

## Best Practices

### 1. Use Base Components

Always extend from `BaseComponent` or `BaseFormComponent` to get built-in error handling:

```typescript
export class MyComponent extends BaseComponent {
  // Your component logic here
}
```

### 2. Wrap Critical UI Sections

Use error boundaries around critical UI sections:

```html
<app-error-boundary>
  <critical-feature-component></critical-feature-component>
</app-error-boundary>
```

### 3. Handle Offline Scenarios

Use offline-aware operations for data modifications:

```typescript
this.executeOfflineAwareOperation(
  () => this.dataService.updateData(data),
  fallbackData,
  { queueWhenOffline: true }
);
```

### 4. Provide User-Friendly Messages

Always provide context-specific error messages:

```typescript
this.handleError(error, 'Failed to save your changes. Please try again.');
```

### 5. Implement Retry Logic

Use retry mechanisms for transient failures:

```typescript
this.retryOperation(
  () => this.apiService.getData(),
  { maxRetries: 3, exponentialBackoff: true }
);
```

## Testing

### Unit Testing

Test error handling scenarios in your components:

```typescript
it('should handle API errors gracefully', () => {
  const error = new HttpErrorResponse({ status: 500 });
  spyOn(component, 'handleError');
  
  // Trigger error scenario
  component.loadData();
  
  expect(component.handleError).toHaveBeenCalledWith(error);
});
```

### Integration Testing

Test the complete error handling flow:

```typescript
it('should display error boundary on component failure', () => {
  // Simulate component error
  // Verify error boundary is displayed
  // Test retry functionality
});
```

## Monitoring and Logging

### Error Logs

The system maintains error logs that can be accessed for debugging:

```typescript
// Get error logs
const errorLogs = this.errorHandlingService.getErrorLogs();

// Clear error logs
this.errorHandlingService.clearErrorLogs();
```

### Production Monitoring

In production, critical errors are automatically sent to logging services for monitoring and alerting.

## Troubleshooting

### Common Issues

1. **Error boundaries not catching errors**: Ensure components extend BaseComponent
2. **Validation errors not displaying**: Check form configuration and validation setup
3. **Offline actions not queuing**: Verify ConnectionService is properly initialized
4. **Retry logic not working**: Check retry conditions and configuration

### Debug Mode

Enable debug mode to see detailed error information:

```typescript
// In development environment
const showErrorSummary = !environment.production;
```

## Migration Guide

### From Basic Error Handling

1. Replace manual error handling with BaseComponent
2. Add error boundaries around critical sections
3. Update form components to use BaseFormComponent
4. Add global error display to app layout

### Configuration Updates

Update your app configuration to include the error handling service:

```typescript
// app.config.ts
import { ErrorHandlingService } from './core/services/error-handling.service';

export const appConfig: ApplicationConfig = {
  providers: [
    // ... other providers
    {
      provide: ErrorHandler,
      useClass: ErrorHandlingService
    }
  ]
};
```

This comprehensive error handling system ensures a robust, user-friendly experience even when things go wrong, providing clear feedback, recovery options, and maintaining application stability.