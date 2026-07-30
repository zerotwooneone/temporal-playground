using Temporalio.Activities;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Application.ProviderOnboarding;

public interface IProviderActivities
{
    [Activity]
    Task ActivateProvider(ActivateProviderInput input);
}

// Primitive DTOs for activity parameters
public record ActivateProviderInput(uint ProviderId, int Status);
