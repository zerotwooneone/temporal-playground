using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.IdentityAndAccess;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
    {
        var dbo = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id.ToString(), cancellationToken);

        if (dbo == null) return null;

        return MapToDomain(dbo);
    }

    public async Task SaveAsync(User aggregate, CancellationToken cancellationToken = default)
    {
        var id = aggregate.Id.ToString();
        var existing = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (existing == null)
        {
            existing = new UserDbo();
            MapToDbo(aggregate, existing);
            _dbContext.Users.Add(existing);
        }
        else
        {
            MapToDbo(aggregate, existing);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private User MapToDomain(UserDbo dbo)
    {
        var id = UserId.Create(dbo.Id).Value ?? throw new InvalidOperationException($"Invalid UserId in database: {dbo.Id}");
        var publicId = UserPublicId.Create(dbo.PublicId).Value ?? throw new InvalidOperationException($"Invalid UserPublicId in database: {dbo.PublicId}");

        // Deserialize AssignedRoles JSON
        var roleIds = JsonSerializer.Deserialize<List<string>>(dbo.AssignedRolesJson) ?? new List<string>();
        var assignedRoles = roleIds
            .Select(roleIdStr => RoleId.Create(roleIdStr).Value)
            .Where(roleId => roleId != null)
            .ToList()!;

        // Use internal constructor for rehydration
        return new User(
            id: id,
            publicId: publicId,
            username: dbo.Username,
            email: dbo.Email,
            isActive: dbo.IsActive,
            assignedRoles: assignedRoles
        );
    }

    private void MapToDbo(User user, UserDbo dbo)
    {
        dbo.Id = user.Id.ToString();
        dbo.PublicId = user.PublicId.ToString();
        dbo.Username = user.Username;
        dbo.Email = user.Email;
        dbo.IsActive = user.IsActive;

        // Serialize AssignedRoles to JSON
        var roleIds = user.AssignedRoles.Select(roleId => roleId.ToString()).ToList();
        dbo.AssignedRolesJson = JsonSerializer.Serialize(roleIds);
    }
}
