using Microsoft.Extensions.DependencyInjection;

namespace InjectedTests.Abstractions;

public interface IConfigurableDependencies
{
    public void ConfigureDependencies(Action<IServiceCollection> configure);
}
