import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { of, throwError } from 'rxjs';

import { PerformanceReviewComponent } from './performance-review.component';
import { PerformanceService } from '../../../services/performance.service';
import { EmployeeService } from '../../../services/employee.service';
import { PerformanceReview, PerformanceReviewStatus, GoalStatus } from '../../../models/performance.models';
import { Employee } from '../../../models/employee.models';

describe('PerformanceReviewComponent', () => {
    let component: PerformanceReviewComponent;
    let fixture: ComponentFixture<PerformanceReviewComponent>;
    let mockPerformanceService: jasmine.SpyObj<PerformanceService>;
    let mockEmployeeService: jasmine.SpyObj<EmployeeService>;
    let mockRouter: jasmine.SpyObj<Router>;
    let mockModalService: jasmine.SpyObj<NgbModal>;

    const mockEmployees: Employee[] = [
        {
            id: 1,
            employeeId: 'EMP001',
            firstName: 'John',
            lastName: 'Doe',
            email: 'john.doe@example.com',
            phone: '1234567890',
            designation: 'Developer',
            department: 'IT',
            branchId: 1,
            basicSalary: 50000,
            dateOfBirth: '1990-01-01',
            joiningDate: '2020-01-01',
            status: 'Active' as any,
            createdAt: '2020-01-01T00:00:00Z'
        }
    ];

    const mockReviews: PerformanceReview[] = [
        {
            id: 1,
            employeeId: 1,
            reviewerId: 2,
            reviewPeriod: 'Q1 2024',
            startDate: new Date('2024-01-01'),
            endDate: new Date('2024-03-31'),
            status: PerformanceReviewStatus.InProgress,
            overallRating: 4,
            goals: [
                {
                    id: 1,
                    performanceReviewId: 1,
                    title: 'Goal 1',
                    description: 'Description 1',
                    targetValue: 'Target 1',
                    weight: 50,
                    status: GoalStatus.Completed,
                    rating: 4,
                    comments: 'Good progress'
                }
            ],
            feedback: 'Good performance',
            employeeSelfAssessment: 'Self assessment',
            managerComments: 'Manager comments',
            createdAt: new Date(),
            updatedAt: new Date(),
            employee: {
                id: 1,
                firstName: 'John',
                lastName: 'Doe',
                employeeId: 'EMP001',
                designation: 'Developer'
            }
        }
    ];

    beforeEach(async () => {
        const performanceServiceSpy = jasmine.createSpyObj('PerformanceService', [
            'getPerformanceReviews',
            'getPerformanceReview',
            'createPerformanceReview',
            'updatePerformanceReview',
            'submitPerformanceReview'
        ]);
        const employeeServiceSpy = jasmine.createSpyObj('EmployeeService', ['getEmployees']);
        const routerSpy = jasmine.createSpyObj('Router', ['navigate']);
        const modalServiceSpy = jasmine.createSpyObj('NgbModal', ['open']);

        await TestBed.configureTestingModule({
            imports: [PerformanceReviewComponent, ReactiveFormsModule],
            providers: [
                { provide: PerformanceService, useValue: performanceServiceSpy },
                { provide: EmployeeService, useValue: employeeServiceSpy },
                { provide: Router, useValue: routerSpy },
                { provide: NgbModal, useValue: modalServiceSpy }
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(PerformanceReviewComponent);
        component = fixture.componentInstance;
        mockPerformanceService = TestBed.inject(PerformanceService) as jasmine.SpyObj<PerformanceService>;
        mockEmployeeService = TestBed.inject(EmployeeService) as jasmine.SpyObj<EmployeeService>;
        mockRouter = TestBed.inject(Router) as jasmine.SpyObj<Router>;
        mockModalService = TestBed.inject(NgbModal) as jasmine.SpyObj<NgbModal>;
    });

    beforeEach(() => {
        mockEmployeeService.getEmployees.and.returnValue(of({ items: mockEmployees, totalCount: 1, page: 1, pageSize: 10, totalPages: 1 }));
        mockPerformanceService.getPerformanceReviews.and.returnValue(of(mockReviews));
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load employees and reviews on init', () => {
        component.ngOnInit();

        expect(mockEmployeeService.getEmployees).toHaveBeenCalled();
        expect(mockPerformanceService.getPerformanceReviews).toHaveBeenCalled();
        expect(component.employees).toEqual(mockEmployees);
        expect(component.reviews).toEqual(mockReviews);
    });

    it('should create review form with default goal', () => {
        const form = component.createReviewForm();

        expect(form.get('employeeId')).toBeTruthy();
        expect(form.get('reviewPeriod')).toBeTruthy();
        expect(form.get('startDate')).toBeTruthy();
        expect(form.get('endDate')).toBeTruthy();
        expect(form.get('goals')).toBeTruthy();
        expect(component.goalsArray.length).toBe(1);
    });

    it('should add and remove goals', () => {
        component.addGoal();
        expect(component.goalsArray.length).toBe(2);

        component.removeGoal(1);
        expect(component.goalsArray.length).toBe(1);
    });

    it('should filter reviews based on search criteria', () => {
        component.selectedEmployeeId = '1';
        component.selectedStatus = 'InProgress';
        component.searchPeriod = 'Q1';

        component.loadReviews();

        expect(mockPerformanceService.getPerformanceReviews).toHaveBeenCalledWith(1, 'InProgress');
    });

    it('should clear filters', () => {
        component.selectedEmployeeId = '1';
        component.selectedStatus = 'InProgress';
        component.searchPeriod = 'Q1';

        component.clearFilters();

        expect(component.selectedEmployeeId).toBe('');
        expect(component.selectedStatus).toBe('');
        expect(component.searchPeriod).toBe('');
        expect(mockPerformanceService.getPerformanceReviews).toHaveBeenCalled();
    });

    it('should open create modal', () => {
        const mockModalRef = { close: jasmine.createSpy(), dismiss: jasmine.createSpy() };
        mockModalService.open.and.returnValue(mockModalRef as any);

        component.openCreateModal();

        expect(component.isEditMode).toBeFalse();
        expect(component.currentReview).toBeNull();
        expect(mockModalService.open).toHaveBeenCalled();
    });

    it('should populate form for editing', () => {
        const review = mockReviews[0];
        component.editReview(review);

        expect(component.isEditMode).toBeTrue();
        expect(component.currentReview).toEqual(review);
        expect(component.reviewForm.get('employeeId')?.value).toBe(review.employeeId);
        expect(component.reviewForm.get('reviewPeriod')?.value).toBe(review.reviewPeriod);
    });

    it('should save new review', () => {
        const mockModalRef = { close: jasmine.createSpy(), dismiss: jasmine.createSpy() };
        const newReview = { ...mockReviews[0], id: 2 };

        mockPerformanceService.createPerformanceReview.and.returnValue(of(newReview));

        component.reviewForm.patchValue({
            employeeId: 1,
            reviewPeriod: 'Q2 2024',
            startDate: '2024-04-01',
            endDate: '2024-06-30'
        });

        component.saveReview(mockModalRef);

        expect(mockPerformanceService.createPerformanceReview).toHaveBeenCalled();
        expect(mockModalRef.close).toHaveBeenCalled();
    });

    it('should handle save review error', () => {
        const mockModalRef = { close: jasmine.createSpy(), dismiss: jasmine.createSpy() };
        const consoleSpy = spyOn(console, 'error');

        mockPerformanceService.createPerformanceReview.and.returnValue(throwError('Error'));

        component.reviewForm.patchValue({
            employeeId: 1,
            reviewPeriod: 'Q2 2024',
            startDate: '2024-04-01',
            endDate: '2024-06-30'
        });

        component.saveReview(mockModalRef);

        expect(consoleSpy).toHaveBeenCalledWith('Error saving review:', 'Error');
        expect(component.loading).toBeFalse();
    });

    it('should navigate to view review', () => {
        const review = mockReviews[0];
        component.viewReview(review);

        expect(mockRouter.navigate).toHaveBeenCalledWith(['/performance/reviews', review.id]);
    });

    it('should submit review with confirmation', () => {
        const review = mockReviews[0];
        spyOn(window, 'confirm').and.returnValue(true);
        mockPerformanceService.submitPerformanceReview.and.returnValue(of(review));

        component.submitReview(review);

        expect(window.confirm).toHaveBeenCalled();
        expect(mockPerformanceService.submitPerformanceReview).toHaveBeenCalledWith(review.id);
    });

    it('should not submit review without confirmation', () => {
        const review = mockReviews[0];
        spyOn(window, 'confirm').and.returnValue(false);

        component.submitReview(review);

        expect(window.confirm).toHaveBeenCalled();
        expect(mockPerformanceService.submitPerformanceReview).not.toHaveBeenCalled();
    });

    it('should get correct status badge class', () => {
        expect(component.getStatusBadgeClass(PerformanceReviewStatus.Draft)).toBe('bg-secondary');
        expect(component.getStatusBadgeClass(PerformanceReviewStatus.InProgress)).toBe('bg-primary');
        expect(component.getStatusBadgeClass(PerformanceReviewStatus.Completed)).toBe('bg-success');
    });

    it('should calculate goals progress correctly', () => {
        const goals = [
            { status: GoalStatus.Completed },
            { status: GoalStatus.InProgress },
            { status: GoalStatus.Exceeded }
        ];

        const progress = component.getGoalsProgress(goals);
        expect(progress).toBe(67); // 2 out of 3 completed/exceeded
    });

    it('should count completed goals correctly', () => {
        const goals = [
            { status: GoalStatus.Completed },
            { status: GoalStatus.InProgress },
            { status: GoalStatus.Exceeded }
        ];

        const completed = component.getCompletedGoals(goals);
        expect(completed).toBe(2); // completed + exceeded
    });

    it('should get correct progress bar class', () => {
        expect(component.getProgressBarClass(85)).toBe('bg-success');
        expect(component.getProgressBarClass(65)).toBe('bg-warning');
        expect(component.getProgressBarClass(45)).toBe('bg-danger');
    });

    it('should handle empty goals array', () => {
        expect(component.getGoalsProgress([])).toBe(0);
        expect(component.getCompletedGoals([])).toBe(0);
        expect(component.getGoalsProgress(null as any)).toBe(0);
        expect(component.getCompletedGoals(null as any)).toBe(0);
    });
});