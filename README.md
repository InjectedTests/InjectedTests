# Injected Tests

## [What is Injected Tests?](nuget.readme.md#what-is-injected-tests)

## [Why should I use Injected Tests?](nuget.readme.md#why-should-i-use-injected-tests)

## How do I use Injected Tests?

See the [samples overview][samplesoverview] or these select examples:

* [xUnit.net][xunitsample]
* [MSTest][mstestsample]
* [NUnit][nunitsample]

## Supported Dependency Injection Frameworks

* `Microsoft.Extensions.DependencyInjection.ServiceProvider`
* `Microsoft.Extensions.Hosting.Host`
* `Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory`

And it is easy to add support for your own dependency injection framework (see [Autofac sample][autofacsample]).

## How can I contribute?

There is no process yet in place for contributing but we welcome feedback, suggestions, bug reports and PRs.

[samplesoverview]: samples/README.md
[xunitsample]: samples/InjectedTests.Basic.XUnit/BasicTest.cs
[mstestsample]: samples/InjectedTests.Basic.MSTest/BasicTest.cs
[nunitsample]: samples/InjectedTests.Basic.NUnit/BasicTest.cs
[autofacsample]: samples/InjectedTests.Autofac
