namespace RoadRegistry.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Extensions;
using Extracts;
using Extracts.Dbase;
using FeatureCompare;
using Framework;
using Messages;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class RoadNetworkChangesArchive : EventSourcedEntity
{
    public static readonly Func<RoadNetworkChangesArchive> Factory = () => new RoadNetworkChangesArchive();

    private RoadNetworkChangesArchive()
    {
        On<RoadNetworkChangesArchiveUploaded>(e =>
        {
            Id = new ArchiveId(e.ArchiveId);
            Description = !string.IsNullOrEmpty(e.Description)
                ? new ExtractDescription(e.Description)
                : new ExtractDescription();
        });
    }

    public ArchiveId Id { get; private set; }
    public ExtractDescription Description { get; private set; }

    public static RoadNetworkChangesArchive Upload(ArchiveId id, Stream readStream)
    {
        var extractDescription = ReadExtractDescriptionFromArchive(readStream);

        return Upload(id, extractDescription);
    }

    public static RoadNetworkChangesArchive Upload(ArchiveId id, ExtractDescription extractDescription)
    {
        var instance = new RoadNetworkChangesArchive();

        instance.Apply(new RoadNetworkChangesArchiveUploaded
        {
            ArchiveId = id,
            Description = extractDescription
        });

        return instance;
    }

    public async Task<ZipArchiveProblems> ValidateArchiveUsing(ZipArchive archive, Guid? ticketId, IZipArchiveValidator validator, CancellationToken cancellationToken)
    {
        var problems = await validator.ValidateAsync(archive, new ZipArchiveValidatorContext(ZipArchiveMetadata.Empty), cancellationToken);
        if (!problems.HasError())
        {
            Apply(
                new RoadNetworkChangesArchiveAccepted
                {
                    ArchiveId = Id,
                    Description = Description,
                    Problems = problems.Select(problem => problem.Translate()).ToArray(),
                    TicketId = ticketId
                });
        }
        else
        {
            Apply(
                new RoadNetworkChangesArchiveRejected
                {
                    ArchiveId = Id,
                    Description = Description,
                    Problems = problems.Select(problem => problem.Translate()).ToArray(),
                    TicketId = ticketId
                });
        }
        return problems;
    }

    private static ExtractDescription ReadExtractDescriptionFromArchive(Stream readStream)
    {
        using (var sourceStream = new MemoryStream())
        {
            readStream.CopyTo(sourceStream);

            using (var archive = new ZipArchive(sourceStream, ZipArchiveMode.Read, true))
            {
                var transactionZoneFileEntry = archive.FindEntry(FeatureType.Change.ToDbaseFileName(ExtractFileName.Transactiezones));
                if (transactionZoneFileEntry is not null)
                {
                    using var reader = new BinaryReader(transactionZoneFileEntry.Open(), Encoding.UTF8);

                    var header = DbaseFileHeader.Read(reader, new DbaseFileHeaderReadBehavior(true));

                    using var records = header.CreateDbaseRecordEnumerator<TransactionZoneDbaseRecord>(reader);
                    if (records.MoveNext())
                    {
                        return !string.IsNullOrEmpty(records.Current!.BESCHRIJV.Value)
                            ? new ExtractDescription(records.Current.BESCHRIJV.Value)
                            : new ExtractDescription();
                    }
                }
            }
        }

        return new ExtractDescription();
    }
}
