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

        private ArchiveId _id;

        private RoadNetworkChangesArchive()
        {
            On<RoadNetworkChangesArchiveUploaded>(e => _id = new ArchiveId(e.ArchiveId));
        }

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
            var errors = validator.Validate(archive);
            if (errors.Count == 0)
            {
                Apply(
                    new RoadNetworkChangesArchiveAccepted
                    {
                        ArchiveId = _id,
                        Warnings = new Messages.Problem[0]
                    });
            }
            else
            {
                Apply(
                    new RoadNetworkChangesArchiveRejected
                    {
                        ArchiveId = _id,
                        Errors = errors.Select(error => error.Translate()).ToArray(),
                        Warnings = new Messages.Problem[0]
                    });
            }
        }
    }
}