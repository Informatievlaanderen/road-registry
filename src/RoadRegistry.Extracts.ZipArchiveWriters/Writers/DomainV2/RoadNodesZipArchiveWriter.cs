namespace RoadRegistry.Extracts.ZipArchiveWriters.Writers.DomainV2;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Geometries;
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
            var records = nodes
                .OrderBy(record => record.Id)
                .Select(x =>
                {
                    //TODO-pr indien x.Grensknoop null, dan:
                    //‘-8’ indien >10m van de gewestgrens)
                    //'0' indien ≤10m van de gewestgrens
                    short grensknoop = 0;

                    var dbfRecord = new RoadNodeDbaseRecord
                    {
                        WK_OIDN = { Value = x.RoadNodeId },
                        TYPE = { Value = x.Type },
                        GRENSKNOOP = { Value = grensknoop },
                        CREATIE = { Value = x.Origin.Timestamp.ToBrusselsDateTime() },
                        VERSIE = { Value = x.LastModified.Timestamp.ToBrusselsDateTime() },
                    };

                    return ((DbaseRecord)dbfRecord, (Geometry)x.Geometry.Value);
                });

            await writer.WriteToArchive(archive, extractFilename, featureType, ShapeType.Point, RoadNodeDbaseRecord.Schema, records, cancellationToken);
        }
    }

    private RoadNodeType Migrate(RoadNodeType type)
    {
        switch(type)
    }
}
