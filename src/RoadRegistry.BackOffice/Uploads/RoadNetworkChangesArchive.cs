namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.IO.Compression;
using System.Linq;
using Framework;
using Messages;

public class RoadNetworkChangesArchive : EventSourcedEntity
{
    private RoadNetworkChangesArchive()
    {
        On<RoadNetworkChangesArchiveUploaded>(e => { Id = new ArchiveId(e.ArchiveId); });
    }

    public static readonly Func<RoadNetworkChangesArchive> Factory = () => new RoadNetworkChangesArchive();

    public ArchiveId Id { get; private set; }

    public static RoadNetworkChangesArchive Upload(ArchiveId id)
    {
        var instance = new RoadNetworkChangesArchive();
        instance.Apply(new RoadNetworkChangesArchiveUploaded
        {
            ArchiveId = id
        });
        return instance;
    }

    public ZipArchiveProblems ValidateArchiveUsing(ZipArchive archive, IZipArchiveValidator validator)
    {
        var problems = validator.Validate(archive, ZipArchiveMetadata.Empty);
        if (!problems.OfType<FileError>().Any())
            Apply(
                new RoadNetworkChangesArchiveAccepted
                {
                    ArchiveId = Id,
                    Problems = problems.Select(problem => problem.Translate()).ToArray()
                });
        else
            Apply(
                new RoadNetworkChangesArchiveRejected
                {
                    ArchiveId = Id,
                    Problems = problems.Select(problem => problem.Translate()).ToArray()
                });
        return problems;
    }
}