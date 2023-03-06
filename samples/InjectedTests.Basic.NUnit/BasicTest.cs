using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;

namespace InjectedTests;

public sealed class BasicTest
{
    private ServiceProviderBootstrapper bootstrapper;

    private TestService Service => bootstrapper.GetRequiredService<TestService>();

    [SetUp]
    public void SetUp()
    {
        bootstrapper = new ServiceProviderBootstrapper()
            .ConfigureServices(s => s.TryAddSingleton<TestService>());
    }

    [TearDown]
    public ValueTask TearDown()
    {
        return bootstrapper.DisposeAsync();
    }

    [Test]
    public void BasicUsage()
    {
        Assert.AreEqual(0, Service.DoWork());
    }

    [Test]
    public void WithAdditionalConfiguration()
    {
        bootstrapper.ConfigureServices(s => s.TryAddTransient(p => new TestInitialValue { Initial = 42 }));
        Assert.AreEqual(42, Service.DoWork());
    }

    [Test]
    public void WithInitialization()
    {
        bootstrapper.ConfigureInitializer(b => b.With<TestService>(s => s.DoWork()));
        Assert.AreEqual(1, Service.DoWork());
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
