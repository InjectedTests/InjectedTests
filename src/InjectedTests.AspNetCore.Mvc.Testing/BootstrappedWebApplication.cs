using Microsoft.AspNetCore.Mvc.Testing;

namespace InjectedTests;

internal sealed class BootstrappedWebApplication<T> : BootstrappedWebApplication
    where T : class
{
    private readonly WebApplicationFactory<T> factory;
    private readonly WebApplicationFactoryClientOptions clientOptions;

    public BootstrappedWebApplication(WebBootstrapperBuilder<T> builder)
    {
        factory = builder.Factory;
        clientOptions = builder.ClientOptions;
    }

    public override IServiceProvider Services => factory.Services;

    public override HttpClient CreateClient()
    {
        return factory.CreateClient(clientOptions);
    }

    public override ValueTask DisposeAsync()
    {
        return factory.DisposeAsync();
    }
}

internal abstract class BootstrappedWebApplication : IAsyncDisposable
{
    public abstract IServiceProvider Services { get; }

    public abstract HttpClient CreateClient();

    public abstract ValueTask DisposeAsync();
}
