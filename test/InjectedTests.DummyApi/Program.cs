var builder = CreateHostBuilder(args);
using var host = builder.Build();
try
{
    await host.RunAsync();
}
finally
{
    if (host is IAsyncDisposable asyncDisposable)
    {
        await asyncDisposable.DisposeAsync();
    }
}
