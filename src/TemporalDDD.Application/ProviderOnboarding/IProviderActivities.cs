using Temporalio.Activities;
using TemporalDDD.Domain.ProviderCredentialing;

namespace TemporalDDD.Application.ProviderOnboarding;

public interface IProviderActivities
{
    [Activity]
    Task ActivateProvider(uint providerId, EvaluationStatus status);
}
