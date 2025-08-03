import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WeatherService, WeatherData } from '../../../core/services/weather.service';
import { Subscription, interval } from 'rxjs';

@Component({
  selector: 'app-weather-time-widget',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="weather-time-widget">
      <div class="time-section">
        <div class="current-time">{{ currentTime }}</div>
        <div class="current-date">{{ currentDate }}</div>
      </div>
      
      <div class="weather-section" *ngIf="weatherData">
        <div class="weather-main">
          <div class="weather-icon">
            <img [src]="getWeatherIconUrl(weatherData.icon)" 
                 [alt]="weatherData.description"
                 class="weather-img">
          </div>
          <div class="weather-temp">{{ weatherData.temperature }}°C</div>
        </div>
        <div class="weather-details">
          <div class="weather-location">
            <i class="fas fa-map-marker-alt"></i>
            {{ weatherData.location }}
          </div>
          <div class="weather-description">{{ weatherData.description | titlecase }}</div>
          <div class="weather-extra">
            <span class="feels-like">
              <i class="fas fa-thermometer-half"></i>
              Feels like {{ weatherData.feelsLike }}°C
            </span>
            <span class="humidity">
              <i class="fas fa-tint"></i>
              {{ weatherData.humidity }}%
            </span>
            <span class="wind">
              <i class="fas fa-wind"></i>
              {{ weatherData.windSpeed }} m/s
            </span>
          </div>
        </div>
      </div>
      
      <div class="widget-actions">
        <button class="btn btn-sm btn-outline-light" 
                (click)="refreshWeather()"
                [disabled]="isRefreshing">
          <i class="fas fa-sync-alt" [class.fa-spin]="isRefreshing"></i>
          Refresh
        </button>
      </div>
    </div>
  `,
  styles: [`
    .weather-time-widget {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border-radius: 16px;
      padding: 1.5rem;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
      position: relative;
      overflow: hidden;
    }

    .weather-time-widget::before {
      content: '';
      position: absolute;
      top: -50%;
      right: -50%;
      width: 100%;
      height: 100%;
      background: radial-gradient(circle, rgba(255,255,255,0.1) 0%, transparent 70%);
      pointer-events: none;
    }

    .time-section {
      text-align: center;
      margin-bottom: 1.5rem;
      position: relative;
      z-index: 1;
    }

    .current-time {
      font-size: 2.5rem;
      font-weight: 700;
      line-height: 1;
      margin-bottom: 0.25rem;
      text-shadow: 0 2px 4px rgba(0,0,0,0.3);
    }

    .current-date {
      font-size: 1rem;
      opacity: 0.9;
      font-weight: 500;
    }

    .weather-section {
      position: relative;
      z-index: 1;
    }

    .weather-main {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 1rem;
      margin-bottom: 1rem;
    }

    .weather-icon {
      width: 60px;
      height: 60px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .weather-img {
      width: 100%;
      height: 100%;
      object-fit: contain;
      filter: drop-shadow(0 2px 4px rgba(0,0,0,0.3));
    }

    .weather-temp {
      font-size: 2rem;
      font-weight: 700;
      text-shadow: 0 2px 4px rgba(0,0,0,0.3);
    }

    .weather-details {
      text-align: center;
    }

    .weather-location {
      font-size: 0.9rem;
      margin-bottom: 0.5rem;
      opacity: 0.9;
    }

    .weather-location i {
      margin-right: 0.25rem;
    }

    .weather-description {
      font-size: 1rem;
      font-weight: 500;
      margin-bottom: 1rem;
      opacity: 0.95;
    }

    .weather-extra {
      display: flex;
      justify-content: space-around;
      font-size: 0.8rem;
      opacity: 0.9;
      gap: 0.5rem;
      flex-wrap: wrap;
    }

    .weather-extra span {
      display: flex;
      align-items: center;
      gap: 0.25rem;
    }

    .widget-actions {
      position: absolute;
      top: 1rem;
      right: 1rem;
      z-index: 2;
    }

    .btn-outline-light {
      border: 1px solid rgba(255,255,255,0.3);
      color: white;
      background: rgba(255,255,255,0.1);
      backdrop-filter: blur(10px);
      font-size: 0.75rem;
      padding: 0.25rem 0.75rem;
      border-radius: 20px;
      transition: all 0.2s ease;
    }

    .btn-outline-light:hover {
      background: rgba(255,255,255,0.2);
      border-color: rgba(255,255,255,0.5);
      color: white;
      transform: translateY(-1px);
    }

    .btn-outline-light:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    @media (max-width: 768px) {
      .weather-time-widget {
        padding: 1rem;
      }

      .current-time {
        font-size: 2rem;
      }

      .weather-temp {
        font-size: 1.5rem;
      }

      .weather-extra {
        font-size: 0.75rem;
        justify-content: center;
      }
    }
  `]
})
export class WeatherTimeWidgetComponent implements OnInit, OnDestroy {
  currentTime: string = '';
  currentDate: string = '';
  weatherData: WeatherData | null = null;
  isRefreshing: boolean = false;

  private timeSubscription?: Subscription;
  private weatherSubscription?: Subscription;

  constructor(private weatherService: WeatherService) {}

  ngOnInit(): void {
    this.updateTime();
    this.startTimeUpdates();
    this.loadWeatherData();
  }

  ngOnDestroy(): void {
    this.timeSubscription?.unsubscribe();
    this.weatherSubscription?.unsubscribe();
  }

  private startTimeUpdates(): void {
    this.timeSubscription = interval(1000).subscribe(() => {
      this.updateTime();
    });
  }

  private updateTime(): void {
    const now = new Date();
    
    this.currentTime = now.toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: true
    });

    this.currentDate = now.toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  private loadWeatherData(): void {
    this.weatherSubscription = this.weatherService.weather$.subscribe(
      weather => {
        this.weatherData = weather;
      }
    );
  }

  refreshWeather(): void {
    this.isRefreshing = true;
    this.weatherService.refreshWeather();
    
    // Reset refreshing state after 2 seconds
    setTimeout(() => {
      this.isRefreshing = false;
    }, 2000);
  }

  getWeatherIconUrl(icon: string): string {
    return `https://openweathermap.org/img/wn/${icon}@2x.png`;
  }
}