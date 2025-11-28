namespace RoadRegistry.ValueObjects;

using System.Collections.Generic;

public static class RoadTypes
{
    public static readonly IReadOnlyCollection<char> All = new[]
    {
        'A',
        'B',
        'N',
        'R',
        'T'
    };
}
