﻿namespace RoadRegistry.Model
{
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class RoadSegmentWidthTests
    {
        private readonly Fixture _fixture;

        public RoadSegmentWidthTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRoadSegmentWidth();
        }

        [Fact]
        public void VerifyBehavior()
        {
            new CompositeIdiomaticAssertion(
                new GuardClauseAssertion(_fixture, new Int32RangeBehaviorExpectation(0, 45, -8, -9)),
                new ImplicitConversionOperatorAssertion<int>(
                    () => _fixture.Create<int>() % RoadSegmentWidth.Maximum.ToInt32(),
                    value => new RoadSegmentWidth(value)),
                new ExplicitConversionMethodAssertion<int>(
                    () => _fixture.Create<int>() % RoadSegmentWidth.Maximum.ToInt32(),
                    value => new RoadSegmentWidth(value)),
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
            ).Verify(typeof(RoadSegmentWidth));
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = _fixture.Create<int>() % RoadSegmentWidth.Maximum.ToInt32();
            var sut = new RoadSegmentWidth(value);

            Assert.Equal(value.ToString(), sut.ToString());
        }
    }
}
