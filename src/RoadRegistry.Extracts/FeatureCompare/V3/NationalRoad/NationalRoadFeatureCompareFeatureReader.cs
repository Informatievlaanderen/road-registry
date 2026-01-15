namespace RoadRegistry.Extracts.FeatureCompare.V3.NationalRoad;

using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Infrastructure.Extensions;
using Schemas.ExtractV2.RoadSegments;
using Uploads;

public class NationalRoadFeatureCompareFeatureReader : VersionedZipArchiveFeatureReader<Feature<NationalRoadFeatureCompareAttributes>>
{
    private const ExtractFileName FileName = ExtractFileName.AttNationweg;

    public NationalRoadFeatureCompareFeatureReader(FileEncoding encoding)
        : base(new ExtractsFeatureReader(encoding))
    {
    }

    public override (List<Feature<NationalRoadFeatureCompareAttributes>>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ZipArchiveFeatureReaderContext context)
    {
        var (features, problems) = base.Read(archive, featureType, context);

        problems += archive.ValidateUniqueIdentifiers(features, featureType, FileName, feature => feature.Attributes.Id);

        switch (featureType)
        {
            case FeatureType.Change:
                problems += archive.ValidateUniqueNationalRoads(features, featureType, FileName);
                break;
        }

        return (features, problems);
    }

    private sealed class ExtractsFeatureReader : ZipArchiveDbaseFeatureReader<RoadSegmentNationalRoadAttributeDbaseRecord, Feature<NationalRoadFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, NationalRoadFeatureCompareFeatureReader.FileName, RoadSegmentNationalRoadAttributeDbaseRecord.Schema)
        {
        }

        protected override (Feature<NationalRoadFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, RecordNumber recordNumber, RoadSegmentNationalRoadAttributeDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                NW_OIDN = dbaseRecord.NW_OIDN.GetValue(),
                WS_OIDN = dbaseRecord.WS_OIDN.GetValue(),
                IDENT2 = dbaseRecord.IDENT2.GetValue()
            }.ToFeature(featureType, FileName, recordNumber);
        }
    }

    private sealed record DbaseRecordData
    {
        public int? NW_OIDN { get; init; }
        public int? WS_OIDN { get; init; }
        public string IDENT2 { get; init; }

        public (Feature<NationalRoadFeatureCompareAttributes>, ZipArchiveProblems) ToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber)
        {
            var problemBuilder = fileName
                .AtDbaseRecord(featureType, recordNumber)
                .WithIdentifier(nameof(NW_OIDN), NW_OIDN);

            var problems = ZipArchiveProblems.None;

            AttributeId ReadId()
            {
                if (NW_OIDN is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(NW_OIDN));
                }
                else if (AttributeId.Accepts(NW_OIDN.Value))
                {
                    return new AttributeId(NW_OIDN.Value);
                }
                else
                {
                    problems += problemBuilder.IdentifierZero();
                }

                return default;
            }

            RoadSegmentId ReadRoadSegmentId()
            {
                if (WS_OIDN is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(WS_OIDN));
                }
                else if (RoadSegmentId.Accepts(WS_OIDN.Value))
                {
                    return new RoadSegmentId(WS_OIDN.Value);
                }
                else
                {
                    problems += problemBuilder.RoadSegmentIdOutOfRange(WS_OIDN.Value);
                }

                return default;
            }

            NationalRoadNumber ReadNumber()
            {
                if (IDENT2 is null)
                {
                    problems += problemBuilder.RequiredFieldIsNull(nameof(IDENT2));
                }
                else if (NationalRoadNumber.TryParse(IDENT2, out var value))
                {
                    return value;
                }
                else
                {
                    problems += problemBuilder.NotNationalRoadNumber(IDENT2);
                }

                return default;
            }

            var feature = Feature.New(recordNumber, new NationalRoadFeatureCompareAttributes
            {
                Id = ReadId(),
                RoadSegmentId = ReadRoadSegmentId(),
                Number = ReadNumber()
            });
            return (feature, problems);
        }
    }
}
