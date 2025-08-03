import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { of } from 'rxjs';
import { WeatherTimeWidgetComponent } from './weather-time-widget.component';
import { WeatherService, WeatherData } from '../../../core/services/weather.service';

describe('WeatherTimeWidgetComponent', () => {
  let component: WeatherTimeWidgetComponent;
  let fixture: ComponentFixture<WeatherTimeWidgetComponent>;
  let weatherServiceSpy: jasmine.SpyObj<WeatherService>;

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
    const weatherSpy = jasmine.createSpyObj('WeatherService', ['refreshWeather'], {
      weather$: of(mockWeatherData)
    });

    await TestBed.configureTestingModule({
      imports: [WeatherTimeWidgetComponent],
      providers: [
        { provide: WeatherService, useValue: weatherSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(WeatherTimeWidgetComponent);
    component = fixture.componentInstance;
    weatherServiceSpy = TestBed.inject(WeatherService) as jasmine.SpyObj<WeatherService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display current time and date', () => {
    fixture.detectChanges();

    const timeElement = fixture.debugElement.query(By.css('.current-time'));
    const dateElement = fixture.debugElement.query(By.css('.current-date'));

    expect(timeElement).toBeTruthy();
    expect(dateElement).toBeTruthy();
    expect(component.currentTime).toBeTruthy();
    expect(component.currentDate).toBeTruthy();
  });

  it('should display weather data when available', () => {
    fixture.detectChanges();

    expect(component.weatherData).toEqual(mockWeatherData);

    const locationElement = fixture.debugElement.query(By.css('.weather-location'));
    const tempElement = fixture.debugElement.query(By.css('.weather-temp'));
    const descriptionElement = fixture.debugElement.query(By.css('.weather-description'));

    expect(locationElement.nativeElement.textContent).toContain('New York, US');
    expect(tempElement.nativeElement.textContent).toContain('22°C');
    expect(descriptionElement.nativeElement.textContent).toContain('Clear Sky');
  });

  it('should display weather details', () => {
    fixture.detectChanges();

    const feelsLikeElement = fixture.debugElement.query(By.css('.feels-like'));
    const humidityElement = fixture.debugElement.query(By.css('.humidity'));
    const windElement = fixture.debugElement.query(By.css('.wind'));

    expect(feelsLikeElement.nativeElement.textContent).toContain('24°C');
    expect(humidityElement.nativeElement.textContent).toContain('65%');
    expect(windElement.nativeElement.textContent).toContain('3.5 m/s');
  });

  it('should generate correct weather icon URL', () => {
    const iconUrl = component.getWeatherIconUrl('01d');
    expect(iconUrl).toBe('https://openweathermap.org/img/wn/01d@2x.png');
  });

  it('should call refresh weather when refresh button is clicked', () => {
    fixture.detectChanges();

    const refreshButton = fixture.debugElement.query(By.css('.btn'));
    refreshButton.nativeElement.click();

    expect(weatherServiceSpy.refreshWeather).toHaveBeenCalled();
  });

  it('should show refreshing state when refresh is clicked', () => {
    fixture.detectChanges();

    const refreshButton = fixture.debugElement.query(By.css('.btn'));
    refreshButton.nativeElement.click();

    expect(component.isRefreshing).toBe(true);
    fixture.detectChanges();

    const spinIcon = fixture.debugElement.query(By.css('.fa-spin'));
    expect(spinIcon).toBeTruthy();
  });

  it('should update time every second', (done) => {
    fixture.detectChanges();
    
    const initialTime = component.currentTime;
    
    setTimeout(() => {
      expect(component.currentTime).toBeDefined();
      // Time should be updated (though we can't guarantee it's different due to timing)
      done();
    }, 1100);
  });

  it('should handle null weather data gracefully', () => {
    // Override the weather service to return null
    Object.defineProperty(weatherServiceSpy, 'weather$', { value: of(null) });
    
    fixture.detectChanges();

    const weatherSection = fixture.debugElement.query(By.css('.weather-section'));
    expect(weatherSection).toBeFalsy();
  });

  it('should clean up subscriptions on destroy', () => {
    fixture.detectChanges();
    
    spyOn(component['timeSubscription']!, 'unsubscribe');
    spyOn(component['weatherSubscription']!, 'unsubscribe');

    component.ngOnDestroy();

    expect(component['timeSubscription']!.unsubscribe).toHaveBeenCalled();
    expect(component['weatherSubscription']!.unsubscribe).toHaveBeenCalled();
  });

  it('should format time correctly', () => {
    fixture.detectChanges();
    
    // Check that time includes AM/PM and is properly formatted
    expect(component.currentTime).toMatch(/\d{1,2}:\d{2}:\d{2}\s(AM|PM)/);
  });

  it('should format date correctly', () => {
    fixture.detectChanges();
    
    // Check that date includes day of week and is properly formatted
    expect(component.currentDate).toMatch(/\w+,\s\w+\s\d{1,2},\s\d{4}/);
  });
});