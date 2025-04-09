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
    private readonly V1.Readers.TransactionZoneFeatureCompareFeatureReader _v1;
    private readonly V2.Readers.TransactionZoneFeatureCompareFeatureReader _v2;

    public TransactionZoneZipArchiveReader(V1.Readers.TransactionZoneFeatureCompareFeatureReader v1, V2.Readers.TransactionZoneFeatureCompareFeatureReader v2)
    {
        _v1 = v1;
        _v2 = v2;
    }

    public TransactionZoneDetails Read(ZipArchive archive)
    {
        const ExtractFileName extractFileName = ExtractFileName.Transactiezones;
        const FeatureType featureType = FeatureType.Change;

        {
            var context = new V2.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty);
            var (transactionZones, problems) = _v2.Read(archive, featureType, context);
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
        }

        {
            var context = new V1.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty);
            var (transactionZones, problems) = _v1.Read(archive, featureType, extractFileName, context);
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
        }

        throw new InvalidOperationException();
    }
}

public sealed record TransactionZoneDetails
{
    public required DownloadId DownloadId { get; init; }
    public required ExtractDescription Description { get; init; }
    public required OperatorName OperatorName { get; init; }
    public required OrganizationId Organization { get; init; }
}
