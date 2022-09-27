namespace RoadRegistry.BackOffice.Core;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.BlobStore;
using SqlStreamStore.Streams;

public static class MetadataExtensions
{
    public static Metadata AtVersion(this Metadata metadata, int version)
    {
        return metadata.Add(new KeyValuePair<MetadataKey, string>(RoadNetworkSnapshotReaderWriter.AtVersionKey, version.ToString()));
    }

    public static bool TryGetAtVersion(this Metadata metadata, out int version)
    {
        var found = metadata.Where(metadatum => metadatum.Key == RoadNetworkSnapshotReaderWriter.AtVersionKey).ToArray();
        if (found.Length == 1)
        {
            version = int.Parse(found[0].Value);
            return true;
        }

        version = ExpectedVersion.NoStream;
        return false;
    }
}
