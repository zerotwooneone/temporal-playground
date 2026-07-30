using Temporalio.Activities;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;

namespace TemporalDDD.Application.ProviderOnboarding;

public interface IComplianceActivities
{
    [Activity]
    Task<int> PerformComplianceCheck(PerformComplianceInput input);
}

// Primitive DTOs for activity parameters
public record PerformComplianceInput(string LicenseNumber);
