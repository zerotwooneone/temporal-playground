using Microsoft.Extensions.DependencyInjection;
using TemporalDDD.Application.ProviderCredentialing;
using TemporalDDD.Infrastructure.Queries.ProviderCredentialing;

namespace TemporalDDD.Infrastructure.Queries;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureQueries(this IServiceCollection services)
    {
        services.AddScoped<IPendingManualReviewsQuery, PendingManualReviewsQuery>();
        return services;
    }
}
