using InjectedTests.Internal;

namespace InjectedTests;

public sealed class WaitableSynchronizationContextTest
{
    #region state

    private Func<ValueTask<IReadOnlyList<SynchronizationContext>>> work;
    private IReadOnlyList<SynchronizationContext> contexts;

    #endregion

    [Fact]
    public void ExecuteOnContext_ExecutesSynchronously_SynchronizationContextCorrect()
    {
        Given_Work_Synchronous();
        When_Context_ExecuteWork();
        Then_SynchronizationContexts_AllCorrect();
    }

    [Fact]
    public void ExecuteOnContext_ExecutesAsynchronously_SynchronizationContextCorrect()
    {
        Given_Work_Asynchronous();
        When_Context_ExecuteWork();
        Then_SynchronizationContexts_AllCorrect();
    }

    [Fact]
    public void ExecuteOnContext_ExecuteContextInContext_SynchronizationContextCorrect()
    {
        Given_Work_NestedContext();
        When_Context_ExecuteWork();
        Then_SynchronizationContexts_AllCorrect();
    }

    [Fact]
    public void ExecuteOnContext_SynchronousException_ExceptionThrown()
    {
        Given_Work_SynchronousException();
        Then_Context_ExecuteWorkThrowsException();
    }

    [Fact]
    public void ExecuteOnContext_AsynchronousException_ExceptionThrown()
    {
        Given_Work_AsynchronousException();
        Then_Context_ExecuteWorkThrowsException();
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

    private void When_Context_ExecuteWork()
    {
        contexts = WaitableSynchronizationContext.ExecuteOnContext(work, CancellationToken.None);
    }

    private void Then_SynchronizationContexts_AllCorrect()
    {
        Assert.All(contexts, c => Assert.IsType<WaitableSynchronizationContext>(c));
    }

    private void Then_Context_ExecuteWorkThrowsException()
    {
        var exception = Assert.Throws<Exception>(When_Context_ExecuteWork);
        Assert.Equal("boom", exception.Message);
    }

    private static ValueTask<IReadOnlyList<SynchronizationContext>> Helper_ExecuteSynchronousWork()
    {
        var contexts = new List<SynchronizationContext>
        {
            SynchronizationContext.Current
        };

        return new(contexts);
    }

    private static async ValueTask<IReadOnlyList<SynchronizationContext>> Helper_ExecuteAsynchronousWork()
    {
        var contexts = new List<SynchronizationContext>
        {
            SynchronizationContext.Current
        };

        await Task.Delay(TimeSpan.FromMilliseconds(1));

        contexts.Add(SynchronizationContext.Current);

        return contexts;
    }

    private static ValueTask<IReadOnlyList<SynchronizationContext>> Helper_ExecuteNestedAsynchronousWork()
    {
        var contexts = new List<SynchronizationContext>
        {
            SynchronizationContext.Current
        };

        var other = WaitableSynchronizationContext
            .ExecuteOnContext(Helper_ExecuteAsynchronousWork, CancellationToken.None);

        contexts.AddRange(other);

        contexts.Add(SynchronizationContext.Current);

        return new(contexts);
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

    #endregion
}
