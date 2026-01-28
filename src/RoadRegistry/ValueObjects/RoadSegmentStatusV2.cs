namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RoadRegistry.Extensions;

public sealed class RoadSegmentStatusV2 : IEquatable<RoadSegmentStatusV2>, IDutchToString
{
    public static readonly RoadSegmentStatusV2 Gepland =
        new(
            nameof(Gepland),
            new DutchTranslation(
                1,
                "gepland",
                "Er werd een vergunning aangevraagd of verleend voor de weg, of de weg is in aanbouw."
            )
        );

    public static readonly RoadSegmentStatusV2 Gerealiseerd =
        new(
            nameof(Gerealiseerd),
            new DutchTranslation(
                2,
                "gerealiseerd",
                "De weg werd gerealiseerd (werken beëindigd) en in gebruik genomen."
            )
        );

    public static readonly RoadSegmentStatusV2 NietGerealiseerd =
        new(
            nameof(NietGerealiseerd),
            new DutchTranslation(
                3,
                "niet gerealiseerd",
                "De weg werd niet gerealiseerd, bijvoorbeeld omdat de vergunning niet verleend werd, vervallen is of ingetrokken werd."
            )
        );

    public static readonly RoadSegmentStatusV2 BuitenGebruik =
        new(
            nameof(BuitenGebruik),
            new DutchTranslation(
                4,
                "buiten gebruik",
                "De weg werd niet opgeheven en bestaat juridisch gezien, maar is niet meer zichtbaar op het terrein of kan niet gebruikt worden."
            )
        );

    public static readonly RoadSegmentStatusV2 Gehistoreerd =
        new(
            nameof(Gehistoreerd),
            new DutchTranslation(
                5,
                "gehistoreerd",
                "De weg werd opgeheven, gesloopt, samengevoegd of gesplitst."
            )
        );

    public static readonly RoadSegmentStatusV2[] All =
    {
        Gepland, Gerealiseerd, NietGerealiseerd, BuitenGebruik, Gehistoreerd
    };

    public sealed record Edit
    {
        public static readonly ImmutableArray<RoadSegmentStatusV2> Editable = [..All];
    }

    public static readonly IReadOnlyDictionary<int, RoadSegmentStatusV2> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier);

    public static readonly IReadOnlyDictionary<string, RoadSegmentStatusV2> ByName =
        All.ToDictionary(key => key.Translation.Name);

    private readonly string _value;

    private RoadSegmentStatusV2(string value, DutchTranslation dutchTranslation)
    {
        _value = value;
        Translation = dutchTranslation;
    }

    public DutchTranslation Translation { get; }

    public static bool CanParse(string value)
    {
        return TryParse(value.ThrowIfNull(), out _);
    }

    public static bool CanParseUsingDutchName(string value)
    {
        return TryParseUsingDutchName(value, out _);
    }

    public bool Equals(RoadSegmentStatusV2? other)
    {
        return other != null && other._value == _value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RoadSegmentStatusV2 type && Equals(type);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(RoadSegmentStatusV2 left, RoadSegmentStatusV2 right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RoadSegmentStatusV2 left, RoadSegmentStatusV2 right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string?(RoadSegmentStatusV2? instance)
    {
        return instance?.ToString();
    }

    public static implicit operator int(RoadSegmentStatusV2 instance)
    {
        return instance.Translation.Identifier;
    }

    public static RoadSegmentStatusV2 Parse(string value)
    {
        if (!TryParse(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known road segment status.");
        return parsed;
    }

    public static RoadSegmentStatusV2 ParseUsingDutchName(string value)
    {
        if (!TryParseUsingDutchName(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known road segment status.");
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

    public static bool TryParse(string value, out RoadSegmentStatusV2 parsed)
    {
        ArgumentNullException.ThrowIfNull(value);

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
    }

    public static bool TryParseUsingDutchName(string value, out RoadSegmentStatusV2 parsed)
    {
        parsed = Array.Find(All, candidate => candidate.Translation.Name == value);
        return parsed != null;
    }

    public sealed class DutchTranslation
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
