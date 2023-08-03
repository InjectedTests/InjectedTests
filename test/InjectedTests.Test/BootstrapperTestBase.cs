namespace InjectedTests;

public abstract class BootstrapperTestBase : IAsyncLifetime
{
    #region state

    protected TestService service { get; set; }

    protected abstract IConfigurableDependencies ConfigurableDependencies { get; }
    protected abstract IAsyncDisposable BootstrapperDisposable { get; }
    protected abstract IServiceProvider ServiceProvider { get; }

    #endregion

    #region lifecycle

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await BootstrapperDisposable.DisposeAsync();
    }

    #endregion

    [Fact]
    public void Initialize_Resolve_ServiceInitialized()
    {
        Given_Bootstrapper_ServiceConfigured();
        Given_Bootstrapper_ServiceInitializerConfigured();
        When_Bootstrapper_ResolveService();
        Then_Service_Initialized();
    }

    [Fact]
    public async Task Dispose_DisposeAfterResolve_ServiceDisposed()
    {
        Given_Bootstrapper_ServiceConfigured();
        When_Bootstrapper_ResolveService();
        await When_Bootstrapper_DisposedAsync();
        Then_Service_Disposed();
    }

    #region given, when, then

    private void Given_Bootstrapper_ServiceConfigured()
    {
        ConfigurableDependencies.ConfigureDependencies(s => s.TryAddSingleton<TestService>());
    }

    private void Given_Bootstrapper_ServiceInitializerConfigured()
    {
        ConfigurableDependencies.ConfigureInitializer(b => b.With<TestService>(Helper_InitializeService));
    }

    private void When_Bootstrapper_ResolveService()
    {
        service = ServiceProvider.GetRequiredService<TestService>();
    }

    protected async Task When_Bootstrapper_DisposedAsync()
    {
        await BootstrapperDisposable.DisposeAsync();
    }

    protected void Then_Service_Disposed()
    {
        Assert.True(service.Disposed);
    }

    private void Then_Service_Initialized()
    {
        Assert.True(service.Initialized);
    }

    private void Helper_InitializeService(TestService service)
    {
        service.Initialized = true;
    }

    protected sealed class TestService : IAsyncDisposable
    {
        public bool Disposed { get; private set; }
        public bool Initialized { get; set; }

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return default;
        }
    }

    #endregion
}
