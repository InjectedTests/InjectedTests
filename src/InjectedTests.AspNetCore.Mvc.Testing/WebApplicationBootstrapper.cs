using InjectedTests.Abstractions;
using InjectedTests.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace InjectedTests;

public sealed class WebApplicationBootstrapper<T> :
    IConfigurableBootstrapper,
    IServiceProvider,
    IAsyncDisposable
    where T : class
{
    private readonly BootstrapperStateMachine<WebBootstrapperBuilder<T>, BootstrappedWebApplication<T>> state;

    public WebApplicationBootstrapper()
    {
        state = new(WebBootstrappingStrategy<T>.Instance);
    }

    public HttpClient Client => state.Bootstrapped.CreateClient();

    public object? GetService(Type serviceType) => state.Bootstrapped.Factory.Services.GetService(serviceType);

    public WebApplicationBootstrapper<T> ConfigureHost(Action<IWebHostBuilder> configure)
    {
        state.Configure(b => b.Factory = b.Factory.WithWebHostBuilder(configure));
        return this;
    }

    public WebApplicationBootstrapper<T> ConfigureClient(Action<WebApplicationFactoryClientOptions> configure)
    {
        state.Configure(b => configure(b.ClientOptions));
        return this;
    }

    void IConfigurableBootstrapper.ConfigureServices(Action<IServiceCollection> configure)
    {
        ConfigureHost(b => b.ConfigureServices(configure));
    }

    public async ValueTask DisposeAsync()
    {
        await state.DisposeAsync().ConfigureAwait(false);
    }
}
