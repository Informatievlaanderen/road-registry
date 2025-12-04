// namespace RoadRegistry.BackOffice
// {
//     using System;
//     using System.Diagnostics.Contracts;
//     using System.Globalization;
//
//     public readonly struct RoadSegmentMeasurement : IEquatable<RoadSegmentMeasurement>, IComparable<RoadSegmentMeasurement>
//     {
//         public static readonly RoadSegmentMeasurement Zero = new RoadSegmentMeasurement(0.0m);
//
//         private readonly decimal _value;
//
//         public RoadSegmentMeasurement(decimal value)
//         {
//             if (value < 0.0m)
//                 throw new ArgumentOutOfRangeException(nameof(value), value, "The road segment measurement must be greater than or equal to zero.");
//
//             _value = value;
//         }
//
//         public static RoadSegmentMeasurement FromDouble(double value)
//         {
//             return new RoadSegmentMeasurement(Convert.ToDecimal(value));
//         }
//
//         public static bool Accepts(double value)
//         {
//             return value >= 0.0;
//         }
//
//         public static bool Accepts(decimal value)
//         {
//             return value >= 0.0m;
//         }
//
//         [Pure]
//         public decimal ToDecimal() => _value;
//         [Pure]
//         public double ToDouble() => decimal.ToDouble(_value);
//         public bool Equals(RoadSegmentMeasurement other) => _value.Equals(other._value);
//         public bool Equals(RoadSegmentMeasurement other, double tolerance) => Math.Abs(_value - other._value) < Convert.ToDecimal(tolerance);
//         public override bool Equals(object other) => other is RoadSegmentMeasurement id && Equals(id);
//         public override int GetHashCode() => _value.GetHashCode();
//         public override string ToString() => _value.ToString(CultureInfo.InvariantCulture);
//         public int CompareTo(RoadSegmentMeasurement other) => _value.CompareTo(other._value);
//         public int CompareTo(RoadSegmentMeasurement other, double tolerance)
//         {
//             if (Equals(other, tolerance))
//             {
//                 return 0;
//             }
//             return _value.CompareTo(other._value);
//         }
//
//         public static bool operator ==(RoadSegmentMeasurement left, RoadSegmentMeasurement right) => left.Equals(right);
//         public static bool operator !=(RoadSegmentMeasurement left, RoadSegmentMeasurement right) => !left.Equals(right);
//         public static bool operator <(RoadSegmentMeasurement left, RoadSegmentMeasurement right) => left.CompareTo(right) == -1;
//         public static bool operator <=(RoadSegmentMeasurement left, RoadSegmentMeasurement right) => left.CompareTo(right) <= 0;
//         public static bool operator >(RoadSegmentMeasurement left, RoadSegmentMeasurement right) => left.CompareTo(right) == 1;
//         public static bool operator >=(RoadSegmentMeasurement left, RoadSegmentMeasurement right) => left.CompareTo(right) >= 0;
//         public static implicit operator decimal(RoadSegmentMeasurement instance) => instance._value;
//     }
// }


