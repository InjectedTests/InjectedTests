#if !NET8_0_OR_GREATER

#pragma warning disable IDE0130 // This is a shim that should replace missing system types.
namespace System;
#pragma warning restore IDE0130

internal static class TaskExtensions
{
    public static async Task<T> WaitAsync<T>(this Task<T> task, TimeSpan timeout, CancellationToken cancellationToken)
    {
        await ((Task)task).WaitAsync(timeout, cancellationToken).ConfigureAwait(false);
        return await task.ConfigureAwait(false);
    }

    public static async Task WaitAsync(this Task task, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var delayTask = Task.Delay(timeout, cancellationToken);
        var completedTask = await Task.WhenAny(task, delayTask).ConfigureAwait(false);

        if (ReferenceEquals(completedTask, delayTask))
        {
            throw new TimeoutException();
        }

        await task.ConfigureAwait(false);
    }
}

internal sealed class TaskCompletionSource
{
    private readonly TaskCompletionSource<bool> inner = new();

    public Task Task => inner.Task;

    public void SetResult()
    {
        inner.SetResult(true);
    }
}

#endif
