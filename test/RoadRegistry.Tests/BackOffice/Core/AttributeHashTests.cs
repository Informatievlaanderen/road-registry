namespace RoadRegistry.BackOffice.Core
{
    using System.Linq;
    using Albedo;
    using AutoFixture;
    using AutoFixture.Idioms;
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
        public void DiffersWhenCategoryDiffers()
        {
            var value1 = _fixture.Create<RoadSegmentCategory>();
            var value2 = new Generator<RoadSegmentCategory>(_fixture)
                .First(candidate => candidate != value1);
            var sut = _fixture.Create<AttributeHash>();
            var left = sut.With(value1);
            var right = sut.With(value2);
            Assert.NotEqual(left, right);
        }

        [Fact]
        public void DiffersWhenMorphologyDiffers()
        {
            var value1 = _fixture.Create<RoadSegmentMorphology>();
            var value2 = new Generator<RoadSegmentMorphology>(_fixture)
                .First(candidate => candidate != value1);
            var sut = _fixture.Create<AttributeHash>();
            var left = sut.With(value1);
            var right = sut.With(value2);
            Assert.NotEqual(left, right);
        }

        [Fact]
        public void DiffersWhenAccessRestrictionDiffers()
        {
            var value1 = _fixture.Create<RoadSegmentAccessRestriction>();
            var value2 = new Generator<RoadSegmentAccessRestriction>(_fixture)
                .First(candidate => candidate != value1);
            var sut = _fixture.Create<AttributeHash>();
            var left = sut.With(value1);
            var right = sut.With(value2);
            Assert.NotEqual(left, right);
        }

        [Fact]
        public void DiffersWhenStatusDiffers()
        {
            var value1 = _fixture.Create<RoadSegmentStatus>();
            var value2 = new Generator<RoadSegmentStatus>(_fixture)
                .First(candidate => candidate != value1);
            var sut = _fixture.Create<AttributeHash>();
            var left = sut.With(value1);
            var right = sut.With(value2);
            Assert.NotEqual(left, right);
        }

        [Fact]
        public void DiffersWhenOrganizationDiffers()
        {
            var value1 = _fixture.Create<OrganizationId>();
            var value2 = new Generator<OrganizationId>(_fixture)
                .First(candidate => candidate != value1);
            var sut = _fixture.Create<AttributeHash>();
            var left = sut.With(value1);
            var right = sut.With(value2);
            Assert.NotEqual(left, right);
        }

        [Fact]
        public void DiffersWhenLeftSideDiffers()
        {
            var value1 = _fixture.Create<CrabStreetnameId?>();
            var value2 = new Generator<CrabStreetnameId?>(_fixture)
                .First(candidate => candidate != value1);
            var sut = _fixture.Create<AttributeHash>();
            var left = sut.WithLeftSide(value1);
            var right = sut.WithLeftSide(value2);
            Assert.NotEqual(left, right);
        }

        [Fact]
        public void DiffersWhenRightSideDiffers()
        {
            var value1 = _fixture.Create<CrabStreetnameId?>();
            var value2 = new Generator<CrabStreetnameId?>(_fixture)
                .First(candidate => candidate != value1);
            var sut = _fixture.Create<AttributeHash>();
            var left = sut.WithRightSide(value1);
            var right = sut.WithRightSide(value2);
            Assert.NotEqual(left, right);
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var sut = _fixture.Create<AttributeHash>();

            Assert.True(int.TryParse(sut.ToString(), out _));
        }
    }
}
