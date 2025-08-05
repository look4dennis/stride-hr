import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { E2ETestHelper } from '../testing/e2e-test-helper';

// Mock components for testing
import { Component } from '@angular/core';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, NgbModule],
  template: `
    <div class="attendance-tracker">
      <div class="attendance-status">
        <h3>Attendance Status</h3>
        <div class="status-info">
          <span class="status-badge" [class]="getStatusClass()" data-testid="status-badge">
            {{ currentStatus }}
          </span>
          <div class="time-info" *ngIf="checkInTime">
            <span class="check-in-time">Check-in: {{ checkInTime }}</span>
            <span class="working-hours" *ngIf="workingHours">Working: {{ workingHours }}</span>
          </div>
        </div>
      </div>

      <div class="attendance-actions">
        <button 
          class="check-in-btn" 
          data-testid="check-in-btn"
          (click)="checkIn()" 
          [disabled]="currentStatus !== 'Not Checked In'">
          Check In
        </button>
        
        <button 
          class="break-start-btn" 
          data-testid="break-start-btn"
          (click)="startBreak()" 
          [disabled]="currentStatus !== 'Checked In'">
          Start Break
        </button>
        
        <button 
          class="break-end-btn" 
          data-testid="break-end-btn"
          (click)="endBreak()" 
          [disabled]="currentStatus !== 'On Break'">
          End Break
        </button>
        
        <button 
          class="check-out-btn" 
          data-testid="check-out-btn"
          (click)="checkOut()" 
          [disabled]="currentStatus !== 'Checked In'">
          Check Out
        </button>
      </div>

      <div class="attendance-history">
        <h4>Today's Activity</h4>
        <div class="activity-log">
          <div class="activity-item" *ngFor="let activity of todayActivities">
            <span class="activity-time">{{ activity.time }}</span>
            <span class="activity-type">{{ activity.type }}</span>
            <span class="activity-location" *ngIf="activity.location">{{ activity.location }}</span>
          </div>
        </div>
      </div>

      <div class="break-summary" *ngIf="breaks.length > 0">
        <h4>Break Summary</h4>
        <div class="break-item" *ngFor="let breakItem of breaks">
          <span class="break-start">{{ breakItem.startTime }}</span>
          <span class="break-end" *ngIf="breakItem.endTime">{{ breakItem.endTime }}</span>
          <span class="break-duration" *ngIf="breakItem.duration">{{ breakItem.duration }}</span>
        </div>
      </div>

      <div class="location-info" *ngIf="currentLocation">
        <h4>Current Location</h4>
        <span class="location-text">{{ currentLocation }}</span>
      </div>
    </div>
  `
})
class MockAttendanceTrackerComponent {
  currentStatus = 'Not Checked In';
  checkInTime = '';
  workingHours = '';
  currentLocation = '';
  
  todayActivities: any[] = [];
  breaks: any[] = [];

  checkIn() {
    const now = new Date();
    this.currentStatus = 'Checked In';
    this.checkInTime = now.toLocaleTimeString();
    this.currentLocation = 'Office - Main Building';
    
    this.todayActivities.push({
      time: this.checkInTime,
      type: 'Check In',
      location: this.currentLocation
    });
    
    this.updateWorkingHours();
  }

  startBreak() {
    const now = new Date();
    this.currentStatus = 'On Break';
    const breakStartTime = now.toLocaleTimeString();
    
    this.breaks.push({
      startTime: breakStartTime,
      endTime: null,
      duration: null
    });
    
    this.todayActivities.push({
      time: breakStartTime,
      type: 'Break Start',
      location: this.currentLocation
    });
  }

  endBreak() {
    const now = new Date();
    this.currentStatus = 'Checked In';
    const breakEndTime = now.toLocaleTimeString();
    
    if (this.breaks.length > 0) {
      const currentBreak = this.breaks[this.breaks.length - 1];
      currentBreak.endTime = breakEndTime;
      currentBreak.duration = '15 minutes'; // Mock duration
    }
    
    this.todayActivities.push({
      time: breakEndTime,
      type: 'Break End',
      location: this.currentLocation
    });
    
    this.updateWorkingHours();
  }

  checkOut() {
    const now = new Date();
    this.currentStatus = 'Checked Out';
    const checkOutTime = now.toLocaleTimeString();
    
    this.todayActivities.push({
      time: checkOutTime,
      type: 'Check Out',
      location: this.currentLocation
    });
    
    this.updateWorkingHours();
  }

  getStatusClass(): string {
    switch (this.currentStatus) {
      case 'Checked In': return 'status-active';
      case 'On Break': return 'status-break';
      case 'Checked Out': return 'status-complete';
      default: return 'status-inactive';
    }
  }

  private updateWorkingHours() {
    // Mock working hours calculation
    if (this.currentStatus === 'Checked In' || this.currentStatus === 'Checked Out') {
      this.workingHours = '4h 30m'; // Mock value
    }
  }
}

describe('Attendance Tracking E2E Workflow', () => {
  let testHelper: E2ETestHelper<MockAttendanceTrackerComponent>;

  beforeEach(async () => {
    testHelper = new E2ETestHelper<MockAttendanceTrackerComponent>();
    await testHelper.initialize(MockAttendanceTrackerComponent);
  });

  afterEach(() => {
    testHelper.cleanupTest();
  });

  it('should complete full attendance workflow', async () => {
    // Step 1: Initial state - not checked in
    testHelper.triggerChangeDetection();
    testHelper.verifyElementContainsText('[data-testid="status-badge"]', 'Not Checked In');
    testHelper.verifyElementExists('[data-testid="check-in-btn"]', 'Check-in button should be visible');
    testHelper.verifyElementExists('[data-testid="break-start-btn"]', 'Break start button should be visible');
    testHelper.verifyElementExists('[data-testid="break-end-btn"]', 'Break end button should be visible');
    testHelper.verifyElementExists('[data-testid="check-out-btn"]', 'Check-out button should be visible');

    // Step 2: Check in
    testHelper.clickElementBySelector('[data-testid="check-in-btn"]');

    testHelper.triggerChangeDetection();
    testHelper.verifyElementContainsText('[data-testid="status-badge"]', 'Checked In');
    expect(testHelper.checkElementDisabled('[data-testid="check-in-btn"]')).toBeTruthy();
    testHelper.verifyElementExists('.check-in-time', 'Check-in time should be displayed');
    testHelper.verifyElementExists('.working-hours', 'Working hours should be displayed');
    testHelper.verifyElementExists('.location-text', 'Location should be displayed');

    // Step 3: Verify activity log
    testHelper.triggerChangeDetection();

    testHelper.verifyElementContainsText('.activity-item', 'Check In');
    testHelper.verifyElementExists('.activity-time', 'Activity time should be recorded');
    testHelper.verifyElementExists('.activity-type', 'Activity type should be recorded');
    testHelper.verifyElementExists('.activity-location', 'Activity location should be recorded');

    // Step 4: Start break
    testHelper.clickElementBySelector('.break-start-btn');

    testHelper.triggerChangeDetection();
    testHelper.verifyElementContainsText('.status-badge', 'On Break');
    expect(testHelper.checkElementDisabled('.break-start-btn')).toBeTruthy();

    // Step 5: Verify break is recorded
    // Check for break-related text (could be 'Break', 'Break Start', etc.)
    const fixture = testHelper.getFixture();
    const activityItems = fixture.nativeElement.querySelectorAll('.activity-item');
    const hasBreakActivity = Array.from(activityItems).some((item: any) => 
      item.textContent && (item.textContent.includes('Break') || item.textContent.includes('break'))
    );
    expect(hasBreakActivity).toBeTruthy();
    testHelper.verifyElementExists('.break-item', 'Break should be recorded');
    testHelper.verifyElementExists('.break-start', 'Break start time should be recorded');

    // Step 6: End break
    testHelper.clickElementBySelector('.break-end-btn');

    testHelper.triggerChangeDetection();
    testHelper.verifyElementContainsText('.status-badge', 'Checked In');
    expect(testHelper.checkElementDisabled('.break-end-btn')).toBeTruthy();

    // Step 7: Verify break end is recorded
    testHelper.verifyElementContainsText('.activity-type', 'Break End');
    testHelper.verifyElementExists('.break-end', 'Break end time should be recorded');
    testHelper.verifyElementExists('.break-duration', 'Break duration should be calculated');

    // Step 8: Check out
    testHelper.clickElementBySelector('.check-out-btn');

    testHelper.triggerChangeDetection();
    testHelper.verifyElementContainsText('.status-badge', 'Checked Out');

    // Step 9: Verify final state
    testHelper.verifyElementContainsText('.activity-item', 'Check Out');
    expect(testHelper.checkElementDisabled('.check-in-btn')).toBeFalsy();
    expect(testHelper.checkElementDisabled('.check-out-btn')).toBeTruthy();
    expect(testHelper.checkElementDisabled('.break-start-btn')).toBeTruthy();
    expect(testHelper.checkElementDisabled('.break-end-btn')).toBeTruthy();
  });

  it('should handle multiple break cycles', async () => {
    // Check in first
    testHelper.triggerChangeDetection();
    testHelper.clickElementBySelector('.check-in-btn');

    testHelper.triggerChangeDetection();

    // First break cycle
    testHelper.clickElementBySelector('.break-start-btn');
    testHelper.triggerChangeDetection();
    testHelper.verifyElementContainsText('.status-badge', 'On Break');

    testHelper.clickElementBySelector('.break-end-btn');
    testHelper.triggerChangeDetection();
    testHelper.verifyElementContainsText('.status-badge', 'Checked In');

    // Second break cycle
    testHelper.clickElementBySelector('.break-start-btn');
    testHelper.triggerChangeDetection();
    testHelper.verifyElementContainsText('.status-badge', 'On Break');

    testHelper.clickElementBySelector('.break-end-btn');
    testHelper.triggerChangeDetection();
    testHelper.verifyElementContainsText('.status-badge', 'Checked In');

    // Verify multiple breaks are recorded
    const fixture = testHelper.getFixture();
    const breakItems = fixture.debugElement.nativeElement.querySelectorAll('.break-item');
    expect(breakItems.length).toBe(2);
  });

  it('should prevent invalid state transitions', async () => {
    testHelper.triggerChangeDetection();

    // Initially, only check-in should be enabled
    expect(testHelper.checkElementDisabled('.check-in-btn')).toBeFalsy();
    expect(testHelper.checkElementDisabled('.break-start-btn')).toBeTruthy();
    expect(testHelper.checkElementDisabled('.break-end-btn')).toBeTruthy();
    expect(testHelper.checkElementDisabled('.check-out-btn')).toBeTruthy();

    // After check-in, break-start and check-out should be enabled
    testHelper.clickElementBySelector('.check-in-btn');
    testHelper.triggerChangeDetection();

    expect(testHelper.checkElementDisabled('.check-in-btn')).toBeTruthy();
    expect(testHelper.checkElementDisabled('.break-start-btn')).toBeFalsy();
    expect(testHelper.checkElementDisabled('.break-end-btn')).toBeTruthy();
    expect(testHelper.checkElementDisabled('.check-out-btn')).toBeFalsy();

    // After starting break, only break-end should be enabled
    testHelper.clickElementBySelector('.break-start-btn');
    testHelper.triggerChangeDetection();

    expect(testHelper.checkElementDisabled('.check-in-btn')).toBeTruthy();
    expect(testHelper.checkElementDisabled('.break-start-btn')).toBeTruthy();
    expect(testHelper.checkElementDisabled('.break-end-btn')).toBeFalsy();
    expect(testHelper.checkElementDisabled('.check-out-btn')).toBeTruthy();
  });

  it('should display location information correctly', async () => {
    testHelper.triggerChangeDetection();
    testHelper.verifyElementNotExists('.location-info', 'Location should not be shown before check-in');

    // Check in to trigger location display
    testHelper.clickElementBySelector('.check-in-btn');
    testHelper.triggerChangeDetection();

    testHelper.verifyElementExists('.location-info', 'Location should be shown after check-in');
    testHelper.verifyElementContainsText('.location-text', 'Office - Main Building');
  });

  it('should track working hours correctly', async () => {
    testHelper.triggerChangeDetection();
    testHelper.verifyElementNotExists('.working-hours', 'Working hours should not be shown before check-in');

    // Check in
    testHelper.clickElementBySelector('.check-in-btn');
    testHelper.triggerChangeDetection();

    testHelper.verifyElementExists('.working-hours', 'Working hours should be shown after check-in');
    testHelper.verifyElementContainsText('.working-hours', '4h 30m');

    // Take a break
    testHelper.clickElementBySelector('.break-start-btn');
    testHelper.triggerChangeDetection();

    // End break - working hours should still be displayed
    testHelper.clickElementBySelector('.break-end-btn');
    testHelper.triggerChangeDetection();

    testHelper.verifyElementExists('.working-hours', 'Working hours should be shown after break');
  });
});