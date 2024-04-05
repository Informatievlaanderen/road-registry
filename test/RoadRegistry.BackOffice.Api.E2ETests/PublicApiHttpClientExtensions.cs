namespace RoadRegistry.BackOffice.Api.E2ETests;

using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using RoadSegments;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;

internal static class PublicApiHttpClientExtensions
{
    public static async Task<RoadSegmentId> CreateOutlineRoadSegment(this PublicApiHttpClient client, PostRoadSegmentOutlineParameters request, CancellationToken cancellationToken)
    {
        var response = await client.PostAsJsonAsync("v2/wegsegmenten/acties/schetsen", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var ticketUrl = response.Headers.Location;

        var sw = Stopwatch.StartNew();

        while (true)
        {
            if (sw.Elapsed.TotalMinutes > 5)
            {
                throw new TimeoutException($"Timed out at [{sw.Elapsed}] while waiting for outlined roadsegment to be created");
            }

            var ticketResponse = await client.GetFromJsonAsync<TicketResponse>(ticketUrl, cancellationToken: cancellationToken);

            switch (ticketResponse?.Status)
            {
                case "complete":
                    var locationResult = ticketResponse.GetResult<LocationResult>();
                    return new RoadSegmentId(int.Parse(locationResult.Location.ToString().Split('/').Last()));
                case "error":
                    throw new Exception($"Failed trying to create an outlined roadsegment: {ticketUrl}");
            }

            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}
