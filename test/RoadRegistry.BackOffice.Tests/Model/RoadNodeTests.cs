namespace RoadRegistry.BackOffice.Model
{
    using System.Linq;
    using Aiv.Vbr.Shaperon;
    using AutoFixture;
    using Xunit;

    public class FullyDisconnectedRoadNodeTests
    {
        private readonly Fixture _fixture;
        private readonly RoadNodeId _id;
        private readonly PointM _geometry;
        private readonly RoadNode _sut;

        public FullyDisconnectedRoadNodeTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizePointM();
            _id = _fixture.Create<RoadNodeId>();
            _geometry = _fixture.Create<PointM>();
            _sut = new RoadNode(_id, _geometry);
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
        public void SegmentsReturnsExpectedResult()
        {
            Assert.Empty(_sut.Segments);
        }

        [Fact]
        public void ConnectWithReturnsExpectedResult()
        {
            var link = _fixture.Create<RoadSegmentId>();

            var result = _sut.ConnectWith(link);

            Assert.Equal(_sut.Id, result.Id);
            Assert.Equal(new[] { link }, result.Segments);
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
        private readonly RoadSegmentId _link1;
        private readonly RoadSegmentId _link2;
        private readonly RoadNode _sut;
        private readonly PointM _geometry;

        public ConnectedRoadNodeTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizePointM();
            _id = _fixture.Create<RoadNodeId>();
            _geometry = _fixture.Create<PointM>();
            _link1 = _fixture.Create<RoadSegmentId>();
            _link2 = _fixture.Create<RoadSegmentId>();
            _sut = new RoadNode(_id, _geometry).ConnectWith(_link1).ConnectWith(_link2);
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
        public void SegmentsReturnsExpectedResult()
        {
            Assert.Equal(new[] { _link1, _link2 }.OrderBy(_ => _), _sut.Segments.OrderBy(_ => _));
        }

        [Fact]
        public void ConnectWithReturnsExpectedResult()
        {
            var link = _fixture.Create<RoadSegmentId>();

            var result = _sut.ConnectWith(link);

            Assert.Equal(_sut.Id, result.Id);
            Assert.Equal(new[] { _link1, _link2, link }.OrderBy(_ => _), result.Segments.OrderBy(_ => _));
        }

        [Fact]
        public void DisconnectFromUnknownLinkReturnsExpectedResult()
        {
            var result = _sut.DisconnectFrom(_fixture.Create<RoadSegmentId>());

            Assert.Equal(_sut.Id, result.Id);
            Assert.Equal(new[] { _link1, _link2 }.OrderBy(_ => _), result.Segments.OrderBy(_ => _));
        }

        [Fact]
        public void DisconnectFromKnownLinkReturnsExpectedResult()
        {
            var result = _sut.DisconnectFrom(_link1);

            Assert.Equal(_sut.Id, result.Id);
            Assert.Equal(new[] { _link2 }, result.Segments);
        }
    }
}
