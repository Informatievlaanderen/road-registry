namespace RoadRegistry.BackOffice.Api.E2ETests;

using RoadSegments;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

internal static class PublicApiHttpClientExtensions
{
    private sealed record TicketResponse(string Status);

    public static async Task CreateOutlineRoadSegment(this PublicApiHttpClient client, PostRoadSegmentOutlineParameters request, CancellationToken cancellationToken)
    {
        var response = await client.PostAsJsonAsync("v2/wegsegmenten/acties/schetsen", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        //TODO-rik wait for ticket response
        while (true)
        {
            var ticketResponse = await client.GetFromJsonAsync<TicketResponse>(response.Headers.Location, cancellationToken: cancellationToken);

            if (ticketResponse.Status == "completed")
            {
                break;
            }
        }
        
        //TODO-rik return new roadsegmentid from ticket result
        return;
    }
}
