namespace InjectedTests.Internal;

internal sealed class LiveWait
{
    private static readonly LiveWait Instance = new();

    private LiveWait()
    {
    }

    public LiveWaitSynchronizationContext Context { get; } = new(new());

    public static void RunToEnd(Func<ValueTask> work, CancellationToken cancellationToken = default)
    {
        LiveWaitSynchronizationContext.RunOnContext(Instance, work, cancellationToken);
    }

    public static T RunToEnd<T>(Func<ValueTask<T>> work, CancellationToken cancellationToken = default)
    {
        T? result = default;

        RunToEnd(async () => { result = await work(); }, cancellationToken);

        return result ?? throw new InvalidOperationException("Work has not completed yet.");
    }
}
