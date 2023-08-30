namespace RoadRegistry.BackOffice.Messages;

using System;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public interface IRoadRegistryIdentity
{
    public RoadRegistryIdentity IdentityData { get; set; }
}

public sealed record RoadRegistryIdentity
{
    public IDictionary<string, object?> Metadata { get; init; }
    public ProvenanceData ProvenanceData { get; init; }
}

public abstract class RoadRegistryCommandIdentity : IRoadRegistryIdentity
{
    public RoadRegistryIdentity IdentityData { get; set; }
}

public class UploadRoadNetworkExtractChangesArchive : RoadRegistryCommandIdentity
{
    public string ArchiveId { get; set; }
    public Guid DownloadId { get; set; }
    public string RequestId { get; set; }
    public Guid UploadId { get; set; }
    public bool FeatureCompareCompleted { get; set; }
    public bool UseZipArchiveFeatureCompareTranslator { get; set; }
}
