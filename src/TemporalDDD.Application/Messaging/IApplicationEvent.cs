using System.Text.Json.Serialization;

namespace TemporalDDD.Application.Messaging;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]

[JsonDerivedType(typeof(ProviderCredentialing.CredentialEvaluationCreatedEvent), "EvaluationCreated")]
[JsonDerivedType(typeof(ProviderCredentialing.CredentialEvaluationApprovedEvent), "EvaluationApproved")]
[JsonDerivedType(typeof(ProviderCredentialing.CredentialEvaluationRejectedEvent), "EvaluationRejected")]
[JsonDerivedType(typeof(ProviderCredentialing.CredentialEvaluationRequiresManualReviewEvent), "EvaluationRequiresManualReview")]
public interface IApplicationEvent
{
}
