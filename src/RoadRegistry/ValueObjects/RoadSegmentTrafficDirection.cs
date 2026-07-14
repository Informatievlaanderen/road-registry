namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using RoadRegistry.Extensions;

public sealed class RoadSegmentTrafficDirection : IEquatable<RoadSegmentTrafficDirection>, IDutchToString
{
    public static readonly RoadSegmentTrafficDirection Forward =
        new(
            nameof(Forward),
            new DutchTranslation(1, "heen", "Het attribuut is van toepassing in de heenrichting (t.o.v. de digitalisatiezin van het wegsegment).")
        );
    public static readonly RoadSegmentTrafficDirection Backward =
        new(
            nameof(Backward),
            new DutchTranslation(2, "terug", "Het attribuut is van toepassing in de terugrichting (t.o.v. de digitalisatiezin van het wegsegment).")
        );
    public static readonly RoadSegmentTrafficDirection Both =
        new(
            nameof(Both),
            new DutchTranslation(3, "beide", "Het attribuut is van toepassing in beide richtingen (t.o.v. de digitalisatiezin van het wegsegment).")
        );
    public static readonly RoadSegmentTrafficDirection None =
        new(
            nameof(None),
            new DutchTranslation(4, "geen", "Het attribuut is in geen enkele richting van toepassing.")
        );

    public static readonly RoadSegmentTrafficDirection[] All =
    [
        Forward, Backward, Both, None
    ];

    public static readonly IReadOnlyDictionary<string, RoadSegmentTrafficDirection> ByName =
        All.ToDictionary(key => key.Translation.Name);

    public static RoadSegmentTrafficDirection FromAccess(bool forward, bool backward)
    {
        return forward
            ? backward ? Both : Forward
            : backward ? Backward : None;
    }

    public bool IsForwardAccessAllowed => Equals(Forward) || Equals(Both);
    public bool IsBackwardAccessAllowed => Equals(Backward) || Equals(Both);

    private readonly string _value;

    private RoadSegmentTrafficDirection(string value, DutchTranslation dutchTranslation)
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

    public bool Equals(RoadSegmentTrafficDirection? other)
    {
        return other != null && other._value == _value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RoadSegmentTrafficDirection type && Equals(type);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static bool operator ==(RoadSegmentTrafficDirection? left, RoadSegmentTrafficDirection? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RoadSegmentTrafficDirection? left, RoadSegmentTrafficDirection? right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string?(RoadSegmentTrafficDirection? instance)
    {
        return instance?.ToString();
    }

    public static RoadSegmentTrafficDirection Parse(string value)
    {
        if (!TryParse(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known traffic direction.");
        return parsed;
    }

    public static RoadSegmentTrafficDirection ParseUsingDutchName(string value)
    {
        if (!TryParseUsingDutchName(value.ThrowIfNull(), out var parsed)) throw new FormatException($"The value {value} is not a well known traffic direction.");
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

    public static bool TryParse(string value, out RoadSegmentTrafficDirection? parsed)
    {
        ArgumentNullException.ThrowIfNull(value);

        value = value.Trim();

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
    }

    public static bool TryParseUsingDutchName(string value, out RoadSegmentTrafficDirection? parsed)
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

        public int Identifier { get; }
        public string Name { get; }
        public string Description { get; }
    }
}
