namespace RoadRegistry.BackOffice
{
    using System;

    // https://statbel.fgov.be/nl/over-statbel/methodologie/classificaties/geografie
    public readonly struct MunicipalityNISCode : IEquatable<MunicipalityNISCode>
    {
        public const int ExactLength = 5;

        private readonly string _value;

        public MunicipalityNISCode(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.Length != ExactLength) throw new ArgumentException($"The municipality nis code must be exactly {ExactLength} characters long. The actual value was '{value}'.", nameof(value));
            _value = value;
        }

        public bool Equals(MunicipalityNISCode other) => _value == other._value;
        public override bool Equals(object obj) => obj is MunicipalityNISCode other && Equals(other);
        public override int GetHashCode() => _value?.GetHashCode() ?? 0;
        public override string ToString() => _value;
        public static implicit operator string(MunicipalityNISCode instance) => instance.ToString();
        public static bool operator ==(MunicipalityNISCode left, MunicipalityNISCode right) => left.Equals(right);
        public static bool operator !=(MunicipalityNISCode left, MunicipalityNISCode right) => !left.Equals(right);
    }
}
