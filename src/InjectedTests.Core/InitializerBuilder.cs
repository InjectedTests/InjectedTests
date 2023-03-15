using InjectedTests.Abstractions;
using InjectedTests.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace InjectedTests;

public sealed class InitializerBuilder
{
    public InitializerBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }

    public InitializerBuilder With(Func<ValueTask> initializer)
    {
        Services.AddTransient<IInitializer>(p => ActivatorUtilities.CreateInstance<Initializer>(p, initializer));
        return this;
    }

    public InitializerBuilder With<T>(Func<T, ValueTask> initializer)
    {
        Services.AddTransient<IInitializer>(p => ActivatorUtilities.CreateInstance<Initializer<T>>(p, initializer));
        return this;
    }
}
