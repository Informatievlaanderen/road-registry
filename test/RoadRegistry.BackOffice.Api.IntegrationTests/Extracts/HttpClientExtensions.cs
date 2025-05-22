namespace RoadRegistry.BackOffice.Api.IntegrationTests.Extracts;

using Api.Extracts;
using GeoJSON.Net.Feature;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts;

internal static class HttpClientExtensions
{
    public static async Task<DownloadExtractResponseBody> RequestDownloadExtract(this HttpClient client, DownloadExtractRequestBody request, CancellationToken cancellationToken)
    {
        var response = await client.PostAsJsonAsync<DownloadExtractResponseBody>("v1/extracts/downloadrequests", request, cancellationToken);
        return response!;
    }

    public static async Task CloseExtract(this HttpClient client, string downloadId, CloseRequestBody request, CancellationToken cancellationToken)
    {
        var response = await client.PutAsJsonAsync($"v1/extracts/{downloadId}/close", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public static async Task<FeatureCollection> GetOverlappingTransactionZonesGeoJson(this HttpClient client, CancellationToken cancellationToken)
    {
        var json = await client.GetStringAsync("v1/extracts/overlappingtransactionzones.geojson", cancellationToken);
        return JsonConvert.DeserializeObject<FeatureCollection>(json)!;
    }

    private static async Task<T?> PostAsJsonAsync<T>(this HttpClient client, string requestUri, object value, CancellationToken cancellationToken)
    {
        var response = await client.PostAsJsonAsync(requestUri, value, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<T>(json);
    }
    
    private static async Task<T?> PutAsJsonAsync<T>(this HttpClient client, string requestUri, object value, CancellationToken cancellationToken)
    {
        var response = await client.PutAsJsonAsync(requestUri, value, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<T>(json);
    }
}
