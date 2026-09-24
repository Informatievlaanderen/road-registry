namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure;

using System.Net;
using ProblemDetailsOptions = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetailsOptions;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using RoadRegistry.BackOffice.Api.Infrastructure;
using Be.Vlaanderen.Basisregisters.BasicApiProblem;

// A road segment whose inwinning is complete is answered with a 404 and a Link header pointing at v3
// (RoadSegmentsController.GetRoadSegment); public-api reads that header off the backend response and passes it on. The
// problem details middleware rebuilds the response it turns into a problem and keeps only the headers it is told to
// keep, so without RoadRegistryProblemDetails.Configure the header never reaches the caller.
public class ProblemDetailsLinkHeaderTests
{
    private const string LinkHeaderValue = "<https://example.com/v3/wegsegmenten/30>; rel=\"successor-version\"";

    [Fact]
    public async Task GivenTheProblemDetailsConfiguration_ThenTheLinkHeaderReachesTheCaller()
    {
        using var server = await CreateServer(RoadRegistryProblemDetails.Configure);

        var response = await server.CreateClient().GetAsync("/not-found-with-link");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        response.Headers.GetValues(HeaderNames.Link).Should().Equal(LinkHeaderValue);
    }

    // What the configuration is there for: the middleware drops the header without it.
    [Fact]
    public async Task GivenNoProblemDetailsConfiguration_ThenTheLinkHeaderIsDropped()
    {
        using var server = await CreateServer(_ => { });

        var response = await server.CreateClient().GetAsync("/not-found-with-link");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Headers.Contains(HeaderNames.Link).Should().BeFalse();
    }

    private static async Task<TestServer> CreateServer(Action<ProblemDetailsOptions> configureProblemDetails)
    {
        var host = await new HostBuilder()
            .ConfigureWebHost(webHost => webHost
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    // The middleware writes its response through MVC's result executor.
                    services.AddControllers();
                    services.AddProblemDetails(configureProblemDetails);
                })
                .Configure(app =>
                {
                    app.UseProblemDetails();
                    app.Run(context =>
                    {
                        context.Response.Headers[HeaderNames.Link] = LinkHeaderValue;
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        return Task.CompletedTask;
                    });
                }))
            .StartAsync();

        return host.GetTestServer();
    }
}
