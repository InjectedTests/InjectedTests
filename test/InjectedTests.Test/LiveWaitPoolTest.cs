using InjectedTests.Internal;

namespace InjectedTests;

public sealed class LiveWaitPoolTest(ITestContextAccessor context)
{
    #region state

    private readonly LiveWaitPool pool = new();
    private readonly TaskCompletionSource<int> workCompletionSource = new();
    private Func<ValueTask> work;
    private Task workerTask;
    private int workThreadId;
    private int workerThreadId;

    private CancellationToken TestCancellationToken => context.Current.CancellationToken;

    #endregion

    [Fact]
    public async ValueTask Enqueue_NoWorkers_Executed()
    {
        When_Pool_EnqueueWorkCallback();
        await When_Work_Completes();
    }

    [Fact]
    public void RunToEnd_EndsImmediately_ExecutedOnWorker()
    {
        Given_Work_EndsImmediately();
        When_Pool_RunWorkToEnd();
        Then_Work_ExecutedOnWorkerThread();
    }

    [Fact]
    public void RunToEnd_WithCallback_ExecutedOnWorker()
    {
        Given_Work_WithCallback();
        When_Pool_RunWorkToEnd();
        Then_Work_ExecutedOnWorkerThread();
    }

    [Fact]
    public async ValueTask Concurrency_EnqueueCallbacksWhileWorkersComeAndGo_AllCallbacksExecuted()
    {
        When_Pool_StartWorkers();
        await When_Pool_EnqueueManyCallbacks();
        await When_Pool_StopWorkers();
    }

    private async ValueTask When_Pool_StopWorkers()
    {
        workCompletionSource.SetCanceled();
        await workerTask.WaitAsync(TimeSpan.FromSeconds(5), TestCancellationToken);
    }

    private async ValueTask When_Pool_EnqueueManyCallbacks()
    {
        var tasks = Enumerable
            .Range(0, 100_000)
            .Select(_ => EnqueueCallback());

        await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(5), TestCancellationToken);
        return;

        Task EnqueueCallback()
        {
            var completionSource = new TaskCompletionSource<int>();
            pool.Enqueue(new(Helper_ExecuteCallback, completionSource));
            return completionSource.Task;
        }
    }

    private void When_Pool_StartWorkers()
    {
        workerTask = Task.Run(Helper_RunWorkers, TestCancellationToken);
    }

    private void Helper_RunWorkers()
    {
        while (!workCompletionSource.Task.IsCompleted)
        {
            pool.RunToEnd(async () => await Task.Yield(), TestCancellationToken);
        }
    }

    #region given, when, then

    private void Given_Work_EndsImmediately()
    {
        work = ExecuteWorkImmediately;
        return;

        ValueTask ExecuteWorkImmediately()
        {
            workThreadId = Environment.CurrentManagedThreadId;
            return default;
        }
    }

    private void Given_Work_WithCallback()
    {
        work = ExecuteWorkWithCallback;
        return;

        async ValueTask ExecuteWorkWithCallback()
        {
            When_Pool_EnqueueWorkCallback();
            await When_Work_Completes();
        }
    }

    private void When_Pool_EnqueueWorkCallback()
    {
        pool.Enqueue(new(Helper_ExecuteCallback, workCompletionSource));
    }

    private void When_Pool_RunWorkToEnd()
    {
        pool.RunToEnd(work, TestCancellationToken);
        workerThreadId = Environment.CurrentManagedThreadId;
    }

    private async ValueTask When_Work_Completes()
    {
        workThreadId = await workCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestCancellationToken);
    }

    private void Then_Work_ExecutedOnWorkerThread()
    {
        Assert.Equal(workerThreadId, workThreadId);
    }

    private static void Helper_ExecuteCallback(object state)
    {
        ((TaskCompletionSource<int>)state).SetResult(Environment.CurrentManagedThreadId);
    }

    #endregion
}
