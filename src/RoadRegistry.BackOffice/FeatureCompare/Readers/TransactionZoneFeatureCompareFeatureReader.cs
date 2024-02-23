namespace RoadRegistry.BackOffice.FeatureCompare.Readers;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Extensions;
using Extracts;
using Extracts.Dbase;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using Translators;
using Uploads;

public class TransactionZoneFeatureCompareFeatureReader : VersionedZipArchiveFeatureReader<Feature<TransactionZoneFeatureCompareAttributes>>
{
    public TransactionZoneFeatureCompareFeatureReader(FileEncoding encoding)
        : base(new ExtractsFeatureReader(encoding))
    {
    }

    private sealed class ExtractsFeatureReader : ZipArchiveDbaseFeatureReader<TransactionZoneDbaseRecord, Feature<TransactionZoneFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, TransactionZoneDbaseRecord.Schema, treatHasNoDbaseRecordsAsError: true)
        {
        }

        protected override (Feature<TransactionZoneFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber, TransactionZoneDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                BESCHRIJV = dbaseRecord.BESCHRIJV.GetValue(),
                DOWNLOADID = dbaseRecord.DOWNLOADID.GetValue(),
                OPERATOR = dbaseRecord.OPERATOR.GetValue(),
                ORG = dbaseRecord.ORG.GetValue()
            }.ToFeature(featureType, fileName, recordNumber, context);
        }

        protected override (List<Feature<TransactionZoneFeatureCompareAttributes>>, ZipArchiveProblems) ReadFeatures(FeatureType featureType, ExtractFileName fileName, ZipArchiveEntry entry, IDbaseRecordEnumerator<TransactionZoneDbaseRecord> records, ZipArchiveFeatureReaderContext context)
        {
            var (features, problems) = base.ReadFeatures(featureType, fileName, entry, records, context);

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
