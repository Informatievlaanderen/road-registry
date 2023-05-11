namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts.Dbase.RoadSegments;

public class NationalRoadFeatureCompareFeatureReader : VersionedFeatureReader<Feature<NationalRoadFeatureCompareAttributes>>
{
    public NationalRoadFeatureCompareFeatureReader(Encoding encoding)
        : base(new ExtractsFeatureReader(encoding),
            new UploadsV2FeatureReader(encoding),
            new UploadsV1FeatureReader(encoding))
    {
    }

    private sealed class ExtractsFeatureReader : DbaseFeatureReader<RoadSegmentNationalRoadAttributeDbaseRecord, Feature<NationalRoadFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, RoadSegmentNationalRoadAttributeDbaseRecord.Schema)
        {
        }

        protected override Feature<NationalRoadFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, RoadSegmentNationalRoadAttributeDbaseRecord dbaseRecord)
        {
            return new Feature<NationalRoadFeatureCompareAttributes>(recordNumber, new NationalRoadFeatureCompareAttributes
            {
                Number = dbaseRecord.IDENT2.Value,
                Id = dbaseRecord.NW_OIDN.Value,
                RoadSegmentId = dbaseRecord.WS_OIDN.Value
            });
        }
    }

    private sealed class UploadsV2FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNationalRoadAttributeDbaseRecord, Feature<NationalRoadFeatureCompareAttributes>>
    {
        public UploadsV2FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNationalRoadAttributeDbaseRecord.Schema)
        {
        }

        protected override Feature<NationalRoadFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentNationalRoadAttributeDbaseRecord dbaseRecord)
        {
            return new Feature<NationalRoadFeatureCompareAttributes>(recordNumber, new NationalRoadFeatureCompareAttributes
            {
                Number = dbaseRecord.IDENT2.Value,
                Id = dbaseRecord.NW_OIDN.Value,
                RoadSegmentId = dbaseRecord.WS_OIDN.Value
            });
        }
    }

    private sealed class UploadsV1FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentNationalRoadAttributeDbaseRecord, Feature<NationalRoadFeatureCompareAttributes>>
    {
        public UploadsV1FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentNationalRoadAttributeDbaseRecord.Schema)
        {
        }

        protected override Feature<NationalRoadFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentNationalRoadAttributeDbaseRecord dbaseRecord)
        {
            return new Feature<NationalRoadFeatureCompareAttributes>(recordNumber, new NationalRoadFeatureCompareAttributes
            {
                Number = dbaseRecord.IDENT2.Value,
                Id = dbaseRecord.NW_OIDN.Value,
                RoadSegmentId = dbaseRecord.WS_OIDN.Value
            });
        }
    }
}
