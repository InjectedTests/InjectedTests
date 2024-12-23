using InjectedTests.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace InjectedTests.Extensibility;

public static class InitializationExtensions
{
    public static T ConfigureInitializer<T>(this T bootstrapper, Action<IInitializerBuilder> configure)
        where T : IConfigurableBootstrapper
    {
        return bootstrapper.ConfigureServices<T>(s => configure(new InitializerBuilder(s)));
    }

    public static async ValueTask InitializeAsync(this IServiceProvider services)
    {
        var scope = services.CreateScope();
        try
        {
            foreach (var initializer in scope.ServiceProvider.GetServices<IInitializer>())
            {
                await initializer.InitializeAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            await scope.TryDisposeAsync().ConfigureAwait(false);
        }
    }
}
