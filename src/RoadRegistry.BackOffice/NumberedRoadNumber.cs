namespace RoadRegistry.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    // public class NumberedRoadNumber : IEquatable<NumberedRoadNumber>
    // {
    //     private readonly string _value;
    //
    //     public static readonly NumberedRoadNumber[] All = ReadAllFromResource();
    //
    //     private static NumberedRoadNumber[] ReadAllFromResource()
    //     {
    //         using(var stream = Assembly
    //             .GetAssembly(typeof(NumberedRoadNumber))
    //             .GetManifestResourceStream(typeof(NumberedRoadNumber), "ident8.txt"))
    //         {
    //             if (stream != null)
    //                 using (var reader = new StreamReader(stream, Encoding.UTF8))
    //                 {
    //                     var numbers = new List<NumberedRoadNumber>(5813);
    //                     var line = reader.ReadLine();
    //                     while (line != null)
    //                     {
    //                         numbers.Add(new NumberedRoadNumber(line));
    //                         line = reader.ReadLine();
    //                     }
    //
    //                     return numbers.ToArray();
    //                 }
    //
    //             return Array.Empty<NumberedRoadNumber>();
    //         }
    //     }
    //
    //     private NumberedRoadNumber(string value)
    //     {
    //         _value = value;
    //     }
    //
    //     public static bool CanParse(string value)
    //     {
    //         if (value == null)
    //         {
    //             throw new ArgumentNullException(nameof(value));
    //         }
    //
    //         return Array.Find(All, candidate => candidate._value == value) != null;
    //     }
    //
    //     public static bool TryParse(string value, out NumberedRoadNumber parsed)
    //     {
    //         if (value == null)
    //         {
    //             throw new ArgumentNullException(nameof(value));
    //         }
    //
    //         parsed = Array.Find(All, candidate => candidate._value == value);
    //         return parsed != null;
    //     }
    //
    //     public static NumberedRoadNumber Parse(string value)
    //     {
    //         if (value == null)
    //         {
    //             throw new ArgumentNullException(nameof(value));
    //         }
    //
    //         if (!TryParse(value, out var parsed))
    //         {
    //             throw new FormatException($"The value {value} is not a well known numbered road number.");
    //         }
    //         return parsed;
    //     }
    //
    //     public bool Equals(NumberedRoadNumber other) => other != null && other._value == _value;
    //     public override bool Equals(object obj) => obj is NumberedRoadNumber type && Equals(type);
    //     public override int GetHashCode() => _value.GetHashCode();
    //     public override string ToString() => _value;
    //
    //     public static implicit operator string(NumberedRoadNumber instance) => instance.ToString();
    //     public static bool operator ==(NumberedRoadNumber left, NumberedRoadNumber right) => Equals(left, right);
    //     public static bool operator !=(NumberedRoadNumber left, NumberedRoadNumber right) => !Equals(left, right);
    // }

    public struct NumberedRoadNumber : IEquatable<NumberedRoadNumber>
    {
        private readonly char[] _value;

        public static readonly char[] RoadTypes =
        {
            'A',
            'B',
            'N',
            'R',
            'T'
        };

        private NumberedRoadNumber(char[] value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static bool CanParse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            return value.Length == 8
                   && Array.Exists(RoadTypes, candidate => candidate.Equals(value[0]))
                   && Enumerable.Range(1, 7).All(position => char.IsDigit(value[position]))
                   && (value[7] == '1' || value[7] == '2');
        }

        public static bool TryParse(string value, out NumberedRoadNumber parsed)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (value.Length != 8)
            {
                parsed = default;
                return false;
            }

            if (!Array.Exists(RoadTypes, candidate => candidate.Equals(value[0])))
            {
                parsed = default;
                return false;
            }

            if (!Enumerable.Range(1, 7).All(position => char.IsDigit(value[position])))
            {
                parsed = default;
                return false;
            }

            if (value[7] != '1' && value[7] != '2')
            {
                parsed = default;
                return false;
            }

            parsed = new NumberedRoadNumber(value.ToCharArray());
            return true;
        }

        public static NumberedRoadNumber Parse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (value.Length != 8)
            {
                throw new FormatException($"The numbered road number value must be exactly 8 characters long. Actual value was {value}.");
            }

            if (!Array.Exists(RoadTypes, candidate => candidate.Equals(value[0])))
            {
                throw new FormatException($"The numbered road number value must have a road type of {string.Join(',', RoadTypes)}. Actual value was {value}.");
            }

            if (!Enumerable.Range(1, 7).All(position => char.IsDigit(value[position])))
            {
                throw new FormatException($"The numbered road number value must end with digits. Actual value was {value}.");
            }

            if (value[7] != '1' && value[7] != '2')
            {
                throw new FormatException($"The numbered road number value must end with a digit that is either 1 or 2. Actual value was {value}.");
            }

            return new NumberedRoadNumber(value.ToCharArray());
        }

        public bool Equals(NumberedRoadNumber other) => other._value == _value;
        public override bool Equals(object obj) => obj is NumberedRoadNumber type && Equals(type);
        public override int GetHashCode() => new string(_value).GetHashCode();
        public override string ToString() => new string(_value);

        public static implicit operator string(NumberedRoadNumber instance) => instance.ToString();
        public static bool operator ==(NumberedRoadNumber left, NumberedRoadNumber right) => Equals(left, right);
        public static bool operator !=(NumberedRoadNumber left, NumberedRoadNumber right) => !Equals(left, right);
    }
}
