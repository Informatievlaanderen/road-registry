namespace RoadRegistry;

using System;

public static class StreamKeyFactory
{
    public static string Create<TIdentifier>(Type aggregateType, TIdentifier identifier)
    {
        return $"{aggregateType.Name}-{identifier}";
    }
}
