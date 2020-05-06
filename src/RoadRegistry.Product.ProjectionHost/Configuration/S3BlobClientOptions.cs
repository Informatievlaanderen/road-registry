namespace RoadRegistry.Product.ProjectionHost.Configuration
{
    using System;
    using System.Collections.Generic;

    public class S3BlobClientOptions
    {
        private IDictionary<string, string> _buckets;

        public IDictionary<string, string> Buckets
        {
            get => _buckets;
            set => _buckets = new Dictionary<string, string>(value, StringComparer.OrdinalIgnoreCase);
        }
    }
}
