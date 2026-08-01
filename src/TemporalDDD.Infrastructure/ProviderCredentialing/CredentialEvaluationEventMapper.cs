using TemporalDDD.Application.Messaging;
using TemporalDDD.Application.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.SeedWork;

namespace TemporalDDD.Infrastructure.ProviderCredentialing;

public class CredentialEvaluationEventMapper : ICredentialEvaluationEventMapper
{
    public IApplicationEvent MapToApplicationEvent(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            Domain.ProviderCredentialing.CredentialEvaluationCreated e => 
                new Application.ProviderCredentialing.CredentialEvaluationCreatedEvent(
                    EvaluationId: e.EvaluationId.ToString(),
                    ProviderId: e.ProviderId.ToString(),
                    TargetStatus: (int)e.TargetStatus),
            
            Domain.ProviderCredentialing.CredentialEvaluationApproved e => 
                new Application.ProviderCredentialing.CredentialEvaluationApprovedEvent(
                    EvaluationId: e.EvaluationId.ToString(),
                    ComplianceNotes: e.ComplianceNotes?.Value),
            
            Domain.ProviderCredentialing.CredentialEvaluationRejected e => 
                new Application.ProviderCredentialing.CredentialEvaluationRejectedEvent(
                    EvaluationId: e.EvaluationId.ToString(),
                    ComplianceNotes: e.ComplianceNotes.Value),
            
            Domain.ProviderCredentialing.CredentialEvaluationRequiresManualReview e => 
                new Application.ProviderCredentialing.CredentialEvaluationRequiresManualReviewEvent(
                    EvaluationId: e.EvaluationId.ToString()),
            
            _ => new Application.Messaging.UnknownTypeEvent(
                new InvalidOperationException($"Unknown domain event type: {domainEvent.GetType().Name}").ToString())
        };
    }
}
