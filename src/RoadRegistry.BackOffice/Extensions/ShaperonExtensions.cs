namespace RoadRegistry.BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.Shaperon;

public static class ShaperonExtensions
{
    public static string GetValue(this DbaseString field)
    {
        return field.Value;
    }

    public static int? GetValue(this DbaseInt32 field)
    {
        return field.HasValue ? field.Value : null;
    }

    public static int? GetValue(this DbaseNullableInt32 field)
    {
        return field.Value;
    }

    public static double? GetValue(this DbaseDouble field)
    {
        return field.HasValue ? field.Value : null;
    }

    public static double? GetValue(this DbaseNullableDouble field)
    {
        return field.Value;
    }
}
