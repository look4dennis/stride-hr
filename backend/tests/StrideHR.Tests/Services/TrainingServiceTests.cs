using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models;
using StrideHR.Infrastructure.Services;
using Xunit;
using FluentAssertions;

namespace StrideHR.Tests.Services;

public class TrainingServiceTests
{
    private readonly Mock<ITrainingModuleRepository> _mockModuleRepository;
    private readonly Mock<ITrainingAssignmentRepository> _mockAssignmentRepository;
    private readonly Mock<IAssessmentRepository> _mockAssessmentRepository;
    private readonly Mock<IAssessmentAttemptRepository> _mockAttemptRepository;
    private readonly Mock<ICertificationRepository> _mockCertificationRepository;
    private readonly Mock<IRepository<Employee>> _mockEmployeeRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<TrainingService>> _mockLogger;
    private readonly TrainingService _trainingService;

    public TrainingServiceTests()
    {
        _mockModuleRepository = new Mock<ITrainingModuleRepository>();
        _mockAssignmentRepository = new Mock<ITrainingAssignmentRepository>();
        _mockAssessmentRepository = new Mock<IAssessmentRepository>();
        _mockAttemptRepository = new Mock<IAssessmentAttemptRepository>();
        _mockCertificationRepository = new Mock<ICertificationRepository>();
        _mockEmployeeRepository = new Mock<IRepository<Employee>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<TrainingService>>();

        _trainingService = new TrainingService(
            _mockModuleRepository.Object,
            _mockAssignmentRepository.Object,
            _mockAssessmentRepository.Object,
            _mockAttemptRepository.Object,
            _mockCertificationRepository.Object,
            _mockEmployeeRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    #region Training Module Tests

    [Fact]
    public async Task CreateTrainingModuleAsync_ValidData_ReturnsTrainingModuleDto()
    {
        // Arrange
        var dto = new CreateTrainingModuleDto
        {
            Title = "Test Training Module",
            Description = "Test Description",
            Content = "Test Content",
            Type = TrainingType.OnlineModule,
            Level = TrainingLevel.Beginner,
            EstimatedDurationMinutes = 60,
            IsMandatory = true
        };

        var trainingModule = new TrainingModule
        {
            Id = 1,
            Title = dto.Title,
            Description = dto.Description,
            Content = dto.Content,
            Type = dto.Type,
            Level = dto.Level,
            EstimatedDurationMinutes = dto.EstimatedDurationMinutes,
            IsMandatory = dto.IsMandatory,
            CreatedBy = "1",
            CreatedAt = DateTime.UtcNow
        };

        var expectedDto = new TrainingModuleDto
        {
            Id = 1,
            Title = dto.Title,
            Description = dto.Description,
            Content = dto.Content,
            Type = dto.Type,
            Level = dto.Level,
            EstimatedDurationMinutes = dto.EstimatedDurationMinutes,
            IsMandatory = dto.IsMandatory,
            CreatedBy = 1,
            CreatedAt = trainingModule.CreatedAt
        };

        _mockMapper.Setup(m => m.Map<TrainingModule>(dto)).Returns(trainingModule);
        _mockModuleRepository.Setup(r => r.AddAsync(It.IsAny<TrainingModule>())).ReturnsAsync(trainingModule);
        _mockModuleRepository.Setup(r => r.GetByIdAsync(trainingModule.Id)).ReturnsAsync(trainingModule);
        _mockMapper.Setup(m => m.Map<TrainingModuleDto>(trainingModule)).Returns(expectedDto);

        // Act
        var result = await _trainingService.CreateTrainingModuleAsync(dto, 1);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(dto.Title);
        result.Description.Should().Be(dto.Description);
        result.Type.Should().Be(dto.Type);
        result.Level.Should().Be(dto.Level);
        result.EstimatedDurationMinutes.Should().Be(dto.EstimatedDurationMinutes);
        result.IsMandatory.Should().Be(dto.IsMandatory);

        _mockModuleRepository.Verify(r => r.AddAsync(It.IsAny<TrainingModule>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTrainingModuleAsync_ExistingModule_ReturnsTrainingModuleDto()
    {
        // Arrange
        var moduleId = 1;
        var trainingModule = new TrainingModule
        {
            Id = moduleId,
            Title = "Test Module",
            Description = "Test Description",
            Type = TrainingType.OnlineModule,
            Level = TrainingLevel.Beginner,
            EstimatedDurationMinutes = 60,
            IsMandatory = true,
            IsActive = true
        };

        var expectedDto = new TrainingModuleDto
        {
            Id = moduleId,
            Title = trainingModule.Title,
            Description = trainingModule.Description,
            Type = trainingModule.Type,
            Level = trainingModule.Level,
            EstimatedDurationMinutes = trainingModule.EstimatedDurationMinutes,
            IsMandatory = trainingModule.IsMandatory,
            IsActive = trainingModule.IsActive
        };

        _mockModuleRepository.Setup(r => r.GetByIdAsync(moduleId)).ReturnsAsync(trainingModule);
        _mockMapper.Setup(m => m.Map<TrainingModuleDto>(trainingModule)).Returns(expectedDto);

        // Act
        var result = await _trainingService.GetTrainingModuleAsync(moduleId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(moduleId);
        result.Title.Should().Be(trainingModule.Title);
        result.IsActive.Should().BeTrue();

        _mockModuleRepository.Verify(r => r.GetByIdAsync(moduleId), Times.Once);
    }

    [Fact]
    public async Task GetTrainingModuleAsync_NonExistentModule_ReturnsNull()
    {
        // Arrange
        var moduleId = 999;
        _mockModuleRepository.Setup(r => r.GetByIdAsync(moduleId)).ReturnsAsync((TrainingModule?)null);

        // Act
        var result = await _trainingService.GetTrainingModuleAsync(moduleId);

        // Assert
        result.Should().BeNull();
        _mockModuleRepository.Verify(r => r.GetByIdAsync(moduleId), Times.Once);
    }

    [Fact]
    public async Task DeleteTrainingModuleAsync_ModuleWithActiveAssignments_ThrowsInvalidOperationException()
    {
        // Arrange
        var moduleId = 1;
        var trainingModule = new TrainingModule { Id = moduleId, IsActive = true };
        var activeAssignments = new List<TrainingAssignment>
        {
            new TrainingAssignment { Id = 1, Status = TrainingAssignmentStatus.InProgress }
        };

        _mockModuleRepository.Setup(r => r.GetByIdAsync(moduleId)).ReturnsAsync(trainingModule);
        _mockAssignmentRepository.Setup(r => r.GetAssignmentsByModuleAsync(moduleId))
            .ReturnsAsync(activeAssignments);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _trainingService.DeleteTrainingModuleAsync(moduleId));

        _mockModuleRepository.Verify(r => r.UpdateAsync(It.IsAny<TrainingModule>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region Assessment Tests

    [Fact]
    public async Task CreateAssessmentAsync_ValidData_ReturnsAssessmentDto()
    {
        // Arrange
        var dto = new CreateAssessmentDto
        {
            TrainingModuleId = 1,
            Title = "Test Assessment",
            Description = "Test Description",
            Type = AssessmentType.Quiz,
            TimeLimit = 60,
            PassingScore = 70,
            MaxAttempts = 3,
            Questions = new List<CreateAssessmentQuestionDto>
            {
                new CreateAssessmentQuestionDto
                {
                    QuestionText = "What is 2+2?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string> { "3", "4", "5" },
                    CorrectAnswers = new List<string> { "4" },
                    Points = 1
                }
            }
        };

        var assessment = new Assessment
        {
            Id = 1,
            TrainingModuleId = dto.TrainingModuleId,
            Title = dto.Title,
            Description = dto.Description,
            Type = dto.Type,
            TimeLimit = dto.TimeLimit,
            PassingScore = dto.PassingScore,
            MaxAttempts = dto.MaxAttempts,
            CreatedBy = "1",
            CreatedAt = DateTime.UtcNow
        };

        var expectedDto = new AssessmentDto
        {
            Id = 1,
            TrainingModuleId = dto.TrainingModuleId,
            Title = dto.Title,
            Description = dto.Description,
            Type = dto.Type,
            TimeLimit = dto.TimeLimit,
            PassingScore = dto.PassingScore,
            MaxAttempts = dto.MaxAttempts
        };

        _mockMapper.Setup(m => m.Map<Assessment>(dto)).Returns(assessment);
        _mockAssessmentRepository.Setup(r => r.AddAsync(It.IsAny<Assessment>())).ReturnsAsync(assessment);
        _mockMapper.Setup(m => m.Map<AssessmentDto>(assessment)).Returns(expectedDto);

        // Act
        var result = await _trainingService.CreateAssessmentAsync(dto, 1);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(dto.Title);
        result.Type.Should().Be(dto.Type);
        result.PassingScore.Should().Be(dto.PassingScore);

        _mockAssessmentRepository.Verify(r => r.AddAsync(It.IsAny<Assessment>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task StartAssessmentAttemptAsync_ValidData_ReturnsAttemptId()
    {
        // Arrange
        var assessmentId = 1;
        var employeeId = 1;
        var trainingProgressId = 1;

        var assessment = new Assessment
        {
            Id = assessmentId,
            Title = "Test Assessment",
            Questions = new List<AssessmentQuestion>
            {
                new AssessmentQuestion { Id = 1, Points = 1 },
                new AssessmentQuestion { Id = 2, Points = 2 }
            }
        };

        var attempt = new AssessmentAttempt
        {
            Id = 1,
            AssessmentId = assessmentId,
            EmployeeId = employeeId,
            TrainingProgressId = trainingProgressId,
            AttemptNumber = 1,
            Status = AssessmentAttemptStatus.InProgress
        };

        _mockAssessmentRepository.Setup(r => r.GetAssessmentWithQuestionsAsync(assessmentId))
            .ReturnsAsync(assessment);
        _mockAssessmentRepository.Setup(r => r.CanEmployeeRetakeAssessmentAsync(assessmentId, employeeId))
            .ReturnsAsync(true);
        _mockAttemptRepository.Setup(r => r.GetAttemptCountAsync(assessmentId, employeeId))
            .ReturnsAsync(0);
        _mockAttemptRepository.Setup(r => r.AddAsync(It.IsAny<AssessmentAttempt>()))
            .Callback<AssessmentAttempt>(a => a.Id = 1)
            .ReturnsAsync(attempt);

        // Act
        var result = await _trainingService.StartAssessmentAttemptAsync(assessmentId, employeeId, trainingProgressId);

        // Assert
        result.Should().Be(1);
        _mockAttemptRepository.Verify(r => r.AddAsync(It.IsAny<AssessmentAttempt>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task StartAssessmentAttemptAsync_CannotRetake_ThrowsInvalidOperationException()
    {
        // Arrange
        var assessmentId = 1;
        var employeeId = 1;
        var trainingProgressId = 1;

        var assessment = new Assessment { Id = assessmentId };

        _mockAssessmentRepository.Setup(r => r.GetAssessmentWithQuestionsAsync(assessmentId))
            .ReturnsAsync(assessment);
        _mockAssessmentRepository.Setup(r => r.CanEmployeeRetakeAssessmentAsync(assessmentId, employeeId))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _trainingService.StartAssessmentAttemptAsync(assessmentId, employeeId, trainingProgressId));

        _mockAttemptRepository.Verify(r => r.AddAsync(It.IsAny<AssessmentAttempt>()), Times.Never);
    }

    #endregion

    #region Certification Tests

    [Fact]
    public async Task IssueCertificationAsync_ValidData_ReturnsCertificationDto()
    {
        // Arrange
        var dto = new CreateCertificationDto
        {
            TrainingModuleId = 1,
            EmployeeId = 1,
            CertificationName = "Test Certification",
            Score = 85
        };

        var certification = new Certification
        {
            Id = 1,
            TrainingModuleId = dto.TrainingModuleId,
            EmployeeId = dto.EmployeeId,
            CertificationName = dto.CertificationName,
            CertificationNumber = "CERT-2024-000001",
            IssuedBy = 1,
            IssuedDate = DateTime.UtcNow,
            Score = dto.Score,
            Status = CertificationStatus.Active
        };

        var expectedDto = new CertificationDto
        {
            Id = 1,
            TrainingModuleId = dto.TrainingModuleId,
            EmployeeId = dto.EmployeeId,
            CertificationName = dto.CertificationName,
            CertificationNumber = "CERT-2024-000001",
            Status = CertificationStatus.Active,
            Score = dto.Score
        };

        _mockCertificationRepository.Setup(r => r.GenerateCertificationNumberAsync())
            .ReturnsAsync("CERT-2024-000001");
        _mockMapper.Setup(m => m.Map<Certification>(dto)).Returns(certification);
        _mockCertificationRepository.Setup(r => r.AddAsync(It.IsAny<Certification>()))
            .ReturnsAsync(certification);
        _mockMapper.Setup(m => m.Map<CertificationDto>(certification)).Returns(expectedDto);

        // Act
        var result = await _trainingService.IssueCertificationAsync(dto, 1);

        // Assert
        result.Should().NotBeNull();
        result.CertificationName.Should().Be(dto.CertificationName);
        result.CertificationNumber.Should().Be("CERT-2024-000001");
        result.Status.Should().Be(CertificationStatus.Active);

        _mockCertificationRepository.Verify(r => r.AddAsync(It.IsAny<Certification>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RevokeCertificationAsync_ExistingCertification_ReturnsTrue()
    {
        // Arrange
        var certificationId = 1;
        var reason = "Policy violation";
        var certification = new Certification
        {
            Id = certificationId,
            Status = CertificationStatus.Active
        };

        _mockCertificationRepository.Setup(r => r.GetByIdAsync(certificationId))
            .ReturnsAsync(certification);

        // Act
        var result = await _trainingService.RevokeCertificationAsync(certificationId, reason);

        // Assert
        result.Should().BeTrue();
        certification.Status.Should().Be(CertificationStatus.Revoked);
        certification.Notes.Should().Be(reason);

        _mockCertificationRepository.Verify(r => r.UpdateAsync(certification), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region Training Assignment Tests

    [Fact]
    public async Task AssignTrainingToEmployeesAsync_ValidData_ReturnsAssignmentDtos()
    {
        // Arrange
        var dto = new CreateTrainingAssignmentDto
        {
            TrainingModuleId = 1,
            EmployeeIds = new List<int> { 1, 2 },
            DueDate = DateTime.Today.AddDays(30),
            Notes = "Complete by end of month"
        };

        var assignments = new List<TrainingAssignment>
        {
            new TrainingAssignment
            {
                Id = 1,
                TrainingModuleId = dto.TrainingModuleId,
                EmployeeId = 1,
                AssignedBy = 1,
                DueDate = dto.DueDate,
                Notes = dto.Notes,
                Status = TrainingAssignmentStatus.Assigned
            },
            new TrainingAssignment
            {
                Id = 2,
                TrainingModuleId = dto.TrainingModuleId,
                EmployeeId = 2,
                AssignedBy = 1,
                DueDate = dto.DueDate,
                Notes = dto.Notes,
                Status = TrainingAssignmentStatus.Assigned
            }
        };

        var expectedDtos = assignments.Select(a => new TrainingAssignmentDto
        {
            Id = a.Id,
            TrainingModuleId = a.TrainingModuleId,
            EmployeeId = a.EmployeeId,
            Status = a.Status
        }).ToList();

        _mockAssignmentRepository.Setup(r => r.IsEmployeeAssignedToModuleAsync(It.IsAny<int>(), dto.TrainingModuleId))
            .ReturnsAsync(false);
        _mockMapper.Setup(m => m.Map<IEnumerable<TrainingAssignmentDto>>(It.IsAny<IEnumerable<TrainingAssignment>>()))
            .Returns(expectedDtos);

        // Act
        var result = await _trainingService.AssignTrainingToEmployeesAsync(dto, 1);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(r => r.Status == TrainingAssignmentStatus.Assigned).Should().BeTrue();

        _mockAssignmentRepository.Verify(r => r.AddAsync(It.IsAny<TrainingAssignment>()), Times.Exactly(2));
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    #endregion
         }
