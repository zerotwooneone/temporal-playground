using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using TemporalDDD.Application.IdentityAndAccess;
using TemporalDDD.Application.Messaging;
using TemporalDDD.Application.PlacementMatching;
using TemporalDDD.Application.ProviderCredentialing;
using TemporalDDD.Application.TimesheetProcessing;
using TemporalDDD.Application.WorkflowOrchestration;
using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.PlacementMatching;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TimesheetProcessing;
using TemporalDDD.Domain.TravelLogistics;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Infrastructure.IdentityAndAccess;
using TemporalDDD.Infrastructure.Messaging;
using TemporalDDD.Infrastructure.Persistence;
using TemporalDDD.Infrastructure.SharedKernel;
using TemporalDDD.Infrastructure.Testing;
using TemporalDDD.Infrastructure.ProviderCredentialing;
using TemporalDDD.Infrastructure.PlacementMatching;
using TemporalDDD.Infrastructure.TimesheetProcessing;
using TemporalDDD.Infrastructure.TravelLogistics;
using TemporalDDD.Infrastructure.WorkflowOrchestration;

namespace TemporalDDD.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTimeProvider(this IServiceCollection services)
    {
        services.AddSingleton<ITimeProvider, SystemTimeProvider>();
        return services;
    }

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
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();

        // Register queries
        services.AddScoped<IFacilityRequirementsQuery, FacilityRequirementsQuery>();
        services.AddScoped<IProviderAvailabilityQuery, ProviderAvailabilityQuery>();
        services.AddScoped<IFacilityBillingQuery, FacilityBillingQuery>();
        services.AddScoped<ICredentialEvaluationStatusQuery, CredentialEvaluationStatusQuery>();
        services.AddScoped<IEvaluationPublicIdQuery, EvaluationPublicIdQuery>();
        services.AddScoped<IWorkflowDefinitionQuery, WorkflowDefinitionQuery>();

        // Register event mappers
        services.AddScoped<ICredentialEvaluationEventMapper, CredentialEvaluationEventMapper>();
        services.AddScoped<IIdentityEventMapper, IdentityEventMapper>();
        services.AddScoped<IWorkflowEventMapper, WorkflowEventMapper>();

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

    public static IServiceCollection AddMessaging(this IServiceCollection services, string connectionString, string inputQueueName)
    {
        services.AddRebus(configure => configure
            .Transport(t => t.UseRabbitMq(connectionString, inputQueueName)))
            ;
        
        services.AddSingleton<IMessagePublisher, RebusMessagePublisher>();

        return services;
    }
}
