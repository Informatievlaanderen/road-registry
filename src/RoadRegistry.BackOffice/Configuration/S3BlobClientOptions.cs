namespace RoadRegistry.BackOffice.Configuration;

using System;
using System.Collections.Generic;
using RoadRegistry.BackOffice;

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

    public string FindBucketName(string key)
    {
        if (Buckets is not null && Buckets.TryGetValue(key, out var name))
        {
            return name;
        }

        return null;
    }
}
