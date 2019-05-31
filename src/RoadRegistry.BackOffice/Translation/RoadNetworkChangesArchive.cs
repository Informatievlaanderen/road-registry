namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.IO.Compression;
    using System.Linq;
    using Framework;
    using Messages;

    public class RoadNetworkChangesArchive : EventSourcedEntity
    {
        public static readonly Func<RoadNetworkChangesArchive> Factory = () => new RoadNetworkChangesArchive();

        private RoadNetworkChangesArchive()
        {
            On<RoadNetworkChangesArchiveUploaded>(e => Id = new ArchiveId(e.ArchiveId));
        }

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

        public void ValidateArchiveUsing(ZipArchive archive, IZipArchiveValidator validator)
        {
            var problems = validator.Validate(archive);
            if (!problems.OfType<FileError>().Any())
            {
                Apply(
                    new RoadNetworkChangesArchiveAccepted
                    {
                        ArchiveId = Id,
                        Problems = problems.Select(problem => problem.Translate()).ToArray()
                    });
            }
            else
            {
                Apply(
                    new RoadNetworkChangesArchiveRejected
                    {
                        ArchiveId = Id,
                        Problems = problems.Select(problem => problem.Translate()).ToArray()
                    });
            }
        }
    }
}
