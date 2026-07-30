using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure;

public class DatabaseInitializationService : IHostedService
{
    private const string MutexName = @"Global\TemporalDDDSqliteMigrationMutex";
    private const int MutexWaitTimeoutSeconds = 10;
    private readonly IServiceProvider _serviceProvider;

    public DatabaseInitializationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Use named system mutex for cross-process synchronization
        // This ensures that if API and Worker start simultaneously, only one attempts to migrate
        bool createdNew;
        var mutex = new Mutex(false, MutexName, out createdNew);

        try
        {
            // Wait for mutex with timeout to handle race conditions
            if (!mutex.WaitOne(TimeSpan.FromSeconds(MutexWaitTimeoutSeconds)))
            {
                throw new InvalidOperationException(
                    $"Database initialization timeout after {MutexWaitTimeoutSeconds} seconds. " +
                    "Another process may be holding the initialization lock.");
            }

            try
            {
                // This thread has the lock - perform the migration
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // Ensure the database directory exists
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var dbDirectory = Path.Combine(localAppData, "TemporalDDD");
                if (!Directory.Exists(dbDirectory))
                {
                    Directory.CreateDirectory(dbDirectory);
                }
                
                await dbContext.Database.MigrateAsync(cancellationToken);
            }
            finally
            {
                // Release the mutex so other processes can proceed
                mutex.ReleaseMutex();
            }
        }
        finally
        {
            // Always dispose the mutex
            mutex.Dispose();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
