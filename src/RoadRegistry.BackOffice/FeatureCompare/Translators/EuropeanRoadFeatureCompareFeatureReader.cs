namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts.Dbase.RoadSegments;

public class EuropeanRoadFeatureCompareFeatureReader : VersionedFeatureReader<Feature<EuropeanRoadFeatureCompareAttributes>>
{
    public EuropeanRoadFeatureCompareFeatureReader(Encoding encoding)
        : base(new ExtractsFeatureReader(encoding),
            new UploadsV2FeatureReader(encoding),
            new UploadsV1FeatureReader(encoding))
    {
    }

    private sealed class ExtractsFeatureReader : DbaseFeatureReader<RoadSegmentEuropeanRoadAttributeDbaseRecord, Feature<EuropeanRoadFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema)
        {
        }

        protected override Feature<EuropeanRoadFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, RoadSegmentEuropeanRoadAttributeDbaseRecord dbaseRecord)
        {
            return new Feature<EuropeanRoadFeatureCompareAttributes>(recordNumber, new EuropeanRoadFeatureCompareAttributes
            {
                Id = dbaseRecord.EU_OIDN.Value,
                Number = dbaseRecord.EUNUMMER.Value,
                RoadSegmentId = dbaseRecord.WS_OIDN.Value
            });
        }
    }

    private sealed class UploadsV2FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentEuropeanRoadAttributeDbaseRecord, Feature<EuropeanRoadFeatureCompareAttributes>>
    {
        public UploadsV2FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema)
        {
        }

        protected override Feature<EuropeanRoadFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentEuropeanRoadAttributeDbaseRecord dbaseRecord)
        {
            return new Feature<EuropeanRoadFeatureCompareAttributes>(recordNumber, new EuropeanRoadFeatureCompareAttributes
            {
                Id = dbaseRecord.EU_OIDN.Value,
                Number = dbaseRecord.EUNUMMER.Value,
                RoadSegmentId = dbaseRecord.WS_OIDN.Value
            });
        }
    }

    private sealed class UploadsV1FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentEuropeanRoadAttributeDbaseRecord, Feature<EuropeanRoadFeatureCompareAttributes>>
    {
        public UploadsV1FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentEuropeanRoadAttributeDbaseRecord.Schema)
        {
        }

        protected override Feature<EuropeanRoadFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentEuropeanRoadAttributeDbaseRecord dbaseRecord)
        {
            return new Feature<EuropeanRoadFeatureCompareAttributes>(recordNumber, new EuropeanRoadFeatureCompareAttributes
            {
                Id = dbaseRecord.EU_OIDN.Value,
                Number = dbaseRecord.EUNUMMER.Value,
                RoadSegmentId = dbaseRecord.WS_OIDN.Value
            });
        }
    }
}
