namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure.Attributes;

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Api.Infrastructure.Controllers.Attributes;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.RoadSegments.V1;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using ValidationProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ValidationProblemDetails;

// A parameter marked [JsonProperty(Required = Required.Always)] is enforced by Newtonsoft while the body is bound, so
// a request that leaves one out never reaches the action and never reaches the validation the action runs - which is
// where the endpoints name the missing parameter themselves. Without the translation in ValidationFilterAttribute
// every such request is answered with the generic 'Ongeldig JSON formaat.', leaving the caller to guess which
// parameter is at fault.
//
// These run over a real MVC pipeline with the real parameter types on purpose: both halves the translation leans on -
// the model state key and Newtonsoft's wording of the error - are produced by the framework, not by us, so asserting
// them against a hand-built model state would only restate what the filter does.
public class ValidationFilterAttributeTests
{
    [Fact]
    public async Task GivenParametersAreLeftOut_ThenEachOneIsNamed()
    {
        var problem = await Post("v2/geometriemethode", "[{}]");

        ValidationErrors(problem).Should().Equal(
            ("[0].wegsegmenten", "De parameter 'wegsegmenten' is verplicht."),
            ("[0].geometriemethode", "De parameter 'geometriemethode' is verplicht."));
    }

    [Fact]
    public async Task GivenAParameterIsNull_ThenItIsNamed()
    {
        var problem = await Post("v2/geometriemethode", """[{"wegsegmenten":null,"geometriemethode":"ingemeten"}]""");

        ValidationErrors(problem).Should().Equal(
            ("[0].wegsegmenten", "De parameter 'wegsegmenten' is verplicht."));
    }

    // Every item is reported, not only the first one that is incomplete.
    [Fact]
    public async Task GivenSeveralItemsMissAParameter_ThenEachItemIsNamed()
    {
        var problem = await Post("v2/geometriemethode", """[{"wegsegmenten":[1]},{"geometriemethode":"ingemeten"}]""");

        ValidationErrors(problem).Should().Equal(
            ("[0].geometriemethode", "De parameter 'geometriemethode' is verplicht."),
            ("[1].wegsegmenten", "De parameter 'wegsegmenten' is verplicht."));
    }

    // A parameter nested inside an item of the body is reported at its own path. The side an attribute applies to
    // is as required as the attribute itself, so leaving it out is answered the same way.
    [Fact]
    public async Task GivenANestedParameterIsLeftOut_ThenItIsNamed()
    {
        var problem = await Post("v2/attributen", """[{"wegsegmenten":[1],"straatnaam":[{"identificator":"79632"}]}]""");

        ValidationErrors(problem).Should().Equal(
            ("[0].straatnaam[0].kant", "De parameter 'kant' is verplicht."));
    }

    // A parameter at the root of the body has no path in front of it.
    [Fact]
    public async Task GivenAParameterAtTheRootOfTheBodyIsLeftOut_ThenItIsNamed()
    {
        var problem = await Post("v2/verwijderen", "{}");

        ValidationErrors(problem).Should().Equal(
            ("wegsegmenten", "De parameter 'wegsegmenten' is verplicht."));
    }

    // The v1 endpoints mark their parameters the same way and are answered the same way.
    [Fact]
    public async Task GivenAParameterOfAV1EndpointIsLeftOut_ThenItIsNamed()
    {
        var problem = await Post("v1/verwijderen", "{}");

        ValidationErrors(problem).Should().Equal(
            ("wegsegmenten", "De parameter 'wegsegmenten' is verplicht."));
    }

    // Nothing more specific than 'this is not the JSON the endpoint takes' can be said about a body that is unusable
    // for another reason, so it keeps the generic answer it has always had.
    [Fact]
    public async Task GivenABodyThatIsNotTheExpectedShape_ThenTheGenericFailureIsKept()
    {
        var problem = await Post("v2/verwijderen", """{"wegsegmenten":"geen array"}""");

        ValidationErrors(problem).Should().Equal(
            ("request", "Ongeldig JSON formaat."));
    }

    [Fact]
    public async Task GivenACompleteBody_ThenTheActionRuns()
    {
        using var server = await CreateServer();

        var response = await server.CreateClient().PostAsync(
            "probe/v2/geometriemethode",
            new StringContent("""[{"wegsegmenten":[1],"geometriemethode":"ingemeten"}]""", Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<ValidationProblemDetails> Post(string route, string json)
    {
        using var server = await CreateServer();

        var response = await server.CreateClient().PostAsync(
            $"probe/{route}",
            new StringContent(json, Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        return JsonConvert.DeserializeObject<ValidationProblemDetails>(await response.Content.ReadAsStringAsync());
    }

    private static IEnumerable<(string Parameter, string Reason)> ValidationErrors(ValidationProblemDetails problem)
    {
        return problem.ValidationErrors.SelectMany(x => x.Value.Select(error => (x.Key, error.Reason)));
    }

    private static async Task<TestServer> CreateServer()
    {
        var host = await new HostBuilder()
            .ConfigureWebHost(webHost => webHost
                .UseTestServer()
                .ConfigureServices(services => services
                    .AddControllers(options => options.Filters.Add<ValidationFilterAttribute>())
                    .AddNewtonsoftJson()
                    .AddApplicationPart(typeof(RequiredParameterProbeController).Assembly))
                .Configure(app => app
                    .UseRouting()
                    .UseEndpoints(endpoints => endpoints.MapControllers())))
            .StartAsync();

        return host.GetTestServer();
    }
}

// Stands in for the real endpoints: it binds the very parameter types they bind, so the bodies above travel the same
// model binding the API uses, without needing the API's own dependencies.
[Route("probe")]
public class RequiredParameterProbeController : ControllerBase
{
    [HttpPost("v2/geometriemethode")]
    public IActionResult ChangeGeometryDrawMethodV2([FromBody] ChangeRoadSegmentGeometryDrawMethodV2Parameters parameters) => Ok();

    [HttpPost("v2/attributen")]
    public IActionResult ChangeAttributesV2([FromBody] ChangeRoadSegmentAttributesV2Parameters parameters) => Ok();

    [HttpPost("v2/verwijderen")]
    public IActionResult DeleteRoadSegmentsV2([FromBody] DeleteRoadSegmentsV2Parameters parameters) => Ok();

    [HttpPost("v1/verwijderen")]
    public IActionResult DeleteRoadSegmentsV1([FromBody] DeleteRoadSegmentsParameters parameters) => Ok();
}
