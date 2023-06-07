namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts.Dbase.GradeSeparatedJuntions;

public class GradeSeparatedJunctionFeatureCompareFeatureReader : VersionedFeatureReader<Feature<GradeSeparatedJunctionFeatureCompareAttributes>>
{
    public GradeSeparatedJunctionFeatureCompareFeatureReader(Encoding encoding)
        : base(new ExtractsFeatureReader(encoding),
            new UploadsV2FeatureReader(encoding),
            new UploadsV1FeatureReader(encoding))
    {
    }

    private sealed class ExtractsFeatureReader : DbaseFeatureReader<GradeSeparatedJunctionDbaseRecord, Feature<GradeSeparatedJunctionFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, GradeSeparatedJunctionDbaseRecord.Schema)
        {
        }

        protected override Feature<GradeSeparatedJunctionFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, GradeSeparatedJunctionDbaseRecord dbaseRecord)
        {
            return new Feature<GradeSeparatedJunctionFeatureCompareAttributes>(recordNumber, new GradeSeparatedJunctionFeatureCompareAttributes
            {
                UpperRoadSegmentId = dbaseRecord.BO_WS_OIDN.Value,
                Id = dbaseRecord.OK_OIDN.Value,
                LowerRoadSegmentId = dbaseRecord.ON_WS_OIDN.Value,
                Type = dbaseRecord.TYPE.Value
            });
        }
    }

    private sealed class UploadsV2FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.GradeSeparatedJunctionDbaseRecord, Feature<GradeSeparatedJunctionFeatureCompareAttributes>>
    {
        public UploadsV2FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.GradeSeparatedJunctionDbaseRecord.Schema)
        {
        }

        protected override Feature<GradeSeparatedJunctionFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.GradeSeparatedJunctionDbaseRecord dbaseRecord)
        {
            return new Feature<GradeSeparatedJunctionFeatureCompareAttributes>(recordNumber, new GradeSeparatedJunctionFeatureCompareAttributes
            {
                UpperRoadSegmentId = dbaseRecord.BO_WS_OIDN.Value,
                Id = dbaseRecord.OK_OIDN.Value,
                LowerRoadSegmentId = dbaseRecord.ON_WS_OIDN.Value,
                Type = dbaseRecord.TYPE.Value
            });
        }
    }

    private sealed class UploadsV1FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.GradeSeparatedJunctionDbaseRecord, Feature<GradeSeparatedJunctionFeatureCompareAttributes>>
    {
        public UploadsV1FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.GradeSeparatedJunctionDbaseRecord.Schema)
        {
        }

        protected override Feature<GradeSeparatedJunctionFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.GradeSeparatedJunctionDbaseRecord dbaseRecord)
        {
            return new Feature<GradeSeparatedJunctionFeatureCompareAttributes>(recordNumber, new GradeSeparatedJunctionFeatureCompareAttributes
            {
                UpperRoadSegmentId = dbaseRecord.BO_WS_OIDN.Value,
                Id = dbaseRecord.OK_OIDN.Value,
                LowerRoadSegmentId = dbaseRecord.ON_WS_OIDN.Value,
                Type = dbaseRecord.TYPE.Value
            });
        }
    }
}
