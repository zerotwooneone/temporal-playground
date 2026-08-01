namespace TemporalDDD.Application.Messaging;

/// <summary>
/// This is the target of a source generator - any type that implements this interface will be added like
/// [JsonDerivedType(typeof(FullyQualifiedTypeName), "FullyQualifiedTypeName")]
/// to this interface. Implementers should use only primitive types that can be serialized by System.Text.Json.
/// </summary>
public partial interface IApplicationEvent
{
}
