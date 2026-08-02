# TemporalDDD.UI - Project Guidelines

## Architecture Principles

### Shared Nothing Architecture
The UI project must **not** depend on any other project layer (Application, Domain, Infrastructure). This is a strict boundary:

- **NO references to TemporalDDD.Application**
- **NO references to TemporalDDD.Domain**  
- **NO references to TemporalDDD.Infrastructure**

### Rationale
- UI is a separate boundary that should have its own contracts
- Changes to internal layers (Application/Domain) should not break the UI
- SignalR contracts are independent of event bus/Temporal serialization
- Prevents accidental coupling to implementation details

## Data Transfer Objects (DTOs)

### PublicId vs Database Id
The UI project must **never** have access to auto-incrementing database Id columns. All database entities use a **PublicId** (GUID) column for external references:

- **Database Id**: Auto-incrementing integer (internal use only, never exposed to UI)
- **PublicId**: GUID (exposed to UI, users, and external systems)

### When to Duplicate DTOs
If the UI needs to receive data from backend APIs via SignalR or HTTP:

1. **Create duplicate DTO definitions in the UI project** (e.g., in `Messaging/` or `Models/` directories)
2. **Mirror the structure** of backend DTOs exactly
3. **Use local namespace** (e.g., `TemporalDDD.UI.Messaging`)
4. **Always use PublicId properties** (GUID strings), never database Id properties (integers)

### Example
Instead of:
```csharp
@using TemporalDDD.Application.ProviderCredentialing  // ❌ FORBIDDEN
```

Do this:
```csharp
@using TemporalDDD.UI.Messaging  // ✅ CORRECT

// Create local duplicate:
// TemporalDDD.UI/Messaging/CredentialEvaluationCreatedEvent.cs
public sealed record CredentialEvaluationCreatedEvent(
    string EvaluationId,
    string ProviderId,
    int TargetStatus);
```

## Clean Architecture Boundaries

### Allowed Dependencies
- **UI → API**: Via HTTP calls (IHttpClientFactory)
- **UI → SignalR**: Via SignalR Client (Microsoft.AspNetCore.SignalR.Client)
- **UI → Internal Services**: Services defined within UI project (e.g., PersonaStateService)

### Forbidden Dependencies
- **UI → Application**: Never reference Application layer types
- **UI → Domain**: Never reference Domain layer types
- **UI → Infrastructure**: Never reference Infrastructure layer types

## SignalR Integration

### Event Handling Pattern
When receiving events via SignalR:

1. **Use specific SignalR method names per event type** (not generic "ApplicationEvent")
2. **Create typed handlers** using local DTO definitions
3. **Keep SignalR contracts independent** of event bus serialization

### Correct Pattern
```csharp
// Backend (CredentialEventHandler.cs)
await _hubContext.Clients.Group(evaluationPublicId)
    .SendAsync("CredentialEvaluationApproved", context.Event, cancellationToken);

// Frontend (ProviderCredentialing.razor)
_hubConnection.On<Messaging.CredentialEvaluationApprovedEvent>(
    "CredentialEvaluationApproved", 
    async (eventData) => { /* handle */ });
```

### Incorrect Pattern
```csharp
// ❌ Do not use generic event handler
_hubConnection.On<object>("ApplicationEvent", async (message) => { 
    // Requires deserialization and depends on JsonDerivedType implementation detail
});
```

## HTTP API Integration

### API Calls
- Use `IHttpClientFactory` to create HTTP clients
- Define request/response DTOs locally in UI project
- Do not share DTOs with API project

## Code Generation Guidelines

When using AI code generation tools, enforce these rules:

1. **Never add project references** to Application/Domain/Infrastructure
2. **Always create local DTOs** for API/SignalR contracts
3. **Use specific SignalR methods** per event type
4. **Keep UI contracts independent** of internal serialization concerns

## File Organization

Organize by **feature slices**, not by technical type:

```
TemporalDDD.UI/
├── Features/
│   ├── ProviderCredentialing/
│   │   ├── Pages/
│   │   │   └── Workflows/
│   │   │       └── ProviderCredentialing.razor
│   │   ├── Messaging/           # SignalR event DTOs for this feature
│   │   │   ├── CredentialEvaluationCreatedEvent.cs
│   │   │   ├── CredentialEvaluationApprovedEvent.cs
│   │   │   ├── CredentialEvaluationRejectedEvent.cs
│   │   │   └── CredentialEvaluationRequiresManualReviewEvent.cs
│   │   └── Models/              # HTTP API DTOs for this feature
│   ├── ProviderMatching/
│   └── TimesheetProcessing/
├── Shared/
│   ├── Components/              # Reusable Razor components
│   └── Services/                # Cross-cutting UI services (e.g., PersonaStateService)
└── Pages/                       # Non-feature-specific pages (if any)
```

## Summary

The UI project is a **boundary layer** with its own contracts. Duplication of DTOs is intentional and correct - it maintains clean separation and prevents cascading changes from breaking the UI.
