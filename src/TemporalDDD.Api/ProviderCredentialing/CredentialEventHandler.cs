using Microsoft.AspNetCore.SignalR;
using TemporalDDD.Application.Messaging;
using TemporalDDD.Application.ProviderCredentialing;
using TemporalDDD.Api.Messaging;

namespace TemporalDDD.Api.ProviderCredentialing;

public class CredentialEventHandler : 
    IEventHandler<CredentialEvaluationApprovedEvent>,
    IEventHandler<CredentialEvaluationCreatedEvent>,
    IEventHandler<CredentialEvaluationRejectedEvent>,
    IEventHandler<CredentialEvaluationRequiresManualReviewEvent>
{
    private readonly IHubContext<ApplicationEventHub> _hubContext;
    private readonly IEvaluationPublicIdQuery _evaluationPublicIdQuery;

    public CredentialEventHandler(
        IHubContext<ApplicationEventHub> hubContext,
        IEvaluationPublicIdQuery evaluationPublicIdQuery)
    {
        _hubContext = hubContext;
        _evaluationPublicIdQuery = evaluationPublicIdQuery;
    }

    public async Task HandleAsync(IEventContext<CredentialEvaluationApprovedEvent> context, CancellationToken cancellationToken)
    {
        var evaluationPublicId = await _evaluationPublicIdQuery.GetEvaluationPublicIdAsync(context.Event.EvaluationId, cancellationToken);
        
        if (evaluationPublicId != null)
        {
            var contractEvent = new TemporalDDD.Contracts.ProviderCredentialing.CredentialEvaluationApprovedEvent(evaluationPublicId, context.Event.ComplianceNotes);
            await _hubContext.Clients.Group(evaluationPublicId).SendAsync("CredentialEvaluationApproved", contractEvent, cancellationToken);
        }
    }

    public async Task HandleAsync(IEventContext<CredentialEvaluationCreatedEvent> context, CancellationToken cancellationToken)
    {
        var evaluationPublicId = await _evaluationPublicIdQuery.GetEvaluationPublicIdAsync(context.Event.EvaluationId, cancellationToken);
        
        if (evaluationPublicId != null)
        {
            var contractEvent = new TemporalDDD.Contracts.ProviderCredentialing.CredentialEvaluationCreatedEvent(evaluationPublicId, context.Event.ProviderId, context.Event.TargetStatus);
            await _hubContext.Clients.Group(evaluationPublicId).SendAsync("CredentialEvaluationCreated", contractEvent, cancellationToken);
        }
    }

    public async Task HandleAsync(IEventContext<CredentialEvaluationRejectedEvent> context, CancellationToken cancellationToken)
    {
        var evaluationPublicId = await _evaluationPublicIdQuery.GetEvaluationPublicIdAsync(context.Event.EvaluationId, cancellationToken);
        
        if (evaluationPublicId != null)
        {
            var contractEvent = new TemporalDDD.Contracts.ProviderCredentialing.CredentialEvaluationRejectedEvent(evaluationPublicId, context.Event.ComplianceNotes);
            await _hubContext.Clients.Group(evaluationPublicId).SendAsync("CredentialEvaluationRejected", contractEvent, cancellationToken);
        }
    }

    public async Task HandleAsync(IEventContext<CredentialEvaluationRequiresManualReviewEvent> context, CancellationToken cancellationToken)
    {
        var evaluationPublicId = await _evaluationPublicIdQuery.GetEvaluationPublicIdAsync(context.Event.EvaluationId, cancellationToken);
        
        if (evaluationPublicId != null)
        {
            var contractEvent = new TemporalDDD.Contracts.ProviderCredentialing.CredentialEvaluationRequiresManualReviewEvent(evaluationPublicId);
            await _hubContext.Clients.Group(evaluationPublicId).SendAsync("CredentialEvaluationRequiresManualReview", contractEvent, cancellationToken);
        }
    }
}