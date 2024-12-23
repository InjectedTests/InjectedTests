using Autofac;

namespace InjectedTests;

internal sealed class InitializerBuilder : IInitializerBuilder
{
    private readonly ContainerBuilder builder;

    public InitializerBuilder(ContainerBuilder builder)
    {
        this.builder = builder;
    }

    public IInitializerBuilder EnsureDependency<T>()
         where T : class
    {
        builder.RegisterType<T>().InstancePerDependency();
        return this;
    }

    public IInitializerBuilder With(Func<ValueTask> initializer)
    {
        builder.Register<IInitializer>(_ => new Initializer(initializer));
        return this;
    }

    public IInitializerBuilder With<T>(Func<T, ValueTask> initializer)
    {
        builder.Register<IInitializer>(c => new Initializer<T>(initializer, c.Resolve<T>()));
        return this;
    }
}
