# Temporal Workflow Guidelines

This document provides guidelines for creating effective Temporal workflows in the TemporalDDD application.

## Core Principles

### 1. Prefer Domain Types as Parameters
Workflows should accept domain types (value objects, entity IDs) rather than primitive types. This ensures type safety and prevents data corruption.

**❌ Bad:**
```csharp
public async Task RunAsync(uint providerId, string licenseNumber, string medicalBoard, DateTime expiryDate)
```

**✅ Good:**
```csharp
public async Task RunAsync(ProviderId providerId, LicenseNumber licenseNumber, MedicalBoard medicalBoard, LicenseExpiryDate expiryDate)
```

**Rationale:**
- Domain types encapsulate validation logic
- Prevents invalid data from entering the workflow
- Makes the workflow self-documenting
- Enables compile-time type checking

### 2. Keep Workflows Orchestrated, Not Operational
Workflows should orchestrate activities, not contain business logic. Business logic belongs in activities or domain entities.

**❌ Bad:**
```csharp
public async Task RunAsync(ProviderId providerId)
{
    // Business logic in workflow
    if (providerId.Value % 2 == 0)
    {
        await ExecuteActivityAsync(...);
    }
}
```

**✅ Good:**
```csharp
public async Task RunAsync(ProviderId providerId)
{
    // Workflow only orchestrates
    var result = await ExecuteActivityAsync((a) => a.EvaluateProvider(providerId));
    
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

### 8. Use Domain Exceptions for Business Failures
Throw domain-specific exceptions for business logic failures, not generic exceptions.

```csharp
if (!_manualReviewSignal?.Approved == true)
{
    throw new ApplicationFailedException("Manual review rejected");
}
```

### 9. Keep Workflow State Minimal
Store only necessary state in workflow instance variables. Avoid storing large objects.

```csharp
// ✅ Good: Store only IDs
private uint? _assignmentId;

// ❌ Bad: Store entire entities
private Assignment? _assignment;
```

### 10. Use Descriptive Workflow IDs
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
new WorkflowOptions { TaskQueue = "CREDENTIALING_TASK_QUEUE" }
```

## Testing Considerations

- Workflows should be testable without Temporal server
- Activities should be mockable
- Domain types should simplify test data creation
- Consider workflow replay testing for long-running workflows
