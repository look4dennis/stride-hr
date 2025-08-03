import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { WeatherService, WeatherData } from './weather.service';

describe('WeatherService', () => {
  let service: WeatherService;
  let httpMock: HttpTestingController;

  const mockWeatherResponse = {
    name: 'New York',
    sys: { country: 'US' },
    main: {
      temp: 22,
      feels_like: 24,
      humidity: 65
    },
    weather: [{
      description: 'clear sky',
      icon: '01d'
    }],
    wind: {
      speed: 3.5
    }
  };

  const expectedWeatherData: WeatherData = {
    location: 'New York, US',
    temperature: 22,
    description: 'clear sky',
    icon: '01d',
    humidity: 65,
    windSpeed: 3.5,
    feelsLike: 24
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [WeatherService]
    });
    
    httpMock = TestBed.inject(HttpTestingController);

    // Mock geolocation
    const mockGeolocation = {
      getCurrentPosition: jasmine.createSpy('getCurrentPosition').and.callFake((success) => {
        success({
          coords: {
            latitude: 40.7128,
            longitude: -74.0060
          }
        });
      })
    };
    Object.defineProperty(navigator, 'geolocation', {
      value: mockGeolocation,
      configurable: true
    });

    service = TestBed.inject(WeatherService);
    
    // Handle the initialization request
    const initReq = httpMock.expectOne(request => request.url.includes('weather'));
    initReq.flush(mockWeatherResponse);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get weather by coordinates', () => {
    service.getWeatherByCoordinates(40.7128, -74.0060).subscribe(weather => {
      expect(weather).toEqual(expectedWeatherData);
    });

    const req = httpMock.expectOne(request => 
      request.url.includes('weather') && 
      request.url.includes('lat=40.7128') && 
      request.url.includes('lon=-74.006')
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockWeatherResponse);
  });

  it('should get weather by city', () => {
    service.getWeatherByCity('New York').subscribe(weather => {
      expect(weather).toEqual(expectedWeatherData);
    });

    const req = httpMock.expectOne(request => 
      request.url.includes('weather') && 
      request.url.includes('q=New York')
    );
    expect(req.request.method).toBe('GET');
    req.flush(mockWeatherResponse);
  });

  it('should return mock data on API error', () => {
    service.getWeatherByCoordinates(40.7128, -74.0060).subscribe(weather => {
      expect(weather.location).toBe('New York, US');
      expect(weather.temperature).toBe(22);
      expect(weather.description).toBe('Clear sky');
    });

    const req = httpMock.expectOne(request => request.url.includes('weather'));
    req.error(new ErrorEvent('Network error'));
  });

  it('should initialize weather on service creation', () => {
    // The service should have already made an HTTP request on initialization (handled in beforeEach)
    // Just verify the service was created successfully
    expect(service).toBeTruthy();
  });

  it('should handle geolocation error gracefully', () => {
    // Mock geolocation error
    const mockGeolocationError = {
      getCurrentPosition: jasmine.createSpy('getCurrentPosition').and.callFake((success, error) => {
        error({ code: 1, message: 'Permission denied' });
      })
    };
    Object.defineProperty(navigator, 'geolocation', {
      value: mockGeolocationError,
      configurable: true
    });

    // Create a new service instance to trigger initialization
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [WeatherService]
    });
    
    const httpMockNew = TestBed.inject(HttpTestingController);
    const newService = TestBed.inject(WeatherService);

    // Should fallback to default location (New York)
    const req = httpMockNew.expectOne(request => 
      request.url.includes('lat=40.7128') && 
      request.url.includes('lon=-74.006')
    );
    req.flush(mockWeatherResponse);
    
    httpMockNew.verify();
    expect(newService).toBeTruthy();
  });

  it('should handle missing geolocation API', () => {
    // Mock missing geolocation
    Object.defineProperty(navigator, 'geolocation', {
      value: undefined,
      configurable: true
    });

    // Create a new service instance to trigger initialization
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [WeatherService]
    });
    
    const httpMockNew = TestBed.inject(HttpTestingController);
    const newService = TestBed.inject(WeatherService);

    // Should fallback to default location
    const req = httpMockNew.expectOne(request => 
      request.url.includes('lat=40.7128') && 
      request.url.includes('lon=-74.006')
    );
    req.flush(mockWeatherResponse);
    
    httpMockNew.verify();
    expect(newService).toBeTruthy();
  });

  it('should transform weather response correctly', () => {
    const transformedData = service['transformWeatherResponse'](mockWeatherResponse);
    expect(transformedData).toEqual(expectedWeatherData);
  });

  it('should provide mock weather data', () => {
    const mockData = service['getMockWeatherData']();
    expect(mockData.location).toBe('New York, US');
    expect(mockData.temperature).toBe(22);
    expect(mockData.description).toBe('Clear sky');
    expect(mockData.icon).toBe('01d');
  });

  it('should get current weather from subject', () => {
    // After initialization (handled in beforeEach), should have weather data
    const currentWeather = service.getCurrentWeather();
    expect(currentWeather).toEqual(expectedWeatherData);
  });

  it('should refresh weather data', () => {
    service.refreshWeather();

    // Should make a new HTTP request
    const req = httpMock.expectOne(request => request.url.includes('weather'));
    expect(req.request.method).toBe('GET');
    req.flush(mockWeatherResponse);
  });

  it('should round temperature values correctly', () => {
    const responseWithDecimals = {
      ...mockWeatherResponse,
      main: {
        ...mockWeatherResponse.main,
        temp: 22.7,
        feels_like: 24.3
      }
    };

    const transformedData = service['transformWeatherResponse'](responseWithDecimals);
    expect(transformedData.temperature).toBe(23);
    expect(transformedData.feelsLike).toBe(24);
  });
});