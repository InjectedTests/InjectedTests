using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InjectedTests;

public sealed class ContainerBootstrapperTest : IAsyncDisposable
{
    #region state

    private readonly ContainerBootstrapper bootstrapper = new();

    private TestService service;

    #endregion

    #region lifecycle

    public async ValueTask DisposeAsync()
    {
        await bootstrapper.DisposeAsync();
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

    [Fact]
    public async Task Dispose_DisposeAfterResolveScoped_ScopedServiceDisposed()
    {
        Given_Bootstrapper_ScopedServiceConfigured();
        When_Bootstrapper_ResolveScopedService();
        await When_Bootstrapper_DisposedAsync();
        Then_Service_Disposed();
    }

    #region given, when, then

    private void Given_Bootstrapper_ServiceConfigured()
    {
        bootstrapper.ConfigureContainer(b => b.RegisterType<TestService>().SingleInstance());
    }

    private void Given_Bootstrapper_ScopedServiceConfigured()
    {
        bootstrapper.ConfigureTestScope();
        bootstrapper.ConfigureContainer(b => b.RegisterType<TestService>().InstancePerLifetimeScope());
    }

    private void Given_Bootstrapper_ServiceInitializerConfigured()
    {
        bootstrapper.ConfigureInitializer(b => b.With<TestService>(Helper_InitializeService));
    }

    private void When_Bootstrapper_ResolveService()
    {
        service = bootstrapper.Resolve<TestService>();
    }

    private void When_Bootstrapper_ResolveScopedService()
    {
        service = bootstrapper.GetRequiredScopedService<TestService>();
    }

    private async Task When_Bootstrapper_DisposedAsync()
    {
        await bootstrapper.DisposeAsync();
    }

    private void Then_Service_Disposed()
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

    private sealed class TestService : IAsyncDisposable
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
