namespace InjectedTests.Extensibility;

public interface IBootstrappingStrategy<TConfiguration, TBootstrapped>
{
    TConfiguration CreateConfiguration();

    ValueTask<TBootstrapped> BootstrapAsync(TConfiguration configuration);

    IServiceProvider GetServiceProvider(TBootstrapped bootstrapped);
}
