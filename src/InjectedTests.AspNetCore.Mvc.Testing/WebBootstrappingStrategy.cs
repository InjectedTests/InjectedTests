using InjectedTests.Extensibility;

namespace InjectedTests;

internal sealed class WebBootstrappingStrategy<T> :
    IBootstrappingStrategy<WebBootstrapperBuilder<T>, BootstrappedWebApplication<T>>
    where T : class
{
    private WebBootstrappingStrategy()
    {
    }

    public static WebBootstrappingStrategy<T> Instance { get; } = new();

    public WebBootstrapperBuilder<T> CreateConfiguration() => new();

    public ValueTask<BootstrappedWebApplication<T>> BootstrapAsync(WebBootstrapperBuilder<T> configuration)
    {
        return new(new BootstrappedWebApplication<T>(configuration));
    }

    public IServiceProvider GetServiceProvider(BootstrappedWebApplication<T> bootstrapped)
    {
        return bootstrapped.Factory.Services;
    }
}
