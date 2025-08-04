import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { FormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { of, throwError } from 'rxjs';

import { ProjectMonitoringComponent } from './project-monitoring.component';
import { ProjectService } from '../../../services/project.service';
import { ProjectDashboard, ProjectAnalytics, ProjectHoursReport } from '../../../models/project.models';

describe('ProjectMonitoringComponent', () => {
  let component: ProjectMonitoringComponent;
  let fixture: ComponentFixture<ProjectMonitoringComponent>;
  let mockProjectService: jasmine.SpyObj<ProjectService>;

  const mockDashboard: ProjectDashboard = {
    teamLeaderId: 1,
    teamLeaderName: 'John Doe',
    projectAnalytics: [
      {
        projectId: 1,
        projectName: 'Test Project',
        metrics: {
          totalHoursWorked: 80,
          estimatedHours: 100,
          hoursVariance: -20,
          budgetUtilized: 8000,
          budgetVariance: -2000,
          completionPercentage: 75,
          totalTasks: 10,
          completedTasks: 7,
          overdueTasks: 1,
          teamMembersCount: 3,
          averageTaskCompletionTime: 2.5
        },
        trends: {
          dailyProgress: [],
          weeklyHours: [],
          teamProductivity: [],
          taskStatusTrends: []
        },
        performance: {
          overallEfficiency: 85,
          qualityScore: 90,
          timelineAdherence: 80,
          budgetAdherence: 120,
          teamSatisfaction: 85,
          performanceGrade: 'A',
          strengthAreas: ['High task completion rate'],
          improvementAreas: ['Time estimation']
        },
        risks: [],
        generatedAt: new Date()
      }
    ],
    teamOverview: {
      totalProjects: 3,
      activeProjects: 2,
      completedProjects: 1,
      delayedProjects: 1,
      totalBudget: 30000,
      budgetUtilized: 25000,
      totalTeamMembers: 8,
      overallProductivity: 85,
      averageProjectHealth: 7.5
    },
    criticalAlerts: [
      {
        id: 1,
        projectId: 1,
        alertType: 'Budget Alert',
        message: 'Project is over budget',
        severity: 'Critical',
        createdAt: new Date(),
        isResolved: false
      }
    ],
    highRisks: [
      {
        id: 1,
        projectId: 1,
        riskType: 'Technical Risk',
        description: 'Technology may become obsolete',
        severity: 'High',
        probability: 0.3,
        impact: 0.8,
        mitigationPlan: 'Monitor technology trends',
        status: 'Active',
        identifiedAt: new Date()
      }
    ]
  };

  const mockHoursReports: ProjectHoursReport[] = [
    {
      projectId: 1,
      projectName: 'Test Project',
      estimatedHours: 100,
      actualHours: 80,
      remainingHours: 20,
      completionPercentage: 75,
      isOverBudget: false,
      teamMembers: []
    }
  ];

  beforeEach(async () => {
    const projectServiceSpy = jasmine.createSpyObj('ProjectService', [
      'getTeamLeaderDashboard',
      'getTeamHoursTracking'
    ]);

    await TestBed.configureTestingModule({
      imports: [
        ProjectMonitoringComponent,
        HttpClientTestingModule,
        FormsModule,
        NgbModule
      ],
      providers: [
        { provide: ProjectService, useValue: projectServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectMonitoringComponent);
    component = fixture.componentInstance;
    mockProjectService = TestBed.inject(ProjectService) as jasmine.SpyObj<ProjectService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load dashboard data on init', async () => {
    // Arrange
    mockProjectService.getTeamLeaderDashboard.and.returnValue(of(mockDashboard));
    mockProjectService.getTeamHoursTracking.and.returnValue(of(mockHoursReports));

    // Act
    component.ngOnInit();
    await fixture.whenStable();

    // Assert
    expect(component.dashboard).toEqual(mockDashboard);
    expect(component.hoursReports).toEqual(mockHoursReports);
    expect(component.isLoading).toBeFalse();
    expect(component.error).toBeNull();
  });

  it('should handle error when loading dashboard data', async () => {
    // Arrange
    const errorMessage = 'Failed to load dashboard data';
    mockProjectService.getTeamLeaderDashboard.and.returnValue(throwError({ message: errorMessage }));

    // Act
    component.ngOnInit();
    await fixture.whenStable();

    // Assert
    expect(component.dashboard).toBeNull();
    expect(component.isLoading).toBeFalse();
    expect(component.error).toBe(errorMessage);
  });

  it('should refresh data when refreshData is called', async () => {
    // Arrange
    mockProjectService.getTeamLeaderDashboard.and.returnValue(of(mockDashboard));
    mockProjectService.getTeamHoursTracking.and.returnValue(of(mockHoursReports));

    // Act
    component.refreshData();
    await fixture.whenStable();

    // Assert
    expect(mockProjectService.getTeamLeaderDashboard).toHaveBeenCalled();
    expect(mockProjectService.getTeamHoursTracking).toHaveBeenCalled();
    expect(component.dashboard).toEqual(mockDashboard);
    expect(component.hoursReports).toEqual(mockHoursReports);
  });

  it('should set date range correctly for today', () => {
    // Arrange
    mockProjectService.getTeamHoursTracking.and.returnValue(of(mockHoursReports));
    spyOn(component, 'loadHoursTrackingWithDateRange' as any);

    // Act
    component.setDateRange('today');

    // Assert
    expect((component as any).loadHoursTrackingWithDateRange).toHaveBeenCalled();
  });

  it('should set date range correctly for week', () => {
    // Arrange
    mockProjectService.getTeamHoursTracking.and.returnValue(of(mockHoursReports));
    spyOn(component, 'loadHoursTrackingWithDateRange' as any);

    // Act
    component.setDateRange('week');

    // Assert
    expect((component as any).loadHoursTrackingWithDateRange).toHaveBeenCalled();
  });

  it('should set date range correctly for month', () => {
    // Arrange
    mockProjectService.getTeamHoursTracking.and.returnValue(of(mockHoursReports));
    spyOn(component, 'loadHoursTrackingWithDateRange' as any);

    // Act
    component.setDateRange('month');

    // Assert
    expect((component as any).loadHoursTrackingWithDateRange).toHaveBeenCalled();
  });

  it('should set date range correctly for quarter', () => {
    // Arrange
    mockProjectService.getTeamHoursTracking.and.returnValue(of(mockHoursReports));
    spyOn(component, 'loadHoursTrackingWithDateRange' as any);

    // Act
    component.setDateRange('quarter');

    // Assert
    expect((component as any).loadHoursTrackingWithDateRange).toHaveBeenCalled();
  });

  it('should not set date range for invalid range', () => {
    // Arrange
    spyOn(component, 'loadHoursTrackingWithDateRange' as any);

    // Act
    component.setDateRange('invalid');

    // Assert
    expect((component as any).loadHoursTrackingWithDateRange).not.toHaveBeenCalled();
  });

  it('should display loading state', () => {
    // Arrange
    component.isLoading = true;

    // Act
    fixture.detectChanges();

    // Assert
    const loadingElement = fixture.nativeElement.querySelector('.spinner-border');
    expect(loadingElement).toBeTruthy();
  });

  it('should display error state', () => {
    // Arrange
    component.isLoading = false;
    component.error = 'Test error message';

    // Act
    fixture.detectChanges();

    // Assert
    const errorElement = fixture.nativeElement.querySelector('.alert-danger');
    expect(errorElement).toBeTruthy();
    expect(errorElement.textContent).toContain('Test error message');
  });

  it('should display dashboard data when loaded', () => {
    // Arrange
    component.isLoading = false;
    component.dashboard = mockDashboard;
    component.hoursReports = mockHoursReports;

    // Act
    fixture.detectChanges();

    // Assert
    const totalProjectsCard = fixture.nativeElement.querySelector('.card.bg-primary h3');
    expect(totalProjectsCard.textContent.trim()).toBe('3');

    const activeProjectsCard = fixture.nativeElement.querySelector('.card.bg-success h3');
    expect(activeProjectsCard.textContent.trim()).toBe('2');
  });

  it('should display critical alerts when available', () => {
    // Arrange
    component.isLoading = false;
    component.dashboard = mockDashboard;

    // Act
    fixture.detectChanges();

    // Assert
    const alertsSection = fixture.nativeElement.querySelector('.card-header.bg-danger');
    expect(alertsSection).toBeTruthy();
    expect(alertsSection.textContent).toContain('Critical Alerts');
  });

  it('should display high risks when available', () => {
    // Arrange
    component.isLoading = false;
    component.dashboard = mockDashboard;

    // Act
    fixture.detectChanges();

    // Assert
    const risksSection = fixture.nativeElement.querySelector('.card-header.bg-warning');
    expect(risksSection).toBeTruthy();
    expect(risksSection.textContent).toContain('High Risks');
  });

  it('should display project analytics', () => {
    // Arrange
    component.isLoading = false;
    component.dashboard = mockDashboard;

    // Act
    fixture.detectChanges();

    // Assert
    const analyticsSection = fixture.nativeElement.querySelector('.card-header h5');
    expect(analyticsSection.textContent).toContain('Project Analytics');
  });

  it('should display hours tracking summary', () => {
    // Arrange
    component.isLoading = false;
    component.dashboard = mockDashboard;
    component.hoursReports = mockHoursReports;

    // Act
    fixture.detectChanges();

    // Assert
    const hoursSection = fixture.nativeElement.querySelectorAll('.card-header h5')[1];
    expect(hoursSection.textContent).toContain('Hours Tracking Summary');
  });

  it('should display empty state when no data', () => {
    // Arrange
    component.isLoading = false;
    component.dashboard = null;
    component.error = null;

    // Act
    fixture.detectChanges();

    // Assert
    const emptyState = fixture.nativeElement.querySelector('.text-center.py-5');
    expect(emptyState).toBeTruthy();
    expect(emptyState.textContent).toContain('No Monitoring Data Available');
  });

  it('should cleanup on destroy', () => {
    // Arrange
    spyOn(component['destroy$'], 'next');
    spyOn(component['destroy$'], 'complete');

    // Act
    component.ngOnDestroy();

    // Assert
    expect(component['destroy$'].next).toHaveBeenCalled();
    expect(component['destroy$'].complete).toHaveBeenCalled();
  });
});