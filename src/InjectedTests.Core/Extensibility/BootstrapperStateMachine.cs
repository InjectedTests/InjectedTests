using InjectedTests.Internal;

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

    public TBootstrapped Bootstrapped => Volatile.Read(ref state) switch
    {
        BootstrappedState bootstrapped => bootstrapped.Instance,
        { } s => WaitableSynchronizationContext
            .ExecuteOnContext(s.EnsureBootstrappedAsync, CancellationToken.None),
    };

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

    private async ValueTask<TBootstrapped> MoveToBootstrappedAsync(ConfiguringState configuring)
    {
        var bootstrapping = new BootstrappingState(this, configuring);
        var comparedTo = Interlocked.CompareExchange(ref state, bootstrapping, configuring);
        if (ReferenceEquals(configuring, comparedTo))
        {
            var bootstrapped = await bootstrapping.Task.ConfigureAwait(false);
            try
            {
                comparedTo = Interlocked.CompareExchange(ref state, bootstrapped, bootstrapping);
                if (ReferenceEquals(bootstrapping, comparedTo))
                {
                    var instance = bootstrapped.Instance;
                    bootstrapped = null;
                    return instance;
                }
            }
            finally
            {
                if (bootstrapped is not null)
                {
                    await bootstrapped.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        return await comparedTo.EnsureBootstrappedAsync().ConfigureAwait(false);
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

    private static T SafeWait<T>(ValueTask<T> task)
    {
        return task switch
        {
            { IsCompleted: true } t => t.Result,
            { } t => t.AsTask().ConfigureAwait(false).GetAwaiter().GetResult(),
        };
    }
}
