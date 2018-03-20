using System.Linq;
using AutoFixture;
using Xunit;

namespace RoadRegistry
{
    public class FullyDisconnectedRoadNodeTests
    {
        private readonly Fixture _fixture;
        private readonly RoadNodeId _id;
        private readonly RoadNode _sut;

        public FullyDisconnectedRoadNodeTests()
        {
            _fixture = new Fixture();
            _id = _fixture.Create<RoadNodeId>();
            _sut = new RoadNode(_id);
        }

        [Fact]
        public void IdReturnsExpectedResult()
        {
            Assert.Equal(_id, _sut.Id);
        }

        [Fact]
        public void SegmentsReturnsExpectedResult()
        {
            Assert.Empty(_sut.Segments);
        }

        [Fact]
        public void ConnectWithReturnsExpectedResult()
        {
            var segment = _fixture.Create<RoadSegmentId>();

            var result = _sut.ConnectWith(segment);

            Assert.Equal(_sut.Id, result.Id);
            Assert.Equal(new RoadSegmentId[] { segment }, result.Segments);
        }

        [Fact]
        public void DisconnectFromReturnsExpectedResult()
        {
            var result = _sut.DisconnectFrom(_fixture.Create<RoadSegmentId>());

            Assert.Equal(_sut.Id, result.Id);
            Assert.Empty(result.Segments);
        }
    }

    public class ConnectedRoadNodeTests
    {
        private readonly Fixture _fixture;
        private readonly RoadNodeId _id;
        private readonly RoadSegmentId _segment1;
        private readonly RoadSegmentId _segment2;
        private readonly RoadNode _sut;

        public ConnectedRoadNodeTests()
        {
            _fixture = new Fixture();
            _id = _fixture.Create<RoadNodeId>();
            _segment1 = _fixture.Create<RoadSegmentId>();
            _segment2 = _fixture.Create<RoadSegmentId>();
            _sut = new RoadNode(_id).ConnectWith(_segment1).ConnectWith(_segment2);
        }

        [Fact]
        public void IdReturnsExpectedResult()
        {
            Assert.Equal(_id, _sut.Id);
        }

        [Fact]
        public void SegmentsReturnsExpectedResult()
        {
            Assert.Equal(new [] { _segment1, _segment2 }.OrderBy(_ => _), _sut.Segments.OrderBy(_ => _));
        }

        [Fact]
        public void ConnectWithReturnsExpectedResult()
        {
            var segment = _fixture.Create<RoadSegmentId>();

            var result = _sut.ConnectWith(segment);

            Assert.Equal(_sut.Id, result.Id);
            Assert.Equal(new RoadSegmentId[] { _segment1, _segment2, segment }.OrderBy(_ => _), result.Segments.OrderBy(_ => _));
        }

        [Fact]
        public void DisconnectFromUnknownSegmentReturnsExpectedResult()
        {
            var result = _sut.DisconnectFrom(_fixture.Create<RoadSegmentId>());

            Assert.Equal(_sut.Id, result.Id);
            Assert.Equal(new [] { _segment1, _segment2 }.OrderBy(_ => _), result.Segments.OrderBy(_ => _));
        }

        [Fact]
        public void DisconnectFromKnownSegmentReturnsExpectedResult()
        {
            var result = _sut.DisconnectFrom(_segment1);

            Assert.Equal(_sut.Id, result.Id);
            Assert.Equal(new [] { _segment2 }, result.Segments);
        }
    }
}