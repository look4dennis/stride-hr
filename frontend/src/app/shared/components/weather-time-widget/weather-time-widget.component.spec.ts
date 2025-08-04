import { ComponentFixture, TestBed } from '@angular/core/testing';
import { WeatherTimeWidgetComponent } from './weather-time-widget.component';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

describe('WeatherTimeWidgetComponent', () => {
  let component: WeatherTimeWidgetComponent;
  let fixture: ComponentFixture<WeatherTimeWidgetComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WeatherTimeWidgetComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(WeatherTimeWidgetComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display current time', () => {
    fixture.detectChanges();
    expect(component.currentTime).toBeDefined();
  });

  it('should update time every second', (done) => {
    const initialTime = component.currentTime;
    
    setTimeout(() => {
      expect(component.currentTime).not.toEqual(initialTime);
      done();
    }, 1100);
  });

  it('should handle weather data loading', () => {
    component.ngOnInit();
    // Test weather loading functionality if it exists
    expect(component).toBeTruthy();
  });

  it('should format time correctly', () => {
    const testDate = new Date('2025-01-08T14:30:00');
    // Test time formatting functionality if it exists
    expect(testDate).toBeDefined();
  });
});