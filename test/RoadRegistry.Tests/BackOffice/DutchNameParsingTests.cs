namespace RoadRegistry.Tests.BackOffice;

using System.Collections;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using RoadRegistry.RoadSegment.ValueObjects;

// Values given by name (e.g. 'niet van toepassing', 'gepland', 'links') are accepted whatever their casing.
public class DutchNameParsingTests
{
    // Every value object that parses its Dutch name from a list of known values with a DutchTranslation.
    private static Type[] FindTypesWithTranslatedValues()
    {
        return typeof(RoadSegmentAttributeSide).Assembly.GetTypes()
            .Where(x => x.GetMethod("ParseUsingDutchName", BindingFlags.Public | BindingFlags.Static, [typeof(string)]) is not null
                        && x.GetField("All", BindingFlags.Public | BindingFlags.Static)?.FieldType.IsArray == true
                        && x.GetProperty("Translation") is not null)
            .OrderBy(x => x.FullName)
            .ToArray();
    }

    public static TheoryData<Type> TypesWithTranslatedValues() => new(FindTypesWithTranslatedValues());

    [Fact]
    public void AllTypesWithTranslatedValuesAreFound()
    {
        FindTypesWithTranslatedValues().Should().Contain([
            typeof(RoadSegmentAttributeSide),
            typeof(RoadSegmentStatusV2),
            typeof(RoadSegmentMorphologyV2),
            typeof(RoadSegmentLaneDirection)
        ]);
    }

    [Theory]
    [MemberData(nameof(TypesWithTranslatedValues))]
    public void DutchNamesAreParsedCaseInsensitively(Type type)
    {
        var parse = type.GetMethod("ParseUsingDutchName", BindingFlags.Public | BindingFlags.Static, [typeof(string)])!;
        var translation = type.GetProperty("Translation")!;

        foreach (var candidate in (IEnumerable)type.GetField("All", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!)
        {
            var name = (string)translation.PropertyType.GetProperty("Name")!.GetValue(translation.GetValue(candidate))!;

            // Also guards against two values whose names only differ in casing: the parse would then return the wrong one.
            parse.Invoke(null, [name.ToUpperInvariant()]).Should().Be(candidate, $"'{name.ToUpperInvariant()}' should parse as {type.Name} '{name}'");
        }
    }

    [Theory]
    [InlineData("niet gekend")]
    [InlineData("Niet Gekend")]
    [InlineData("NIET VAN TOEPASSING")]
    public void DutchNameMappingsAreParsedCaseInsensitively(string value)
    {
        var expectedStreetName = value.StartsWith("niet gekend", StringComparison.OrdinalIgnoreCase) ? StreetNameLocalId.Unknown : StreetNameLocalId.NotApplicable;
        StreetNameLocalId.ParseUsingDutchName(value).Should().Be(expectedStreetName);

        var expectedWidth = value.StartsWith("niet gekend", StringComparison.OrdinalIgnoreCase) ? RoadSegmentWidth.Unknown : RoadSegmentWidth.NotApplicable;
        RoadSegmentWidth.ParseUsingDutchName(value).Should().Be(expectedWidth);

        var expectedLaneCount = value.StartsWith("niet gekend", StringComparison.OrdinalIgnoreCase) ? RoadSegmentLaneCount.Unknown : RoadSegmentLaneCount.NotApplicable;
        RoadSegmentLaneCount.ParseUsingDutchName(value).Should().Be(expectedLaneCount);
    }

    [Fact]
    public void NumberedRoadOrdinalUnknownIsParsedCaseInsensitively()
    {
        RoadSegmentNumberedRoadOrdinal.ParseUsingDutchName("NIET GEKEND").Should().Be(RoadSegmentNumberedRoadOrdinal.Unknown);
    }
}
