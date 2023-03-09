using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InjectedTests;

internal sealed class HostBootstrapperBuilder
{
    public bool IsAutoRunEnabled { get; set; } = true;

    public bool UseTestHostLifetime { get; set; } = true;

    public IHostBuilder HostBuilder { get; } = new HostBuilder()
        .UseEnvironment(Environments.Development);

    public IHost Build()
    {
        if (UseTestHostLifetime)
        {
            HostBuilder.ConfigureServices(ConfigureTestHostLifetime);
        }

        return HostBuilder.Build();
    }

    public async ValueTask PrepareAsync(IHost host)
    {
        if (IsAutoRunEnabled)
        {
            await host.StartAsync().ConfigureAwait(false);
        }
    }

    private static void ConfigureTestHostLifetime(HostBuilderContext context, IServiceCollection services)
    {
        if (services.Any(d => d.ServiceType == typeof(IHostLifetime)))
        {
            services
                .RemoveAll<IHostLifetime>()
                .AddSingleton<IHostLifetime, TestHostLifetime>();
        }
    }
}
