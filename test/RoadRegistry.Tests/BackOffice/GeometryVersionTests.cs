namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class GeometryVersionTests
{
    private readonly Fixture _fixture;

    public GeometryVersionTests()
    {
        _fixture = new Fixture();
    }

    [Theory]
    [InlineData(1, 2, -1)]
    [InlineData(2, 1, 1)]
    [InlineData(1, 1, 0)]
    public void CompareToReturnsExpectedResult(int left, int right, int expected)
    {
        var sut = new GeometryVersion(left);

        var result = sut.CompareTo(new GeometryVersion(right));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _fixture.Create<int>();
        var sut = new GeometryVersion(value);

        Assert.Equal(value.ToString(), sut.ToString());
    }

    [Fact]
    public void VerifyBehavior()
    {
        new CompositeIdiomaticAssertion(
            new GuardClauseAssertion(_fixture, new NegativeInt32BehaviorExpectation()),
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
            new ComparableCompareToSelfAssertion(_fixture),
            new LessThanOperatorCompareToSelfAssertion(_fixture),
            new LessThanOrEqualOperatorCompareToSelfAssertion(_fixture),
            new GreaterThanOperatorCompareToSelfAssertion(_fixture),
            new GreaterThanOrEqualOperatorCompareToSelfAssertion(_fixture)
        ).Verify(typeof(GeometryVersion));
    }
}
