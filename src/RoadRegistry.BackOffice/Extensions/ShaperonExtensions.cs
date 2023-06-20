namespace RoadRegistry.BackOffice.Extensions
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public static class ShaperonExtensions
    {
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
    }
}
