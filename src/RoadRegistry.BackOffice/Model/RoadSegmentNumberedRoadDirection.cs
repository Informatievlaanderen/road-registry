namespace RoadRegistry.BackOffice.Model
{
    using System;

    public class RoadSegmentNumberedRoadDirection : IEquatable<RoadSegmentNumberedRoadDirection>
    {
        public static readonly RoadSegmentNumberedRoadDirection Unknown =
            new RoadSegmentNumberedRoadDirection(
                nameof(Unknown),
                new DutchTranslation(
                    -8,
                    "niet gekend",
                    "Geen informatie beschikbaar"
                )
            );
        public static readonly RoadSegmentNumberedRoadDirection Forward =
            new RoadSegmentNumberedRoadDirection(
                nameof(Forward),
                new DutchTranslation(
                    1,
                    "gelijklopend met de digitalisatiezin",
                    "Nummering weg slaat op de richting die de digitalisatiezin van het wegsegment volgt."
                )
            );
        public static readonly RoadSegmentNumberedRoadDirection Backward =
            new RoadSegmentNumberedRoadDirection(
                nameof(Backward),
                new DutchTranslation(
                    2,
                    "tegengesteld aan de digitalisatiezin",
                    "Nummering weg slaat op de richting die tegengesteld loopt aan de digitalisatiezin van het wegsegment."
                )
            );

        public static readonly RoadSegmentNumberedRoadDirection[] All = {
            Unknown, Forward, Backward
        };

        private readonly string _value;
        private readonly DutchTranslation _dutchTranslation;

        private RoadSegmentNumberedRoadDirection(string value, DutchTranslation dutchTranslation)
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

        public static bool TryParse(string value, out RoadSegmentNumberedRoadDirection parsed)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static RoadSegmentNumberedRoadDirection Parse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known numbered road segment direction.");
            }
            return parsed;
        }

        public bool Equals(RoadSegmentNumberedRoadDirection other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is RoadSegmentNumberedRoadDirection type && Equals(type);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;
        public static implicit operator string(RoadSegmentNumberedRoadDirection instance) => instance.ToString();
        public static bool operator ==(RoadSegmentNumberedRoadDirection left, RoadSegmentNumberedRoadDirection right) => Equals(left, right);
        public static bool operator !=(RoadSegmentNumberedRoadDirection left, RoadSegmentNumberedRoadDirection right) => !Equals(left, right);

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
