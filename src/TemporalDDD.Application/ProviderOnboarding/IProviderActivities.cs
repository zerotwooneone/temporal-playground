using Temporalio.Activities;
using TemporalDDD.Domain.ProviderOnboarding;

namespace TemporalDDD.Application.ProviderOnboarding;

public interface IProviderActivities
{
    [Activity]
    Task ActivateProvider(string providerId, ComplianceStatus status);
}
