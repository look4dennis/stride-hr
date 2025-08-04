import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { AdminSettingsComponent } from './admin-settings.component';

describe('AdminSettingsComponent', () => {
  let component: AdminSettingsComponent;
  let fixture: ComponentFixture<AdminSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminSettingsComponent, RouterTestingModule]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display page header', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Administrative Settings');
  });

  it('should display system stats', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const statValues = compiled.querySelectorAll('.stat-value');
    
    expect(statValues.length).toBe(4);
    expect(statValues[0].textContent).toContain('1'); // Organizations
    expect(statValues[1].textContent).toContain('3'); // Branches
    expect(statValues[2].textContent).toContain('8'); // Roles
    expect(statValues[3].textContent).toContain('125'); // Users
  });

  it('should display all settings cards', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const settingsCards = compiled.querySelectorAll('.settings-card');
    
    expect(settingsCards.length).toBe(6);
    
    // Check if all expected cards are present
    const cardTitles = Array.from(settingsCards).map(card => 
      card.querySelector('h3')?.textContent?.trim()
    );
    
    expect(cardTitles).toContain('Organization Settings');
    expect(cardTitles).toContain('Branch Management');
    expect(cardTitles).toContain('Roles & Permissions');
    expect(cardTitles).toContain('System Configuration');
    expect(cardTitles).toContain('Security Settings');
    expect(cardTitles).toContain('Integrations');
  });

  it('should have correct router links', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const routerLinks = compiled.querySelectorAll('[routerLink]');
    
    const expectedLinks = [
      '/settings/organization',
      '/settings/branches',
      '/settings/roles',
      '/settings/system',
      '/settings/security',
      '/settings/integrations'
    ];
    
    expectedLinks.forEach(link => {
      const linkElement = Array.from(routerLinks).find(el => 
        el.getAttribute('routerLink') === link
      );
      expect(linkElement).toBeTruthy();
    });
  });

  it('should initialize system stats', () => {
    expect(component.systemStats).toBeDefined();
    expect(component.systemStats.totalOrganizations).toBe(1);
    expect(component.systemStats.totalBranches).toBe(3);
    expect(component.systemStats.totalRoles).toBe(8);
    expect(component.systemStats.totalUsers).toBe(125);
  });

  it('should call loadSystemStats on init', () => {
    spyOn(component as any, 'loadSystemStats');
    component.ngOnInit();
    expect((component as any).loadSystemStats).toHaveBeenCalled();
  });
});