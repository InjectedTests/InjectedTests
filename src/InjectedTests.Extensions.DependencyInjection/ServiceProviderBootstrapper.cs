using Microsoft.Extensions.DependencyInjection;
using InjectedTests.Abstractions;
using InjectedTests.Extensibility;

namespace InjectedTests;

public sealed class ServiceProviderBootstrapper :
    IConfigurableBootstrapper,
    IInitializableBootstrapper,
    IServiceProvider,
    IAsyncDisposable
{
    private readonly BootstrapperStateMachine<IServiceCollection, IServiceProvider> state;

    public ServiceProviderBootstrapper(ServiceProviderOptions? options = null)
    {
        options ??= DefaultOptions;

        state = new(new ServiceProviderBootstrappingStrategy(options));
    }

    private static ServiceProviderOptions DefaultOptions => new()
    {
        ValidateOnBuild = false,
        ValidateScopes = true
    };

    public IServiceProvider Services => state.Bootstrapped;

    public object? GetService(Type serviceType) => Services.GetService(serviceType);

    void IConfigurableBootstrapper.ConfigureServices(Action<IServiceCollection> configure)
    {
        state.Configure(configure);
    }

    void IInitializableBootstrapper.ConfigureInitializer(Action<IInitializerBuilder> configure)
    {
        ((IConfigurableBootstrapper)this).ConfigureInitializer(configure);
    }

    public async ValueTask DisposeAsync()
    {
        await state.DisposeAsync().ConfigureAwait(false);
    }
}
