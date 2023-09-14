namespace InjectedTests;

public sealed class InitializerTest : IAsyncLifetime
{
    #region state

    private readonly ServiceProviderBootstrapper bootstrapper = new ServiceProviderBootstrapper()
        .ConfigureServices(s => s.TryAddSingleton<List<int>>());

    private bool isInitialized;

    private IReadOnlyList<int> Events => bootstrapper.GetRequiredService<List<int>>();

    #endregion

    #region lifecycle

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await bootstrapper.DisposeAsync();
    }

    #endregion

    [Fact]
    public void Initialize_InitializeWithoutDependencies_InitializerCalled()
    {
        Given_Bootstrapper_InitializerWithoutDependenciesConfigured();
        Then_Initializer_Called();
    }

    [Fact]
    public void Initialize_ScopedDependencies_InitializerCalled()
    {
        Given_Bootstrapper_InitializerWithScopedDependencyConfigured();
        Then_Initializer_Called();
    }

    [Fact]
    public void Initialize_InitializeTwice_InitializeCallsInOrder()
    {
        Given_Bootstrapper_EventInitializerConfigured(1);
        Given_Bootstrapper_EventInitializerConfigured(2);
        Then_Events_Are(1, 2);
    }

    #region given, when, then

    private void Given_Bootstrapper_InitializerWithoutDependenciesConfigured()
    {
        bootstrapper.ConfigureInitializer(b => b.With(() => isInitialized = true));
    }

    private void Given_Bootstrapper_InitializerWithScopedDependencyConfigured()
    {
        bootstrapper
            .ConfigureServices(s => s.TryAddScoped(p => this))
            .ConfigureInitializer(b => b.With<InitializerTest>(t => t.isInitialized = true));
    }

    private void Given_Bootstrapper_EventInitializerConfigured(int eventValue)
    {
        bootstrapper.ConfigureInitializer(b => b.With<List<int>>(l => l.Add(eventValue)));
    }

    private void Then_Initializer_Called()
    {
        Assert.Empty(Events);
        Assert.True(isInitialized);
    }

    private void Then_Events_Are(params int[] expected)
    {
        Assert.Equal(expected, Events);
    }

    #endregion
}
