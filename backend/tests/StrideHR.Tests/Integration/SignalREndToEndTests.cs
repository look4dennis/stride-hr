using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StrideHR.Core.Enums;
using StrideHR.Core.Models.Notification;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;
using FluentAssertions;

namespace StrideHR.Tests.Integration;

public class SignalREndToEndTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private HubConnection? _hubConnection;

    public SignalREndToEndTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Add any test-specific services here
                services.AddLogging(logging => logging.AddConsole());
            });
        });
        
        _httpClient = _factory.CreateClient();
    }

    [Fact]
    public async Task SignalRHub_ShouldConnectSuccessfully()
    {
        // Arrange
        var hubUrl = _factory.Server.BaseAddress + "hubs/notification";
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult(GenerateTestJwtToken())!;
            })
            .Build();

        // Act
        await _hubConnection.StartAsync();

        // Assert
        _hubConnection.State.Should().Be(HubConnectionState.Connected);
    }

    [Fact]
    public async Task SignalRHub_ShouldReceiveNotifications()
    {
        // Arrange
        var hubUrl = _factory.Server.BaseAddress + "hubs/notification";
        var notificationReceived = false;
        object? receivedNotification = null;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult(GenerateTestJwtToken())!;
            })
            .Build();

        _hubConnection.On<object>("NotificationReceived", notification =>
        {
            notificationReceived = true;
            receivedNotification = notification;
        });

        await _hubConnection.StartAsync();

        // Act
        // Simulate sending a notification through the service
        using var scope = _factory.Services.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<StrideHR.Core.Interfaces.Services.IRealTimeNotificationService>();
        
        var testNotification = new NotificationDto
        {
            Id = 1,
            Title = "Test Notification",
            Message = "This is a test message",
            Type = NotificationType.Announcement,
            Priority = NotificationPriority.Normal,
            CreatedAt = DateTime.UtcNow
        };

        await notificationService.SendNotificationToUserAsync(123, testNotification);

        // Wait for the notification to be received
        await Task.Delay(1000);

        // Assert
        notificationReceived.Should().BeTrue();
        receivedNotification.Should().NotBeNull();
    }

    [Fact]
    public async Task SignalRHub_ShouldHandlePingPong()
    {
        // Arrange
        var hubUrl = _factory.Server.BaseAddress + "hubs/notification";
        var pongReceived = false;
        object? pongData = null;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult(GenerateTestJwtToken())!;
            })
            .Build();

        _hubConnection.On<object>("Pong", data =>
        {
            pongReceived = true;
            pongData = data;
        });

        await _hubConnection.StartAsync();

        // Act
        await _hubConnection.InvokeAsync("Ping");

        // Wait for the pong response
        await Task.Delay(500);

        // Assert
        pongReceived.Should().BeTrue();
        pongData.Should().NotBeNull();
    }

    [Fact]
    public async Task SignalRHub_ShouldHandleHeartbeat()
    {
        // Arrange
        var hubUrl = _factory.Server.BaseAddress + "hubs/notification";
        var heartbeatReceived = false;
        object? heartbeatData = null;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult(GenerateTestJwtToken())!;
            })
            .Build();

        _hubConnection.On<object>("Heartbeat", data =>
        {
            heartbeatReceived = true;
            heartbeatData = data;
            
            // Respond to heartbeat
            _ = Task.Run(async () =>
            {
                try
                {
                    await _hubConnection.InvokeAsync("HeartbeatResponse");
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the test
                    Console.WriteLine($"Error responding to heartbeat: {ex.Message}");
                }
            });
        });

        await _hubConnection.StartAsync();

        // Wait for heartbeat (the background service sends them periodically)
        await Task.Delay(2000);

        // Assert
        heartbeatReceived.Should().BeTrue();
        heartbeatData.Should().NotBeNull();
    }

    [Fact]
    public async Task SignalRHub_ShouldHandleConnectionRecovery()
    {
        // Arrange
        var hubUrl = _factory.Server.BaseAddress + "hubs/notification";
        var recoveryStarted = false;
        object? recoveryData = null;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult(GenerateTestJwtToken())!;
            })
            .Build();

        _hubConnection.On<object>("ConnectionRecoveryStarted", data =>
        {
            recoveryStarted = true;
            recoveryData = data;
        });

        await _hubConnection.StartAsync();

        // Act
        await _hubConnection.InvokeAsync("RequestConnectionRecovery");

        // Wait for the recovery response
        await Task.Delay(500);

        // Assert
        recoveryStarted.Should().BeTrue();
        recoveryData.Should().NotBeNull();
    }

    [Fact]
    public async Task SignalRHub_ShouldHandleGroupOperations()
    {
        // Arrange
        var hubUrl = _factory.Server.BaseAddress + "hubs/notification";
        var groupJoined = false;
        var groupLeft = false;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult(GenerateTestJwtToken())!;
            })
            .Build();

        _hubConnection.On<object>("GroupJoined", data => groupJoined = true);
        _hubConnection.On<object>("GroupLeft", data => groupLeft = true);

        await _hubConnection.StartAsync();

        // Act
        await _hubConnection.InvokeAsync("JoinGroup", "TestGroup");
        await Task.Delay(200);
        
        await _hubConnection.InvokeAsync("LeaveGroup", "TestGroup");
        await Task.Delay(200);

        // Assert
        groupJoined.Should().BeTrue();
        groupLeft.Should().BeTrue();
    }

    [Fact]
    public async Task SignalRHub_ShouldHandleAttendanceStatusUpdate()
    {
        // Arrange
        var hubUrl = _factory.Server.BaseAddress + "hubs/notification";
        var statusConfirmed = false;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult(GenerateTestJwtToken())!;
            })
            .Build();

        _hubConnection.On<object>("AttendanceStatusConfirmed", data => statusConfirmed = true);

        await _hubConnection.StartAsync();

        // Act
        await _hubConnection.InvokeAsync("UpdateAttendanceStatus", "CheckIn");
        await Task.Delay(500);

        // Assert
        statusConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task SignalRHub_ShouldRejectInvalidAttendanceStatus()
    {
        // Arrange
        var hubUrl = _factory.Server.BaseAddress + "hubs/notification";
        var errorReceived = false;
        string? errorMessage = null;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult(GenerateTestJwtToken())!;
            })
            .Build();

        _hubConnection.On<string>("Error", message =>
        {
            errorReceived = true;
            errorMessage = message;
        });

        await _hubConnection.StartAsync();

        // Act
        await _hubConnection.InvokeAsync("UpdateAttendanceStatus", "InvalidStatus");
        await Task.Delay(500);

        // Assert
        errorReceived.Should().BeTrue();
        errorMessage.Should().Be("Invalid attendance status");
    }

    [Fact]
    public async Task SignalRHub_ShouldHandleConnectionEstablishment()
    {
        // Arrange
        var hubUrl = _factory.Server.BaseAddress + "hubs/notification";
        var connectionEstablished = false;
        object? connectionData = null;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult(GenerateTestJwtToken())!;
            })
            .Build();

        _hubConnection.On<object>("ConnectionEstablished", data =>
        {
            connectionEstablished = true;
            connectionData = data;
        });

        // Act
        await _hubConnection.StartAsync();
        await Task.Delay(500);

        // Assert
        connectionEstablished.Should().BeTrue();
        connectionData.Should().NotBeNull();
    }

    private string GenerateTestJwtToken()
    {
        var handler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "123"),
                new Claim("EmployeeId", "456"),
                new Claim("BranchId", "789"),
                new Claim("OrganizationId", "101"),
                new Claim(ClaimTypes.Role, "Employee"),
                new Claim(ClaimTypes.Name, "Test User")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("test-secret-key-that-is-long-enough-for-hmac-sha256")),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };

        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    public void Dispose()
    {
        _hubConnection?.DisposeAsync().AsTask().Wait();
        _httpClient?.Dispose();
    }
}