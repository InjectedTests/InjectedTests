using InjectedTests.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace InjectedTests;

internal sealed class ServiceProviderBootstrappingStrategy(ServiceProviderOptions options)
    : IBootstrappingStrategy<IServiceCollection, IServiceProvider>
{
    public IServiceCollection CreateConfiguration() => new ServiceCollection();

    public ValueTask<IServiceProvider> BootstrapAsync(IServiceCollection configuration)
    {
        var provider = configuration.BuildServiceProvider(options);

        return new(provider);
    }

    public ValueTask InitializeAsync(IServiceProvider bootstrapped)
    {
        return bootstrapped.InitializeAsync();
    }
}
