namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadSegmentLaneAttribute = RoadRegistry.BackOffice.Core.RoadSegmentLaneAttribute;

public class RoadSegmentLaneAttributeTests
{
    private readonly Fixture _fixture;

    public RoadSegmentLaneAttributeTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeAttributeId();
        _fixture.CustomizeRoadSegmentLaneCount();
        _fixture.CustomizeRoadSegmentLaneDirection();
        _fixture.CustomizeRoadSegmentPosition();
        _fixture.CustomizeRoadSegmentGeometryVersion();
    }

    [Fact]
    public void IsDynamicRoadSegmentAttribute()
    {
        _fixture.CustomizeRoadSegmentLaneAttribute();

        var sut = _fixture.Create<RoadSegmentLaneAttribute>();

        Assert.IsAssignableFrom<DynamicRoadSegmentAttribute>(sut);
    }

    [Fact]
    public void PropertiesReturnExpectedResult()
    {
        var generator = new Generator<RoadSegmentPosition>(_fixture);
        var attributeId = _fixture.Create<AttributeId>();
        var temporaryId = _fixture.Create<AttributeId>();
        var laneCount = _fixture.Create<RoadSegmentLaneCount>();
        var laneDirection = _fixture.Create<RoadSegmentLaneDirection>();
        var from = generator.First();
        var to = generator.First(candidate => candidate > from);
        var asOfGeometryVersion = _fixture.Create<GeometryVersion>();

        var sut = new RoadSegmentLaneAttribute(
            attributeId,
            temporaryId,
            laneCount,
            laneDirection,
            from,
            to,
            asOfGeometryVersion
        );

        Assert.Equal(attributeId, sut.Id);
        Assert.Equal(laneCount, sut.Count);
        Assert.Equal(laneDirection, sut.Direction);
        Assert.Equal(from, sut.From);
        Assert.Equal(to, sut.To);
        Assert.Equal(asOfGeometryVersion, sut.AsOfGeometryVersion);
    }
}
