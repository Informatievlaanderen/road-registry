namespace RoadRegistry.BackOffice.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class RoadSegmentLaneDirection : IEquatable<RoadSegmentLaneDirection>
    {
        public static readonly RoadSegmentLaneDirection Unknown =
            new RoadSegmentLaneDirection(
                nameof(Unknown),
                new DutchTranslation(
                    -8,
                    "niet gekend",
                    "Geen informatie beschikbaar"
                )
            );
        public static readonly RoadSegmentLaneDirection Forward =
            new RoadSegmentLaneDirection(
                nameof(Forward),
                new DutchTranslation(
                    1,
                    "gelijklopend met de digitalisatiezin",
                    "Aantal rijstroken slaat op de richting die de digitalisatiezin van het wegsegment volgt."
                )
            );
        public static readonly RoadSegmentLaneDirection Backward =
            new RoadSegmentLaneDirection(
                nameof(Backward),
                new DutchTranslation(
                    2,
                    "tegengesteld aan de digitalisatiezin",
                    "Aantal rijstroken slaat op de richting die tegengesteld loopt aan de digitalisatiezin van het wegsegment."
                )
            );
        public static readonly RoadSegmentLaneDirection Independent =
            new RoadSegmentLaneDirection(
                nameof(Independent),
                new DutchTranslation(
                    3,
                    "onafhankelijk van de digitalisatiezin",
                    "Aantal rijstroken slaat op het totaal in beide richtingen, onafhankelijk van de digitalisatiezin van het wegsegment."
                )
            );

        public static readonly RoadSegmentLaneDirection[] All = {
            Unknown, Forward, Backward, Independent
        };

        public static readonly IReadOnlyDictionary<int, RoadSegmentLaneDirection> ByIdentifier =
            All.ToDictionary(key => key.Translation.Identifier);

        private readonly string _value;
        private readonly DutchTranslation _dutchTranslation;

        private RoadSegmentLaneDirection(string value, DutchTranslation dutchTranslation)
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

        public static bool TryParse(string value, out RoadSegmentLaneDirection parsed)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static RoadSegmentLaneDirection Parse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known lane direction.");
            }
            return parsed;
        }

        public bool Equals(RoadSegmentLaneDirection other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is RoadSegmentLaneDirection type && Equals(type);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;
        public static implicit operator string(RoadSegmentLaneDirection instance) => instance.ToString();
        public static bool operator ==(RoadSegmentLaneDirection left, RoadSegmentLaneDirection right) => Equals(left, right);
        public static bool operator !=(RoadSegmentLaneDirection left, RoadSegmentLaneDirection right) => !Equals(left, right);

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
