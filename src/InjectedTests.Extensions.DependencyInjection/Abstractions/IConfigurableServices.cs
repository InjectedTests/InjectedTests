using Microsoft.Extensions.DependencyInjection;

namespace InjectedTests.Abstractions;

public interface IConfigurableServices
{
    public void ConfigureServices(Action<IServiceCollection> configure);
}
