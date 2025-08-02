using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.API.Hubs;
using System.Security.Claims;
using Xunit;

namespace StrideHR.Tests.Hubs;

public class NotificationHubTests
{
    private readonly Mock<ILogger<NotificationHub>> _mockLogger;
    private readonly NotificationHub _hub;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly Mock<IHubCallerClients> _mockClients;

    public NotificationHubTests()
    {
        _mockLogger = new Mock<ILogger<NotificationHub>>();
        _mockContext = new Mock<HubCallerContext>();
        _mockGroups = new Mock<IGroupManager>();
        _mockClients = new Mock<IHubCallerClients>();

        _hub = new NotificationHub(_mockLogger.Object)
        {
            Context = _mockContext.Object,
            Groups = _mockGroups.Object,
            Clients = _mockClients.Object
        };
    }

    [Fact]
    public async Task OnConnectedAsync_ValidUser_AddsToGroups()
    {
        // Arrange
        var userId = "123";
        var branchId = "456";
        var role = "Employee";
        var connectionId = "connection123";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("BranchId", branchId),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _mockContext.Setup(c => c.User).Returns(principal);
        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

        _mockGroups.Setup(g => g.AddToGroupAsync(connectionId, $"User_{userId}", default))
            .Returns(Task.CompletedTask);
        _mockGroups.Setup(g => g.AddToGroupAsync(connectionId, $"Branch_{branchId}", default))
            .Returns(Task.CompletedTask);
        _mockGroups.Setup(g => g.AddToGroupAsync(connectionId, $"Role_{role}", default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.OnConnectedAsync();

        // Assert
        _mockGroups.Verify(g => g.AddToGroupAsync(connectionId, $"User_{userId}", default), Times.Once);
        _mockGroups.Verify(g => g.AddToGroupAsync(connectionId, $"Branch_{branchId}", default), Times.Once);
        _mockGroups.Verify(g => g.AddToGroupAsync(connectionId, $"Role_{role}", default), Times.Once);
    }

    [Fact]
    public async Task JoinGroup_ValidGroupName_AddsToGroup()
    {
        // Arrange
        var groupName = "TestGroup";
        var connectionId = "connection123";
        var userId = "123";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };

        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _mockContext.Setup(c => c.User).Returns(principal);
        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

        _mockGroups.Setup(g => g.AddToGroupAsync(connectionId, groupName, default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.JoinGroup(groupName);

        // Assert
        _mockGroups.Verify(g => g.AddToGroupAsync(connectionId, groupName, default), Times.Once);
    }

    [Fact]
    public async Task LeaveGroup_ValidGroupName_RemovesFromGroup()
    {
        // Arrange
        var groupName = "TestGroup";
        var connectionId = "connection123";
        var userId = "123";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };

        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _mockContext.Setup(c => c.User).Returns(principal);
        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

        _mockGroups.Setup(g => g.RemoveFromGroupAsync(connectionId, groupName, default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.LeaveGroup(groupName);

        // Assert
        _mockGroups.Verify(g => g.RemoveFromGroupAsync(connectionId, groupName, default), Times.Once);
    }

    [Fact]
    public async Task SendBirthdayWish_ValidParameters_SendsToTargetUser()
    {
        // Arrange
        var fromUserId = "123";
        var fromUserName = "John Doe";
        var toUserId = 456;
        var message = "Happy Birthday!";
        var connectionId = "connection123";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, fromUserId),
            new Claim(ClaimTypes.Name, fromUserName)
        };

        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _mockContext.Setup(c => c.User).Returns(principal);
        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

        var mockClientProxy = new Mock<IClientProxy>();
        _mockClients.Setup(c => c.Group($"User_{toUserId}")).Returns(mockClientProxy.Object);

        mockClientProxy.Setup(c => c.SendCoreAsync("BirthdayWishReceived", It.IsAny<object[]>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.SendBirthdayWish(toUserId, message);

        // Assert
        _mockClients.Verify(c => c.Group($"User_{toUserId}"), Times.Once);
        mockClientProxy.Verify(
            c => c.SendCoreAsync("BirthdayWishReceived", 
                It.Is<object[]>(args => args.Length == 1), 
                default), 
            Times.Once);
    }

    [Fact]
    public async Task UpdateAttendanceStatus_ValidStatus_BroadcastsToBranch()
    {
        // Arrange
        var userId = "123";
        var branchId = "456";
        var status = "CheckedIn";
        var connectionId = "connection123";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("BranchId", branchId)
        };

        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _mockContext.Setup(c => c.User).Returns(principal);
        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

        var mockClientProxy = new Mock<IClientProxy>();
        _mockClients.Setup(c => c.Group($"Branch_{branchId}")).Returns(mockClientProxy.Object);

        mockClientProxy.Setup(c => c.SendCoreAsync("AttendanceStatusUpdated", It.IsAny<object[]>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.UpdateAttendanceStatus(status);

        // Assert
        _mockClients.Verify(c => c.Group($"Branch_{branchId}"), Times.Once);
        mockClientProxy.Verify(
            c => c.SendCoreAsync("AttendanceStatusUpdated", 
                It.Is<object[]>(args => args.Length == 1), 
                default), 
            Times.Once);
    }

    [Fact]
    public async Task AcknowledgeProductivityAlert_ValidAlertId_SendsAcknowledgment()
    {
        // Arrange
        var userId = "123";
        var alertId = 789;
        var connectionId = "connection123";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };

        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _mockContext.Setup(c => c.User).Returns(principal);
        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

        var mockClientProxy = new Mock<IClientProxy>();
        var mockSingleClientProxy = mockClientProxy.As<ISingleClientProxy>();
        _mockClients.Setup(c => c.Caller).Returns(mockSingleClientProxy.Object);

        mockSingleClientProxy.Setup(c => c.SendCoreAsync("ProductivityAlertAcknowledged", It.IsAny<object[]>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.AcknowledgeProductivityAlert(alertId);

        // Assert
        _mockClients.Verify(c => c.Caller, Times.Once);
        mockSingleClientProxy.Verify(
            c => c.SendCoreAsync("ProductivityAlertAcknowledged", 
                It.Is<object[]>(args => args.Length == 1), 
                default), 
            Times.Once);
    }
}