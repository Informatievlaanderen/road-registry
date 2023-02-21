namespace RoadRegistry.Hosts.Configuration;

using System;
using System.Collections.Generic;
using BackOffice;

public class S3BlobClientOptions: IHasConfigurationKey
{
    private IDictionary<string, string> _buckets;

    public IDictionary<string, string> Buckets
    {
        get => _buckets;
        set => _buckets = new Dictionary<string, string>(value, StringComparer.OrdinalIgnoreCase);
    }

    public string GetConfigurationKey()
    {
        return "S3BlobClientOptions";
    }
}
