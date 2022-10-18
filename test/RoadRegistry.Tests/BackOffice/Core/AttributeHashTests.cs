namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using AutoFixture.Idioms;
using Framework.Assertions;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using Xunit;

public class AttributeHashTests
{
    private readonly Fixture _fixture;

    public AttributeHashTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeRoadSegmentCategory();
        _fixture.CustomizeRoadSegmentMorphology();
        _fixture.CustomizeRoadSegmentStatus();
        _fixture.CustomizeRoadSegmentAccessRestriction();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeAttributeHash();
    }

    [Fact]
    public void DiffersWhenAccessRestrictionDiffers()
    {
        var value1 = _fixture.Create<RoadSegmentAccessRestriction>();
        var value2 = new Generator<RoadSegmentAccessRestriction>(_fixture)
            .First(candidate => candidate != value1);
        var sut = _fixture.Create<AttributeHash>();
        var left = sut.With(value1);
        var right = sut.With(value2);
        Assert.NotEqual(left, right);
    }

    [Fact]
    public void DiffersWhenCategoryDiffers()
    {
        var value1 = _fixture.Create<RoadSegmentCategory>();
        var value2 = new Generator<RoadSegmentCategory>(_fixture)
            .First(candidate => candidate != value1);
        var sut = _fixture.Create<AttributeHash>();
        var left = sut.With(value1);
        var right = sut.With(value2);
        Assert.NotEqual(left, right);
    }

    [Fact]
    public void DiffersWhenLeftSideDiffers()
    {
        var value1 = _fixture.Create<CrabStreetnameId?>();
        var value2 = new Generator<CrabStreetnameId?>(_fixture)
            .First(candidate => candidate != value1);
        var sut = _fixture.Create<AttributeHash>();
        var left = sut.WithLeftSide(value1);
        var right = sut.WithLeftSide(value2);
        Assert.NotEqual(left, right);
    }

    [Fact]
    public void DiffersWhenMorphologyDiffers()
    {
        var value1 = _fixture.Create<RoadSegmentMorphology>();
        var value2 = new Generator<RoadSegmentMorphology>(_fixture)
            .First(candidate => candidate != value1);
        var sut = _fixture.Create<AttributeHash>();
        var left = sut.With(value1);
        var right = sut.With(value2);
        Assert.NotEqual(left, right);
    }

    [Fact]
    public void DiffersWhenOrganizationDiffers()
    {
        var value1 = _fixture.Create<OrganizationId>();
        var value2 = new Generator<OrganizationId>(_fixture)
            .First(candidate => candidate != value1);
        var sut = _fixture.Create<AttributeHash>();
        var left = sut.With(value1);
        var right = sut.With(value2);
        Assert.NotEqual(left, right);
    }

    [Fact]
    public void DiffersWhenRightSideDiffers()
    {
        var value1 = _fixture.Create<CrabStreetnameId?>();
        var value2 = new Generator<CrabStreetnameId?>(_fixture)
            .First(candidate => candidate != value1);
        var sut = _fixture.Create<AttributeHash>();
        var left = sut.WithRightSide(value1);
        var right = sut.WithRightSide(value2);
        Assert.NotEqual(left, right);
    }

    [Fact]
    public void DiffersWhenStatusDiffers()
    {
        var value1 = _fixture.Create<RoadSegmentStatus>();
        var value2 = new Generator<RoadSegmentStatus>(_fixture)
            .First(candidate => candidate != value1);
        var sut = _fixture.Create<AttributeHash>();
        var left = sut.With(value1);
        var right = sut.With(value2);
        Assert.NotEqual(left, right);
    }

    [Fact]
    public void ToStringReturnsExpectedResult()
    {
        var sut = _fixture.Create<AttributeHash>();

        Assert.True(int.TryParse(sut.ToString(), out _));
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
        ).Verify(typeof(AttributeHash));
    }
}