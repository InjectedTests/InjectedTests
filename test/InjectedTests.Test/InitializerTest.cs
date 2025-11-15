namespace InjectedTests;

public sealed class InitializerTest : IAsyncLifetime
{
    #region state

    private readonly ServiceProviderBootstrapper bootstrapper = new ServiceProviderBootstrapper()
        .ConfigureServices(s => s.TryAddSingleton<List<int>>());

    private bool IsInitialized { get; set; }
    private bool IsInitializerDependencyDisposed { get; set; }

    private IReadOnlyList<int> Events => bootstrapper.GetRequiredService<List<int>>();

    #endregion

    #region lifecycle

    public ValueTask InitializeAsync()
    {
        return default;
    }

    public async ValueTask DisposeAsync()
    {
        await bootstrapper.DisposeAsync();
    }

    #endregion

    [Fact]
    public void Initialize_InitializeWithoutDependencies_InitializerCalled()
    {
        Given_Bootstrapper_InitializerWithoutDependenciesConfigured();
        When_Bootstrapper_Initializes();
        Then_Initializer_Called();
    }

    [Fact]
    public void Initialize_ScopedDependencies_InitializerCalled()
    {
        Given_Bootstrapper_InitializerWithScopedDependencyConfigured();
        When_Bootstrapper_Initializes();
        Then_Initializer_Called();
        Then_InitializerDependency_Disposed();
    }

    [Fact]
    public void Initialize_InitializeTwice_InitializeCallsInOrder()
    {
        Given_Bootstrapper_EventInitializerConfigured(1);
        Given_Bootstrapper_EventInitializerConfigured(2);
        When_Bootstrapper_Initializes();
        Then_Events_Are(1, 2);
    }

    #region given, when, then

    private void Given_Bootstrapper_InitializerWithoutDependenciesConfigured()
    {
        bootstrapper.ConfigureInitializer(b => b.With(() => IsInitialized = true));
    }

    private void Given_Bootstrapper_InitializerWithScopedDependencyConfigured()
    {
        bootstrapper
            .ConfigureServices(s => s.TryAddSingleton(this))
            .ConfigureServices(s => s.TryAddScoped<ScopedTestInitializerDependency>())
            .ConfigureInitializer(b => b.With<ScopedTestInitializerDependency>(d => d.Initialize()));
    }

    private void Given_Bootstrapper_EventInitializerConfigured(int eventValue)
    {
        bootstrapper.ConfigureInitializer(b => b.With<List<int>>(l => l.Add(eventValue)));
    }

    private void When_Bootstrapper_Initializes()
    {
        Assert.NotNull(bootstrapper.Services);
    }

    private void Then_Initializer_Called()
    {
        Assert.True(IsInitialized);
    }

    private void Then_InitializerDependency_Disposed()
    {
        Assert.True(IsInitializerDependencyDisposed);
    }

    private void Then_Events_Are(params int[] expected)
    {
        Assert.Equal(expected, Events);
    }

    private sealed class ScopedTestInitializerDependency(InitializerTest test) : IAsyncDisposable
    {
        public void Initialize()
        {
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(test.IsInitializerDependencyDisposed, this);
#else
            if (test.IsInitializerDependencyDisposed)
            {
                throw new ObjectDisposedException(nameof(ScopedTestInitializerDependency));
            }
#endif

            test.IsInitialized = true;
        }

        public ValueTask DisposeAsync()
        {
            test.IsInitializerDependencyDisposed = true;

            return default;
        }
    }

    #endregion
}
