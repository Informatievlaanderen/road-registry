namespace RoadRegistry.BackOffice.Core
{
    using Albedo;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Framework;
    using RoadRegistry.Framework;
    using RoadRegistry.Framework.Assertions;
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
            var factoryMethod = Methods.Select(() => AttributeHash.FromHashCode(default));
            new CompositeIdiomaticAssertion(
                new EquatableEqualsSelfAssertion(_fixture),
                new StaticFactoryMethodBasedEquatableEqualsOtherAssertion(_fixture, factoryMethod),
                new StaticFactoryMethodBasedEqualityOperatorEqualsSelfAssertion(_fixture, factoryMethod),
                new StaticFactoryMethodBasedEqualityOperatorEqualsOtherAssertion(_fixture, factoryMethod),
                new StaticFactoryMethodBasedInequalityOperatorEqualsSelfAssertion(_fixture, factoryMethod),
                new StaticFactoryMethodBasedInequalityOperatorEqualsOtherAssertion(_fixture, factoryMethod),
                new EqualsNewObjectAssertion(_fixture),
                new EqualsNullAssertion(_fixture),
                new EqualsSelfAssertion(_fixture),
                new StaticFactoryMethodBasedEqualsOtherAssertion(_fixture, factoryMethod),
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
