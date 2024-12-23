namespace InjectedTests;

public sealed class ServiceProviderBootstrapperTest : BootstrapperTestBase
{
    #region state

    private readonly ServiceProviderBootstrapper bootstrapper = new();

    protected override IConfigurableBootstrapper ConfigurableBootstrapper => bootstrapper;
    protected override IInitializableBootstrapper InitializableBootstrapper => bootstrapper;
    protected override IAsyncDisposable BootstrapperDisposable => bootstrapper;
    protected override IServiceProvider ServiceProvider => bootstrapper;

    #endregion
}
