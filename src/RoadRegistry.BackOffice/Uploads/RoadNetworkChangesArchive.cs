namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Framework;
using Messages;
using NetTopologySuite.IO;
using Schema.V2;

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
        ExtractDescription extractDescription = new();

        using (var sourceStream = new MemoryStream())
        {
            readStream.CopyTo(sourceStream);

            using (var archive = new ZipArchive(sourceStream, ZipArchiveMode.Read, true))
            {
                var transactionzoneFileEntry = archive.Entries.SingleOrDefault(e => e.Name.ToLowerInvariant() == "transactiezones.dbf");
                if (transactionzoneFileEntry is not null)
                {
                    using var reader = new BinaryReader(transactionzoneFileEntry.Open(), Encoding.UTF8);
                    Be.Vlaanderen.Basisregisters.Shaperon.DbaseFileHeader header = null;

                    try
                    {
                        header = Be.Vlaanderen.Basisregisters.Shaperon.DbaseFileHeader.Read(reader, new DbaseFileHeaderReadBehavior(true));
                    }
                    catch (Exception)
                    {
                    }

                    if (header is not null)
                    {
                        using var records = header.CreateDbaseRecordEnumerator<TransactionZoneDbaseRecord>(reader);
                        while (records.MoveNext())
                        {
                            extractDescription = !string.IsNullOrEmpty(records.Current.BESCHRIJV.Value)
                                ? new ExtractDescription(records.Current.BESCHRIJV.Value)
                                : new ExtractDescription();
                        }
                    }
                }
            }

            sourceStream.Position = 0;
            readStream = sourceStream;
        }

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

    public ZipArchiveProblems ValidateArchiveUsing(ZipArchive archive, IZipArchiveValidator validator)
    {
        var problems = validator.Validate(archive, ZipArchiveMetadata.Empty);
        if (!problems.OfType<FileError>().Any())
            Apply(
                new RoadNetworkChangesArchiveAccepted
                {
                    ArchiveId = Id,
                    Description = Description,
                    Problems = problems.Select(problem => problem.Translate()).ToArray()
                });
        else
            Apply(
                new RoadNetworkChangesArchiveRejected
                {
                    ArchiveId = Id,
                    Description = Description,
                    Problems = problems.Select(problem => problem.Translate()).ToArray()
                });
        return problems;
    }
}
