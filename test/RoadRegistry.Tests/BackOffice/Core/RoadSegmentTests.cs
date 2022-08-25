namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using Xunit;

public class RoadSegmentTests
{
    private readonly AttributeHash _attributeHash;
    private readonly RoadNodeId _end;
    private readonly MultiLineString _geometry;
    private readonly RoadSegmentId _id;
    private readonly RoadNodeId _start;
    private readonly RoadSegment _sut;

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
        fixture.CustomizeAttributeHash();
        _id = fixture.Create<RoadSegmentId>();
        _start = fixture.Create<RoadNodeId>();
        _end = fixture.Create<RoadNodeId>();
        _geometry = fixture.Create<MultiLineString>();
        _attributeHash = fixture.Create<AttributeHash>();
        _sut = new RoadSegment(_id, _geometry, _start, _end, _attributeHash);
    }

    [Fact]
    public void IdReturnsExpectedResult()
    {
        Assert.Equal(_id, _sut.Id);
    }

    [Fact]
    public void GeometryReturnsExpectedResult()
    {
        Assert.Equal(_geometry, _sut.Geometry);
    }

    [Fact]
    public void StartReturnsExpectedResult()
    {
        Assert.Equal(_start, _sut.Start);
    }

    [Fact]
    public void EndReturnsExpectedResult()
    {
        Assert.Equal(_end, _sut.End);
    }

    [Fact]
    public void AttributeHashReturnsExpectedResult()
    {
        Assert.Equal(_attributeHash, _sut.AttributeHash);
    }

    [Fact]
    public void ThrowsWhenStartIsSameAsEnd()
    {
        Assert.Throws<ArgumentException>(() => new RoadSegment(_id, _geometry, _start, _start, _attributeHash));
    }

    [Fact]
    public void NodesReturnsExpectedResult()
    {
        Assert.Equal(new[] { _start, _end }, _sut.Nodes);
    }

    [Theory]
    [InlineData(1, 2, 1, new[] { 2 })]
    [InlineData(1, 2, 2, new[] { 1 })]
    [InlineData(1, 2, 3, new int[0])]
    public void SelectOppositeNodeReturnsExpectedResult(
        int start,
        int end,
        int node,
        int[] opposite
    )
    {
        var sut = new RoadSegment(
            _id,
            _geometry,
            new RoadNodeId(start),
            new RoadNodeId(end),
            _attributeHash);

        var result = sut.SelectOppositeNode(new RoadNodeId(node)).ToArray();

        var expected = Array.ConvertAll(opposite, value => new RoadNodeId(value));
        Assert.Equal(expected, result);
    }
}
