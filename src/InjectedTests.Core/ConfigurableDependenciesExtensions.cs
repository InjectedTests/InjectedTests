using InjectedTests.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace InjectedTests;

public static class ConfigurableDependenciesExtensions
{
    public static T ConfigureDependencies<T>(this T bootstrapper, Action<IServiceCollection> configure)
        where T : IConfigurableDependencies
    {
        bootstrapper.ConfigureDependencies(configure);
        return bootstrapper;
    }
}
