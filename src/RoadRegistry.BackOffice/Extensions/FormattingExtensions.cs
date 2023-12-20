using RoadRegistry.BackOffice.Core;
using System;

namespace RoadRegistry.BackOffice.Extensions
{
    public static class FormattingExtensions
    {
        public static string ToRoundedMeasurementString(this double value)
        {
            return Math.Round(value, Precisions.GeometryPrecision).ToInvariantString();
        }
    }
}
