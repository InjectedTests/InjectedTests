using InjectedTests.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InjectedTests;

public static class InitializationExtensions
{
    public static T ConfigureInitializer<T>(this T bootstrapper, Action<InitializerBuilder> configure)
        where T : IConfigurableBootstrapper
    {
        return bootstrapper.ConfigureServices<T>(r => configure(new InitializerBuilder(r)));
    }

    public static InitializerBuilder With(this InitializerBuilder builder, Action initializer)
    {
        return builder.With(() =>
        {
            initializer();
            return default;
        });
    }

    public static InitializerBuilder With<T>(this InitializerBuilder builder, Action<T> initializer)
    {
        return builder.With<T>(d =>
        {
            initializer(d);
            return default;
        });
    }

    public static InitializerBuilder With<T1, T2>(this InitializerBuilder builder, Func<T1, T2, ValueTask> initializer)
    {
        return builder
            .TryAddDependencies<T1, T2>()
            .With<InitializerDependencies<T1, T2>>(d => initializer(d.Dependency1, d.Dependency2));
    }

    public static InitializerBuilder With<T1, T2>(this InitializerBuilder builder, Action<T1, T2> initializer)
    {
        return builder.With<T1, T2>((d1, d2) =>
        {
            initializer(d1, d2);
            return default;
        });
    }

    public static InitializerBuilder With<T1, T2, T3>(this InitializerBuilder builder, Func<T1, T2, T3, ValueTask> initializer)
    {
        return builder
            .TryAddDependencies<T1, T2>()
            .With<InitializerDependencies<T1, T2>, T3>((d1, d2) => initializer(d1.Dependency1, d1.Dependency2, d2));
    }

    public static InitializerBuilder With<T1, T2, T3>(this InitializerBuilder builder, Action<T1, T2, T3> initializer)
    {
        return builder.With<T1, T2, T3>((d1, d2, d3) =>
        {
            initializer(d1, d2, d3);
            return default;
        });
    }

    public static async ValueTask InitializeAsync(this IServiceProvider services)
    {
        foreach (var initializer in services.GetServices<IInitializer>())
        {
            await initializer.InitializeAsync().ConfigureAwait(false);
        }
    }

    private static InitializerBuilder TryAddDependencies<T1, T2>(this InitializerBuilder builder)
    {
        builder.Services.TryAddTransient<InitializerDependencies<T1, T2>>();
        return builder;
    }

    private sealed class InitializerDependencies<T1, T2>
    {
        public InitializerDependencies(T1 dependency1, T2 dependency2)
        {
            Dependency1 = dependency1;
            Dependency2 = dependency2;
        }

        public T1 Dependency1 { get; }
        public T2 Dependency2 { get; }
    }
}
