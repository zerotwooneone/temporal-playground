namespace TemporalDDD.Application.ProviderOnboarding;

/// <summary>
/// Primitive DTO for Provider Onboarding Workflow input.
/// All fields are primitive types to ensure clean JSON serialization with Temporal.
/// </summary>
public record OnboardingInput(
    string ProviderId,
    string LicenseNumber
);
