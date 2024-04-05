namespace RoadRegistry.BackOffice.Api.E2ETests;

using GeoJSON.Net.Feature;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.Extracts;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

internal static class BackOfficeApiHttpClientExtensions
{
    public static async Task<DownloadExtractResponseBody> RequestDownloadExtract(this BackOfficeApiHttpClient client, DownloadExtractRequestBody request, CancellationToken cancellationToken)
    {
        return await client.PostAsJsonAsync<DownloadExtractResponseBody>("v1/extracts/downloadrequests", request, cancellationToken);
    }

    public static async Task CloseExtract(this BackOfficeApiHttpClient client, string downloadId, CloseRequestBody request, CancellationToken cancellationToken)
    {
        var response = await client.PutAsJsonAsync($"v1/extracts/{downloadId}/close", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public static async Task<FeatureCollection> GetOverlappingTransactionZonesGeoJson(this BackOfficeApiHttpClient client, CancellationToken cancellationToken)
    {
        var json = await client.GetStringAsync("v1/extracts/overlappingtransactionzones.geojson", cancellationToken);
        return JsonConvert.DeserializeObject<FeatureCollection>(json);
    }
}
