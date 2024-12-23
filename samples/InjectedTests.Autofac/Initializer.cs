namespace InjectedTests;

internal interface IInitializer
{
    ValueTask InitializeAsync();
}

internal sealed class Initializer : IInitializer
{
    private readonly Func<ValueTask> initializer;

    public Initializer(Func<ValueTask> initializer)
    {
        this.initializer = initializer;
    }

    public ValueTask InitializeAsync()
    {
        return initializer();
    }
}

internal sealed class Initializer<T> : IInitializer
{
    private readonly Func<T, ValueTask> initializer;
    private readonly T dependency;

    public Initializer(Func<T, ValueTask> initializer, T dependency)
    {
        this.initializer = initializer;
        this.dependency = dependency;
    }

    public ValueTask InitializeAsync()
    {
        return initializer(dependency);
    }
}
