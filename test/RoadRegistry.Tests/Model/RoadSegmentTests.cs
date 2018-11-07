namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using Xunit;

    public class RoadSegmentTests
    {
        private readonly Fixture _fixture;
        private readonly RoadSegmentId _id;
        private readonly RoadNodeId _source;
        private readonly RoadNodeId _target;
        private readonly RoadSegment _sut;

        public RoadSegmentTests()
        {
            _fixture = new Fixture();
            _id = _fixture.Create<RoadSegmentId>();
            _source = _fixture.Create<RoadNodeId>();
            _target = _fixture.Create<RoadNodeId>();
            _sut = new RoadSegment(_id, _source, _target);
        }

        [Fact]
        public void IdReturnsExpectedResult()
        {
            Assert.Equal(_id, _sut.Id);
        }

        [Fact]
        public void SourceReturnsExpectedResult()
        {
            Assert.Equal(_source, _sut.Start);
        }

        [Fact]
        public void TargetReturnsExpectedResult()
        {
            Assert.Equal(_target, _sut.End);
        }

        [Fact]
        public void ThrowsWhenSourceIsSameAsTarget()
        {
            Assert.Throws<ArgumentException>(() => new RoadSegment(_id, _source, _source));
        }

        [Fact]
        public void NodesReturnsExpectedResult()
        {
            Assert.Equal(new[] { _source, _target }, _sut.Nodes);
        }

        [Theory]
        [InlineData(1, 2, 1, new[] { 2 })]
        [InlineData(1, 2, 2, new[] { 1 })]
        [InlineData(1, 2, 3, new int[0])]
        public void SelectCounterNodeReturnsExpectedResult(
            int source,
            int target,
            int toCounter,
            int[] expected
        )
        {
            var sut = new RoadSegment(_fixture.Create<RoadSegmentId>(), new RoadNodeId(source), new RoadNodeId(target));

            var result = sut.SelectCounterNode(new RoadNodeId(toCounter)).ToArray();

            Assert.Equal(Array.ConvertAll(expected, value => new RoadNodeId(value)), result);
        }
    }
}
