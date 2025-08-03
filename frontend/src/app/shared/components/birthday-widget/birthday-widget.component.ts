import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BirthdayService, BirthdayEmployee } from '../../../core/services/birthday.service';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-birthday-widget',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="birthday-widget" *ngIf="todayBirthdays.length > 0">
      <div class="widget-header">
        <h5 class="widget-title">
          <i class="fas fa-birthday-cake"></i>
          Today's Birthdays
        </h5>
        <span class="birthday-count">{{ todayBirthdays.length }}</span>
      </div>

      <div class="birthday-list">
        <div class="birthday-item" *ngFor="let employee of todayBirthdays">
          <div class="birthday-avatar">
            <img [src]="getProfilePhoto(employee)" 
                 [alt]="employee.firstName + ' ' + employee.lastName"
                 class="avatar-img">
            <div class="birthday-badge">
              <i class="fas fa-birthday-cake"></i>
            </div>
          </div>
          
          <div class="birthday-info">
            <div class="employee-name">
              {{ employee.firstName }} {{ employee.lastName }}
            </div>
            <div class="employee-details">
              {{ employee.designation }} • {{ employee.department }}
            </div>
            <div class="birthday-age">
              Turning {{ employee.age }} today! 🎉
            </div>
          </div>

          <div class="birthday-actions">
            <button class="btn btn-primary btn-sm rounded-pill"
                    (click)="openWishModal(employee)"
                    [disabled]="isWishSent(employee.id)">
              <i class="fas fa-heart"></i>
              {{ isWishSent(employee.id) ? 'Sent' : 'Send Wishes' }}
            </button>
          </div>
        </div>
      </div>

      <!-- Birthday Wish Modal -->
      <div class="modal fade" 
           [class.show]="showWishModal" 
           [style.display]="showWishModal ? 'block' : 'none'"
           *ngIf="showWishModal">
        <div class="modal-dialog modal-dialog-centered">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title">
                <i class="fas fa-birthday-cake text-primary"></i>
                Send Birthday Wishes to {{ selectedEmployee?.firstName }}
              </h5>
              <button type="button" class="btn-close" (click)="closeWishModal()"></button>
            </div>
            <div class="modal-body">
              <div class="birthday-employee-info">
                <img [src]="getProfilePhoto(selectedEmployee!)" 
                     [alt]="selectedEmployee?.firstName"
                     class="birthday-modal-avatar">
                <div>
                  <h6>{{ selectedEmployee?.firstName }} {{ selectedEmployee?.lastName }}</h6>
                  <p class="text-muted">{{ selectedEmployee?.designation }}</p>
                </div>
              </div>
              
              <div class="wish-templates mb-3">
                <label class="form-label">Quick Templates:</label>
                <div class="template-buttons">
                  <button class="btn btn-outline-primary btn-sm" 
                          *ngFor="let template of wishTemplates"
                          (click)="selectTemplate(template)">
                    {{ template.title }}
                  </button>
                </div>
              </div>

              <div class="mb-3">
                <label for="wishMessage" class="form-label">Your Message:</label>
                <textarea class="form-control" 
                          id="wishMessage"
                          rows="4"
                          [(ngModel)]="wishMessage"
                          placeholder="Write your birthday wishes here..."
                          maxlength="500"></textarea>
                <div class="form-text">{{ wishMessage.length }}/500 characters</div>
              </div>
            </div>
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" (click)="closeWishModal()">
                Cancel
              </button>
              <button type="button" 
                      class="btn btn-primary"
                      (click)="sendWish()"
                      [disabled]="!wishMessage.trim() || isSendingWish">
                <i class="fas fa-paper-plane" *ngIf="!isSendingWish"></i>
                <i class="fas fa-spinner fa-spin" *ngIf="isSendingWish"></i>
                {{ isSendingWish ? 'Sending...' : 'Send Wishes' }}
              </button>
            </div>
          </div>
        </div>
      </div>
      <div class="modal-backdrop fade" 
           [class.show]="showWishModal" 
           *ngIf="showWishModal"
           (click)="closeWishModal()"></div>
    </div>

    <!-- No Birthdays Message -->
    <div class="birthday-widget no-birthdays" *ngIf="todayBirthdays.length === 0">
      <div class="no-birthday-content">
        <i class="fas fa-calendar-day"></i>
        <h6>No Birthdays Today</h6>
        <p class="text-muted">Check back tomorrow for birthday celebrations!</p>
      </div>
    </div>
  `,
  styles: [`
    .birthday-widget {
      background: white;
      border-radius: 16px;
      padding: 1.5rem;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
      border: 1px solid var(--gray-100);
      transition: all 0.2s ease-in-out;
    }

    .birthday-widget:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
    }

    .widget-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 1.5rem;
    }

    .widget-title {
      font-weight: 600;
      color: var(--text-primary);
      margin: 0;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .widget-title i {
      color: #ff6b6b;
    }

    .birthday-count {
      background: linear-gradient(135deg, #ff6b6b 0%, #ee5a52 100%);
      color: white;
      padding: 0.25rem 0.75rem;
      border-radius: 20px;
      font-size: 0.875rem;
      font-weight: 600;
    }

    .birthday-list {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .birthday-item {
      display: flex;
      align-items: center;
      gap: 1rem;
      padding: 1rem;
      background: linear-gradient(135deg, #fff5f5 0%, #fef2f2 100%);
      border-radius: 12px;
      border: 1px solid #fecaca;
      transition: all 0.2s ease;
    }

    .birthday-item:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(255, 107, 107, 0.15);
    }

    .birthday-avatar {
      position: relative;
      flex-shrink: 0;
    }

    .avatar-img {
      width: 60px;
      height: 60px;
      border-radius: 50%;
      object-fit: cover;
      border: 3px solid #ff6b6b;
    }

    .birthday-badge {
      position: absolute;
      bottom: -5px;
      right: -5px;
      background: #ff6b6b;
      color: white;
      width: 24px;
      height: 24px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 0.75rem;
      border: 2px solid white;
    }

    .birthday-info {
      flex: 1;
    }

    .employee-name {
      font-weight: 600;
      color: var(--text-primary);
      margin-bottom: 0.25rem;
    }

    .employee-details {
      font-size: 0.875rem;
      color: var(--text-secondary);
      margin-bottom: 0.25rem;
    }

    .birthday-age {
      font-size: 0.875rem;
      color: #ff6b6b;
      font-weight: 500;
    }

    .birthday-actions {
      flex-shrink: 0;
    }

    .btn-primary {
      background: linear-gradient(135deg, #ff6b6b 0%, #ee5a52 100%);
      border: none;
      font-weight: 500;
      padding: 0.5rem 1rem;
      transition: all 0.2s ease;
    }

    .btn-primary:hover {
      transform: translateY(-1px);
      box-shadow: 0 4px 8px rgba(255, 107, 107, 0.4);
    }

    .btn-primary:disabled {
      background: #6c757d;
      opacity: 0.6;
      transform: none;
      box-shadow: none;
    }

    /* No Birthdays State */
    .no-birthdays {
      text-align: center;
      padding: 2rem;
      background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
    }

    .no-birthday-content i {
      font-size: 3rem;
      color: var(--gray-400);
      margin-bottom: 1rem;
    }

    .no-birthday-content h6 {
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }

    /* Modal Styles */
    .modal {
      z-index: 1050;
    }

    .modal-backdrop {
      z-index: 1040;
    }

    .birthday-employee-info {
      display: flex;
      align-items: center;
      gap: 1rem;
      margin-bottom: 1.5rem;
      padding: 1rem;
      background: #f8f9fa;
      border-radius: 8px;
    }

    .birthday-modal-avatar {
      width: 60px;
      height: 60px;
      border-radius: 50%;
      object-fit: cover;
      border: 3px solid #ff6b6b;
    }

    .template-buttons {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
    }

    .template-buttons .btn {
      font-size: 0.875rem;
      padding: 0.375rem 0.75rem;
    }

    @media (max-width: 768px) {
      .birthday-item {
        flex-direction: column;
        text-align: center;
        gap: 0.75rem;
      }

      .birthday-info {
        text-align: center;
      }

      .template-buttons {
        justify-content: center;
      }
    }
  `]
})
export class BirthdayWidgetComponent implements OnInit, OnDestroy {
  todayBirthdays: BirthdayEmployee[] = [];
  showWishModal: boolean = false;
  selectedEmployee: BirthdayEmployee | null = null;
  wishMessage: string = '';
  isSendingWish: boolean = false;
  sentWishes: Set<number> = new Set();

  wishTemplates = [
    { title: 'Happy Birthday!', message: 'Wishing you a very happy birthday! May this special day bring you joy, happiness, and wonderful memories. Have a fantastic celebration!' },
    { title: 'Best Wishes', message: 'Happy Birthday! Hope your special day is filled with happiness, laughter, and all your favorite things. Enjoy your day!' },
    { title: 'Celebration Time', message: 'It\'s your special day! Wishing you a birthday filled with sweet moments and wonderful memories. Happy Birthday!' },
    { title: 'Many Happy Returns', message: 'Many happy returns of the day! May this birthday be the beginning of a year filled with good luck, good health, and much happiness.' }
  ];

  private birthdaySubscription?: Subscription;

  constructor(
    private birthdayService: BirthdayService,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadBirthdays();
  }

  ngOnDestroy(): void {
    this.birthdaySubscription?.unsubscribe();
  }

  private loadBirthdays(): void {
    this.birthdaySubscription = this.birthdayService.todayBirthdays$.subscribe(
      birthdays => {
        this.todayBirthdays = birthdays;
      }
    );
  }

  getProfilePhoto(employee: BirthdayEmployee): string {
    return employee.profilePhoto || '/assets/images/default-avatar.png';
  }

  openWishModal(employee: BirthdayEmployee): void {
    this.selectedEmployee = employee;
    this.wishMessage = '';
    this.showWishModal = true;
    document.body.classList.add('modal-open');
  }

  closeWishModal(): void {
    this.showWishModal = false;
    this.selectedEmployee = null;
    this.wishMessage = '';
    document.body.classList.remove('modal-open');
  }

  selectTemplate(template: any): void {
    this.wishMessage = template.message;
  }

  sendWish(): void {
    if (!this.selectedEmployee || !this.wishMessage.trim()) {
      return;
    }

    this.isSendingWish = true;

    this.birthdayService.sendBirthdayWish(this.selectedEmployee.id, this.wishMessage.trim())
      .subscribe({
        next: (wish) => {
          this.sentWishes.add(this.selectedEmployee!.id);
          this.notificationService.showSuccess(
            `Birthday wishes sent to ${this.selectedEmployee!.firstName}! 🎉`
          );
          this.closeWishModal();
          this.isSendingWish = false;
        },
        error: (error) => {
          console.error('Error sending birthday wish:', error);
          this.notificationService.showError('Failed to send birthday wishes. Please try again.');
          this.isSendingWish = false;
        }
      });
  }

  isWishSent(employeeId: number): boolean {
    return this.sentWishes.has(employeeId);
  }
}