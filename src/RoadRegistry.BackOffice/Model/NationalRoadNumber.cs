namespace RoadRegistry.BackOffice.Model
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;

    public class NationalRoadNumber : IEquatable<NationalRoadNumber>
    {
        private readonly string _value;

        public static readonly NationalRoadNumber[] All = ReadAllFromResource();

        private static NationalRoadNumber[] ReadAllFromResource()
        {
            using(var stream = Assembly
                .GetAssembly(typeof(NationalRoadNumber))
                .GetManifestResourceStream(typeof(NationalRoadNumber), "ident2.txt"))
            {
                if (stream != null)
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        var numbers = new List<NationalRoadNumber>(747);
                        var line = reader.ReadLine();
                        while (line != null)
                        {
                            numbers.Add(new NationalRoadNumber(line));
                            line = reader.ReadLine();
                        }

                        return numbers.ToArray();
                    }

                return Array.Empty<NationalRoadNumber>();
            }
        }

        private NationalRoadNumber(string value)
        {
            _value = value;
        }

        public static bool CanParse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return Array.Find(All, candidate => candidate._value == value) != null;
        }

        public static bool TryParse(string value, out NationalRoadNumber parsed)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static NationalRoadNumber Parse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known national road number.");
            }
            return parsed;
        }

        public bool Equals(NationalRoadNumber other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is NationalRoadNumber type && Equals(type);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;

        public static implicit operator string(NationalRoadNumber instance) => instance.ToString();
        public static bool operator ==(NationalRoadNumber left, NationalRoadNumber right) => Equals(left, right);
        public static bool operator !=(NationalRoadNumber left, NationalRoadNumber right) => !Equals(left, right);
    }
}
