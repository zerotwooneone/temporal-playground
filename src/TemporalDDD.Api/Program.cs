using TemporalDDD.Api;
using Temporalio.Client;
using TemporalDDD.Infrastructure;
using TemporalDDD.Infrastructure.Queries;
using TemporalDDD.Api.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

// Add Database
var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
var dbPath = Path.Combine(localAppData, "TemporalDDD", "temporal_playground.sqlite");
builder.Services.AddDatabase($"Data Source={dbPath}");
builder.Services.AddTimeProvider();
builder.Services.AddInfrastructureQueries();

// Add Database Initialization Hosted Service
builder.Services.AddHostedService<DatabaseInitializationService>();

// Add Testing utilities
builder.Services.AddTesting();

// Add Messaging with RabbitMQ
var rabbitMqConnectionString = builder.Configuration["RabbitMQ:ConnectionString"] ?? throw new InvalidOperationException("RabbitMQ:ConnectionString not found in configuration");
var rabbitMqInputQueue = builder.Configuration["RabbitMQ:InputQueueName"] ?? throw new InvalidOperationException("RabbitMQ:InputQueueName not found in configuration");
builder.Services.AddMessaging(rabbitMqConnectionString, rabbitMqInputQueue);

// Register event handlers via source generator
builder.Services.AddApplicationEventHandlers();

// Add Temporal Client
builder.Services.AddSingleton<ITemporalClient>(sp => 
{
    var client = TemporalClient.ConnectAsync(new("localhost:7233")).GetAwaiter().GetResult();
    return client;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
await app.SubscribeToApplicationEventsAsync();

// Serve React static files
app.UseStaticFiles();

// SPA routing for React app
app.MapFallbackToFile("/react/index.html");

app.MapControllers();
app.MapHub<ApplicationEventHub>("/hubs/applicationevents");

app.Run();
