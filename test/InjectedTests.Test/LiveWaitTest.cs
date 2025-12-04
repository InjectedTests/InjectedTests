using InjectedTests.Internal;

namespace InjectedTests;

public sealed class LiveWaitTest(ITestContextAccessor context)
{
    #region state

    private readonly TaskCompletionSource delayedTaskTrigger = new();

    private Func<ValueTask<IReadOnlyList<SynchronizationContext>>> work;
    private IReadOnlyList<SynchronizationContext> contexts;
    private Task<IReadOnlyList<SynchronizationContext>> delayedTask;

    private CancellationToken TestCancellationToken => context.Current.CancellationToken;

    #endregion

    [Fact]
    public void RunToEnd_ExecutesSynchronously_SynchronizationContextCorrect()
    {
        Given_Work_Synchronous();
        When_LiveWait_RunToEnd();
        Then_SynchronizationContexts_AllLiveWait();
    }

    [Fact]
    public void RunToEnd_ExecutesAsynchronously_SynchronizationContextCorrect()
    {
        Given_Work_Asynchronous();
        When_LiveWait_RunToEnd();
        Then_SynchronizationContexts_AllLiveWait();
    }

    [Fact]
    public void RunToEnd_ExecuteContextInContext_SynchronizationContextCorrect()
    {
        Given_Work_NestedContext();
        When_LiveWait_RunToEnd();
        Then_SynchronizationContexts_AllLiveWait();
    }

    [Fact]
    public void RunToEnd_SynchronousException_ExceptionThrown()
    {
        Given_Work_SynchronousException();
        Then_LiveWait_RunToEndThrowsException();
    }

    [Fact]
    public void RunToEnd_AsynchronousException_ExceptionThrown()
    {
        Given_Work_AsynchronousException();
        Then_LiveWait_RunToEndThrowsException();
    }

    [Fact]
    public async ValueTask RunToEnd_ContinuationExecutesLater_ExecutedOnDefaultContext()
    {
        Given_Work_AsynchronousDelayed();
        When_LiveWait_RunToEnd();
        When_DelayedWork_Starts();
        await When_DelayedWork_Completes();
        Then_SynchronizationContexts_NotEmpty();
    }

    #region given, when, then

    private void Given_Work_Synchronous()
    {
        work = Helper_ExecuteSynchronousWork;
    }

    private void Given_Work_Asynchronous()
    {
        work = Helper_ExecuteAsynchronousWork;
    }

    private void Given_Work_NestedContext()
    {
        work = Helper_ExecuteNestedAsynchronousWork;
    }

    private void Given_Work_SynchronousException()
    {
        work = Helper_ExecuteSynchronousException;
    }

    private void Given_Work_AsynchronousException()
    {
        work = Helper_ExecuteAsynchronousException;
    }

    private void Given_Work_AsynchronousDelayed()
    {
        work = Helper_StartAsynchronousDelayed;
    }

    private void When_LiveWait_RunToEnd()
    {
        contexts = LiveWait.RunToEnd(work, CancellationToken.None);
    }

    private void When_DelayedWork_Starts()
    {
        delayedTaskTrigger.SetResult();
    }

    private async ValueTask When_DelayedWork_Completes()
    {
        contexts = await delayedTask.WaitAsync(TimeSpan.FromSeconds(5), TestCancellationToken);
    }

    private void Then_SynchronizationContexts_NotEmpty()
    {
        Assert.NotEmpty(contexts);
    }

    private void Then_SynchronizationContexts_AllLiveWait()
    {
        Then_SynchronizationContexts_NotEmpty();
        Assert.All(contexts, c => Assert.IsType<LiveWaitSynchronizationContext>(c));
    }

    private void Then_LiveWait_RunToEndThrowsException()
    {
        var exception = Assert.Throws<Exception>(When_LiveWait_RunToEnd);
        Assert.Equal("boom", exception.Message);
    }

    private static ValueTask<IReadOnlyList<SynchronizationContext>> Helper_ExecuteSynchronousWork()
    {
        return new([SynchronizationContext.Current,]);
    }

    private static async ValueTask<IReadOnlyList<SynchronizationContext>> Helper_ExecuteAsynchronousWork()
    {
        var preContext = SynchronizationContext.Current;
        await Task.Delay(TimeSpan.FromMilliseconds(1));
        var postContext = SynchronizationContext.Current;

        return [
            preContext,
            postContext,
        ];
    }

    private static ValueTask<IReadOnlyList<SynchronizationContext>> Helper_ExecuteNestedAsynchronousWork()
    {
        var preContext = SynchronizationContext.Current;
        var other = LiveWait.RunToEnd(Helper_ExecuteAsynchronousWork);
        var postContext = SynchronizationContext.Current;

        return new([
            preContext,
            ..other,
            postContext,
        ]);
    }

    private static ValueTask<IReadOnlyList<SynchronizationContext>> Helper_ExecuteSynchronousException()
    {
        throw new("boom");
    }

    private static async ValueTask<IReadOnlyList<SynchronizationContext>> Helper_ExecuteAsynchronousException()
    {
        await Task.Delay(TimeSpan.FromMilliseconds(1));

        throw new("boom");
    }

    private ValueTask<IReadOnlyList<SynchronizationContext>> Helper_StartAsynchronousDelayed()
    {
        delayedTask = Helper_ExecuteAsynchronousDelayed();
        return new([]);
    }

    private async Task<IReadOnlyList<SynchronizationContext>> Helper_ExecuteAsynchronousDelayed()
    {
        await delayedTaskTrigger.Task;
        return await Helper_ExecuteAsynchronousWork();
    }

    #endregion
}
