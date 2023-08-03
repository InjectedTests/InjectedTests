namespace InjectedTests.Abstractions;

public sealed class DependencyDefinition
{
    public DependencyDefinition(
        Type serviceType,
        DependencyLifetime lifetime,
        Type implementationType)
    {
        ServiceType = serviceType;
        Lifetime = lifetime;
        ImplementationType = implementationType;
    }
    public DependencyDefinition(
        Type serviceType,
        DependencyLifetime lifetime,
        Func<IServiceProvider, object> implementationFactory)
    {
        ServiceType = serviceType;
        Lifetime = lifetime;
        ImplementationFactory = implementationFactory;
    }

    public DependencyLifetime Lifetime { get; }
    public Type ServiceType { get; }
    public Type? ImplementationType { get; }
    public Func<IServiceProvider, object>? ImplementationFactory { get; }

    public static DependencyDefinition CreateTransient<TService, TImplementation>()
    {
        return new(typeof(TService), DependencyLifetime.Transient, typeof(TImplementation));
    }

    public static DependencyDefinition CreateSingleton<TService, TImplementation>()
    {
        return new(typeof(TService), DependencyLifetime.Singleton, typeof(TImplementation));
    }

    public static DependencyDefinition CreateTransient<TService, TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class
    {
        return new(typeof(TService), DependencyLifetime.Transient, factory);
    }

    public static DependencyDefinition CreateSingleton<TService, TImplementation>(Func<IServiceProvider, TImplementation> factory)
        where TImplementation : class
    {
        return new(typeof(TService), DependencyLifetime.Singleton, factory);
    }
}
