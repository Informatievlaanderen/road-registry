// ReSharper disable UnusedMember.Global

namespace RoadRegistry.Tests.BackOffice.Core;

public static class DynamicAttributePositionCases
{
    public static IEnumerable<object[]> NegativeFromPosition
    {
        get
        {
            yield return new object[] { decimal.MinValue };
            yield return new object[] { -0.1m };
        }
    }

    public static IEnumerable<object[]> ToPositionLessThanFromPosition
    {
        get
        {
            yield return new object[] { 0.0m, 0.0m };
            yield return new object[] { 0.1m, 0.0m };
            yield return new object[] { 0.1m, 0.1m };
        }
    }
}
