namespace InjectedTests;

public abstract class ServicesBootstrapperTestBase : BootstrapperTestBase
{
    #region state

    protected abstract IConfigurableServices ConfigurableServices { get; }

    #endregion

    [Fact]
    public async Task Dispose_DisposeAfterResolveScoped_ScopedServiceDisposed()
    {
        Given_Bootstrapper_ScopedServiceConfigured();
        When_Bootstrapper_ResolveScopedService();
        await When_Bootstrapper_DisposedAsync();
        Then_Service_Disposed();
    }

    #region given, when, then

    private void Given_Bootstrapper_ScopedServiceConfigured()
    {
        ConfigurableServices.ConfigureTestScope();
        ConfigurableServices.ConfigureServices(s => s.TryAddScoped<TestService>());
    }

    private void When_Bootstrapper_ResolveScopedService()
    {
        service = ServiceProvider.GetRequiredScopedService<TestService>();
    }

    #endregion
}
