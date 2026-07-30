using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TemporalDDD.Application.PlacementMatching;
using TemporalDDD.Application.TimesheetProcessing;
using TemporalDDD.Domain.PlacementMatching;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.TimesheetProcessing;
using TemporalDDD.Domain.TravelLogistics;
using TemporalDDD.Infrastructure.Persistence;
using TemporalDDD.Infrastructure.Testing;
using TemporalDDD.Infrastructure.ProviderCredentialing;
using TemporalDDD.Infrastructure.PlacementMatching;
using TemporalDDD.Infrastructure.TimesheetProcessing;
using TemporalDDD.Infrastructure.TravelLogistics;

namespace TemporalDDD.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        // Configure SQLite with WAL mode for better concurrency
        var connectionStringWithWal = $"{connectionString};Journal Mode=WAL;BusyTimeout=5000;";
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionStringWithWal));

        // Register repositories
        services.AddScoped<IProviderProfileRepository, ProviderProfileRepository>();
        services.AddScoped<ICredentialEvaluationRepository, CredentialEvaluationRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        services.AddScoped<ITimesheetRepository, TimesheetRepository>();
        services.AddScoped<IFlightBookingRepository, FlightBookingRepository>();
        services.AddScoped<ILodgingBookingRepository, LodgingBookingRepository>();

        // Register queries
        services.AddScoped<IFacilityRequirementsQuery, FacilityRequirementsQuery>();
        services.AddScoped<IProviderAvailabilityQuery, ProviderAvailabilityQuery>();
        services.AddScoped<IFacilityBillingQuery, FacilityBillingQuery>();

        return services;
    }

    public static IServiceCollection AddTesting(this IServiceCollection services)
    {
        // Register testing utilities for chaos simulation
        services.AddTransient<ChaosHttpClient>();

        return services;
    }
}
