namespace RoadRegistry.Sync.OrganizationRegistry;

using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Exceptions;
using RoadRegistry.Extensions;

public interface IOrganizationReader
{
    Task ReadAsync(long startAtChangeId, Func<Organization, Task> handler, CancellationToken cancellationToken);
}

public class OrganizationReader : IOrganizationReader
{
    private readonly IHttpClientFactory _factory;
    private readonly OrganizationConsumerOptions _options;

    public OrganizationReader(IHttpClientFactory factory, OrganizationConsumerOptions options)
    {
        _factory = factory.ThrowIfNull();
        _options = options.ThrowIfNull();
    }

    public async Task ReadAsync(long startAtChangeId, Func<Organization, Task> handler, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.OrganizationRegistrySyncUrl))
        {
            throw new ConfigurationErrorsException($"{nameof(_options.OrganizationRegistrySyncUrl)} is not configured");
        }

        ArgumentNullException.ThrowIfNull(handler);

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync(CreateSyncUri(startAtChangeId), cancellationToken);
        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            throw new OrganizationRegistryTemporarilyUnavailableException();
        }

        var scrollId = string.Empty;
        int? totalItems = null;
        var itemsCount = 0;

        Task HandleOrganization(Organization organization)
        {
            itemsCount++;
            return handler(organization);
        }

        while (totalItems is null || itemsCount < totalItems)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (response.Headers.TryGetValues("x-search-metadata", out var metadataJson))
            {
                var metadata = JsonConvert.DeserializeObject<SearchMetadata>(metadataJson.First());
                if (metadata is not null)
                {
                    scrollId = metadata.ScrollId;
                    totalItems = metadata.TotalItems;
                }

                await InternalReadAsync(response, HandleOrganization, cancellationToken);

                response = await httpClient.GetAsync(CreateScrollUri(scrollId), cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error ({(int)response.StatusCode}) while trying to get organizations");
                }
            }
            else
            {
                await InternalReadAsync(response, HandleOrganization, cancellationToken);
            }
        }
    }

    private Uri CreateSyncUri(long changeId)
    {
        return new Uri(new Uri(_options.OrganizationRegistrySyncUrl), $"/v1/search/organisations?q=changeId:[{changeId} TO *]&fields=name,ovoNumber,kboNumber,keys&sort=changeId,id&scroll=true");
    }

    private Uri CreateScrollUri(string scrollId)
    {
        return new Uri(new Uri(_options.OrganizationRegistrySyncUrl), $"/v1/search/organisations/scroll?id={scrollId}");
    }

    private async Task InternalReadAsync(HttpResponseMessage response, Func<Organization, Task> handler, CancellationToken cancellationToken)
    {
        var results =
            await response.Content.ReadFromJsonAsync<IEnumerable<Organization>?>(
                cancellationToken: cancellationToken);

        if (results is not null)
        {
            foreach (var organization in results)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await handler(organization);
            }
        }
    }
}
