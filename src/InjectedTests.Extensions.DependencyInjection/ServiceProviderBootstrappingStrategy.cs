using InjectedTests.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace InjectedTests;

internal sealed class ServiceProviderBootstrappingStrategy
    : IBootstrappingStrategy<IServiceCollection, IServiceProvider>
{
    private readonly ServiceProviderOptions options;

    public ServiceProviderBootstrappingStrategy(ServiceProviderOptions options)
    {
        this.options = options;
    }

    public IServiceCollection CreateConfiguration() => new ServiceCollection();

    public ValueTask<IServiceProvider> BootstrapAsync(IServiceCollection configuration)
    {
        var provider = configuration.BuildServiceProvider(options);

        return new ValueTask<IServiceProvider>(provider);
    }

    public IServiceProvider GetServiceProvider(IServiceProvider bootstrapped) => bootstrapped;
}
