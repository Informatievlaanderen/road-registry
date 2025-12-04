namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadSegmentWidthAttribute = RoadRegistry.BackOffice.Core.RoadSegmentWidthAttribute;

public class RoadSegmentWidthAttributeTests
{
    private readonly Fixture _fixture;

    public RoadSegmentWidthAttributeTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeAttributeId();
        _fixture.CustomizeRoadSegmentWidth();
        _fixture.CustomizeRoadSegmentPosition();
        _fixture.CustomizeRoadSegmentGeometryVersion();
    }

    [Fact]
    public void IsDynamicRoadSegmentAttribute()
    {
        _fixture.CustomizeRoadSegmentWidthAttribute();

        var sut = _fixture.Create<RoadSegmentWidthAttribute>();

        Assert.IsAssignableFrom<DynamicRoadSegmentAttribute>(sut);
    }

    [Fact]
    public void PropertiesReturnExpectedResult()
    {
        var generator = new Generator<RoadSegmentPosition>(_fixture);
        var attributeId = _fixture.Create<AttributeId>();
        var temporaryId = _fixture.Create<AttributeId>();
        var width = _fixture.Create<RoadSegmentWidth>();
        var from = generator.First();
        var to = generator.First(candidate => candidate > from);
        var asOfGeometryVersion = _fixture.Create<GeometryVersion>();

        var sut = new RoadSegmentWidthAttribute(
            attributeId,
            temporaryId,
            width,
            from,
            to,
            asOfGeometryVersion
        );

        Assert.Equal(attributeId, sut.Id);
        Assert.Equal(width, sut.Width);
        Assert.Equal(from, sut.From);
        Assert.Equal(to, sut.To);
        Assert.Equal(asOfGeometryVersion, sut.AsOfGeometryVersion);
    }
}
