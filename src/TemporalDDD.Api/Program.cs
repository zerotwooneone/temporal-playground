using Temporalio.Client;

var builder = WebApplication.CreateBuilder(args);
TemporalDDD.Api.DatabaseBootstrapper.Initialize(builder.Configuration);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

app.Run();
