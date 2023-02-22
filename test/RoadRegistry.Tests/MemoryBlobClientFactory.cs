namespace RoadRegistry.Tests;

using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
using RoadRegistry.BackOffice;

public class MemoryBlobClientFactory : IBlobClientFactory
{
    private readonly MemoryBlobClient _blobClient = new();

    public IBlobClient Create(string bucketKey)
    {
        return _blobClient;
    }
}
