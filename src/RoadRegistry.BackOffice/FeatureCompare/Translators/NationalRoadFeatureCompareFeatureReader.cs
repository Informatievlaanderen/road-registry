namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extensions;
using Extracts;
using Extracts.Dbase.RoadSegments;
using Uploads;

public class NationalRoadFeatureCompareFeatureReader : VersionedFeatureReader<Feature<NationalRoadFeatureCompareAttributes>>
{
    private static readonly Func<FeatureType, bool> TreatHasNoRecordsAsWarning = _ => true;

    public NationalRoadFeatureCompareFeatureReader(Encoding encoding)
        : base(new ExtractsFeatureReader(encoding),
            new UploadsV2FeatureReader(encoding),
            new UploadsV1FeatureReader(encoding))
    {
    }

    public override (List<Feature<NationalRoadFeatureCompareAttributes>>, ZipArchiveProblems) Read(ZipArchive archive, FeatureType featureType, ExtractFileName fileName, ZipArchiveFeatureReaderContext context)
    {
        var (features, problems) = base.Read(archive, featureType, fileName, context);

        problems += archive.ValidateUniqueIdentifiers(features, featureType, fileName, feature => feature.Attributes.Id);

        return (features, problems);
    }

    private sealed class ExtractsFeatureReader : DbaseFeatureReader<RoadSegmentNationalRoadAttributeDbaseRecord, Feature<NationalRoadFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, RoadSegmentNationalRoadAttributeDbaseRecord.Schema, TreatHasNoRecordsAsWarning)
        {
        }

        protected override (Feature<NationalRoadFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber, RoadSegmentNationalRoadAttributeDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                NW_OIDN = dbaseRecord.NW_OIDN.GetValue(),
                WS_OIDN = dbaseRecord.WS_OIDN.GetValue(),
                IDENT2 = dbaseRecord.IDENT2.GetValue()
            }.ToFeature(featureType, fileName, recordNumber);
        }
    }

    private sealed class UploadsV2FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNationalRoadAttributeDbaseRecord, Feature<NationalRoadFeatureCompareAttributes>>
    {
        public UploadsV2FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNationalRoadAttributeDbaseRecord.Schema, TreatHasNoRecordsAsWarning)
        {
        }

        protected override (Feature<NationalRoadFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNationalRoadAttributeDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                NW_OIDN = dbaseRecord.NW_OIDN.GetValue(),
                WS_OIDN = dbaseRecord.WS_OIDN.GetValue(),
                IDENT2 = dbaseRecord.IDENT2.GetValue()
            }.ToFeature(featureType, fileName, recordNumber);
        }
    }

    private sealed class UploadsV1FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentNationalRoadAttributeDbaseRecord, Feature<NationalRoadFeatureCompareAttributes>>
    {
        public UploadsV1FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentNationalRoadAttributeDbaseRecord.Schema, TreatHasNoRecordsAsWarning)
        {
        }

        protected override (Feature<NationalRoadFeatureCompareAttributes>, ZipArchiveProblems) ConvertToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentNationalRoadAttributeDbaseRecord dbaseRecord, ZipArchiveFeatureReaderContext context)
        {
            return new DbaseRecordData
            {
                NW_OIDN = dbaseRecord.NW_OIDN.GetValue(),
                WS_OIDN = dbaseRecord.WS_OIDN.GetValue(),
                IDENT2 = dbaseRecord.IDENT2.GetValue()
            }.ToFeature(featureType, fileName, recordNumber);
        }
    }

    private sealed record DbaseRecordData
    {
        public int? NW_OIDN { get; init; }
        public int? WS_OIDN { get; init; }
        public string IDENT2 { get; init; }

        public (Feature<NationalRoadFeatureCompareAttributes>, ZipArchiveProblems) ToFeature(FeatureType featureType, ExtractFileName fileName, RecordNumber recordNumber)
        {
            var problemBuilder = fileName.AtDbaseRecord(featureType, recordNumber);

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
