﻿using Microsoft.Extensions.DependencyInjection;
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
        Assert.That(Service.DoWork(), Is.EqualTo(0));
    }

    [Test]
    public void WithAdditionalConfiguration()
    {
        bootstrapper.ConfigureServices(s => s.TryAddTransient(p => new TestInitialValue { Initial = 42 }));
        Assert.That(Service.DoWork(), Is.EqualTo(42));
    }

    [Test]
    public void WithInitialization()
    {
        bootstrapper.ConfigureInitializer(b => b.With<TestService>(s => s.DoWork()));
        Assert.That(Service.DoWork(), Is.EqualTo(1));
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
