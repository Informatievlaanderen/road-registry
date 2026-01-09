namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadSegmentSurfaceAttribute = RoadRegistry.BackOffice.Core.RoadSegmentSurfaceAttribute;

public class RoadSegmentSurfaceAttributeTests
{
    private readonly Fixture _fixture;

    public RoadSegmentSurfaceAttributeTests()
    {
        _fixture = FixtureFactory.Create();
        _fixture.CustomizeAttributeId();
        _fixture.CustomizeRoadSegmentSurfaceType();
        _fixture.CustomizeRoadSegmentPosition();
        _fixture.CustomizeRoadSegmentGeometryVersion();
    }

    [Fact]
    public void IsDynamicRoadSegmentAttribute()
    {
        _fixture.CustomizeRoadSegmentSurfaceAttribute();

        var sut = _fixture.Create<RoadSegmentSurfaceAttribute>();

        Assert.IsAssignableFrom<DynamicRoadSegmentAttribute>(sut);
    }

    [Fact]
    public void PropertiesReturnExpectedResult()
    {
        var generator = new Generator<RoadSegmentPosition>(_fixture);
        var attributeId = _fixture.Create<AttributeId>();
        var temporaryId = _fixture.Create<AttributeId>();
        var type = _fixture.Create<RoadSegmentSurfaceType>();
        var from = generator.First();
        var to = generator.First(candidate => candidate > from);
        var asOfGeometryVersion = _fixture.Create<GeometryVersion>();

        var sut = new RoadSegmentSurfaceAttribute(
            attributeId,
            temporaryId,
            type,
            from,
            to,
            asOfGeometryVersion
        );

        Assert.Equal(attributeId, sut.Id);
        Assert.Equal(type, sut.Type);
        Assert.Equal(from, sut.From);
        Assert.Equal(to, sut.To);
        Assert.Equal(asOfGeometryVersion, sut.AsOfGeometryVersion);
    }
}
