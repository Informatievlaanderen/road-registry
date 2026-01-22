// ReSharper disable InconsistentNaming

namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class RoadSegmentGeometryDrawMethodV2 : IEquatable<RoadSegmentGeometryDrawMethodV2>, IDutchToString
{
    public static readonly RoadSegmentGeometryDrawMethodV2 Ingeschetst =
        new(
            nameof(Ingeschetst),
            new DutchTranslation(
                1,
                "ingeschetst",
                "Wegsegment waarvan de geometrie ingeschetst werd door een decentrale beheerder."
            )
        );

    public static readonly RoadSegmentGeometryDrawMethodV2 Ingemeten =
        new(
            nameof(Ingemeten),
            new DutchTranslation(
                2,
                "ingemeten",
                "Wegsegment waarvan de geometrie ingemeten werd via een centrale karteringsopdracht."
            )
        );

    public static readonly RoadSegmentGeometryDrawMethodV2[] All =
    {
        Ingeschetst, Ingemeten
    };
    public static readonly RoadSegmentGeometryDrawMethodV2[] Allowed =
    {
        Ingeschetst, Ingemeten
    };

    public static readonly IReadOnlyDictionary<int, RoadSegmentGeometryDrawMethodV2> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier);

    private readonly string _value;

    private RoadSegmentGeometryDrawMethodV2(string value, DutchTranslation dutchTranslation)
    {
        _value = value;
        Translation = dutchTranslation;
    }

    public DutchTranslation Translation { get; }

    public override bool Equals(object obj)
    {
        return obj is RoadSegmentGeometryDrawMethodV2 type && Equals(type);
    }

    public bool Equals(RoadSegmentGeometryDrawMethodV2 other)
    {
        return other != null && other._value == _value;
    }

    public static bool CanParse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        return Array.Find(All, candidate => candidate._value == value) != null;
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(RoadSegmentGeometryDrawMethodV2 left, RoadSegmentGeometryDrawMethodV2 right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RoadSegmentGeometryDrawMethodV2 left, RoadSegmentGeometryDrawMethodV2 right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string?(RoadSegmentGeometryDrawMethodV2? instance)
    {
        return instance?.ToString();
    }
    public static implicit operator int(RoadSegmentGeometryDrawMethodV2 instance)
    {
        return instance.Translation.Identifier;
    }

    public static RoadSegmentGeometryDrawMethodV2 Parse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (!TryParse(value, out var parsed)) throw new FormatException($"The value {value} is not a well known road segment geometry draw method.");
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

    public static bool TryParse(string value, out RoadSegmentGeometryDrawMethodV2 parsed)
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
