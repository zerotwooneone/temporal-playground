using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TemporalDDD.Application.PlacementMatching;
using TemporalDDD.Application.ProviderCredentialing;
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


        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));

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
        services.AddScoped<ICredentialEvaluationStatusQuery, CredentialEvaluationStatusQuery>();
        services.AddScoped<IPendingManualReviewsQuery, PendingManualReviewsQuery>();

        // Register event mappers
        services.AddScoped<ICredentialEvaluationEventMapper, CredentialEvaluationEventMapper>();

        return services;
    }

    public static IServiceCollection AddTesting(this IServiceCollection services)
    {
        // Register Random as singleton for consistent chaos simulation
        services.AddSingleton<Random>(sp => new Random());

        // Register testing utilities for chaos simulation
        services.AddTransient<ChaosHttpClient>();

        return services;
    }
}
