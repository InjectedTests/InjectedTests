using Microsoft.AspNetCore.Mvc.Testing;

namespace InjectedTests;

internal sealed class WebBootstrapperBuilder<T>
    where T : class
{
    public WebApplicationFactory<T> Factory { get; set; } = new();

    public WebApplicationFactoryClientOptions ClientOptions { get; } = new();
}
