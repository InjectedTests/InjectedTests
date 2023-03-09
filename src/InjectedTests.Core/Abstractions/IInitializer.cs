namespace InjectedTests.Abstractions;

public interface IInitializer
{
    ValueTask InitializeAsync();
}
