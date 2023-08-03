namespace InjectedTests.Abstractions;

public interface IDependencyBuilder
{
    public void TryAdd(DependencyDefinition definition);
    public void AddEnumerable(DependencyDefinition definition);
}
