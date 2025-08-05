import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface PerformanceMetrics {
  pageLoadTime: number;
  firstContentfulPaint: number;
  largestContentfulPaint: number;
  cumulativeLayoutShift: number;
  firstInputDelay: number;
  timeToInteractive: number;
  bundleSize: number;
  memoryUsage: number;
}

export interface ComponentPerformance {
  componentName: string;
  renderTime: number;
  changeDetectionTime: number;
  memoryFootprint: number;
}

@Injectable({
  providedIn: 'root'
})
export class PerformanceService {
  private metricsSubject = new BehaviorSubject<PerformanceMetrics | null>(null);
  private componentMetrics: ComponentPerformance[] = [];
  private observer?: PerformanceObserver;

  constructor() {
    this.initializePerformanceMonitoring();
  }

  get metrics$(): Observable<PerformanceMetrics | null> {
    return this.metricsSubject.asObservable();
  }

  private initializePerformanceMonitoring(): void {
    if (typeof window !== 'undefined' && 'performance' in window) {
      // Monitor Core Web Vitals
      this.observeWebVitals();
      
      // Monitor resource loading
      this.observeResourceTiming();
      
      // Monitor long tasks
      this.observeLongTasks();
    }
  }

  private observeWebVitals(): void {
    // Largest Contentful Paint (LCP)
    if ('PerformanceObserver' in window) {
      this.observer = new PerformanceObserver((list) => {
        const entries = list.getEntries();
        const lastEntry = entries[entries.length - 1];
        
        if (lastEntry.entryType === 'largest-contentful-paint') {
          this.updateMetric('largestContentfulPaint', lastEntry.startTime);
        }
      });
      
      try {
        this.observer.observe({ entryTypes: ['largest-contentful-paint'] });
      } catch (e) {
        console.warn('LCP observation not supported');
      }
    }

    // First Input Delay (FID)
    if ('PerformanceObserver' in window) {
      const fidObserver = new PerformanceObserver((list) => {
        const entries = list.getEntries();
        entries.forEach((entry: any) => {
          if (entry.name === 'first-input') {
            this.updateMetric('firstInputDelay', entry.processingStart - entry.startTime);
          }
        });
      });
      
      try {
        fidObserver.observe({ entryTypes: ['first-input'] });
      } catch (e) {
        console.warn('FID observation not supported');
      }
    }

    // Cumulative Layout Shift (CLS)
    if ('PerformanceObserver' in window) {
      let clsValue = 0;
      const clsObserver = new PerformanceObserver((list) => {
        const entries = list.getEntries();
        entries.forEach((entry: any) => {
          if (!entry.hadRecentInput) {
            clsValue += entry.value;
            this.updateMetric('cumulativeLayoutShift', clsValue);
          }
        });
      });
      
      try {
        clsObserver.observe({ entryTypes: ['layout-shift'] });
      } catch (e) {
        console.warn('CLS observation not supported');
      }
    }
  }

  private observeResourceTiming(): void {
    if ('PerformanceObserver' in window) {
      const resourceObserver = new PerformanceObserver((list) => {
        const entries = list.getEntries();
        entries.forEach((entry: any) => {
          if (entry.initiatorType === 'script' || entry.initiatorType === 'link') {
            // Monitor critical resource loading times
            console.debug(`Resource ${entry.name} loaded in ${entry.duration}ms`);
          }
        });
      });
      
      try {
        resourceObserver.observe({ entryTypes: ['resource'] });
      } catch (e) {
        console.warn('Resource timing observation not supported');
      }
    }
  }

  private observeLongTasks(): void {
    if ('PerformanceObserver' in window) {
      const longTaskObserver = new PerformanceObserver((list) => {
        const entries = list.getEntries();
        entries.forEach((entry) => {
          console.warn(`Long task detected: ${entry.duration}ms`, entry);
        });
      });
      
      try {
        longTaskObserver.observe({ entryTypes: ['longtask'] });
      } catch (e) {
        console.warn('Long task observation not supported');
      }
    }
  }

  private updateMetric(key: keyof PerformanceMetrics, value: number): void {
    const currentMetrics = this.metricsSubject.value || {} as PerformanceMetrics;
    const updatedMetrics = { ...currentMetrics, [key]: value };
    this.metricsSubject.next(updatedMetrics);
  }

  /**
   * Measure component performance
   */
  measureComponent(componentName: string, renderFn: () => void): void {
    const startTime = performance.now();
    const startMemory = this.getMemoryUsage();
    
    renderFn();
    
    const endTime = performance.now();
    const endMemory = this.getMemoryUsage();
    
    const componentPerf: ComponentPerformance = {
      componentName,
      renderTime: endTime - startTime,
      changeDetectionTime: 0, // Will be updated by change detection hooks
      memoryFootprint: endMemory - startMemory
    };
    
    this.componentMetrics.push(componentPerf);
    
    if (componentPerf.renderTime > 16) { // More than one frame (60fps)
      console.warn(`Slow component render: ${componentName} took ${componentPerf.renderTime}ms`);
    }
  }

  /**
   * Get current performance metrics
   */
  getCurrentMetrics(): PerformanceMetrics {
    const navigation = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming;
    const paint = performance.getEntriesByType('paint');
    
    const fcp = paint.find(entry => entry.name === 'first-contentful-paint');
    
    return {
      pageLoadTime: navigation ? navigation.loadEventEnd - navigation.fetchStart : 0,
      firstContentfulPaint: fcp ? fcp.startTime : 0,
      largestContentfulPaint: this.metricsSubject.value?.largestContentfulPaint || 0,
      cumulativeLayoutShift: this.metricsSubject.value?.cumulativeLayoutShift || 0,
      firstInputDelay: this.metricsSubject.value?.firstInputDelay || 0,
      timeToInteractive: this.calculateTTI(),
      bundleSize: this.estimateBundleSize(),
      memoryUsage: this.getMemoryUsage()
    };
  }

  /**
   * Get component performance metrics
   */
  getComponentMetrics(): ComponentPerformance[] {
    return [...this.componentMetrics];
  }

  /**
   * Clear component metrics
   */
  clearComponentMetrics(): void {
    this.componentMetrics = [];
  }

  /**
   * Optimize images by lazy loading
   */
  optimizeImages(): void {
    if ('IntersectionObserver' in window) {
      const imageObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            const img = entry.target as HTMLImageElement;
            if (img.dataset['src']) {
              img.src = img.dataset['src'];
              img.removeAttribute('data-src');
              imageObserver.unobserve(img);
            }
          }
        });
      });

      document.querySelectorAll('img[data-src]').forEach(img => {
        imageObserver.observe(img);
      });
    }
  }

  /**
   * Preload critical resources
   */
  preloadCriticalResources(resources: string[]): void {
    resources.forEach(resource => {
      const link = document.createElement('link');
      link.rel = 'preload';
      link.href = resource;
      
      if (resource.endsWith('.js')) {
        link.as = 'script';
      } else if (resource.endsWith('.css')) {
        link.as = 'style';
      } else if (resource.match(/\.(woff|woff2|ttf|otf)$/)) {
        link.as = 'font';
        link.crossOrigin = 'anonymous';
      }
      
      document.head.appendChild(link);
    });
  }

  /**
   * Enable performance monitoring in production
   */
  enableProductionMonitoring(): void {
    // Send metrics to analytics service
    const metrics = this.getCurrentMetrics();
    
    // Only send if metrics are meaningful
    if (metrics.pageLoadTime > 0) {
      this.sendMetricsToAnalytics(metrics);
    }
  }

  private calculateTTI(): number {
    // Simplified TTI calculation
    const navigation = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming;
    return navigation ? navigation.domInteractive - navigation.fetchStart : 0;
  }

  private estimateBundleSize(): number {
    // Estimate based on loaded resources
    const resources = performance.getEntriesByType('resource');
    return resources
      .filter((resource: any) => resource.initiatorType === 'script')
      .reduce((total: number, resource: any) => total + (resource.transferSize || 0), 0);
  }

  private getMemoryUsage(): number {
    if ('memory' in performance) {
      return (performance as any).memory.usedJSHeapSize;
    }
    return 0;
  }

  private sendMetricsToAnalytics(metrics: PerformanceMetrics): void {
    // Implementation would send to your analytics service
    console.log('Performance metrics:', metrics);
  }

  /**
   * Generate performance report
   */
  generateReport(): string {
    const metrics = this.getCurrentMetrics();
    const componentMetrics = this.getComponentMetrics();
    
    return `
=== Frontend Performance Report ===
Page Load Time: ${metrics.pageLoadTime.toFixed(2)}ms
First Contentful Paint: ${metrics.firstContentfulPaint.toFixed(2)}ms
Largest Contentful Paint: ${metrics.largestContentfulPaint.toFixed(2)}ms
Cumulative Layout Shift: ${metrics.cumulativeLayoutShift.toFixed(4)}
First Input Delay: ${metrics.firstInputDelay.toFixed(2)}ms
Time to Interactive: ${metrics.timeToInteractive.toFixed(2)}ms
Bundle Size: ${(metrics.bundleSize / 1024).toFixed(2)}KB
Memory Usage: ${(metrics.memoryUsage / 1024 / 1024).toFixed(2)}MB

Component Performance:
${componentMetrics.map(c => 
  `- ${c.componentName}: ${c.renderTime.toFixed(2)}ms render, ${(c.memoryFootprint / 1024).toFixed(2)}KB memory`
).join('\n')}

Performance Assessment:
${this.assessPerformance(metrics)}
`;
  }

  private assessPerformance(metrics: PerformanceMetrics): string {
    const issues: string[] = [];
    const recommendations: string[] = [];

    if (metrics.pageLoadTime > 3000) {
      issues.push('Page load time exceeds 3 seconds');
      recommendations.push('Optimize bundle size and enable code splitting');
    }

    if (metrics.firstContentfulPaint > 1800) {
      issues.push('First Contentful Paint is slow');
      recommendations.push('Optimize critical rendering path and reduce render-blocking resources');
    }

    if (metrics.largestContentfulPaint > 2500) {
      issues.push('Largest Contentful Paint exceeds 2.5 seconds');
      recommendations.push('Optimize images and implement lazy loading');
    }

    if (metrics.cumulativeLayoutShift > 0.1) {
      issues.push('High Cumulative Layout Shift detected');
      recommendations.push('Set explicit dimensions for images and ads');
    }

    if (metrics.firstInputDelay > 100) {
      issues.push('First Input Delay is high');
      recommendations.push('Reduce JavaScript execution time and implement code splitting');
    }

    if (issues.length === 0) {
      return '✅ All performance metrics are within acceptable ranges';
    }

    return `Issues: ${issues.join(', ')}\nRecommendations: ${recommendations.join(', ')}`;
  }
}