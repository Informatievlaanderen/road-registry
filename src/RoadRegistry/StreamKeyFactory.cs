namespace RoadRegistry;

using System;

public class StreamKeyFactory
{
    public static string Create<TIdentifier>(Type entityType, TIdentifier identifier)
    {
        return $"{entityType.Name}-{identifier}";
    }
}
