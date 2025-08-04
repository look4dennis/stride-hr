import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PerformanceService } from './performance.service';
import { 
  PerformanceReview, 
  PIP, 
  TrainingModule, 
  EmployeeTraining, 
  Certification,
  CreatePerformanceReviewDto,
  CreatePIPDto,
  CreateTrainingModuleDto,
  EnrollEmployeeDto,
  PerformanceReviewStatus,
  PIPStatus,
  TrainingDifficulty,
  TrainingStatus
} from '../models/performance.models';

describe('PerformanceService', () => {
  let service: PerformanceService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [PerformanceService]
    });
    service = TestBed.inject(PerformanceService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('Performance Review Methods', () => {
    it('should get performance reviews', () => {
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
          goals: [],
          feedback: 'Good performance',
          employeeSelfAssessment: 'Self assessment',
          managerComments: 'Manager comments',
          createdAt: new Date(),
          updatedAt: new Date()
        }
      ];

      service.getPerformanceReviews().subscribe(reviews => {
        expect(reviews).toEqual(mockReviews);
        expect(reviews.length).toBe(1);
        expect(reviews[0].reviewPeriod).toBe('Q1 2024');
      });

      const req = httpMock.expectOne('/api/performance/reviews');
      expect(req.request.method).toBe('GET');
      req.flush(mockReviews);
    });

    it('should get performance reviews with filters', () => {
      const employeeId = 1;
      const status = 'InProgress';

      service.getPerformanceReviews(employeeId, status).subscribe();

      const req = httpMock.expectOne('/api/performance/reviews?employeeId=1&status=InProgress');
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });

    it('should get performance review by id', () => {
      const mockReview: PerformanceReview = {
        id: 1,
        employeeId: 1,
        reviewerId: 2,
        reviewPeriod: 'Q1 2024',
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-03-31'),
        status: PerformanceReviewStatus.InProgress,
        overallRating: 4,
        goals: [],
        feedback: 'Good performance',
        employeeSelfAssessment: 'Self assessment',
        managerComments: 'Manager comments',
        createdAt: new Date(),
        updatedAt: new Date()
      };

      service.getPerformanceReview(1).subscribe(review => {
        expect(review).toEqual(mockReview);
        expect(review.id).toBe(1);
      });

      const req = httpMock.expectOne('/api/performance/reviews/1');
      expect(req.request.method).toBe('GET');
      req.flush(mockReview);
    });

    it('should create performance review', () => {
      const createDto: CreatePerformanceReviewDto = {
        employeeId: 1,
        reviewPeriod: 'Q1 2024',
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-03-31'),
        goals: [
          {
            title: 'Goal 1',
            description: 'Description 1',
            targetValue: 'Target 1',
            weight: 50
          }
        ]
      };

      const mockResponse: PerformanceReview = {
        id: 1,
        employeeId: 1,
        reviewerId: 2,
        reviewPeriod: 'Q1 2024',
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-03-31'),
        status: PerformanceReviewStatus.Draft,
        overallRating: 0,
        goals: [],
        feedback: '',
        employeeSelfAssessment: '',
        managerComments: '',
        createdAt: new Date(),
        updatedAt: new Date()
      };

      service.createPerformanceReview(createDto).subscribe(review => {
        expect(review).toEqual(mockResponse);
        expect(review.employeeId).toBe(1);
      });

      const req = httpMock.expectOne('/api/performance/reviews');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(createDto);
      req.flush(mockResponse);
    });

    it('should submit performance review', () => {
      const mockResponse: PerformanceReview = {
        id: 1,
        employeeId: 1,
        reviewerId: 2,
        reviewPeriod: 'Q1 2024',
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-03-31'),
        status: PerformanceReviewStatus.EmployeeReview,
        overallRating: 0,
        goals: [],
        feedback: '',
        employeeSelfAssessment: '',
        managerComments: '',
        createdAt: new Date(),
        updatedAt: new Date()
      };

      service.submitPerformanceReview(1).subscribe(review => {
        expect(review.status).toBe(PerformanceReviewStatus.EmployeeReview);
      });

      const req = httpMock.expectOne('/api/performance/reviews/1/submit');
      expect(req.request.method).toBe('POST');
      req.flush(mockResponse);
    });
  });

  describe('PIP Methods', () => {
    it('should get PIPs', () => {
      const mockPIPs: PIP[] = [
        {
          id: 1,
          employeeId: 1,
          managerId: 2,
          title: 'Performance Improvement Plan',
          description: 'Improve communication skills',
          startDate: new Date('2024-01-01'),
          endDate: new Date('2024-06-30'),
          status: PIPStatus.Active,
          improvementAreas: [],
          milestones: [],
          supportResources: 'Training materials',
          createdAt: new Date(),
          updatedAt: new Date()
        }
      ];

      service.getPIPs().subscribe(pips => {
        expect(pips).toEqual(mockPIPs);
        expect(pips.length).toBe(1);
      });

      const req = httpMock.expectOne('/api/performance/pips');
      expect(req.request.method).toBe('GET');
      req.flush(mockPIPs);
    });

    it('should create PIP', () => {
      const createDto: CreatePIPDto = {
        employeeId: 1,
        title: 'Performance Improvement Plan',
        description: 'Improve communication skills',
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-06-30'),
        supportResources: 'Training materials',
        improvementAreas: [
          {
            area: 'Communication',
            currentState: 'Poor communication',
            expectedState: 'Clear communication',
            actionPlan: 'Take communication course'
          }
        ],
        milestones: [
          {
            title: 'Complete training',
            description: 'Complete communication training',
            dueDate: new Date('2024-03-31')
          }
        ]
      };

      const mockResponse: PIP = {
        id: 1,
        employeeId: 1,
        managerId: 2,
        title: 'Performance Improvement Plan',
        description: 'Improve communication skills',
        startDate: new Date('2024-01-01'),
        endDate: new Date('2024-06-30'),
        status: PIPStatus.Active,
        improvementAreas: [],
        milestones: [],
        supportResources: 'Training materials',
        createdAt: new Date(),
        updatedAt: new Date()
      };

      service.createPIP(createDto).subscribe(pip => {
        expect(pip).toEqual(mockResponse);
        expect(pip.employeeId).toBe(1);
      });

      const req = httpMock.expectOne('/api/performance/pips');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(createDto);
      req.flush(mockResponse);
    });
  });

  describe('Training Module Methods', () => {
    it('should get training modules', () => {
      const mockModules: TrainingModule[] = [
        {
          id: 1,
          title: 'Communication Skills',
          description: 'Learn effective communication',
          category: 'Soft Skills',
          duration: 120,
          difficulty: TrainingDifficulty.Beginner,
          content: 'Training content',
          materials: [],
          assessments: [],
          prerequisites: [],
          isActive: true,
          createdBy: 1,
          createdAt: new Date(),
          updatedAt: new Date()
        }
      ];

      service.getTrainingModules().subscribe(modules => {
        expect(modules).toEqual(mockModules);
        expect(modules.length).toBe(1);
      });

      const req = httpMock.expectOne('/api/performance/training/modules');
      expect(req.request.method).toBe('GET');
      req.flush(mockModules);
    });

    it('should create training module', () => {
      const createDto: CreateTrainingModuleDto = {
        title: 'Communication Skills',
        description: 'Learn effective communication',
        category: 'Soft Skills',
        duration: 120,
        difficulty: TrainingDifficulty.Beginner,
        content: 'Training content',
        prerequisites: []
      };

      const mockResponse: TrainingModule = {
        id: 1,
        title: 'Communication Skills',
        description: 'Learn effective communication',
        category: 'Soft Skills',
        duration: 120,
        difficulty: TrainingDifficulty.Beginner,
        content: 'Training content',
        materials: [],
        assessments: [],
        prerequisites: [],
        isActive: true,
        createdBy: 1,
        createdAt: new Date(),
        updatedAt: new Date()
      };

      service.createTrainingModule(createDto).subscribe(module => {
        expect(module).toEqual(mockResponse);
        expect(module.title).toBe('Communication Skills');
      });

      const req = httpMock.expectOne('/api/performance/training/modules');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(createDto);
      req.flush(mockResponse);
    });
  });

  describe('Employee Training Methods', () => {
    it('should get employee trainings', () => {
      const mockTrainings: EmployeeTraining[] = [
        {
          id: 1,
          employeeId: 1,
          trainingModuleId: 1,
          enrolledDate: new Date(),
          status: TrainingStatus.InProgress,
          progress: 50,
          attempts: 1,
          certificateIssued: false
        }
      ];

      service.getEmployeeTrainings().subscribe(trainings => {
        expect(trainings).toEqual(mockTrainings);
        expect(trainings.length).toBe(1);
      });

      const req = httpMock.expectOne('/api/performance/training/enrollments');
      expect(req.request.method).toBe('GET');
      req.flush(mockTrainings);
    });

    it('should enroll employee', () => {
      const enrollDto: EnrollEmployeeDto = {
        employeeId: 1,
        trainingModuleId: 1
      };

      const mockResponse: EmployeeTraining = {
        id: 1,
        employeeId: 1,
        trainingModuleId: 1,
        enrolledDate: new Date(),
        status: TrainingStatus.NotStarted,
        progress: 0,
        attempts: 0,
        certificateIssued: false
      };

      service.enrollEmployee(enrollDto).subscribe(training => {
        expect(training).toEqual(mockResponse);
        expect(training.employeeId).toBe(1);
      });

      const req = httpMock.expectOne('/api/performance/training/enroll');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(enrollDto);
      req.flush(mockResponse);
    });
  });

  describe('Certification Methods', () => {
    it('should get certifications', () => {
      const mockCertifications: Certification[] = [
        {
          id: 1,
          employeeId: 1,
          trainingModuleId: 1,
          certificateNumber: 'CERT-001',
          issuedDate: new Date(),
          score: 85,
          certificateUrl: 'http://example.com/cert.pdf',
          isValid: true
        }
      ];

      service.getCertifications().subscribe(certifications => {
        expect(certifications).toEqual(mockCertifications);
        expect(certifications.length).toBe(1);
      });

      const req = httpMock.expectOne('/api/performance/certifications');
      expect(req.request.method).toBe('GET');
      req.flush(mockCertifications);
    });

    it('should download certificate', () => {
      const mockBlob = new Blob(['certificate content'], { type: 'application/pdf' });

      service.downloadCertificate(1).subscribe(blob => {
        expect(blob).toEqual(mockBlob);
      });

      const req = httpMock.expectOne('/api/performance/certifications/1/download');
      expect(req.request.method).toBe('GET');
      expect(req.request.responseType).toBe('blob');
      req.flush(mockBlob);
    });
  });

  describe('Analytics Methods', () => {
    it('should get performance analytics', () => {
      const mockAnalytics = {
        totalReviews: 10,
        averageRating: 4.2,
        completedGoals: 85
      };

      service.getPerformanceAnalytics().subscribe(analytics => {
        expect(analytics).toEqual(mockAnalytics);
      });

      const req = httpMock.expectOne('/api/performance/analytics');
      expect(req.request.method).toBe('GET');
      req.flush(mockAnalytics);
    });

    it('should get training analytics', () => {
      const mockAnalytics = {
        totalEnrollments: 50,
        completionRate: 75,
        averageScore: 82
      };

      service.getTrainingAnalytics().subscribe(analytics => {
        expect(analytics).toEqual(mockAnalytics);
      });

      const req = httpMock.expectOne('/api/performance/training/analytics');
      expect(req.request.method).toBe('GET');
      req.flush(mockAnalytics);
    });
  });
});