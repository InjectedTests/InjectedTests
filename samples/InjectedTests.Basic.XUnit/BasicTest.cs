using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace InjectedTests;

public sealed class BasicTest : IAsyncDisposable
{
    private readonly ServiceProviderBootstrapper bootstrapper = new ServiceProviderBootstrapper()
        .ConfigureServices(s => s.TryAddSingleton<TestService>());

    private TestService Service => bootstrapper.GetRequiredService<TestService>();

    public ValueTask DisposeAsync()
    {
        return bootstrapper.DisposeAsync();
    }

    [Fact]
    public void BasicUsage()
    {
        Assert.Equal(0, Service.DoWork());
    }

    [Fact]
    public void WithAdditionalConfiguration()
    {
        bootstrapper.ConfigureServices(s => s.TryAddTransient(p => new TestInitialValue { Initial = 42 }));
        Assert.Equal(42, Service.DoWork());
    }

    [Fact]
    public void WithInitialization()
    {
        bootstrapper.ConfigureInitializer(b => b.With<TestService>(s => s.DoWork()));
        Assert.Equal(1, Service.DoWork());
    }

    private sealed class TestService : IDisposable
    {
        private int _counter;

        public TestService(TestInitialValue initial = default)
        {
            _counter = initial?.Initial ?? 0;
        }

        public int DoWork()
        {
            return _counter++;
        }

        public void Dispose() { }
    }

    private sealed class TestInitialValue
    {
        public int Initial { get; set; }
    }
}
