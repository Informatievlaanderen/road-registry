namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure;

using System.Collections.Generic;
using System.Linq;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoadRegistry.BackOffice.Api.IntegrationTests.Authentication;

// AcmIdmTests call every endpoint of the API with and without the scopes it requires, but they need SQL Server and real
// ACM/IDM tokens, so they only run on schedule - and a new endpoint missing from their list went unnoticed until then.
// Which endpoints there are only takes the API's services, not a running API: the host is built, never started, so
// Startup.Configure - which checks the databases - does not run.
public class AcmIdmEndpointsTests
{
    [Fact]
    public void EveryEndpointIsInTheAcmIdmTests()
    {
        using var host = new WebHostBuilder()
            .ConfigureServices(services => services.AddAutofac())
            .UseConfiguration(new ConfigurationBuilder().AddInMemoryCollection(Configuration).Build())
            .UseStartup<RoadRegistry.BackOffice.Api.Infrastructure.Startup>()
            .UseTestServer()
            .Build();

        var apiDescriptions = host.Services
            .GetRequiredService<IApiDescriptionGroupCollectionProvider>()
            .ApiDescriptionGroups.Items
            .SelectMany(group => group.Items)
            .ToList();
        Assert.NotEmpty(apiDescriptions);

        var missingEndpoints = apiDescriptions
            .Where(apiDescription => !AcmIdmEndpoints.All.Any(endpoint =>
                endpoint.Method.ToString().Equals(apiDescription.HttpMethod, StringComparison.InvariantCultureIgnoreCase)
                && endpoint.Uri.Equals(apiDescription.RelativePath, StringComparison.InvariantCultureIgnoreCase)))
            .Select(apiDescription => $"{apiDescription.HttpMethod} {apiDescription.RelativePath}")
            .ToList();

        Assert.True(
            missingEndpoints.Count == 0,
            $"Endpoints missing from AcmIdmEndpoints (test/RoadRegistry.BackOffice.Api.IntegrationTests/Authentication):{Environment.NewLine}{string.Join(Environment.NewLine, missingEndpoints)}");
    }

    // What the API reads while its services are registered; nothing connects to any of it.
    private static readonly Dictionary<string, string> Configuration = new()
    {
        ["ConnectionStrings:EditorProjections"] = "Data Source=localhost;Initial Catalog=road-registry",
        ["ConnectionStrings:ProductProjections"] = "Data Source=localhost;Initial Catalog=road-registry",
        ["ConnectionStrings:Events"] = "Data Source=localhost;Initial Catalog=road-registry",
        ["ConnectionStrings:StreetNameProjections"] = "Data Source=localhost;Initial Catalog=road-registry",
        ["OAuth2IntrospectionOptions:ClientId"] = "clientid",
        ["OAuth2IntrospectionOptions:ClientSecret"] = "clientsecret",
        ["OAuth2IntrospectionOptions:Authority"] = "https://authenticatie-ti.vlaanderen.be/op/v1/auth",
        ["OAuth2IntrospectionOptions:IntrospectionEndpoint"] = "https://authenticatie-ti.vlaanderen.be/op/v1/introspect",
        ["BaseUrl"] = "https://api.staging-basisregisters.vlaanderen"
    };
}
