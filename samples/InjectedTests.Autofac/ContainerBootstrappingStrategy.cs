using Autofac;
using Autofac.Core;
using InjectedTests.Extensibility;

namespace InjectedTests;

internal sealed class ContainerBootstrappingStrategy : IBootstrappingStrategy<ContainerBuilder, IContainer>
{
    private ContainerBootstrappingStrategy()
    {
    }

    public static ContainerBootstrappingStrategy Instance { get; } = new();

    public ContainerBuilder CreateConfiguration() => new();

    public ValueTask<IContainer> BootstrapAsync(ContainerBuilder configuration)
    {
        return new(configuration.Build());
    }

    public async ValueTask InitializeAsync(IContainer bootstrapped)
    {
        var scope = bootstrapped.BeginLifetimeScope();
        try
        {
            foreach (var initializer in scope.Resolve<IEnumerable<IInitializer>>())
            {
                await initializer.InitializeAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            await scope.TryDisposeAsync().ConfigureAwait(false);
        }
    }
}
