namespace RoadRegistry.BackOffice.FeatureCompare.V2.Readers;

using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extensions;
using Extracts;
using Models;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Infrastructure.Extensions;
using RoadRegistry.Extracts.Schemas.ExtractV1;
using RoadRegistry.Extracts.Uploads;
using Translators;
using Uploads;

public class TransactionZoneFeatureCompareFeatureReader : VersionedZipArchiveFeatureReader<Feature<TransactionZoneFeatureCompareAttributes>>
{
    private const ExtractFileName FileName = ExtractFileName.Transactiezones;

    public TransactionZoneFeatureCompareFeatureReader(FileEncoding encoding)
        : base(new ExtractsFeatureReader(encoding))
    {
    }

    private sealed class ExtractsFeatureReader : ZipArchiveDbaseFeatureReader<TransactionZoneDbaseRecord, Feature<TransactionZoneFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, TransactionZoneFeatureCompareFeatureReader.FileName, TransactionZoneDbaseRecord.Schema, treatHasNoDbaseRecordsAsError: true)
        {
        }

        protected override (Feature<TransactionZoneFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, RecordNumber recordNumber, TransactionZoneDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                BESCHRIJV = dbaseRecord.BESCHRIJV.GetValue(),
                DOWNLOADID = dbaseRecord.DOWNLOADID.GetValue(),
                OPERATOR = dbaseRecord.OPERATOR.GetValue(),
                ORG = dbaseRecord.ORG.GetValue()
            }.ToFeature(featureType, FileName, recordNumber, context);
        }

        protected override (List<Feature<TransactionZoneFeatureCompareAttributes>>, ZipArchiveProblems) ReadFeatures(FeatureType featureType, ZipArchiveEntry entry, IDbaseRecordEnumerator<TransactionZoneDbaseRecord> records, ZipArchiveFeatureReaderContext context)
        {
            var (features, problems) = base.ReadFeatures(featureType, entry, records, context);

            if (features.Count > 1)
            {
                problems += entry.HasTooManyDbaseRecords(1, features.Count);
            }

            return (features, problems);
        }
    }

    private sealed record DbaseRecordData
    {
        public string BESCHRIJV { get; init; }
        public string DOWNLOADID { get; init; }
        public string OPERATOR { get; init; }
        public string ORG { get; init; }

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

            OperatorName ReadOperatorName()
            {
                if (!string.IsNullOrEmpty(OPERATOR))
                {
                    if (OperatorName.AcceptsValue(OPERATOR))
                    {
                        return new OperatorName(OPERATOR);
                    }
                    else
                    {
                        problems += problemBuilder.OperatorNameOutOfRange(OPERATOR);
                    }
                }

                return OperatorName.Unknown;
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
                    if (expectedDownloadId.HasValue && !value.Equals(expectedDownloadId.Value))
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

            OrganizationId ReadOrganization()
            {
                if (ORG is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(ORG));
                }
                else if (OrganizationId.AcceptsValue(ORG))
                {
                    return new OrganizationId(ORG);
                }
                else
                {
                    problems += problemBuilder.OrganizationIdOutOfRange(ORG);
                }

                return OrganizationId.Unknown;
            }

            var feature = Feature.New(recordNumber, new TransactionZoneFeatureCompareAttributes
            {
                Description = ReadDescription(),
                OperatorName = ReadOperatorName(),
                DownloadId = ReadDownloadId(),
                Organization = ReadOrganization()
            });
            return (feature, problems);
        }
    }
}
