namespace RoadRegistry.Tests.BackOffice;

using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class RoadNetworkRevisionTests
{
    private readonly Fixture _fixture;

    public RoadNetworkRevisionTests()
    {
        _fixture = FixtureFactory.Create();
    }

    [Fact]
    public void NextHasExpectedResult()
    {
        var value = new Generator<int>(_fixture).First(candidate => candidate >= 0 && candidate < int.MaxValue);
        var sut = new RoadNetworkRevision(value);

        var result = sut.Next();

        Assert.Equal(new RoadNetworkRevision(value + 1), result);
    }

    [Fact]
    public void NextThrowsWhenMaximumHasBeenReached()
    {
        var sut = new RoadNetworkRevision(int.MaxValue);

        Assert.Throws<NotSupportedException>(() => sut.Next());
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
            new GetHashCodeSuccessiveAssertion(_fixture)
        ).Verify(typeof(RoadNetworkRevision));
    }
}
