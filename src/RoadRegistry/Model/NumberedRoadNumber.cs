namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;

    public class NumberedRoadNumber : IEquatable<NumberedRoadNumber>
    {
        private readonly string _value;

        public static readonly NumberedRoadNumber[] All = ReadAllFromResource();

        private static NumberedRoadNumber[] ReadAllFromResource()
        {
            using(var stream = Assembly
                .GetAssembly(typeof(NumberedRoadNumber))
                .GetManifestResourceStream(typeof(NumberedRoadNumber), "ident8.txt"))
            {
                if (stream != null)
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        var numbers = new List<NumberedRoadNumber>(5813);
                        var line = reader.ReadLine();
                        while (line != null)
                        {
                            numbers.Add(new NumberedRoadNumber(line));
                            line = reader.ReadLine();
                        }

                        return numbers.ToArray();
                    }

                return Array.Empty<NumberedRoadNumber>();
            }
        }

        private NumberedRoadNumber(string value)
        {
            _value = value;
        }

        public static bool CanParse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return Array.Find(All, candidate => candidate._value == value) != null;
        }

        public static bool TryParse(string value, out NumberedRoadNumber parsed)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static NumberedRoadNumber Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known numbered road number.");
            }
            return parsed;
        }

        public bool Equals(NumberedRoadNumber other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is NumberedRoadNumber type && Equals(type);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;

        public static implicit operator string(NumberedRoadNumber instance) => instance.ToString();
        public static bool operator ==(NumberedRoadNumber left, NumberedRoadNumber right) => Equals(left, right);
        public static bool operator !=(NumberedRoadNumber left, NumberedRoadNumber right) => !Equals(left, right);
    }
}
