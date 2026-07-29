using Microsoft.Extensions.Hosting;
using Temporalio.Client;
using Temporalio.Worker;
using TemporalDDD.Application.ProviderOnboarding;
using TemporalDDD.Infrastructure.ProviderOnboarding;

var builder = Host.CreateApplicationBuilder(args);
TemporalDDD.Worker.DatabaseBootstrapper.Initialize(builder.Configuration);

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
