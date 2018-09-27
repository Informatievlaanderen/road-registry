namespace RoadRegistry.Model
{
    using System.Linq;
    using AutoFixture;
    using Xunit;

    public class RoadSegmentHardeningAttributeTests
    {
        private readonly Fixture _fixture;

        public RoadSegmentHardeningAttributeTests()
        {
            _fixture = new Fixture();    
            _fixture.CustomizeRoadSegmentHardeningType();
            _fixture.CustomizeRoadSegmentPosition();
            _fixture.CustomizeRoadSegmentGeometryVersion();
        }

        [Fact]
        public void IsDynamicRoadSegmentAttribute()
        {
            _fixture.CustomizeRoadSegmentHardeningAttribute();

            var sut = _fixture.Create<RoadSegmentHardeningAttribute>();

            Assert.IsAssignableFrom<DynamicRoadSegmentAttribute>(sut);
        }

        [Fact]
        public void PropertiesReturnExpectedResult()
        {
            var generator = new Generator<RoadSegmentPosition>(_fixture);
            var type = _fixture.Create<RoadSegmentHardeningType>();
            var from = generator.First();
            var to = generator.First(candidate => candidate > from);
            var asOfGeometryVersion = _fixture.Create<GeometryVersion>();

            var sut = new RoadSegmentHardeningAttribute(
                type,
                from,
                to,
                asOfGeometryVersion
            );

            Assert.Equal(type, sut.Type);
            Assert.Equal(from, sut.From);
            Assert.Equal(to, sut.To);
            Assert.Equal(asOfGeometryVersion, sut.AsOfGeometryVersion);
        }
    }   
}