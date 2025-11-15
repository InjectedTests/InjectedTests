namespace InjectedTests.Extensibility;

public static class DisposableExtensions
{
    public static async ValueTask TryDisposeAsync(this object instance)
    {
        switch (instance)
        {
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }
}
