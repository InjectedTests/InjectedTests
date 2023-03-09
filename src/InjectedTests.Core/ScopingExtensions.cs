using InjectedTests.Abstractions;
using InjectedTests.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InjectedTests;

public static class ScopingExtensions
{
    public static T ConfigureTestScope<T>(this T bootstrapper)
        where T : IConfigurableBootstrapper
    {
        return bootstrapper.ConfigureServices<T>(s => s.TryAddSingleton<TestScope>());
    }

    public static IServiceProvider GetScopedServiceProvider(this IServiceProvider rootProvider)
    {
        return rootProvider.GetRequiredService<TestScope>().ServiceProvider;
    }

    public static T GetRequiredScopedService<T>(this IServiceProvider rootProvider)
        where T : notnull
    {
        return rootProvider.GetScopedServiceProvider().GetRequiredService<T>();
    }

    private sealed class TestScope : IAsyncDisposable
    {
        private readonly IServiceScopeFactory scopeFactory;
        private IServiceScope? scope;

        public TestScope(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        public IServiceProvider ServiceProvider => (scope ??= scopeFactory.CreateScope()).ServiceProvider;

        public async ValueTask DisposeAsync()
        {
            if (scope is { } toDispose)
            {
                await toDispose.TryDisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
