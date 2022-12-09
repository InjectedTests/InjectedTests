using System.Net;
using Microsoft.Extensions.DependencyInjection;

namespace InjectedTests;

public sealed class WebApplicationBootstrapperTest : IAsyncDisposable
{
    #region state

    private readonly WebApplicationBootstrapper<Program> bootstrapper = new();

    private readonly UriBuilder uriBuilder = new()
    {
        Path = "/",
    };

    private readonly Uri differentBaseAddress = new("https://a.b/");

    private HttpStatusCode? statusCode;

    private HttpClient Client => bootstrapper.Client;

    #endregion

    #region lifecycle

    public async ValueTask DisposeAsync()
    {
        await bootstrapper.DisposeAsync();
    }

    #endregion

    [Fact]
    public async Task Client_GetRootPath_CorrectStatusCode()
    {
        await When_Client_GetUri();
        Then_Reponse_HasStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ConfigureServices_GetRootPath_CorrectStatusCode()
    {
        Given_Application_ReturnsStatusCode(HttpStatusCode.NoContent);
        await When_Client_GetUri();
        Then_Reponse_HasStatusCode(HttpStatusCode.NoContent);
    }

    [Fact]
    public void ClientOptions_GetRootPath_CorrectStatusCode()
    {
        Given_Client_DifferentBaseAddress();
        Then_Client_HasDifferentBaseAddress();
    }

    #region given, when, then

    private void Given_Application_ReturnsStatusCode(HttpStatusCode response)
    {
        bootstrapper.ConfigureServices(s => s.Configure<DummyResponseOptions>(o => o.Response = response));
    }

    private void Given_Client_DifferentBaseAddress()
    {
        bootstrapper.ConfigureClient(o => o.BaseAddress = differentBaseAddress);
    }

    private async Task When_Client_GetUri()
    {
        using var response = await Client.GetAsync(uriBuilder.Uri);

        statusCode = response.StatusCode;
    }

    private void Then_Reponse_HasStatusCode(HttpStatusCode expected)
    {
        Assert.Equal(expected, statusCode);
    }

    private void Then_Client_HasDifferentBaseAddress()
    {
        Assert.Equal(differentBaseAddress, Client.BaseAddress);
    }

    #endregion
}
