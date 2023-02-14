namespace RoadRegistry.Tests.Infrastructure.Modules;

using Autofac;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
using Hosts.Infrastructure.Modules;

public class BlobClientTestModule : BlobClientModule
{
    private readonly MemoryBlobClient _blobClient = new();

    protected override IBlobClient CreateBlobClient(IComponentContext c, string bucketKey)
    {
        return _blobClient;
    }
}
