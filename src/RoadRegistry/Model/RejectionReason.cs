//namespace RoadRegistry.Model
//{
//    using System;
//
//    public static class Because
//    {
//
//    }
//
//    public class RejectionReason : IEquatable<RejectionReason>
//    {
//        private readonly string _value;
//        public static readonly RejectionReason LanesAreNotAdjacent = new RejectionReason(nameof(LanesAreNotAdjacent));
//
//        public static readonly RejectionReason[] All = {
//            LanesAreNotAdjacent
//        };
//
//        private RejectionReason(string value)
//        {
//            _value = value;
//        }
//
//        public static bool CanParse(string value)
//        {
//            if (value == null) throw new ArgumentNullException(nameof(value));
//            return Array.Find(All, candidate => candidate._value == value) != null;
//        }
//
//        public static bool TryParse(string value, out RejectionReason parsed)
//        {
//            if (value == null) throw new ArgumentNullException(nameof(value));
//            parsed = Array.Find(All, candidate => candidate._value == value);
//            return parsed != null;
//        }
//
//        public static RejectionReason Parse(string value)
//        {
//            if (value == null) throw new ArgumentNullException(nameof(value));
//            if (!TryParse(value, out var parsed))
//            {
//                throw new FormatException($"The value {value} is not a well known rejection reason.");
//            }
//            return parsed;
//        }
//
//        public bool Equals(RejectionReason other) => other != null && other._value == _value;
//        public override bool Equals(object obj) => obj is RejectionReason type && Equals(type);
//        public override int GetHashCode() => _value.GetHashCode();
//        public override string ToString() => _value;
//
//        public static implicit operator string(RejectionReason instance) => instance.ToString();
//        public static bool operator ==(RejectionReason left, RejectionReason right) => Equals(left, right);
//        public static bool operator !=(RejectionReason left, RejectionReason right) => !Equals(left, right);
//    }
//}
