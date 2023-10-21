using InjectedTests.Extensibility;

namespace InjectedTests;

internal sealed class WebBootstrappingStrategy<T> :
    IBootstrappingStrategy<WebBootstrapperBuilder, BootstrappedWebApplication>
    where T : class
{
    private WebBootstrappingStrategy()
    {
    }

    public static WebBootstrappingStrategy<T> Instance { get; } = new();

    public WebBootstrapperBuilder CreateConfiguration() => new WebBootstrapperBuilder<T>();

    public ValueTask<BootstrappedWebApplication> BootstrapAsync(WebBootstrapperBuilder configuration)
    {
        return new(configuration.Build());
    }

    public IServiceProvider GetServiceProvider(BootstrappedWebApplication bootstrapped)
    {
        return bootstrapped.Services;
    }
}
