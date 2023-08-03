using Microsoft.Extensions.DependencyInjection;
using InjectedTests.Abstractions;
using InjectedTests.Extensibility;
using InjectedTests.Internal;

namespace InjectedTests;

public sealed class ServiceProviderBootstrapper :
    IConfigurableDependencies,
    IConfigurableServices,
    IServiceProvider,
    IAsyncDisposable
{
    private readonly BootstrapperStateMachine<IServiceCollection, IServiceProvider> state;

    public ServiceProviderBootstrapper(ServiceProviderOptions? options = null)
    {
        options ??= DefaultOptions;

        state = new(new ServiceProviderBootstrappingStrategy(options));
    }

    private static ServiceProviderOptions DefaultOptions => new ServiceProviderOptions
    {
        ValidateOnBuild = true,
        ValidateScopes = true
    };

    public IServiceProvider Services => state.Bootstrapped;

    public object? GetService(Type serviceType) => Services.GetService(serviceType);

    void IConfigurableDependencies.ConfigureDependencies(Action<IDependencyBuilder> configure)
    {
        state.Configure(s => configure(new ServiceDependencyBuilder(s)));
    }

    void IConfigurableServices.ConfigureServices(Action<IServiceCollection> configure)
    {
        state.Configure(configure);
    }

    public async ValueTask DisposeAsync()
    {
        await state.DisposeAsync().ConfigureAwait(false);
    }
}
