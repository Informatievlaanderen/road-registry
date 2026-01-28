namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class GradeSeparatedJunctionType : IEquatable<GradeSeparatedJunctionType>, IDutchToString
{
    public static readonly GradeSeparatedJunctionType Tunnel =
        new(
            nameof(Tunnel),
            new DutchTranslation(
                1,
                "tunnel",
                "Een tunnel is een doorgang voor een weg, spoorweg, aardeweg of pad die onder de grond, onder water of in een langwerpige, overdekte uitgraving is gelegen."
            )
        );

    public static readonly GradeSeparatedJunctionType Bridge =
        new(
            nameof(Bridge),
            new DutchTranslation(
                2,
                "brug",
                "Een brug is een doorgang voor een weg, spoorweg, aardeweg of pad die boven de grond of boven water gelegen is. Een brug kan vast of beweegbaar zijn."
            )
        );

    public static readonly GradeSeparatedJunctionType Unknown =
        new(
            nameof(Unknown),
            new DutchTranslation(
                -8,
                "niet gekend",
                "Geen informatie beschikbaar."
            )
        );

    public static readonly GradeSeparatedJunctionType[] All =
    {
        Unknown, Tunnel, Bridge
    };

    public static readonly IReadOnlyDictionary<int, GradeSeparatedJunctionType> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier);

    private readonly string _value;

    private GradeSeparatedJunctionType(string value, DutchTranslation dutchTranslation)
    {
        _value = value;
        Translation = dutchTranslation;
    }

    public DutchTranslation Translation { get; }

    public static bool CanParse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        return Array.Find(All, candidate => candidate._value == value) != null;
    }

    public bool Equals(GradeSeparatedJunctionType? other)
    {
        return other != null && other._value == _value;
    }

    public override bool Equals(object? obj)
    {
        return obj is GradeSeparatedJunctionType type && Equals(type);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(GradeSeparatedJunctionType left, GradeSeparatedJunctionType right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(GradeSeparatedJunctionType left, GradeSeparatedJunctionType right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string?(GradeSeparatedJunctionType? instance)
    {
        return instance?.ToString();
    }

    public static GradeSeparatedJunctionType Parse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (!TryParse(value, out var parsed)) throw new FormatException($"The value {value} is not a well known type of grade separated junction.");
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

    public static bool TryParse(string value, out GradeSeparatedJunctionType parsed)
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
