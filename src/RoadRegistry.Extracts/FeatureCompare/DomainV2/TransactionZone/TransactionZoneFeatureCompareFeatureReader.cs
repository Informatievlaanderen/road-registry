namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.TransactionZone;

using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extensions;
using Infrastructure.ShapeFile;
using NetTopologySuite.Geometries;
using RoadRegistry.Extracts.Infrastructure.Extensions;
using RoadRegistry.Extracts.Uploads;
using Schemas.Inwinning;

public class TransactionZoneFeatureCompareFeatureReader : VersionedZipArchiveFeatureReader<Feature<TransactionZoneFeatureCompareAttributes>>
{
    private const ExtractFileName FileName = ExtractFileName.Transactiezones;

    public TransactionZoneFeatureCompareFeatureReader(FileEncoding encoding)
        : base(new InwinningFeatureReader(encoding))
    {
    }

    private sealed class InwinningFeatureReader : ZipArchiveShapeFeatureReader<TransactionZoneDbaseRecord, Feature<TransactionZoneFeatureCompareAttributes>>
    {
        public InwinningFeatureReader(Encoding encoding)
            : base(encoding, TransactionZoneFeatureCompareFeatureReader.FileName, TransactionZoneDbaseRecord.Schema, treatHasNoDbaseRecordsAsError: true)
        {
        }

        protected override (Feature<TransactionZoneFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, RecordNumber recordNumber, TransactionZoneDbaseRecord dbaseRecord, Geometry geometry, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                Geometry = (MultiPolygon)geometry,
                BESCHRIJV = dbaseRecord.BESCHRIJV.GetValue(),
                DOWNLOADID = dbaseRecord.DOWNLOADID.GetValue()
            }.ToFeature(featureType, FileName, recordNumber, context);
        }

        protected override (List<Feature<TransactionZoneFeatureCompareAttributes>>, ZipArchiveProblems) ReadFeatures(FeatureType featureType, ZipArchiveEntry dbfEntry, ZipArchiveEntry shpEntry, IShapeFileRecordEnumerator<TransactionZoneDbaseRecord> records, ZipArchiveFeatureReaderContext context)
        {
            var (features, problems) = base.ReadFeatures(featureType, dbfEntry, shpEntry, records, context);

            if (features.Count > 1)
            {
                problems += dbfEntry.HasTooManyDbaseRecords(1, features.Count);
            }

            return (features, problems);
        }
    }

    private sealed record DbaseRecordData
    {
        public required MultiPolygon Geometry { get; init; }
        public required string BESCHRIJV { get; init; }
        public required string DOWNLOADID { get; init; }

        public (Feature<TransactionZoneFeatureCompareAttributes>, ZipArchiveProblems) ToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber, ZipArchiveFeatureReaderContext context)
        {
            var problemBuilder = fileName.AtDbaseRecord(featureType, recordNumber);

            var problems = ZipArchiveProblems.None;

            ExtractDescription ReadDescription()
            {
                if (string.IsNullOrEmpty(BESCHRIJV))
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(BESCHRIJV));
                }
                else if (ExtractDescription.AcceptsValue(BESCHRIJV))
                {
                    return new ExtractDescription(BESCHRIJV);
                }
                else
                {
                    problems += problemBuilder.ExtractDescriptionOutOfRange(BESCHRIJV);
                }

                return default;
            }

            DownloadId ReadDownloadId()
            {
                if (string.IsNullOrEmpty(DOWNLOADID))
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(DOWNLOADID));
                }
                else if (DownloadId.TryParse(DOWNLOADID, out var value))
                {
                    var expectedDownloadId = context.ZipArchiveMetadata.DownloadId;
                    if (expectedDownloadId is not null && !value.Equals(expectedDownloadId.Value))
                    {
                        problems += problemBuilder.DownloadIdDiffersFromMetadata(DOWNLOADID, expectedDownloadId.ToString());
                    }

                    return value;
                }
                else
                {
                    problems += problemBuilder.DownloadIdInvalidFormat(DOWNLOADID);
                }

                return default;
            }

            var feature = Feature.New(recordNumber, new TransactionZoneFeatureCompareAttributes
            {
                Geometry = Geometry.ToExtractGeometry(),
                Description = ReadDescription(),
                DownloadId = ReadDownloadId()
            });
            return (feature, problems);
        }
    }
}
