namespace RoadRegistry.BackOffice.Model
{
    using System;

    public class RoadSegmentSurfaceType : IEquatable<RoadSegmentSurfaceType>
    {
        public static readonly RoadSegmentSurfaceType NotApplicable =
            new RoadSegmentSurfaceType(
                nameof(NotApplicable),
                new DutchTranslation(
                    -9,
                    "niet van toepassing",
                    "Niet van toepassing"
                )
            );
        public static readonly RoadSegmentSurfaceType Unknown =
            new RoadSegmentSurfaceType(
                nameof(Unknown),
                new DutchTranslation(
                    -8,
                    "niet gekend",
                    "Geen informatie beschikbaar"
                )
            );
        public static readonly RoadSegmentSurfaceType SolidSurface =
            new RoadSegmentSurfaceType(
                nameof(SolidSurface),
                new DutchTranslation(
                    1,
                    "weg met vaste verharding",
                    "Weg met een wegdek bestaande uit in vast verband aangebrachte materialen zoals asfalt, beton, klinkers, kasseien, enz."
                )
            );
        public static readonly RoadSegmentSurfaceType LooseSurface =
            new RoadSegmentSurfaceType(
                nameof(LooseSurface),
                new DutchTranslation(
                    2,
                    "weg met losse verharding",
                    "Weg met een wegverharding bestaande uit losliggende materialen (bv. grind, kiezel, steenslag, puin, enz.)."
                )
            );

        public static readonly RoadSegmentSurfaceType[] All = {
            NotApplicable, Unknown, SolidSurface, LooseSurface
        };

        private readonly string _value;
        private readonly DutchTranslation _dutchTranslation;

        private RoadSegmentSurfaceType(string value, DutchTranslation dutchTranslation)
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

        public static bool TryParse(string value, out RoadSegmentSurfaceType parsed)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static RoadSegmentSurfaceType Parse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known type of road surface.");
            }
            return parsed;
        }

        public bool Equals(RoadSegmentSurfaceType other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is RoadSegmentSurfaceType type && Equals(type);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;
        public static implicit operator string(RoadSegmentSurfaceType instance) => instance.ToString();
        public static bool operator ==(RoadSegmentSurfaceType left, RoadSegmentSurfaceType right) => Equals(left, right);
        public static bool operator !=(RoadSegmentSurfaceType left, RoadSegmentSurfaceType right) => !Equals(left, right);

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
