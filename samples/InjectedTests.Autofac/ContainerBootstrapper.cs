using Autofac;
using Autofac.Core;
using InjectedTests.Abstractions;
using InjectedTests.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace InjectedTests;

public sealed class ContainerBootstrapper :
    IConfigurableBootstrapper,
    IComponentContext,
    IServiceProvider,
    IAsyncDisposable
{
    private readonly BootstrapperStateMachine<ContainerBootstrapperBuilder, IContainer> state;

    public ContainerBootstrapper()
    {
        state = new(ContainerBootstrappingStrategy.Instance);
    }

    IComponentRegistry IComponentContext.ComponentRegistry => state.Bootstrapped.ComponentRegistry;

    public ContainerBootstrapper ConfigureContainer(Action<ContainerBuilder> configure)
    {
        state.Configure(b => b.Configure(configure));
        return this;
    }

    void IConfigurableBootstrapper.ConfigureServices(Action<IServiceCollection> configure)
    {
        state.Configure(b => b.Configure(configure));
    }

    object IComponentContext.ResolveComponent(ResolveRequest request)
    {
        return state.Bootstrapped.ResolveComponent(request);
    }

    object IServiceProvider.GetService(Type serviceType)
    {
        return ((IServiceProvider)state.Bootstrapped).GetService(serviceType);
    }

    public ValueTask DisposeAsync()
    {
        return state.DisposeAsync();
    }
}
