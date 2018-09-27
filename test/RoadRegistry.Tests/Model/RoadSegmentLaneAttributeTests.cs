namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class RoadSegmentLaneAttributeTests
    {
        private readonly Fixture _fixture;

        public RoadSegmentLaneAttributeTests()
        {
            _fixture = new Fixture();    
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
            var laneCount = _fixture.Create<RoadSegmentLaneCount>();
            var laneDirection = _fixture.Create<RoadSegmentLaneDirection>();
            var from = generator.First();
            var to = generator.First(candidate => candidate > from);
            var asOfGeometryVersion = _fixture.Create<GeometryVersion>();

            var sut = new RoadSegmentLaneAttribute(
                laneCount,
                laneDirection,
                from,
                to,
                asOfGeometryVersion
            );

            Assert.Equal(laneCount, sut.Count);
            Assert.Equal(laneDirection, sut.Direction);
            Assert.Equal(from, sut.From);
            Assert.Equal(to, sut.To);
            Assert.Equal(asOfGeometryVersion, sut.AsOfGeometryVersion);
        }
    }
}