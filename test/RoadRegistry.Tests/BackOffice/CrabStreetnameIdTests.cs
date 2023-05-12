namespace RoadRegistry.Tests.BackOffice;

using System.Globalization;
using Albedo;
using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class CrabStreetnameIdTests
{
    private readonly Fixture _fixture;

    public CrabStreetnameIdTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void CtorValueCanNotBeNegative()
    {
        new GuardClauseAssertion(_fixture, new NegativeInt32BehaviorExpectation()).Verify(
            Constructors.Select(() => new CrabStreetnameId(0)));
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = _fixture.Create<int>();
        var sut = new CrabStreetnameId(value);

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
            new GetHashCodeSuccessiveAssertion(_fixture)
        ).Verify(typeof(CrabStreetnameId));
    }
}