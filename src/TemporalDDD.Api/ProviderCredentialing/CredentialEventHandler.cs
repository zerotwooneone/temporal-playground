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
            await _hubContext.Clients.Group(evaluationPublicId).SendAsync("ApplicationEvent", context.Event, cancellationToken);
        }
    }

    public async Task HandleAsync(IEventContext<CredentialEvaluationCreatedEvent> context, CancellationToken cancellationToken)
    {
        var evaluationPublicId = await _evaluationPublicIdQuery.GetEvaluationPublicIdAsync(context.Event.EvaluationId, cancellationToken);
        
        if (evaluationPublicId != null)
        {
            await _hubContext.Clients.Group(evaluationPublicId).SendAsync("ApplicationEvent", context.Event, cancellationToken);
        }
    }

    public async Task HandleAsync(IEventContext<CredentialEvaluationRejectedEvent> context, CancellationToken cancellationToken)
    {
        var evaluationPublicId = await _evaluationPublicIdQuery.GetEvaluationPublicIdAsync(context.Event.EvaluationId, cancellationToken);
        
        if (evaluationPublicId != null)
        {
            await _hubContext.Clients.Group(evaluationPublicId).SendAsync("ApplicationEvent", context.Event, cancellationToken);
        }
    }

    public async Task HandleAsync(IEventContext<CredentialEvaluationRequiresManualReviewEvent> context, CancellationToken cancellationToken)
    {
        var evaluationPublicId = await _evaluationPublicIdQuery.GetEvaluationPublicIdAsync(context.Event.EvaluationId, cancellationToken);
        
        if (evaluationPublicId != null)
        {
            await _hubContext.Clients.Group(evaluationPublicId).SendAsync("ApplicationEvent", context.Event, cancellationToken);
        }
    }
}