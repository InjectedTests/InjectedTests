using InjectedTests.Abstractions;

namespace InjectedTests;

public static class InitializationExtensions
{
    public static T ConfigureInitializer<T>(this T bootstrapper, Action<IInitializerBuilder> configure)
        where T : IInitializableBootstrapper
    {
        bootstrapper.ConfigureInitializer(configure);
        return bootstrapper;
    }

    public static IInitializerBuilder With(this IInitializerBuilder builder, Action initializer)
    {
        return builder.With(() =>
        {
            initializer();
            return default;
        });
    }

    public static IInitializerBuilder With<T>(this IInitializerBuilder builder, Action<T> initializer)
    {
        return builder.With<T>(d =>
        {
            initializer(d);
            return default;
        });
    }

    public static IInitializerBuilder With<T1, T2>(this IInitializerBuilder builder, Func<T1, T2, ValueTask> initializer)
    {
        return builder
            .EnsureDependency<InitializerDependencies<T1, T2>>()
            .With<InitializerDependencies<T1, T2>>(d => initializer(d.Dependency1, d.Dependency2));
    }

    public static IInitializerBuilder With<T1, T2>(this IInitializerBuilder builder, Action<T1, T2> initializer)
    {
        return builder.With<T1, T2>((d1, d2) =>
        {
            initializer(d1, d2);
            return default;
        });
    }

    public static IInitializerBuilder With<T1, T2, T3>(this IInitializerBuilder builder, Func<T1, T2, T3, ValueTask> initializer)
    {
        return builder
            .EnsureDependency<InitializerDependencies<T1, T2>>()
            .With<InitializerDependencies<T1, T2>, T3>((d1, d2) => initializer(d1.Dependency1, d1.Dependency2, d2));
    }

    public static IInitializerBuilder With<T1, T2, T3>(this IInitializerBuilder builder, Action<T1, T2, T3> initializer)
    {
        return builder.With<T1, T2, T3>((d1, d2, d3) =>
        {
            initializer(d1, d2, d3);
            return default;
        });
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
