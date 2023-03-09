using Autofac;
using InjectedTests.Extensibility;

namespace InjectedTests;

internal sealed class ContainerBootstrappingStrategy : IBootstrappingStrategy<ContainerBootstrapperBuilder, IContainer>
{
    private ContainerBootstrappingStrategy()
    {
    }

    public static ContainerBootstrappingStrategy Instance { get; } = new();

    public ContainerBootstrapperBuilder CreateConfiguration() => new();

    public ValueTask<IContainer> BootstrapAsync(ContainerBootstrapperBuilder configuration)
    {
        return new(configuration.Build());
    }

    public IServiceProvider GetServiceProvider(IContainer bootstrapped) => (IServiceProvider)bootstrapped;
}
