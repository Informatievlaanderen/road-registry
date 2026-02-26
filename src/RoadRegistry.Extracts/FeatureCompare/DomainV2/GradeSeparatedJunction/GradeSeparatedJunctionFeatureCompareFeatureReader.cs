namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.GradeSeparatedJunction;

using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.Extracts.Infrastructure.Extensions;
using RoadRegistry.Extracts.Uploads;
using Schemas.Inwinning.GradeSeparatedJuntions;

public class GradeSeparatedJunctionFeatureCompareFeatureReader : VersionedZipArchiveFeatureReader<Feature<GradeSeparatedJunctionFeatureCompareAttributes>>
{
    private const ExtractFileName FileName = ExtractFileName.RltOgkruising;

    public GradeSeparatedJunctionFeatureCompareFeatureReader(FileEncoding encoding)
        : base(new InwinningFeatureReader(encoding))
    {
    }

    public override (List<Feature<GradeSeparatedJunctionFeatureCompareAttributes>>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ZipArchiveFeatureReaderContext context)
    {
        var (features, problems) = base.Read(archive, featureType, context);

        problems += archive.ValidateUniqueIdentifiers(features, featureType, FileName, feature => feature.Attributes.Id);

        return (features, problems);
    }

    private sealed class InwinningFeatureReader : ZipArchiveDbaseFeatureReader<GradeSeparatedJunctionDbaseRecord, Feature<GradeSeparatedJunctionFeatureCompareAttributes>>
    {
        public InwinningFeatureReader(Encoding encoding)
            : base(encoding, ExtractFileName.RltOgkruising, GradeSeparatedJunctionDbaseRecord.Schema)
        {
        }

        protected override (Feature<GradeSeparatedJunctionFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, RecordNumber recordNumber, GradeSeparatedJunctionDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                OK_OIDN = dbaseRecord.OK_OIDN.GetValue(),
                BO_WS_TEMPID = dbaseRecord.BO_TEMPID.GetValue(),
                ON_WS_TEMPID = dbaseRecord.ON_TEMPID.GetValue(),
                TYPE = dbaseRecord.TYPE.GetValue()
            }.ToFeature(featureType, FileName, recordNumber);
        }
    }

    private sealed record DbaseRecordData
    {
        public int? OK_OIDN { get; init; }
        public int? BO_WS_TEMPID { get; init; }
        public int? ON_WS_TEMPID { get; init; }
        public int? TYPE { get; init; }

        public (Feature<GradeSeparatedJunctionFeatureCompareAttributes>, ZipArchiveProblems) ToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber)
        {
            var problemBuilder = fileName
                .AtDbaseRecord(featureType, recordNumber)
                .WithIdentifier(nameof(OK_OIDN), OK_OIDN);

            var problems = ZipArchiveProblems.None;

            GradeSeparatedJunctionId ReadId()
            {
                if (OK_OIDN is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(OK_OIDN));
                }
                else if (GradeSeparatedJunctionId.Accepts(OK_OIDN.Value))
                {
                    return new GradeSeparatedJunctionId(OK_OIDN.Value);
                }
                else
                {
                    problems += problemBuilder.IdentifierZero();
                }

                return default;
            }

            RoadSegmentTempId ReadUpperRoadSegmentId()
            {
                if (BO_WS_TEMPID is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(BO_WS_TEMPID));
                }
                else if (RoadSegmentTempId.Accepts(BO_WS_TEMPID.Value))
                {
                    return new RoadSegmentTempId(BO_WS_TEMPID.Value);
                }
                else
                {
                    problems += problemBuilder.UpperRoadSegmentIdOutOfRange(BO_WS_TEMPID.Value);
                }

                return default;
            }

            RoadSegmentTempId ReadLowerRoadSegmentId()
            {
                if (ON_WS_TEMPID is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(ON_WS_TEMPID));
                }
                else if (RoadSegmentTempId.Accepts(ON_WS_TEMPID.Value))
                {
                    return new RoadSegmentTempId(ON_WS_TEMPID.Value);
                }
                else
                {
                    problems += problemBuilder.LowerRoadSegmentIdOutOfRange(ON_WS_TEMPID.Value);
                }

                return default;
            }

            GradeSeparatedJunctionTypeV2 ReadType()
            {
                if (TYPE is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(TYPE));
                }
                else if (GradeSeparatedJunctionTypeV2.ByIdentifier.TryGetValue(TYPE.Value, out var value))
                {
                    return value;
                }
                else
                {
                    problems += problemBuilder.GradeSeparatedJunctionTypeV2Mismatch(TYPE.Value);
                }

                return default;
            }

            var feature = Feature.New(recordNumber, new GradeSeparatedJunctionFeatureCompareAttributes
            {
                Id = ReadId(),
                UpperRoadSegmentId = ReadUpperRoadSegmentId(),
                LowerRoadSegmentId = ReadLowerRoadSegmentId(),
                Type = ReadType()
            });
            return (feature, problems);
        }
    }
}
