using RoadRegistry.BackOffice.Core;
using System;

namespace RoadRegistry.BackOffice.Extensions
{
    public static class FormattingExtensions
    {
        public static string ToRoundedMeasurementString(this double value)
        {
            return value.ToRoundedMeasurement().ToInvariantString();
        }

        public static double ToRoundedMeasurement(this double value)
        {
            return Math.Round(value, Precisions.GeometryPrecision);
        }

        public static string ToRoundedMeasurementString(this decimal value)
        {
            return value.ToRoundedMeasurement().ToInvariantString();
        }

        public static decimal ToRoundedMeasurement(this decimal value)
        {
            return Math.Round(value, Precisions.GeometryPrecision);
        }
    }
}
