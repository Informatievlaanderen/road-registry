namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;

public class RoadSegmentTests
{
    private readonly AttributeHash _attributeHash;
    private readonly RoadNodeId _end;
    private readonly MultiLineString _geometry;
    private readonly GeometryVersion _geometryVersion;
    private readonly RoadSegmentId _id;
    private readonly RoadNodeId _start;
    private readonly RoadSegment _sut;
    private readonly RoadSegmentVersion _version;

    public RoadSegmentTests()
    {
        var fixture = new Fixture();
        fixture.CustomizePolylineM();
        fixture.CustomizeRoadNodeId();
        fixture.CustomizeRoadSegmentId();
        fixture.CustomizeRoadSegmentCategory();
        fixture.CustomizeRoadSegmentMorphology();
        fixture.CustomizeRoadSegmentStatus();
        fixture.CustomizeRoadSegmentAccessRestriction();
        fixture.CustomizeOrganizationId();
        fixture.CustomizeRoadSegmentLaneCount();
        fixture.CustomizeRoadSegmentLaneDirection();
        fixture.CustomizeRoadSegmentLaneAttribute();
        fixture.CustomizeRoadSegmentSurfaceType();
        fixture.CustomizeRoadSegmentSurfaceAttribute();
        fixture.CustomizeRoadSegmentWidth();
        fixture.CustomizeRoadSegmentWidthAttribute();
        fixture.CustomizeAttributeHash();
        fixture.CustomizeRoadSegmentGeometryDrawMethod();
        _id = fixture.Create<RoadSegmentId>();
        _version = fixture.Create<RoadSegmentVersion>();
        _start = fixture.Create<RoadNodeId>();
        _end = fixture.Create<RoadNodeId>();
        _geometry = fixture.Create<MultiLineString>();
        _geometryVersion = fixture.Create<GeometryVersion>();
        _attributeHash = fixture.Create<AttributeHash>();
        _sut = new RoadSegment(_id, _version, _geometry, _geometryVersion, _start, _end, _attributeHash, null);
    }

    [Fact]
    public void AttributeHashReturnsExpectedResult()
    {
        Assert.Equal(_attributeHash, _sut.AttributeHash);
    }

    [Fact]
    public void EndReturnsExpectedResult()
    {
        Assert.Equal(_end, _sut.End);
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
    public void NodesReturnsExpectedResult()
    {
        Assert.Equal(new[] { _start, _end }, _sut.Nodes);
    }

    [Theory]
    [InlineData(1, 2, 1, 2)]
    [InlineData(1, 2, 2, 1)]
    [InlineData(1, 2, 3, null)]
    public void GetOppositeNodeReturnsExpectedResult(
        int start,
        int end,
        int node,
        int? opposite
    )
    {
        var sut = new RoadSegment(
            _id,
            _version,
            _geometry,
            _geometryVersion,
            new RoadNodeId(start),
            new RoadNodeId(end),
            _attributeHash,
            null);

        var result = sut.GetOppositeNode(new RoadNodeId(node));

        RoadNodeId? expected = opposite is not null
            ? new RoadNodeId(opposite.Value)
            : null;

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, 2, 2, 3, 2)]
    [InlineData(1, 2, 3, 2, 2)]
    [InlineData(2, 1, 2, 3, 2)]
    [InlineData(2, 1, 3, 2, 2)]
    [InlineData(1, 2, 3, 4, null)]
    public void GetCommonNodeReturnsExpectedResult(
        int segment1Start,
        int segment1End,
        int segment2Start,
        int segment2End,
        int? expectedCommonNode
    )
    {
        var segment1 = new RoadSegment(
            _id,
            _version,
            _geometry,
            _geometryVersion,
            new RoadNodeId(segment1Start),
            new RoadNodeId(segment1End),
            _attributeHash,
            null);
        var segment2 = new RoadSegment(
            _id,
            _version,
            _geometry,
            _geometryVersion,
            new RoadNodeId(segment2Start),
            new RoadNodeId(segment2End),
            _attributeHash,
            null);

        var commonNode = segment1.GetCommonNode(segment2);

        RoadNodeId? expected = expectedCommonNode is not null
            ? new RoadNodeId(expectedCommonNode.Value)
            : null;

        commonNode.Should().Be(expected);
    }

    [Fact]
    public void StartReturnsExpectedResult()
    {
        Assert.Equal(_start, _sut.Start);
    }

    [Fact]
    public void ThrowsWhenStartIsSameAsEnd()
    {
        Assert.Throws<ArgumentException>(() => new RoadSegment(_id, _version, _geometry, _geometryVersion, _start, _start, _attributeHash, null));
    }
}
