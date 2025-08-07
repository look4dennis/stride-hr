import { 
  Directive, 
  ElementRef, 
  Input, 
  Output, 
  EventEmitter, 
  OnInit, 
  OnDestroy, 
  TemplateRef, 
  ViewContainerRef,
  ChangeDetectorRef,
  NgZone
} from '@angular/core';
import { fromEvent, Subject } from 'rxjs';
import { throttleTime, takeUntil } from 'rxjs/operators';

export interface VirtualScrollConfig {
  itemHeight: number;
  containerHeight: number;
  bufferSize: number;
  threshold: number;
}

export interface VirtualScrollItem {
  index: number;
  data: any;
}

@Directive({
  selector: '[appVirtualScroll]'
})
export class VirtualScrollDirective implements OnInit, OnDestroy {
  @Input() items: any[] = [];
  @Input() itemHeight: number = 50;
  @Input() containerHeight: number = 400;
  @Input() bufferSize: number = 5;
  @Input() threshold: number = 100;
  @Input() itemTemplate!: TemplateRef<any>;

  @Output() scrollEnd = new EventEmitter<void>();
  @Output() visibleRangeChange = new EventEmitter<{start: number, end: number}>();

  private destroy$ = new Subject<void>();
  private scrollContainer!: HTMLElement;
  private contentContainer!: HTMLElement;
  private visibleItems: VirtualScrollItem[] = [];
  private startIndex = 0;
  private endIndex = 0;
  private totalHeight = 0;

  constructor(
    private elementRef: ElementRef,
    private viewContainer: ViewContainerRef,
    private cdr: ChangeDetectorRef,
    private ngZone: NgZone
  ) {}

  ngOnInit(): void {
    this.setupScrollContainer();
    this.setupScrollListener();
    this.calculateVisibleItems();
    this.renderItems();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupScrollContainer(): void {
    const element = this.elementRef.nativeElement;
    
    // Create scroll container
    this.scrollContainer = document.createElement('div');
    this.scrollContainer.style.height = `${this.containerHeight}px`;
    this.scrollContainer.style.overflowY = 'auto';
    this.scrollContainer.style.position = 'relative';
    
    // Create content container
    this.contentContainer = document.createElement('div');
    this.contentContainer.style.position = 'relative';
    
    // Calculate total height
    this.totalHeight = this.items.length * this.itemHeight;
    this.contentContainer.style.height = `${this.totalHeight}px`;
    
    this.scrollContainer.appendChild(this.contentContainer);
    element.parentNode?.insertBefore(this.scrollContainer, element);
    element.style.display = 'none';
  }

  private setupScrollListener(): void {
    this.ngZone.runOutsideAngular(() => {
      fromEvent(this.scrollContainer, 'scroll')
        .pipe(
          throttleTime(16), // ~60fps
          takeUntil(this.destroy$)
        )
        .subscribe(() => {
          this.ngZone.run(() => {
            this.onScroll();
          });
        });
    });
  }

  private onScroll(): void {
    const scrollTop = this.scrollContainer.scrollTop;
    const scrollHeight = this.scrollContainer.scrollHeight;
    const clientHeight = this.scrollContainer.clientHeight;

    // Check if near bottom for infinite scroll
    if (scrollHeight - scrollTop - clientHeight < this.threshold) {
      this.scrollEnd.emit();
    }

    this.calculateVisibleItems();
    this.renderItems();
  }

  private calculateVisibleItems(): void {
    const scrollTop = this.scrollContainer.scrollTop;
    const visibleStart = Math.floor(scrollTop / this.itemHeight);
    const visibleEnd = Math.min(
      visibleStart + Math.ceil(this.containerHeight / this.itemHeight),
      this.items.length - 1
    );

    // Add buffer
    this.startIndex = Math.max(0, visibleStart - this.bufferSize);
    this.endIndex = Math.min(this.items.length - 1, visibleEnd + this.bufferSize);

    this.visibleItems = [];
    for (let i = this.startIndex; i <= this.endIndex; i++) {
      this.visibleItems.push({
        index: i,
        data: this.items[i]
      });
    }

    this.visibleRangeChange.emit({
      start: this.startIndex,
      end: this.endIndex
    });
  }

  private renderItems(): void {
    // Clear existing views
    this.viewContainer.clear();

    // Create spacer for items before visible range
    if (this.startIndex > 0) {
      const topSpacer = document.createElement('div');
      topSpacer.style.height = `${this.startIndex * this.itemHeight}px`;
      this.contentContainer.appendChild(topSpacer);
    }

    // Render visible items
    this.visibleItems.forEach(item => {
      const view = this.viewContainer.createEmbeddedView(this.itemTemplate, {
        $implicit: item.data,
        index: item.index
      });
      
      const element = view.rootNodes[0] as HTMLElement;
      if (element) {
        element.style.position = 'absolute';
        element.style.top = `${item.index * this.itemHeight}px`;
        element.style.height = `${this.itemHeight}px`;
        element.style.width = '100%';
        this.contentContainer.appendChild(element);
      }
    });

    // Create spacer for items after visible range
    if (this.endIndex < this.items.length - 1) {
      const bottomSpacer = document.createElement('div');
      const remainingItems = this.items.length - 1 - this.endIndex;
      bottomSpacer.style.height = `${remainingItems * this.itemHeight}px`;
      this.contentContainer.appendChild(bottomSpacer);
    }

    this.cdr.detectChanges();
  }

  // Public methods for external control
  scrollToIndex(index: number): void {
    const scrollTop = index * this.itemHeight;
    this.scrollContainer.scrollTop = scrollTop;
  }

  scrollToTop(): void {
    this.scrollContainer.scrollTop = 0;
  }

  scrollToBottom(): void {
    this.scrollContainer.scrollTop = this.scrollContainer.scrollHeight;
  }

  refresh(): void {
    this.totalHeight = this.items.length * this.itemHeight;
    this.contentContainer.style.height = `${this.totalHeight}px`;
    this.calculateVisibleItems();
    this.renderItems();
  }
}