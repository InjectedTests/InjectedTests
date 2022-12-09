using InjectedTests.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InjectedTests;

internal sealed class BootstrappedHost : IAsyncDisposable
{
    public BootstrappedHost(IHost host)
    {
        Host = host;
    }

    public IHost Host { get; }

    public async ValueTask DisposeAsync()
    {
        try
        {
            var lifetime = Host.Services.GetRequiredService<IHostApplicationLifetime>();
            if (lifetime.ApplicationStarted.IsCancellationRequested)
            {
                await Host.StopAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            await Host.TryDisposeAsync().ConfigureAwait(false);
        }
    }
}
