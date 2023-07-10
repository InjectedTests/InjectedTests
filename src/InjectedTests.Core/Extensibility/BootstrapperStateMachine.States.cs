namespace InjectedTests.Extensibility;

partial class BootstrapperStateMachine<TConfiguration, TBootstrapped>
{
    private abstract class BootstrapperState : IAsyncDisposable
    {
        public abstract void Configure(Action<TConfiguration> configure);

        public abstract ValueTask<TBootstrapped> EnsureBootstrappedAsync();

        public abstract ValueTask DisposeAsync();
    }

    private sealed class ConfiguringState : BootstrapperState
    {
        private readonly BootstrapperStateMachine<TConfiguration, TBootstrapped> stateMachine;
        private readonly Action<TConfiguration> configure;

        public ConfiguringState(BootstrapperStateMachine<TConfiguration, TBootstrapped> stateMachine)
            : this(stateMachine, _ => { })
        { }

        private ConfiguringState(
            BootstrapperStateMachine<TConfiguration, TBootstrapped> stateMachine,
            Action<TConfiguration> configure)
        {
            this.stateMachine = stateMachine;
            this.configure = configure;
        }

        public override void Configure(Action<TConfiguration> configure)
        {
            var updated = new ConfiguringState(stateMachine, this.configure + configure);
            stateMachine.MoveToConfiguring(this, updated);
        }

        public override ValueTask<TBootstrapped> EnsureBootstrappedAsync()
        {
            return stateMachine.MoveToBootstrappedAsync(this);
        }

        public override ValueTask DisposeAsync()
        {
            return new();
        }

        public void Configure(TConfiguration configuration)
        {
            configure(configuration);
        }
    }

    private sealed class BootstrappingState : BootstrapperState
    {
        private readonly BootstrapperStateMachine<TConfiguration, TBootstrapped> stateMachine;
        private readonly ConfiguringState configuringState;
        private readonly Lazy<ValueTask<BootstrappedState>> taskCache;

        public BootstrappingState(BootstrapperStateMachine<TConfiguration, TBootstrapped> stateMachine, ConfiguringState configuringState)
        {
            this.stateMachine = stateMachine;
            this.configuringState = configuringState;

            taskCache = new(BootstrapAsync);
        }

        public ValueTask<BootstrappedState> Task => taskCache.Value;

        public override void Configure(Action<TConfiguration> configure)
        {
            throw new InvalidOperationException("Configuration is only possible before bootstrapping.");
        }

        public async override ValueTask<TBootstrapped> EnsureBootstrappedAsync()
        {
            var bootstrapped = await Task.ConfigureAwait(false);
            return bootstrapped.Instance;
        }

        public override ValueTask DisposeAsync()
        {
            return default;
        }

        private ValueTask<BootstrappedState> BootstrapAsync()
        {
            return stateMachine.BootstrapAsync(configuringState);
        }
    }

    private sealed class BootstrappedState : BootstrapperState
    {
        public BootstrappedState(TBootstrapped instance)
        {
            Instance = instance;
        }

        public TBootstrapped Instance { get; }

        public override void Configure(Action<TConfiguration> configure)
        {
            throw new InvalidOperationException("Configuration is only possible before bootstrapping.");
        }

        public override ValueTask<TBootstrapped> EnsureBootstrappedAsync()
        {
            return new(Instance);
        }

        public override async ValueTask DisposeAsync()
        {
            await Instance.TryDisposeAsync().ConfigureAwait(false);
        }
    }

    private sealed class DisposedState : BootstrapperState
    {
        private DisposedState()
        {
        }

        public static DisposedState Instance { get; } = new();

        public override void Configure(Action<TConfiguration> configure)
        {
            throw new ObjectDisposedException(nameof(DisposedState));
        }

        public override ValueTask<TBootstrapped> EnsureBootstrappedAsync()
        {
            throw new ObjectDisposedException(nameof(DisposedState));
        }

        public override ValueTask DisposeAsync()
        {
            return new();
        }
    }
}
