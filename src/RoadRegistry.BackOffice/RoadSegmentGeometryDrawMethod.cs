// ReSharper disable InconsistentNaming
namespace RoadRegistry.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class RoadSegmentGeometryDrawMethod : IEquatable<RoadSegmentGeometryDrawMethod>
    {
        public static readonly RoadSegmentGeometryDrawMethod Outlined =
            new RoadSegmentGeometryDrawMethod(
                nameof(Outlined),
                new DutchTranslation(
                    1,
                    "ingeschetst",
                    "Wegsegment waarvan de geometrie ingeschetst werd."
                )
            );

        public static readonly RoadSegmentGeometryDrawMethod Measured =
            new RoadSegmentGeometryDrawMethod(
                nameof(Measured),
                new DutchTranslation(
                    2,
                    "ingemeten",
                    "Wegsegment waarvan de geometrie ingemeten werd (bv. overgenomen uit as-built-plan of andere dataset)."
                )
            );
        public static readonly RoadSegmentGeometryDrawMethod Measured_according_to_GRB_specifications =
            new RoadSegmentGeometryDrawMethod(
                nameof(Measured_according_to_GRB_specifications),
                new DutchTranslation(
                    3,
                    "ingemeten volgens GRB-specificaties",
                    "Wegsegment waarvan de geometrie werd ingemeten volgens GRB-specificaties."
                )
            );

        public static readonly RoadSegmentGeometryDrawMethod[] All = {
            Outlined, Measured, Measured_according_to_GRB_specifications
        };

        public static readonly IReadOnlyDictionary<int, RoadSegmentGeometryDrawMethod> ByIdentifier =
            All.ToDictionary(key => key.Translation.Identifier);

        private readonly string _value;

        private RoadSegmentGeometryDrawMethod(string value, DutchTranslation dutchTranslation)
        {
            _value = value;
            Translation = dutchTranslation;
        }

        public DutchTranslation Translation { get; }

        public static bool CanParse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return Array.Find(All, candidate => candidate._value == value) != null;
        }

        public static bool TryParse(string value, out RoadSegmentGeometryDrawMethod parsed)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static RoadSegmentGeometryDrawMethod Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known road segment geometry draw method.");
            }
            return parsed;
        }

        public bool Equals(RoadSegmentGeometryDrawMethod other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is RoadSegmentGeometryDrawMethod type && Equals(type);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;
        public static implicit operator string(RoadSegmentGeometryDrawMethod instance) => instance.ToString();
        public static bool operator ==(RoadSegmentGeometryDrawMethod left, RoadSegmentGeometryDrawMethod right) => Equals(left, right);
        public static bool operator !=(RoadSegmentGeometryDrawMethod left, RoadSegmentGeometryDrawMethod right) => !Equals(left, right);

        public class DutchTranslation
        {
            internal DutchTranslation(int identifier, string name, string description)
            {
                Identifier = identifier;
                Name = name;
                Description = description;
            }

            public int Identifier { get; }

            public string Name { get; }

            public string Description { get; }
        }
    }
}
