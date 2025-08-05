import { Directive, ElementRef, Input, OnInit, OnDestroy, Renderer2 } from '@angular/core';

@Directive({
  selector: '[appLazyImage]',
  standalone: true
})
export class LazyImageDirective implements OnInit, OnDestroy {
  @Input('appLazyImage') src!: string;
  @Input() placeholder = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzIwIiBoZWlnaHQ9IjE4MCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBmaWxsPSIjZGRkIi8+PHRleHQgeD0iNTAlIiB5PSI1MCUiIGZvbnQtZmFtaWx5PSJBcmlhbCIgZm9udC1zaXplPSIxNCIgZmlsbD0iIzk5OSIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZHk9Ii4zZW0iPkxvYWRpbmcuLi48L3RleHQ+PC9zdmc+';
  @Input() errorImage = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzIwIiBoZWlnaHQ9IjE4MCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBmaWxsPSIjZjVmNWY1Ii8+PHRleHQgeD0iNTAlIiB5PSI1MCUiIGZvbnQtZmFtaWx5PSJBcmlhbCIgZm9udC1zaXplPSIxNCIgZmlsbD0iIzk5OSIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZHk9Ii4zZW0iPkVycm9yPC90ZXh0Pjwvc3ZnPg==';
  @Input() threshold = 0.1;
  @Input() rootMargin = '50px';

  private observer?: IntersectionObserver;
  private loaded = false;
  private error = false;

  constructor(
    private el: ElementRef<HTMLImageElement>,
    private renderer: Renderer2
  ) {}

  ngOnInit(): void {
    this.setupLazyLoading();
  }

  ngOnDestroy(): void {
    if (this.observer) {
      this.observer.disconnect();
    }
  }

  private setupLazyLoading(): void {
    // Set placeholder initially
    this.renderer.setAttribute(this.el.nativeElement, 'src', this.placeholder);
    this.renderer.addClass(this.el.nativeElement, 'lazy-loading');

    // Check if IntersectionObserver is supported
    if ('IntersectionObserver' in window) {
      this.createIntersectionObserver();
    } else {
      // Fallback for older browsers
      this.loadImage();
    }
  }

  private createIntersectionObserver(): void {
    const options: IntersectionObserverInit = {
      threshold: this.threshold,
      rootMargin: this.rootMargin
    };

    this.observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting && !this.loaded && !this.error) {
          this.loadImage();
          this.observer?.unobserve(this.el.nativeElement);
        }
      });
    }, options);

    this.observer.observe(this.el.nativeElement);
  }

  private loadImage(): void {
    if (this.loaded || this.error) return;

    const img = new Image();
    
    img.onload = () => {
      this.onImageLoad();
    };

    img.onerror = () => {
      this.onImageError();
    };

    // Start loading the actual image
    img.src = this.src;
  }

  private onImageLoad(): void {
    if (this.loaded) return;

    this.loaded = true;
    this.renderer.setAttribute(this.el.nativeElement, 'src', this.src);
    this.renderer.removeClass(this.el.nativeElement, 'lazy-loading');
    this.renderer.addClass(this.el.nativeElement, 'lazy-loaded');

    // Add fade-in animation
    this.renderer.setStyle(this.el.nativeElement, 'opacity', '0');
    this.renderer.setStyle(this.el.nativeElement, 'transition', 'opacity 0.3s ease-in-out');
    
    setTimeout(() => {
      this.renderer.setStyle(this.el.nativeElement, 'opacity', '1');
    }, 10);
  }

  private onImageError(): void {
    if (this.error) return;

    this.error = true;
    this.renderer.setAttribute(this.el.nativeElement, 'src', this.errorImage);
    this.renderer.removeClass(this.el.nativeElement, 'lazy-loading');
    this.renderer.addClass(this.el.nativeElement, 'lazy-error');
  }
}

/**
 * Responsive image directive for optimal loading
 */
@Directive({
  selector: '[appResponsiveImage]',
  standalone: true
})
export class ResponsiveImageDirective implements OnInit {
  @Input('appResponsiveImage') baseSrc!: string;
  @Input() sizes = '(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 33vw';
  @Input() breakpoints = [320, 640, 768, 1024, 1280, 1920];
  @Input() format = 'webp';
  @Input() fallbackFormat = 'jpg';

  constructor(
    private el: ElementRef<HTMLImageElement>,
    private renderer: Renderer2
  ) {}

  ngOnInit(): void {
    this.setupResponsiveImage();
  }

  private setupResponsiveImage(): void {
    // Generate srcset for different breakpoints
    const srcset = this.generateSrcSet();
    
    // Set srcset attribute
    this.renderer.setAttribute(this.el.nativeElement, 'srcset', srcset);
    this.renderer.setAttribute(this.el.nativeElement, 'sizes', this.sizes);

    // Set fallback src
    const fallbackSrc = this.generateImageUrl(this.baseSrc, 800, this.fallbackFormat);
    this.renderer.setAttribute(this.el.nativeElement, 'src', fallbackSrc);

    // Add loading attribute for native lazy loading
    this.renderer.setAttribute(this.el.nativeElement, 'loading', 'lazy');
  }

  private generateSrcSet(): string {
    return this.breakpoints
      .map(width => {
        const url = this.generateImageUrl(this.baseSrc, width, this.format);
        return `${url} ${width}w`;
      })
      .join(', ');
  }

  private generateImageUrl(baseSrc: string, width: number, format: string): string {
    // This would typically integrate with your image optimization service
    // For now, we'll assume a URL pattern
    const extension = baseSrc.split('.').pop();
    const baseUrl = baseSrc.replace(`.${extension}`, '');
    return `${baseUrl}_${width}.${format}`;
  }
}

/**
 * Image optimization service
 */
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ImageOptimizationService {
  private readonly SUPPORTED_FORMATS = ['avif', 'webp', 'jpg', 'png'];
  private readonly DEFAULT_QUALITY = 85;

  /**
   * Get the best supported image format for the browser
   */
  getBestFormat(): string {
    // Check for AVIF support
    if (this.supportsFormat('avif')) {
      return 'avif';
    }
    
    // Check for WebP support
    if (this.supportsFormat('webp')) {
      return 'webp';
    }
    
    // Fallback to JPEG
    return 'jpg';
  }

  /**
   * Check if browser supports a specific image format
   */
  private supportsFormat(format: string): boolean {
    const canvas = document.createElement('canvas');
    canvas.width = 1;
    canvas.height = 1;
    
    try {
      const dataUrl = canvas.toDataURL(`image/${format}`);
      return dataUrl.indexOf(`data:image/${format}`) === 0;
    } catch {
      return false;
    }
  }

  /**
   * Generate optimized image URL
   */
  generateOptimizedUrl(
    originalUrl: string,
    width?: number,
    height?: number,
    quality = this.DEFAULT_QUALITY,
    format?: string
  ): string {
    const params = new URLSearchParams();
    
    if (width) params.set('w', width.toString());
    if (height) params.set('h', height.toString());
    if (quality !== this.DEFAULT_QUALITY) params.set('q', quality.toString());
    if (format) params.set('f', format);
    
    const separator = originalUrl.includes('?') ? '&' : '?';
    return `${originalUrl}${separator}${params.toString()}`;
  }

  /**
   * Preload critical images
   */
  preloadImages(urls: string[]): void {
    urls.forEach(url => {
      const link = document.createElement('link');
      link.rel = 'preload';
      link.as = 'image';
      link.href = url;
      document.head.appendChild(link);
    });
  }

  /**
   * Create blur placeholder for images
   */
  createBlurPlaceholder(width: number, height: number, color = '#f0f0f0'): string {
    const canvas = document.createElement('canvas');
    canvas.width = width;
    canvas.height = height;
    
    const ctx = canvas.getContext('2d');
    if (ctx) {
      ctx.fillStyle = color;
      ctx.fillRect(0, 0, width, height);
    }
    
    return canvas.toDataURL();
  }
}