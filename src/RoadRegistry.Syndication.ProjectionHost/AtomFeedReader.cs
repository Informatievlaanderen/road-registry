namespace RoadRegistry.Syndication.ProjectionHost;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;

public interface IRegistryAtomFeedReader
{
    /// <summary>
    ///     Reads the entries of an Atom feed at the provided feedUrl.
    /// </summary>
    /// <returns>A list of <see cref="IAtomEntry " />.</returns>
    Task<IEnumerable<IAtomEntry>> ReadEntriesAsync(Uri feedUrl, long? after, bool embedEvent = true, bool embedObject = true);
}

public class RegistryAtomFeedReader : IRegistryAtomFeedReader
{
    public RegistryAtomFeedReader(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<RegistryAtomFeedReader>();
        _httpClient = httpClientFactory.CreateClient(HttpClientName);
    }

    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    public const string HttpClientName = "registryFeedClient";

    public async Task<IEnumerable<IAtomEntry>> ReadEntriesAsync(
        Uri feedUrl,
        long? after,
        bool embedEvent = true,
        bool embedObject = true)
    {
        var entries = new List<IAtomEntry>();

        var embedString = string.Empty;
        if (embedObject && embedEvent)
            embedString = "embed: \"event,object\"";
        else if (embedObject)
            embedString = "embed: \"object\"";
        else if (embedEvent)
            embedString = "embed: \"event\"";

        _httpClient.DefaultRequestHeaders.Remove("X-Filtering");
        if (after.HasValue)
        {
            var from = after + 1;
            var filter = string.IsNullOrEmpty(embedString)
                ? $"{{ position: {from} }}"
                : $"{{ position: {from}, {embedString} }}";
            _httpClient.DefaultRequestHeaders.Add("X-Filtering", filter);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Add("X-Filtering", $"{{ {embedString} }}");
        }

        _logger.LogInformation("Performing HTTP request GET {FeedUrl} with headers: {@Params}", feedUrl,
            _httpClient.DefaultRequestHeaders.ToDictionary(x => x.Key, x => x.Value));
        using (var response = await _httpClient.GetAsync(feedUrl))
        await using (var responseStream = await response.Content.ReadAsStreamAsync())
        using (var xmlReader = XmlReader.Create(responseStream, new XmlReaderSettings { Async = true }))
        {
            var atomReader = new AtomFeedReader(xmlReader);
            while (await atomReader.Read())
                if (atomReader.ElementType == SyndicationElementType.Item)
                    entries.Add(await atomReader.ReadEntry());
        }

        return entries;
    }
}
