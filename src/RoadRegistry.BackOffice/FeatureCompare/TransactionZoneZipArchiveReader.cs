namespace RoadRegistry.BackOffice.FeatureCompare;

using System;
using System.IO.Compression;
using System.Linq;
using Extracts;
using Uploads;

public interface ITransactionZoneZipArchiveReader
{
    TransactionZoneDetails Read(ZipArchive archive);
}

public class TransactionZoneZipArchiveReader: ITransactionZoneZipArchiveReader
{
    private const FeatureType FeatureType = Extracts.FeatureType.Change;

    private readonly V1.Readers.TransactionZoneFeatureCompareFeatureReader _v1;
    private readonly V2.Readers.TransactionZoneFeatureCompareFeatureReader _v2;

    public TransactionZoneZipArchiveReader(V1.Readers.TransactionZoneFeatureCompareFeatureReader v1, V2.Readers.TransactionZoneFeatureCompareFeatureReader v2)
    {
        _v1 = v1;
        _v2 = v2;
    }

    public TransactionZoneDetails Read(ZipArchive archive)
    {
        return ReadV2(archive)
            ?? ReadV1(archive)
            ?? throw new InvalidOperationException();
    }

    private TransactionZoneDetails? ReadV2(ZipArchive archive)
    {
        var context = new V2.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty);
        var (transactionZones, problems) = _v2.Read(archive, FeatureType, context);
        problems.ThrowIfError();

        var transactionZone = transactionZones.SingleOrDefault()?.Attributes;
        if (transactionZone is not null)
        {
            return new TransactionZoneDetails
            {
                DownloadId = transactionZone.DownloadId,
                Description = transactionZone.Description,
                OperatorName = transactionZone.OperatorName,
                Organization = transactionZone.Organization,
            };
        }

        return null;
    }

    private TransactionZoneDetails? ReadV1(ZipArchive archive)
    {
        const ExtractFileName extractFileName = ExtractFileName.Transactiezones;

        var context = new V1.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty);
        var (transactionZones, problems) = _v1.Read(archive, FeatureType, extractFileName, context);
        problems.ThrowIfError();

        var transactionZone = transactionZones.SingleOrDefault()?.Attributes;
        if (transactionZone is not null)
        {
            return new TransactionZoneDetails
            {
                DownloadId = transactionZone.DownloadId,
                Description = transactionZone.Description,
                OperatorName = transactionZone.OperatorName,
                Organization = transactionZone.Organization,
            };
        }

        return null;
    }
}

public sealed record TransactionZoneDetails
{
    public required DownloadId DownloadId { get; init; }
    public required ExtractDescription Description { get; init; }
    public required OperatorName OperatorName { get; init; }
    public required OrganizationId Organization { get; init; }
}
