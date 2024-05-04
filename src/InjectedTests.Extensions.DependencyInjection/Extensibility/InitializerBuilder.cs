using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InjectedTests.Extensibility;

internal sealed class InitializerBuilder : IInitializerBuilder
{
    private readonly IServiceCollection services;

    public InitializerBuilder(IServiceCollection services)
    {
        this.services = services;
    }

    public IInitializerBuilder EnsureDependency<T>()
         where T : class
    {
        services.TryAddTransient<T>();
        return this;
    }

    public IInitializerBuilder With(Func<ValueTask> initializer)
    {
        services.AddTransient<IInitializer>(p => ActivatorUtilities.CreateInstance<Initializer>(p, initializer));
        return this;
    }

    public IInitializerBuilder With<T>(Func<T, ValueTask> initializer)
    {
        services.AddTransient<IInitializer>(p => ActivatorUtilities.CreateInstance<Initializer<T>>(p, initializer));
        return this;
    }
}
