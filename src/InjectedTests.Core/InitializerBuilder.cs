using InjectedTests.Abstractions;
using InjectedTests.Internal;

namespace InjectedTests;

public sealed class InitializerBuilder
{
    public InitializerBuilder(IDependencyBuilder dependencies)
    {
        Dependencies = dependencies;
    }

    public IDependencyBuilder Dependencies { get; }

    public InitializerBuilder With(Func<ValueTask> initializer)
    {
        var definition = DependencyDefinition.CreateTransient<IInitializer, Initializer>(p => new Initializer(initializer));
        Dependencies.AddEnumerable(definition);
        return this;
    }

    public InitializerBuilder With<T>(Func<T, ValueTask> initializer)
    {
        var definition = DependencyDefinition.CreateTransient<IInitializer, Initializer<T>>(p => CreateInitializer(p, initializer));
        Dependencies.AddEnumerable(definition);
        return this;
    }

    private Initializer<T> CreateInitializer<T>(IServiceProvider services, Func<T, ValueTask> initializer)
    {
        var dependencies = services.GetRequiredService<T>();
        return new(initializer, dependencies);
    }
}
