namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts.Dbase.RoadSegments;

public class NumberedRoadFeatureCompareFeatureReader : VersionedFeatureReader<Feature<NumberedRoadFeatureCompareAttributes>>
{
    public NumberedRoadFeatureCompareFeatureReader(Encoding encoding)
        : base(new ExtractsFeatureReader(encoding),
            new UploadsV2FeatureReader(encoding),
            new UploadsV1FeatureReader(encoding))
    {
    }

    private sealed class ExtractsFeatureReader : DbaseFeatureReader<RoadSegmentNumberedRoadAttributeDbaseRecord, Feature<NumberedRoadFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, RoadSegmentNumberedRoadAttributeDbaseRecord.Schema)
        {
        }

        protected override Feature<NumberedRoadFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, RoadSegmentNumberedRoadAttributeDbaseRecord dbaseRecord)
        {
            return new Feature<NumberedRoadFeatureCompareAttributes>(recordNumber, new NumberedRoadFeatureCompareAttributes
            {
                Id = dbaseRecord.GW_OIDN.Value,
                Number = dbaseRecord.IDENT8.Value,
                Direction = dbaseRecord.RICHTING.Value,
                Ordinal = dbaseRecord.VOLGNUMMER.Value,
                RoadSegmentId = dbaseRecord.WS_OIDN.Value
            });
        }
    }

    private sealed class UploadsV2FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord, Feature<NumberedRoadFeatureCompareAttributes>>
    {
        public UploadsV2FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord.Schema)
        {
        }

        protected override Feature<NumberedRoadFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord dbaseRecord)
        {
            return new Feature<NumberedRoadFeatureCompareAttributes>(recordNumber, new NumberedRoadFeatureCompareAttributes
            {
                Id = dbaseRecord.GW_OIDN.Value,
                Number = dbaseRecord.IDENT8.Value,
                Direction = dbaseRecord.RICHTING.Value,
                Ordinal = dbaseRecord.VOLGNUMMER.Value,
                RoadSegmentId = dbaseRecord.WS_OIDN.Value
            });
        }
    }

    private sealed class UploadsV1FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord, Feature<NumberedRoadFeatureCompareAttributes>>
    {
        public UploadsV1FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord.Schema)
        {
        }

        protected override Feature<NumberedRoadFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentNumberedRoadAttributeDbaseRecord dbaseRecord)
        {
            return new Feature<NumberedRoadFeatureCompareAttributes>(recordNumber, new NumberedRoadFeatureCompareAttributes
            {
                Id = dbaseRecord.GW_OIDN.Value,
                Number = dbaseRecord.IDENT8.Value,
                Direction = dbaseRecord.RICHTING.Value,
                Ordinal = dbaseRecord.VOLGNUMMER.Value,
                RoadSegmentId = dbaseRecord.WS_OIDN.Value
            });
        }
    }
}
