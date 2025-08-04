import { Component, OnInit, OnDestroy, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProjectService } from '../../../services/project.service';
import { ProjectCollaboration, ProjectComment, ProjectActivity, CreateProjectComment, CreateCommentReply } from '../../../models/project.models';
import { Subject, takeUntil, firstValueFrom } from 'rxjs';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import * as signalR from '@microsoft/signalr';

@Component({
  selector: 'app-project-collaboration',
  standalone: true,
  imports: [CommonModule, FormsModule, NgbModule],
  template: `
    <div class="project-collaboration-container">
      <!-- Header -->
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h4 class="mb-0">
          <i class="fas fa-users text-primary me-2"></i>
          Team Collaboration
        </h4>
        <div class="d-flex gap-2">
          <button class="btn btn-outline-primary btn-sm" (click)="refreshData()">
            <i class="fas fa-sync-alt" [class.fa-spin]="isLoading"></i>
            Refresh
          </button>
          <div class="dropdown">
            <button class="btn btn-outline-secondary btn-sm dropdown-toggle" type="button" data-bs-toggle="dropdown">
              <i class="fas fa-filter me-1"></i>
              View
            </button>
            <ul class="dropdown-menu">
              <li><a class="dropdown-item" href="#" (click)="setActiveTab('comments')">Comments</a></li>
              <li><a class="dropdown-item" href="#" (click)="setActiveTab('activities')">Activities</a></li>
              <li><a class="dropdown-item" href="#" (click)="setActiveTab('team')">Team Members</a></li>
            </ul>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div *ngIf="isLoading" class="text-center py-4">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
      </div>

      <!-- Collaboration Content -->
      <div *ngIf="!isLoading && collaboration" class="row">
        <!-- Left Column - Comments and Activities -->
        <div class="col-lg-8">
          <!-- Tabs -->
          <ul class="nav nav-tabs mb-3">
            <li class="nav-item">
              <a class="nav-link" 
                 [class.active]="activeTab === 'comments'"
                 href="#" 
                 (click)="setActiveTab('comments')">
                <i class="fas fa-comments me-1"></i>
                Comments ({{ collaboration.comments.length }})
              </a>
            </li>
            <li class="nav-item">
              <a class="nav-link" 
                 [class.active]="activeTab === 'activities'"
                 href="#" 
                 (click)="setActiveTab('activities')">
                <i class="fas fa-history me-1"></i>
                Activities ({{ collaboration.activities.length }})
              </a>
            </li>
          </ul>

          <!-- Comments Tab -->
          <div *ngIf="activeTab === 'comments'">
            <!-- Add Comment Form -->
            <div class="card mb-3">
              <div class="card-body">
                <form (ngSubmit)="addComment()" #commentForm="ngForm">
                  <div class="mb-3">
                    <textarea 
                      class="form-control" 
                      rows="3" 
                      placeholder="Add a comment..."
                      [(ngModel)]="newComment.comment"
                      name="comment"
                      required>
                    </textarea>
                  </div>
                  <div class="d-flex justify-content-between align-items-center">
                    <div class="form-check" *ngIf="selectedTaskId">
                      <input class="form-check-input" type="checkbox" id="taskComment" [(ngModel)]="isTaskComment" name="taskComment">
                      <label class="form-check-label" for="taskComment">
                        Comment on specific task
                      </label>
                    </div>
                    <button type="submit" 
                            class="btn btn-primary"
                            [disabled]="!commentForm.valid || isSubmittingComment">
                      <i class="fas fa-paper-plane me-1"></i>
                      {{ isSubmittingComment ? 'Posting...' : 'Post Comment' }}
                    </button>
                  </div>
                </form>
              </div>
            </div>

            <!-- Comments List -->
            <div class="comments-list">
              <div *ngFor="let comment of collaboration.comments" class="card mb-3">
                <div class="card-body">
                  <div class="d-flex align-items-start">
                    <img [src]="comment.employeePhoto || '/assets/images/default-avatar.png'" 
                         class="rounded-circle me-3" 
                         width="40" 
                         height="40" 
                         [alt]="comment.employeeName">
                    <div class="flex-grow-1">
                      <div class="d-flex justify-content-between align-items-start mb-2">
                        <div>
                          <strong>{{ comment.employeeName }}</strong>
                          <small class="text-muted ms-2">{{ comment.createdAt | date:'short' }}</small>
                          <span *ngIf="comment.taskId" class="badge bg-info ms-2">Task Comment</span>
                        </div>
                        <div class="dropdown" *ngIf="canDeleteComment(comment)">
                          <button class="btn btn-sm btn-outline-secondary dropdown-toggle" type="button" data-bs-toggle="dropdown">
                            <i class="fas fa-ellipsis-v"></i>
                          </button>
                          <ul class="dropdown-menu">
                            <li><a class="dropdown-item text-danger" href="#" (click)="deleteComment(comment.id)">
                              <i class="fas fa-trash me-1"></i>Delete
                            </a></li>
                          </ul>
                        </div>
                      </div>
                      <p class="mb-2">{{ comment.comment }}</p>
                      
                      <!-- Replies -->
                      <div *ngIf="comment.replies.length > 0" class="replies mt-3">
                        <div *ngFor="let reply of comment.replies" class="d-flex align-items-start mb-2 ms-3">
                          <img [src]="reply.employeePhoto || '/assets/images/default-avatar.png'" 
                               class="rounded-circle me-2" 
                               width="30" 
                               height="30" 
                               [alt]="reply.employeeName">
                          <div class="flex-grow-1">
                            <div class="bg-light p-2 rounded">
                              <div class="d-flex justify-content-between align-items-start">
                                <div>
                                  <strong class="small">{{ reply.employeeName }}</strong>
                                  <small class="text-muted ms-1">{{ reply.createdAt | date:'short' }}</small>
                                </div>
                                <button *ngIf="canDeleteReply(reply)" 
                                        class="btn btn-sm btn-link text-danger p-0"
                                        (click)="deleteReply(reply.id)">
                                  <i class="fas fa-times"></i>
                                </button>
                              </div>
                              <p class="mb-0 small">{{ reply.reply }}</p>
                            </div>
                          </div>
                        </div>
                      </div>

                      <!-- Reply Form -->
                      <div *ngIf="replyingToComment === comment.id" class="mt-3">
                        <form (ngSubmit)="addReply(comment.id)" #replyForm="ngForm">
                          <div class="input-group">
                            <input type="text" 
                                   class="form-control" 
                                   placeholder="Write a reply..."
                                   [(ngModel)]="newReply.reply"
                                   name="reply"
                                   required>
                            <button type="submit" 
                                    class="btn btn-outline-primary"
                                    [disabled]="!replyForm.valid || isSubmittingReply">
                              <i class="fas fa-paper-plane"></i>
                            </button>
                            <button type="button" 
                                    class="btn btn-outline-secondary"
                                    (click)="cancelReply()">
                              Cancel
                            </button>
                          </div>
                        </form>
                      </div>
                      <div *ngIf="replyingToComment !== comment.id" class="mt-2">
                        <button class="btn btn-sm btn-link text-primary p-0" (click)="startReply(comment.id)">
                          <i class="fas fa-reply me-1"></i>Reply
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div *ngIf="collaboration.comments.length === 0" class="text-center py-4">
                <i class="fas fa-comments fa-3x text-muted mb-3"></i>
                <p class="text-muted">No comments yet. Be the first to comment!</p>
              </div>
            </div>
          </div>

          <!-- Activities Tab -->
          <div *ngIf="activeTab === 'activities'">
            <div class="activities-list">
              <div *ngFor="let activity of collaboration.activities" class="card mb-2">
                <div class="card-body py-2">
                  <div class="d-flex align-items-center">
                    <div class="activity-icon me-3">
                      <i class="fas" 
                         [class.fa-comment]="activity.activityType === 'Comment'"
                         [class.fa-tasks]="activity.activityType === 'Task'"
                         [class.fa-edit]="activity.activityType === 'Update'"
                         [class.fa-plus]="activity.activityType === 'Create'"
                         [class.fa-trash]="activity.activityType === 'Delete'"
                         [class.text-primary]="activity.activityType === 'Comment'"
                         [class.text-success]="activity.activityType === 'Create'"
                         [class.text-warning]="activity.activityType === 'Update'"
                         [class.text-danger]="activity.activityType === 'Delete'">
                      </i>
                    </div>
                    <div class="flex-grow-1">
                      <div class="d-flex justify-content-between align-items-center">
                        <div>
                          <strong>{{ activity.employeeName }}</strong>
                          <span class="ms-2">{{ activity.description }}</span>
                        </div>
                        <small class="text-muted">{{ activity.createdAt | date:'short' }}</small>
                      </div>
                      <div *ngIf="activity.details" class="small text-muted mt-1">
                        {{ activity.details }}
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div *ngIf="collaboration.activities.length === 0" class="text-center py-4">
                <i class="fas fa-history fa-3x text-muted mb-3"></i>
                <p class="text-muted">No activities recorded yet.</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Right Column - Team Members and Stats -->
        <div class="col-lg-4">
          <!-- Communication Stats -->
          <div class="card mb-3">
            <div class="card-header">
              <h6 class="mb-0">
                <i class="fas fa-chart-bar me-2"></i>
                Communication Stats
              </h6>
            </div>
            <div class="card-body">
              <div class="row text-center">
                <div class="col-6">
                  <div class="border-end">
                    <h4 class="text-primary mb-0">{{ collaboration.communicationStats.totalComments }}</h4>
                    <small class="text-muted">Comments</small>
                  </div>
                </div>
                <div class="col-6">
                  <h4 class="text-success mb-0">{{ collaboration.communicationStats.totalActivities }}</h4>
                  <small class="text-muted">Activities</small>
                </div>
              </div>
              <hr>
              <div class="text-center">
                <div class="mb-2">
                  <strong>{{ collaboration.communicationStats.activeTeamMembers }}</strong>
                  <small class="text-muted">active members this week</small>
                </div>
                <small class="text-muted">
                  Last activity: {{ collaboration.communicationStats.lastActivity | date:'short' }}
                </small>
              </div>
            </div>
          </div>

          <!-- Team Members -->
          <div class="card">
            <div class="card-header">
              <h6 class="mb-0">
                <i class="fas fa-users me-2"></i>
                Team Members ({{ collaboration.teamMembers.length }})
              </h6>
            </div>
            <div class="card-body p-0">
              <div class="list-group list-group-flush">
                <div *ngFor="let member of collaboration.teamMembers" 
                     class="list-group-item d-flex align-items-center">
                  <img [src]="member.employeePhoto || '/assets/images/default-avatar.png'" 
                       class="rounded-circle me-3" 
                       width="32" 
                       height="32" 
                       [alt]="member.employeeName">
                  <div class="flex-grow-1">
                    <div class="fw-bold">{{ member.employeeName }}</div>
                    <small class="text-muted">{{ member.role }}</small>
                  </div>
                  <div class="text-end">
                    <div class="d-flex gap-2">
                      <span class="badge bg-light text-dark" title="Comments">
                        <i class="fas fa-comment"></i>
                        {{ getMemberCommentCount(member.employeeId) }}
                      </span>
                      <span class="badge bg-light text-dark" title="Activities">
                        <i class="fas fa-activity"></i>
                        {{ getMemberActivityCount(member.employeeId) }}
                      </span>
                    </div>
                    <div class="online-status mt-1">
                      <span class="badge" 
                            [class.bg-success]="isMemberActive(member.employeeId)"
                            [class.bg-secondary]="!isMemberActive(member.employeeId)">
                        {{ isMemberActive(member.employeeId) ? 'Active' : 'Inactive' }}
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Error State -->
      <div *ngIf="!isLoading && error" class="alert alert-danger">
        <i class="fas fa-exclamation-triangle me-2"></i>
        {{ error }}
      </div>
    </div>
  `,
  styles: [`
    .project-collaboration-container {
      padding: 1rem;
    }

    .comments-list .card {
      border-left: 3px solid #007bff;
    }

    .replies {
      border-left: 2px solid #e9ecef;
      padding-left: 1rem;
    }

    .activity-icon {
      width: 30px;
      text-align: center;
    }

    .activities-list .card {
      border-left: 3px solid #28a745;
    }

    .online-status .badge {
      font-size: 0.7rem;
    }

    .nav-tabs .nav-link {
      color: #6c757d;
      border: none;
      border-bottom: 2px solid transparent;
    }

    .nav-tabs .nav-link.active {
      color: #007bff;
      border-bottom-color: #007bff;
      background-color: transparent;
    }

    .border-end {
      border-right: 1px solid #dee2e6 !important;
    }

    .fa-spin {
      animation: fa-spin 2s infinite linear;
    }

    @keyframes fa-spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
  `]
})
export class ProjectCollaborationComponent implements OnInit, OnDestroy {
  @Input() projectId!: number;
  @Input() selectedTaskId?: number;

  collaboration: ProjectCollaboration | null = null;
  activeTab = 'comments';
  isLoading = false;
  error: string | null = null;

  // Comment form
  newComment: CreateProjectComment = {
    projectId: 0,
    taskId: undefined,
    comment: ''
  };
  isTaskComment = false;
  isSubmittingComment = false;

  // Reply form
  newReply: CreateCommentReply = {
    commentId: 0,
    reply: ''
  };
  replyingToComment: number | null = null;
  isSubmittingReply = false;

  // SignalR connection
  private hubConnection: signalR.HubConnection | null = null;
  private destroy$ = new Subject<void>();

  constructor(private projectService: ProjectService) {}

  ngOnInit(): void {
    if (this.projectId) {
      this.newComment.projectId = this.projectId;
      this.loadCollaborationData();
      this.setupSignalRConnection();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  async loadCollaborationData(): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      this.collaboration = await firstValueFrom(this.projectService.getProjectCollaboration(this.projectId));
    } catch (error: any) {
      console.error('Error loading collaboration data:', error);
      this.error = error.message || 'Failed to load collaboration data';
    } finally {
      this.isLoading = false;
    }
  }

  refreshData(): void {
    this.loadCollaborationData();
  }

  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }

  async addComment(): Promise<void> {
    if (!this.newComment.comment.trim()) return;

    this.isSubmittingComment = true;

    try {
      if (this.isTaskComment && this.selectedTaskId) {
        this.newComment.taskId = this.selectedTaskId;
      }

      const comment = await this.projectService.addProjectComment(this.newComment).toPromise();
      
      if (this.collaboration) {
        this.collaboration.comments.unshift(comment);
        this.collaboration.communicationStats.totalComments++;
      }

      // Reset form
      this.newComment.comment = '';
      this.newComment.taskId = undefined;
      this.isTaskComment = false;

    } catch (error: any) {
      console.error('Error adding comment:', error);
      this.error = error.message || 'Failed to add comment';
    } finally {
      this.isSubmittingComment = false;
    }
  }

  startReply(commentId: number): void {
    this.replyingToComment = commentId;
    this.newReply.commentId = commentId;
    this.newReply.reply = '';
  }

  cancelReply(): void {
    this.replyingToComment = null;
    this.newReply = { commentId: 0, reply: '' };
  }

  async addReply(commentId: number): Promise<void> {
    if (!this.newReply.reply.trim()) return;

    this.isSubmittingReply = true;

    try {
      const reply = await this.projectService.addCommentReply(this.newReply).toPromise();
      
      if (this.collaboration) {
        const comment = this.collaboration.comments.find(c => c.id === commentId);
        if (comment) {
          comment.replies.push(reply);
        }
      }

      this.cancelReply();

    } catch (error: any) {
      console.error('Error adding reply:', error);
      this.error = error.message || 'Failed to add reply';
    } finally {
      this.isSubmittingReply = false;
    }
  }

  async deleteComment(commentId: number): Promise<void> {
    if (!confirm('Are you sure you want to delete this comment?')) return;

    try {
      await this.projectService.deleteComment(commentId).toPromise();
      
      if (this.collaboration) {
        this.collaboration.comments = this.collaboration.comments.filter(c => c.id !== commentId);
        this.collaboration.communicationStats.totalComments--;
      }

    } catch (error: any) {
      console.error('Error deleting comment:', error);
      this.error = error.message || 'Failed to delete comment';
    }
  }

  async deleteReply(replyId: number): Promise<void> {
    if (!confirm('Are you sure you want to delete this reply?')) return;

    try {
      await this.projectService.deleteCommentReply(replyId).toPromise();
      
      if (this.collaboration) {
        this.collaboration.comments.forEach(comment => {
          comment.replies = comment.replies.filter(r => r.id !== replyId);
        });
      }

    } catch (error: any) {
      console.error('Error deleting reply:', error);
      this.error = error.message || 'Failed to delete reply';
    }
  }

  canDeleteComment(comment: ProjectComment): boolean {
    // In a real app, this would check if the current user is the comment author or has admin rights
    return true; // Simplified for demo
  }

  canDeleteReply(reply: any): boolean {
    // In a real app, this would check if the current user is the reply author or has admin rights
    return true; // Simplified for demo
  }

  getMemberCommentCount(employeeId: number): number {
    if (!this.collaboration) return 0;
    return this.collaboration.communicationStats.memberActivities
      .find(ma => ma.employeeId === employeeId)?.commentsCount || 0;
  }

  getMemberActivityCount(employeeId: number): number {
    if (!this.collaboration) return 0;
    return this.collaboration.communicationStats.memberActivities
      .find(ma => ma.employeeId === employeeId)?.activitiesCount || 0;
  }

  isMemberActive(employeeId: number): boolean {
    if (!this.collaboration) return false;
    const memberActivity = this.collaboration.communicationStats.memberActivities
      .find(ma => ma.employeeId === employeeId);
    
    if (!memberActivity) return false;
    
    const lastActivity = new Date(memberActivity.lastActivity);
    const weekAgo = new Date();
    weekAgo.setDate(weekAgo.getDate() - 7);
    
    return lastActivity > weekAgo;
  }

  private async setupSignalRConnection(): Promise<void> {
    try {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl('/projectHub')
        .build();

      await this.hubConnection.start();
      
      // Join project group
      await this.hubConnection.invoke('JoinProjectGroup', this.projectId);

      // Listen for real-time updates
      this.hubConnection.on('ReceiveNotification', (notification) => {
        console.log('Received notification:', notification);
        // Handle real-time notifications
      });

      this.hubConnection.on('ReceiveProjectUpdate', (update) => {
        console.log('Received project update:', update);
        if (update.updateType === 'activity_logged') {
          this.refreshData();
        }
      });

    } catch (error) {
      console.error('Error setting up SignalR connection:', error);
    }
  }
}