import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProjectCollaborationComponent } from './project-collaboration.component';
import { ProjectService } from '../../../services/project.service';
import { of } from 'rxjs';
import { ProjectCollaboration } from '../../../models/project.models';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

// Mock SignalR
const mockHubConnection = {
  start: jasmine.createSpy('start').and.returnValue(Promise.resolve()),
  stop: jasmine.createSpy('stop').and.returnValue(Promise.resolve()),
  invoke: jasmine.createSpy('invoke').and.returnValue(Promise.resolve()),
  on: jasmine.createSpy('on'),
  off: jasmine.createSpy('off')
};

const mockSignalR = {
  HubConnectionBuilder: jasmine.createSpy('HubConnectionBuilder').and.returnValue({
    withUrl: jasmine.createSpy('withUrl').and.returnValue({
      build: jasmine.createSpy('build').and.returnValue(mockHubConnection)
    })
  })
};

// Mock the SignalR module
(window as any)['signalR'] = mockSignalR;

describe('ProjectCollaborationComponent', () => {
  let component: ProjectCollaborationComponent;
  let fixture: ComponentFixture<ProjectCollaborationComponent>;
  let projectService: jasmine.SpyObj<ProjectService>;

  const mockCollaboration: ProjectCollaboration = {
    projectId: 1,
    projectName: 'Test Project',
    comments: [],
    activities: [],
    teamMembers: [],
    communicationStats: {
      totalComments: 0,
      totalActivities: 0,
      activeTeamMembers: 0,
      lastActivity: new Date(),
      memberActivities: []
    }
  };

  beforeEach(async () => {
    const projectServiceSpy = jasmine.createSpyObj('ProjectService', [
      'getProjectCollaboration',
      'addProjectComment',
      'addCommentReply'
    ]);

    await TestBed.configureTestingModule({
      imports: [ProjectCollaborationComponent],
      providers: [
        { provide: ProjectService, useValue: projectServiceSpy },
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectCollaborationComponent);
    component = fixture.componentInstance;
    projectService = TestBed.inject(ProjectService) as jasmine.SpyObj<ProjectService>;

    // Setup default spy returns
    projectService.getProjectCollaboration.and.returnValue(of(mockCollaboration));
    projectService.addProjectComment.and.returnValue(of({} as any));
    projectService.addCommentReply.and.returnValue(of({} as any));

    // Set required input
    component.projectId = 1;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load collaboration data on init', () => {
    component.ngOnInit();
    expect(projectService.getProjectCollaboration).toHaveBeenCalledWith(1);
  });

  it('should setup SignalR connection on init', async () => {
    spyOn(component as any, 'setupSignalRConnection').and.returnValue(Promise.resolve());

    component.ngOnInit();

    expect((component as any).setupSignalRConnection).toHaveBeenCalled();
  });

  it('should stop SignalR connection on destroy', () => {
    (component as any).hubConnection = mockHubConnection;

    component.ngOnDestroy();

    expect(mockHubConnection.stop).toHaveBeenCalled();
  });

  it('should handle SignalR connection errors gracefully', async () => {
    const consoleSpy = spyOn(console, 'error');
    mockHubConnection.start.and.returnValue(Promise.reject(new Error('Connection failed')));

    await (component as any).setupSignalRConnection();

    expect(consoleSpy).toHaveBeenCalledWith('Error setting up SignalR connection:', jasmine.any(Error));
  });

  it('should add comment successfully', () => {
    component.newComment = {
      projectId: 1,
      comment: 'Test comment'
    };

    component.addComment();

    expect(projectService.addProjectComment).toHaveBeenCalledWith(component.newComment);
  });

  it('should not add empty comment', () => {
    component.newComment = {
      projectId: 1,
      comment: ''
    };

    component.addComment();

    expect(projectService.addProjectComment).not.toHaveBeenCalled();
  });

  it('should reply to comment successfully', () => {
    component.startReply(1);
    component.newReply.reply = 'Test reply';

    component.addReply(1);

    expect(projectService.addCommentReply).toHaveBeenCalledWith(component.newReply);
  });
});