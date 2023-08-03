using InjectedTests.Abstractions;

namespace InjectedTests;

public static class ConfigurableDependenciesExtensions
{
    public static T ConfigureDependencies<T>(this T bootstrapper, Action<IDependencyBuilder> configure)
        where T : IConfigurableDependencies
    {
        bootstrapper.ConfigureDependencies(configure);
        return bootstrapper;
    }
}
