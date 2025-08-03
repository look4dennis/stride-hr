# StrideHR API Documentation

## Overview

StrideHR provides a comprehensive REST API for managing all aspects of human resources across global organizations. The API supports multi-branch operations, real-time integrations, and extensive third-party system connectivity.

## Base URL

- **Production**: `https://api.stridehr.com`
- **Staging**: `https://staging-api.stridehr.com`
- **Development**: `http://localhost:5000`

## Authentication

All API endpoints require JWT Bearer token authentication unless otherwise specified.

### Getting an Access Token

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "your-password"
}
```

### Using the Token

Include the token in the Authorization header:

```http
Authorization: Bearer your-jwt-token-here
```

## API Endpoints

### 🔗 Integration APIs

#### Webhook Management

Webhooks allow real-time notifications to external systems when events occur in StrideHR.

**Available Events:**
- `employee.created`, `employee.updated`, `employee.deleted`
- `attendance.checked_in`, `attendance.checked_out`
- `leave.requested`, `leave.approved`, `leave.rejected`
- `payroll.processed`, `payroll.approved`
- `project.created`, `project.completed`
- `task.assigned`, `task.completed`

##### Create Webhook Subscription

```http
POST /api/webhook
Content-Type: application/json
Authorization: Bearer {token}

{
  "name": "Employee Events Webhook",
  "url": "https://your-app.com/webhooks/stridehr",
  "secret": "your-webhook-secret-key",
  "events": [
    "employee.created",
    "employee.updated",
    "attendance.checked_in",
    "leave.approved"
  ],
  "isActive": true
}
```

##### Get Webhook Subscriptions

```http
GET /api/webhook/organization/{organizationId}
Authorization: Bearer {token}
```

##### Test Webhook

```http
POST /api/webhook/{subscriptionId}/test?eventType=test.event
Authorization: Bearer {token}
```

##### Webhook Payload Format

```json
{
  "eventType": "employee.created",
  "timestamp": "2024-12-01T10:00:00Z",
  "organizationId": 1,
  "data": {
    "employeeId": 123,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@company.com",
    "department": "Engineering"
  }
}
```

#### Calendar Integration

Integrate with Google Calendar and Outlook for seamless calendar management.

##### Connect Google Calendar

```http
POST /api/calendarintegration/google/connect?employeeId=123
Content-Type: application/json
Authorization: Bearer {token}

"authorization_code_from_google_oauth"
```

##### Create Calendar Event

```http
POST /api/calendarintegration/google/events?employeeId=123
Content-Type: application/json
Authorization: Bearer {token}

{
  "title": "Team Meeting",
  "description": "Weekly team sync meeting",
  "startTime": "2024-12-01T10:00:00Z",
  "endTime": "2024-12-01T11:00:00Z",
  "location": "Conference Room A",
  "isAllDay": false,
  "attendeeEmails": [
    "john.doe@company.com",
    "jane.smith@company.com"
  ],
  "eventType": "Meeting"
}
```

##### Get Calendar Events

```http
GET /api/calendarintegration/google/events?employeeId=123&startDate=2024-12-01&endDate=2024-12-07
Authorization: Bearer {token}
```

#### External System Integration

Connect with payroll and accounting systems for seamless data exchange.

##### Connect Payroll System

```http
POST /api/externalintegration/payroll/connect?organizationId=1&systemType=ADP
Content-Type: application/json
Authorization: Bearer {token}

{
  "apiUrl": "https://api.adp.com/hr/v2",
  "apiKey": "your-api-key",
  "username": "your-username",
  "password": "your-password",
  "customFields": {
    "companyId": "COMP123",
    "environment": "production"
  }
}
```

##### Export Payroll Data

```http
POST /api/externalintegration/payroll/export?organizationId=1&systemType=ADP
Content-Type: application/json
Authorization: Bearer {token}

{
  "payrollPeriodStart": "2024-11-01T00:00:00Z",
  "payrollPeriodEnd": "2024-11-30T23:59:59Z",
  "employeeIds": [1, 2, 3],
  "branchIds": [1],
  "format": "Json",
  "customParameters": {
    "includeDeductions": true,
    "includeBenefits": true
  }
}
```

##### Connect Accounting System

```http
POST /api/externalintegration/accounting/connect?organizationId=1&systemType=QuickBooks
Content-Type: application/json
Authorization: Bearer {token}

{
  "apiUrl": "https://api.quickbooks.com/v3",
  "apiKey": "your-api-key",
  "companyId": "123456789",
  "username": "your-username",
  "password": "your-password",
  "accountMappings": {
    "salariesAccount": "6000",
    "benefitsAccount": "6100",
    "taxesAccount": "2200"
  }
}
```

### 📊 Data Import/Export

#### Bulk Data Operations

```http
POST /api/dataimportexport/import
Content-Type: multipart/form-data
Authorization: Bearer {token}

file: employee_data.xlsx
entityType: Employee
options: {"validateOnly": false, "skipDuplicates": true}
```

```http
GET /api/dataimportexport/export?entityType=Employee&format=Excel&branchId=1
Authorization: Bearer {token}
```

## Response Format

All API responses follow a consistent format:

### Success Response

```json
{
  "success": true,
  "data": {
    // Response data here
  },
  "message": "Operation completed successfully"
}
```

### Error Response

```json
{
  "success": false,
  "message": "Error description",
  "errors": [
    "Detailed error message 1",
    "Detailed error message 2"
  ]
}
```

### Paginated Response

```json
{
  "success": true,
  "data": [
    // Array of items
  ],
  "totalCount": 150,
  "pageSize": 20,
  "currentPage": 1,
  "totalPages": 8
}
```

## Error Codes

| Code | Description |
|------|-------------|
| 400 | Bad Request - Invalid input data |
| 401 | Unauthorized - Invalid or missing token |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource doesn't exist |
| 409 | Conflict - Resource already exists |
| 422 | Unprocessable Entity - Validation failed |
| 429 | Too Many Requests - Rate limit exceeded |
| 500 | Internal Server Error - Server error |

## Rate Limiting

API requests are rate-limited to ensure system stability:

- **Standard endpoints**: 1000 requests per hour per user
- **Bulk operations**: 100 requests per hour per user
- **Webhook deliveries**: 10,000 per hour per organization

Rate limit headers are included in responses:

```http
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 999
X-RateLimit-Reset: 1638360000
```

## Webhook Security

### Signature Verification

All webhook payloads are signed using HMAC-SHA256. Verify the signature to ensure authenticity:

```javascript
const crypto = require('crypto');

function verifyWebhookSignature(payload, signature, secret) {
  const expectedSignature = 'sha256=' + 
    crypto.createHmac('sha256', secret)
          .update(payload)
          .digest('hex');
  
  return crypto.timingSafeEqual(
    Buffer.from(signature),
    Buffer.from(expectedSignature)
  );
}
```

### Retry Policy

Failed webhook deliveries are retried with exponential backoff:

- Initial retry: 1 minute
- Second retry: 5 minutes
- Third retry: 15 minutes
- Fourth retry: 1 hour
- Fifth retry: 6 hours

After 5 failed attempts, the webhook is marked as failed and requires manual retry.

## SDKs and Libraries

### JavaScript/Node.js

```bash
npm install @stridehr/api-client
```

```javascript
const StrideHR = require('@stridehr/api-client');

const client = new StrideHR({
  apiKey: 'your-api-key',
  baseUrl: 'https://api.stridehr.com'
});

// Create webhook subscription
const webhook = await client.webhooks.create({
  name: 'My Webhook',
  url: 'https://myapp.com/webhook',
  events: ['employee.created', 'employee.updated']
});
```

### Python

```bash
pip install stridehr-api
```

```python
from stridehr import StrideHRClient

client = StrideHRClient(
    api_key='your-api-key',
    base_url='https://api.stridehr.com'
)

# Connect Google Calendar
result = client.calendar.connect_google(
    employee_id=123,
    authorization_code='auth_code_from_google'
)
```

### C#

```bash
dotnet add package StrideHR.ApiClient
```

```csharp
using StrideHR.ApiClient;

var client = new StrideHRClient("your-api-key", "https://api.stridehr.com");

// Export payroll data
var exportResult = await client.Integrations.ExportPayrollDataAsync(
    organizationId: 1,
    systemType: PayrollSystemType.ADP,
    request: new PayrollExportRequest
    {
        PayrollPeriodStart = DateTime.UtcNow.AddDays(-30),
        PayrollPeriodEnd = DateTime.UtcNow,
        Format = PayrollExportFormat.Json
    }
);
```

## Testing

### Postman Collection

Import our Postman collection for easy API testing:

```bash
curl -o stridehr-api.postman_collection.json \
  https://api.stridehr.com/docs/postman-collection
```

### Test Environment

Use our sandbox environment for testing:

- **Base URL**: `https://sandbox-api.stridehr.com`
- **Test Organization ID**: `999`
- **Test Employee ID**: `1`

## Support

- **Documentation**: https://docs.stridehr.com
- **API Status**: https://status.stridehr.com
- **Support Email**: api-support@stridehr.com
- **Developer Forum**: https://community.stridehr.com

## Changelog

### v1.0.0 (Current)
- Initial API release
- Webhook support for real-time notifications
- Google Calendar integration
- External payroll and accounting system integrations
- Comprehensive data import/export capabilities
- Full CRUD operations for all HR entities

### Upcoming Features
- Outlook Calendar integration
- Microsoft Teams integration
- Advanced AI analytics endpoints
- GraphQL API support
- Real-time collaboration features