namespace InjectedTests;

public sealed class BootstrapperStateMachineTest : IAsyncLifetime
{
    #region state

    private const string Bootstrapping = "bootstrapping";
    private const string Bootstrapped = "bootstrapped";
    private const string Configuring = "configuring";
    private const string Disposed = "disposed";
    private const string DisposedInitializerScope = "disposedScope";
    private const string Initializing = "initializing";

    private readonly List<(string Id, int Index)> events = new();
    private readonly List<Task<TestTarget>> bootstrappingTasks = new();
    private readonly BootstrapperStateMachine<TestTarget, TestTarget> state;
    private TestTarget bootstrapped;
    private IReadOnlyList<TestTarget> bootstrappedInstances;

    private TaskCompletionSource<bool> BootstrappingStartedSource { get; } = new();
    private TaskCompletionSource<bool> FinishBootstrappingSource { get; } = new();
    private bool InitializeThrows { get; set; }

    #endregion

    #region lifecycle

    public BootstrapperStateMachineTest()
    {
        state = new(new TestBootstrappingStrategy(this));
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (bootstrapped != null)
        {
            await bootstrapped.DisposeAsync();
        }

        await state.DisposeAsync();
    }

    #endregion

    [Fact]
    public void Sequence_Bootstrap_SequenceCorrectDuringBootstrap()
    {
        Given_Bootstrapper_BootstrapCompletesImmediately();
        When_State_Configure();
        When_State_GetBootstrapped();
        Then_Events_Are(Bootstrapping, Configuring, Bootstrapped, Initializing, DisposedInitializerScope);
    }

    [Fact]
    public void Sequence_Bootstrap_OnlyOneInstanceCreated()
    {
        Given_Bootstrapper_BootstrapCompletesImmediately();
        When_State_GetBootstrapped();
        Then_Events_AllHaveIdZero();
    }

    [Fact]
    public async Task Sequence_BootstrapTwiceConcurrently_OnlyOneInstanceReturned()
    {
        When_State_StartBootstrapping();
        When_State_StartBootstrapping();
        When_Bootstrapper_BootstrappingCompletes();
        await When_BootstrappingTasks_AllComplete();
        Then_BootstrappedInstances_AllSameInstance();
    }

    [Fact]
    public async Task Sequence_DisposeBeforeBootstrappingCompletes_ObjectDisposedException()
    {
        When_State_StartBootstrapping();
        await When_Bootstrapper_BootstrappingStarted();
        await When_State_Dispose();
        When_Bootstrapper_BootstrappingCompletes();
        await Then_BootstrappingTasks_AllThrowObjectDisposedException();
    }

    [Fact]
    public async Task Sequence_DisposeBeforeBootstrappingCompletes_InstanceDisposed()
    {
        When_State_StartBootstrapping();
        await When_Bootstrapper_BootstrappingStarted();
        await When_State_Dispose();
        When_Bootstrapper_BootstrappingCompletes();
        await Then_BootstrappingTasks_AllThrowObjectDisposedException();
        Then_Events_EndsWith(Disposed);
    }

    [Fact]
    public async Task Dispose_DisposeWithoutBootstrap_DisposeNotCalled()
    {
        Given_Bootstrapper_BootstrapCompletesImmediately();
        await When_State_Dispose();
        Then_Events_Are();
    }

    [Fact]
    public async Task Dispose_DisposeWithBootstrap_DisposeCalled()
    {
        Given_Bootstrapper_BootstrapCompletesImmediately();
        When_State_GetBootstrapped();
        await When_State_Dispose();
        Then_Events_EndsWith(Disposed);
    }

    [Fact]
    public void Dispose_InitializeThrows_BootstrappedDisposed()
    {
        Given_Bootstrapper_BootstrapCompletesImmediately();
        Given_Initializer_Throws();
        When_State_GetBootstrappedThrows();
        Then_Events_Are(Bootstrapping, Bootstrapped, Initializing, DisposedInitializerScope, Disposed);
    }

    #region given, when, then

    private void Given_Bootstrapper_BootstrapCompletesImmediately()
    {
        When_Bootstrapper_BootstrappingCompletes();
    }

    private void Given_Initializer_Throws()
    {
        InitializeThrows = true;
    }

    private void When_State_Configure()
    {
        state.Configure(t => t.AddEvent(Configuring));
    }

    private void When_State_StartBootstrapping()
    {
        bootstrappingTasks.Add(Task.Run(() => state.Bootstrapped));
    }

    private void When_State_GetBootstrapped()
    {
        bootstrapped = state.Bootstrapped;
    }

    private void When_State_GetBootstrappedThrows()
    {
        Assert.Throws<Exception>(When_State_GetBootstrapped);
    }

    private async Task When_State_Dispose()
    {
        await state.DisposeAsync();
    }

    private Task When_Bootstrapper_BootstrappingStarted()
    {
        return BootstrappingStartedSource.Task;
    }

    private void When_Bootstrapper_BootstrappingCompletes()
    {
        FinishBootstrappingSource.SetResult(true);
    }

    private async Task When_BootstrappingTasks_AllComplete()
    {
        bootstrappedInstances = await Task.WhenAll(bootstrappingTasks);
    }

    private void Then_Events_Are(params string[] expected)
    {
        Assert.Equal(expected, events.Select(e => e.Id));
    }

    private void Then_Events_AllHaveIdZero()
    {
        Assert.All(events, e => Assert.Equal(0, e.Index));
    }

    private void Then_Events_EndsWith(string expected)
    {
        Assert.Equal(expected, events.LastOrDefault().Id);
    }

    private void Then_BootstrappedInstances_AllSameInstance()
    {
        Assert.NotEmpty(bootstrappedInstances);
        var first = bootstrappedInstances[0];
        Assert.All(bootstrappedInstances, i => Assert.Same(first, i));
    }

    private async Task Then_BootstrappingTasks_AllThrowObjectDisposedException()
    {
        Assert.NotEmpty(bootstrappingTasks);
        foreach (var task in bootstrappingTasks)
        {
            await Assert.ThrowsAsync<ObjectDisposedException>(() => task);
        }
    }

    private void Helper_AddEvent(string id, int index)
    {
        events.Add((id, index));
    }

    private sealed class TestBootstrappingStrategy : IBootstrappingStrategy<TestTarget, TestTarget>
    {
        private readonly BootstrapperStateMachineTest test;
        private int targetCount;

        public TestBootstrappingStrategy(BootstrapperStateMachineTest test)
        {
            this.test = test;
        }

        public TestTarget CreateConfiguration()
        {
            var target = new TestTarget(test, targetCount++);
            target.AddEvent(Bootstrapping);
            return target;
        }

        public async ValueTask<TestTarget> BootstrapAsync(TestTarget configuration)
        {
            test.BootstrappingStartedSource?.TrySetResult(true);
            await test.FinishBootstrappingSource.Task;
            configuration.AddEvent(Bootstrapped);
            return configuration;
        }

        public IServiceProvider GetServiceProvider(TestTarget bootstrapped)
        {
            return bootstrapped;
        }
    }

    private sealed class TestTarget : IServiceProvider, IInitializer, IAsyncDisposable, IServiceScopeFactory
    {
        private readonly BootstrapperStateMachineTest test;
        private readonly int index;

        public TestTarget(BootstrapperStateMachineTest test, int index)
        {
            this.test = test;
            this.index = index;
        }

        public object GetService(Type serviceType)
        {
            return serviceType switch
            {
                _ when serviceType == typeof(IServiceScopeFactory) => this,
                _ when serviceType == typeof(IEnumerable<IInitializer>) => new IInitializer[] { this },
                _ => null,
            };
        }

        public ValueTask InitializeAsync()
        {
            AddEvent(Initializing);

            if (test.InitializeThrows)
            {
                throw new Exception("Initialize throws.");
            }

            return default;
        }

        public IServiceScope CreateScope()
        {
            return new TestScope(this);
        }

        public ValueTask DisposeAsync()
        {
            AddEvent(Disposed);
            return default;
        }

        public void AddEvent(string id)
        {
            test.Helper_AddEvent(id, index);
        }
    }

    private sealed class TestScope : IServiceScope
    {
        private readonly TestTarget target;

        public TestScope(TestTarget target)
        {
            this.target = target;
        }

        public IServiceProvider ServiceProvider => target;

        public void Dispose()
        {
            target.AddEvent(DisposedInitializerScope);
        }
    }

    #endregion
}
