using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace InjectedTests;

internal sealed class ContainerBootstrapperBuilder
{
    private readonly ContainerBuilder builder = new();
    private IServiceCollection services;

    public void Configure(Action<ContainerBuilder> configure)
    {
        TryPopulateFromServices();
        configure(builder);
    }

    public void Configure(Action<IServiceCollection> configure)
    {
        services ??= new ServiceCollection();
        configure(services);
    }

    public IContainer Build()
    {
        TryPopulateFromServices();
        return builder.Build();
    }

    private void TryPopulateFromServices()
    {
        var source = services;
        services = null;

        if (source is not null)
        {
            builder.Populate(source);
        }
    }
}
