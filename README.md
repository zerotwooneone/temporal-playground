# Temporal + DDD + Clean Architecture Prototype

This project is a proof-of-concept demonstrating how to integrate Temporal into a strict Domain-Driven Design (DDD) and Clean Architecture (Hexagonal) .NET environment.

## Architectural Goals
1. **Domain Isolation:** The `Domain` project has **zero** external dependencies. It contains pure C# models, aggregates, and domain rules.
2. **Temporal as Application Orchestrator:** The `Application` project contains Temporal Workflows acting as "Process Managers" (Sagas). It defines the interfaces (Ports) for Activities but does not implement them.
3. **Infrastructure as Adapters:** The `Infrastructure` project implements the Temporal Activities, database repositories, and external API clients.
4. **Feature Slices:** Within each layer, code is grouped by feature (e.g., `ProviderOnboarding`) rather than technical concern (e.g., `Models`, `Services`) to maximize cohesion.

## Project Structure

- **TemporalDDD.Domain**
  - Pure domain models, aggregates, and business rules (no external dependencies)
  - [DomainDrivenDesign.md](src/TemporalDDD.Domain/DomainDrivenDesign.md)

- **TemporalDDD.Application**
  - Temporal workflows and activity interfaces (orchestration layer)
  - [TemporalWorkflow.md](src/TemporalDDD.Application/TemporalWorkflow.md)

- **TemporalDDD.Infrastructure**
  - Activity implementations, repositories, external API clients
  - [TemporalActivity.md](src/TemporalDDD.Infrastructure/TemporalActivity.md)
  - [CQRS.md](src/TemporalDDD.Infrastructure/CQRS.md)

- **TemporalDDD.Worker**
  - Temporal worker host

- **TemporalDDD.Api**
  - HTTP API for starting workflows (port 5000/5001)
  - [Controller.md](src/TemporalDDD.Api/Controller.md)

- **TemporalDDD.UI**
  - Blazor UI for workflow management (port 5000/5001)

## Prerequisites
* .NET 10 SDK
* Local Temporal CLI (`temporal server start-dev`)

## Getting Started
1. Start the local Temporal dev server: `temporal server start-dev`
2. Run the Worker project: `dotnet run --project src/TemporalDDD.Worker`
3. Run the API project: `dotnet run --project src/TemporalDDD.Api`
4. Run the UI project: `dotnet run --project src/TemporalDDD.UI`