namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extracts;
using Extracts.Dbase.RoadNodes;
using Point = NetTopologySuite.Geometries.Point;

public class RoadNodeFeatureCompareFeatureReader : VersionedFeatureReader<Feature<RoadNodeFeatureCompareAttributes>>
{
    public RoadNodeFeatureCompareFeatureReader(Encoding encoding)
        : base(new ExtractsFeatureReader(encoding),
            new UploadsV2FeatureReader(encoding),
            new UploadsV1FeatureReader(encoding))
    {
    }

    public override List<Feature<RoadNodeFeatureCompareAttributes>> Read(IReadOnlyCollection<ZipArchiveEntry> entries, FeatureType featureType, ExtractFileName fileName)
    {
        var features = base.Read(entries, featureType, fileName);

        var shpFileName = featureType.GetShpFileName(fileName);
        var shpEntry = entries.SingleOrDefault(x => x.Name.Equals(shpFileName, StringComparison.InvariantCultureIgnoreCase));
        if (shpEntry is null)
        {
            throw new FileNotFoundException($"File '{shpFileName}' was not found in zip archive", shpFileName);
        }

        var shpReader = new ZipArchiveShapeFileReader();
        foreach (var (geometry, recordNumber) in shpReader.Read(shpEntry))
        {
            var feature = features.Single(x => x.RecordNumber.Equals(recordNumber));
            feature.Attributes.Geometry = (Point)geometry;
        }

        var featuresWithoutGeometry = features.Where(x => x.Attributes.Geometry is null).ToArray();
        if (featuresWithoutGeometry.Any())
        {
            throw new InvalidOperationException($"{featuresWithoutGeometry.Length} {fileName} records have no geometry");
        }

        return features;
    }

    private sealed class ExtractsFeatureReader : DbaseFeatureReader<RoadNodeDbaseRecord, Feature<RoadNodeFeatureCompareAttributes>>
    {
        public ExtractsFeatureReader(Encoding encoding)
            : base(encoding, RoadNodeDbaseRecord.Schema)
        {
        }

        protected override Feature<RoadNodeFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, RoadNodeDbaseRecord dbaseRecord)
        {
            return new Feature<RoadNodeFeatureCompareAttributes>(recordNumber, new RoadNodeFeatureCompareAttributes
            {
                Type = dbaseRecord.TYPE.Value,
                Id = dbaseRecord.WK_OIDN.Value
            });
        }
    }

    private sealed class UploadsV2FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadNodeDbaseRecord, Feature<RoadNodeFeatureCompareAttributes>>
    {
        public UploadsV2FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadNodeDbaseRecord.Schema)
        {
        }

        protected override Feature<RoadNodeFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V2.Schema.RoadNodeDbaseRecord dbaseRecord)
        {
            return new Feature<RoadNodeFeatureCompareAttributes>(recordNumber, new RoadNodeFeatureCompareAttributes
            {
                Type = dbaseRecord.TYPE.Value,
                Id = dbaseRecord.WK_OIDN.Value
            });
        }
    }

    private sealed class UploadsV1FeatureReader : DbaseFeatureReader<Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadNodeDbaseRecord, Feature<RoadNodeFeatureCompareAttributes>>
    {
        public UploadsV1FeatureReader(Encoding encoding)
            : base(encoding, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadNodeDbaseRecord.Schema)
        {
        }

        protected override Feature<RoadNodeFeatureCompareAttributes> ConvertDbfRecordToFeature(RecordNumber recordNumber, Uploads.Dbase.BeforeFeatureCompare.V1.Schema.RoadNodeDbaseRecord dbaseRecord)
        {
            return new Feature<RoadNodeFeatureCompareAttributes>(recordNumber, new RoadNodeFeatureCompareAttributes
            {
                Type = dbaseRecord.TYPE.Value,
                Id = dbaseRecord.WK_OIDN.Value
            });
        }
    }
}
