using Microsoft.AspNetCore.Mvc.Testing;

namespace InjectedTests;

internal sealed class BootstrappedWebApplication<T> : IAsyncDisposable
    where T : class
{
    private readonly WebApplicationFactoryClientOptions clientOptions;

    public BootstrappedWebApplication(WebBootstrapperBuilder<T> builder)
    {
        Factory = builder.Factory;
        clientOptions = builder.ClientOptions;
    }

    public WebApplicationFactory<T> Factory { get; }

    public HttpClient CreateClient() => Factory.CreateClient(clientOptions);

    public ValueTask DisposeAsync()
    {
        return Factory.DisposeAsync();
    }
}
