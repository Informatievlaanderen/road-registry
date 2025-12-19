namespace RoadRegistry.BackOffice.ZipArchiveWriters.DomainV2.Writers;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Utilities;
using NetTopologySuite.Geometries;
using RoadNode;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Infrastructure.ShapeFile;
using RoadRegistry.Extracts.Schemas.ExtractV2.RoadNodes;
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
                    var dbfRecord = new RoadNodeDbaseRecord
                    {
                        WK_OIDN = { Value = x.RoadNodeId },
                        WK_UIDN = { Value = $"{x.RoadNodeId}_{new Rfc3339SerializableDateTimeOffset(x.LastModified.Timestamp.ToBelgianDateTimeOffset()).ToString()}"},
                        TYPE = { Value = x.Type },
                        LBLTYPE = { Value = x.Type.ToDutchString() },
                        BEGINTIJD = { Value = x.Origin.Timestamp.ToBrusselsDateTime() },
                        BEGINORG = { Value = x.Origin.OrganizationId }
                    };

                    return ((DbaseRecord)dbfRecord, (Geometry)x.Geometry.ToGeometry());
                });

            await writer.WriteToArchive(archive, extractFilename, featureType, ShapeType.Point, RoadNodeDbaseRecord.Schema, records, cancellationToken);
        }
    }
}
