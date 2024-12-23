using Autofac;
using Autofac.Core;
using InjectedTests.Abstractions;
using InjectedTests.Extensibility;

namespace InjectedTests;

public sealed class ContainerBootstrapper :
    IInitializableBootstrapper,
    IComponentContext,
    IServiceProvider,
    IAsyncDisposable
{
    private readonly BootstrapperStateMachine<ContainerBuilder, IContainer> state;

    public ContainerBootstrapper()
    {
        state = new(ContainerBootstrappingStrategy.Instance);
    }

    IComponentRegistry IComponentContext.ComponentRegistry => state.Bootstrapped.ComponentRegistry;

    public ContainerBootstrapper ConfigureContainer(Action<ContainerBuilder> configure)
    {
        state.Configure(b => configure(b));
        return this;
    }

    void IInitializableBootstrapper.ConfigureInitializer(Action<IInitializerBuilder> configure)
    {
        ConfigureContainer(b => configure(new InitializerBuilder(b)));
    }

    object IComponentContext.ResolveComponent(in ResolveRequest request)
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
