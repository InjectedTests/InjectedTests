using InjectedTests.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace InjectedTests;

public static class ConfigurableBootstrapperExtensions
{
    public static T ConfigureServices<T>(this T bootstrapper, Action<IServiceCollection> configure)
        where T : IConfigurableBootstrapper
    {
        bootstrapper.ConfigureServices(configure);
        return bootstrapper;
    }
}
