namespace RoadRegistry.Hosts.Infrastructure.Options;

using System;
using System.Collections.Generic;
using System.Net.Http;

public abstract class HealthCheckEndpointOptionsBuilder<TOptions> : HealthCheckOptionsBuilder<TOptions>
{
    private readonly Dictionary<Uri, Action<IHttpClientFactory, Uri>> _endpoints = new();

    public IHealthCheckOptionsBuilder CheckEndpoint(Uri endpoint, Action<IHttpClientFactory, Uri> handler)
    {
        _endpoints.Add(endpoint, handler);
        return this;
    }
}
