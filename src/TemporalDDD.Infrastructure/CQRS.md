# CQRS (Command Query Responsibility Segregation) Guidelines

This document outlines the CQRS patterns and rules for separating command (write) and query (read) operations in the infrastructure layer.

## Core Principles

### 1. Repository vs Query Interface Separation

**I*Repository Interfaces (Domain Layer)**
- **Purpose**: Handle data mutations (commands) for domain aggregates
- **Location**: Domain layer (within each bounded context)
- **Usage**: When data could potentially be mutated
- **Operations**: Add, Update, Delete, GetById for aggregate roots
- **Scope**: Bound to a single aggregate root within its bounded context

**I*Query Interfaces (Application Layer)**
- **Purpose**: Handle read-only, optimized queries for specific use cases
- **Location**: Application layer (can cross bounded contexts)
- **Usage**: When data is read-only and purpose-built for a specific caller
- **Operations**: Optimized read methods returning DTOs or value types
- **Scope**: Can span multiple bounded contexts for use-case-specific data needs

### 2. When to Use Repository vs Query

**Use I*Repository when:**
- You need to persist or modify a domain aggregate
- The operation is part of a command that changes state
- You need to enforce aggregate invariants and business rules
- The operation is scoped to a single bounded context
- You need optimistic concurrency control (OCC) for the aggregate

**Use I*Query when:**
- You need to read data for display or decision-making
- The operation is read-only and will not mutate state
- You need to join data from multiple bounded contexts
- You need optimized, purpose-built queries for a specific use case
- Performance is critical and you want to avoid loading full aggregates
- The query is called from a single location (use-case specific)

### 3. Query Interface Return Types

**Rule**: Query interfaces MUST NOT return domain aggregates.

**Preferred Return Types (in order of preference):**
1. **Domain Value Objects** - e.g., `Money`, `DateRange`, `PersonName`
2. **Read-Only DTOs** - Simple data transfer objects for complex queries
3. **Primitives** - Only when a single value is needed (e.g., `bool`, `int`)

**Rationale:**
- Aggregates encapsulate business logic and should not be exposed to read paths
- Value objects maintain type safety and domain semantics
- DTOs allow for optimized projections without loading full aggregate graphs
- Prevents accidental mutation of domain state through query results

**Examples:**

```csharp
// ❌ BAD - Returns domain aggregate
public interface IProviderAvailabilityQuery
{
    ProviderProfile GetProviderProfile(ProviderId id);
}

// ✅ GOOD - Returns value objects and DTOs
public interface IProviderAvailabilityQuery
{
    ProviderAvailabilityDto GetProviderAvailability(ProviderId id);
    DateRange GetAvailablePeriod(ProviderId id);
    bool IsAvailable(ProviderId id, DateRange period);
}
```

### 4. Repository Interface Restrictions

**Rule**: Repository interfaces MUST NOT contain query methods.

**Forbidden on Repositories:**
- `GetAll()`, `Find()`, `Search()`, `Query()` methods
- Methods that return DTOs or projections
- Methods that join across aggregates or bounded contexts
- Read-only operations that don't involve aggregate persistence

**Allowed on Repositories:**
- `AddAsync(T aggregate)` - Persist new aggregate
- `UpdateAsync(T aggregate)` - Update existing aggregate
- `DeleteAsync(TId id)` - Delete aggregate
- `GetByIdAsync(TId id)` - Load aggregate by ID (for command operations)
- `GetByIdWithVersionAsync(TId id, int expectedVersion)` - Load with OCC

**Rationale:**
- Repositories are for aggregate lifecycle management, not data access
- Queries should be optimized separately for read performance
- Prevents "leaky" query logic from creeping into domain layer
- Maintains clear separation between command and query responsibilities

**Examples:**

```csharp
// ❌ BAD - Repository with query methods
public interface IAssignmentRepository
{
    Task<Assignment> GetByIdAsync(AssignmentId id);
    Task<IEnumerable<Assignment>> GetPendingAssignmentsAsync(); // Forbidden
    Task<IEnumerable<Assignment>> SearchByProviderAsync(ProviderId providerId); // Forbidden
    Task AddAsync(Assignment assignment);
    Task UpdateAsync(Assignment assignment);
}

// ✅ GOOD - Repository with only aggregate lifecycle methods
public interface IAssignmentRepository
{
    Task<Assignment?> GetByIdAsync(AssignmentId id);
    Task AddAsync(Assignment assignment);
    Task UpdateAsync(Assignment assignment);
    Task DeleteAsync(AssignmentId id);
}
```

### 5. Cross-Domain Query Guidelines

**Rule**: Query interfaces that cross bounded contexts belong in the Application layer.

**Examples:**
- `IProviderAvailabilityQuery` (Application.PlacementMatching) - Queries ProviderCredentialing for placement matching
- `IFacilityBillingQuery` (Application.TimesheetProcessing) - Queries PlacementMatching for billing

**Rationale:**
- Application layer orchestrates use cases that may span multiple bounded contexts
- Domain layer should remain isolated within its context
- Queries are use-case specific and belong at the application coordination level

### 6. Implementation Guidelines

**Repository Implementation:**
- Implement in Infrastructure layer using EF Core or other ORM
- Use `DbSet<T>` for aggregate persistence
- Implement OCC using version columns/ETags where required
- Ensure idempotency for write operations (critical for Temporal workflows)

**Query Implementation:**
- Implement in Infrastructure layer using Dapper, raw SQL, or EF Core projections
- Optimize for read performance (use indexes, avoid unnecessary joins)
- Return read-only data (immutable DTOs or value objects)
- Cache aggressively where appropriate (queries are side-effect free)

## Summary

| Aspect | Repository | Query |
|--------|-----------|-------|
| **Layer** | Domain | Application |
| **Purpose** | Mutate aggregates | Read-optimized data access |
| **Returns** | Domain aggregates | Value objects, DTOs, primitives |
| **Scope** | Single bounded context | Can cross bounded contexts |
| **Queries** | Forbidden | Required |
| **OCC** | Supported | Not applicable |
| **Idempotency** | Required | Not applicable (read-only) |
