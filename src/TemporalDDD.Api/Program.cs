using Temporalio.Client;
using TemporalDDD.Infrastructure;
using TemporalDDD.Api.Messaging;
using TemporalDDD.Api.ProviderCredentialing;

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

// Add Database Initialization Hosted Service
builder.Services.AddHostedService<DatabaseInitializationService>();

// Add Testing utilities
builder.Services.AddTesting();

// Add Messaging with RabbitMQ
builder.Services.AddMessaging("amqp://guest:guest@localhost:5672");

// Register event handlers
builder.Services.AddProviderCredentialingHandlers();

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

app.MapControllers();
app.MapHub<ApplicationEventHub>("/hubs/applicationevents");

app.Run();
