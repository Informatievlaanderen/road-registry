namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using System.Collections.Immutable;
using System.Reflection;
using AutoFixture;
using FluentAssertions;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentAttributesEqualsOnlyNonDynamicAttributesTests
{
    private readonly Fixture _fixture;

    public RoadSegmentAttributesEqualsOnlyNonDynamicAttributesTests()
    {
        _fixture = new RoadNetworkTestData().Fixture;
    }

    [Fact]
    public void EnsureThatDynamicAttributesAreIgnored()
    {
        // Arrange
        var baseline = _fixture.Create<RoadSegmentAttributes>();

        // Find all dynamic attribute properties
        var dynamicProperties =
            typeof(RoadSegmentAttributes)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                    p.PropertyType.IsGenericType &&
                    p.PropertyType.GetGenericTypeDefinition() == typeof(RoadSegmentDynamicAttributeValues<>))
                .ToList();
        dynamicProperties.Should().NotBeEmpty("Otherwise this test is meaningless");

        // For every dynamic property: changing it must NOT affect EqualsOnlyNonDynamicAttributes
        foreach (var property in dynamicProperties)
        {
            var modified = ReplaceDynamicProperty(baseline, property);

            baseline.EqualsOnlyNonDynamicAttributes(modified).Should().BeTrue($"EqualsOnlyNonDynamicAttributes must ignore {property.Name}");
        }
        RoadSegmentAttributes ReplaceDynamicProperty(RoadSegmentAttributes original, PropertyInfo property)
        {
            var newValue = _fixture.Create(property.PropertyType);

            // use `with` + reflection
            var clone = original with { };
            property.SetValue(clone, newValue);

            return clone;
        }
    }

    [Fact]
    public void MustDetectChanges()
    {
        var fixture = new RoadNetworkTestData().Fixture;
        var baseline = fixture.Create<RoadSegmentAttributes>();

        baseline.EqualsOnlyNonDynamicAttributes(baseline).Should().BeTrue();

        // GeometryDrawMethod must be checked
        var geometryChanged = baseline with
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingeschetst
        };
        baseline.EqualsOnlyNonDynamicAttributes(geometryChanged).Should().BeFalse();

        // EuropeanRoadNumbers must be checked
        var europeanChanged = baseline with
        {
            EuropeanRoadNumbers = ImmutableList.Create(fixture.CreateWhichIsDifferentThan(baseline.EuropeanRoadNumbers.Single()))
        };
        baseline.EqualsOnlyNonDynamicAttributes(europeanChanged).Should().BeFalse();

        // NationalRoadNumbers must be checked
        var nationalChanged = baseline with
        {
            NationalRoadNumbers = ImmutableList.Create(fixture.CreateWhichIsDifferentThan(baseline.NationalRoadNumbers.Single()))
        };
        baseline.EqualsOnlyNonDynamicAttributes(nationalChanged).Should().BeFalse();
    }
}
