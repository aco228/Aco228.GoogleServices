using Aco228.Common.Extensions;
using Aco228.Common.Services;
using Aco228.GoogleServices.Models;
using Aco228.GoogleServices.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aco228.GoogleServices;

public static class ServicesExtensions
{
    public static void RegisterGoogleServices(this IServiceCollection services, GoogleSetupOptions googleSetupOptions)
    {
        services.AddSingleton(googleSetupOptions);
        services.AddSingleton<ISecretProvider, GoogleSecret>();
        services.RegisterServicesFromAssembly(typeof(IGoogleClientProvider).Assembly);
    }
}