namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using AutoFixture;
using FluentAssertions;
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
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>()
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<RoadSegmentAccessRestrictionV2>())
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<RoadSegmentAccessRestrictionV2>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_Category()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>()
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<RoadSegmentCategoryV2>())
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<RoadSegmentCategoryV2>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_Morphology()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>()
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<RoadSegmentMorphologyV2>())
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<RoadSegmentMorphologyV2>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_Status()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>()
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<RoadSegmentStatusV2>())
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<RoadSegmentStatusV2>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_StreetNameId()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>()
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<StreetNameLocalId>())
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<StreetNameLocalId>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_MaintenanceAuthorityId()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>()
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<OrganizationId>())
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<OrganizationId>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_SurfaceType()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>()
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<RoadSegmentSurfaceTypeV2>())
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<RoadSegmentSurfaceTypeV2>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_CarAccess()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            CarAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>()
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<VehicleAccess>())
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<VehicleAccess>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_BikeAccess()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            BikeAccess = new RoadSegmentDynamicAttributeValues<VehicleAccess>()
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<VehicleAccess>())
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<VehicleAccess>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_PedestrianAccess()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>()
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<bool>())
                .Add(null, RoadSegmentAttributeSide.Both, _fixture.Create<bool>())
        });
    }

    private void EnsureValidatorIsUsedForAttribute(Func<RoadSegmentAttributes, RoadSegmentAttributes> attributesBuilder)
    {
        var attributes = _fixture.Create<RoadSegmentAttributes>();

        var problems = new RoadSegmentAttributesValidator().Validate(
            new RoadSegmentId(1),
            attributesBuilder(attributes),
            0);

        problems.Should().Contain(x => x.Reason.EndsWith("ValueNotUniqueWithinSegment"));
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
            attributes.Add(from is not null ? new RoadSegmentPositionCoverage(from.Value, to!.Value) : null, attributeValue.Side, new StreetNameLocalId(attributeValue.Value));
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
