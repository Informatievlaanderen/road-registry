namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure;

using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using RoadRegistry.BackOffice.Api.IntegrationTests.Authentication;

// AcmIdmTests call every endpoint of the API with and without the scopes it requires, but they need SQL Server and real
// ACM/IDM tokens, so they only run on schedule - and a new endpoint missing from their list went unnoticed until then.
// Which endpoints there are only takes the API's services, composed the way SwaggerTests compose them, and in the same
// non-parallel collection, for the same reason.
[Collection("Swagger")]
public class AcmIdmEndpointsTests
{
    [Fact]
    public void EveryEndpointIsInTheAcmIdmTests()
    {
        var apiDescriptions = SwaggerTests.BuildApiServiceProvider()
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
}
