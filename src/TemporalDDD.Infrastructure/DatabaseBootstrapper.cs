using Microsoft.Extensions.Configuration;
using System.Threading;

namespace TemporalDDD.Infrastructure;

public static class DatabaseBootstrapper
{
    private const string MutexName = @"Global\TemporalDDD_Sqlite_Init_Mutex";
    private const int MutexWaitTimeoutSeconds = 10;

    public static void Initialize(IConfiguration configuration)
    {
        var runtimeDbName = configuration["DatabaseSettings:RuntimeDbName"] ?? "temporal_playground.sqlite";
        var resetOnStartup = bool.Parse(configuration["DatabaseSettings:ResetOnStartup"] ?? "true");
        var templateToLoad = configuration["DatabaseSettings:TemplateToLoad"] ?? "base_empty.sqlite";

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var temporalDddDir = Path.Combine(localAppData, "TemporalDDD");
        
        if (!Directory.Exists(temporalDddDir))
        {
            Directory.CreateDirectory(temporalDddDir);
        }

        var runtimeDbPath = Path.Combine(temporalDddDir, runtimeDbName);
        var templatePath = Path.Combine(AppContext.BaseDirectory, "DbTemplates", templateToLoad);

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Database template not found at: {templatePath}");
        }

        // Use named system mutex for cross-process synchronization
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
                // This thread has the lock - check and initialize the database
                if (resetOnStartup)
                {
                    File.Copy(templatePath, runtimeDbPath, overwrite: true);
                    Console.WriteLine($"[DatabaseBootstrapper] Reset database from template: {templatePath}");
                }
                else
                {
                    if (!File.Exists(runtimeDbPath))
                    {
                        File.Copy(templatePath, runtimeDbPath, overwrite: false);
                        Console.WriteLine($"[DatabaseBootstrapper] Initialized database from template: {templatePath}");
                    }
                    else
                    {
                        Console.WriteLine($"[DatabaseBootstrapper] Using existing database: {runtimeDbPath}");
                    }
                }
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
}
