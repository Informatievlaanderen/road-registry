namespace RoadRegistry.Tests.BackOffice;

using Albedo;
using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class RoadSegmentPositionTests
{
    private readonly Fixture _fixture;

    public RoadSegmentPositionTests()
    {
        _fixture = new Fixture();
    }

    public static IEnumerable<object[]> AcceptsDecimalCases
    {
        get
        {
            yield return new object[] { decimal.MinValue, false };
            yield return new object[] { -1.0m, false };
            yield return new object[] { 0.0m, true };
            yield return new object[] { 1.0m, true };
            yield return new object[] { decimal.MaxValue, true };
        }
    }

    [Theory]
    [MemberData(nameof(AcceptsDecimalCases))]
    public void AcceptsDecimalReturnsExpectedResult(decimal value, bool expected)
    {
        var result = RoadSegmentPosition.Accepts(value);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(double.MinValue, false)]
    [InlineData(-1.0, false)]
    [InlineData(0.0, true)]
    [InlineData(1.0, true)]
    [InlineData(double.MaxValue, true)]
    public void AcceptsDoubleReturnsExpectedResult(double value, bool expected)
    {
        var result = RoadSegmentPosition.Accepts(value);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1.0, 2.0, -1)]
    [InlineData(2.0, 1.0, 1)]
    [InlineData(1.0, 1.0, 0)]
    public void CompareToReturnsExpectedResult(double left, double right, int expected)
    {
        var sut = new RoadSegmentPosition(new decimal(left));

        var result = sut.CompareTo(new RoadSegmentPosition(new decimal(right)));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void FromDoubleReturnsExpectedResult()
    {
        var value = _fixture.Create<double>();

        var result = RoadSegmentPosition.FromDouble(value);

        Assert.Equal(new RoadSegmentPosition(Convert.ToDecimal(value)), result);
    }

    [Fact]
    public void ToDoubleReturnsExpectedValue()
    {
        var value = _fixture.Create<decimal>();
        var sut = new RoadSegmentPosition(value);

        var result = sut.ToDouble();

        Assert.Equal(decimal.ToDouble(value), result);
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _fixture.Create<decimal>();
        var sut = new RoadSegmentPosition(value);

        Assert.Equal(value.ToString(), sut.ToString());
    }

    [Fact]
    public void VerifyBehavior()
    {
        new CompositeIdiomaticAssertion(
            new ImplicitConversionOperatorAssertion<decimal>(_fixture),
            new ExplicitConversionMethodAssertion<decimal>(_fixture),
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
            new GetHashCodeSuccessiveAssertion(_fixture),
            new ComparableCompareToSelfAssertion(_fixture),
            new LessThanOperatorCompareToSelfAssertion(_fixture),
            new LessThanOrEqualOperatorCompareToSelfAssertion(_fixture),
            new GreaterThanOperatorCompareToSelfAssertion(_fixture),
            new GreaterThanOrEqualOperatorCompareToSelfAssertion(_fixture)
        ).Verify(typeof(RoadSegmentPosition));

        new GuardClauseAssertion(_fixture, new NegativeDecimalBehaviorExpectation())
            .Verify(Constructors.Select(() => new RoadSegmentPosition(0.0m)));

        new GuardClauseAssertion(_fixture, new NegativeDoubleBehaviorExpectation())
            .Verify(Methods.Select(() => RoadSegmentPosition.FromDouble(0.0)));
    }

    [Fact]
    public void ZeroReturnsExpectedValue()
    {
        Assert.Equal(0.0m, RoadSegmentPosition.Zero.ToDecimal());
    }
}
