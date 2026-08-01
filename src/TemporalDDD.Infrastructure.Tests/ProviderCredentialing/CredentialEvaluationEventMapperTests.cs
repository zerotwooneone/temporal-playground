using TemporalDDD.Application.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SeedWork;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Infrastructure.ProviderCredentialing;

namespace TemporalDDD.Infrastructure.Tests.ProviderCredentialing;

public class CredentialEvaluationEventMapperTests
{
    private readonly CredentialEvaluationEventMapper _mapper;

    public CredentialEvaluationEventMapperTests()
    {
        _mapper = new CredentialEvaluationEventMapper();
    }

    #region MapToApplicationEvent Tests
    [Fact]
    public void MapToApplicationEvent_WhenEventIsCreated_MapsToCreatedEvent()
    {
        // Arrange
        var providerId = ProviderId.New();
        var evaluationId = CredentialEvaluationId.New();
        var domainEvent = new CredentialEvaluationCreated(evaluationId, providerId, EvaluationStatus.Pending);

        // Act
        var applicationEvent = _mapper.MapToApplicationEvent(domainEvent);

        // Assert
        applicationEvent.Should().BeOfType<CredentialEvaluationCreatedEvent>();
        var createdEvent = applicationEvent as CredentialEvaluationCreatedEvent;
        createdEvent.Should().NotBeNull();
        createdEvent!.EvaluationId.Should().Be(evaluationId.ToString());
        createdEvent.ProviderId.Should().Be(providerId.ToString());
        createdEvent.TargetStatus.Should().Be((int)EvaluationStatus.Pending);
    }

    [Fact]
    public void MapToApplicationEvent_WhenEventIsApproved_MapsToApprovedEvent()
    {
        // Arrange
        var evaluationId = CredentialEvaluationId.New();
        var complianceNotes = ComplianceNotes.Create("All checks passed").Value!;
        var domainEvent = new CredentialEvaluationApproved(evaluationId, complianceNotes);

        // Act
        var applicationEvent = _mapper.MapToApplicationEvent(domainEvent);

        // Assert
        applicationEvent.Should().BeOfType<CredentialEvaluationApprovedEvent>();
        var approvedEvent = applicationEvent as CredentialEvaluationApprovedEvent;
        approvedEvent.Should().NotBeNull();
        approvedEvent!.EvaluationId.Should().Be(evaluationId.ToString());
        approvedEvent.ComplianceNotes.Should().Be("All checks passed");
    }

    [Fact]
    public void MapToApplicationEvent_WhenEventIsRejected_MapsToRejectedEvent()
    {
        // Arrange
        var evaluationId = CredentialEvaluationId.New();
        var complianceNotes = ComplianceNotes.Create("Missing documentation").Value!;
        var domainEvent = new CredentialEvaluationRejected(evaluationId, complianceNotes);

        // Act
        var applicationEvent = _mapper.MapToApplicationEvent(domainEvent);

        // Assert
        applicationEvent.Should().BeOfType<CredentialEvaluationRejectedEvent>();
        var rejectedEvent = applicationEvent as CredentialEvaluationRejectedEvent;
        rejectedEvent.Should().NotBeNull();
        rejectedEvent!.EvaluationId.Should().Be(evaluationId.ToString());
        rejectedEvent.ComplianceNotes.Should().Be("Missing documentation");
    }

    [Fact]
    public void MapToApplicationEvent_WhenEventRequiresManualReview_MapsToManualReviewEvent()
    {
        // Arrange
        var evaluationId = CredentialEvaluationId.New();
        var domainEvent = new CredentialEvaluationRequiresManualReview(evaluationId);

        // Act
        var applicationEvent = _mapper.MapToApplicationEvent(domainEvent);

        // Assert
        applicationEvent.Should().BeOfType<CredentialEvaluationRequiresManualReviewEvent>();
        var manualReviewEvent = applicationEvent as CredentialEvaluationRequiresManualReviewEvent;
        manualReviewEvent.Should().NotBeNull();
        manualReviewEvent!.EvaluationId.Should().Be(evaluationId.ToString());
    }

    [Fact]
    public void MapToApplicationEvent_WhenEventTypeIsUnknown_ThrowsInvalidOperationException()
    {
        // Arrange
        var unknownEvent = new UnknownDomainEvent();

        // Act
        var act = () => _mapper.MapToApplicationEvent(unknownEvent);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Unknown domain event type*");
    }
    #endregion

    #region MapToApplicationEvents Tests
    [Fact]
    public void MapToApplicationEvents_WhenMultipleEvents_MapsAllCorrectly()
    {
        // Arrange
        var providerId = ProviderId.New();
        var evaluationId = CredentialEvaluationId.New();
        var complianceNotes = ComplianceNotes.Create("Approved").Value!;
        
        var domainEvents = new IDomainEvent[]
        {
            new CredentialEvaluationCreated(evaluationId, providerId, EvaluationStatus.Pending),
            new CredentialEvaluationApproved(evaluationId, complianceNotes)
        };

        // Act
        var applicationEvents = ((ICredentialEvaluationEventMapper)_mapper).MapToApplicationEvents(domainEvents).ToList();

        // Assert
        applicationEvents.Should().HaveCount(2);
        applicationEvents[0].Should().BeOfType<CredentialEvaluationCreatedEvent>();
        applicationEvents[1].Should().BeOfType<CredentialEvaluationApprovedEvent>();
    }
    #endregion

    // Helper class for testing unknown event types
    private class UnknownDomainEvent : IDomainEvent
    {
        public DateTimeOffset OccurredOn => new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }
}
