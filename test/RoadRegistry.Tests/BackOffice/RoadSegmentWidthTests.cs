namespace RoadRegistry.Tests.BackOffice;

using Albedo;
using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class RoadSegmentWidthTests
{
    private readonly Fixture _fixture;

    public RoadSegmentWidthTests()
    {
        _fixture = FixtureFactory.Create();
        _fixture.CustomizeRoadSegmentWidth();
    }

    [Theory]
    [InlineData(int.MinValue, false)]
    [InlineData(-10, false)]
    [InlineData(-9, true)]
    [InlineData(-8, true)]
    [InlineData(-7, false)]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(50, true)]
    [InlineData(51, false)]
    [InlineData(int.MaxValue, false)]
    public void AcceptsReturnsExpectedResult(int value, bool expected)
    {
        var result = RoadSegmentWidth.Accepts(value);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _fixture.Create<int>() % RoadSegmentWidth.Maximum.ToInt32();
        var sut = new RoadSegmentWidth(value);

        Assert.Equal(value.ToString(), sut.ToString());
    }

    [Fact]
    public void VerifyBehavior()
    {
        _fixture.Customizations.Add(
            new FiniteSequenceGenerator<int>(Enumerable.Range(RoadSegmentWidth.Minimum, RoadSegmentWidth.Maximum).ToArray()));
        new CompositeIdiomaticAssertion(
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
        ).Verify(typeof(RoadSegmentWidth));

        new GuardClauseAssertion(_fixture,
                new Int32RangeBehaviorExpectation(RoadSegmentWidth.Minimum, RoadSegmentWidth.Maximum, RoadSegmentWidth.Unknown, RoadSegmentWidth.NotApplicable))
            .Verify(Constructors.Select(() => new RoadSegmentWidth(0)));
    }
}
