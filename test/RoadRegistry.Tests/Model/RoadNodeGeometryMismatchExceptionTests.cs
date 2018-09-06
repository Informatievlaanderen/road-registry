namespace RoadRegistry.Model
{
    using AutoFixture;
    using Xunit;

    public class RoadNodeGeometryMismatchExceptionTests
    {
        private readonly Fixture _fixture;

        public RoadNodeGeometryMismatchExceptionTests()
        {
            _fixture = new Fixture();
        }

        [Fact]
        public void IsRoadRegistryException()
        {
            var sut = _fixture.Create<RoadNodeGeometryMismatchException>();
            Assert.IsAssignableFrom<RoadRegistryException>(sut);
        }

        [Fact]
        public void VerifyProperties()
        {
            var id = _fixture.Create<RoadNodeId>();
            var sut = new RoadNodeGeometryMismatchException(id);
            Assert.Equal(id, sut.Id);
        }
    }
}