namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class RoadSegmentLaneDirection : IEquatable<RoadSegmentLaneDirection>, IDutchToString
{
    public static readonly RoadSegmentLaneDirection Backward =
        new(
            nameof(Backward),
            new DutchTranslation(
                2,
                "tegengesteld aan de digitalisatiezin",
                "Aantal rijstroken slaat op de richting die tegengesteld loopt aan de digitalisatiezin van het wegsegment."
            )
        );

    public static readonly RoadSegmentLaneDirection Forward =
        new(
            nameof(Forward),
            new DutchTranslation(
                1,
                "gelijklopend met de digitalisatiezin",
                "Aantal rijstroken slaat op de richting die de digitalisatiezin van het wegsegment volgt."
            )
        );

    public static readonly RoadSegmentLaneDirection Independent =
        new(
            nameof(Independent),
            new DutchTranslation(
                3,
                "onafhankelijk van de digitalisatiezin",
                "Aantal rijstroken slaat op het totaal in beide richtingen, onafhankelijk van de digitalisatiezin van het wegsegment."
            )
        );

    public static readonly RoadSegmentLaneDirection Unknown =
        new(
            nameof(Unknown),
            new DutchTranslation(
                -8,
                "niet gekend",
                "Geen informatie beschikbaar"
            )
        );

    public static readonly RoadSegmentLaneDirection NotApplicable =
        new(
            nameof(NotApplicable),
            new DutchTranslation(
                -9,
                "niet van toepassing",
                "Niet van toepassing"
            )
        );

    public static readonly RoadSegmentLaneDirection[] All =
    {
        NotApplicable, Unknown, Forward, Backward, Independent
    };

    public static readonly IReadOnlyDictionary<int, RoadSegmentLaneDirection> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier);

    public static readonly IReadOnlyDictionary<string, RoadSegmentLaneDirection> ByName =
        All.ToDictionary(key => key.Translation.Name);

    private readonly string _value;

    private RoadSegmentLaneDirection(string value, DutchTranslation dutchTranslation)
    {
        _value = value;
        Translation = dutchTranslation;
    }

    public DutchTranslation Translation { get; }

    public bool Equals(RoadSegmentLaneDirection other)
    {
        return other != null && other._value == _value;
    }

    public static bool CanParse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        return Array.Find(All, candidate => candidate._value == value) != null;
    }

    public static bool CanParseUsingDutchName(string value) => ParseUsingDutchName(value) is not null;

    public override bool Equals(object obj)
    {
        return obj is RoadSegmentLaneDirection type && Equals(type);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(RoadSegmentLaneDirection left, RoadSegmentLaneDirection right)
    {
        return Equals(left, right);
    }

    public static implicit operator string(RoadSegmentLaneDirection instance)
    {
        return instance?.ToString();
    }

    public static bool operator !=(RoadSegmentLaneDirection left, RoadSegmentLaneDirection right)
    {
        return !Equals(left, right);
    }

    public static RoadSegmentLaneDirection Parse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (!TryParse(value, out var parsed)) throw new FormatException($"The value {value} is not a well known lane direction.");
        return parsed;
    }

    public static RoadSegmentLaneDirection ParseUsingDutchName(string value)
    {
        return value == null ? null : Array.Find(All, candidate => candidate.Translation.Name == value);
    }

    public override string ToString()
    {
        return _value;
    }

    public string ToDutchString()
    {
        return Translation.Name;
    }

    public static bool TryParse(string value, out RoadSegmentLaneDirection parsed)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
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
}
