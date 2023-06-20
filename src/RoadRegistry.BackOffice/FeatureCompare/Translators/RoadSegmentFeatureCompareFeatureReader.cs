namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts;
using Extracts.Dbase.RoadSegments;
using NetTopologySuite.Geometries;

public class RoadSegmentFeatureCompareFeatureReader : VersionedFeatureReader<Feature<RoadSegmentFeatureCompareAttributes>>
{
    public RoadSegmentFeatureCompareFeatureReader(Encoding encoding)
        : base(new ExtractsFeatureReader(encoding),
            new UploadsV2FeatureReader(encoding),
            new UploadsV1FeatureReader(encoding))
    {
    }

    public override List<Feature<RoadSegmentFeatureCompareAttributes>> Read(IReadOnlyCollection<ZipArchiveEntry> entries, FeatureType featureType, ExtractFileName fileName)
    {
        var features = base.Read(entries, featureType, fileName);

        var shpFileName = featureType.GetShpFileName(fileName);
        var shpEntry = entries.Single(x => x.Name.Equals(shpFileName, StringComparison.InvariantCultureIgnoreCase));
        if (shpEntry is null)
        {
            throw new FileNotFoundException($"File '{shpFileName}' was not found in zip archive", shpFileName);
        }

        var shpReader = new ZipArchiveShapeFileReader();
        foreach (var (geometry, recordNumber) in shpReader.Read(shpEntry))
        {
            var multiLineString = geometry as MultiLineString;
            if (multiLineString is null)
            {
                var lineString = (LineString)geometry;
                multiLineString = new MultiLineString(new[] { lineString }, lineString.Factory)
                {
                    SRID = lineString.SRID
                };
            }

            var feature = features.Single(x => x.RecordNumber.Equals(recordNumber));
            feature.Attributes.Geometry = multiLineString;
        }

        var featuresWithoutGeometry = features.Where(x => x.Attributes.Geometry is null).ToArray();
        if (featuresWithoutGeometry.Any())
        {
            throw new InvalidOperationException($"{featuresWithoutGeometry.Length} {fileName} records have no geometry");
        }

        return features;
    }

    private sealed class ExtractsFeatureReader : DbaseFeatureReader<RoadSegmentDbaseRecord, Feature<RoadSegmentFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, RoadSegmentDbaseRecord.Schema)
        {
        }

        protected override Feature<RoadSegmentFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, RoadSegmentDbaseRecord dbaseRecord)
        {
            return new Feature<RoadSegmentFeatureCompareAttributes>(recordNumber, new RoadSegmentFeatureCompareAttributes
            {
                StartNodeId = dbaseRecord.B_WK_OIDN.HasValue ? dbaseRecord.B_WK_OIDN.Value : 0,
                MaintenanceAuthority = dbaseRecord.BEHEER.Value,
                EndNodeId = dbaseRecord.E_WK_OIDN.HasValue ? dbaseRecord.E_WK_OIDN.Value : 0,
                LeftStreetNameId = dbaseRecord.LSTRNMID.Value,
                Method = dbaseRecord.METHODE.Value,
                Morphology = dbaseRecord.MORF.Value,
                RightStreetNameId = dbaseRecord.RSTRNMID.Value,
                Status = dbaseRecord.STATUS.Value,
                AccessRestriction = dbaseRecord.TGBEP.Value,
                Category = dbaseRecord.WEGCAT.Value,
                Id = dbaseRecord.WS_OIDN.Value
            });
        }
    }

    private sealed class UploadsV2FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentDbaseRecord, Feature<RoadSegmentFeatureCompareAttributes>>
    {
        public UploadsV2FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentDbaseRecord.Schema)
        {
        }

        protected override Feature<RoadSegmentFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadSegmentDbaseRecord dbaseRecord)
        {
            return new Feature<RoadSegmentFeatureCompareAttributes>(recordNumber, new RoadSegmentFeatureCompareAttributes
            {
                StartNodeId = dbaseRecord.B_WK_OIDN.HasValue ? dbaseRecord.B_WK_OIDN.Value : 0,
                MaintenanceAuthority = dbaseRecord.BEHEER.Value,
                EndNodeId = dbaseRecord.E_WK_OIDN.HasValue ? dbaseRecord.E_WK_OIDN.Value : 0,
                LeftStreetNameId = dbaseRecord.LSTRNMID.Value,
                Method = dbaseRecord.METHODE.Value,
                Morphology = dbaseRecord.MORF.Value,
                RightStreetNameId = dbaseRecord.RSTRNMID.Value,
                Status = dbaseRecord.STATUS.Value,
                AccessRestriction = dbaseRecord.TGBEP.Value,
                Category = dbaseRecord.WEGCAT.Value,
                Id = dbaseRecord.WS_OIDN.Value
            });
        }
    }

    private sealed class UploadsV1FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentDbaseRecord, Feature<RoadSegmentFeatureCompareAttributes>>
    {
        public UploadsV1FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentDbaseRecord.Schema)
        {
        }

        protected override Feature<RoadSegmentFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadSegmentDbaseRecord dbaseRecord)
        {
            return new Feature<RoadSegmentFeatureCompareAttributes>(recordNumber, new RoadSegmentFeatureCompareAttributes
            {
                StartNodeId = dbaseRecord.B_WK_OIDN.HasValue ? dbaseRecord.B_WK_OIDN.Value : 0,
                MaintenanceAuthority = dbaseRecord.BEHEER.Value,
                EndNodeId = dbaseRecord.E_WK_OIDN.HasValue ? dbaseRecord.E_WK_OIDN.Value : 0,
                LeftStreetNameId = dbaseRecord.LSTRNMID.Value,
                Method = dbaseRecord.METHODE.Value,
                Morphology = dbaseRecord.MORF.Value,
                RightStreetNameId = dbaseRecord.RSTRNMID.Value,
                Status = dbaseRecord.STATUS.Value,
                AccessRestriction = dbaseRecord.TGBEP.Value,
                Category = dbaseRecord.WEGCAT.Value,
                Id = dbaseRecord.WS_OIDN.Value
            });
        }
    }
}
