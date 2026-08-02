# TemporalDDD.Contracts - Project Guidelines

## Purpose

This project contains **shared contracts** (DTOs, events, request/response models) used across different layers of the system. It is the **only** project that should be shared between UI, API, and other external consumers.

## Strict Constraints

### NO Dependencies
The Contracts project must **never** depend on any other project:

- **NO Entity Framework** (EF Core, EF6)
- **NO Temporal** (Temporalio, Temporal activities/workflows)
- **NO Domain layer** (TemporalDDD.Domain)
- **NO Application layer** (TemporalDDD.Application)
- **NO Infrastructure layer** (TemporalDDD.Infrastructure)
- **NO other external frameworks** that introduce business logic

This project must remain **dependency-free** to ensure it can be consumed by any layer without pulling in unwanted dependencies.

### NO Business Logic
Contracts are **pure data structures** only:

- **NO DDD entities** - these belong in Domain layer
- **NO aggregates** - these belong in Domain layer
- **NO value objects** - these belong in Domain layer
- **NO business rules** - these belong in Domain or Application layers
- **NO validation logic** - this belongs in Application or Domain layers
- **NO domain events** - these belong in Application layer

### What Contracts Contains
Only the following types are allowed:

- **DTOs** (Data Transfer Objects) for API requests/responses
- **SignalR events** for real-time communication
- **Simple records/classes** with primitive properties only
- **Enums** for type-safe constants

All types must be **serializable** (JSON-compatible) and contain **only primitive types** (string, int, bool, DateTime, etc.).

### PublicId vs Database Id
Contracts must **never** expose auto-incrementing database Id columns. All database entities use a **PublicId** (GUID) column for external references:

- **Database Id**: Auto-incrementing integer (internal use only, never exposed in contracts)
- **PublicId**: GUID (exposed in contracts for UI, users, and external systems)

When creating DTOs, always use PublicId properties (GUID strings), never database Id properties (integers).

## File Organization

Organize by **feature slices**, not by technical type:

```
TemporalDDD.Contracts/
├── ProviderCredentialing/
│   ├── CredentialEvaluationCreatedEvent.cs
│   ├── CredentialEvaluationApprovedEvent.cs
│   ├── CredentialEvaluationRejectedEvent.cs
│   ├── CredentialEvaluationRequiresManualReviewEvent.cs
│   ├── CredentialingStartResponse.cs
│   └── PendingManualReviewDto.cs
├── ProviderMatching/
│   ├── OfferProposedEvent.cs
│   ├── OfferAcceptedEvent.cs
│   └── MatchingRequestDto.cs
└── TimesheetProcessing/
    ├── TimesheetSubmittedEvent.cs
    └── TimesheetApprovalResponse.cs
```

## Code Examples

### ✅ CORRECT - Pure DTO
```csharp
namespace TemporalDDD.Contracts.ProviderCredentialing;

public sealed record CredentialEvaluationApprovedEvent(
    string EvaluationId,
    string? ComplianceNotes);
```

### ❌ INCORRECT - Contains business logic
```csharp
namespace TemporalDDD.Contracts.ProviderCredentialing;

public sealed record CredentialEvaluationApprovedEvent
{
    public string EvaluationId { get; init; }
    
    // ❌ Business logic - this belongs in Domain layer
    public bool IsValid => !string.IsNullOrEmpty(EvaluationId);
}
```

### ❌ INCORRECT - Depends on Domain
```csharp
using TemporalDDD.Domain.ProviderCredentialing; // ❌ FORBIDDEN

namespace TemporalDDD.Contracts.ProviderCredentialing;

public sealed record CredentialEvaluationApprovedEvent(
    EvaluationId Id); // ❌ Domain type - use string instead
```

### ❌ INCORRECT - Contains validation
```csharp
namespace TemporalDDD.Contracts.ProviderCredentialing;

public sealed record CredentialEvaluationApprovedEvent
{
    private string _evaluationId;
    
    public string EvaluationId
    {
        get => _evaluationId;
        init => _evaluationId = value ?? throw new ArgumentNullException(nameof(value)); // ❌ Validation belongs elsewhere
    }
}
```

## AI Code Generation Rules

When using AI tools to generate code in this project:

1. **NEVER add project references** to Domain, Application, or Infrastructure
2. **NEVER use EF attributes** like `[Key]`, `[Table]`, `[Column]`
3. **NEVER use Temporal attributes** like `[Activity]`, `[Workflow]`
4. **NEVER add business logic** to DTOs - keep them as simple records
5. **ALWAYS use primitive types** (string, int, bool, DateTime) instead of domain types
6. **ALWAYS organize by feature** (ProviderCredentialing, ProviderMatching, etc.)
7. **ALWAYS make types serializable** - no complex nested objects or circular references

## Why These Rules Matter

- **Prevents coupling**: If Contracts depends on Domain, then UI depends on Domain indirectly
- **Maintains clean boundaries**: Each layer should have clear responsibilities
- **Enables independent evolution**: Contracts can change without affecting business logic
- **Prevents dumping ground**: Without strict rules, this becomes a place for "everything else"

## Summary

The Contracts project is a **pure data contract layer**. It contains only simple, serializable DTOs organized by feature. It has **zero dependencies** and **zero business logic**. Any deviation from these rules is a bug that must be fixed.
