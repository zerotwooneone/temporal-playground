# Temporal + DDD + Clean Architecture Prototype

This project is a proof-of-concept demonstrating how to integrate Temporal into a strict Domain-Driven Design (DDD) and Clean Architecture (Hexagonal) .NET environment.

## Architectural Goals
1. **Domain Isolation:** The `Domain` project has **zero** external dependencies. It contains pure C# models, aggregates, and domain rules.
2. **Temporal as Application Orchestrator:** The `Application` project contains Temporal Workflows acting as "Process Managers" (Sagas). It defines the interfaces (Ports) for Activities but does not implement them.
3. **Infrastructure as Adapters:** The `Infrastructure` project implements the Temporal Activities, database repositories, and external API clients.
4. **Feature Slices:** Within each layer, code is grouped by feature (e.g., `ProviderOnboarding`) rather than technical concern (e.g., `Models`, `Services`) to maximize cohesion.

## Project Structure
```text
/src
  /TemporalDDD.Domain
    /ProviderOnboarding
      ProviderProfile.cs
      CredentialEvaluation.cs
      ComplianceStatus.cs
  /TemporalDDD.Application
    /ProviderOnboarding
      /Workflows
        IProviderOnboardingWorkflow.cs
        ProviderOnboardingWorkflow.cs
      /Activities
        IComplianceActivities.cs
        IProviderActivities.cs
  /TemporalDDD.Infrastructure
    /ProviderOnboarding
      /Activities
        ComplianceActivities.cs     <-- Implements IComplianceActivities
        ProviderActivities.cs       <-- Implements IProviderActivities
      /Persistence
        ProviderRepository.cs
      /External
        MedicalBoardClient.cs
  /TemporalDDD.Worker
    Program.cs                      <-- Hosts the Temporal Worker
  /TemporalDDD.Api
    Controllers/
      OnboardingController.cs       <-- Starts workflows via HTTP
```

## Prerequisites
* .NET 8 SDK
* Local Temporal CLI (`temporal server start-dev`)

## Getting Started
1. Start the local Temporal dev server: `temporal server start-dev`
2. Run the Worker project: `dotnet run --project src/TemporalDDD.Worker`
3. Run the API project: `dotnet run --project src/TemporalDDD.Api`
4. Trigger a workflow via POST to the API endpoint.