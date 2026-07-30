# Temporal Workflow Guidelines

This document provides guidelines for creating effective Temporal workflows in the TemporalDDD application.

## Core Principles

### 1. Use Primitive DTOs for Workflow Parameters
Temporal workflows and activities must use a single, primitive-based Request Record (DTO) for their parameters to ensure clean JSON serialization and future-proof versioning.

Edge layers (API) must validate user input by attempting to create Domain types. If successful, the API maps the valid data into the primitive DTO and starts the workflow. The workflow acts as an Anti-Corruption Layer, elevating the primitive DTO back into Domain types. Because the data was already guaranteed valid by the API, any failure during this elevation must be treated as a catastrophic system error (InvalidOperationException), not a standard validation error.

**❌ Bad (Domain types or scattered primitives in signature):**
```csharp
// Bad: Domain types break serialization
public async Task RunAsync(ProviderId provider, LicenseNumber license) 

// Bad: Scattered primitives break Temporal versioning
public async Task RunAsync(uint providerId, string licenseNumber) 
```

**✅ Good (Single Primitive Input Record with Explicit Assertions):**
```csharp
// 1. Define a strictly primitive Input Record in your Contracts project
public record CredentialingInput(uint ProviderId, string LicenseNumber, string MedicalBoard, DateTimeOffset ExpiryDate);

// 2. The Workflow Signature accepts the single record
[WorkflowMethod]
public async Task RunAsync(CredentialingInput input)
{
    // 3. Elevate to Domain types instantly using Catastrophic Assertions.
    // We throw InvalidOperationException because the API already validated this. 
    // If it fails here, a developer bypassed the API or our internal queue is corrupted.
    
    var providerIdResult = ProviderId.Create(input.ProviderId);
    if (providerIdResult.IsFailure)
        throw new InvalidOperationException($"Internal Corruption: Invalid ProviderId. {providerIdResult.Error}");
        
    var licenseResult = LicenseNumber.Create(input.LicenseNumber);
    if (licenseResult.IsFailure)
        throw new InvalidOperationException($"Internal Corruption: Invalid License. {licenseResult.Error}");
        
    var boardResult = MedicalBoard.Create(input.MedicalBoard);
    if (boardResult.IsFailure)
        throw new InvalidOperationException($"Internal Corruption: Invalid MedicalBoard. {boardResult.Error}");
        
    var expiryResult = LicenseExpiryDate.Create(input.ExpiryDate);
    if (expiryResult.IsFailure)
        throw new InvalidOperationException($"Internal Corruption: Invalid ExpiryDate. {expiryResult.Error}");

    // 4. Extract the guaranteed-valid values
    var providerId = providerIdResult.Value;
    var licenseNumber = licenseResult.Value;
    var medicalBoard = boardResult.Value;
    var expiryDate = expiryResult.Value;

    // ... proceed with pure Domain types
}
```

**Edge Layer (API/Controller) Example:**
```csharp
[HttpPost]
public async Task<IActionResult> StartCredentialing(CredentialingRequest request)
{
    // 1. Validate at the edge (Fail fast with HTTP 400 - No exceptions thrown)
    var providerIdResult = ProviderId.Create(request.ProviderId);
    if (providerIdResult.IsFailure) return BadRequest(providerIdResult.Error);

    var licenseResult = LicenseNumber.Create(request.LicenseNumber);
    if (licenseResult.IsFailure) return BadRequest(licenseResult.Error);

    // 2. Map the validated domain values into the Primitive DTO
    var workflowInput = new CredentialingInput(
        providerIdResult.Value.Value,
        licenseResult.Value.Value,
        request.MedicalBoard,
        request.ExpiryDate
    );

    // 3. Pass the single JSON-friendly object to Temporal
    await _temporalClient.ExecuteWorkflowAsync(
        (ICredentialingWorkflow wf) => wf.RunAsync(workflowInput),
        workflowOptions
    );

    return Accepted();
}
```

**Why this is superior:**
- **Versioning**: You can add `string StateCode` to `CredentialingInput` tomorrow without breaking the method signature of currently running workflows.
- **Clear Intent**: The explicit `throw new InvalidOperationException` clearly signals to other developers (and to Temporal's retry engine) that this is not a user typo—it is a broken system invariant that requires engineering intervention.
- **Temporal Serialization**: System.Text.Json serializes C# record types perfectly out of the box with zero configuration or domain pollution.

### 2. When to Throw Which Exception
To ensure proper Temporal workflow behavior, we must completely divorce Domain Validation bugs from Business Rule failures. Here is the strict breakdown based on Temporal's execution model.

#### Standard Exceptions (e.g., InvalidOperationException)
**The Concept:** "My code is broken, or my environment is down. Pause everything until a human fixes it."

**Temporal Behavior:** Triggers a Workflow Task Failure. The workflow does not fail. It goes into a suspended state and retries that specific block of code infinitely.

**When to use it:**
- Elevating DTOs to Domain Objects at the workflow boundary (because the API should have already validated this)
- Null reference exceptions
- Database connection failures inside activities

**Correct Example:**
```csharp
// The API promised us this was a valid ID. If it's not, we have a system bug.
var providerIdResult = ProviderId.Create(input.ProviderId);
if (providerIdResult.IsFailure)
{
    // Throwing this freezes the workflow. A dev fixes the bug, restarts the worker, 
    // and the workflow resumes successfully.
    throw new InvalidOperationException($"System invariant broken: {providerIdResult.Error}");
}
```

#### ApplicationFailureException
**The Concept:** "The code works perfectly, the environment is healthy, but a strict Business Rule dictates this process must be permanently terminated."

**Temporal Behavior:** Triggers a Workflow Execution Failure. The workflow immediately terminates. It turns red in the Temporal UI. It will never retry.

**When to use it:**
- A fraud detection service returns a hard "Deny" on an applicant
- A user explicitly cancels an order that is past the point of no return
- An external regulatory board rejects a medical license

**Correct Example:**
```csharp
[WorkflowMethod]
public async Task RunAsync(CredentialingInput input)
{
    // 1. DTO -> Domain Object (Using standard exception for code bugs)
    var providerId = ProviderId.Create(input.ProviderId).Value 
        ?? throw new InvalidOperationException("Invalid ID");

    // 2. Business Logic Execution
    var boardStatus = await Activities.CheckMedicalBoardAsync(providerId);

    if (boardStatus == "LicenseRevoked")
    {
        // The system isn't broken; the doctor is legally disqualified.
        // We permanently kill the workflow execution.
        throw ApplicationFailureException.New(
            "Applicant's medical license is revoked. Credentialing permanently failed.",
            "BusinessRuleViolation"
        );
    }
}
```

**Summary:**
Never throw an `ApplicationFailureException` for a Domain validation failure (`IsFailure`) at the entry point of a workflow. Domain validation failures at the workflow boundary imply corrupted system state, which requires an `InvalidOperationException` to safely pause the workflow via a Task Failure. Reserve `ApplicationFailureException` strictly for explicit business process terminations.

### 3. Keep Workflows Orchestrated, Not Operational
Workflows should orchestrate activities, not contain business logic. Business logic belongs in activities or domain entities.

**❌ Bad:**
```csharp
public async Task RunAsync(uint providerId)
{
    // Business logic in workflow
    if (providerId % 2 == 0)
    {
        await ExecuteActivityAsync(...);
    }
}
```

**✅ Good:**
```csharp
public async Task RunAsync(uint providerId)
{
    // Convert to domain type for internal use
    var providerIdResult = ProviderId.Create(providerId);
    if (providerIdResult.IsFailure)
        throw new ArgumentException($"Invalid provider ID: {providerIdResult.Error}");
    
    var validatedProviderId = providerIdResult.Value;
    
    // Workflow only orchestrates
    var result = await ExecuteActivityAsync((a) => a.EvaluateProvider(validatedProviderId));
    
    if (result.RequiresManualReview)
    {
        await ExecuteActivityAsync((a) => a.RequestManualReview(result.EvaluationId));
    }
}
```

### 3. Use Signals for External Events
Use workflow signals to handle external events that occur asynchronously (e.g., manual review completion, offer acceptance).

```csharp
[WorkflowSignal]
public async Task ManualReviewCompletedAsync(bool approved, string? notes = null)
{
    _manualReviewSignal = new ManualReviewCompletedSignal(approved, notes);
}
```

### 4. Implement Idempotency
Workflows should be idempotent. Use workflow IDs as idempotency keys for external operations (e.g., payment gateways).

```csharp
var idempotencyKey = Workflow.Info.WorkflowId;
await ExecuteActivityAsync((a) => a.SubmitPayment(amount, idempotencyKey));
```

### 5. Handle Compensation in Sagas
For multi-step operations that need rollback, implement compensating transactions in exception handlers.

```csharp
try
{
    _flightId = await ExecuteActivityAsync((a) => a.BookFlight(...));
    _hotelId = await ExecuteActivityAsync((a) => a.BookHotel(...));
}
catch (Exception ex)
{
    if (_flightId.HasValue)
        await ExecuteActivityAsync((a) => a.CancelFlight(_flightId.Value));
    if (_hotelId.HasValue)
        await ExecuteActivityAsync((a) => a.CancelHotel(_hotelId.Value));
    throw;
}
```

### 6. Use WaitCondition for Synchronization
Use `Workflow.WaitConditionAsync` to wait for signals or other conditions, avoiding busy-waiting.

```csharp
await Workflow.WaitConditionAsync(() => 
    _offerAcceptedSignal is not null || 
    _offerRejectedSignal is not null
);
```

### 7. Set Appropriate Timeouts
Always set `StartToCloseTimeout` for activities to prevent hanging workflows.

```csharp
await ExecuteActivityAsync(
    (a) => a.PerformOperation(...),
    new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
);
```

### 8. Keep Workflow State Minimal
Store only necessary state in workflow instance variables. Use primitive types for workflow state to ensure serializability. Avoid storing large objects or domain types.

```csharp
// ✅ Good: Store only primitive IDs
private uint? _assignmentId;

// ❌ Bad: Store entire entities or domain types
private Assignment? _assignment;
private AssignmentId? _assignmentId;
```

### 9. Use Descriptive Workflow IDs
Generate workflow IDs that are both human-readable and unique.

```csharp
var workflowId = $"provider-credentialing-{providerId.Value}-{Guid.NewGuid():N}";
```

## Workflow Naming Conventions

- **Workflow classes**: `{Feature}Workflow` (e.g., `ProviderCredentialingWorkflow`)
- **Signal methods**: `{Event}Async` (e.g., `ManualReviewCompletedAsync`)
- **Signal records**: `{Event}Signal` (e.g., `ManualReviewCompletedSignal`)
- **Workflow IDs**: `{feature}-{entity}-{id}-{guid}` (e.g., `provider-credentialing-123-abc123`)

## Task Queue Naming

Use feature-specific task queues to enable independent scaling:

```csharp
new WorkflowOptions { TaskQueue = "ONBOARDING_TASK_QUEUE" }
```

## Testing Considerations

- Workflows should be testable without Temporal server
- Activities should be mockable
- Domain types should simplify test data creation
- Consider workflow replay testing for long-running workflows
