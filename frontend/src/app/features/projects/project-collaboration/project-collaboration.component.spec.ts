import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { FormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { of, throwError } from 'rxjs';

import { ProjectCollaborationComponent } from './project-collaboration.component';
import { ProjectService } from '../../../services/project.service';
import { ProjectCollaboration, ProjectComment, ProjectActivity, CreateProjectComment, CreateCommentReply } from '../../../models/project.models';

describe('ProjectCollaborationComponent', () => {
  let component: ProjectCollaborationComponent;
  let fixture: ComponentFixture<ProjectCollaborationComponent>;
  let mockProjectService: jasmine.SpyObj<ProjectService>;

  const mockCollaboration: ProjectCollaboration = {
    projectId: 1,
    projectName: 'Test Project',
    comments: [
      {
        id: 1,
        projectId: 1,
        employeeId: 123,
        employeeName: 'John Doe',
        employeePhoto: '/assets/images/john.jpg',
        comment: 'This is a test comment',
        createdAt: new Date(),
        replies: [
          {
            id: 1,
            commentId: 1,
            employeeId: 456,
            employeeName: 'Jane Smith',
            employeePhoto: '/assets/images/jane.jpg',
            reply: 'This is a test reply',
            createdAt: new Date()
          }
        ]
      }
    ],
    activities: [
      {
        id: 1,
        projectId: 1,
        employeeId: 123,
        employeeName: 'John Doe',
        activityType: 'Comment',
        description: 'Added comment to project',
        details: 'Comment details',
        createdAt: new Date()
      }
    ],
    teamMembers: [
      {
        employeeId: 123,
        employeeName: 'John Doe',
        employeePhoto: '/assets/images/john.jpg',
        role: 'Developer',
        joinedAt: new Date()
      },
      {
        employeeId: 456,
        employeeName: 'Jane Smith',
        employeePhoto: '/assets/images/jane.jpg',
        role: 'Designer',
        joinedAt: new Date()
      }
    ],
    communicationStats: {
      totalComments: 1,
      totalActivities: 1,
      activeTeamMembers: 2,
      lastActivity: new Date(),
      memberActivities: [
        {
          employeeId: 123,
          employeeName: 'John Doe',
          commentsCount: 1,
          activitiesCount: 1,
          lastActivity: new Date()
        },
        {
          employeeId: 456,
          employeeName: 'Jane Smith',
          commentsCount: 0,
          activitiesCount: 0,
          lastActivity: new Date(Date.now() - 8 * 24 * 60 * 60 * 1000) // 8 days ago
        }
      ]
    }
  };

  beforeEach(async () => {
    const projectServiceSpy = jasmine.createSpyObj('ProjectService', [
      'getProjectCollaboration',
      'addProjectComment',
      'addCommentReply',
      'deleteComment',
      'deleteCommentReply'
    ]);

    await TestBed.configureTestingModule({
      imports: [
        ProjectCollaborationComponent,
        HttpClientTestingModule,
        FormsModule,
        NgbModule
      ],
      providers: [
        { provide: ProjectService, useValue: projectServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectCollaborationComponent);
    component = fixture.componentInstance;
    component.projectId = 1;
    mockProjectService = TestBed.inject(ProjectService) as jasmine.SpyObj<ProjectService>;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load collaboration data on init', async () => {
    // Arrange
    mockProjectService.getProjectCollaboration.and.returnValue(of(mockCollaboration));

    // Act
    component.ngOnInit();
    await fixture.whenStable();

    // Assert
    expect(component.collaboration).toEqual(mockCollaboration);
    expect(component.isLoading).toBeFalse();
    expect(component.error).toBeNull();
    expect(mockProjectService.getProjectCollaboration).toHaveBeenCalledWith(1);
  });

  it('should handle error when loading collaboration data', async () => {
    // Arrange
    const errorMessage = 'Failed to load collaboration data';
    mockProjectService.getProjectCollaboration.and.returnValue(throwError({ message: errorMessage }));

    // Act
    component.ngOnInit();
    await fixture.whenStable();

    // Assert
    expect(component.collaboration).toBeNull();
    expect(component.isLoading).toBeFalse();
    expect(component.error).toBe(errorMessage);
  });

  it('should set active tab correctly', () => {
    // Act
    component.setActiveTab('activities');

    // Assert
    expect(component.activeTab).toBe('activities');
  });

  it('should add comment successfully', async () => {
    // Arrange
    component.collaboration = mockCollaboration;
    component.newComment = {
      projectId: 1,
      comment: 'New test comment'
    };

    const newComment: ProjectComment = {
      id: 2,
      projectId: 1,
      employeeId: 123,
      employeeName: 'John Doe',
      employeePhoto: '/assets/images/john.jpg',
      comment: 'New test comment',
      createdAt: new Date(),
      replies: []
    };

    mockProjectService.addProjectComment.and.returnValue(of(newComment));

    // Act
    await component.addComment();

    // Assert
    expect(mockProjectService.addProjectComment).toHaveBeenCalledWith(component.newComment);
    expect(component.collaboration.comments.length).toBe(2);
    expect(component.collaboration.comments[0]).toEqual(newComment);
    expect(component.collaboration.communicationStats.totalComments).toBe(2);
    expect(component.newComment.comment).toBe('');
    expect(component.isSubmittingComment).toBeFalse();
  });

  it('should handle error when adding comment', async () => {
    // Arrange
    component.newComment = {
      projectId: 1,
      comment: 'New test comment'
    };

    const errorMessage = 'Failed to add comment';
    mockProjectService.addProjectComment.and.returnValue(throwError({ message: errorMessage }));

    // Act
    await component.addComment();

    // Assert
    expect(component.error).toBe(errorMessage);
    expect(component.isSubmittingComment).toBeFalse();
  });

  it('should not add empty comment', async () => {
    // Arrange
    component.newComment = {
      projectId: 1,
      comment: '   '
    };

    // Act
    await component.addComment();

    // Assert
    expect(mockProjectService.addProjectComment).not.toHaveBeenCalled();
  });

  it('should start reply correctly', () => {
    // Act
    component.startReply(1);

    // Assert
    expect(component.replyingToComment).toBe(1);
    expect(component.newReply.commentId).toBe(1);
    expect(component.newReply.reply).toBe('');
  });

  it('should cancel reply correctly', () => {
    // Arrange
    component.replyingToComment = 1;
    component.newReply = { commentId: 1, reply: 'test reply' };

    // Act
    component.cancelReply();

    // Assert
    expect(component.replyingToComment).toBeNull();
    expect(component.newReply).toEqual({ commentId: 0, reply: '' });
  });

  it('should add reply successfully', async () => {
    // Arrange
    component.collaboration = mockCollaboration;
    component.newReply = {
      commentId: 1,
      reply: 'New test reply'
    };

    const newReply = {
      id: 2,
      commentId: 1,
      employeeId: 123,
      employeeName: 'John Doe',
      employeePhoto: '/assets/images/john.jpg',
      reply: 'New test reply',
      createdAt: new Date()
    };

    mockProjectService.addCommentReply.and.returnValue(of(newReply));

    // Act
    await component.addReply(1);

    // Assert
    expect(mockProjectService.addCommentReply).toHaveBeenCalledWith(component.newReply);
    expect(component.collaboration.comments[0].replies.length).toBe(2);
    expect(component.replyingToComment).toBeNull();
    expect(component.isSubmittingReply).toBeFalse();
  });

  it('should delete comment successfully', async () => {
    // Arrange
    component.collaboration = mockCollaboration;
    spyOn(window, 'confirm').and.returnValue(true);
    mockProjectService.deleteComment.and.returnValue(of(void 0));

    // Act
    await component.deleteComment(1);

    // Assert
    expect(mockProjectService.deleteComment).toHaveBeenCalledWith(1);
    expect(component.collaboration.comments.length).toBe(0);
    expect(component.collaboration.communicationStats.totalComments).toBe(0);
  });

  it('should not delete comment when user cancels', async () => {
    // Arrange
    spyOn(window, 'confirm').and.returnValue(false);

    // Act
    await component.deleteComment(1);

    // Assert
    expect(mockProjectService.deleteComment).not.toHaveBeenCalled();
  });

  it('should delete reply successfully', async () => {
    // Arrange
    component.collaboration = mockCollaboration;
    spyOn(window, 'confirm').and.returnValue(true);
    mockProjectService.deleteCommentReply.and.returnValue(of(void 0));

    // Act
    await component.deleteReply(1);

    // Assert
    expect(mockProjectService.deleteCommentReply).toHaveBeenCalledWith(1);
    expect(component.collaboration.comments[0].replies.length).toBe(0);
  });

  it('should return correct member comment count', () => {
    // Arrange
    component.collaboration = mockCollaboration;

    // Act
    const count = component.getMemberCommentCount(123);

    // Assert
    expect(count).toBe(1);
  });

  it('should return correct member activity count', () => {
    // Arrange
    component.collaboration = mockCollaboration;

    // Act
    const count = component.getMemberActivityCount(123);

    // Assert
    expect(count).toBe(1);
  });

  it('should determine if member is active correctly', () => {
    // Arrange
    component.collaboration = mockCollaboration;

    // Act
    const isActive123 = component.isMemberActive(123);
    const isActive456 = component.isMemberActive(456);

    // Assert
    expect(isActive123).toBeTrue(); // Recent activity
    expect(isActive456).toBeFalse(); // Activity more than 7 days ago
  });

  it('should return true for canDeleteComment', () => {
    // Arrange
    const comment = mockCollaboration.comments[0];

    // Act
    const canDelete = component.canDeleteComment(comment);

    // Assert
    expect(canDelete).toBeTrue();
  });

  it('should return true for canDeleteReply', () => {
    // Arrange
    const reply = mockCollaboration.comments[0].replies[0];

    // Act
    const canDelete = component.canDeleteReply(reply);

    // Assert
    expect(canDelete).toBeTrue();
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

  it('should display collaboration data when loaded', () => {
    // Arrange
    component.isLoading = false;
    component.collaboration = mockCollaboration;

    // Act
    fixture.detectChanges();

    // Assert
    const headerElement = fixture.nativeElement.querySelector('h4');
    expect(headerElement.textContent).toContain('Team Collaboration');

    const commentsTab = fixture.nativeElement.querySelector('.nav-link');
    expect(commentsTab.textContent).toContain('Comments (1)');
  });

  it('should display communication stats', () => {
    // Arrange
    component.isLoading = false;
    component.collaboration = mockCollaboration;

    // Act
    fixture.detectChanges();

    // Assert
    const statsCard = fixture.nativeElement.querySelector('.card-header h6');
    expect(statsCard.textContent).toContain('Communication Stats');
  });

  it('should display team members', () => {
    // Arrange
    component.isLoading = false;
    component.collaboration = mockCollaboration;

    // Act
    fixture.detectChanges();

    // Assert
    const teamCard = fixture.nativeElement.querySelectorAll('.card-header h6')[1];
    expect(teamCard.textContent).toContain('Team Members (2)');
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