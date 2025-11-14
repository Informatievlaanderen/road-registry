namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.BackOffice;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentAttributesValidatorTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Fixture _fixture;

    public RoadSegmentAttributesValidatorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _fixture = new RoadNetworkTestData().Fixture;
    }

    [Fact]
    public void WhenFromAndToAreNullAndSideIsBoth_ThenOnly1ValueIsAllowed()
    {
        AssertValidateResult([
            (null, null, RoadSegmentAttributeSide.Both, 1)
        ], expectedErrorCodes: []);

        AssertValidateResult([
            (null, null, RoadSegmentAttributeSide.Both, 1),
            (null, null, RoadSegmentAttributeSide.Both, 2),
        ], expectedErrorCodes: ["RoadSegmentStreetNameValueNotUniqueWithinSegment"]);
    }

    [Fact]
    public void WhenFromAndToIsNullAndSideIsNotNull_Then1ValuePerSideIsAllowed()
    {
        AssertValidateResult([
            (null, null, RoadSegmentAttributeSide.Both, 1)
        ], expectedErrorCodes: []);

        AssertValidateResult([
            (null, null, RoadSegmentAttributeSide.Left, 1)
        ], expectedErrorCodes: []);

        AssertValidateResult([
            (null, null, RoadSegmentAttributeSide.Left, 1),
            (null, null, RoadSegmentAttributeSide.Right, 1)
        ], expectedErrorCodes: []);

        AssertValidateResult([
            (null, null, RoadSegmentAttributeSide.Both, 1),
            (null, null, RoadSegmentAttributeSide.Both, 2)
        ], expectedErrorCodes: ["RoadSegmentStreetNameValueNotUniqueWithinSegment"]);

        AssertValidateResult([
            (null, null, RoadSegmentAttributeSide.Left, 1),
            (null, null, RoadSegmentAttributeSide.Left, 2)
        ], expectedErrorCodes: ["RoadSegmentStreetNameValueNotUniqueWithinSegment"]);
    }

    [Fact]
    public void WhenOnlyFromOrToIsNull_ThenError()
    {
        AssertValidateResult([
                (0, 5, RoadSegmentAttributeSide.Both, 1)
            ],
            segmentLength: 5,
            expectedErrorCodes: []);

        AssertValidateResult([
            (5, null, RoadSegmentAttributeSide.Both, 1)
        ], expectedErrorCodes: ["RoadSegmentStreetNameFromOrToPositionIsNull"]);

        AssertValidateResult([
            (null, 5, RoadSegmentAttributeSide.Both, 1)
        ], expectedErrorCodes: ["RoadSegmentStreetNameFromOrToPositionIsNull"]);
    }

    [Fact]
    public void WhenFromAndToAreNotNull_ThenFirstFromMustBeZero()
    {
        AssertValidateResult(
            [
                (1, 5, RoadSegmentAttributeSide.Both, 1)
            ],
            segmentLength: 5,
            expectedErrorCodes: ["RoadSegmentStreetNameFromPositionNotEqualToZero"]);
    }

    [Fact]
    public void WhenFromAndToAreNotNull_ThenFromAndToMustBeDifferent()
    {
        AssertValidateResult(
            [
                (0, 1, RoadSegmentAttributeSide.Both, 1),
                (1, 1, RoadSegmentAttributeSide.Both, 2)
            ],
            segmentLength: 1,
            expectedErrorCodes: ["RoadSegmentStreetNameHasLengthOfZero"]);
    }

    [Fact]
    public void WhenFromAndToAreNotNull_ThenFromAndToMustBeAdjacent()
    {
        AssertValidateResult(
            [
                (0, 1, RoadSegmentAttributeSide.Both, 1),
                (4, 5, RoadSegmentAttributeSide.Both, 2)
            ],
            segmentLength: 5,
            expectedErrorCodes: ["RoadSegmentStreetNameNotAdjacent"]);
    }

    [Fact]
    public void WhenFromAndToAreNotNull_ThenLastToMustBeEqualToSegmentLength()
    {
        AssertValidateResult(
            [
                (0, 1, RoadSegmentAttributeSide.Both, 1),
                (1, 2, RoadSegmentAttributeSide.Both, 2)
            ],
            segmentLength: 5,
            expectedErrorCodes: ["RoadSegmentStreetNameToPositionNotEqualToLength"]);
    }

    [Fact]
    public void WhenEuropeanRoadsAreNotUnique_ThenError()
    {
        AssertValidateResult(_fixture.Create<RoadSegmentAttributes>() with
        {
            EuropeanRoadNumbers = [EuropeanRoadNumber.E17, EuropeanRoadNumber.E17]
        }, ["RoadSegmentEuropeanRoadsNotUnique"]);
    }

    [Fact]
    public void WhenNationalRoadsAreNotUnique_ThenError()
    {
        AssertValidateResult(_fixture.Create<RoadSegmentAttributes>() with
        {
            NationalRoadNumbers = [NationalRoadNumber.Parse("N001"), NationalRoadNumber.Parse("N001")]
        }, ["RoadSegmentNationalRoadsNotUnique"]);
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_AccessRestriction()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>()
                .Add(null, _fixture.Create<RoadSegmentPosition>(), _fixture.Create<RoadSegmentAttributeSide>(), _fixture.Create<RoadSegmentAccessRestriction>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_Category()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>()
                .Add(null, _fixture.Create<RoadSegmentPosition>(), _fixture.Create<RoadSegmentAttributeSide>(), _fixture.Create<RoadSegmentCategory>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_Morphology()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>()
                .Add(null, _fixture.Create<RoadSegmentPosition>(), _fixture.Create<RoadSegmentAttributeSide>(), _fixture.Create<RoadSegmentMorphology>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_Status()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>()
                .Add(null, _fixture.Create<RoadSegmentPosition>(), _fixture.Create<RoadSegmentAttributeSide>(), _fixture.Create<RoadSegmentStatus>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_StreetNameId()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>()
                .Add(null, _fixture.Create<RoadSegmentPosition>(), _fixture.Create<RoadSegmentAttributeSide>(), _fixture.Create<StreetNameLocalId>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_MaintenanceAuthorityId()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>()
                .Add(null, _fixture.Create<RoadSegmentPosition>(), _fixture.Create<RoadSegmentAttributeSide>(), _fixture.Create<OrganizationId>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_SurfaceType()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>()
                .Add(null, _fixture.Create<RoadSegmentPosition>(), _fixture.Create<RoadSegmentAttributeSide>(), _fixture.Create<RoadSegmentSurfaceType>())
        });
    }

    private void EnsureValidatorIsUsedForAttribute(Func<RoadSegmentAttributes, RoadSegmentAttributes> attributesBuilder)
    {
        var attributes = _fixture.Create<RoadSegmentAttributes>();

        var problems = new RoadSegmentAttributesValidator().Validate(
            new RoadSegmentId(1),
            attributesBuilder(attributes),
            0);

        problems.HasError().Should().BeTrue();
        problems.Should().Contain(x => x.Reason.EndsWith("FromOrToPositionIsNull"));
    }

    private void AssertValidateResult((decimal? From, decimal? To, RoadSegmentAttributeSide Side, int Value)[] attributeValues, string[] expectedErrorCodes, double segmentLength = 0)
    {
        var attributes = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>();

        foreach (var attributeValue in attributeValues)
        {
            var from = attributeValue.From is not null
                ? new RoadSegmentPosition(attributeValue.From.Value)
                : (RoadSegmentPosition?)null;
            var to = attributeValue.To is not null
                ? new RoadSegmentPosition(attributeValue.To.Value)
                : (RoadSegmentPosition?)null;
            attributes.Add(from, to, attributeValue.Side, new StreetNameLocalId(attributeValue.Value));
        }

        AssertValidateResult(_fixture.Create<RoadSegmentAttributes>() with
        {
            StreetNameId = attributes
        }, expectedErrorCodes, segmentLength);
    }

    private void AssertValidateResult(RoadSegmentAttributes attributes, string[] expectedErrorCodes, double segmentLength = 0)
    {
        var problems = new RoadSegmentAttributesValidator().Validate(
            new RoadSegmentId(1),
            attributes,
            segmentLength);
        foreach (var problem in problems)
        {
            _testOutputHelper.WriteLine(problem.Describe());
        }

        if (expectedErrorCodes.Any())
        {
            foreach (var errorCode in expectedErrorCodes)
            {
                problems.Should().Contain(x => x.Reason == errorCode);
            }
        }
        else
        {
            problems.Should().BeEmpty();
        }
    }
}
