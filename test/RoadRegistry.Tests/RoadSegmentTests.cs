using System;
using System.Linq;
using AutoFixture;
using AutoFixture.Idioms;
using Xunit;

namespace RoadRegistry
{
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
            Assert.Equal(_source, _sut.Source);
        }

        [Fact]
        public void TargetReturnsExpectedResult()
        {
            Assert.Equal(_target, _sut.Target);
        }

        [Fact]
        public void ThrowsWhenSourceIsSameAsTarget()
        {
            Assert.Throws<ArgumentException>(() => new RoadSegment(_id, _source, _source));
        }
        
        [Fact]
        public void NodesReturnsExpectedResult()
        {
            Assert.Equal(new [] { _source, _target }, _sut.Nodes);
        }

        [Theory]
        [InlineData(1L, 2L, 1L, new [] { 2L })]
        [InlineData(1L, 2L, 2L, new [] { 1L })]
        [InlineData(1L, 2L, 3L, new long[0])]
        public void SelectCounterNodeReturnsExpectedResult(
            long source,
            long target,
            long to_counter,
            long[] expected
        )
        {
            var sut = new RoadSegment(_fixture.Create<RoadSegmentId>(), new RoadNodeId(source), new RoadNodeId(target));

            var result = sut.SelectCounterNode(new RoadNodeId(to_counter)).ToArray();

            Assert.Equal(Array.ConvertAll(expected, value => new RoadNodeId(value)), result);
        }
    }
}