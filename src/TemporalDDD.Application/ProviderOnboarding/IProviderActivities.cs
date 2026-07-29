using Temporalio.Activities;
using TemporalDDD.Domain.ProviderOnboarding;

namespace TemporalDDD.Application.ProviderOnboarding;

public interface IProviderActivities
{
    [Activity]
    Task ActivateProvider(uint providerId, ComplianceStatus status);
}
