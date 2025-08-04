import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, BehaviorSubject } from 'rxjs';
import { PwaInstallPromptComponent } from './pwa-install-prompt.component';
import { PwaService } from '../../../services/pwa.service';

describe('PwaInstallPromptComponent', () => {
  let component: PwaInstallPromptComponent;
  let fixture: ComponentFixture<PwaInstallPromptComponent>;
  let mockPwaService: jasmine.SpyObj<PwaService>;
  let canInstallSubject: BehaviorSubject<boolean>;
  let updateAvailableSubject: BehaviorSubject<boolean>;
  let isOnlineSubject: BehaviorSubject<boolean>;

  beforeEach(async () => {
    canInstallSubject = new BehaviorSubject<boolean>(false);
    updateAvailableSubject = new BehaviorSubject<boolean>(false);
    isOnlineSubject = new BehaviorSubject<boolean>(true);

    const pwaServiceSpy = jasmine.createSpyObj('PwaService', [
      'isStandalone',
      'promptInstall',
      'applyUpdate'
    ], {
      canInstall$: canInstallSubject.asObservable(),
      updateAvailable$: updateAvailableSubject.asObservable(),
      isOnline$: isOnlineSubject.asObservable()
    });

    await TestBed.configureTestingModule({
      imports: [PwaInstallPromptComponent],
      providers: [
        { provide: PwaService, useValue: pwaServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PwaInstallPromptComponent);
    component = fixture.componentInstance;
    mockPwaService = TestBed.inject(PwaService) as jasmine.SpyObj<PwaService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with correct default values', () => {
    mockPwaService.isStandalone.and.returnValue(false);
    
    fixture.detectChanges();
    
    expect(component.showInstallPrompt).toBe(false);
    expect(component.updateAvailable).toBe(false);
    expect(component.isOnline).toBe(true);
    expect(component.installing).toBe(false);
    expect(component.updating).toBe(false);
  });

  it('should show install prompt when can install and not standalone', () => {
    mockPwaService.isStandalone.and.returnValue(false);
    
    fixture.detectChanges();
    
    canInstallSubject.next(true);
    
    expect(component.showInstallPrompt).toBe(true);
  });

  it('should not show install prompt when standalone', () => {
    mockPwaService.isStandalone.and.returnValue(true);
    
    fixture.detectChanges();
    
    canInstallSubject.next(true);
    
    expect(component.showInstallPrompt).toBe(false);
  });

  it('should show update banner when update is available', () => {
    fixture.detectChanges();
    
    updateAvailableSubject.next(true);
    
    expect(component.updateAvailable).toBe(true);
  });

  it('should show offline status when offline', () => {
    fixture.detectChanges();
    
    isOnlineSubject.next(false);
    
    expect(component.isOnline).toBe(false);
  });

  it('should call promptInstall when install button is clicked', async () => {
    mockPwaService.promptInstall.and.returnValue(Promise.resolve(true));
    mockPwaService.isStandalone.and.returnValue(false);
    
    fixture.detectChanges();
    canInstallSubject.next(true);
    fixture.detectChanges();
    
    await component.installApp();
    
    expect(mockPwaService.promptInstall).toHaveBeenCalled();
    expect(component.showInstallPrompt).toBe(false);
  });

  it('should handle install failure gracefully', async () => {
    mockPwaService.promptInstall.and.returnValue(Promise.resolve(false));
    mockPwaService.isStandalone.and.returnValue(false);
    
    fixture.detectChanges();
    canInstallSubject.next(true);
    fixture.detectChanges();
    
    await component.installApp();
    
    expect(mockPwaService.promptInstall).toHaveBeenCalled();
    expect(component.showInstallPrompt).toBe(true); // Should still show if install failed
  });

  it('should set installing state during install', async () => {
    let resolvePromise: (value: boolean) => void;
    const installPromise = new Promise<boolean>(resolve => {
      resolvePromise = resolve;
    });
    
    mockPwaService.promptInstall.and.returnValue(installPromise);
    mockPwaService.isStandalone.and.returnValue(false);
    
    fixture.detectChanges();
    canInstallSubject.next(true);
    fixture.detectChanges();
    
    const installPromiseResult = component.installApp();
    
    expect(component.installing).toBe(true);
    
    resolvePromise!(true);
    await installPromiseResult;
    
    expect(component.installing).toBe(false);
  });

  it('should dismiss install prompt', () => {
    mockPwaService.isStandalone.and.returnValue(false);
    
    fixture.detectChanges();
    canInstallSubject.next(true);
    fixture.detectChanges();
    
    component.dismissPrompt();
    
    expect(component.showInstallPrompt).toBe(false);
    expect(localStorage.getItem('pwa-install-dismissed')).toBeTruthy();
  });

  it('should apply update when update button is clicked', async () => {
    mockPwaService.applyUpdate.and.returnValue(Promise.resolve());
    
    fixture.detectChanges();
    updateAvailableSubject.next(true);
    fixture.detectChanges();
    
    await component.applyUpdate();
    
    expect(mockPwaService.applyUpdate).toHaveBeenCalled();
  });

  it('should set updating state during update', async () => {
    let resolvePromise: () => void;
    const updatePromise = new Promise<void>(resolve => {
      resolvePromise = resolve;
    });
    
    mockPwaService.applyUpdate.and.returnValue(updatePromise);
    
    fixture.detectChanges();
    updateAvailableSubject.next(true);
    fixture.detectChanges();
    
    const updatePromiseResult = component.applyUpdate();
    
    expect(component.updating).toBe(true);
    
    resolvePromise!();
    await updatePromiseResult;
    
    expect(component.updating).toBe(false);
  });

  it('should dismiss update banner', () => {
    fixture.detectChanges();
    updateAvailableSubject.next(true);
    fixture.detectChanges();
    
    component.dismissUpdate();
    
    expect(component.updateAvailable).toBe(false);
  });

  it('should render install banner when showInstallPrompt is true', () => {
    mockPwaService.isStandalone.and.returnValue(false);
    
    fixture.detectChanges();
    canInstallSubject.next(true);
    fixture.detectChanges();
    
    const installBanner = fixture.nativeElement.querySelector('.pwa-install-banner');
    expect(installBanner).toBeTruthy();
    
    const installButton = fixture.nativeElement.querySelector('.pwa-install-banner .btn-primary');
    expect(installButton).toBeTruthy();
    expect(installButton.textContent.trim()).toContain('Install');
  });

  it('should render update banner when updateAvailable is true', () => {
    fixture.detectChanges();
    updateAvailableSubject.next(true);
    fixture.detectChanges();
    
    const updateBanner = fixture.nativeElement.querySelector('.pwa-update-banner');
    expect(updateBanner).toBeTruthy();
    
    const updateButton = fixture.nativeElement.querySelector('.pwa-update-banner .btn-info');
    expect(updateButton).toBeTruthy();
    expect(updateButton.textContent.trim()).toContain('Update');
  });

  it('should render offline status when offline', () => {
    fixture.detectChanges();
    isOnlineSubject.next(false);
    fixture.detectChanges();
    
    const offlineStatus = fixture.nativeElement.querySelector('.offline-status');
    expect(offlineStatus).toBeTruthy();
    expect(offlineStatus.textContent).toContain('You\'re offline');
  });

  it('should not render banners when conditions are not met', () => {
    mockPwaService.isStandalone.and.returnValue(false);
    
    fixture.detectChanges();
    
    const installBanner = fixture.nativeElement.querySelector('.pwa-install-banner');
    const updateBanner = fixture.nativeElement.querySelector('.pwa-update-banner');
    const offlineStatus = fixture.nativeElement.querySelector('.offline-status');
    
    expect(installBanner).toBeFalsy();
    expect(updateBanner).toBeFalsy();
    expect(offlineStatus).toBeFalsy();
  });

  afterEach(() => {
    localStorage.clear();
  });
});