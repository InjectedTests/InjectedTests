using InjectedTests.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace InjectedTests;

public static class ConfigurableServicesExtensions
{
    public static T ConfigureServices<T>(this T services, Action<IServiceCollection> configure)
        where T : IConfigurableServices
    {
        services.ConfigureServices(configure);
        return services;
    }
}
