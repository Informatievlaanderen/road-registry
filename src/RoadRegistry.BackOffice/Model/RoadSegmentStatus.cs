namespace RoadRegistry.BackOffice.Model
{
    using System;

    public class RoadSegmentStatus : IEquatable<RoadSegmentStatus>
    {
        public static readonly RoadSegmentStatus Unknown =
            new RoadSegmentStatus(
                nameof(Unknown),
                new DutchTranslation(
                    -8,
                    "niet gekend",
                    "Geen informatie beschikbaar"

                )
            );
        public static readonly RoadSegmentStatus PermitRequested =
            new RoadSegmentStatus(
                nameof(PermitRequested),
                new DutchTranslation(
                    1,
                    "vergunning aangevraagd",
                    "Geometrie komt voor op officieel document in behandeling."
                )
            );
        public static readonly RoadSegmentStatus BuildingPermitGranted =
            new RoadSegmentStatus(
                nameof(BuildingPermitGranted),
                new DutchTranslation(
                    2,
                    "bouwvergunning verleend",
                    "Geometrie komt voor op goedgekeurd, niet vervallen bouwdossier."
                )
            );
        public static readonly RoadSegmentStatus UnderConstruction =
            new RoadSegmentStatus(
                nameof(UnderConstruction),
                new DutchTranslation(
                    3,
                    "in aanbouw",
                    "Aanvang der werken is gemeld."
                )
            );
        public static readonly RoadSegmentStatus InUse =
            new RoadSegmentStatus(
                nameof(InUse),
                new DutchTranslation(
                    4,
                    "in gebruik",
                    "Werken zijn opgeleverd."
                )
            );
        public static readonly RoadSegmentStatus OutOfUse =
            new RoadSegmentStatus(
                nameof(OutOfUse),
                new DutchTranslation(
                    5,
                    "buiten gebruik",
                    "Fysieke weg is buiten gebruik gesteld maar niet gesloopt."
                )
            );

        public static readonly RoadSegmentStatus[] All = {
            Unknown, PermitRequested, BuildingPermitGranted, UnderConstruction, InUse, OutOfUse
        };

        private readonly string _value;
        private readonly DutchTranslation _dutchTranslation;

        private RoadSegmentStatus(string value, DutchTranslation dutchTranslation)
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

        public static bool TryParse(string value, out RoadSegmentStatus parsed)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static RoadSegmentStatus Parse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known road segment status.");
            }
            return parsed;
        }

        public bool Equals(RoadSegmentStatus other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is RoadSegmentStatus type && Equals(type);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;
        public static implicit operator string(RoadSegmentStatus instance) => instance.ToString();
        public static bool operator ==(RoadSegmentStatus left, RoadSegmentStatus right) => Equals(left, right);
        public static bool operator !=(RoadSegmentStatus left, RoadSegmentStatus right) => !Equals(left, right);

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
