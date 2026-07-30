using Temporalio.Activities;

namespace TemporalDDD.Application.ProviderOnboarding;

public interface IProviderActivities
{
    [Activity]
    Task ActivateProvider(ActivateProviderInput input);
}

// Primitive DTOs for activity parameters
public record ActivateProviderInput(string ProviderId, int Status);
