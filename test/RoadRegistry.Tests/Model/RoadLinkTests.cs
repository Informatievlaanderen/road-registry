namespace RoadRegistry.Model
{
    using System;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;
    
    public class RoadLinkTests
    {
        private readonly Fixture _fixture;
        private readonly RoadLinkId _id;
        private readonly RoadNodeId _source;
        private readonly RoadNodeId _target;
        private readonly RoadLink _sut;

        public RoadLinkTests()
        {
            _fixture = new Fixture();
            _id = _fixture.Create<RoadLinkId>();
            _source = _fixture.Create<RoadNodeId>();
            _target = _fixture.Create<RoadNodeId>();
            _sut = new RoadLink(_id, _source, _target);
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
            Assert.Throws<ArgumentException>(() => new RoadLink(_id, _source, _source));
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
            var sut = new RoadLink(_fixture.Create<RoadLinkId>(), new RoadNodeId(source), new RoadNodeId(target));

            var result = sut.SelectCounterNode(new RoadNodeId(to_counter)).ToArray();

            Assert.Equal(Array.ConvertAll(expected, value => new RoadNodeId(value)), result);
        }
    }
}