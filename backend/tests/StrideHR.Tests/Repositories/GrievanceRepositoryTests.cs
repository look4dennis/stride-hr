using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Models.Grievance;
using StrideHR.Infrastructure.Data;
using StrideHR.Infrastructure.Repositories;
using Xunit;

namespace StrideHR.Tests.Repositories;

public class GrievanceRepositoryTests : IDisposable
{
    private readonly StrideHRDbContext _context;
    private readonly GrievanceRepository _repository;

    public GrievanceRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<StrideHRDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StrideHRDbContext(options);
        _repository = new GrievanceRepository(_context);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        var branch = new Branch
        {
            Id = 1,
            Name = "Test Branch",
            Country = "US",
            Currency = "USD",
            TimeZone = "UTC",
            OrganizationId = 1
        };

        var employees = new List<Employee>
        {
            new Employee
            {
                Id = 1,
                EmployeeId = "EMP001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                BranchId = 1,
                Status = EmployeeStatus.Active
            },
            new Employee
            {
                Id = 2,
                EmployeeId = "EMP002",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                BranchId = 1,
                Status = EmployeeStatus.Active
            }
        };

        var grievances = new List<Grievance>
        {
            new Grievance
            {
                Id = 1,
                GrievanceNumber = "GRV-2025-01-0001",
                Title = "Test Grievance 1",
                Description = "Test Description 1",
                Category = GrievanceCategory.WorkplaceHarassment,
                Priority = GrievancePriority.High,
                Status = GrievanceStatus.Submitted,
                SubmittedById = 1,
                IsAnonymous = false,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                DueDate = DateTime.UtcNow.AddDays(5)
            },
            new Grievance
            {
                Id = 2,
                GrievanceNumber = "GRV-2025-01-0002",
                Title = "Test Grievance 2",
                Description = "Test Description 2",
                Category = GrievanceCategory.PayrollIssues,
                Priority = GrievancePriority.Medium,
                Status = GrievanceStatus.UnderReview,
                SubmittedById = 2,
                AssignedToId = 1,
                IsAnonymous = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                DueDate = DateTime.UtcNow.AddDays(-1) // Overdue
            },
            new Grievance
            {
                Id = 3,
                GrievanceNumber = "GRV-2025-01-0003",
                Title = "Test Grievance 3",
                Description = "Test Description 3",
                Category = GrievanceCategory.Discrimination,
                Priority = GrievancePriority.Critical,
                Status = GrievanceStatus.Resolved,
                SubmittedById = 1,
                AssignedToId = 2,
                ResolvedById = 2,
                IsEscalated = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                ResolvedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        _context.Branches.Add(branch);
        _context.Employees.AddRange(employees);
        _context.Grievances.AddRange(grievances);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByGrievanceNumberAsync_ExistingNumber_ReturnsGrievance()
    {
        // Arrange
        var grievanceNumber = "GRV-2025-01-0001";

        // Act
        var result = await _repository.GetByGrievanceNumberAsync(grievanceNumber);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(grievanceNumber, result.GrievanceNumber);
        Assert.Equal("Test Grievance 1", result.Title);
    }

    [Fact]
    public async Task GetByGrievanceNumberAsync_NonExistingNumber_ReturnsNull()
    {
        // Arrange
        var grievanceNumber = "GRV-2025-01-9999";

        // Act
        var result = await _repository.GetByGrievanceNumberAsync(grievanceNumber);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetWithDetailsAsync_ExistingId_ReturnsGrievanceWithDetails()
    {
        // Arrange
        var grievanceId = 1;

        // Act
        var result = await _repository.GetWithDetailsAsync(grievanceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(grievanceId, result.Id);
        Assert.NotNull(result.SubmittedBy);
        Assert.Equal("John", result.SubmittedBy.FirstName);
    }

    [Fact]
    public async Task SearchAsync_WithSearchTerm_ReturnsMatchingGrievances()
    {
        // Arrange
        var criteria = new GrievanceSearchCriteria
        {
            SearchTerm = "Test Grievance 1",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (grievances, totalCount) = await _repository.SearchAsync(criteria);

        // Assert
        Assert.Single(grievances);
        Assert.Equal(1, totalCount);
        Assert.Equal("Test Grievance 1", grievances[0].Title);
    }

    [Fact]
    public async Task SearchAsync_WithStatusFilter_ReturnsMatchingGrievances()
    {
        // Arrange
        var criteria = new GrievanceSearchCriteria
        {
            Status = GrievanceStatus.UnderReview,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (grievances, totalCount) = await _repository.SearchAsync(criteria);

        // Assert
        Assert.Single(grievances);
        Assert.Equal(1, totalCount);
        Assert.Equal(GrievanceStatus.UnderReview, grievances[0].Status);
    }

    [Fact]
    public async Task SearchAsync_WithCategoryFilter_ReturnsMatchingGrievances()
    {
        // Arrange
        var criteria = new GrievanceSearchCriteria
        {
            Category = GrievanceCategory.WorkplaceHarassment,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (grievances, totalCount) = await _repository.SearchAsync(criteria);

        // Assert
        Assert.Single(grievances);
        Assert.Equal(1, totalCount);
        Assert.Equal(GrievanceCategory.WorkplaceHarassment, grievances[0].Category);
    }

    [Fact]
    public async Task SearchAsync_WithPriorityFilter_ReturnsMatchingGrievances()
    {
        // Arrange
        var criteria = new GrievanceSearchCriteria
        {
            Priority = GrievancePriority.Critical,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (grievances, totalCount) = await _repository.SearchAsync(criteria);

        // Assert
        Assert.Single(grievances);
        Assert.Equal(1, totalCount);
        Assert.Equal(GrievancePriority.Critical, grievances[0].Priority);
    }

    [Fact]
    public async Task SearchAsync_WithAnonymousFilter_ReturnsMatchingGrievances()
    {
        // Arrange
        var criteria = new GrievanceSearchCriteria
        {
            IsAnonymous = true,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (grievances, totalCount) = await _repository.SearchAsync(criteria);

        // Assert
        Assert.Single(grievances);
        Assert.Equal(1, totalCount);
        Assert.True(grievances[0].IsAnonymous);
    }

    [Fact]
    public async Task SearchAsync_WithOverdueFilter_ReturnsOverdueGrievances()
    {
        // Arrange
        var criteria = new GrievanceSearchCriteria
        {
            IsOverdue = true,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var (grievances, totalCount) = await _repository.SearchAsync(criteria);

        // Assert
        Assert.Single(grievances);
        Assert.Equal(1, totalCount);
        Assert.True(grievances[0].DueDate < DateTime.UtcNow);
        Assert.NotEqual(GrievanceStatus.Resolved, grievances[0].Status);
        Assert.NotEqual(GrievanceStatus.Closed, grievances[0].Status);
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var criteria = new GrievanceSearchCriteria
        {
            PageNumber = 1,
            PageSize = 2,
            SortBy = "CreatedAt",
            SortDescending = false
        };

        // Act
        var (grievances, totalCount) = await _repository.SearchAsync(criteria);

        // Assert
        Assert.Equal(2, grievances.Count);
        Assert.Equal(3, totalCount);
        // Should return the oldest two grievances first
        Assert.True(grievances[0].CreatedAt <= grievances[1].CreatedAt);
    }

    [Fact]
    public async Task GetBySubmitterIdAsync_ExistingSubmitter_ReturnsGrievances()
    {
        // Arrange
        var submitterId = 1;

        // Act
        var result = await _repository.GetBySubmitterIdAsync(submitterId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, g => Assert.Equal(submitterId, g.SubmittedById));
    }

    [Fact]
    public async Task GetByAssignedToIdAsync_ExistingAssignee_ReturnsGrievances()
    {
        // Arrange
        var assignedToId = 1;

        // Act
        var result = await _repository.GetByAssignedToIdAsync(assignedToId);

        // Assert
        Assert.Single(result);
        Assert.Equal(assignedToId, result[0].AssignedToId);
    }

    [Fact]
    public async Task GetOverdueGrievancesAsync_ReturnsOverdueGrievances()
    {
        // Act
        var result = await _repository.GetOverdueGrievancesAsync();

        // Assert
        Assert.Single(result);
        Assert.True(result[0].DueDate < DateTime.UtcNow);
        Assert.NotEqual(GrievanceStatus.Resolved, result[0].Status);
        Assert.NotEqual(GrievanceStatus.Closed, result[0].Status);
    }

    [Fact]
    public async Task GetEscalatedGrievancesAsync_ReturnsEscalatedGrievances()
    {
        // Act
        var result = await _repository.GetEscalatedGrievancesAsync();

        // Assert
        Assert.Single(result);
        Assert.True(result[0].IsEscalated);
    }

    [Fact]
    public async Task GetAnonymousGrievancesAsync_ReturnsAnonymousGrievances()
    {
        // Act
        var result = await _repository.GetAnonymousGrievancesAsync();

        // Assert
        Assert.Single(result);
        Assert.True(result[0].IsAnonymous);
    }

    [Fact]
    public async Task GenerateGrievanceNumberAsync_ReturnsUniqueNumber()
    {
        // Act
        var result = await _repository.GenerateGrievanceNumberAsync();

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("GRV-", result);
        Assert.Contains(DateTime.UtcNow.Year.ToString(), result);
        Assert.Contains(DateTime.UtcNow.Month.ToString("D2"), result);
    }

    [Fact]
    public async Task GenerateGrievanceNumberAsync_GeneratesSequentialNumbers()
    {
        // Act
        var result1 = await _repository.GenerateGrievanceNumberAsync();
        
        // Add a grievance with the generated number
        var grievance = new Grievance
        {
            GrievanceNumber = result1,
            Title = "Test",
            Description = "Test",
            Category = GrievanceCategory.Other,
            Priority = GrievancePriority.Low,
            SubmittedById = 1
        };
        
        await _repository.AddAsync(grievance);
        await _repository.SaveChangesAsync();
        
        var result2 = await _repository.GenerateGrievanceNumberAsync();

        // Assert
        Assert.NotEqual(result1, result2);
        
        // Extract the sequence numbers
        var seq1 = int.Parse(result1.Split('-').Last());
        var seq2 = int.Parse(result2.Split('-').Last());
        
        Assert.Equal(seq1 + 1, seq2);
    }

    [Fact]
    public async Task GetAnalyticsAsync_ReturnsCorrectAnalytics()
    {
        // Act
        var result = await _repository.GetAnalyticsAsync();

        // Assert
        Assert.Equal(3, result.TotalGrievances);
        Assert.Equal(1, result.OpenGrievances); // Submitted + UnderReview
        Assert.Equal(1, result.ResolvedGrievances);
        Assert.Equal(0, result.ClosedGrievances);
        Assert.Equal(1, result.EscalatedGrievances);
        Assert.Equal(1, result.OverdueGrievances);
        Assert.Equal(1, result.AnonymousGrievances);
        
        // Check category stats
        Assert.Equal(3, result.CategoryStats.Count);
        var harassmentStats = result.CategoryStats.First(cs => cs.Category == GrievanceCategory.WorkplaceHarassment);
        Assert.Equal(1, harassmentStats.Count);
        Assert.Equal(33.33, Math.Round(harassmentStats.Percentage, 2));
        
        // Check priority stats
        Assert.Equal(3, result.PriorityStats.Count);
        var highPriorityStats = result.PriorityStats.First(ps => ps.Priority == GrievancePriority.High);
        Assert.Equal(1, highPriorityStats.Count);
        
        // Check status stats
        Assert.Equal(3, result.StatusStats.Count);
        var submittedStats = result.StatusStats.First(ss => ss.Status == GrievanceStatus.Submitted);
        Assert.Equal(1, submittedStats.Count);
    }

    [Fact]
    public async Task GetAnalyticsAsync_WithDateRange_ReturnsFilteredAnalytics()
    {
        // Arrange
        var fromDate = DateTime.UtcNow.AddDays(-7);
        var toDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = await _repository.GetAnalyticsAsync(fromDate, toDate);

        // Assert
        // Should only include grievances created within the date range
        Assert.Equal(2, result.TotalGrievances); // Grievances 1 and 2 are within range
    }

    [Fact]
    public async Task GetGrievancesByEscalationLevelAsync_ReturnsCorrectGrievances()
    {
        // Arrange
        var escalationLevel = (int)EscalationLevel.Level1_DirectManager;

        // Act
        var result = await _repository.GetGrievancesByEscalationLevelAsync(escalationLevel);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, g => Assert.Equal(EscalationLevel.Level1_DirectManager, g.CurrentEscalationLevel));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}