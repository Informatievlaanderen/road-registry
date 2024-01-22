namespace RoadRegistry.BackOffice.Messages;

using System;

public static class StreetNameEvents
{
    public static readonly Type[] All =
    {
        typeof(StreetNameCreated),
        typeof(StreetNameModified),
        typeof(StreetNameRemoved)
    };
}
