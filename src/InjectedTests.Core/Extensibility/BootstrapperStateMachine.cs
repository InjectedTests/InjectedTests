namespace InjectedTests.Extensibility;

public sealed partial class BootstrapperStateMachine<TConfiguration, TBootstrapped> : IAsyncDisposable
    where TConfiguration : class
    where TBootstrapped : class
{
    private readonly IBootstrappingStrategy<TConfiguration, TBootstrapped> bootstrapper;
    private BootstrapperState state;

    public BootstrapperStateMachine(IBootstrappingStrategy<TConfiguration, TBootstrapped> bootstrapper)
    {
        this.bootstrapper = bootstrapper;
        state = new ConfiguringState(this);
    }

    public TBootstrapped Bootstrapped => Volatile
        .Read(ref state)
        .EnsureBootstrapped();

    public void Configure(Action<TConfiguration> configure) => Volatile
        .Read(ref state)
        .Configure(configure);

    public ValueTask DisposeAsync() => Interlocked
        .Exchange(ref state, DisposedState.Instance)
        .DisposeAsync();

    private void MoveToConfiguring(ConfiguringState current, ConfiguringState updated)
    {
        var exchanged = Interlocked.CompareExchange(ref state, updated, current);
        if (!ReferenceEquals(current, exchanged))
        {
            throw new InvalidOperationException("Concurrent configuring detected.");
        }
    }

    private async ValueTask<BootstrappedState> MoveToBootstrappedAsync(ConfiguringState configuring)
    {
        var bootstrapping = new BootstrappingState(this, configuring);
        try
        {
            var exchangedForBootstrapping = Interlocked.CompareExchange(ref state, bootstrapping, configuring);
            if (!ReferenceEquals(configuring, exchangedForBootstrapping))
            {
                throw new InvalidOperationException("Concurrent bootstrapping detected.");
            }

            var bootstrapped = await bootstrapping.Task.ConfigureAwait(false);
            try
            {
                var exchangedForBootstrapped = Interlocked.CompareExchange(ref state, bootstrapped, bootstrapping);
                if (!ReferenceEquals(bootstrapping, exchangedForBootstrapped))
                {
                    throw new InvalidOperationException("Concurrent bootstrapping detected.");
                }

                return bootstrapped;
            }
            catch
            {
                await bootstrapped.DisposeAsync().ConfigureAwait(false);
                throw;
            }
        }
        catch
        {
            await bootstrapping.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    private async ValueTask<BootstrappedState> BootstrapAsync(ConfiguringState configuring)
    {
        var configuration = bootstrapper.CreateConfiguration();
        configuring.Configure(configuration);

        var bootstrapped = await bootstrapper.BootstrapAsync(configuration).ConfigureAwait(false);
        try
        {
            await bootstrapper.GetServiceProvider(bootstrapped).InitializeAsync().ConfigureAwait(false);
            return new(bootstrapped);
        }
        catch
        {
            await bootstrapped.TryDisposeAsync().ConfigureAwait(false);
            throw;
        }
    }
}
