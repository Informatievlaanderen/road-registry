namespace RoadRegistry.Tests.BackOffice;

using Albedo;
using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class RoadSegmentLaneCountTests
{
    private readonly Fixture _fixture;

    public RoadSegmentLaneCountTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeRoadSegmentLaneCount();
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
    [InlineData(10, true)]
    [InlineData(11, false)]
    [InlineData(int.MaxValue, false)]
    public void AcceptsReturnsExpectedResult(int value, bool expected)
    {
        var result = RoadSegmentLaneCount.Accepts(value);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _fixture.Create<int>() % RoadSegmentLaneCount.Maximum;
        var sut = new RoadSegmentLaneCount(value);

        Assert.Equal(value.ToString(), sut.ToString());
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("10", 10)]
    [InlineData("niet gekend", -8)]
    [InlineData("niet van toepassing", -9)]
    public void ToDutchStringReturnsExpectedResult(string expected, int value)
    {
        var sut = new RoadSegmentLaneCount(value);

        Assert.Equal(expected, sut.ToDutchString());
    }

    [Fact]
    public void VerifyBehavior()
    {
        _fixture.Customizations.Add(
            new FiniteSequenceGenerator<int>(Enumerable.Range(RoadSegmentLaneCount.Minimum, RoadSegmentLaneCount.Maximum).ToArray()));
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
        ).Verify(typeof(RoadSegmentLaneCount));

        new GuardClauseAssertion(_fixture, new Int32RangeBehaviorExpectation(RoadSegmentLaneCount.Minimum, RoadSegmentLaneCount.Maximum, RoadSegmentLaneCount.Unknown, RoadSegmentLaneCount.NotApplicable))
            .Verify(Constructors.Select(() => new RoadSegmentLaneCount(0)));
    }
}
