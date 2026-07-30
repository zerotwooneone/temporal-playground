using Microsoft.Extensions.Hosting;
using Temporalio.Client;
using Temporalio.Worker;
using TemporalDDD.Application.ProviderOnboarding;
using TemporalDDD.Infrastructure.ProviderOnboarding;
using TemporalDDD.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);
DatabaseBootstrapper.Initialize(builder.Configuration);

// Add Database
var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
var dbPath = Path.Combine(localAppData, "TemporalDDD", "temporal_playground.sqlite");
builder.Services.AddDatabase($"Data Source={dbPath}");

var client = await TemporalClient.ConnectAsync(new("localhost:7233"));

var complianceActivities = new ComplianceActivities();
var providerActivities = new ProviderActivities();

using var tokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    tokenSource.Cancel();
    eventArgs.Cancel = true;
};

using var worker = new TemporalWorker(
    client,
    new TemporalWorkerOptions("ONBOARDING_TASK_QUEUE")
        .AddActivity(complianceActivities.PerformComplianceCheck)
        .AddActivity(providerActivities.ActivateProvider)
        .AddWorkflow<ProviderOnboardingWorkflow>());

Console.WriteLine("Worker started. Press Ctrl+C to exit.");

try
{
    await worker.ExecuteAsync(tokenSource.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Worker cancelled");
}
