namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using NetTopologySuite.Geometries;
    using Xunit;

    public class RoadSegmentTests
    {
        private readonly Fixture _fixture;
        private readonly RoadSegmentId _id;
        private readonly RoadNodeId _start;
        private readonly RoadNodeId _end;
        private readonly RoadSegment _sut;
        private readonly MultiLineString _geometry;

        public RoadSegmentTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizePolylineM();
            _fixture.CustomizeRoadNodeId();
            _fixture.CustomizeRoadSegmentId();
            _id = _fixture.Create<RoadSegmentId>();
            _start = _fixture.Create<RoadNodeId>();
            _end = _fixture.Create<RoadNodeId>();
            _geometry = _fixture.Create<MultiLineString>();
            _sut = new RoadSegment(_id, _geometry, _start, _end, AttributeHash.None);
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
        public void ThrowsWhenStartIsSameAsEnd()
        {
            Assert.Throws<ArgumentException>(() => new RoadSegment(_id, _geometry, _start, _start,
                AttributeHash.None));
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
            int toCounter,
            int[] expected
        )
        {
            var sut = new RoadSegment(_fixture.Create<RoadSegmentId>(), _fixture.Create<MultiLineString>(),
                new RoadNodeId(start), new RoadNodeId(end),
                AttributeHash.None);

            var result = sut.SelectOppositeNode(new RoadNodeId(toCounter)).ToArray();

            Assert.Equal(Array.ConvertAll(expected, value => new RoadNodeId(value)), result);
        }
    }
}
