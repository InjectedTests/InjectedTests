namespace InjectedTests.Extensibility;

public static class DisposableExtensions
{
    public static async ValueTask TryDisposeAsync<T>(this T instance)
        where T : class
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
