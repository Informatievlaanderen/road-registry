namespace RoadRegistry.Model
{
    using System.Linq;
    using AutoFixture;
    using Xunit;
    
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
        public void LinksReturnsExpectedResult()
        {
            Assert.Empty(_sut.Links);
        }

        [Fact]
        public void ConnectWithReturnsExpectedResult()
        {
            var link = _fixture.Create<RoadLinkId>();

            var result = _sut.ConnectWith(link);

            Assert.Equal(_sut.Id, result.Id);
            Assert.Equal(new RoadLinkId[] { link }, result.Links);
        }

        [Fact]
        public void DisconnectFromReturnsExpectedResult()
        {
            var result = _sut.DisconnectFrom(_fixture.Create<RoadLinkId>());

            Assert.Equal(_sut.Id, result.Id);
            Assert.Empty(result.Links);
        }
    }

    public class ConnectedRoadNodeTests
    {
        private readonly Fixture _fixture;
        private readonly RoadNodeId _id;
        private readonly RoadLinkId _link1;
        private readonly RoadLinkId _link2;
        private readonly RoadNode _sut;

        public ConnectedRoadNodeTests()
        {
            _fixture = new Fixture();
            _id = _fixture.Create<RoadNodeId>();
            _link1 = _fixture.Create<RoadLinkId>();
            _link2 = _fixture.Create<RoadLinkId>();
            _sut = new RoadNode(_id).ConnectWith(_link1).ConnectWith(_link2);
        }

        [Fact]
        public void IdReturnsExpectedResult()
        {
            Assert.Equal(_id, _sut.Id);
        }

        [Fact]
        public void LinksReturnsExpectedResult()
        {
            Assert.Equal(new [] { _link1, _link2 }.OrderBy(_ => _), _sut.Links.OrderBy(_ => _));
        }

        [Fact]
        public void ConnectWithReturnsExpectedResult()
        {
            var link = _fixture.Create<RoadLinkId>();

            var result = _sut.ConnectWith(link);

            Assert.Equal(_sut.Id, result.Id);
            Assert.Equal(new RoadLinkId[] { _link1, _link2, link }.OrderBy(_ => _), result.Links.OrderBy(_ => _));
        }

        [Fact]
        public void DisconnectFromUnknownLinkReturnsExpectedResult()
        {
            var result = _sut.DisconnectFrom(_fixture.Create<RoadLinkId>());

            Assert.Equal(_sut.Id, result.Id);
            Assert.Equal(new [] { _link1, _link2 }.OrderBy(_ => _), result.Links.OrderBy(_ => _));
        }

        [Fact]
        public void DisconnectFromKnownLinkReturnsExpectedResult()
        {
            var result = _sut.DisconnectFrom(_link1);

            Assert.Equal(_sut.Id, result.Id);
            Assert.Equal(new [] { _link2 }, result.Links);
        }
    }
}