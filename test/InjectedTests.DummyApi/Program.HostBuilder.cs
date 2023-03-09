using InjectedTests;
using Microsoft.Extensions.Options;

internal sealed partial class Program
{
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return new HostBuilder()
            .ConfigureDefaults(args)
            .ConfigureWebHostDefaults(Configure);
    }

    private static void Configure(IWebHostBuilder builder)
    {
        builder
            .ConfigureServices(Configure)
            .Configure(Configure);
    }

    private static void Configure(IServiceCollection services)
    {
        services.AddRouting();
        services.AddOptions<DummyResponseOptions>();
    }

    private static void Configure(IApplicationBuilder builder)
    {
        builder.UseRouting();
        builder.UseEndpoints(Configure);
    }

    private static void Configure(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", Handle);
    }

    private static Task Handle(HttpContext context)
    {
        var options = context
            .RequestServices
            .GetRequiredService<IOptionsSnapshot<DummyResponseOptions>>()
            .Value;

        context.Response.StatusCode = (int)options.Response;

        return Task.CompletedTask;
    }
}
