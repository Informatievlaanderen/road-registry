namespace RoadRegistry.Model
{
    using AutoFixture;
    using Xunit;

    public class RoadNodeIdTakenExceptionTests
    {
        private readonly Fixture _fixture;

        public RoadNodeIdTakenExceptionTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void IsRoadRegistryException()
        {
            var sut = _fixture.Create<RoadNodeIdTakenException>();
            Assert.IsAssignableFrom<RoadRegistryException>(sut);
        }

        [Fact]
        public void VerifyProperties()
        {
            var id = _fixture.Create<RoadNodeId>();
            var sut = new RoadNodeIdTakenException(id);
            Assert.Equal(id, sut.Id);
        }
    }
}