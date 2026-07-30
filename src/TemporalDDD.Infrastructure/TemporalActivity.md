# Temporal Activity Guidelines

This document provides guidelines for creating effective Temporal activities in the TemporalDDD infrastructure layer.

## Core Principles

### 1. Use Primitive DTOs for Activity Parameters
Temporal activities must use a single, primitive-based Request Record (DTO) for their parameters to ensure clean JSON serialization and future-proof versioning.

Activities should convert primitive DTO parameters to Domain types immediately upon entry. Because the workflow that calls the activity should have already validated this data, any failure during this elevation must be treated as a catastrophic system error (InvalidOperationException), not a standard validation error.

**❌ Bad (Domain types or scattered primitives in signature):**
```csharp
// Bad: Domain types break serialization
public async Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(ProviderId provider, LicenseNumber license) 

// Bad: Scattered primitives break Temporal versioning
public async Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(uint providerId, string licenseNumber) 
```

**✅ Good (Single Primitive Input Record with Explicit Assertions):**
```csharp
// 1. Define a strictly primitive Input Record in your Contracts project
public record FetchLicenseInput(uint ProviderId, string LicenseNumber, string MedicalBoard);

// 2. The Activity Signature accepts the single record
[Activity]
public async Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(FetchLicenseInput input)
{
    // 3. Elevate to Domain types instantly using Catastrophic Assertions.
    // We throw InvalidOperationException because the workflow already validated this. 
    // If it fails here, a developer bypassed the workflow validation or our internal queue is corrupted.
    
    var providerIdResult = ProviderId.Create(input.ProviderId);
    if (providerIdResult.IsFailure)
        throw new InvalidOperationException($"Internal Corruption: Invalid ProviderId. {providerIdResult.Error}");
        
    var licenseResult = LicenseNumber.Create(input.LicenseNumber);
    if (licenseResult.IsFailure)
        throw new InvalidOperationException($"Internal Corruption: Invalid License. {licenseResult.Error}");
        
    var boardResult = MedicalBoard.Create(input.MedicalBoard);
    if (boardResult.IsFailure)
        throw new InvalidOperationException($"Internal Corruption: Invalid MedicalBoard. {boardResult.Error}");

    // 4. Extract the guaranteed-valid values
    var providerId = providerIdResult.Value;
    var licenseNumber = licenseResult.Value;
    var medicalBoard = boardResult.Value;

    // ... proceed with pure Domain types
}
```

**Why this is superior:**
- **Versioning**: You can add `string StateCode` to `FetchLicenseInput` tomorrow without breaking the method signature of currently running activities.
- **Clear Intent**: The explicit `throw new InvalidOperationException` clearly signals to other developers (and to Temporal's retry engine) that this is not a user typo—it is a broken system invariant that requires engineering intervention.
- **Temporal Serialization**: System.Text.Json serializes C# record types perfectly out of the box with zero configuration or domain pollution.

### 2. Activities Should Contain Business Logic
Activities are where business logic belongs. Workflows should only orchestrate activities.

**✅ Good:**
```csharp
public async Task<PayrollCalculationResult> CalculatePayrollAndTaxesAsync(TimesheetId timesheetId)
{
    var timesheet = await _timesheetRepository.GetByIdAsync(timesheetId);
    const decimal taxRate = 0.25m;
    timesheet.CalculatePayroll(taxRate);
    await _timesheetRepository.SaveAsync(timesheet);
    
    return new PayrollCalculationResult(
        GrossPay: timesheet.GrossPay.Amount,
        TaxAmount: timesheet.TaxAmount.Amount,
        NetPay: timesheet.NetPay.Amount
    );
}
```

### 2. Interact with External Systems
Activities should handle all external system interactions (APIs, databases, message queues).

```csharp
public async Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(string licenseNumber, string medicalBoard)
{
    var response = await _httpClient.GetAsync($"/api/medical-board/{medicalBoard}/license/{licenseNumber}");
    return await response.Content.ReadFromJsonAsync<MedicalBoardLicenseInfo>();
}
```

### 3. Be Idempotent When Possible
Activities should be idempotent to safely handle retries.

```csharp
public async Task<string> SubmitBankTransferAsync(uint timesheetId, string idempotencyKey)
{
    // The idempotencyKey ensures duplicate requests don't result in duplicate payments
    var response = await _httpClient.PostAsJsonAsync($"/api/payments/transfer", 
        new { TimesheetId = timesheetId, IdempotencyKey = idempotencyKey });
    return await response.Content.ReadAsStringAsync();
}
```

### 4. Have Clear, Single Responsibilities
Each activity should do one thing well. Split complex operations into multiple activities.

**❌ Bad:**
```csharp
public async Task ProcessTimesheetAsync(uint timesheetId)
{
    // Too many responsibilities
    ValidateTimesheet(timesheetId);
    CalculatePayroll(timesheetId);
    SubmitPayment(timesheetId);
    SendInvoice(timesheetId);
}
```

**✅ Good:**
```csharp
public async Task ValidateTimesheetRulesAsync(uint timesheetId) { }
public async Task<PayrollCalculationResult> CalculatePayrollAndTaxesAsync(uint timesheetId) { }
public async Task<string> SubmitBankTransferAsync(uint timesheetId, string idempotencyKey) { }
public async Task<string> GenerateAndSendInvoiceAsync(uint timesheetId, decimal facilityBillRate) { }
```

### 5. Use Domain Types Internally, Primitives for Parameters
Activity parameters should use primitive DTOs (see Principle 1), but activities should convert these to Domain types immediately and use Domain types internally for type safety and self-documenting code.

**❌ Bad (Using primitives throughout):**
```csharp
public async Task<uint> ProposeAssignmentAsync(uint providerId, uint facilityId, decimal score)
{
    // Using primitives throughout - error-prone
    var assignment = new Assignment(providerId, facilityId, score);
    await _repository.SaveAsync(assignment);
    return assignment.Id;
}
```

**✅ Good (Convert to Domain types, use internally):**
```csharp
public async Task<AssignmentId> ProposeAssignmentAsync(ProposeAssignmentInput input)
{
    // Convert primitive DTO to Domain types with fail-fast validation
    var providerIdResult = ProviderId.Create(input.ProviderId);
    if (providerIdResult.IsFailure)
        throw new InvalidOperationException($"Internal Corruption: Invalid ProviderId. {providerIdResult.Error}");
    
    var facilityIdResult = FacilityId.Create(input.FacilityId);
    if (facilityIdResult.IsFailure)
        throw new InvalidOperationException($"Internal Corruption: Invalid FacilityId. {facilityIdResult.Error}");
    
    var positionIdResult = PositionId.Create(input.PositionId);
    if (positionIdResult.IsFailure)
        throw new InvalidOperationException($"Internal Corruption: Invalid PositionId. {positionIdResult.Error}");
    
    var matchScoreResult = MatchScore.Create(input.MatchScore);
    if (matchScoreResult.IsFailure)
        throw new InvalidOperationException($"Internal Corruption: Invalid MatchScore. {matchScoreResult.Error}");

    // Use Domain types internally for type safety
    var assignment = Assignment.Create(
        providerIdResult.Value,
        facilityIdResult.Value,
        positionIdResult.Value,
        matchScoreResult.Value
    );
    
    await _repository.SaveAsync(assignment);
    return assignment.Id;
}
```

### 6. Activities Should Not Make Workflow Orchestration Decisions
Let workflows decide what to do next. Activities should just do their job.

**❌ Bad:**
```csharp
public async Task EvaluateLicenseAsync(LicenseNumber licenseNumber)
{
    var isValid = await CheckLicense(licenseNumber);
    if (isValid)
    {
        await ActivateProvider(); // Orchestration decision in activity
    }
}
```

**✅ Good:**
```csharp
public async Task<EvaluationResult> EvaluateLicenseAsync(LicenseNumber licenseNumber)
{
    var isValid = await CheckLicense(licenseNumber);
    return new EvaluationResult(isValid);
}
// Workflow decides what to do with the result
```

### 7. Activities Should Not Contain Workflow State
Activities should be stateless. Store state in the workflow, not in activities.

**❌ Bad:**
```csharp
public class MyActivities
{
    private Assignment? _assignment; // State in activity
    
    public async Task CreateAssignment(...)
    {
        _assignment = new Assignment(...);
    }
}
```

**✅ Good:**
```csharp
public class MyActivities
{
    public async Task<uint> CreateAssignment(...)
    {
        var assignment = new Assignment(...);
        await _repository.SaveAsync(assignment);
        return assignment.Id;
    }
}
```

### 8. Activities Should Not Call Other Activities Directly
Let the workflow orchestrate activity calls. Activities should be independent.

**❌ Bad:**
```csharp
public async Task ProcessCredentialing(uint providerId)
{
    await FetchLicense(providerId);
    await EvaluateCompliance(providerId); // Calling another activity
}
```

**✅ Good:**
```csharp
// Each activity is independent
public async Task<LicenseInfo> FetchLicenseAsync(ProviderId providerId) { }
public async Task<EvaluationResult> EvaluateComplianceAsync(LicenseInfo licenseInfo) { }
// Workflow orchestrates the sequence
```

### 9. Activities Should Not Call Internal APIs
Activities should not make HTTP calls to the application's own API endpoints (localhost). Instead, execute the logic directly by calling the appropriate application layer interfaces, repositories, or domain methods.

**Rationale**:
- Eliminates unnecessary HTTP overhead and latency
- Avoids serialization/deserialization costs
- Provides compile-time type safety
- Simplifies error handling and debugging
- Reduces network dependencies within the same process
- Enables better testability without HTTP mocking

**❌ Bad:**
```csharp
public async Task<CredentialEvaluationId> EvaluateLicenseAsync(ProviderId providerId, LicenseNumber licenseNumber)
{
    // Making HTTP call to internal API - unnecessary overhead
    var response = await _httpClient.PostAsJsonAsync("/api/providercredentialing/start", new
    {
        ProviderId = providerId.Value,
        LicenseNumber = licenseNumber.Value
    });
    var result = await response.Content.ReadFromJsonAsync<StartCredentialingResponse>();
    return result.EvaluationId;
}
```

**✅ Good:**
```csharp
public async Task<CredentialEvaluationId> EvaluateLicenseAsync(ProviderId providerId, LicenseNumber licenseNumber)
{
    // Execute logic directly using domain and application layer
    var evaluation = CredentialEvaluation.Create(
        providerId,
        licenseNumber,
        MedicalBoard.Create("Default").Value,
        LicenseExpiryDate.Create(DateTimeOffset.UtcNow.AddYears(2)).Value);
    
    evaluation.MarkAsCompliant("License verified successfully");
    await _credentialEvaluationRepository.SaveAsync(evaluation);
    
    return evaluation.Id;
}
```

**Note**: Activities should only make HTTP calls to truly external systems (third-party APIs, external services, etc.), not to the application's own API endpoints.

## Dependency Injection

Activities should use constructor injection for dependencies (repositories, HTTP clients, etc.).

```csharp
public class ProviderCredentialingActivities : IProviderCredentialingActivities
{
    private readonly ICredentialEvaluationRepository _credentialEvaluationRepository;
    private readonly IProviderProfileRepository _providerProfileRepository;
    private readonly ChaosHttpClient _chaosHttpClient;

    public ProviderCredentialingActivities(
        ICredentialEvaluationRepository credentialEvaluationRepository,
        IProviderProfileRepository providerProfileRepository,
        ChaosHttpClient chaosHttpClient)
    {
        _credentialEvaluationRepository = credentialEvaluationRepository;
        _providerProfileRepository = providerProfileRepository;
        _chaosHttpClient = chaosHttpClient;
    }
}
```

## Error Handling

Activities should throw appropriate exceptions for different failure scenarios:

- **Domain exceptions**: For business rule violations
- **Application exceptions**: For expected application failures
- **Infrastructure exceptions**: For external system failures

```csharp
public async Task ActivateProviderProfileAsync(ProviderId providerId)
{
    var providerProfile = await _providerProfileRepository.GetByIdAsync(providerId);
    
    if (providerProfile == null)
    {
        throw new InvalidOperationException($"Provider profile {providerId} not found");
    }
    
    providerProfile.Activate();
    await _providerProfileRepository.SaveAsync(providerProfile);
}
```

## Chaos Engineering

For testing and resilience, consider using chaos simulation for external API calls:

```csharp
public async Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(string licenseNumber, string medicalBoard)
{
    _chaosHttpClient
        .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
        .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

    var response = await _chaosHttpClient.GetAsync($"/api/medical-board/{medicalBoard}/license/{licenseNumber}");
    return await response.Content.ReadFromJsonAsync<MedicalBoardLicenseInfo>();
}
```

## Activity Naming Conventions

- **Activity classes**: `{Feature}Activities` (e.g., `ProviderCredentialingActivities`)
- **Activity methods**: `{Verb}{Noun}Async` (e.g., `FetchMedicalBoardLicenseAsync`, `EvaluateComplianceAsync`)
- **Activity interfaces**: `I{Feature}Activities` (e.g., `IProviderCredentialingActivities`)

## [Activity] Attribute Placement

**Important**: Activities must have the `[Activity]` attribute on BOTH the interface AND the implementation class.

**❌ Bad:**
```csharp
// Interface - missing [Activity] attribute
public interface IProviderCredentialingActivities
{
    Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(LicenseNumber licenseNumber, MedicalBoard medicalBoard);
}

// Implementation - has [Activity] attribute (incomplete)
public class ProviderCredentialingActivities : IProviderCredentialingActivities
{
    [Activity]
    public async Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(LicenseNumber licenseNumber, MedicalBoard medicalBoard)
    {
        // ...
    }
}
```

**✅ Good:**
```csharp
// Interface - has [Activity] attribute
public interface IProviderCredentialingActivities
{
    [Activity]
    Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(LicenseNumber licenseNumber, MedicalBoard medicalBoard);
}

// Implementation - also has [Activity] attribute
public class ProviderCredentialingActivities : IProviderCredentialingActivities
{
    [Activity]
    public async Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(LicenseNumber licenseNumber, MedicalBoard medicalBoard)
    {
        // ...
    }
}
```

**Rationale**: Temporal requires the `[Activity]` attribute on both the interface (for workflow discovery) and the implementation (for worker registration). This ensures proper activity discovery and registration regardless of how the activity is invoked.

## Activity Registration

Register activities in the Worker project's dependency injection container:

```csharp
services.AddTransient<ProviderCredentialingActivities>();
services.AddTransient<PlacementMatchingActivities>();
services.AddTransient<TimesheetProcessingActivities>();
services.AddTransient<TravelLogisticsActivities>();
```

## Testing Considerations

- Activities should be testable without Temporal server
- Use dependency injection to mock external dependencies
- Test both success and failure scenarios
- Consider chaos testing for resilience
- Activities should be deterministic for replay testing
