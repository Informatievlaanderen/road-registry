namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts.Dbase.RoadSegments;

public class RoadSegmentWidthFeatureCompareFeatureReader : VersionedFeatureReader<Feature<RoadSegmentWidthFeatureCompareAttributes>>
{
    public RoadSegmentWidthFeatureCompareFeatureReader(Encoding encoding)
        : base(new ExtractsFeatureReader(encoding),
            new UploadsV2FeatureReader(encoding),
            new UploadsV1FeatureReader(encoding))
    {
    }

    private sealed class ExtractsFeatureReader : DbaseFeatureReader<RoadSegmentWidthAttributeDbaseRecord, Feature<RoadSegmentWidthFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, RoadSegmentWidthAttributeDbaseRecord.Schema)
        {
        }

        protected override Feature<RoadSegmentWidthFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, RoadSegmentWidthAttributeDbaseRecord dbaseRecord)
        {
            return new Feature<RoadSegmentWidthFeatureCompareAttributes>(recordNumber, new RoadSegmentWidthFeatureCompareAttributes
            {
                Id = dbaseRecord.WB_OIDN.Value,
                RoadSegmentId = dbaseRecord.WS_OIDN.Value,
                FromPosition = dbaseRecord.VANPOS.Value,
                ToPosition = dbaseRecord.TOTPOS.Value,
                Width = dbaseRecord.BREEDTE.Value
            });
        }
    }

    private sealed class UploadsV2FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentWidthAttributeDbaseRecord, Feature<RoadSegmentWidthFeatureCompareAttributes>>
    {
        public UploadsV2FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentWidthAttributeDbaseRecord.Schema)
        {
        }

        protected override Feature<RoadSegmentWidthFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentWidthAttributeDbaseRecord dbaseRecord)
        {
            return new Feature<RoadSegmentWidthFeatureCompareAttributes>(recordNumber, new RoadSegmentWidthFeatureCompareAttributes
            {
                Id = dbaseRecord.WB_OIDN.Value,
                RoadSegmentId = dbaseRecord.WS_OIDN.Value,
                FromPosition = dbaseRecord.VANPOS.Value,
                ToPosition = dbaseRecord.TOTPOS.Value,
                Width = dbaseRecord.BREEDTE.Value
            });
        }
    }

    private sealed class UploadsV1FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentWidthAttributeDbaseRecord, Feature<RoadSegmentWidthFeatureCompareAttributes>>
    {
        public UploadsV1FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentWidthAttributeDbaseRecord.Schema)
        {
        }

        protected override Feature<RoadSegmentWidthFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentWidthAttributeDbaseRecord dbaseRecord)
        {
            return new Feature<RoadSegmentWidthFeatureCompareAttributes>(recordNumber, new RoadSegmentWidthFeatureCompareAttributes
            {
                Id = dbaseRecord.WB_OIDN.Value,
                RoadSegmentId = dbaseRecord.WS_OIDN.Value,
                FromPosition = dbaseRecord.VANPOS.Value,
                ToPosition = dbaseRecord.TOTPOS.Value,
                Width = dbaseRecord.BREEDTE.Value
            });
        }
    }
}
