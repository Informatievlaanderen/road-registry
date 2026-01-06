namespace RoadRegistry.Tests.BackOffice;

using System.Globalization;
using Albedo;
using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class GradeSeparatedJunctionIdTests
{
    private readonly Fixture _fixture;

    public GradeSeparatedJunctionIdTests()
    {
        _fixture = FixtureFactory.Create();
    }

    [Theory]
    [InlineData(int.MinValue, false)]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(int.MaxValue, true)]
    public void AcceptsReturnsExpectedResult(int value, bool expected)
    {
        var result = GradeSeparatedJunctionId.Accepts(value);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, 2, -1)]
    [InlineData(2, 1, 1)]
    public void CompareToReturnsExpectedResult(int left, int right, int expected)
    {
        var sut = new GradeSeparatedJunctionId(left);

        var result = sut.CompareTo(new GradeSeparatedJunctionId(right));

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, 2, 2)]
    [InlineData(2, 1, 2)]
    [InlineData(2, 2, 2)]
    public void MaxHasExpectedResult(int left, int right, int expected)
    {
        var result = GradeSeparatedJunctionId.Max(new GradeSeparatedJunctionId(left), new GradeSeparatedJunctionId(right));

        Assert.Equal(new GradeSeparatedJunctionId(expected), result);
    }

    [Theory]
    [InlineData(1, 2, 1)]
    [InlineData(2, 1, 1)]
    [InlineData(1, 1, 1)]
    public void MinHasExpectedResult(int left, int right, int expected)
    {
        var result = GradeSeparatedJunctionId.Min(new GradeSeparatedJunctionId(left), new GradeSeparatedJunctionId(right));

        Assert.Equal(new GradeSeparatedJunctionId(expected), result);
    }

    [Fact]
    public void NextHasExpectedResult()
    {
        var value = new Generator<int>(_fixture).First(candidate => candidate >= 0 && candidate < int.MaxValue);
        var sut = new GradeSeparatedJunctionId(value);

        var result = sut.Next();

        Assert.Equal(new GradeSeparatedJunctionId(value + 1), result);
    }

    [Fact]
    public void NextThrowsWhenMaximumHasBeenReached()
    {
        var sut = new GradeSeparatedJunctionId(int.MaxValue);

        Assert.Throws<NotSupportedException>(() => sut.Next());
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _fixture.Create<int>();
        var sut = new GradeSeparatedJunctionId(value);

        Assert.Equal(value.ToString(CultureInfo.InvariantCulture), sut.ToString());
    }

    [Fact]
    public void VerifyBehavior()
    {
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
            new GetHashCodeSuccessiveAssertion(_fixture),
            new ComparableCompareToSelfAssertion(_fixture)
        ).Verify(typeof(GradeSeparatedJunctionId));

        new GuardClauseAssertion(_fixture, new NegativeInt32BehaviorExpectation())
            .Verify(Constructors.Select(() => new GradeSeparatedJunctionId(0)));
    }
}
