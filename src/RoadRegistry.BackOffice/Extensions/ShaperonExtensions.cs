namespace RoadRegistry.BackOffice.Extensions
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public static class ShaperonExtensions
    {
        public static string GetValue(this DbaseString field)
        {
            return field.Value;
        }

        public static DbaseInt32 SetValue(this DbaseInt32 field, int? value)
        {
            if (value is null)
            {
                field.Reset();
            }
            else
            {
                field.Value = value.Value;
            }

            return field;
        }
        public static int? GetValue(this DbaseInt32 field)
        {
            return field.HasValue ? field.Value : null;
        }
        public static int? GetValue(this DbaseNullableInt32 field)
        {
            return field.Value;
        }

        public static DbaseDouble SetValue(this DbaseDouble field, double? value)
        {
            if (value is null)
            {
                field.Reset();
            }
            else
            {
                field.Value = value.Value;
            }

            return field;
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
}
