namespace RoadRegistry.BackOffice.Model
{
    using AutoFixture;
    using AutoFixture.Idioms;
    using Framework;
    using Xunit;

    public class AttributeHashTests
    {
        private readonly Fixture _fixture;

        public AttributeHashTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRoadSegmentCategory();
            _fixture.CustomizeRoadSegmentMorphology();
            _fixture.CustomizeRoadSegmentStatus();
            _fixture.CustomizeRoadSegmentAccessRestriction();
            _fixture.CustomizeOrganizationId();
            _fixture.CustomizeAttributeHash();
        }

        [Fact]
        public void VerifyBehavior()
        {
            new CompositeIdiomaticAssertion(
                new EquatableEqualsSelfAssertion(_fixture),
                new EquatableEqualsOtherAssertion(_fixture),
                new EqualityOperatorEqualsSelfAssertion(_fixture),
                new EqualityOperatorEqualsOtherAssertion(_fixture),
                new InequalityOperatorEqualsSelfAssertion(_fixture),
                new InequalityOperatorEqualsOtherAssertion(_fixture),
                new EqualsNewObjectAssertion(_fixture),
                new EqualsNullAssertion(_fixture),
                new EqualsSelfAssertion(_fixture),
                new EqualsOtherAssertion(_fixture),
                new EqualsSuccessiveAssertion(_fixture),
                new GetHashCodeSuccessiveAssertion(_fixture)
            ).Verify(typeof(AttributeHash));
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var sut = _fixture.Create<AttributeHash>();

            Assert.True(int.TryParse(sut.ToString(), out _));
        }
    }
}
