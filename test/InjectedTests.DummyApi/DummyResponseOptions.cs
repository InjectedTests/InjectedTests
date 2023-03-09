using System.Net;

namespace InjectedTests;

public class DummyResponseOptions
{
    public HttpStatusCode Response { get; set; } = HttpStatusCode.OK;
}
