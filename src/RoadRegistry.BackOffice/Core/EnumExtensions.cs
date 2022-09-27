namespace RoadRegistry.BackOffice.Core;

using System;

public static class EnumExtensions
{
    public static string GetDisplayName<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        return Enum.GetName(value);
    }
}
