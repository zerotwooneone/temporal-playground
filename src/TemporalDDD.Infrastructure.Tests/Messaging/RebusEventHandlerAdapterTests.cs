using Rebus.Messages;
using Rebus.Testing;
using Rebus.Transport;
using TemporalDDD.Application.Messaging;
using TemporalDDD.Application.ProviderCredentialing;
using TemporalDDD.Infrastructure.Messaging;
using Moq;
using Rebus.TestHelpers;

namespace TemporalDDD.Infrastructure.Tests.Messaging;

public class RebusEventHandlerAdapterTests
{
    [Fact]
    public async Task HandleAsync_WhenHandlerExists_CallsHandlerWithCorrectContext()
    {
        // Arrange
        var mockHandler = new Mock<IEventHandler<CredentialEvaluationCreatedEvent>>();
        var handlers = new[] { mockHandler.Object };
        var adapter = new RebusEventHandlerAdapter<CredentialEvaluationCreatedEvent>(handlers);

        var testEvent = new CredentialEvaluationCreatedEvent(
            EvaluationId: "eval-123",
            ProviderId: "provider-456",
            TargetStatus: 1
        );

        var headers = new Dictionary<string, string>
        {
            [Headers.CorrelationId] = "corr-789",
            [Headers.MessageId] = "msg-101"
        };

        var body = Array.Empty<byte>();
        var transportMessage = new TransportMessage(headers, body);

        // Act
        using (new FakeMessageContextScope(transportMessage))
        {
            await adapter.Handle(testEvent);
        }

        // Assert
        mockHandler.Verify(
            h => h.HandleAsync(
                It.Is<IEventContext<CredentialEvaluationCreatedEvent>>(
                    ctx => ctx.Event == testEvent
                        && ctx.CorrelationId == "corr-789"
                        && ctx.Headers == headers
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_WhenMultipleHandlersExist_CallsAllHandlers()
    {
        // Arrange
        var mockHandler1 = new Mock<IEventHandler<CredentialEvaluationCreatedEvent>>();
        var mockHandler2 = new Mock<IEventHandler<CredentialEvaluationCreatedEvent>>();
        var handlers = new[] { mockHandler1.Object, mockHandler2.Object };
        var adapter = new RebusEventHandlerAdapter<CredentialEvaluationCreatedEvent>(handlers);

        var testEvent = new CredentialEvaluationCreatedEvent(
            EvaluationId: "eval-123",
            ProviderId: "provider-456",
            TargetStatus: 1
        );

        var headers = new Dictionary<string, string>
        {
            [Headers.CorrelationId] = "corr-789"
        };

        var body = Array.Empty<byte>();
        var transportMessage = new TransportMessage(headers, body);

        // Act
        using (new FakeMessageContextScope(transportMessage))
        {
            await adapter.Handle(testEvent);
        }

        // Assert
        mockHandler1.Verify(
            h => h.HandleAsync(It.IsAny<IEventContext<CredentialEvaluationCreatedEvent>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        mockHandler2.Verify(
            h => h.HandleAsync(It.IsAny<IEventContext<CredentialEvaluationCreatedEvent>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_WhenNoHandlersExists_DoesNotThrow()
    {
        // Arrange
        var handlers = Enumerable.Empty<IEventHandler<CredentialEvaluationCreatedEvent>>();
        var adapter = new RebusEventHandlerAdapter<CredentialEvaluationCreatedEvent>(handlers);

        var testEvent = new CredentialEvaluationCreatedEvent(
            EvaluationId: "eval-123",
            ProviderId: "provider-456",
            TargetStatus: 1
        );

        var headers = new Dictionary<string, string>();
        var body = Array.Empty<byte>();
        var transportMessage = new TransportMessage(headers, body);

        // Act & Assert
        using (new FakeMessageContextScope(transportMessage))
        {
            var exception = await Record.ExceptionAsync(() => adapter.Handle(testEvent));
            exception.Should().BeNull();
        }
    }

    [Fact]
    public async Task HandleAsync_WhenHeadersContainCustomValues_PassesHeadersToContext()
    {
        // Arrange
        var mockHandler = new Mock<IEventHandler<CredentialEvaluationCreatedEvent>>();
        var handlers = new[] { mockHandler.Object };
        var adapter = new RebusEventHandlerAdapter<CredentialEvaluationCreatedEvent>(handlers);

        var testEvent = new CredentialEvaluationCreatedEvent(
            EvaluationId: "eval-123",
            ProviderId: "provider-456",
            TargetStatus: 1
        );

        var headers = new Dictionary<string, string>
        {
            [Headers.CorrelationId] = "corr-789",
            ["custom-header-1"] = "value-1",
            ["custom-header-2"] = "value-2"
        };

        var body = Array.Empty<byte>();
        var transportMessage = new TransportMessage(headers, body);

        // Act
        using (new FakeMessageContextScope(transportMessage))
        {
            await adapter.Handle(testEvent);
        }

        // Assert
        mockHandler.Verify(
            h => h.HandleAsync(
                It.Is<IEventContext<CredentialEvaluationCreatedEvent>>(
                    ctx => ctx.Headers["custom-header-1"] == "value-1"
                        && ctx.Headers["custom-header-2"] == "value-2"
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
}
