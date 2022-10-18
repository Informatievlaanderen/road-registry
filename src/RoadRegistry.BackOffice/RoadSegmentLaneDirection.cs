namespace RoadRegistry.BackOffice;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class RoadSegmentLaneDirection : IEquatable<RoadSegmentLaneDirection>
{
    private RoadSegmentLaneDirection(string value, DutchTranslation dutchTranslation)
    {
        _value = value;
        Translation = dutchTranslation;
    }

    private readonly string _value;

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

    public static readonly RoadSegmentLaneDirection[] All =
    {
        Unknown, Forward, Backward, Independent
    };


    public static readonly IReadOnlyDictionary<int, RoadSegmentLaneDirection> ByIdentifier =
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

    public bool Equals(RoadSegmentLaneDirection other)
    {
        return other != null && other._value == _value;
    }

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
        return instance.ToString();
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

    public override string ToString()
    {
        return _value;
    }

    public DutchTranslation Translation { get; }

    public static bool TryParse(string value, out RoadSegmentLaneDirection parsed)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
    }
}
