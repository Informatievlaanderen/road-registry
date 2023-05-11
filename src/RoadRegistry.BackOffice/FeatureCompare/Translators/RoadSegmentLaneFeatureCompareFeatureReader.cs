namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts.Dbase.RoadSegments;

public class RoadSegmentLaneFeatureCompareFeatureReader : VersionedFeatureReader<Feature<RoadSegmentLaneFeatureCompareAttributes>>
{
    public RoadSegmentLaneFeatureCompareFeatureReader(Encoding encoding)
        : base(new ExtractsFeatureReader(encoding),
            new UploadsV2FeatureReader(encoding),
            new UploadsV1FeatureReader(encoding))
    {
    }

    private sealed class ExtractsFeatureReader : DbaseFeatureReader<RoadSegmentLaneAttributeDbaseRecord, Feature<RoadSegmentLaneFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, RoadSegmentLaneAttributeDbaseRecord.Schema)
        {
        }

        protected override Feature<RoadSegmentLaneFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, RoadSegmentLaneAttributeDbaseRecord dbaseRecord)
        {
            return new Feature<RoadSegmentLaneFeatureCompareAttributes>(recordNumber, new RoadSegmentLaneFeatureCompareAttributes
            {
                Id = dbaseRecord.RS_OIDN.Value,
                RoadSegmentId = dbaseRecord.WS_OIDN.Value,
                FromPosition = dbaseRecord.VANPOS.Value,
                ToPosition = dbaseRecord.TOTPOS.Value,
                Count = dbaseRecord.AANTAL.Value,
                Direction = dbaseRecord.RICHTING.Value
            });
        }
    }

    private sealed class UploadsV2FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentLaneAttributeDbaseRecord, Feature<RoadSegmentLaneFeatureCompareAttributes>>
    {
        public UploadsV2FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentLaneAttributeDbaseRecord.Schema)
        {
        }

        protected override Feature<RoadSegmentLaneFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentLaneAttributeDbaseRecord dbaseRecord)
        {
            return new Feature<RoadSegmentLaneFeatureCompareAttributes>(recordNumber, new RoadSegmentLaneFeatureCompareAttributes
            {
                Id = dbaseRecord.RS_OIDN.Value,
                RoadSegmentId = dbaseRecord.WS_OIDN.Value,
                FromPosition = dbaseRecord.VANPOS.Value,
                ToPosition = dbaseRecord.TOTPOS.Value,
                Count = dbaseRecord.AANTAL.Value,
                Direction = dbaseRecord.RICHTING.Value
            });
        }
    }

    private sealed class UploadsV1FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentLaneAttributeDbaseRecord, Feature<RoadSegmentLaneFeatureCompareAttributes>>
    {
        public UploadsV1FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentLaneAttributeDbaseRecord.Schema)
        {
        }

        protected override Feature<RoadSegmentLaneFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentLaneAttributeDbaseRecord dbaseRecord)
        {
            return new Feature<RoadSegmentLaneFeatureCompareAttributes>(recordNumber, new RoadSegmentLaneFeatureCompareAttributes
            {
                Id = dbaseRecord.RS_OIDN.Value,
                RoadSegmentId = dbaseRecord.WS_OIDN.Value,
                FromPosition = dbaseRecord.VANPOS.Value,
                ToPosition = dbaseRecord.TOTPOS.Value,
                Count = dbaseRecord.AANTAL.Value,
                Direction = dbaseRecord.RICHTING.Value
            });
        }
    }
}
