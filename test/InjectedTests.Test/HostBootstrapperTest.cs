namespace InjectedTests;

public sealed class HostBootstrapperTest : BootstrapperTestBase
{
    #region state

    private const string Started = "started";
    private const string Stopped = "stopped";
    private const string Disposed = "disposed";
    private const string TestConfigurationValue = "test_value";
    private const string TestConfigurationKey = "test_key";

    private readonly HostBootstrapper bootstrapper = new();

    private readonly List<string> hostedServiceEvents = new();

    protected override IConfigurableBootstrapper ConfigurableBootstrapper => bootstrapper;
    protected override IInitializableBootstrapper InitializableBootstrapper => bootstrapper;
    protected override IAsyncDisposable BootstrapperDisposable => bootstrapper;
    protected override IServiceProvider ServiceProvider => bootstrapper;

    #endregion

    [Fact]
    public void Run_Resolve_HostedServiceStarted()
    {
        Given_Bootstrapper_HostedServiceConfigured();
        When_Bootstrapper_ResolveHostedService();
        Then_HostedServiceEvents_Are(Started);
    }

    [Fact]
    public void Run_ResolveWithoutAutoRun_HostedServiceNotStarted()
    {
        Given_Bootstrapper_HostedServiceConfigured();
        Given_Bootstrapper_AutoRunDisabled();
        When_Bootstrapper_ResolveHostedService();
        Then_HostedServiceEvents_Are();
    }

    [Fact]
    public async Task Run_DisposeBootstrapper_HostedServiceStoppedAndDisposed()
    {
        Given_Bootstrapper_HostedServiceConfigured();
        When_Bootstrapper_ResolveHostedService();
        await When_Bootstrapper_DisposedAsync();
        Then_HostedServiceEvents_Are(Started, Stopped, Disposed);
    }

    [Fact]
    public async Task Run_DisposeBootstrapperWithoutAutoRun_HostedServiceDisposed()
    {
        Given_Bootstrapper_HostedServiceConfigured();
        Given_Bootstrapper_AutoRunDisabled();
        When_Bootstrapper_ResolveHostedService();
        await When_Bootstrapper_DisposedAsync();
        Then_HostedServiceEvents_Are(Disposed);
    }

    [Fact]
    public async Task Run_DisposeBootstrapperAfterManuallyStarting_HostedServiceStoppedAndDisposed()
    {
        Given_Bootstrapper_HostedServiceConfigured();
        Given_Bootstrapper_AutoRunDisabled();
        await When_Bootstrapper_StartAsync();
        When_Bootstrapper_ResolveHostedService();
        await When_Bootstrapper_DisposedAsync();
        Then_HostedServiceEvents_Are(Started, Stopped, Disposed);
    }

    [Fact]
    public void ConfigureHost_AddHostConfiguration_HasConfigurationValue()
    {
        Given_Bootstrapper_AddHostConfiguration();
        Then_Host_HasConfigurationValue();
    }

    [Fact]
    public void Lifetime_DefaultLifetime_HasTestLifetime()
    {
        Then_Host_HasTestLifetime();
    }

    [Fact]
    public void Lifetime_UseOriginalLifetime_HasOriginalLifetime()
    {
        Given_Bootstrapper_UseOriginalLifetime();
        Then_Host_HasConsoleLifetime();
    }

    [Fact]
    public void Environment_DefaultEnvironment_IsDevelopment()
    {
        Then_Host_HasDevelopmentEnvironment();
    }

    [Fact]
    public void Environment_StagingEnvironment_IsDevelopment()
    {
        Given_Bootstrapper_StagingEnvironment();
        Then_Host_HasStagingEnvironment();
    }

    #region given, when, then

    private void Given_Bootstrapper_AddHostConfiguration()
    {
        bootstrapper.ConfigureHost(b => b.ConfigureHostConfiguration(c => c.AddInMemoryCollection(new[] { new KeyValuePair<string, string>(TestConfigurationKey, TestConfigurationValue) })));
    }

    private void Given_Bootstrapper_UseOriginalLifetime()
    {
        bootstrapper.UseOriginalHostLifetime();
    }

    private void Given_Bootstrapper_HostedServiceConfigured()
    {
        bootstrapper.ConfigureServices(s => s.AddHostedService(p => ActivatorUtilities.CreateInstance<TestHostedService>(p, this)));
    }

    private void Given_Bootstrapper_AutoRunDisabled()
    {
        bootstrapper.DisableAutoRun();
    }

    private void Given_Bootstrapper_StagingEnvironment()
    {
        bootstrapper.ConfigureHost(b => b.UseEnvironment(Environments.Staging));
    }

    private async Task When_Bootstrapper_StartAsync()
    {
        await bootstrapper.Host.StartAsync();
    }

    private void When_Bootstrapper_ResolveHostedService()
    {
        Assert.NotEmpty(ServiceProvider.GetServices<IHostedService>());
    }

    private void Then_HostedServiceEvents_Are(params string[] expected)
    {
        Assert.Equal(expected, hostedServiceEvents);
    }

    private void Then_Host_HasConfigurationValue()
    {
        var actual = ServiceProvider
            .GetRequiredService<IConfiguration>()
            .GetValue<string>(TestConfigurationKey);

        Assert.Equal(TestConfigurationValue, actual);
    }

    private void Then_Host_HasTestLifetime()
    {
        Assert.IsType<TestHostLifetime>(ServiceProvider.GetRequiredService<IHostLifetime>());
    }

    private void Then_Host_HasConsoleLifetime()
    {
        Assert.IsType<ConsoleLifetime>(ServiceProvider.GetRequiredService<IHostLifetime>());
    }

    private void Then_Host_HasDevelopmentEnvironment()
    {
        Assert.Equal(Environments.Development, ServiceProvider.GetRequiredService<IHostEnvironment>().EnvironmentName);
    }

    private void Then_Host_HasStagingEnvironment()
    {
        Assert.Equal(Environments.Staging, ServiceProvider.GetRequiredService<IHostEnvironment>().EnvironmentName);
    }

    private void Helper_AddHostedServiceEvent(string id)
    {
        hostedServiceEvents.Add(id);
    }

    private sealed class TestHostedService : IHostedService, IAsyncDisposable
    {
        private readonly HostBootstrapperTest test;

        public TestHostedService(HostBootstrapperTest test)
        {
            this.test = test;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            test.Helper_AddHostedServiceEvent(Started);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            test.Helper_AddHostedServiceEvent(Stopped);
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            test.Helper_AddHostedServiceEvent(Disposed);
            return default;
        }
    }

    #endregion
}
