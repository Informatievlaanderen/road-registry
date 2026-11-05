namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.ExtractZipArchiveWriter.DomainV2;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentAssertions;
using Moq;
using NetTopologySuite.Geometries;
using NodaTime;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Projections;
using RoadRegistry.Extracts.Schemas.DomainV2.RoadNodes;
using RoadRegistry.Extracts.ZipArchiveWriters;
using RoadRegistry.Extracts.ZipArchiveWriters.Writers.DomainV2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using Point = NetTopologySuite.Geometries.Point;

// The example of GAWR-7796, as the bijhouding extract reads it: road segments 1, 2 and 3 are still 'nietGestart' or
// 'locked', road segments 4 to 12 are 'compleet'. The contour holds road segments 3 to 12; 1 and 2 lie outside of it
// and meet 3 at road node C. Road node K connects 3, 4 and 5.
//
//   1 \                    6 /          8 /
//      C ---- 3 ---- K -- 4 -- D -- 7 -- E
//   2 /               \                / |
//                      5 -- F -- 9 ---/  10
//                           |            |
//                           +---- 11 --- G -- 12
public class EditableRoadSegmentsTests
{
    private static readonly EventTimestamp Timestamp = new(Instant.FromUtc(2026, 1, 1, 0, 0), new OrganizationId("TEST"));
    private static readonly GeometryFactory Factory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(WellknownSrids.Lambert08);

    private const int A = 1, B = 2, C = 3, K = 4, D = 5, E = 6, F = 7, G = 8, H6 = 9, H8 = 10, H12 = 11;

    private static readonly Dictionary<int, Coordinate> NodeCoordinates = new()
    {
        { A, new Coordinate(100_000, 200_120) },
        { B, new Coordinate(100_000, 200_020) },
        { C, new Coordinate(100_080, 200_070) },
        { K, new Coordinate(100_175, 200_070) },
        { D, new Coordinate(100_225, 200_120) },
        { E, new Coordinate(100_285, 200_120) },
        { F, new Coordinate(100_230, 200_020) },
        { G, new Coordinate(100_285, 200_020) },
        { H6, new Coordinate(100_190, 200_190) },
        { H8, new Coordinate(100_305, 200_190) },
        { H12, new Coordinate(100_370, 199_980) }
    };

    private static readonly Dictionary<int, (int Start, int End)> Segments = new()
    {
        { 1, (A, C) },
        { 2, (B, C) },
        { 3, (C, K) },
        { 4, (K, D) },
        { 5, (K, F) },
        { 6, (D, H6) },
        { 7, (D, E) },
        { 8, (E, H8) },
        { 9, (F, E) },
        { 10, (E, G) },
        { 11, (F, G) },
        { 12, (G, H12) }
    };

    [Fact]
    public void WhenTicketExample_ThenOnlyTheSegmentsAwayFromTheUnfinishedOnesAreEditable()
    {
        var inContour = BuildRoadSegments(3, 4, 5, 6, 7, 8, 9, 10, 11, 12);
        var connected = BuildRoadSegments(1, 2);

        var result = EditableRoadSegments.Select(inContour, connected, TicketExampleInwinningsstatus());

        result.Select(x => x.ToInt32()).Should().BeEquivalentTo([6, 7, 8, 9, 10, 11, 12]);
    }

    [Fact]
    public void WhenAnUnfinishedSegmentOutsideOfTheContourMeetsOne_ThenThatOneIsNotEditable()
    {
        var inContour = BuildRoadSegments(3, 4, 5, 6, 7, 8, 9, 10, 11, 12);
        var connected = BuildRoadSegments(1, 2).Append(BuildRoadSegment(13, H12, 12)).ToList();
        var inwinningsstatus = TicketExampleInwinningsstatus();
        inwinningsstatus[new RoadSegmentId(13)] = Inwinningsstatus.Locked;

        var result = EditableRoadSegments.Select(inContour, connected, inwinningsstatus);

        result.Select(x => x.ToInt32()).Should().BeEquivalentTo([6, 7, 8, 9, 10, 11]);
    }

    [Fact]
    public void WhenTheStatusOfASegmentIsNotKnown_ThenItIsNotEditable()
    {
        var inContour = BuildRoadSegments(11);

        var result = EditableRoadSegments.Select(inContour, [], new Dictionary<RoadSegmentId, Inwinningsstatus>());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task WhenTicketExample_ThenRoadNodeKGoesIntoTheIntegratiedata()
    {
        var contour = (IPolygonal)Factory.ToGeometry(new Envelope(100_150, 100_355, 199_990, 200_170));
        var inContour = BuildRoadSegments(3, 4, 5, 6, 7, 8, 9, 10, 11, 12);
        var all = BuildRoadSegments(Segments.Keys.ToArray());
        var editableRoadSegmentIds = EditableRoadSegments.Select(inContour, BuildRoadSegments(1, 2), TicketExampleInwinningsstatus());

        var zipArchiveData = new Mock<IZipArchiveDataSession>();
        zipArchiveData
            .Setup(x => x.GetEditableRoadSegmentIds(contour, It.IsAny<CancellationToken>()))
            .ReturnsAsync(editableRoadSegmentIds);
        zipArchiveData
            .Setup(x => x.GetRoadSegments(It.IsAny<IPolygonal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(all);
        zipArchiveData
            .Setup(x => x.GetRoadSegments(contour, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inContour);
        zipArchiveData
            .Setup(x => x.GetRoadNodes(It.IsAny<IPolygonal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildRoadNodes(NodeCoordinates.Keys.ToArray()));
        zipArchiveData
            .Setup(x => x.GetRoadNodes(contour, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildRoadNodes(C, K, D, E, F, G, H6, H8, H12));

        var request = new RoadNetworkExtractAssemblyRequest(
            DownloadId.Parse("099fcc817c4143609a274a00d2d5cabe"),
            new ExtractDescription("GAWR-7796"),
            contour,
            isInformative: false,
            zipArchiveWriterVersion: WellKnownZipArchiveWriterVersions.DomainV2_Bijhouding);
        var context = new ZipArchiveWriteContext();

        using var stream = new MemoryStream();
        using var archive = new ZipArchive(stream, ZipArchiveMode.Update, true, Encoding.UTF8);
        await new RoadNodesZipArchiveWriter(Encoding.UTF8).WriteAsync(archive, request, zipArchiveData.Object, context, CancellationToken.None);

        ReadRoadNodeIds(archive, "eWegknoop.dbf").Should().BeEquivalentTo([D, E, F, G, H6, H8, H12]);
        ReadRoadNodeIds(archive, "Wegknoop.dbf").Should().BeEquivalentTo([D, E, F, G, H6, H8, H12]);
        ReadRoadNodeIds(archive, "iWegknoop.dbf").Should().BeEquivalentTo([A, B, C, K]);
        context.IntegrationSegments!.Select(x => x.RoadSegmentId.ToInt32()).Should().BeEquivalentTo([1, 2, 3, 4, 5]);
    }

    private static Dictionary<RoadSegmentId, Inwinningsstatus> TicketExampleInwinningsstatus()
    {
        return Segments.Keys.ToDictionary(
            id => new RoadSegmentId(id),
            id => id switch
            {
                1 or 3 => Inwinningsstatus.NietGestart,
                2 => Inwinningsstatus.Locked,
                _ => Inwinningsstatus.Compleet
            });
    }

    private static IReadOnlyList<int> ReadRoadNodeIds(ZipArchive archive, string entryName)
    {
        using var stream = archive.GetEntry(entryName)!.Open();
        using var reader = new BinaryReader(stream, Encoding.UTF8);

        var header = DbaseFileHeader.Read(reader, new DbaseFileHeaderReadBehavior(true));
        using var records = header.CreateDbaseRecordEnumerator<RoadNodeDbaseRecord>(reader);
        var ids = new List<int>();
        while (records.MoveNext())
        {
            ids.Add(records.Current!.WK_OIDN.Value);
        }

        return ids;
    }

    private static List<RoadSegmentExtractItem> BuildRoadSegments(params int[] ids)
    {
        return ids.Select(id => BuildRoadSegment(id, Segments[id].Start, Segments[id].End)).ToList();
    }

    private static RoadSegmentExtractItem BuildRoadSegment(int id, int startNodeId, int endNodeId)
    {
        var end = NodeCoordinates.TryGetValue(endNodeId, out var coordinate)
            ? coordinate
            : new Coordinate(NodeCoordinates[startNodeId].X + 50, NodeCoordinates[startNodeId].Y);
        var geometry = RoadSegmentGeometry.Create(Factory.CreateLineString([NodeCoordinates[startNodeId], end]).ToMultiLineString());

        return new RoadSegmentExtractItem
        {
            RoadSegmentId = new RoadSegmentId(id),
            Geometry = geometry,
            StartNodeId = new RoadNodeId(startNodeId),
            EndNodeId = new RoadNodeId(endNodeId),
            GeometryDrawMethod = "Ingemeten",
            Status = "InGebruik",
            AccessRestriction = new ExtractRoadSegmentDynamicAttribute<string>(),
            Category = new ExtractRoadSegmentDynamicAttribute<string>(),
            Morphology = new ExtractRoadSegmentDynamicAttribute<string>(),
            StreetNameId = new ExtractRoadSegmentDynamicAttribute<StreetNameLocalId>(),
            MaintenanceAuthorityId = new ExtractRoadSegmentDynamicAttribute<OrganizationId>(),
            SurfaceType = new ExtractRoadSegmentDynamicAttribute<string>(),
            CarAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(),
            CarAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(),
            BikeAccessForward = new ExtractRoadSegmentDynamicAttribute<bool>(),
            BikeAccessBackward = new ExtractRoadSegmentDynamicAttribute<bool>(),
            PedestrianAccess = new ExtractRoadSegmentDynamicAttribute<bool>(),
            CarTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(),
            BikeTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(),
            PedestrianTrafficDirection = new ExtractRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = [],
            Origin = Timestamp,
            LastModified = Timestamp,
            IsV2 = true
        };
    }

    private static List<RoadNodeExtractItem> BuildRoadNodes(params int[] ids)
    {
        return ids
            .Select(id => new RoadNodeExtractItem
            {
                RoadNodeId = new RoadNodeId(id),
                Geometry = RoadNodeGeometry.Create(new Point(NodeCoordinates[id]) { SRID = WellknownSrids.Lambert08 }),
                Type = "EchteKnoop",
                Grensknoop = false,
                IsV2 = true,
                Origin = Timestamp,
                LastModified = Timestamp
            })
            .ToList();
    }
}
