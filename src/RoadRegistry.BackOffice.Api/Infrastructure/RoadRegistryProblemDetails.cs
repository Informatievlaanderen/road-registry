namespace RoadRegistry.BackOffice.Api.Infrastructure;

using Be.Vlaanderen.Basisregisters.BasicApiProblem;
using Microsoft.Net.Http.Headers;

public static class RoadRegistryProblemDetails
{
    // The problem details middleware rebuilds the response it turns into a problem and keeps only the headers named
    // here; everything else is dropped. Link is how a road segment whose inwinning is complete points at where it lives
    // now (RoadSegmentsController.GetRoadSegment answers 404 with it, and public-api passes it on to the caller), so
    // without this the caller is left with a plain 404.
    public static void Configure(ProblemDetailsOptions options)
    {
        options.AllowedHeaderNames.Add(HeaderNames.Link);
    }
}
