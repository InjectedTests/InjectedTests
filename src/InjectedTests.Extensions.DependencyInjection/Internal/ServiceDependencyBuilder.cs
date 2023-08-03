using InjectedTests.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InjectedTests.Internal;

public sealed class ServiceDependencyBuilder : IDependencyBuilder
{
    private readonly IServiceCollection services;

    public ServiceDependencyBuilder(IServiceCollection services)
    {
        this.services = services;
    }

    public void TryAdd(DependencyDefinition definition)
    {
        services.TryAdd(ToServiceDescriptor(definition));
    }

    public void AddEnumerable(DependencyDefinition definition)
    {
        services.Add(ToServiceDescriptor(definition));
    }

    private static ServiceDescriptor ToServiceDescriptor(DependencyDefinition definition)
    {
        var lifetime = definition.Lifetime switch
        {
            DependencyLifetime.Transient => ServiceLifetime.Transient,
            DependencyLifetime.Singleton => ServiceLifetime.Singleton,
            var l => throw new InvalidOperationException($"Unknown value {l}.")
        };

        return definition switch
        {
            { ImplementationType: { } t } => new ServiceDescriptor(definition.ServiceType, t, lifetime),
            { ImplementationFactory: { } f } => new ServiceDescriptor(definition.ServiceType, f, lifetime),
            { ServiceType: { } t } => throw new InvalidOperationException($"Invalid definition for {t}."),
        };
    }
}
