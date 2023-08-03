namespace InjectedTests.Abstractions;

public interface IConfigurableDependencies
{
    public void ConfigureDependencies(Action<IDependencyBuilder> configure);
}
