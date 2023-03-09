using InjectedTests.Abstractions;
using InjectedTests.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InjectedTests;

public sealed class HostBootstrapper :
    IConfigurableBootstrapper,
    IServiceProvider,
    IAsyncDisposable
{
    private readonly BootstrapperStateMachine<HostBootstrapperBuilder, BootstrappedHost> state;

    public HostBootstrapper()
    {
        state = new(HostBootstrappingStrategy.Instance);
    }

    public IHost Host => state.Bootstrapped.Host;

    public object? GetService(Type serviceType) => Host.Services.GetService(serviceType);

    public HostBootstrapper ConfigureHost(Action<IHostBuilder> configure)
    {
        state.Configure(b => configure(b.HostBuilder));
        return this;
    }

    void IConfigurableBootstrapper.ConfigureServices(Action<IServiceCollection> configure)
    {
        state.Configure(b => b.HostBuilder.ConfigureServices(configure));
    }

    public HostBootstrapper DisableAutoRun()
    {
        state.Configure(b => b.IsAutoRunEnabled = false);
        return this;
    }

    public HostBootstrapper UseOriginalHostLifetime()
    {
        state.Configure(b => b.UseTestHostLifetime = false);
        return this;
    }

    public async ValueTask DisposeAsync()
    {
        await state.DisposeAsync().ConfigureAwait(false);
    }
}
