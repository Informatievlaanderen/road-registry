﻿namespace RoadRegistry.Model
{
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Idioms;
    using Xunit;

    public class RoadSegmentLaneCountTests
    {
        private readonly Fixture _fixture;

        public RoadSegmentLaneCountTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeRoadSegmentLaneCount();
        }

        [Fact]
        public void VerifyBehavior()
        {
            _fixture.Customizations.Add(
                new FiniteSequenceGenerator<int>(Enumerable.Range(0, RoadSegmentLaneCount.Maximum.ToInt32()).ToArray()));
            new CompositeIdiomaticAssertion(
                new GuardClauseAssertion(_fixture,
                    new Int32RangeBehaviorExpectation(0, RoadSegmentLaneCount.Maximum.ToInt32())),
                new ImplicitConversionOperatorAssertion<int>(_fixture),
                new ExplicitConversionMethodAssertion<int>(_fixture),
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
            ).Verify(typeof(RoadSegmentLaneCount));
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = _fixture.Create<int>() % RoadSegmentLaneCount.Maximum.ToInt32();
            var sut = new RoadSegmentLaneCount(value);

            Assert.Equal(value.ToString(), sut.ToString());
        }
    }
}
