namespace RoadRegistry.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class RoadNodeTypeV2 : IEquatable<RoadNodeTypeV2>, IDutchToString
{
    public static readonly RoadNodeTypeV2 RealNode =
        new(
            nameof(RealNode),
            new DutchTranslation(
                1,
                "echte knoop",
            "Een echte knoop is een wegknoop waarin méér dan twee wegsegmenten samenkomen, en waarop uitwisseling van verkeer mogelijk is tussen de verschillende wegsegmenten die erop aansluiten."
            )
        );

    public static readonly RoadNodeTypeV2 FakeNode =
        new(
            nameof(FakeNode),
            new DutchTranslation(
                2,
                "schijnknoop",
                "Punt waar 2 wegsegmenten elkaar raken; slechts twee aansluitende wegsegmenten."
            )
        );

    public static readonly RoadNodeTypeV2 EndNode =
        new(
            nameof(EndNode),
            new DutchTranslation(
                3,
                "eindknoop",
                "Een eindknoop is een wegknoop waarop precies één wegsegment aansluit. Deze knoop markeert het einde van een weg."
            )
        );

    public static readonly RoadNodeTypeV2 ValidationNode =
        new(
            nameof(ValidationNode),
            new DutchTranslation(
                4,
                "validatieknoop",
                "Een validatieknoop is een knoop die toegevoegd werd om de integriteit van het wegennetwerk correct te valideren. Validatieknopen hebben steeds twee aansluitende wegsegmenten."
            )
        );

    public static readonly RoadNodeTypeV2[] All = [RealNode, FakeNode, EndNode, ValidationNode];

    public static readonly IReadOnlyDictionary<int, RoadNodeTypeV2> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier);

    private readonly string _value;

    private RoadNodeTypeV2(string value, DutchTranslation dutchTranslation)
    {
        _value = value;
        Translation = dutchTranslation;
    }

    public DutchTranslation Translation { get; }

    public static bool CanParse(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        return Array.Find(All, candidate => candidate._value == value) != null;
    }

    public override bool Equals(object obj)
    {
        return obj is RoadNodeTypeV2 type && Equals(type);
    }

    public bool Equals(RoadNodeTypeV2 other)
    {
        return other != null && other._value == _value;
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public bool IsAnyOf(params RoadNodeTypeV2[] types)
    {
        if (types == null)
        {
            throw new ArgumentNullException(nameof(types));
        }

        return types.Contains(this);
    }

    public static bool operator ==(RoadNodeTypeV2 left, RoadNodeTypeV2 right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RoadNodeTypeV2 left, RoadNodeTypeV2 right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string?(RoadNodeTypeV2? instance)
    {
        return instance?.ToString();
    }
    public static implicit operator int(RoadNodeTypeV2 instance)
    {
        return instance.Translation.Identifier;
    }

    public static RoadNodeTypeV2 Parse(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (!TryParse(value, out var parsed))
        {
            throw new FormatException($"The value {value} is not a well known road node type.");
        }

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

    public static bool TryParse(string value, out RoadNodeTypeV2 parsed)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

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
