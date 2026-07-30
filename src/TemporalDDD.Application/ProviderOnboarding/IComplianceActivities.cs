using Temporalio.Activities;

namespace TemporalDDD.Application.ProviderOnboarding;

public interface IComplianceActivities
{
    [Activity]
    Task<int> PerformComplianceCheck(PerformComplianceInput input);
}

// Primitive DTOs for activity parameters
public record PerformComplianceInput(string LicenseNumber);
