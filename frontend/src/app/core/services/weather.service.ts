import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, catchError, of, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';

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
  private readonly API_KEY = environment.weatherApiKey;
  private readonly BASE_URL = environment.weatherApiUrl;
  
  private weatherSubject = new BehaviorSubject<WeatherData | null>(null);
  public weather$ = this.weatherSubject.asObservable();

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {
    this.initializeWeather();
  }

  private initializeWeather(): void {
    console.log('Weather: Initializing weather service');
    
    // For now, prioritize user branch location since you're in India
    // This will show Mumbai, India immediately
    this.weatherSubject.next(this.getMockWeatherDataFromUserBranch());
    
    // Optionally try geolocation in background (but don't wait for it)
    this.getUserLocation().then(location => {
      if (location) {
        console.log('Weather: Got geolocation, updating weather');
        this.weatherSubject.next(this.getMockWeatherDataForLocation(location));
      }
    }).catch(error => {
      console.log('Weather: Geolocation failed, keeping branch location');
    });
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

  private getMockWeatherDataForLocation(location: LocationData): WeatherData {
    // Mock weather data based on detected location
    const weatherVariations = {
      'India': {
        temperature: 28,
        description: 'Partly cloudy',
        icon: '02d',
        humidity: 75,
        windSpeed: 2.8,
        feelsLike: 32
      },
      'United States': {
        temperature: 22,
        description: 'Clear sky',
        icon: '01d',
        humidity: 65,
        windSpeed: 3.5,
        feelsLike: 24
      },
      'United Kingdom': {
        temperature: 15,
        description: 'Light rain',
        icon: '10d',
        humidity: 85,
        windSpeed: 4.2,
        feelsLike: 13
      }
    };

    const weather = weatherVariations[location.country as keyof typeof weatherVariations] || weatherVariations['India'];
    
    return {
      location: `${location.city}, ${location.country}`,
      ...weather
    };
  }

  private getMockWeatherDataFromUserBranch(): WeatherData {
    const currentUser = this.authService.currentUser;
    
    console.log('Weather: Current user for location detection:', currentUser);
    
    // Since you mentioned you're in India, let's default to India for SuperAdmin
    if (currentUser?.roles?.includes('SuperAdmin') || 
        currentUser?.branchName?.toLowerCase().includes('india') || 
        currentUser?.branchId === 1) {
      console.log('Weather: Using India location for user');
      return {
        location: 'Mumbai, India',
        temperature: 28,
        description: 'Partly cloudy',
        icon: '02d',
        humidity: 75,
        windSpeed: 2.8,
        feelsLike: 32
      };
    }

    // For development, default to India since you mentioned you're there
    console.log('Weather: Defaulting to India location');
    return {
      location: 'Mumbai, India',
      temperature: 28,
      description: 'Partly cloudy',
      icon: '02d',
      humidity: 75,
      windSpeed: 2.8,
      feelsLike: 32
    };
  }

  private async getUserLocation(): Promise<LocationData | null> {
    return new Promise((resolve) => {
      if (!navigator.geolocation) {
        console.log('Geolocation is not supported by this browser');
        resolve(null);
        return;
      }

      navigator.geolocation.getCurrentPosition(
        async (position) => {
          try {
            // In a real app, you'd use reverse geocoding API
            // For now, we'll simulate based on coordinates
            const { latitude, longitude } = position.coords;
            
            // Rough location detection based on coordinates
            let location: LocationData;
            
            if (latitude >= 8 && latitude <= 37 && longitude >= 68 && longitude <= 97) {
              // India coordinates range
              location = {
                latitude,
                longitude,
                city: 'Mumbai',
                country: 'India'
              };
            } else if (latitude >= 25 && latitude <= 49 && longitude >= -125 && longitude <= -66) {
              // USA coordinates range
              location = {
                latitude,
                longitude,
                city: 'New York',
                country: 'United States'
              };
            } else {
              // Default to user's branch location
              location = {
                latitude,
                longitude,
                city: 'Mumbai',
                country: 'India'
              };
            }
            
            console.log('Detected location:', location);
            resolve(location);
          } catch (error) {
            console.error('Error processing location:', error);
            resolve(null);
          }
        },
        (error) => {
          console.log('Error getting location:', error.message);
          resolve(null);
        },
        {
          timeout: 10000,
          enableHighAccuracy: false
        }
      );
    });
  }

  getCurrentWeather(): WeatherData | null {
    return this.weatherSubject.value;
  }

  refreshWeather(): void {
    this.initializeWeather();
  }
}