namespace RoadRegistry.BackOffice;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class RoadSegmentNumberedRoadDirection : IEquatable<RoadSegmentNumberedRoadDirection>
{
    private RoadSegmentNumberedRoadDirection(string value, DutchTranslation dutchTranslation)
    {
        _value = value;
        Translation = dutchTranslation;
    }

    private readonly string _value;

    public static readonly RoadSegmentNumberedRoadDirection Backward =
        new(
            nameof(Backward),
            new DutchTranslation(
                2,
                "tegengesteld aan de digitalisatiezin",
                "Nummering weg slaat op de richting die tegengesteld loopt aan de digitalisatiezin van het wegsegment."
            )
        );

    public static readonly RoadSegmentNumberedRoadDirection Forward =
        new(
            nameof(Forward),
            new DutchTranslation(
                1,
                "gelijklopend met de digitalisatiezin",
                "Nummering weg slaat op de richting die de digitalisatiezin van het wegsegment volgt."
            )
        );

    public static readonly RoadSegmentNumberedRoadDirection Unknown =
        new(
            nameof(Unknown),
            new DutchTranslation(
                -8,
                "niet gekend",
                "Geen informatie beschikbaar"
            )
        );

    public static readonly RoadSegmentNumberedRoadDirection[] All =
    {
        Unknown, Forward, Backward
    };


    public static readonly IReadOnlyDictionary<int, RoadSegmentNumberedRoadDirection> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier);

    public static bool CanParse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        return Array.Find(All, candidate => candidate._value == value) != null;
    }

    public class DutchTranslation
    {
        internal DutchTranslation(int identifier, string name, string description)
        {
            Identifier = identifier;
            Name = name;
            Description = description;
        }

        public string Description { get; }

        public int Identifier { get; }

        public string Name { get; }
    }

    public override bool Equals(object obj)
    {
        return obj is RoadSegmentNumberedRoadDirection type && Equals(type);
    }

    public bool Equals(RoadSegmentNumberedRoadDirection other)
    {
        return other != null && other._value == _value;
    }


    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(RoadSegmentNumberedRoadDirection left, RoadSegmentNumberedRoadDirection right)
    {
        return Equals(left, right);
    }

    public static implicit operator string(RoadSegmentNumberedRoadDirection instance)
    {
        return instance.ToString();
    }

    public static bool operator !=(RoadSegmentNumberedRoadDirection left, RoadSegmentNumberedRoadDirection right)
    {
        return !Equals(left, right);
    }

    public static RoadSegmentNumberedRoadDirection Parse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (!TryParse(value, out var parsed)) throw new FormatException($"The value {value} is not a well known numbered road segment direction.");
        return parsed;
    }

    public override string ToString()
    {
        return _value;
    }

    public DutchTranslation Translation { get; }

    public static bool TryParse(string value, out RoadSegmentNumberedRoadDirection parsed)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
    }
}
