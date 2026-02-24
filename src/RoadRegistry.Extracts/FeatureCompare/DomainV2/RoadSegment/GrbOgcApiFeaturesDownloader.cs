namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using System.Web;
using Extensions;
using GeoJSON.Net;
using GeoJSON.Net.Converters;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

public interface IGrbOgcApiFeaturesDownloader
{
    Task<IReadOnlyList<OgcFeature>> DownloadFeaturesAsync(
        IEnumerable<string> collectionIds,
        Envelope boundingBox,
        int srid,
        CancellationToken cancellationToken);
}

public sealed class GrbOgcApiFeaturesDownloader : IGrbOgcApiFeaturesDownloader
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly int _pageSize;

    private static readonly JsonSerializerSettings JsonOptions = new()
    {
        Converters = { new GeoJsonConverter() },
        NullValueHandling = NullValueHandling.Ignore
    };

    public GrbOgcApiFeaturesDownloader(HttpClient httpClient, string baseUrl, int pageSize = 1000)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUrl = baseUrl.TrimEnd('/');

        if (pageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

        _pageSize = pageSize;
    }

    public async Task<IReadOnlyList<OgcFeature>> DownloadFeaturesAsync(
        IEnumerable<string> collectionIds,
        Envelope boundingBox,
        int srid,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(collectionIds);
        ArgumentNullException.ThrowIfNull(boundingBox);

        var allFeatures = await Task.WhenAll(collectionIds
            .Select(async collectionId =>
            {
                var features = new List<OgcFeature>();
                await foreach (var feature in DownloadCollectionFeaturesAsync(collectionId, boundingBox, srid, cancellationToken))
                {
                    features.Add(feature);
                }

                return features;
            }));

        return allFeatures.SelectMany(x => x).ToList();
    }

    private async IAsyncEnumerable<OgcFeature> DownloadCollectionFeaturesAsync(
        string collectionId,
        Envelope boundingBox,
        int srid,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionId);
        ArgumentNullException.ThrowIfNull(boundingBox);

        string? nextUrl = BuildItemsUrl(collectionId, boundingBox, srid, offset: 0);

        while (nextUrl is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = await _httpClient.GetAsync(nextUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var collection = JsonConvert.DeserializeObject<OgcJsonFeatureCollection>(json, JsonOptions)
                             ?? throw new InvalidOperationException($"Failed to deserialize OGC feature collection response from: {nextUrl}");

            foreach (var feature in collection.Features
                         .Where(x => x.Type == "Feature" && x.Geometry is not null))
                yield return new OgcFeature(collectionId, feature.Id, feature.Geometry.ToNtsGeometry()!.WithSrid(srid), feature.Properties);

            nextUrl = GetNextLink(collection);

            if (nextUrl is null && collection.NumberReturned == _pageSize)
            {
                nextUrl = AdvanceOffsetUrl(BuildItemsUrl(collectionId, boundingBox, srid, offset: 0), collection);
            }
        }
    }

    private string BuildItemsUrl(string collectionId, Envelope boundingBox, int srid, int offset) =>
        $"{_baseUrl}/collections/{Uri.EscapeDataString(collectionId)}/items" +
        $"?f=application%2Fjson" +
        $"&bbox={boundingBox.MinX:0},{boundingBox.MinY:0},{boundingBox.MaxX:0},{boundingBox.MaxY:0}" +
        $"&bbox-crs=EPSG:{srid}" +
        $"&limit={_pageSize}" +
        $"&crs=EPSG:{srid}" +
        $"&properties=" +
        $"&offset={offset}";

    private static string? GetNextLink(OgcJsonFeatureCollection collection)
    {
        foreach (var link in collection.Links)
        {
            if (string.Equals(link.Rel, "next", StringComparison.OrdinalIgnoreCase))
                return link.Href;
        }

        return null;
    }

    private static string AdvanceOffsetUrl(string firstPageUrl, OgcJsonFeatureCollection collection)
    {
        var uri = new Uri(firstPageUrl);
        var query = HttpUtility.ParseQueryString(uri.Query);

        var currentOffset = int.TryParse(query["offset"], out var o) ? o : 0;
        query["offset"] = (currentOffset + collection.NumberReturned).ToString();

        return new UriBuilder(uri) { Query = query.ToString() }.ToString();
    }

    private sealed class OgcJsonFeature
    {
        [JsonProperty("type")] public string Type { get; init; } = string.Empty;
        [JsonProperty("id")] public string? Id { get; init; }
        [JsonProperty("geometry")] public GeoJSONObject? Geometry { get; init; }
        [JsonProperty("properties")] public Dictionary<string, object>? Properties { get; init; }
    }

    private sealed class OgcJsonFeatureCollection
    {
        [JsonProperty("numberReturned")] public int NumberReturned { get; init; }
        [JsonProperty("features")] public List<OgcJsonFeature> Features { get; init; } = new();
        [JsonProperty("links")] public List<OgcJsonLink> Links { get; init; } = new();
    }

    private sealed class OgcJsonLink
    {
        [JsonProperty("rel")] public string Rel { get; init; } = string.Empty;
        [JsonProperty("href")] public string Href { get; init; } = string.Empty;
    }
}

public sealed class OgcFeature
{
    public string CollectionId { get; }
    public string? Id { get;  }
    public Geometry Geometry { get; }
    public Dictionary<string, object>? Properties { get; }

    public OgcFeature(string collectionid, string? id, Geometry geometry, Dictionary<string, object>? properties)
    {
        CollectionId = collectionid;
        Id = id;
        Geometry = geometry;
        Properties = properties;
    }
}
