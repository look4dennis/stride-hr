using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using StrideHR.Core.Entities;
using StrideHR.Infrastructure.Data;
using StrideHR.Infrastructure.Repositories;

namespace StrideHR.Tests.Infrastructure;

public class RepositoryTests : IDisposable
{
    private readonly StrideHRDbContext _context;
    private readonly Repository<Organization> _repository;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<StrideHRDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StrideHRDbContext(options);
        _repository = new Repository<Organization>(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntityToDatabase()
    {
        // Arrange
        var organization = new Organization
        {
            Name = "Test Organization",
            Email = "test@example.com",
            NormalWorkingHours = 8.0m,
            OvertimeRate = 1.5m,
            ProductiveHoursThreshold = 6
        };

        // Act
        var result = await _repository.AddAsync(organization);
        await _repository.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Test Organization");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenEntityExists()
    {
        // Arrange
        var organization = new Organization
        {
            Name = "Test Organization",
            Email = "test@example.com"
        };

        await _repository.AddAsync(organization);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(organization.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Organization");
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldMarkEntityAsDeleted()
    {
        // Arrange
        var organization = new Organization
        {
            Name = "Test Organization",
            Email = "test@example.com"
        };

        await _repository.AddAsync(organization);
        await _repository.SaveChangesAsync();

        // Act
        await _repository.SoftDeleteAsync(organization.Id, "TestUser");
        await _repository.SaveChangesAsync();

        // Assert
        var deletedEntity = await _context.Organizations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == organization.Id);

        deletedEntity.Should().NotBeNull();
        deletedEntity!.IsDeleted.Should().BeTrue();
        deletedEntity.DeletedBy.Should().Be("TestUser");
        deletedEntity.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify entity is not returned by normal queries (due to global query filter)
        var normalQuery = await _repository.GetByIdAsync(organization.Id);
        normalQuery.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var organizations = new List<Organization>();
        for (int i = 1; i <= 15; i++)
        {
            organizations.Add(new Organization
            {
                Name = $"Organization {i:D2}",
                Email = $"org{i}@example.com"
            });
        }

        await _repository.AddRangeAsync(organizations);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetPagedAsync(
            pageNumber: 2,
            pageSize: 5,
            orderBy: o => o.Name);

        // Assert
        result.TotalCount.Should().Be(15);
        result.Items.Should().HaveCount(5);
        result.Items.First().Name.Should().Be("Organization 06");
        result.Items.Last().Name.Should().Be("Organization 10");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}