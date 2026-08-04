namespace TemporalDDD.Infrastructure.IdentityAndAccess;

public class RoleDbo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string PermissionsJson { get; set; }
}
