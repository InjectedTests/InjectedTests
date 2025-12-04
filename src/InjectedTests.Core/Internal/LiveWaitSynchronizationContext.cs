namespace InjectedTests.Internal;

internal sealed class LiveWaitSynchronizationContext(LiveWaitPool pool) : SynchronizationContext
{
    private LiveWaitPool Pool => pool;

    public static void RunOnContext(LiveWait instance, Func<ValueTask> work, CancellationToken cancellationToken)
    {
        var resetContext = false;
        var previous = Current;

        try
        {
            if (previous is not LiveWaitSynchronizationContext context)
            {
                context = instance.Context;
                SetSynchronizationContext(context);
                resetContext = true;
            }

            context.Pool.RunToEnd(work, cancellationToken);
        }
        finally
        {
            if (resetContext)
            {
                SetSynchronizationContext(previous);
            }
        }
    }

    public override void Post(SendOrPostCallback d, object? state)
    {
        Pool.Enqueue(new(d, state));
    }

    public override SynchronizationContext CreateCopy()
    {
        return this;
    }
}
