namespace RoadRegistry.Tests.BackOffice;

using Albedo;
using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class ChangeRequestIdTests
{
    private readonly Fixture _fixture;

    public ChangeRequestIdTests()
    {
        _fixture = FixtureFactory.Create();
        _fixture.CustomizeChangeRequestId();
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("1234567890123456789012345678901234567890123456789012345678901234", true)]
    [InlineData("123456789ABCDEF123456789ABCDEF123456789ABCDEF123456789ABCDEF1234", true)]
    [InlineData("123456789abcdef123456789abcdef123456789abcdef123456789abcdef1234", true)]
    [InlineData("12345678901234567890123456789012345678901234567890123456789012345", false)]
    [InlineData("123456789012345678901234567890123456789012345678901234567890123456", false)]
    [InlineData("123456789012345678901234567890123456789012345678901234567890123", false)]
    [InlineData("12345678901234567890123456789012345678901234567890123456789012", false)]
    [InlineData("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz", false)]
    public void AcceptsReturnsExceptedResult(string value, bool expected)
    {
        var result = ChangeRequestId.Accepts(value);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CtorValueCanNotBeLongerThan32Bytes()
    {
        var value = _fixture.CreateMany<byte>(ChangeRequestId.ExactLength + 1).ToArray();
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChangeRequestId(value));
    }

    [Fact]
    public void CtorValueCanNotBeNullOrEmpty()
    {
        new GuardClauseAssertion(
            _fixture,
            new NullReferenceBehaviorExpectation()
        ).Verify(Constructors.Select(() => new ChangeRequestId(null)));
    }

    [Fact]
    public void CtorValueCanNotBeShorterThan32Bytes()
    {
        var value = _fixture.CreateMany<byte>(ChangeRequestId.ExactLength - 1).ToArray();
        Assert.Throws<ArgumentOutOfRangeException>(() => new ChangeRequestId(value));
    }

    [Fact]
    public void ImplicitConversionToStringReturnsExpectedResult()
    {
        var generator = new Random();
        var value = string.Concat(
            Enumerable
                .Range(0, 32)
                .Select(index => ((byte)generator.Next(0, 256)).ToString("X2"))
        ).ToLowerInvariant();

        var sut = ChangeRequestId.FromString(value);

        string result = sut;

        Assert.Equal(value, result);
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var generator = new Random();
        var value = string.Concat(
            Enumerable
                .Range(0, 32)
                .Select(index => ((byte)generator.Next(0, 256)).ToString("X2"))
        ).ToLowerInvariant();
        var sut = ChangeRequestId.FromString(value);

        Assert.Equal(value.ToLowerInvariant(), sut.ToString());
    }

    [Fact]
    public void ValueCanNotBeLongerThan64Chars()
    {
        const int length = ChangeRequestId.ExactStringLength + 2;

        var generator = new Random();
        var value = string.Concat(Enumerable.Range(0, length / 2)
            .Select(index => ((byte)generator.Next(0, 256)).ToString("X2")));
        Assert.Throws<ArgumentOutOfRangeException>(() => ChangeRequestId.FromString(value));
    }

    [Fact]
    public void ValueCanNotBeShorterThan64Chars()
    {
        const int length = ChangeRequestId.ExactStringLength - 2;

        var generator = new Random();
        var value = string.Concat(Enumerable.Range(0, length / 2)
            .Select(index => ((byte)generator.Next(0, 256)).ToString("X2")));
        Assert.Throws<ArgumentOutOfRangeException>(() => ChangeRequestId.FromString(value));
    }

    [Theory]
    [InlineData("51d90e5b3a2acc52ba1b6aa157280521e3b0da2973361f42761b61ff5d002dce", true)]
    [InlineData("84DD6F9521C4B33340919B4EF6AF0653F1E0CE8C5FBF40294BBDB865443D38C8", true)]
    [InlineData("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz", false)]
    public void ValueMustBeHex(string value, bool acceptable)
    {
        if (acceptable)
        {
            var sut = ChangeRequestId.FromString(value);
            Assert.Equal(value.ToLowerInvariant(), sut.ToString());
        }
        else
        {
            Assert.Throws<ArgumentException>(() => ChangeRequestId.FromString(value));
        }
    }

    [Fact]
    public void VerifyBehavior()
    {
        new CompositeIdiomaticAssertion(
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
        ).Verify(typeof(ChangeRequestId));
    }
}
