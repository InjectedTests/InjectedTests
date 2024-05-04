namespace InjectedTests.Abstractions;

public interface IInitializableBootstrapper
{
    void ConfigureInitializer(Action<IInitializerBuilder> configure);
}
