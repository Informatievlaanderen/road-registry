namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.EuropeanRoad;

using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using RoadRegistry.Extracts.Infrastructure.Extensions;
using RoadRegistry.Extracts.Schemas.DomainV2.RoadSegments;
using RoadRegistry.Extracts.Uploads;

public class EuropeanRoadFeatureCompareFeatureReader : VersionedZipArchiveFeatureReader<Feature<EuropeanRoadFeatureCompareAttributes>>
{
    private const ExtractFileName FileName = ExtractFileName.AttEuropweg;

    public EuropeanRoadFeatureCompareFeatureReader(FileEncoding encoding)
        : base(new ExtractsFeatureReader(encoding))
    {
    }

    public override (List<Feature<EuropeanRoadFeatureCompareAttributes>>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ZipArchiveFeatureReaderContext context)
    {
        var (features, problems) = base.Read(archive, featureType, context);

        problems += archive.ValidateUniqueIdentifiers(features, featureType, FileName, feature => feature.Attributes.Id);

        switch (featureType)
        {
            case FeatureType.Change:
                problems += archive.ValidateUniqueEuropeanRoads(features, featureType, FileName);
                break;
        }

        return (features, problems);
    }

    private sealed class ExtractsFeatureReader : ZipArchiveDbaseFeatureReader<RoadSegmentEuropeanRoadAttributeDbaseRecord, Feature<EuropeanRoadFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, EuropeanRoadFeatureCompareFeatureReader.FileName, RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema)
        {
        }

        protected override (Feature<EuropeanRoadFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, RecordNumber recordNumber, RoadSegmentEuropeanRoadAttributeDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                EU_OIDN = dbaseRecord.EU_OIDN.GetValue(),
                EUNUMMER = dbaseRecord.EUNUMMER.GetValue(),
                WS_TEMPID = dbaseRecord.WS_TEMPID.GetValue()
            }.ToFeature(featureType, FileName, recordNumber);
        }
    }

    private sealed record DbaseRecordData
    {
        public int? EU_OIDN { get; init; }
        public int? WS_TEMPID { get; init; }
        public string EUNUMMER { get; init; }

        public (Feature<EuropeanRoadFeatureCompareAttributes>, ZipArchiveProblems) ToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber)
        {
            var problemBuilder = fileName
                .AtDbaseRecord(featureType, recordNumber)
                .WithIdentifier(nameof(EU_OIDN), EU_OIDN);

            var problems = ZipArchiveProblems.None;

            AttributeId ReadId()
            {
                if (EU_OIDN is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(EU_OIDN));
                }
                else if (AttributeId.Accepts(EU_OIDN.Value))
                {
                    return new AttributeId(EU_OIDN.Value);
                }
                else
                {
                    problems += problemBuilder.IdentifierZero();
                }

                return default;
            }

            RoadSegmentId ReadRoadSegmentId()
            {
                if (WS_TEMPID is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(WS_TEMPID));
                }
                else if (RoadSegmentId.Accepts(WS_TEMPID.Value))
                {
                    return new RoadSegmentId(WS_TEMPID.Value);
                }
                else
                {
                    problems += problemBuilder.RoadSegmentIdOutOfRange(WS_TEMPID.Value);
                }

                return default;
            }

            EuropeanRoadNumber ReadNumber()
            {
                if (EUNUMMER is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(EUNUMMER));
                }
                else if (EuropeanRoadNumber.TryParse(EUNUMMER, out var value))
                {
                    return value;
                }
                else
                {
                    problems += problemBuilder.NotEuropeanRoadNumber(EUNUMMER);
                }

                return default;
            }

            var feature = Feature.New(recordNumber, new EuropeanRoadFeatureCompareAttributes
            {
                Id = ReadId(),
                RoadSegmentId = ReadRoadSegmentId(),
                Number = ReadNumber()
            });
            return (feature, problems);
        }
    }
}
