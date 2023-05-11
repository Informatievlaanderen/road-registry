namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts.Dbase.RoadSegments;

public class RoadSegmentSurfaceFeatureCompareFeatureReader : VersionedFeatureReader<Feature<RoadSegmentSurfaceFeatureCompareAttributes>>
{
    public RoadSegmentSurfaceFeatureCompareFeatureReader(Encoding encoding)
        : base(new ExtractsFeatureReader(encoding),
            new UploadsV2FeatureReader(encoding),
            new UploadsV1FeatureReader(encoding))
    {
    }

    private sealed class ExtractsFeatureReader : DbaseFeatureReader<RoadSegmentSurfaceAttributeDbaseRecord, Feature<RoadSegmentSurfaceFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, RoadSegmentSurfaceAttributeDbaseRecord.Schema)
        {
        }

        protected override Feature<RoadSegmentSurfaceFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, RoadSegmentSurfaceAttributeDbaseRecord dbaseRecord)
        {
            return new Feature<RoadSegmentSurfaceFeatureCompareAttributes>(recordNumber, new RoadSegmentSurfaceFeatureCompareAttributes
            {
                Id = dbaseRecord.WV_OIDN.Value,
                RoadSegmentId = dbaseRecord.WS_OIDN.Value,
                FromPosition = dbaseRecord.VANPOS.Value,
                ToPosition = dbaseRecord.TOTPOS.Value,
                Type = dbaseRecord.TYPE.Value
            });
        }
    }

    private sealed class UploadsV2FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentSurfaceAttributeDbaseRecord, Feature<RoadSegmentSurfaceFeatureCompareAttributes>>
    {
        public UploadsV2FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentSurfaceAttributeDbaseRecord.Schema)
        {
        }

        protected override Feature<RoadSegmentSurfaceFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentSurfaceAttributeDbaseRecord dbaseRecord)
        {
            return new Feature<RoadSegmentSurfaceFeatureCompareAttributes>(recordNumber, new RoadSegmentSurfaceFeatureCompareAttributes
            {
                Id = dbaseRecord.WV_OIDN.Value,
                RoadSegmentId = dbaseRecord.WS_OIDN.Value,
                FromPosition = dbaseRecord.VANPOS.Value,
                ToPosition = dbaseRecord.TOTPOS.Value,
                Type = dbaseRecord.TYPE.Value
            });
        }
    }

    private sealed class UploadsV1FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentSurfaceAttributeDbaseRecord, Feature<RoadSegmentSurfaceFeatureCompareAttributes>>
    {
        public UploadsV1FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentSurfaceAttributeDbaseRecord.Schema)
        {
        }

        protected override Feature<RoadSegmentSurfaceFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentSurfaceAttributeDbaseRecord dbaseRecord)
        {
            return new Feature<RoadSegmentSurfaceFeatureCompareAttributes>(recordNumber, new RoadSegmentSurfaceFeatureCompareAttributes
            {
                Id = dbaseRecord.WV_OIDN.Value,
                RoadSegmentId = dbaseRecord.WS_OIDN.Value,
                FromPosition = dbaseRecord.VANPOS.Value,
                ToPosition = dbaseRecord.TOTPOS.Value,
                Type = dbaseRecord.TYPE.Value
            });
        }
    }
}
