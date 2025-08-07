import {
  Directive,
  ElementRef,
  Input,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
  Renderer2
} from '@angular/core';

@Directive({
  selector: '[appLazyImage]'
})
export class LazyImageDirective implements OnInit, OnDestroy {
  @Input() appLazyImage!: string; // Image source URL
  @Input() placeholder: string = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjAwIiBoZWlnaHQ9IjIwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBmaWxsPSIjZGRkIi8+PHRleHQgeD0iNTAlIiB5PSI1MCUiIGZvbnQtZmFtaWx5PSJBcmlhbCwgc2Fucy1zZXJpZiIgZm9udC1zaXplPSIxNCIgZmlsbD0iIzk5OSIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZHk9Ii4zZW0iPkxvYWRpbmcuLi48L3RleHQ+PC9zdmc+';
  @Input() errorImage: string = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjAwIiBoZWlnaHQ9IjIwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBmaWxsPSIjZjVmNWY1Ii8+PHRleHQgeD0iNTAlIiB5PSI1MCUiIGZvbnQtZmFtaWx5PSJBcmlhbCwgc2Fucy1zZXJpZiIgZm9udC1zaXplPSIxNCIgZmlsbD0iIzk5OSIgdGV4dC1hbmNob3I9Im1pZGRsZSIgZHk9Ii4zZW0iPkVycm9yPC90ZXh0Pjwvc3ZnPg==';
  @Input() rootMargin: string = '50px';
  @Input() threshold: number = 0.1;

  @Output() imageLoad = new EventEmitter<Event>();
  @Output() imageError = new EventEmitter<Event>();

  private observer!: IntersectionObserver;
  private isLoaded = false;

  constructor(
    private elementRef: ElementRef<HTMLImageElement>,
    private renderer: Renderer2
  ) { }

  ngOnInit(): void {
    this.setPlaceholder();
    this.createObserver();
    this.startObserving();
  }

  ngOnDestroy(): void {
    if (this.observer) {
      this.observer.disconnect();
    }
  }

  private setPlaceholder(): void {
    const img = this.elementRef.nativeElement;
    this.renderer.setAttribute(img, 'src', this.placeholder);
    this.renderer.addClass(img, 'lazy-loading');
  }

  private createObserver(): void {
    const options: IntersectionObserverInit = {
      rootMargin: this.rootMargin,
      threshold: this.threshold
    };

    this.observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting && !this.isLoaded) {
          this.loadImage();
        }
      });
    }, options);
  }

  private startObserving(): void {
    this.observer.observe(this.elementRef.nativeElement);
  }

  private loadImage(): void {
    if (this.isLoaded || !this.appLazyImage) {
      return;
    }

    const img = this.elementRef.nativeElement;
    const imageLoader = new Image();

    // Add loading class
    this.renderer.addClass(img, 'lazy-loading');

    imageLoader.onload = (event: Event) => {
      this.onImageLoad(event);
    };

    imageLoader.onerror = (event: Event | string) => {
      // Create a proper Event object if we receive a string
      const errorEvent = typeof event === 'string' 
        ? new ErrorEvent('error', { message: event })
        : event;
      this.onImageError(errorEvent);
    };

    // Start loading the actual image
    imageLoader.src = this.appLazyImage;
  }

  private onImageLoad(event: Event): void {
    const img = this.elementRef.nativeElement;

    // Update the src attribute
    this.renderer.setAttribute(img, 'src', this.appLazyImage);

    // Update classes
    this.renderer.removeClass(img, 'lazy-loading');
    this.renderer.addClass(img, 'lazy-loaded');

    // Add fade-in animation
    this.renderer.setStyle(img, 'opacity', '0');
    this.renderer.setStyle(img, 'transition', 'opacity 0.3s ease-in-out');

    // Trigger fade-in
    setTimeout(() => {
      this.renderer.setStyle(img, 'opacity', '1');
    }, 10);

    this.isLoaded = true;
    this.observer.unobserve(img);
    this.imageLoad.emit(event);
  }

  private onImageError(event: Event): void {
    const img = this.elementRef.nativeElement;

    // Set error image
    this.renderer.setAttribute(img, 'src', this.errorImage);

    // Update classes
    this.renderer.removeClass(img, 'lazy-loading');
    this.renderer.addClass(img, 'lazy-error');

    this.observer.unobserve(img);
    this.imageError.emit(event);
  }

  // Public method to manually trigger loading
  public load(): void {
    if (!this.isLoaded) {
      this.loadImage();
    }
  }

  // Public method to reload image
  public reload(): void {
    this.isLoaded = false;
    this.setPlaceholder();
    this.loadImage();
  }
}