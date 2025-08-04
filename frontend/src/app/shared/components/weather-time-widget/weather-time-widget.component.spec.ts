import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { of } from 'rxjs';
import { WeatherTimeWidgetComponent } from './weather-time-widget.component';
import { WeatherService, WeatherData } from '../../../core/services/weather.service';

describe('WeatherTimeWidgetComponent', () => {
  let component: WeatherTimeWidgetComponent;
  let fixture: ComponentFixture<WeatherTimeWidgetComponent>;
  let mockWeatherService: jasmine.SpyObj<WeatherService>;

  const mockWeatherData: WeatherData = {
    location: 'New York, US',
    temperature: 22,
    description: 'Clear sky',
    icon: '01d',
    humidity: 65,
    windSpeed: 3.5,
    feelsLike: 24
  };

  beforeEach(async () => {
    const weatherServiceSpy = jasmine.createSpyObj('WeatherService', ['refreshWeather'], {
      weather$: of(mockWeatherData)
    });

    await TestBed.configureTestingModule({
      imports: [WeatherTimeWidgetComponent],
      providers: [
        { provide: WeatherService, useValue: weatherServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(WeatherTimeWidgetComponent);
    component = fixture.componentInstance;
    mockWeatherService = TestBed.inject(WeatherService) as jasmine.SpyObj<WeatherService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with current time and weather data', () => {
    // Act
    component.ngOnInit();
    fixture.detectChanges();

    // Assert
    expect(component.currentTime).toBeDefined();
    expect(component.currentDate).toBeDefined();
    expect(component.weatherData).toEqual(mockWeatherData);
  });

  it('should update time every second', (done) => {
    // Arrange
    const initialTime = component.currentTime;
    
    // Act
    component.ngOnInit();
    
    // Assert
    setTimeout(() => {
      expect(component.currentTime).not.toEqual(initialTime);
      done();
    }, 1100); // Wait slightly more than 1 second
  });

  it('should handle null weather data gracefully', () => {
    // Arrange
    const weatherServiceSpy = jasmine.createSpyObj('WeatherService', ['refreshWeather'], {
      weather$: of(null)
    });
    TestBed.overrideProvider(WeatherService, { useValue: weatherServiceSpy });
    fixture = TestBed.createComponent(WeatherTimeWidgetComponent);
    component = fixture.componentInstance;

    // Act
    component.ngOnInit();
    fixture.detectChanges();

    // Assert
    expect(component.weatherData).toBeNull();
    const weatherSection = fixture.debugElement.query(By.css('.weather-section'));
    expect(weatherSection).toBeFalsy();
  });

  it('should get weather icon URL correctly', () => {
    // Act
    const iconUrl = component.getWeatherIconUrl('01d');

    // Assert
    expect(iconUrl).toBe('https://openweathermap.org/img/wn/01d@2x.png');
  });

  it('should refresh weather data', () => {
    // Act
    component.refreshWeather();

    // Assert
    expect(mockWeatherService.refreshWeather).toHaveBeenCalled();
    expect(component.isRefreshing).toBe(true);
  });

  it('should reset refreshing state after timeout', (done) => {
    // Act
    component.refreshWeather();

    // Assert
    expect(component.isRefreshing).toBe(true);
    
    setTimeout(() => {
      expect(component.isRefreshing).toBe(false);
      done();
    }, 2100); // Wait slightly more than 2 seconds
  });

  it('should display current time', () => {
    // Arrange
    component.currentTime = '2:30:45 PM';
    fixture.detectChanges();

    // Act & Assert
    const timeElement = fixture.debugElement.query(By.css('.current-time'));
    expect(timeElement).toBeTruthy();
    expect(timeElement.nativeElement.textContent.trim()).toBe('2:30:45 PM');
  });

  it('should display current date', () => {
    // Arrange
    component.currentDate = 'Wednesday, January 15, 2025';
    fixture.detectChanges();

    // Act & Assert
    const dateElement = fixture.debugElement.query(By.css('.current-date'));
    expect(dateElement).toBeTruthy();
    expect(dateElement.nativeElement.textContent.trim()).toBe('Wednesday, January 15, 2025');
  });

  it('should display weather data when available', () => {
    // Arrange
    component.weatherData = mockWeatherData;
    fixture.detectChanges();

    // Act & Assert
    const weatherSection = fixture.debugElement.query(By.css('.weather-section'));
    expect(weatherSection).toBeTruthy();

    const temperatureElement = fixture.debugElement.query(By.css('.weather-temp'));
    expect(temperatureElement.nativeElement.textContent.trim()).toBe('22°C');

    const locationElement = fixture.debugElement.query(By.css('.weather-location'));
    expect(locationElement.nativeElement.textContent.trim()).toContain('New York, US');

    const descriptionElement = fixture.debugElement.query(By.css('.weather-description'));
    expect(descriptionElement.nativeElement.textContent.trim()).toBe('Clear Sky');
  });

  it('should display weather icon', () => {
    // Arrange
    component.weatherData = mockWeatherData;
    fixture.detectChanges();

    // Act & Assert
    const iconElement = fixture.debugElement.query(By.css('.weather-img'));
    expect(iconElement).toBeTruthy();
    expect(iconElement.nativeElement.src).toBe('https://openweathermap.org/img/wn/01d@2x.png');
    expect(iconElement.nativeElement.alt).toBe('Clear sky');
  });

  it('should display weather extra details', () => {
    // Arrange
    component.weatherData = mockWeatherData;
    fixture.detectChanges();

    // Act & Assert
    const extraDetails = fixture.debugElement.query(By.css('.weather-extra'));
    expect(extraDetails).toBeTruthy();

    const feelsLike = extraDetails.query(By.css('.feels-like'));
    expect(feelsLike.nativeElement.textContent.trim()).toContain('24°C');

    const humidity = extraDetails.query(By.css('.humidity'));
    expect(humidity.nativeElement.textContent.trim()).toContain('65%');

    const wind = extraDetails.query(By.css('.wind'));
    expect(wind.nativeElement.textContent.trim()).toContain('3.5 m/s');
  });

  it('should handle refresh button click', () => {
    // Arrange
    spyOn(component, 'refreshWeather');
    fixture.detectChanges();

    // Act
    const refreshButton = fixture.debugElement.query(By.css('.btn'));
    refreshButton.nativeElement.click();

    // Assert
    expect(component.refreshWeather).toHaveBeenCalled();
  });

  it('should disable refresh button when refreshing', () => {
    // Arrange
    component.isRefreshing = true;
    fixture.detectChanges();

    // Act & Assert
    const refreshButton = fixture.debugElement.query(By.css('.btn'));
    expect(refreshButton.nativeElement.disabled).toBe(true);
  });

  it('should show spinning icon when refreshing', () => {
    // Arrange
    component.isRefreshing = true;
    fixture.detectChanges();

    // Act & Assert
    const spinIcon = fixture.debugElement.query(By.css('.fa-spin'));
    expect(spinIcon).toBeTruthy();
  });

  it('should cleanup subscriptions on destroy', () => {
    // Arrange
    component.ngOnInit();
    const timeSubscription = component['timeSubscription'];
    const weatherSubscription = component['weatherSubscription'];
    
    if (timeSubscription) {
      spyOn(timeSubscription, 'unsubscribe');
    }
    if (weatherSubscription) {
      spyOn(weatherSubscription, 'unsubscribe');
    }

    // Act
    component.ngOnDestroy();

    // Assert
    if (timeSubscription) {
      expect(timeSubscription.unsubscribe).toHaveBeenCalled();
    }
    if (weatherSubscription) {
      expect(weatherSubscription.unsubscribe).toHaveBeenCalled();
    }
  });

  it('should format time correctly using updateTime method', () => {
    // Arrange
    jasmine.clock().install();
    const testDate = new Date('2025-01-15T14:30:45');
    jasmine.clock().mockDate(testDate);

    // Act
    component['updateTime']();

    // Assert
    expect(component.currentTime).toBe('02:30:45 PM');
    expect(component.currentDate).toBe('Wednesday, January 15, 2025');

    // Cleanup
    jasmine.clock().uninstall();
  });

  it('should not display weather section when weather data is null', () => {
    // Arrange
    component.weatherData = null;
    fixture.detectChanges();

    // Act & Assert
    const weatherSection = fixture.debugElement.query(By.css('.weather-section'));
    expect(weatherSection).toBeFalsy();
  });

  it('should handle weather service subscription', () => {
    // Arrange
    const newWeatherData: WeatherData = {
      ...mockWeatherData,
      temperature: 25,
      description: 'Sunny'
    };
    
    const weatherServiceSpy = jasmine.createSpyObj('WeatherService', ['refreshWeather'], {
      weather$: of(newWeatherData)
    });
    TestBed.overrideProvider(WeatherService, { useValue: weatherServiceSpy });
    fixture = TestBed.createComponent(WeatherTimeWidgetComponent);
    component = fixture.componentInstance;

    // Act
    component.ngOnInit();

    // Assert
    expect(component.weatherData).toEqual(newWeatherData);
  });
});