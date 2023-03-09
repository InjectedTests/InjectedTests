namespace InjectedTests;

public sealed class BootstrapperStateMachineTest : IAsyncDisposable
{
    #region state

    private const string Bootstrapping = "bootstrapping";
    private const string Bootstrapped = "bootstrapped";
    private const string Configuring = "configuring";
    private const string Disposed = "disposed";
    private const string Initializing = "initializing";

    private readonly List<string> events = new();
    private readonly BootstrapperStateMachine<TestTarget, TestTarget> state;
    private TestTarget bootstrapped;

    private bool InitializeThrows { get; set; }

    #endregion

    #region lifecycle

    public BootstrapperStateMachineTest()
    {
        state = new(new TestBootstrappingStrategy(this));
    }

    public async ValueTask DisposeAsync()
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
        When_State_Configure();
        When_State_GetBootstrapped();
        Then_Events_Are(Bootstrapping, Configuring, Bootstrapped, Initializing);
    }

    [Fact]
    public async Task Dispose_DisposeWithoutBootstrap_DisposeNotCalled()
    {
        await When_State_Dispose();
        Then_Events_Are();
    }

    [Fact]
    public async Task Dispose_DisposeWithBootstrap_DisposeCalled()
    {
        When_State_GetBootstrapped();
        await When_State_Dispose();
        Then_Events_EndsWith(Disposed);
    }

    [Fact]
    public void Dispose_InitializeThrows_BootstrappedDisposed()
    {
        Given_Initializer_Throws();
        When_State_GetBootstrappedThrows();
        Then_Events_Are(Bootstrapping, Bootstrapped, Initializing, Disposed);
    }

    #region given, when, then

    private void Given_Initializer_Throws()
    {
        InitializeThrows = true;
    }

    private void When_State_Configure()
    {
        state.Configure(t => t.AddEvent(Configuring));
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

    private void Then_Events_Are(params string[] expected)
    {
        Assert.Equal(expected, events);
    }

    private void Then_Events_EndsWith(string expected)
    {
        Assert.Equal(expected, events.LastOrDefault());
    }

    private void Helper_AddEvent(string id)
    {
        events.Add(id);
    }

    private sealed class TestBootstrappingStrategy : IBootstrappingStrategy<TestTarget, TestTarget>
    {
        private readonly TestTarget target;

        public TestBootstrappingStrategy(BootstrapperStateMachineTest test)
        {
            target = new(test);
        }

        public TestTarget CreateConfiguration()
        {
            target.AddEvent(Bootstrapping);
            return target;
        }

        public ValueTask<TestTarget> BootstrapAsync(TestTarget configuration)
        {
            configuration.AddEvent(Bootstrapped);
            return new ValueTask<TestTarget>(configuration);
        }

        public IServiceProvider GetServiceProvider(TestTarget bootstrapped)
        {
            return bootstrapped;
        }
    }

    private sealed class TestTarget : IServiceProvider, IInitializer, IAsyncDisposable
    {
        private readonly BootstrapperStateMachineTest test;

        public TestTarget(BootstrapperStateMachineTest test)
        {
            this.test = test;
        }

        public object GetService(Type serviceType)
        {
            return serviceType switch
            {
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

        public ValueTask DisposeAsync()
        {
            AddEvent(Disposed);
            return default;
        }

        public void AddEvent(string id)
        {
            test.Helper_AddEvent(id);
        }
    }

    #endregion
}
