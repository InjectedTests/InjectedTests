using System.Collections.Concurrent;

namespace InjectedTests.Internal;

internal sealed class WaitableSynchronizationContext : SynchronizationContext, IDisposable
{
    private readonly ManualResetEventSlim _resetEvent = new();
    private readonly ConcurrentQueue<WorkItem> _workQueue = new();

    public static void ExecuteOnContext(Func<ValueTask> work, CancellationToken cancellationToken)
    {
        var previous = Current;
        if (previous is WaitableSynchronizationContext previousWaitable)
        {
            previousWaitable.Execute(work, cancellationToken);
            return;
        }

        using var context = new WaitableSynchronizationContext();
        SetSynchronizationContext(context);
        try
        {
            context.Execute(work, cancellationToken);
        }
        finally
        {
            SetSynchronizationContext(previous);
        }
    }

    public static T ExecuteOnContext<T>(Func<ValueTask<T>> work, CancellationToken cancellationToken)
    {
        T? result = default;

        ExecuteOnContext(async () => { result = await work(); }, cancellationToken);

        return result ?? throw new InvalidOperationException("Work has not completed yet.");
    }

    public void Execute(Func<ValueTask> work, CancellationToken cancellationToken)
    {
        var task = work();
        if (task.IsCompleted)
        {
            task.ConfigureAwait(false).GetAwaiter().GetResult();
            return;
        }

        ProcessWhile(task.AsTask(), cancellationToken);
    }

    public void ProcessWhile(Task task, CancellationToken cancellationToken)
    {
        while (!task.IsCompleted)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_workQueue.TryDequeue(out var work))
            {
                work.Callback(work.State);
            }
            else
            {
                Wait(cancellationToken);
            }
        }

        task.ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _resetEvent.Dispose();
    }

    public override void Post(SendOrPostCallback d, object? state)
    {
        _workQueue.Enqueue(new(d, state));
        _resetEvent.Set();
    }

    public override SynchronizationContext CreateCopy()
    {
        return this;
    }

    private void Wait(CancellationToken cancellationToken)
    {
        _resetEvent.Reset();

        if (_workQueue.IsEmpty)
        {
            _resetEvent.Wait(cancellationToken);
        }
    }

    private readonly struct WorkItem
    {
        public WorkItem(SendOrPostCallback callback, object? state)
        {
            Callback = callback;
            State = state;
        }

        public SendOrPostCallback Callback { get; }
        public object? State { get; }
    }
}
