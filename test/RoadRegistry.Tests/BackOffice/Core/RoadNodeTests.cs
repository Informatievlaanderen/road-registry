namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadSegment.ValueObjects;

public class FullyDisconnectedRoadNodeTests
{
    private readonly Fixture _fixture;
    private readonly Point _geometry;
    private readonly RoadNodeId _id;
    private readonly RoadNode _sut;
    private readonly RoadNodeType _type;
    private readonly RoadNodeVersion _version;

    public FullyDisconnectedRoadNodeTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeRoadNodeId();
        _fixture.CustomizeRoadNodeVersion();
        _fixture.CustomizeRoadNodeType();
        _fixture.CustomizePoint();
        _id = _fixture.Create<RoadNodeId>();
        _version = _fixture.Create<RoadNodeVersion>();
        _type = _fixture.Create<RoadNodeType>();
        _geometry = _fixture.Create<Point>();
        _sut = new RoadNode(_id, _version, _type, _geometry);
    }

    [Fact]
    public void ConnectWithReturnsExpectedResult()
    {
        var link = _fixture.Create<RoadSegmentId>();

        var result = _sut.ConnectWith(link);

        Assert.Equal(_sut.Id, result.Id);
        Assert.Equal(new[] { link }, result.Segments);
    }

    [Fact]
    public void DisconnectFromReturnsExpectedResult()
    {
        var result = _sut.DisconnectFrom(_fixture.Create<RoadSegmentId>());

        Assert.Equal(_sut.Id, result.Id);
        Assert.Empty(result.Segments);
    }

    [Fact]
    public void GeometryReturnsExpectedResult()
    {
        Assert.Equal(_geometry, _sut.Geometry);
    }

    [Fact]
    public void IdReturnsExpectedResult()
    {
        Assert.Equal(_id, _sut.Id);
    }

    [Fact]
    public void SegmentsReturnsExpectedResult()
    {
        Assert.Empty(_sut.Segments);
    }

    [Fact]
    public void TypeReturnsExpectedResult()
    {
        Assert.Equal(_type, _sut.Type);
    }

    [Fact]
    public void WithGeometryReturnsExpectedResult()
    {
        var geometry = new Generator<Point>(_fixture).First(candidate => !candidate.EqualsExact(_geometry));
        var result = _sut.WithGeometry(geometry);
        Assert.Equal(geometry, result.Geometry);
    }

    [Fact]
    public void WithTypeReturnsExpectedResult()
    {
        var type = new Generator<RoadNodeType>(_fixture).First(candidate => !candidate.Equals(_type));
        var result = _sut.WithType(type);
        Assert.Equal(type, result.Type);
    }

    [Fact]
    public void WithVersionReturnsExpectedResult()
    {
        var version = _fixture.CreateWhichIsDifferentThan(_version);
        var result = _sut.WithVersion(version);
        Assert.Equal(version, result.Version);
    }
}

public class ConnectedRoadNodeTests
{
    private readonly Fixture _fixture;
    private readonly Point _geometry;
    private readonly RoadNodeId _id;
    private readonly RoadSegmentId _link1;
    private readonly RoadSegmentId _link2;
    private readonly RoadNode _sut;
    private readonly RoadNodeType _type;
    private readonly RoadNodeVersion _version;

    public ConnectedRoadNodeTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeRoadNodeId();
        _fixture.CustomizeRoadNodeVersion();
        _fixture.CustomizeRoadNodeType();
        _fixture.CustomizePoint();

        _id = _fixture.Create<RoadNodeId>();
        _version = _fixture.Create<RoadNodeVersion>();
        _type = _fixture.Create<RoadNodeType>();
        _geometry = _fixture.Create<Point>();
        _link1 = _fixture.Create<RoadSegmentId>();
        _link2 = _fixture.Create<RoadSegmentId>();
        _sut = new RoadNode(_id, _version, _type, _geometry).ConnectWith(_link1).ConnectWith(_link2);
    }

    [Fact]
    public void ConnectWithReturnsExpectedResult()
    {
        var link = _fixture.Create<RoadSegmentId>();

        var result = _sut.ConnectWith(link);

        Assert.Equal(_sut.Id, result.Id);
        Assert.Equal(new[] { _link1, _link2, link }.OrderBy(_ => _), result.Segments.OrderBy(_ => _));
    }

    [Fact]
    public void DisconnectFromKnownLinkReturnsExpectedResult()
    {
        var result = _sut.DisconnectFrom(_link1);

        Assert.Equal(_sut.Id, result.Id);
        Assert.Equal(new[] { _link2 }, result.Segments);
    }

    [Fact]
    public void DisconnectFromUnknownLinkReturnsExpectedResult()
    {
        var result = _sut.DisconnectFrom(_fixture.Create<RoadSegmentId>());

        Assert.Equal(_sut.Id, result.Id);
        Assert.Equal(new[] { _link1, _link2 }.OrderBy(_ => _), result.Segments.OrderBy(_ => _));
    }

    [Fact]
    public void GeometryReturnsExpectedResult()
    {
        Assert.Equal(_geometry, _sut.Geometry);
    }

    [Fact]
    public void IdReturnsExpectedResult()
    {
        Assert.Equal(_id, _sut.Id);
    }

    [Fact]
    public void SegmentsReturnsExpectedResult()
    {
        Assert.Equal(new[] { _link1, _link2 }.OrderBy(_ => _), _sut.Segments.OrderBy(_ => _));
    }

    [Fact]
    public void TypeReturnsExpectedResult()
    {
        Assert.Equal(_type, _sut.Type);
    }

    [Fact]
    public void WithGeometryReturnsExpectedResult()
    {
        var geometry = new Generator<Point>(_fixture).First(candidate => !candidate.EqualsExact(_geometry));
        var result = _sut.WithGeometry(geometry);
        Assert.Equal(geometry, result.Geometry);
    }

    [Fact]
    public void WithTypeReturnsExpectedResult()
    {
        var type = new Generator<RoadNodeType>(_fixture).First(candidate => !candidate.Equals(_type));
        var result = _sut.WithType(type);
        Assert.Equal(type, result.Type);
    }
}