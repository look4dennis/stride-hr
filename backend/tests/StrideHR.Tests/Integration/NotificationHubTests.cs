using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using StrideHR.API.Hubs;
using System.Security.Claims;
using Xunit;
using FluentAssertions;

namespace StrideHR.Tests.Integration;

public class NotificationHubTests
{
    private readonly Mock<ILogger<NotificationHub>> _mockLogger;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<ISingleClientProxy> _mockClientProxy;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly NotificationHub _hub;

    public NotificationHubTests()
    {
        _mockLogger = new Mock<ILogger<NotificationHub>>();
        _mockContext = new Mock<HubCallerContext>();
        _mockClients = new Mock<IHubCallerClients>();
        _mockClientProxy = new Mock<ISingleClientProxy>();
        _mockGroups = new Mock<IGroupManager>();

        _hub = new NotificationHub(_mockLogger.Object);

        // Setup hub context
        _hub.Context = _mockContext.Object;
        _hub.Clients = _mockClients.Object;
        _hub.Groups = _mockGroups.Object;

        // Setup default mocks
        _mockContext.Setup(c => c.ConnectionId).Returns("test-connection-id");
        _mockClients.Setup(c => c.Caller).Returns(_mockClientProxy.Object);
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
    }

    [Fact]
    public async Task OnConnectedAsync_WithValidClaims_ShouldAddToGroups()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim("EmployeeId", "456"),
            new Claim("BranchId", "789"),
            new Claim("OrganizationId", "101"),
            new Claim(ClaimTypes.Role, "Employee")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _mockContext.Setup(c => c.User).Returns(principal);

        // Act
        await _hub.OnConnectedAsync();

        // Assert
        _mockGroups.Verify(g => g.AddToGroupAsync("test-connection-id", "User_123", default), Times.Once);
        _mockGroups.Verify(g => g.AddToGroupAsync("test-connection-id", "Branch_789", default), Times.Once);
        _mockGroups.Verify(g => g.AddToGroupAsync("test-connection-id", "Organization_101", default), Times.Once);
        _mockGroups.Verify(g => g.AddToGroupAsync("test-connection-id", "Role_Employee", default), Times.Once);
    }

    [Fact]
    public async Task OnConnectedAsync_WithMissingClaims_ShouldAbortConnection()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "123")
            // Missing EmployeeId claim
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _mockContext.Setup(c => c.User).Returns(principal);
        _mockContext.Setup(c => c.Abort()).Verifiable();

        // Act
        await _hub.OnConnectedAsync();

        // Assert
        _mockContext.Verify(c => c.Abort(), Times.Once);
    }

    [Fact]
    public async Task JoinGroup_WithValidGroup_ShouldAddToGroup()
    {
        // Arrange
        var groupName = "TestGroup";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim("EmployeeId", "456"),
            new Claim(ClaimTypes.Role, "SuperAdmin")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _mockContext.Setup(c => c.User).Returns(principal);

        // Act
        await _hub.JoinGroup(groupName);

        // Assert
        _mockGroups.Verify(g => g.AddToGroupAsync("test-connection-id", groupName, default), Times.Once);
        _mockClientProxy.Verify(
            c => c.SendCoreAsync("GroupJoined", It.IsAny<object[]>(), default),
            Times.Once);
    }

    [Fact]
    public async Task JoinGroup_WithEmptyGroupName_ShouldSendError()
    {
        // Arrange
        var groupName = "";

        // Act
        await _hub.JoinGroup(groupName);

        // Assert
        _mockClientProxy.Verify(
            c => c.SendCoreAsync("Error", It.Is<object[]>(args => args[0].ToString() == "Group name cannot be empty"), default),
            Times.Once);
    }

    [Fact]
    public async Task LeaveGroup_WithValidGroup_ShouldRemoveFromGroup()
    {
        // Arrange
        var groupName = "TestGroup";

        // Act
        await _hub.LeaveGroup(groupName);

        // Assert
        _mockGroups.Verify(g => g.RemoveFromGroupAsync("test-connection-id", groupName, default), Times.Once);
        _mockClientProxy.Verify(
            c => c.SendCoreAsync("GroupLeft", It.IsAny<object[]>(), default),
            Times.Once);
    }

    [Fact]
    public async Task SendBirthdayWish_WithValidData_ShouldSendWish()
    {
        // Arrange
        var toUserId = 456;
        var message = "Happy Birthday!";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim("EmployeeId", "789"),
            new Claim(ClaimTypes.Name, "John Doe")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _mockContext.Setup(c => c.User).Returns(principal);

        // Act
        await _hub.SendBirthdayWish(toUserId, message);

        // Assert
        _mockClients.Verify(c => c.Group($"User_{toUserId}"), Times.Once);
        _mockClientProxy.Verify(
            c => c.SendCoreAsync("BirthdayWishReceived", It.IsAny<object[]>(), default),
            Times.Once);
        _mockClientProxy.Verify(
            c => c.SendCoreAsync("BirthdayWishSent", It.IsAny<object[]>(), default),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAttendanceStatus_WithValidStatus_ShouldBroadcastUpdate()
    {
        // Arrange
        var status = "CheckIn";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim("EmployeeId", "456"),
            new Claim("BranchId", "789")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _mockContext.Setup(c => c.User).Returns(principal);

        // Act
        await _hub.UpdateAttendanceStatus(status);

        // Assert
        _mockClients.Verify(c => c.Group("Branch_789"), Times.Once);
        _mockClientProxy.Verify(
            c => c.SendCoreAsync("AttendanceStatusUpdated", It.IsAny<object[]>(), default),
            Times.Once);
        _mockClientProxy.Verify(
            c => c.SendCoreAsync("AttendanceStatusConfirmed", It.IsAny<object[]>(), default),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAttendanceStatus_WithInvalidStatus_ShouldSendError()
    {
        // Arrange
        var status = "InvalidStatus";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim("EmployeeId", "456"),
            new Claim("BranchId", "789")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _mockContext.Setup(c => c.User).Returns(principal);

        // Act
        await _hub.UpdateAttendanceStatus(status);

        // Assert
        _mockClientProxy.Verify(
            c => c.SendCoreAsync("Error", It.Is<object[]>(args => args[0].ToString() == "Invalid attendance status"), default),
            Times.Once);
    }

    [Fact]
    public async Task Ping_ShouldRespondWithPong()
    {
        // Act
        await _hub.Ping();

        // Assert
        _mockClientProxy.Verify(
            c => c.SendCoreAsync("Pong", It.IsAny<object[]>(), default),
            Times.Once);
    }

    [Fact]
    public async Task GetConnectionStats_WithAdminRole_ShouldReturnStats()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Role, "SuperAdmin")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _mockContext.Setup(c => c.User).Returns(principal);

        // Act
        await _hub.GetConnectionStats();

        // Assert
        _mockClientProxy.Verify(
            c => c.SendCoreAsync("ConnectionStats", It.IsAny<object[]>(), default),
            Times.Once);
    }

    [Fact]
    public async Task GetConnectionStats_WithoutAdminRole_ShouldSendError()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Role, "Employee")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _mockContext.Setup(c => c.User).Returns(principal);

        // Act
        await _hub.GetConnectionStats();

        // Assert
        _mockClientProxy.Verify(
            c => c.SendCoreAsync("Error", It.Is<object[]>(args => args[0].ToString() == "Access denied"), default),
            Times.Once);
    }
}