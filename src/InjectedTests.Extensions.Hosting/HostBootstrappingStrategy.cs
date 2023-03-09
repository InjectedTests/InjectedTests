using InjectedTests.Extensibility;

namespace InjectedTests;

internal sealed class HostBootstrappingStrategy : IBootstrappingStrategy<HostBootstrapperBuilder, BootstrappedHost>
{
    private HostBootstrappingStrategy()
    {
    }

    public static HostBootstrappingStrategy Instance { get; } = new();

    public HostBootstrapperBuilder CreateConfiguration() => new();

    public async ValueTask<BootstrappedHost> BootstrapAsync(HostBootstrapperBuilder configuration)
    {
        var host = configuration.Build();
        try
        {
            await configuration.PrepareAsync(host).ConfigureAwait(false);

            return new(host);
        }
        catch
        {
            await host.TryDisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    public IServiceProvider GetServiceProvider(BootstrappedHost bootstrapped) => bootstrapped.Host.Services;
}
