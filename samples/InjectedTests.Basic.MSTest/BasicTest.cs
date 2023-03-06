using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InjectedTests;

[TestClass]
public sealed class BasicTest
{
    private readonly ServiceProviderBootstrapper bootstrapper = new ServiceProviderBootstrapper()
        .ConfigureServices(s => s.TryAddSingleton<TestService>());

    private TestService Service => bootstrapper.GetRequiredService<TestService>();

    [TestCleanup]
    public Task TestCleanup()
    {
        return bootstrapper.DisposeAsync().AsTask();
    }

    [TestMethod]
    public void BasicUsage()
    {
        Assert.AreEqual(0, Service.DoWork());
    }

    [TestMethod]
    public void WithAdditionalConfiguration()
    {
        bootstrapper.ConfigureServices(s => s.TryAddTransient(p => new TestInitialValue { Initial = 42 }));
        Assert.AreEqual(42, Service.DoWork());
    }

    [TestMethod]
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
