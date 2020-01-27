namespace RoadRegistry.BackOffice
{
    using System;

    public class EuropeanRoadNumber : IEquatable<EuropeanRoadNumber>
    {
        private readonly string _value;
        public static readonly EuropeanRoadNumber E17 = new EuropeanRoadNumber(nameof(E17));
        public static readonly EuropeanRoadNumber E19 = new EuropeanRoadNumber(nameof(E19));
        public static readonly EuropeanRoadNumber E25 = new EuropeanRoadNumber(nameof(E25));
        public static readonly EuropeanRoadNumber E313 = new EuropeanRoadNumber(nameof(E313));
        public static readonly EuropeanRoadNumber E314 = new EuropeanRoadNumber(nameof(E314));
        public static readonly EuropeanRoadNumber E34 = new EuropeanRoadNumber(nameof(E34));
        public static readonly EuropeanRoadNumber E40 = new EuropeanRoadNumber(nameof(E40));
        public static readonly EuropeanRoadNumber E403 = new EuropeanRoadNumber(nameof(E403));
        public static readonly EuropeanRoadNumber E411 = new EuropeanRoadNumber(nameof(E411));
        public static readonly EuropeanRoadNumber E429 = new EuropeanRoadNumber(nameof(E429));


        public static readonly EuropeanRoadNumber[] All = {
            E17, E19, E25, E313, E314, E34,
            E40, E403, E411, E429
        };

        private EuropeanRoadNumber(string value)
        {
            _value = value;
        }

        public static bool CanParse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return Array.Find(All, candidate => candidate._value == value) != null;
        }

        public static bool TryParse(string value, out EuropeanRoadNumber parsed)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static EuropeanRoadNumber Parse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known european road number.");
            }
            return parsed;
        }

        public bool Equals(EuropeanRoadNumber other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is EuropeanRoadNumber type && Equals(type);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;

        public static implicit operator string(EuropeanRoadNumber instance) => instance.ToString();
        public static bool operator ==(EuropeanRoadNumber left, EuropeanRoadNumber right) => Equals(left, right);
        public static bool operator !=(EuropeanRoadNumber left, EuropeanRoadNumber right) => !Equals(left, right);
    }
}
