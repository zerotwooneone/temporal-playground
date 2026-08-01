using Microsoft.Extensions.DependencyInjection;
using TemporalDDD.Application.ProviderCredentialing;

namespace TemporalDDD.Api.ProviderCredentialing;

public static class DependencyInjection
{
    public static IServiceCollection AddProviderCredentialingHandlers(this IServiceCollection services)
    {
        services.AddScoped<CredentialEventHandler>();
        return services;
    }
}
