namespace RoadRegistry.BackOffice.Api.E2ETests;

using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

internal static class HttpClientExtensions
{
    public static async Task<T?> PostAsJsonAsync<T>(this HttpClient client, string requestUri, object value, CancellationToken cancellationToken)
    {
        var response = await client.PostAsJsonAsync(requestUri, value, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<T>(json);
    }

    public static async Task<T?> PutAsJsonAsync<T>(this HttpClient client, string requestUri, object value, CancellationToken cancellationToken)
    {
        var response = await client.PutAsJsonAsync(requestUri, value, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<T>(json);
    }
}
