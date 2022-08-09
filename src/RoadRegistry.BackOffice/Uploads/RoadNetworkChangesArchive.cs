namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.IO.Compression;
using System.Linq;
using Framework;
using Messages;

public class RoadNetworkChangesArchive : EventSourcedEntity
{
    public static readonly Func<RoadNetworkChangesArchive> Factory = () => new RoadNetworkChangesArchive();
    private bool _isFeatureCompare;

    private RoadNetworkChangesArchive()
    {
        On<RoadNetworkChangesArchiveUploaded>(e =>
        {
            Id = new ArchiveId(e.ArchiveId);
            _isFeatureCompare = e.IsFeatureCompare;
        });
    }

    public ArchiveId Id { get; private set; }

    public static RoadNetworkChangesArchive Upload(ArchiveId id, bool isFeatureCompare)
    {
        var instance = new RoadNetworkChangesArchive();
        instance.Apply(new RoadNetworkChangesArchiveUploaded
        {
            ArchiveId = id,
            IsFeatureCompare = isFeatureCompare
        });
        return instance;
    }

    public void ValidateArchiveUsing(ZipArchive archive, IZipArchiveValidator validator)
    {
        var problems = validator.Validate(archive, ZipArchiveMetadata.Empty);
        if (!problems.OfType<FileError>().Any())
            Apply(
                new RoadNetworkChangesArchiveAccepted
                {
                    ArchiveId = Id,
                    IsFeatureCompare = _isFeatureCompare,
                    Problems = problems.Select(problem => problem.Translate()).ToArray()
                });
        else
            Apply(
                new RoadNetworkChangesArchiveRejected
                {
                    ArchiveId = Id,
                    IsFeatureCompare = _isFeatureCompare,
                    Problems = problems.Select(problem => problem.Translate()).ToArray()
                });
    }
}
