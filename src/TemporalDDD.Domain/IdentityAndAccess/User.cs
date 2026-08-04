using TemporalDDD.Domain.IdentityAndAccess.Events;
using TemporalDDD.Domain.SeedWork;

namespace TemporalDDD.Domain.IdentityAndAccess;

public sealed class User : AggregateRoot
{
    public UserId Id { get; private set; }
    public UserPublicId PublicId { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<RoleId> _assignedRoles = new();
    public IReadOnlyCollection<RoleId> AssignedRoles => _assignedRoles.AsReadOnly();

    private User() { }

    // Internal constructor for infrastructure rehydration
    internal User(UserId id, UserPublicId publicId, string username, string email, bool isActive, IEnumerable<RoleId> assignedRoles)
    {
        Id = id;
        PublicId = publicId;
        Username = username;
        Email = email;
        IsActive = isActive;
        _assignedRoles.AddRange(assignedRoles);
    }

    // Factory for creating new user
    public static User Create(string username, string email)
    {
        var user = new User
        {
            Id = UserId.New(),
            PublicId = UserPublicId.New(),
            Username = username,
            Email = email,
            IsActive = true
        };
        
        user.RaiseDomainEvent(new UserCreated(user.Id, user.PublicId, user.Username, user.Email));
        return user;
    }

    public void AssignRole(RoleId roleId)
    {
        if (_assignedRoles.Contains(roleId))
            return;

        _assignedRoles.Add(roleId);
        RaiseDomainEvent(new RoleAssignedToUser(Id, roleId));
    }
}
