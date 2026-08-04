namespace TemporalDDD.Infrastructure.IdentityAndAccess;

public class UserDbo
{
    public string Id { get; set; }
    public string PublicId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public bool IsActive { get; set; }
    public string AssignedRolesJson { get; set; }
}
