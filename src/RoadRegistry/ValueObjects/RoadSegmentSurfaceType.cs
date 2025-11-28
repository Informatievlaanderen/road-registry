namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using RoadRegistry.Extensions;

public sealed class RoadSegmentSurfaceType : IEquatable<RoadSegmentSurfaceType>, IDutchToString
{
    public static readonly RoadSegmentSurfaceType LooseSurface =
        new(
            nameof(LooseSurface),
            new DutchTranslation(
                2,
                "weg met losse verharding",
                "Weg met een wegverharding bestaande uit losliggende materialen (bv. grind, kiezel, steenslag, puin, enz.)."
            )
        );

    public static readonly RoadSegmentSurfaceType NotApplicable =
        new(
            nameof(NotApplicable),
            new DutchTranslation(
                -9,
                "niet van toepassing",
                "Niet van toepassing"
            )
        );

    public static readonly RoadSegmentSurfaceType SolidSurface =
        new(
            nameof(SolidSurface),
            new DutchTranslation(
                1,
                "weg met vaste verharding",
                "Weg met een wegdek bestaande uit in vast verband aangebrachte materialen zoals asfalt, beton, klinkers, kasseien, enz."
            )
        );

    public static readonly RoadSegmentSurfaceType Unknown =
        new(
            nameof(Unknown),
            new DutchTranslation(
                -8,
                "niet gekend",
                "Geen informatie beschikbaar"
            )
        );

    public static readonly RoadSegmentSurfaceType[] All =
    {
        NotApplicable, Unknown, SolidSurface, LooseSurface
    };

    public static readonly IReadOnlyDictionary<int, RoadSegmentSurfaceType> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier);

    public static readonly IReadOnlyDictionary<string, RoadSegmentSurfaceType> ByName =
        All.ToDictionary(key => key.Translation.Name);

    private readonly string _value;

    private RoadSegmentSurfaceType(string value, DutchTranslation dutchTranslation)
    {
        _value = value;
        Translation = dutchTranslation;
    }

    public DutchTranslation Translation { get; }

    public bool Equals(RoadSegmentSurfaceType other)
    {
        return other != null && other._value == _value;
    }

    public static bool CanParse(string value)
    {
        return TryParse(value.ThrowIfNull(), out _);
    }

    public static bool CanParseUsingDutchName(string value)
    {
        return TryParseUsingDutchName(value, out _);
    }

    public override bool Equals(object obj)
    {
        return obj is RoadSegmentSurfaceType type && Equals(type);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(RoadSegmentSurfaceType left, RoadSegmentSurfaceType right)
    {
        return Equals(left, right);
    }

    public static implicit operator string(RoadSegmentSurfaceType instance)
    {
        return instance?.ToString();
    }

    public static bool operator !=(RoadSegmentSurfaceType left, RoadSegmentSurfaceType right)
    {
        return !Equals(left, right);
    }

    public static RoadSegmentSurfaceType Parse(string value)
    {
        if (!TryParse(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known type of road surface.");
        return parsed;
    }

    public static RoadSegmentSurfaceType ParseUsingDutchName(string value)
    {
        if (!TryParseUsingDutchName(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known type of road surface.");
        return parsed;
    }

    public override string ToString()
    {
        return _value;
    }

    public string ToDutchString()
    {
        return Translation.Name;
    }

    public static bool TryParse(string value, out RoadSegmentSurfaceType parsed)
    {
        ArgumentNullException.ThrowIfNull(value);

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
    }

    public static bool TryParseUsingDutchName(string value, out RoadSegmentSurfaceType parsed)
    {
        parsed = Array.Find(All, candidate => candidate.Translation.Name == value);
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
