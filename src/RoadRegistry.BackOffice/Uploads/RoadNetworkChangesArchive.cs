namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Dbase.AfterFeatureCompare.V2.Schema;
using Extensions;
using Extracts;
using FeatureCompare;
using Framework;
using Messages;

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
            FeatureCompareCompleted = false;
        });
        On<RoadNetworkChangesArchiveFeatureCompareCompleted>(e =>
        {
            Id = new ArchiveId(e.ArchiveId);
            Description = !string.IsNullOrEmpty(e.Description)
                ? new ExtractDescription(e.Description)
                : new ExtractDescription();
            FeatureCompareCompleted = true;
        });
    }

    public ArchiveId Id { get; private set; }
    public ExtractDescription Description { get; private set; }
    public bool FeatureCompareCompleted { get; private set; }

    public static RoadNetworkChangesArchive Upload(ArchiveId id, Stream readStream, bool featureCompareCompleted = false)
    {
        var extractDescription = ReadExtractDescriptionFromArchive(readStream);

        return Upload(id, extractDescription, featureCompareCompleted);
    }

    public static RoadNetworkChangesArchive Upload(ArchiveId id, ExtractDescription extractDescription, bool featureCompareCompleted = false)
    {
        var instance = new RoadNetworkChangesArchive();

        if (featureCompareCompleted)
        {
            instance.Apply(new RoadNetworkChangesArchiveFeatureCompareCompleted
            {
                ArchiveId = id,
                Description = extractDescription
            });
        }
        else
        {
            instance.Apply(new RoadNetworkChangesArchiveUploaded
            {
                ArchiveId = id,
                Description = extractDescription
            });
        }

        return instance;
    }

    public async Task<ZipArchiveProblems> ValidateArchiveUsing(ZipArchive archive, IZipArchiveValidator validator, bool useZipArchiveFeatureCompareTranslator, CancellationToken cancellationToken)
    {
        var problems = await validator.ValidateAsync(archive, new ZipArchiveValidatorContext(ZipArchiveMetadata.Empty), cancellationToken);
        if (!problems.OfType<FileError>().Any())
            Apply(
                new RoadNetworkChangesArchiveAccepted
                {
                    ArchiveId = Id,
                    Description = Description,
                    Problems = problems.Select(problem => problem.Translate()).ToArray(),
                    UseZipArchiveFeatureCompareTranslator = useZipArchiveFeatureCompareTranslator
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
