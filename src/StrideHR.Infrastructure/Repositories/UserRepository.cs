using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Interfaces;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

/// <summary>
/// User repository implementation
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && !u.IsDeleted);
    }

    public async Task<User?> GetWithEmployeeAsync(int userId)
    {
        return await _dbSet
            .Include(u => u.Employee)
                .ThenInclude(e => e.Branch)
            .Include(u => u.Employee)
                .ThenInclude(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
    }

    public async Task<User?> GetWithRolesAsync(int userId)
    {
        return await _dbSet
            .Include(u => u.Employee)
                .ThenInclude(e => e.EmployeeRoles)
                    .ThenInclude(er => er.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
    }

    public async Task<User?> GetByPasswordResetTokenAsync(string resetToken)
    {
        return await _dbSet
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => 
                u.PasswordResetToken == resetToken && 
                u.PasswordResetTokenExpiry > DateTime.UtcNow && 
                !u.IsDeleted);
    }

    public async Task<User?> GetByEmailVerificationTokenAsync(string verificationToken)
    {
        return await _dbSet
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => 
                u.EmailVerificationToken == verificationToken && 
                !u.IsEmailVerified && 
                !u.IsDeleted);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => 
            u.Email.ToLower() == email.ToLower() && 
            !u.IsDeleted);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbSet.AnyAsync(u => 
            u.Username.ToLower() == username.ToLower() && 
            !u.IsDeleted);
    }

    public async Task<List<User>> GetByStatusAsync(UserStatus status)
    {
        return await _dbSet
            .Include(u => u.Employee)
            .Where(u => u.Status == status && !u.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<User>> GetLockedUsersAsync()
    {
        return await _dbSet
            .Include(u => u.Employee)
            .Where(u => 
                u.Status == UserStatus.Locked && 
                u.LockedUntil > DateTime.UtcNow && 
                !u.IsDeleted)
            .ToListAsync();
    }

    public async Task UpdateLastLoginAsync(int userId, string ipAddress)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = ipAddress;
            user.UpdatedAt = DateTime.UtcNow;
            
            // Reset failed login attempts on successful login
            user.FailedLoginAttempts = 0;
            
            _dbSet.Update(user);
        }
    }

    public async Task IncrementFailedLoginAttemptsAsync(int userId)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.FailedLoginAttempts++;
            user.UpdatedAt = DateTime.UtcNow;
            
            _dbSet.Update(user);
        }
    }

    public async Task ResetFailedLoginAttemptsAsync(int userId)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.FailedLoginAttempts = 0;
            user.UpdatedAt = DateTime.UtcNow;
            
            _dbSet.Update(user);
        }
    }

    public async Task LockUserAsync(int userId, DateTime lockUntil)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.Status = UserStatus.Locked;
            user.LockedUntil = lockUntil;
            user.UpdatedAt = DateTime.UtcNow;
            
            _dbSet.Update(user);
        }
    }

    public async Task UnlockUserAsync(int userId)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.Status = UserStatus.Active;
            user.LockedUntil = null;
            user.FailedLoginAttempts = 0;
            user.UpdatedAt = DateTime.UtcNow;
            
            _dbSet.Update(user);
        }
    }

    public override async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Employee)
            .Where(u => !u.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public override async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
    }
}