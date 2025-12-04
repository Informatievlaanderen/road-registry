namespace RoadRegistry.Tests.BackOffice;

using Albedo;
using AutoFixture;
using AutoFixture.Idioms;
using AutoFixture.Kernel;
using Framework.Assertions;
using RoadRegistry.BackOffice;

public class ArchiveIdTests
{
    private readonly Fixture _fixture;

    public ArchiveIdTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeArchiveId();
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("1", true)]
    [InlineData("123456789012345678901234567890123", false)]
    [InlineData("12345678901234567890123456789012", true)]
    public void AcceptsReturnsExceptedResult(string value, bool expected)
    {
        var result = ArchiveId.Accepts(value);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CtorValueCanNotBeNullOrEmpty()
    {
        new GuardClauseAssertion(
            _fixture,
            new CompositeBehaviorExpectation(
                new NullReferenceBehaviorExpectation(),
                new EmptyStringBehaviorExpectation()
            )
        ).Verify(Constructors.Select(() => new ArchiveId("")));
    }

    //[Fact]
    //public void CtorGuidValueCanNotBeEmpty()
    //{
    //    new GuardClauseAssertion(
    //        _fixture,
    //        new CompositeBehaviorExpectation(
    //            new NullReferenceBehaviorExpectation(),
    //            new EmptyStringBehaviorExpectation()
    //        )
    //    ).Verify(Constructors.Select(() => new ArchiveId(Guid.Empty)));
    //}

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var value = new string(
            (char)new Random().Next(97, 123), // a-z
            new Random().Next(1, ArchiveId.MaxLength + 1)
        );
        var sut = new ArchiveId(value);

        Assert.Equal(value, sut.ToString());
    }

    [Fact]
    public void ValueCanNotBeLongerThan32Chars()
    {
        const int length = ArchiveId.MaxLength + 1;

        var value = new string((char)new Random().Next(97, 123), length);
        Assert.Throws<ArgumentOutOfRangeException>(() => new ArchiveId(value));
    }

    [Fact]
    public void VerifyBehavior()
    {
        var fixture = new Fixture();

        fixture.Customize<string>(customization =>
            customization.FromFactory(_ =>
                Guid.NewGuid().ToString("N")
            )
        );

        var assertions = new IIdiomaticAssertion[]
        {
            new ImplicitConversionOperatorAssertion<string>(new CompositeSpecimenBuilder(fixture, _fixture)),
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
        };

        foreach (var assertion in assertions)
        {
            if (assertion is ImplicitConversionOperatorAssertion<string>)
            {
                _fixture.CustomizeArchiveId();
            }
            else
            {
                _fixture.Customize<ArchiveId>(composer =>
                    composer.FromFactory(_ =>
                        new ArchiveId(Guid.NewGuid().ToString("N"))
                    )
                );
            }

            assertion.Verify(typeof(ArchiveId));
        }
    }

    //[Fact]
    //public void ShouldSerialize()
    //{
    //    var archiveIdValue = "9573539e13aa439d949762efa1351f80";
    //    var archiveId = new ArchiveId(archiveIdValue);
    //    var sut = JsonConvert.SerializeObject(archiveId, _serializerSettings);
    //    Assert.Equal(archiveIdValue, sut);
    //}

    //[Fact]
    //public void ShouldDeserialize()
    //{
    //    var archiveIdValue = "9573539e13aa439d949762efa1351f80";
    //    var archiveId = new ArchiveId(archiveIdValue);
    //    var sut = JsonConvert.DeserializeObject<ArchiveId>(archiveIdValue, _serializerSettings);
    //    Assert.Equal(archiveId, sut);
    //}
}
