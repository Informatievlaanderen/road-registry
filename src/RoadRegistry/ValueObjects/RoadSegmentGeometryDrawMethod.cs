// ReSharper disable InconsistentNaming

namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class RoadSegmentGeometryDrawMethod : IEquatable<RoadSegmentGeometryDrawMethod>, IDutchToString
{
    public static readonly RoadSegmentGeometryDrawMethod Outlined =
        new(
            nameof(Outlined),
            new DutchTranslation(
                1,
                "ingeschetst",
                "Wegsegment waarvan de geometrie ingeschetst werd."
            )
        );

    public static readonly RoadSegmentGeometryDrawMethod Measured =
        new(
            nameof(Measured),
            new DutchTranslation(
                2,
                "ingemeten",
                "Wegsegment waarvan de geometrie ingemeten werd (bv. overgenomen uit as-built-plan of andere dataset)."
            )
        );

    public static readonly RoadSegmentGeometryDrawMethod Measured_according_to_GRB_specifications =
        new(
            nameof(Measured_according_to_GRB_specifications),
            new DutchTranslation(
                3,
                "ingemeten volgens GRB-specificaties",
                "Wegsegment waarvan de geometrie werd ingemeten volgens GRB-specificaties."
            )
        );

    public static readonly RoadSegmentGeometryDrawMethod[] All =
    {
        Outlined, Measured, Measured_according_to_GRB_specifications
    };
    public static readonly RoadSegmentGeometryDrawMethod[] Allowed =
    {
        Outlined, Measured
    };

    public static readonly IReadOnlyDictionary<int, RoadSegmentGeometryDrawMethod> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier);

    private readonly string _value;

    private RoadSegmentGeometryDrawMethod(string value, DutchTranslation dutchTranslation)
    {
        _value = value;
        Translation = dutchTranslation;
    }

    public DutchTranslation Translation { get; }

    public override bool Equals(object obj)
    {
        return obj is RoadSegmentGeometryDrawMethod type && Equals(type);
    }

    public bool Equals(RoadSegmentGeometryDrawMethod other)
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

    public static bool operator ==(RoadSegmentGeometryDrawMethod left, RoadSegmentGeometryDrawMethod right)
    {
        return Equals(left, right);
    }

    public static implicit operator string(RoadSegmentGeometryDrawMethod instance)
    {
        return instance?.ToString();
    }
    public static implicit operator int(RoadSegmentGeometryDrawMethod instance)
    {
        return instance.Translation.Identifier;
    }

    public static bool operator !=(RoadSegmentGeometryDrawMethod left, RoadSegmentGeometryDrawMethod right)
    {
        return !Equals(left, right);
    }

    public static RoadSegmentGeometryDrawMethod Parse(string value)
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

    public static bool TryParse(string value, out RoadSegmentGeometryDrawMethod parsed)
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
