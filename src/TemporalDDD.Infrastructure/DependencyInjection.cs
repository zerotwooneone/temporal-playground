using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TemporalDDD.Infrastructure.Persistence;
using TemporalDDD.Infrastructure.Testing;

namespace TemporalDDD.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        // Configure SQLite with WAL mode for better concurrency
        var connectionStringWithWal = $"{connectionString};Journal Mode=WAL;BusyTimeout=5000;";
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionStringWithWal));

        return services;
    }

    public static IServiceCollection AddTesting(this IServiceCollection services)
    {
        // Register testing utilities for chaos simulation
        services.AddTransient<ChaosHttpClient>();

        return services;
    }
}
