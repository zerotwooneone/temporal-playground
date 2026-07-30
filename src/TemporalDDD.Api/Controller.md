# Controller Guidelines

This document provides guidelines for creating effective API controllers in the TemporalDDD application.

## Core Principles

### 1. Use Feature Slice Folders
Controllers should be organized by feature slice, not by a generic "Controllers" folder. Each controller belongs to its domain feature folder.

**❌ Bad:**
```
src/TemporalDDD.Api/
  Controllers/
    ProviderCredentialingController.cs
    OnboardingController.cs
```

**✅ Good:**
```
src/TemporalDDD.Api/
  ProviderCredentialing/
    ProviderCredentialingController.cs
  ProviderOnboarding/
    OnboardingController.cs
```

**Rationale:**
- Aligns with feature-sliced design
- Keeps related code co-located
- Makes navigation easier for developers
- Scales better as the application grows

### 2. Convert Primitives to Domain Types Using Result<T>
Controllers should convert incoming primitive values to domain types (value objects, entity IDs) using the `Result<T>` pattern. Domain value objects should return `Result<T>` from their `Create` methods instead of throwing exceptions.

**❌ Bad:**
```csharp
[HttpPost]
public async Task<IActionResult> StartWorkflow([FromBody] Request request)
{
    try
    {
        var providerId = ProviderId.Create(request.ProviderId); // May throw
        var licenseNumber = LicenseNumber.Create(request.LicenseNumber); // May throw
        
        await _temporalClient.StartWorkflowAsync(
            (MyWorkflow wf) => wf.RunAsync(providerId, licenseNumber),
            new WorkflowOptions { ... });
        
        return Ok();
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new { Error = ex.Message });
    }
}
```

**✅ Good:**
```csharp
[HttpPost]
public async Task<IActionResult> StartWorkflow([FromBody] Request request)
{
    // Convert primitives to domain types using Result<T>
    var providerIdResult = ProviderId.Create(request.ProviderId);
    var licenseNumberResult = LicenseNumber.Create(request.LicenseNumber);
    
    // Check for validation failures
    if (providerIdResult.IsFailure)
        return BadRequest(new { Error = providerIdResult.Error });
    
    if (licenseNumberResult.IsFailure)
        return BadRequest(new { Error = licenseNumberResult.Error });
    
    await _temporalClient.StartWorkflowAsync(
        (MyWorkflow wf) => wf.RunAsync(providerIdResult.Value, licenseNumberResult.Value),
        new WorkflowOptions { ... });
    
    return Ok();
}
```

**Rationale:**
- Domain types encapsulate validation logic
- `Result<T>` avoids exception handling overhead
- Prevents invalid data from entering the application layer
- Leverages compile-time type checking
- Makes the code self-documenting
- Centralizes validation logic in value objects
- More functional programming style

### 3. Domain Value Objects Should Return Result<T>
Domain value objects should have `Create` methods that return `Result<T>` instead of throwing exceptions. This enables controllers to handle validation failures without try-catch blocks.

**❌ Bad:**
```csharp
public static ProviderId Create(uint value)
{
    if (value == 0)
        throw new ArgumentException("ProviderId cannot be zero", nameof(value));
    
    return new ProviderId(value);
}
```

**✅ Good:**
```csharp
public static Result<ProviderId> Create(uint value)
{
    if (value == 0)
        return Result<ProviderId>.Failure("ProviderId cannot be zero");
    
    return Result<ProviderId>.Success(new ProviderId(value));
}
```

**Rationale:**
- Avoids exception handling overhead for validation
- Makes validation failures explicit in the type system
- Enables functional error handling patterns
- Controllers can handle errors without try-catch
- Better for performance and clarity

## Additional Guidelines

### 4. Use Descriptive Route Attributes
Route attributes should clearly indicate the resource being accessed.

```csharp
[ApiController]
[Route("api/[controller]")] // Resolves to /api/ProviderCredentialing
public class ProviderCredentialingController : ControllerBase
```

### 5. Keep Controllers Thin
Controllers should only handle HTTP concerns: routing, request/response mapping, and error handling. Business logic belongs in workflows and activities.

### 6. Use Records for DTOs
Request and response DTOs should be defined as records for immutability and conciseness.

```csharp
public record StartCredentialingRequest(uint ProviderId, string LicenseNumber, string MedicalBoard, DateTimeOffset ExpiryDate);
public record CredentialEvaluationStatus
{
    public string WorkflowId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    // ...
}
```

### 7. Dependency Injection
Controllers should use constructor injection for dependencies like `ITemporalClient` and repositories.

```csharp
public class ProviderCredentialingController : ControllerBase
{
    private readonly ITemporalClient _temporalClient;
    private readonly ApplicationDbContext _dbContext;

    public ProviderCredentialingController(ITemporalClient temporalClient, ApplicationDbContext dbContext)
    {
        _temporalClient = temporalClient;
        _dbContext = dbContext;
    }
}
```
