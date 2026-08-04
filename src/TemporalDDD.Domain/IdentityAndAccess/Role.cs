using TemporalDDD.Domain.SeedWork;

namespace TemporalDDD.Domain.IdentityAndAccess;

public sealed class Role : AggregateRoot
{
    public RoleId Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    private readonly List<Permission> _permissions = new();
    public IReadOnlyCollection<Permission> Permissions => _permissions.AsReadOnly();

    private Role() { }

    // Internal constructor for infrastructure rehydration
    internal Role(RoleId id, string name, string description, IEnumerable<Permission> permissions)
    {
        Id = id;
        Name = name;
        Description = description;
        _permissions.AddRange(permissions);
    }

    // Factory for creating new role
    public static Role Create(string name, string description)
    {
        var role = new Role
        {
            Id = RoleId.New(),
            Name = name,
            Description = description
        };
        
        return role;
    }

    public void AddPermission(Permission permission)
    {
        if (_permissions.Contains(permission))
            return;

        _permissions.Add(permission);
    }

    public void RemovePermission(Permission permission)
    {
        _permissions.Remove(permission);
    }
}
