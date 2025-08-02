using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.SupportTicket;
using StrideHR.Infrastructure.Services;
using Xunit;

namespace StrideHR.Tests.Services;

public class SupportTicketServiceTests
{
    private readonly Mock<ISupportTicketRepository> _mockTicketRepository;
    private readonly Mock<ISupportTicketCommentRepository> _mockCommentRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly SupportTicketService _service;

    public SupportTicketServiceTests()
    {
        _mockTicketRepository = new Mock<ISupportTicketRepository>();
        _mockCommentRepository = new Mock<ISupportTicketCommentRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();

        _service = new SupportTicketService(
            _mockTicketRepository.Object,
            _mockCommentRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task CreateTicketAsync_ValidData_ReturnsTicketDto()
    {
        // Arrange
        var dto = new CreateSupportTicketDto
        {
            Title = "Test Ticket",
            Description = "Test Description",
            Category = SupportTicketCategory.Hardware,
            Priority = SupportTicketPriority.Medium,
            RequiresRemoteAccess = false
        };

        var requesterId = 1;
        var ticketNumber = "ST202501010001";

        _mockTicketRepository.Setup(r => r.GenerateTicketNumberAsync())
            .ReturnsAsync(ticketNumber);

        _mockTicketRepository.Setup(r => r.AddAsync(It.IsAny<SupportTicket>()))
            .ReturnsAsync(It.IsAny<SupportTicket>());

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var createdTicket = new SupportTicket
        {
            Id = 1,
            TicketNumber = ticketNumber,
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Priority = dto.Priority,
            Status = SupportTicketStatus.Open,
            RequesterId = requesterId,
            CreatedAt = DateTime.UtcNow,
            Requester = new Employee { Id = requesterId, FirstName = "John", LastName = "Doe", Email = "john.doe@test.com" }
        };

        _mockTicketRepository.Setup(r => r.GetWithDetailsAsync(It.IsAny<int>()))
            .ReturnsAsync(createdTicket);

        // Act
        var result = await _service.CreateTicketAsync(dto, requesterId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ticketNumber, result.TicketNumber);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.Description, result.Description);
        Assert.Equal(SupportTicketStatus.Open, result.Status);
        Assert.Equal(requesterId, result.RequesterId);

        _mockTicketRepository.Verify(r => r.GenerateTicketNumberAsync(), Times.Once);
        _mockTicketRepository.Verify(r => r.AddAsync(It.IsAny<SupportTicket>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTicketByIdAsync_ExistingTicket_ReturnsTicketDto()
    {
        // Arrange
        var ticketId = 1;
        var ticket = new SupportTicket
        {
            Id = ticketId,
            TicketNumber = "ST202501010001",
            Title = "Test Ticket",
            Description = "Test Description",
            Category = SupportTicketCategory.Hardware,
            Priority = SupportTicketPriority.Medium,
            Status = SupportTicketStatus.Open,
            RequesterId = 1,
            CreatedAt = DateTime.UtcNow,
            Requester = new Employee { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@test.com" },
            Comments = new List<SupportTicketComment>()
        };

        _mockTicketRepository.Setup(r => r.GetWithDetailsAsync(ticketId))
            .ReturnsAsync(ticket);

        // Act
        var result = await _service.GetTicketByIdAsync(ticketId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ticketId, result.Id);
        Assert.Equal(ticket.TicketNumber, result.TicketNumber);
        Assert.Equal(ticket.Title, result.Title);

        _mockTicketRepository.Verify(r => r.GetWithDetailsAsync(ticketId), Times.Once);
    }

    [Fact]
    public async Task GetTicketByIdAsync_NonExistingTicket_ThrowsArgumentException()
    {
        // Arrange
        var ticketId = 999;
        _mockTicketRepository.Setup(r => r.GetWithDetailsAsync(ticketId))
            .ReturnsAsync((SupportTicket?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetTicketByIdAsync(ticketId));

        _mockTicketRepository.Verify(r => r.GetWithDetailsAsync(ticketId), Times.Once);
    }

    [Fact]
    public async Task AssignTicketAsync_ValidData_AssignsTicketAndUpdatesStatus()
    {
        // Arrange
        var ticketId = 1;
        var assignedToId = 2;
        var assignedById = 3;

        var ticket = new SupportTicket
        {
            Id = ticketId,
            TicketNumber = "ST202501010001",
            Title = "Test Ticket",
            Status = SupportTicketStatus.Open,
            RequesterId = 1,
            CreatedAt = DateTime.UtcNow,
            Requester = new Employee { Id = 1, FirstName = "John", LastName = "Doe" },
            StatusHistory = new List<SupportTicketStatusHistory>()
        };

        _mockTicketRepository.Setup(r => r.GetWithDetailsAsync(ticketId))
            .ReturnsAsync(ticket);

        _mockTicketRepository.Setup(r => r.UpdateAsync(It.IsAny<SupportTicket>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.AssignTicketAsync(ticketId, assignedToId, assignedById);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(assignedToId, result.AssignedToId);
        Assert.NotNull(result.AssignedAt);

        _mockTicketRepository.Verify(r => r.GetWithDetailsAsync(ticketId), Times.Once);
        _mockTicketRepository.Verify(r => r.UpdateAsync(It.IsAny<SupportTicket>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ResolveTicketAsync_ValidData_ResolvesTicketAndCalculatesResolutionTime()
    {
        // Arrange
        var ticketId = 1;
        var resolution = "Issue resolved by restarting the service";
        var resolvedById = 2;
        var createdAt = DateTime.UtcNow.AddHours(-2);

        var ticket = new SupportTicket
        {
            Id = ticketId,
            TicketNumber = "ST202501010001",
            Title = "Test Ticket",
            Status = SupportTicketStatus.InProgress,
            RequesterId = 1,
            CreatedAt = createdAt,
            Requester = new Employee { Id = 1, FirstName = "John", LastName = "Doe" },
            StatusHistory = new List<SupportTicketStatusHistory>()
        };

        _mockTicketRepository.Setup(r => r.GetWithDetailsAsync(ticketId))
            .ReturnsAsync(ticket);

        _mockTicketRepository.Setup(r => r.UpdateAsync(It.IsAny<SupportTicket>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.ResolveTicketAsync(ticketId, resolution, resolvedById);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(resolution, result.Resolution);
        Assert.NotNull(result.ResolvedAt);
        Assert.NotNull(result.ResolutionTime);
        Assert.Equal(SupportTicketStatus.Resolved, result.Status);

        _mockTicketRepository.Verify(r => r.GetWithDetailsAsync(ticketId), Times.Once);
        _mockTicketRepository.Verify(r => r.UpdateAsync(It.IsAny<SupportTicket>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddCommentAsync_ValidData_AddsCommentToTicket()
    {
        // Arrange
        var ticketId = 1;
        var authorId = 2;
        var dto = new CreateSupportTicketCommentDto
        {
            Comment = "This is a test comment",
            IsInternal = false
        };

        var ticket = new SupportTicket { Id = ticketId };

        _mockTicketRepository.Setup(r => r.GetByIdAsync(ticketId))
            .ReturnsAsync(ticket);

        _mockCommentRepository.Setup(r => r.AddAsync(It.IsAny<SupportTicketComment>()))
            .ReturnsAsync(It.IsAny<SupportTicketComment>());

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var createdComment = new SupportTicketComment
        {
            Id = 1,
            SupportTicketId = ticketId,
            AuthorId = authorId,
            Comment = dto.Comment,
            IsInternal = dto.IsInternal,
            CreatedAt = DateTime.UtcNow,
            Author = new Employee { Id = authorId, FirstName = "Jane", LastName = "Smith" }
        };

        _mockCommentRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(createdComment);

        // Act
        var result = await _service.AddCommentAsync(ticketId, dto, authorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Comment, result.Comment);
        Assert.Equal(authorId, result.AuthorId);
        Assert.Equal(dto.IsInternal, result.IsInternal);

        _mockTicketRepository.Verify(r => r.GetByIdAsync(ticketId), Times.Once);
        _mockCommentRepository.Verify(r => r.AddAsync(It.IsAny<SupportTicketComment>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SearchTicketsAsync_ValidCriteria_ReturnsFilteredResults()
    {
        // Arrange
        var criteria = new SupportTicketSearchCriteria
        {
            Status = SupportTicketStatus.Open,
            Category = SupportTicketCategory.Hardware,
            PageNumber = 1,
            PageSize = 10
        };

        var tickets = new List<SupportTicket>
        {
            new SupportTicket
            {
                Id = 1,
                TicketNumber = "ST202501010001",
                Title = "Test Ticket 1",
                Status = SupportTicketStatus.Open,
                Category = SupportTicketCategory.Hardware,
                Requester = new Employee { FirstName = "John", LastName = "Doe" },
                Comments = new List<SupportTicketComment>()
            }
        };

        _mockTicketRepository.Setup(r => r.SearchAsync(criteria))
            .ReturnsAsync((tickets, 1));

        // Act
        var (resultTickets, totalCount) = await _service.SearchTicketsAsync(criteria);

        // Assert
        Assert.Single(resultTickets);
        Assert.Equal(1, totalCount);
        Assert.Equal("ST202501010001", resultTickets[0].TicketNumber);

        _mockTicketRepository.Verify(r => r.SearchAsync(criteria), Times.Once);
    }

    [Fact]
    public async Task CloseTicketAsync_ValidData_ClosesTicketWithFeedback()
    {
        // Arrange
        var ticketId = 1;
        var closedById = 2;
        var satisfactionRating = 5;
        var feedbackComments = "Great service!";

        var ticket = new SupportTicket
        {
            Id = ticketId,
            TicketNumber = "ST202501010001",
            Title = "Test Ticket",
            Status = SupportTicketStatus.Resolved,
            RequesterId = 1,
            CreatedAt = DateTime.UtcNow.AddHours(-3),
            ResolvedAt = DateTime.UtcNow.AddMinutes(-30),
            Requester = new Employee { Id = 1, FirstName = "John", LastName = "Doe" },
            StatusHistory = new List<SupportTicketStatusHistory>()
        };

        _mockTicketRepository.Setup(r => r.GetWithDetailsAsync(ticketId))
            .ReturnsAsync(ticket);

        _mockTicketRepository.Setup(r => r.UpdateAsync(It.IsAny<SupportTicket>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.CloseTicketAsync(ticketId, closedById, satisfactionRating, feedbackComments);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(SupportTicketStatus.Closed, result.Status);
        Assert.Equal(satisfactionRating, result.SatisfactionRating);
        Assert.Equal(feedbackComments, result.FeedbackComments);
        Assert.NotNull(result.ClosedAt);

        _mockTicketRepository.Verify(r => r.GetWithDetailsAsync(ticketId), Times.Once);
        _mockTicketRepository.Verify(r => r.UpdateAsync(It.IsAny<SupportTicket>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}