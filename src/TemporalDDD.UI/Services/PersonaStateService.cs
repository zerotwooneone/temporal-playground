namespace TemporalDDD.UI.Services;

public class PersonaStateService
{
    private UserRole _currentUserRole = UserRole.Admin;
    
    public UserRole CurrentUserRole
    {
        get => _currentUserRole;
        set
        {
            if (_currentUserRole != value)
            {
                _currentUserRole = value;
                OnChange?.Invoke();
            }
        }
    }
    
    public event Action? OnChange;
    
    public string GetRoleDisplayName(UserRole role)
    {
        return role switch
        {
            UserRole.Admin => "Admin",
            UserRole.PlacementSpecialist => "Marcus (Placement Specialist)",
            UserRole.Credentialing => "Sarah (Credentialing)",
            UserRole.Provider => "Dr. Emily (Provider)",
            _ => "Unknown"
        };
    }
}
