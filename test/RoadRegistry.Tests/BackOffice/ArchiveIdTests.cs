namespace RoadRegistry.Tests.BackOffice;

using Albedo;
using AutoFixture;
using AutoFixture.Idioms;
using AutoFixture.Kernel;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework.Assertions;
using Newtonsoft.Json;
using RoadRegistry.BackOffice;
using Xunit;

public class ArchiveIdTests
{
    public ArchiveIdTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeArchiveId();
    }

    private readonly Fixture _fixture;
    private static readonly JsonSerializerSettings _serializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

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
        var customizedString = new Fixture();
        customizedString.Customize<string>(customization =>
            customization.FromFactory(generator =>
                new string(
                    (char)new Random().Next(97, 123), // a-z
                    generator.Next(1, ArchiveId.MaxLength + 1)
                )
            ));
        new CompositeIdiomaticAssertion(
            new ImplicitConversionOperatorAssertion<string>(
                new CompositeSpecimenBuilder(customizedString, _fixture)),
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
        ).Verify(typeof(ArchiveId));
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
