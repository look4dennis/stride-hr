using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Collections.Concurrent;

namespace StrideHR.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;
    private static readonly ConcurrentDictionary<string, UserConnectionInfo> _connections = new();

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var employeeId = Context.User?.FindFirst("EmployeeId")?.Value;
            var branchId = Context.User?.FindFirst("BranchId")?.Value;
            var organizationId = Context.User?.FindFirst("OrganizationId")?.Value;
            var roles = Context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();

            // Validate required claims
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(employeeId))
            {
                _logger.LogWarning("SignalR connection rejected: Missing required claims (UserId: {UserId}, EmployeeId: {EmployeeId})", 
                    userId, employeeId);
                Context.Abort();
                return;
            }

            // Store connection information
            var connectionInfo = new UserConnectionInfo
            {
                ConnectionId = Context.ConnectionId,
                UserId = userId,
                EmployeeId = employeeId,
                BranchId = branchId,
                OrganizationId = organizationId,
                Roles = roles,
                ConnectedAt = DateTime.UtcNow,
                UserAgent = Context.GetHttpContext()?.Request.Headers.UserAgent.ToString(),
                IpAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString()
            };

            _connections.TryAdd(Context.ConnectionId, connectionInfo);

            // Add user to their personal group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            _logger.LogInformation("User {UserId} (Employee {EmployeeId}) connected to notification hub from {IpAddress}", 
                userId, employeeId, connectionInfo.IpAddress);

            // Add user to their branch group if available
            if (!string.IsNullOrEmpty(branchId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Branch_{branchId}");
                _logger.LogDebug("User {UserId} added to branch {BranchId} group", userId, branchId);
            }

            // Add user to their organization group if available
            if (!string.IsNullOrEmpty(organizationId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Organization_{organizationId}");
                _logger.LogDebug("User {UserId} added to organization {OrganizationId} group", userId, organizationId);
            }

            // Add user to their role groups
            foreach (var role in roles)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{role}");
                _logger.LogDebug("User {UserId} added to role {Role} group", userId, role);
            }

            // Send connection confirmation to client
            await Clients.Caller.SendAsync("ConnectionEstablished", new
            {
                ConnectionId = Context.ConnectionId,
                ConnectedAt = connectionInfo.ConnectedAt,
                UserId = userId,
                EmployeeId = employeeId
            });

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SignalR connection establishment for connection {ConnectionId}", Context.ConnectionId);
            Context.Abort();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var connectionId = Context.ConnectionId;
            
            if (_connections.TryRemove(connectionId, out var connectionInfo))
            {
                var duration = DateTime.UtcNow - connectionInfo.ConnectedAt;
                
                if (exception != null)
                {
                    _logger.LogWarning(exception, "User {UserId} (Employee {EmployeeId}) disconnected from notification hub with error after {Duration}ms", 
                        connectionInfo.UserId, connectionInfo.EmployeeId, duration.TotalMilliseconds);
                }
                else
                {
                    _logger.LogInformation("User {UserId} (Employee {EmployeeId}) disconnected from notification hub normally after {Duration}ms", 
                        connectionInfo.UserId, connectionInfo.EmployeeId, duration.TotalMilliseconds);
                }
            }
            else
            {
                _logger.LogWarning("Connection {ConnectionId} disconnected but was not found in connection tracking", connectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SignalR disconnection for connection {ConnectionId}", Context.ConnectionId);
        }
    }

    // Client can join specific groups with validation
    public async Task JoinGroup(string groupName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                await Clients.Caller.SendAsync("Error", "Group name cannot be empty");
                return;
            }

            // Validate group access permissions
            if (!await ValidateGroupAccess(groupName))
            {
                await Clients.Caller.SendAsync("Error", "Access denied to group");
                _logger.LogWarning("User {UserId} denied access to group {GroupName}", 
                    Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, groupName);
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("User {UserId} joined group {GroupName}", userId, groupName);
            
            await Clients.Caller.SendAsync("GroupJoined", new { GroupName = groupName, JoinedAt = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining group {GroupName} for user {UserId}", 
                groupName, Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await Clients.Caller.SendAsync("Error", "Failed to join group");
        }
    }

    public async Task LeaveGroup(string groupName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                await Clients.Caller.SendAsync("Error", "Group name cannot be empty");
                return;
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("User {UserId} left group {GroupName}", userId, groupName);
            
            await Clients.Caller.SendAsync("GroupLeft", new { GroupName = groupName, LeftAt = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving group {GroupName} for user {UserId}", 
                groupName, Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await Clients.Caller.SendAsync("Error", "Failed to leave group");
        }
    }

    // Handle birthday wishes with validation
    public async Task SendBirthdayWish(int toUserId, string message)
    {
        try
        {
            var fromUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var fromUserName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            var fromEmployeeId = Context.User?.FindFirst("EmployeeId")?.Value;

            if (string.IsNullOrEmpty(fromUserId) || string.IsNullOrEmpty(fromUserName) || string.IsNullOrEmpty(fromEmployeeId))
            {
                await Clients.Caller.SendAsync("Error", "Invalid user information");
                return;
            }

            if (string.IsNullOrWhiteSpace(message) || message.Length > 500)
            {
                await Clients.Caller.SendAsync("Error", "Message must be between 1 and 500 characters");
                return;
            }

            var wishData = new
            {
                FromUserId = fromUserId,
                FromEmployeeId = fromEmployeeId,
                FromUserName = fromUserName,
                Message = message.Trim(),
                Timestamp = DateTime.UtcNow
            };

            // Send to the birthday person
            await Clients.Group($"User_{toUserId}").SendAsync("BirthdayWishReceived", wishData);
            
            // Confirm to sender
            await Clients.Caller.SendAsync("BirthdayWishSent", new { ToUserId = toUserId, SentAt = DateTime.UtcNow });
            
            _logger.LogInformation("Birthday wish sent from {FromUserId} (Employee {FromEmployeeId}) to User {ToUserId}", 
                fromUserId, fromEmployeeId, toUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending birthday wish from {FromUserId} to {ToUserId}", 
                Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, toUserId);
            await Clients.Caller.SendAsync("Error", "Failed to send birthday wish");
        }
    }

    // Handle attendance status updates with validation
    public async Task UpdateAttendanceStatus(string status)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var employeeId = Context.User?.FindFirst("EmployeeId")?.Value;
            var branchId = Context.User?.FindFirst("BranchId")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(employeeId) || string.IsNullOrEmpty(branchId))
            {
                await Clients.Caller.SendAsync("Error", "Invalid user or branch information");
                return;
            }

            // Validate status values
            var validStatuses = new[] { "CheckIn", "CheckOut", "BreakStart", "BreakEnd", "Online", "Offline", "Away" };
            if (!validStatuses.Contains(status))
            {
                await Clients.Caller.SendAsync("Error", "Invalid attendance status");
                return;
            }

            var statusData = new
            {
                UserId = userId,
                EmployeeId = employeeId,
                Status = status,
                Timestamp = DateTime.UtcNow,
                BranchId = branchId
            };

            // Broadcast to branch members (for real-time attendance dashboard)
            await Clients.Group($"Branch_{branchId}").SendAsync("AttendanceStatusUpdated", statusData);
            
            // Confirm to sender
            await Clients.Caller.SendAsync("AttendanceStatusConfirmed", new { Status = status, UpdatedAt = DateTime.UtcNow });
            
            _logger.LogInformation("Attendance status updated for user {UserId} (Employee {EmployeeId}): {Status}", 
                userId, employeeId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating attendance status for user {UserId}", 
                Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await Clients.Caller.SendAsync("Error", "Failed to update attendance status");
        }
    }

    // Handle productivity alerts acknowledgment with validation
    public async Task AcknowledgeProductivityAlert(int alertId)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var employeeId = Context.User?.FindFirst("EmployeeId")?.Value;
            
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(employeeId))
            {
                await Clients.Caller.SendAsync("Error", "Invalid user information");
                return;
            }

            if (alertId <= 0)
            {
                await Clients.Caller.SendAsync("Error", "Invalid alert ID");
                return;
            }

            await Clients.Caller.SendAsync("ProductivityAlertAcknowledged", new 
            { 
                AlertId = alertId, 
                AcknowledgedAt = DateTime.UtcNow,
                UserId = userId,
                EmployeeId = employeeId
            });
            
            _logger.LogInformation("Productivity alert {AlertId} acknowledged by user {UserId} (Employee {EmployeeId})", 
                alertId, userId, employeeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging productivity alert {AlertId} for user {UserId}", 
                alertId, Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await Clients.Caller.SendAsync("Error", "Failed to acknowledge alert");
        }
    }

    // Get connection statistics (for monitoring)
    public async Task GetConnectionStats()
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = Context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();

            // Only allow administrators to view connection stats
            if (!roles.Contains("SuperAdmin") && !roles.Contains("HRManager"))
            {
                await Clients.Caller.SendAsync("Error", "Access denied");
                return;
            }

            var stats = new
            {
                TotalConnections = _connections.Count,
                ConnectionsByBranch = _connections.Values
                    .Where(c => !string.IsNullOrEmpty(c.BranchId))
                    .GroupBy(c => c.BranchId)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ConnectionsByRole = _connections.Values
                    .SelectMany(c => c.Roles)
                    .GroupBy(r => r)
                    .ToDictionary(g => g.Key, g => g.Count()),
                RetrievedAt = DateTime.UtcNow
            };

            await Clients.Caller.SendAsync("ConnectionStats", stats);
            _logger.LogInformation("Connection stats retrieved by user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving connection stats for user {UserId}", 
                Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await Clients.Caller.SendAsync("Error", "Failed to retrieve connection stats");
        }
    }

    // Ping method for connection health checks
    public async Task Ping()
    {
        try
        {
            await Clients.Caller.SendAsync("Pong", new { Timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error responding to ping from connection {ConnectionId}", Context.ConnectionId);
        }
    }

    // Heartbeat response method for connection recovery
    public async Task HeartbeatResponse()
    {
        try
        {
            Services.SignalRConnectionRecoveryService.RecordHeartbeatResponse(Context.ConnectionId);
            _logger.LogDebug("Heartbeat response received from connection {ConnectionId}", Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing heartbeat response from connection {ConnectionId}", Context.ConnectionId);
        }
    }

    // Connection recovery method
    public async Task RequestConnectionRecovery()
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var employeeId = Context.User?.FindFirst("EmployeeId")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(employeeId))
            {
                await Clients.Caller.SendAsync("Error", "Invalid user information for recovery");
                return;
            }

            // Send connection recovery confirmation
            await Clients.Caller.SendAsync("ConnectionRecoveryStarted", new
            {
                ConnectionId = Context.ConnectionId,
                UserId = userId,
                EmployeeId = employeeId,
                RecoveryStartedAt = DateTime.UtcNow
            });

            _logger.LogInformation("Connection recovery requested by user {UserId} (Employee {EmployeeId})", 
                userId, employeeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing connection recovery request from connection {ConnectionId}", 
                Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", "Failed to process connection recovery request");
        }
    }

    // Method to get connection health status
    public async Task GetConnectionHealth()
    {
        try
        {
            var healthInfo = Services.SignalRConnectionRecoveryService.GetConnectionHealth(Context.ConnectionId);
            
            await Clients.Caller.SendAsync("ConnectionHealth", new
            {
                ConnectionId = Context.ConnectionId,
                IsHealthy = healthInfo?.IsHealthy ?? true,
                LastSeen = healthInfo?.LastSeen ?? DateTime.UtcNow,
                ConsecutiveFailures = healthInfo?.ConsecutiveFailures ?? 0,
                RecoveryAttempts = healthInfo?.RecoveryAttempts ?? 0,
                CheckedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connection health for connection {ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", "Failed to get connection health");
        }
    }

    // Helper method to validate group access
    private async Task<bool> ValidateGroupAccess(string groupName)
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var branchId = Context.User?.FindFirst("BranchId")?.Value;
            var organizationId = Context.User?.FindFirst("OrganizationId")?.Value;
            var roles = Context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();

            // Allow access to own user group
            if (groupName == $"User_{userId}")
                return true;

            // Allow access to own branch group
            if (groupName == $"Branch_{branchId}" && !string.IsNullOrEmpty(branchId))
                return true;

            // Allow access to own organization group
            if (groupName == $"Organization_{organizationId}" && !string.IsNullOrEmpty(organizationId))
                return true;

            // Allow access to role groups if user has that role
            if (groupName.StartsWith("Role_"))
            {
                var roleName = groupName.Substring(5);
                return roles.Contains(roleName);
            }

            // Allow administrators to access any group
            if (roles.Contains("SuperAdmin"))
                return true;

            // Deny access to other groups
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating group access for {GroupName}", groupName);
            return false;
        }
    }

    // Static method to get current connections (for external services)
    public static IReadOnlyDictionary<string, UserConnectionInfo> GetCurrentConnections()
    {
        return _connections.AsReadOnly();
    }
}

// Connection information tracking class
public class UserConnectionInfo
{
    public string ConnectionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string? BranchId { get; set; }
    public string? OrganizationId { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime ConnectedAt { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
}