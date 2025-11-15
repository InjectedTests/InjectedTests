namespace InjectedTests.Extensibility;

internal interface IInitializer
{
    ValueTask InitializeAsync();
}

internal sealed class Initializer(Func<ValueTask> initializer) : IInitializer
{
    public ValueTask InitializeAsync()
    {
        return initializer();
    }
}

internal sealed class Initializer<T>(Func<T, ValueTask> initializer, T dependency) : IInitializer
{
    public ValueTask InitializeAsync()
    {
        return initializer(dependency);
    }
}
