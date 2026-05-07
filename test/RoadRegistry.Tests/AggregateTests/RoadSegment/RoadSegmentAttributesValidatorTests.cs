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

    [Theory]
    [MemberData(nameof(HappyFlowTestsCases))]
    public void HappyFlow(HappyFlowTestCase testCase)
    {
        AssertValidateResult(
            testCase.Values,
            segmentLength: testCase.Values.Max(x => x.To),
            expectedErrorCodes: []);
    }
    public static IEnumerable<object[]> HappyFlowTestsCases()
    {
        yield return [new HappyFlowTestCase(
        [
            (0, 10, RoadSegmentAttributeSide.Both, 1)
        ])];
        yield return [new HappyFlowTestCase(
        [
            (0, 10, RoadSegmentAttributeSide.Left, 1),
            (0, 10, RoadSegmentAttributeSide.Right, 1),
        ])];
        yield return [new HappyFlowTestCase(
        [
            (0, 10, RoadSegmentAttributeSide.Left, 1),
            (0, 10, RoadSegmentAttributeSide.Right, 2),
        ])];
        yield return [new HappyFlowTestCase(
        [
            (0, 5, RoadSegmentAttributeSide.Left, 1),
            (0, 10, RoadSegmentAttributeSide.Right, 2),
            (5, 10, RoadSegmentAttributeSide.Left, 3),
        ])];
        yield return [new HappyFlowTestCase(
        [
            (0, 5, RoadSegmentAttributeSide.Left, 1),
            (5, 10, RoadSegmentAttributeSide.Both, 2),
            (0, 5, RoadSegmentAttributeSide.Right, 2),
            (10, 15, RoadSegmentAttributeSide.Left, 1),
            (10, 15, RoadSegmentAttributeSide.Right, 1),
        ])];
    }
    public sealed class HappyFlowTestCase
    {
        public IReadOnlyCollection<(double From, double To, RoadSegmentAttributeSide Side, int Value)> Values { get; }

        public HappyFlowTestCase(ICollection<(int, int, RoadSegmentAttributeSide, int)> values)
        {
            Values = values.Select(x => ((double)x.Item1, (double)x.Item2, x.Item3, x.Item4)).ToArray();
        }
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
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, RoadSegmentAccessRestrictionV2.OpenbareWeg)
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, RoadSegmentAccessRestrictionV2.PrivateWeg)
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_Category()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, RoadSegmentCategoryV2.EuropeseHoofdweg)
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, RoadSegmentCategoryV2.InterlokaleWeg)
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_Morphology()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, RoadSegmentMorphologyV2.Aardeweg)
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, RoadSegmentMorphologyV2.Autosnelweg)
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_StreetNameId()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, StreetNameLocalId.NotApplicable)
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, StreetNameLocalId.Unknown)
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_MaintenanceAuthorityId()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, OrganizationId.Unknown)
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, OrganizationId.DigitaalVlaanderen)
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_SurfaceType()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, RoadSegmentSurfaceTypeV2.Halfverhard)
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, RoadSegmentSurfaceTypeV2.Onverhard)
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_CarAccessForward()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            CarAccessForward = new RoadSegmentDynamicAttributeValues<bool>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, false)
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, true)
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_CarAccessBackward()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            CarAccessBackward = new RoadSegmentDynamicAttributeValues<bool>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, false)
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, true)
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_BikeAccessForward()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            BikeAccessForward = new RoadSegmentDynamicAttributeValues<bool>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, false)
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, true)
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_BikeAccessBackward()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            BikeAccessBackward = new RoadSegmentDynamicAttributeValues<bool>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, false)
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, true)
        });
    }

    [Fact]
    public void EnsureValidatorIsUsedForAttribute_PedestrianAccess()
    {
        EnsureValidatorIsUsedForAttribute(change => change with
        {
            PedestrianAccess = new RoadSegmentDynamicAttributeValues<bool>()
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, false)
                .Add(new(RoadSegmentPositionV2.Zero, RoadSegmentPositionV2.Zero), RoadSegmentAttributeSide.Both, true)
        });
    }

    private void EnsureValidatorIsUsedForAttribute(Func<RoadSegmentAttributes, RoadSegmentAttributes> attributesBuilder)
    {
        var attributes = _fixture.Create<RoadSegmentAttributes>();

        var problems = new RoadSegmentAttributesValidator().Validate(
            attributesBuilder(attributes),
            0);

        problems.Should().Contain(x => x.Reason.EndsWith("ValueNotUniqueWithinSegment"));
    }

    private void AssertValidateResult(IReadOnlyCollection<(double From, double To, RoadSegmentAttributeSide Side, int Value)> attributeValues, string[] expectedErrorCodes, double segmentLength = 0)
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
