import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, catchError, of, map } from 'rxjs';

export interface WeatherData {
  location: string;
  temperature: number;
  description: string;
  icon: string;
  humidity: number;
  windSpeed: number;
  feelsLike: number;
}

export interface LocationData {
  latitude: number;
  longitude: number;
  city: string;
  country: string;
}

@Injectable({
  providedIn: 'root'
})
export class WeatherService {
  private readonly API_KEY = 'demo-api-key'; // This should be in environment config
  private readonly BASE_URL = 'https://api.openweathermap.org/data/2.5';
  
  private weatherSubject = new BehaviorSubject<WeatherData | null>(null);
  public weather$ = this.weatherSubject.asObservable();

  constructor(private http: HttpClient) {
    this.initializeWeather();
  }

  private initializeWeather(): void {
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          this.getWeatherByCoordinates(
            position.coords.latitude,
            position.coords.longitude
          ).subscribe(weather => {
            this.weatherSubject.next(weather);
          });
        },
        (error) => {
          console.warn('Geolocation error:', error);
          // Fallback to default location (e.g., New York)
          this.getWeatherByCoordinates(40.7128, -74.0060).subscribe(weather => {
            this.weatherSubject.next(weather);
          });
        }
      );
    } else {
      // Fallback to default location
      this.getWeatherByCoordinates(40.7128, -74.0060).subscribe(weather => {
        this.weatherSubject.next(weather);
      });
    }
  }

  getWeatherByCoordinates(lat: number, lon: number): Observable<WeatherData> {
    const url = `${this.BASE_URL}/weather?lat=${lat}&lon=${lon}&appid=${this.API_KEY}&units=metric`;
    
    return this.http.get<any>(url).pipe(
      map(response => this.transformWeatherResponse(response)),
      catchError(error => {
        console.error('Weather API error:', error);
        // Return mock data on error
        return of(this.getMockWeatherData());
      })
    );
  }

  getWeatherByCity(city: string): Observable<WeatherData> {
    const url = `${this.BASE_URL}/weather?q=${city}&appid=${this.API_KEY}&units=metric`;
    
    return this.http.get<any>(url).pipe(
      map(response => this.transformWeatherResponse(response)),
      catchError(error => {
        console.error('Weather API error:', error);
        return of(this.getMockWeatherData());
      })
    );
  }

  private transformWeatherResponse(response: any): WeatherData {
    return {
      location: `${response.name}, ${response.sys.country}`,
      temperature: Math.round(response.main.temp),
      description: response.weather[0].description,
      icon: response.weather[0].icon,
      humidity: response.main.humidity,
      windSpeed: response.wind.speed,
      feelsLike: Math.round(response.main.feels_like)
    };
  }

  private getMockWeatherData(): WeatherData {
    return {
      location: 'New York, US',
      temperature: 22,
      description: 'Clear sky',
      icon: '01d',
      humidity: 65,
      windSpeed: 3.5,
      feelsLike: 24
    };
  }

  getCurrentWeather(): WeatherData | null {
    return this.weatherSubject.value;
  }

  refreshWeather(): void {
    this.initializeWeather();
  }
}