namespace InjectedTests.Internal;

internal static class ServiceProviderExtensions
{
    public static IEnumerable<T> GetServices<T>(this IServiceProvider services)
    {
        return services.GetRequiredService<IEnumerable<T>>();
    }

    public static T GetRequiredService<T>(this IServiceProvider services)
    {
        var serviceType = typeof(T);
        var service = services.GetService(serviceType)
            ?? throw new InvalidOperationException($"No service registered for {serviceType}.");

        return (T)service;
    }
}
