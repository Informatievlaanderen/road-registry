namespace RoadRegistry.BackOffice.Model
{
    using System;

    public class GradeSeparatedJunctionType : IEquatable<GradeSeparatedJunctionType>
    {
        public static readonly GradeSeparatedJunctionType Unknown =
            new GradeSeparatedJunctionType(
                nameof(Unknown),
                new DutchTranslation(
                    -8,
                    "niet gekend",
                    "Geen informatie beschikbaar."
                )
            );
        public static readonly GradeSeparatedJunctionType Tunnel =
            new GradeSeparatedJunctionType(
                nameof(Tunnel),
                new DutchTranslation(
                    1,
                    "tunnel",
                    "Een tunnel is een doorgang voor een weg, spoorweg, aardeweg of pad die onder de grond, onder water of in een langwerpige, overdekte uitgraving is gelegen."
                )
            );
        public static readonly GradeSeparatedJunctionType Bridge =
            new GradeSeparatedJunctionType(
                nameof(Bridge),
                new DutchTranslation(
                    2,
                    "brug",
                    "Een brug is een doorgang voor een weg, spoorweg, aardeweg of pad die boven de grond of boven water gelegen is. Een brug kan vast of beweegbaar zijn."
                )
            );

        public static readonly GradeSeparatedJunctionType[] All = {
            Unknown, Tunnel, Bridge
        };

        private readonly string _value;
        private readonly DutchTranslation _dutchTranslation;

        private GradeSeparatedJunctionType(string value, DutchTranslation dutchTranslation)
        {
            _value = value;
            _dutchTranslation = dutchTranslation;
        }

        public DutchTranslation Translation => _dutchTranslation;

        public static bool CanParse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return Array.Find(All, candidate => candidate._value == value) != null;
        }

        public static bool TryParse(string value, out GradeSeparatedJunctionType parsed)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static GradeSeparatedJunctionType Parse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known type of grade separated junction.");
            }
            return parsed;
        }

        public bool Equals(GradeSeparatedJunctionType other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is GradeSeparatedJunctionType type && Equals(type);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;
        public static implicit operator string(GradeSeparatedJunctionType instance) => instance.ToString();
        public static bool operator ==(GradeSeparatedJunctionType left, GradeSeparatedJunctionType right) => Equals(left, right);
        public static bool operator !=(GradeSeparatedJunctionType left, GradeSeparatedJunctionType right) => !Equals(left, right);

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
