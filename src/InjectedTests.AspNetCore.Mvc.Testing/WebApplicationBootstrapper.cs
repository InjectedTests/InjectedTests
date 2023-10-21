using InjectedTests.Abstractions;
using InjectedTests.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace InjectedTests;

public sealed class WebApplicationBootstrapper<T> : WebApplicationBootstrapper
    where T : class
{
    private protected override BootstrapperStateMachine<WebBootstrapperBuilder, BootstrappedWebApplication> State { get; }
        = new(WebBootstrappingStrategy<T>.Instance);
}

public abstract class WebApplicationBootstrapper :
    IConfigurableBootstrapper,
    IServiceProvider,
    IAsyncDisposable
{
    private protected abstract BootstrapperStateMachine<WebBootstrapperBuilder, BootstrappedWebApplication> State { get; }

    public HttpClient Client => State.Bootstrapped.CreateClient();

    public object? GetService(Type serviceType) => State.Bootstrapped.Services.GetService(serviceType);

    public WebApplicationBootstrapper ConfigureHost(Action<IWebHostBuilder> configure)
    {
        State.Configure(b => b.Configure(configure));
        return this;
    }

    public WebApplicationBootstrapper ConfigureClient(Action<WebApplicationFactoryClientOptions> configure)
    {
        State.Configure(b => configure(b.ClientOptions));
        return this;
    }

    void IConfigurableBootstrapper.ConfigureServices(Action<IServiceCollection> configure)
    {
        ConfigureHost(b => b.ConfigureServices(configure));
    }

    public async ValueTask DisposeAsync()
    {
        await State.DisposeAsync().ConfigureAwait(false);
    }
}
