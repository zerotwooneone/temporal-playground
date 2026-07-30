using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TemporalDDD.Application.ProviderOnboarding;
using TemporalDDD.Application.ProviderCredentialing;
using TemporalDDD.Application.PlacementMatching;
using TemporalDDD.Application.TimesheetProcessing;
using TemporalDDD.Application.TravelLogistics;
using TemporalDDD.Infrastructure.ProviderOnboarding;
using TemporalDDD.Infrastructure.ProviderCredentialing;
using TemporalDDD.Infrastructure.PlacementMatching;
using TemporalDDD.Infrastructure.TimesheetProcessing;
using TemporalDDD.Infrastructure.TravelLogistics;
using TemporalDDD.Infrastructure;
using Temporalio.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Add Database
var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
var dbPath = Path.Combine(localAppData, "TemporalDDD", "temporal_playground.sqlite");
builder.Services.AddDatabase($"Data Source={dbPath}");

// Add Database Initialization Hosted Service
builder.Services.AddHostedService<DatabaseInitializationService>();

// Add Testing utilities
builder.Services.AddTesting();

// Register activities with DI
builder.Services.AddScoped<ComplianceActivities>();
builder.Services.AddScoped<ProviderActivities>();
builder.Services.AddScoped<ProviderCredentialingActivities>();
builder.Services.AddScoped<PlacementMatchingActivities>();
builder.Services.AddScoped<TimesheetProcessingActivities>();
builder.Services.AddScoped<TravelLogisticsActivities>();

// Register the Temporal Worker Service
builder.Services.AddHostedTemporalWorker("localhost:7233", "default", "ONBOARDING_TASK_QUEUE")
    .ConfigureOptions(options =>
    {
        // Register the Workflows
        options.AddWorkflow<ProviderOnboardingWorkflow>();
        options.AddWorkflow<ProviderCredentialingWorkflow>();
        options.AddWorkflow<PlacementMatchingWorkflow>();
        options.AddWorkflow<TimesheetProcessingWorkflow>();
        options.AddWorkflow<TravelLogisticsSagaWorkflow>();
    })
    // Register all Activities using DI
    .AddScopedActivities<ComplianceActivities>()
    .AddScopedActivities<ProviderActivities>()
    .AddScopedActivities<ProviderCredentialingActivities>()
    .AddScopedActivities<PlacementMatchingActivities>()
    .AddScopedActivities<TimesheetProcessingActivities>()
    .AddScopedActivities<TravelLogisticsActivities>();

var app = builder.Build();

Console.WriteLine("Worker started. Press Ctrl+C to exit.");

await app.RunAsync();
