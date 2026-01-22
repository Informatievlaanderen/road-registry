namespace RoadRegistry.Extracts.ZipArchiveWriters.Writers.DomainV2;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Geometries;
using Projections;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Infrastructure.ShapeFile;
using RoadRegistry.Extracts.Schemas.DomainV2.RoadNodes;
using ShapeType = NetTopologySuite.IO.Esri.ShapeType;

public class RoadNodesZipArchiveWriter : IZipArchiveWriter
{
    private readonly Encoding _encoding;

    public RoadNodesZipArchiveWriter(Encoding encoding)
    {
        _encoding = encoding.ThrowIfNull();
    }

    public async Task WriteAsync(
        ZipArchive archive,
        RoadNetworkExtractAssemblyRequest request,
        IZipArchiveDataProvider zipArchiveDataProvider,
        ZipArchiveWriteContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(zipArchiveDataProvider);

        var nodes = await zipArchiveDataProvider.GetRoadNodes(request.Contour, cancellationToken);

        const ExtractFileName extractFilename = ExtractFileName.Wegknoop;
        FeatureType[] featureTypes = request.IsInformative
            ? [FeatureType.Extract]
            : [FeatureType.Extract, FeatureType.Change];

        var writer = new ShapeFileRecordWriter(_encoding);

        foreach (var featureType in featureTypes)
        {
            var records = ConvertToDbaseRecords(nodes);

            await writer.WriteToArchive(archive, extractFilename, featureType, ShapeType.Point, RoadNodeDbaseRecord.Schema, records, cancellationToken);
        }
    }

    internal static IEnumerable<(DbaseRecord, Geometry)> ConvertToDbaseRecords(IEnumerable<RoadNodeExtractItem> nodes)
    {
        return nodes
            .OrderBy(record => record.Id)
            .Select(x =>
            {
                var dbfRecord = new RoadNodeDbaseRecord
                {
                    WK_OIDN = { Value = x.RoadNodeId },
                    TYPE = { Value = x.IsV2 ? RoadNodeTypeV2.Parse(x.Type).Translation.Identifier : MigrateRoadNodeType(RoadNodeType.Parse(x.Type)) },
                    GRENSKNOOP = { Value = ToGrensknoopDbfValue(x.Grensknoop, x.Geometry, x.IsV2) },
                    CREATIE = { Value = x.Origin.Timestamp.ToBrusselsDateTime() },
                    VERSIE = { Value = x.LastModified.Timestamp.ToBrusselsDateTime() }
                };

                return ((DbaseRecord)dbfRecord, (Geometry)x.Geometry.Value);
            });
    }

    private static int MigrateRoadNodeType(RoadNodeType v1)
    {
        var mapping = new Dictionary<int, int>
        {
            { 1, 1 },
            { 2, 2 },
            { 3, 3 },
            { 4, 1 },
            { 5, 4 }
        };

        if (mapping.TryGetValue(v1.Translation.Identifier, out var v2))
        {
            return v2;
        }

        throw new NotSupportedException(v1.ToString());
    }

    private static short ToGrensknoopDbfValue(bool grensknoop, RoadNodeGeometry geometry, bool isV2)
    {
        return isV2
            ? grensknoop.ToDbaseShortValue()
            : IsWithin10MeterOfGrens(geometry)
                ? (short)0
                : (short)-8;
    }

    private static bool IsWithin10MeterOfGrens(RoadNodeGeometry geometry)
    {
        //TODO-pr indien x.Grensknoop null, dan:
        //‘-8’ indien >10m van de gewestgrens)
        //'0' indien ≤10m van de gewestgrens
        throw new NotImplementedException();
    }
}
