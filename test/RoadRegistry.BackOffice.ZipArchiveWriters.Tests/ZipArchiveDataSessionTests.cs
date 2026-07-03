namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests;

using System.Threading;
using System.Threading.Tasks;
using Marten;
using NetTopologySuite.Geometries;
using NodaTime;
using RoadRegistry.Extracts.Projections;
using RoadRegistry.Extracts.ZipArchiveWriters;
using RoadRegistry.Extensions;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests;

public class ZipArchiveDataSessionTests
{
    private static readonly StoreOptions _storeOptions = new StoreOptions().ConfigureSerializer();
    private static readonly EventTimestamp _timestamp = new(Instant.MinValue, new OrganizationId("TEST"));

    [Fact]
    public async Task GetRoadSegments_SnapsStartVertexToStartNodeGeometry()
    {
        var nodePoint = CreatePoint(10.0, 20.0);
        var node = BuildRoadNode(1, nodePoint);
        var segment = BuildRoadSegment(100, [new Coordinate(10.005, 20.0), new Coordinate(30.0, 20.0)],
            startNodeId: new RoadNodeId(1), endNodeId: null);

        IDocumentSession session = new InMemoryDocumentStoreSession(_storeOptions);
        session.Store(node);
        session.Store(segment);

        var repo = new FakeRoadNetworkRepository(
            new RoadNetworkIds([new RoadNodeId(1)], [new RoadSegmentId(100)], [], []));

        var sut = new ZipArchiveDataSession(session, repo, null!);
        var segments = await sut.GetRoadSegments(CreateContour(), CancellationToken.None);

        var ls = segments.Single().Geometry.Value.GetSingleLineString();
        Assert.Equal(10.0, ls.Coordinates[0].X);
        Assert.Equal(20.0, ls.Coordinates[0].Y);
        Assert.Equal(30.0, ls.Coordinates[^1].X);
        Assert.Equal(20.0, ls.Coordinates[^1].Y);
    }

    [Fact]
    public async Task GetRoadSegments_SnapsEndVertexToEndNodeGeometry()
    {
        var nodePoint = CreatePoint(30.0, 20.0);
        var node = BuildRoadNode(2, nodePoint);
        var segment = BuildRoadSegment(100, [new Coordinate(10.0, 20.0), new Coordinate(30.005, 20.0)],
            startNodeId: null, endNodeId: new RoadNodeId(2));

        IDocumentSession session = new InMemoryDocumentStoreSession(_storeOptions);
        session.Store(node);
        session.Store(segment);

        var repo = new FakeRoadNetworkRepository(
            new RoadNetworkIds([new RoadNodeId(2)], [new RoadSegmentId(100)], [], []));

        var sut = new ZipArchiveDataSession(session, repo, null!);
        var segments = await sut.GetRoadSegments(CreateContour(), CancellationToken.None);

        var ls = segments.Single().Geometry.Value.GetSingleLineString();
        Assert.Equal(10.0, ls.Coordinates[0].X);
        Assert.Equal(20.0, ls.Coordinates[0].Y);
        Assert.Equal(30.0, ls.Coordinates[^1].X);
        Assert.Equal(20.0, ls.Coordinates[^1].Y);
    }

    [Fact]
    public async Task GetRoadSegments_DoesNotModifyWhenVerticesAlreadyMatchNode()
    {
        var nodePoint = CreatePoint(10.0, 20.0);
        var node = BuildRoadNode(1, nodePoint);
        var segment = BuildRoadSegment(100, [new Coordinate(10.0, 20.0), new Coordinate(30.0, 20.0)],
            startNodeId: new RoadNodeId(1), endNodeId: null);
        var originalWkt = segment.Geometry.Value.AsText();

        IDocumentSession session = new InMemoryDocumentStoreSession(_storeOptions);
        session.Store(node);
        session.Store(segment);

        var repo = new FakeRoadNetworkRepository(
            new RoadNetworkIds([new RoadNodeId(1)], [new RoadSegmentId(100)], [], []));

        var sut = new ZipArchiveDataSession(session, repo, null!);
        var segments = await sut.GetRoadSegments(CreateContour(), CancellationToken.None);

        Assert.Equal(originalWkt, segments.Single().Geometry.Value.AsText());
    }

    [Fact]
    public async Task GetRoadSegments_DoesNotSnapOutlinedSegmentWithNullNodeIds()
    {
        var segment = BuildRoadSegment(100, [new Coordinate(10.005, 20.0), new Coordinate(30.005, 20.0)],
            startNodeId: null, endNodeId: null);
        var originalWkt = segment.Geometry.Value.AsText();

        IDocumentSession session = new InMemoryDocumentStoreSession(_storeOptions);
        session.Store(segment);

        var repo = new FakeRoadNetworkRepository(
            new RoadNetworkIds([], [new RoadSegmentId(100)], [], []));

        var sut = new ZipArchiveDataSession(session, repo, null!);
        var segments = await sut.GetRoadSegments(CreateContour(), CancellationToken.None);

        Assert.Equal(originalWkt, segments.Single().Geometry.Value.AsText());
    }

    private static Point CreatePoint(double x, double y)
    {
        return new Point(x, y) { SRID = WellknownSrids.Lambert08 };
    }

    private static RoadNodeExtractItem BuildRoadNode(int id, Point point)
    {
        return new RoadNodeExtractItem
        {
            RoadNodeId = new RoadNodeId(id),
            Geometry = RoadNodeGeometry.Create(point),
            Type = "realNode",
            Grensknoop = false,
            IsV2 = true,
            Origin = _timestamp,
            LastModified = _timestamp
        };
    }

    private static RoadSegmentExtractItem BuildRoadSegment(int id, Coordinate[] coords, RoadNodeId? startNodeId, RoadNodeId? endNodeId)
    {
        var factory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(WellknownSrids.Lambert08);
        var geometry = RoadSegmentGeometry.Create(factory.CreateLineString(coords).ToMultiLineString());

        return new RoadSegmentExtractItem
        {
            RoadSegmentId = new RoadSegmentId(id),
            Geometry = geometry,
            StartNodeId = startNodeId,
            EndNodeId = endNodeId,
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
            Origin = _timestamp,
            LastModified = _timestamp,
            IsV2 = true
        };
    }

    private static Polygon CreateContour()
    {
        var factory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(WellknownSrids.Lambert08);
        var envelope = new Envelope(0, 100, 0, 100);
        return (Polygon)factory.ToGeometry(envelope);
    }

    private sealed class FakeRoadNetworkRepository : IRoadNetworkRepository
    {
        private readonly RoadNetworkIds _ids;

        public FakeRoadNetworkRepository(RoadNetworkIds ids) => _ids = ids;

        public Task<RoadNetworkIds> GetUnderlyingIdsForExtract(Marten.IDocumentSession session, Geometry geometry)
            => Task.FromResult(_ids);

        public Task<RoadNetworkIds> GetUnderlyingIds(Marten.IDocumentSession session, Geometry? geometry = null, RoadNetworkIds? ids = null)
            => throw new NotImplementedException();

        public Task<RoadNetworkIds> GetUnderlyingIdsWithConnectedSegments(Marten.IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
            => throw new NotImplementedException();

        public Task<ScopedRoadNetwork> Load(Marten.IDocumentSession session, RoadNetworkIds ids, ScopedRoadNetworkId roadNetworkId)
            => throw new NotImplementedException();

        public Task Save(ScopedRoadNetwork roadNetwork, string commandName, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public void Save(Marten.IDocumentSession session, ScopedRoadNetwork roadNetwork, string commandName)
            => throw new NotImplementedException();
    }
}
