using Microsoft.Extensions.Configuration;

namespace TemporalDDD.Api;

public static class DatabaseBootstrapper
{
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
}
