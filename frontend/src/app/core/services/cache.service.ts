import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface CacheItem<T> {
  data: T;
  timestamp: number;
  ttl: number; // Time to live in milliseconds
}

export interface CacheConfig {
  defaultTTL: number;
  maxSize: number;
  enableLogging: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class CacheService {
  private cache = new Map<string, CacheItem<any>>();
  private cacheHits = 0;
  private cacheMisses = 0;
  
  private readonly config: CacheConfig = {
    defaultTTL: 5 * 60 * 1000, // 5 minutes
    maxSize: 100,
    enableLogging: false
  };

  // Cache statistics subject for monitoring
  private statsSubject = new BehaviorSubject({
    hits: 0,
    misses: 0,
    hitRate: 0,
    size: 0
  });

  public stats$ = this.statsSubject.asObservable();

  constructor() {
    // Clean up expired cache entries every minute
    setInterval(() => this.cleanupExpired(), 60000);
  }

  /**
   * Get data from cache or execute the provided function
   */
  getOrSet<T>(
    key: string, 
    dataProvider: () => Observable<T>, 
    ttl: number = this.config.defaultTTL
  ): Observable<T> {
    const cached = this.get<T>(key);
    
    if (cached !== null) {
      this.cacheHits++;
      this.updateStats();
      if (this.config.enableLogging) {
        console.log(`Cache HIT for key: ${key}`);
      }
      return of(cached);
    }

    this.cacheMisses++;
    this.updateStats();
    if (this.config.enableLogging) {
      console.log(`Cache MISS for key: ${key}`);
    }

    return dataProvider().pipe(
      tap(data => this.set(key, data, ttl))
    );
  }

  /**
   * Set data in cache
   */
  set<T>(key: string, data: T, ttl: number = this.config.defaultTTL): void {
    // Implement LRU eviction if cache is full
    if (this.cache.size >= this.config.maxSize) {
      this.evictLRU();
    }

    const cacheItem: CacheItem<T> = {
      data,
      timestamp: Date.now(),
      ttl
    };

    this.cache.set(key, cacheItem);
    this.updateStats();
  }

  /**
   * Get data from cache
   */
  get<T>(key: string): T | null {
    const item = this.cache.get(key);
    
    if (!item) {
      return null;
    }

    // Check if item has expired
    if (Date.now() - item.timestamp > item.ttl) {
      this.cache.delete(key);
      this.updateStats();
      return null;
    }

    return item.data;
  }

  /**
   * Check if key exists in cache and is not expired
   */
  has(key: string): boolean {
    return this.get(key) !== null;
  }

  /**
   * Remove specific key from cache
   */
  delete(key: string): boolean {
    const result = this.cache.delete(key);
    this.updateStats();
    return result;
  }

  /**
   * Clear all cache entries
   */
  clear(): void {
    this.cache.clear();
    this.cacheHits = 0;
    this.cacheMisses = 0;
    this.updateStats();
  }

  /**
   * Invalidate cache entries matching pattern
   */
  invalidatePattern(pattern: string): void {
    const regex = new RegExp(pattern);
    const keysToDelete: string[] = [];

    for (const key of this.cache.keys()) {
      if (regex.test(key)) {
        keysToDelete.push(key);
      }
    }

    keysToDelete.forEach(key => this.cache.delete(key));
    this.updateStats();

    if (this.config.enableLogging) {
      console.log(`Invalidated ${keysToDelete.length} cache entries matching pattern: ${pattern}`);
    }
  }

  /**
   * Get cache statistics
   */
  getStats() {
    return {
      hits: this.cacheHits,
      misses: this.cacheMisses,
      hitRate: this.cacheHits + this.cacheMisses > 0 
        ? (this.cacheHits / (this.cacheHits + this.cacheMisses)) * 100 
        : 0,
      size: this.cache.size,
      maxSize: this.config.maxSize
    };
  }

  /**
   * Cache user preferences
   */
  cacheUserPreferences(userId: string, preferences: any): void {
    this.set(`user_preferences_${userId}`, preferences, 24 * 60 * 60 * 1000); // 24 hours
  }

  /**
   * Get cached user preferences
   */
  getCachedUserPreferences(userId: string): any | null {
    return this.get(`user_preferences_${userId}`);
  }

  /**
   * Cache component state
   */
  cacheComponentState(componentId: string, state: any): void {
    this.set(`component_state_${componentId}`, state, 10 * 60 * 1000); // 10 minutes
  }

  /**
   * Get cached component state
   */
  getCachedComponentState(componentId: string): any | null {
    return this.get(`component_state_${componentId}`);
  }

  /**
   * Cache API response
   */
  cacheApiResponse<T>(endpoint: string, params: any, data: T, ttl?: number): void {
    const key = this.generateApiCacheKey(endpoint, params);
    this.set(key, data, ttl);
  }

  /**
   * Get cached API response
   */
  getCachedApiResponse<T>(endpoint: string, params: any): T | null {
    const key = this.generateApiCacheKey(endpoint, params);
    return this.get<T>(key);
  }

  /**
   * Invalidate API cache for specific endpoint
   */
  invalidateApiCache(endpoint: string): void {
    this.invalidatePattern(`api_${endpoint.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}`);
  }

  /**
   * Enable or disable cache logging
   */
  setLogging(enabled: boolean): void {
    this.config.enableLogging = enabled;
  }

  /**
   * Update cache configuration
   */
  updateConfig(config: Partial<CacheConfig>): void {
    Object.assign(this.config, config);
  }

  /**
   * Generate cache key for API responses
   */
  private generateApiCacheKey(endpoint: string, params: any): string {
    const paramString = params ? JSON.stringify(params) : '';
    return `api_${endpoint}_${this.hashString(paramString)}`;
  }

  /**
   * Simple hash function for generating cache keys
   */
  private hashString(str: string): string {
    let hash = 0;
    if (str.length === 0) return hash.toString();
    
    for (let i = 0; i < str.length; i++) {
      const char = str.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash; // Convert to 32-bit integer
    }
    
    return Math.abs(hash).toString();
  }

  /**
   * Clean up expired cache entries
   */
  private cleanupExpired(): void {
    const now = Date.now();
    const keysToDelete: string[] = [];

    for (const [key, item] of this.cache.entries()) {
      if (now - item.timestamp > item.ttl) {
        keysToDelete.push(key);
      }
    }

    keysToDelete.forEach(key => this.cache.delete(key));
    
    if (keysToDelete.length > 0) {
      this.updateStats();
      if (this.config.enableLogging) {
        console.log(`Cleaned up ${keysToDelete.length} expired cache entries`);
      }
    }
  }

  /**
   * Evict least recently used item
   */
  private evictLRU(): void {
    let oldestKey: string | null = null;
    let oldestTimestamp = Date.now();

    for (const [key, item] of this.cache.entries()) {
      if (item.timestamp < oldestTimestamp) {
        oldestTimestamp = item.timestamp;
        oldestKey = key;
      }
    }

    if (oldestKey) {
      this.cache.delete(oldestKey);
      if (this.config.enableLogging) {
        console.log(`Evicted LRU cache entry: ${oldestKey}`);
      }
    }
  }

  /**
   * Update cache statistics
   */
  private updateStats(): void {
    this.statsSubject.next(this.getStats());
  }
}