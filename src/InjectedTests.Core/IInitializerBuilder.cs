namespace InjectedTests;

public interface IInitializerBuilder
{
    IInitializerBuilder EnsureDependency<T>() where T : class;

    IInitializerBuilder With(Func<ValueTask> initializer);

    IInitializerBuilder With<T>(Func<T, ValueTask> initializer);
}
