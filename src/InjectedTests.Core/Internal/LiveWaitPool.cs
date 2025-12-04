using System.Collections.Concurrent;

namespace InjectedTests.Internal;

internal sealed class LiveWaitPool
{
    private readonly ManualResetEventSlim resetEvent = new();
    private readonly ConcurrentQueue<WorkItem> workQueue = new();
    private readonly SynchronizationContext fallbackContext = new();

    private volatile int enqueueing;
    private volatile int workers;

    public void Enqueue(in WorkItem item)
    {
        Interlocked.Increment(ref enqueueing);
        try
        {
            if (workers > 0)
            {
                workQueue.Enqueue(item);
                resetEvent.Set();
            }
            else
            {
                EnqueueFallback(item);
            }
        }
        finally
        {
            Interlocked.Decrement(ref enqueueing);
        }
    }

    public void RunToEnd(Func<ValueTask> work, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref workers);
        try
        {
            var valueTask = work();
            if (valueTask.IsCompleted)
            {
                valueTask.ConfigureAwait(false).GetAwaiter().GetResult();
                return;
            }

            var task = valueTask.AsTask();
            ProcessWhile(task, cancellationToken);
            task.ConfigureAwait(false).GetAwaiter().GetResult();
        }
        finally
        {
            if (Interlocked.Decrement(ref workers) == 0)
            {
                Drain();
            }
        }
    }

    private void EnqueueFallback(in WorkItem item)
    {
        fallbackContext.Post(item.Callback, item.State);
    }

    private void ProcessWhile(Task task, CancellationToken cancellationToken)
    {
        task.ContinueWith(_ => resetEvent.Set(), cancellationToken);

        while (!task.IsCompleted)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (workQueue.TryDequeue(out var work))
            {
                work.Callback(work.State);
            }
            else
            {
                resetEvent.Reset();

                if (workQueue.IsEmpty)
                {
                    resetEvent.Wait(cancellationToken);
                }
            }
        }
    }

    private void Drain()
    {
        while (true)
        {
            if (workers > 0)
            {
                return;
            }

            if (workQueue.TryDequeue(out var work))
            {
                EnqueueFallback(work);
            }
            else if (enqueueing == 0 && workQueue.IsEmpty)
            {
                return;
            }
        }
    }

#if NET8_0_OR_GREATER
    public readonly record struct WorkItem(SendOrPostCallback Callback, object? State);
#else
    public readonly struct WorkItem(SendOrPostCallback callback, object? state)
    {
        public SendOrPostCallback Callback => callback;
        public object? State => state;
    }
#endif
}
