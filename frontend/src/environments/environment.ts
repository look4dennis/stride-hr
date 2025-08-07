export const environment = {
  production: false,
  apiUrl: 'https://localhost:5001/api',
  weatherApiKey: 'demo-api-key', // Using demo key to prevent console errors
  weatherApiUrl: 'https://api.openweathermap.org/data/2.5',
  signalRUrl: 'https://localhost:5001/hubs',
  fileUploadUrl: 'https://localhost:5001/api/files',
  maxFileSize: 10 * 1024 * 1024, // 10MB
  supportedImageTypes: ['image/jpeg', 'image/png', 'image/gif'],
  supportedDocumentTypes: ['application/pdf', 'application/msword', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document'],
  pagination: {
    defaultPageSize: 20,
    maxPageSize: 100
  },
  cache: {
    defaultTtl: 300000, // 5 minutes
    maxSize: 100
  },
  vapidPublicKey: 'BEl62iUYgUivxIkv69yViEuiBIa40HI80NqIUHI80NqIUHI80NqIUHI80NqIUHI80NqIUHI80NqIUHI80NqIUHI80NqI'
};