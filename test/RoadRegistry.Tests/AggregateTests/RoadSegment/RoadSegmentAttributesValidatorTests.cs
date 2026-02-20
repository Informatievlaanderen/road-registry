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
        _fixture = new RoadNetworkTestDataV2().Fixture;
    }

    [Fact]
    public void FirstFromMustBeZero()
    {
        AssertValidateResult(
            [
                (1, 5, RoadSegmentAttributeSide.Both, 1)
            ],
            segmentLength: 5,
            expectedErrorCodes: ["RoadSegmentStreetNameFromPositionNotEqualToZero"]);
    }

    [Fact]
    public void FromAndToMustBeDifferent()
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
    public void FromAndToMustBeAdjacent()
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
    public void LastToMustBeEqualToSegmentLength()
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
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<RoadSegmentAccessRestrictionV2>())
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<RoadSegmentAccessRestrictionV2>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_Category()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<RoadSegmentCategoryV2>())
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<RoadSegmentCategoryV2>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_Morphology()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<RoadSegmentMorphologyV2>())
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<RoadSegmentMorphologyV2>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_StreetNameId()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<StreetNameLocalId>())
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<StreetNameLocalId>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_MaintenanceAuthorityId()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<OrganizationId>())
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<OrganizationId>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_SurfaceType()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<RoadSegmentSurfaceTypeV2>())
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<RoadSegmentSurfaceTypeV2>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_CarAccessForward()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            CarAccessForward = new RoadSegmentDynamicAttributeValues<bool>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<bool>())
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<bool>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_CarAccessBackward()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            CarAccessBackward = new RoadSegmentDynamicAttributeValues<bool>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<bool>())
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<bool>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_BikeAccessForward()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            BikeAccessForward = new RoadSegmentDynamicAttributeValues<bool>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<bool>())
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<bool>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_BikeAccessBackward()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            BikeAccessBackward = new RoadSegmentDynamicAttributeValues<bool>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<bool>())
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<bool>())
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_PedestrianAccess()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<bool>())
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, _fixture.Create<bool>())
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

    private void AssertValidateResult((double From, double To, RoadSegmentAttributeSide Side, int Value)[] attributeValues, string[] expectedErrorCodes, double segmentLength = 0)
    {
        var attributes = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>();

        foreach (var attributeValue in attributeValues)
        {
            var from = new RoadSegmentPositionV2(attributeValue.From);
            var to = new RoadSegmentPositionV2(attributeValue.To);
            attributes.Add(new RoadSegmentPositionCoverage(from, to!), attributeValue.Side, new StreetNameLocalId(attributeValue.Value));
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
