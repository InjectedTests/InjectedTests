using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace InjectedTests;

internal sealed class WebBootstrapperBuilder<T> : WebBootstrapperBuilder
    where T : class
{
    public WebApplicationFactory<T> Factory { get; private set; } = new();

    public override WebBootstrapperBuilder Configure(Action<IWebHostBuilder> configure)
    {
        Factory = Factory.WithWebHostBuilder(configure);
        return this;
    }

    public override BootstrappedWebApplication Build()
    {
        return new BootstrappedWebApplication<T>(this);
    }
}

internal abstract class WebBootstrapperBuilder
{
    public WebApplicationFactoryClientOptions ClientOptions { get; } = new();

    public abstract WebBootstrapperBuilder Configure(Action<IWebHostBuilder> configure);

    public abstract BootstrappedWebApplication Build();
}
