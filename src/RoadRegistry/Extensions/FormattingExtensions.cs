namespace RoadRegistry.Extensions
{
    using System;

    public static class FormattingExtensions
    {
        public static string ToRoundedMeasurementString(this double value)
        {
            return value.ToRoundedMeasurement().ToInvariantString();
        }
        public static string ToRoundedMeasurementString(this decimal value)
        {
            return value.ToRoundedMeasurement().ToInvariantString();
        }

        public static double ToRoundedMeasurement(this double value)
        {
            return Math.Round(value, Precisions.GeometryPrecision);
        }
        public static decimal ToRoundedMeasurement(this decimal value)
        {
            return Math.Round(value, Precisions.GeometryPrecision);
        }

        public static double RoundToCm(this double value)
        {
            return Math.Round(value, Precisions.GeometryPrecisionV2);
        }

        // Whether a measurement carries more precision than the register works in. A client computing with doubles can
        // send 0.30000000000000004 where it means 0.3, so anything within a thousandth of a millimetre of a centimetre
        // is taken to be that centimetre; 0.125 is a measurement that would silently move when rounded.
        public static bool IsMorePreciseThanCm(this double value)
        {
            return Math.Abs(value - value.RoundToCm()) > 1e-6;
        }
    }
}
