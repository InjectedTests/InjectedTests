namespace InjectedTests;

public sealed class ServiceProviderBootstrapperTest : ServicesBootstrapperTestBase
{
    #region state

    private readonly ServiceProviderBootstrapper bootstrapper = new();

    protected override IConfigurableDependencies ConfigurableDependencies => bootstrapper;
    protected override IConfigurableServices ConfigurableServices => bootstrapper;
    protected override IAsyncDisposable BootstrapperDisposable => bootstrapper;
    protected override IServiceProvider ServiceProvider => bootstrapper;

    #endregion
}
