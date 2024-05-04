using Microsoft.Extensions.DependencyInjection;

namespace InjectedTests.Abstractions;

public interface IConfigurableBootstrapper
{
    public void ConfigureServices(Action<IServiceCollection> configure);
}
