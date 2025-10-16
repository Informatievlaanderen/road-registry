namespace RoadRegistry.BackOffice;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class RoadNodeType : IEquatable<RoadNodeType>
{
    public static readonly RoadNodeType EndNode =
        new(
            nameof(EndNode),
            new DutchTranslation(
                3,
                "eindknoop",
                "Het einde van een doodlopende wegcorridor, slechts één aansluitend wegsegment."
            )
        );

    public static readonly RoadNodeType FakeNode =
        new(
            nameof(FakeNode),
            new DutchTranslation(
                2,
                "schijnknoop",
                "Punt waar 2 wegsegmenten elkaar raken; slechts twee aansluitende wegsegmenten."
            )
        );

    public static readonly RoadNodeType MiniRoundabout =
        new(
            nameof(MiniRoundabout),
            new DutchTranslation(
                4,
                "minirotonde",
                "Kruispunt dat zich in de realiteit voordoet als een rotonde maar niet voldoet aan de geometrische specificaties om opgenomen te worden als een echte rotonde (ringvormige geometrie)."
            )
        );

    public static readonly RoadNodeType RealNode =
        new(
            nameof(RealNode),
            new DutchTranslation(
                1,
                "echte knoop",
                "Punt waar 2 wegsegmenten elkaar snijden; minstens drie aansluitende wegsegmenten."
            )
        );

    public static readonly RoadNodeType TurningLoopNode =
        new(
            nameof(TurningLoopNode),
            new DutchTranslation(
                5,
                "keerlusknoop",
                "Juist twee aansluitende wegsegmenten; wegsegmenten die aan beide zijden begrensd worden door dezelfde wegknoop worden met behulp van een extra wegknoop (= keerlusknoop) opgesplitst."
            )
        );

    public static readonly RoadNodeType[] All = { RealNode, FakeNode, EndNode, MiniRoundabout, TurningLoopNode };

    public static readonly IReadOnlyDictionary<int, RoadNodeType> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier);

    private readonly string _value;

    private RoadNodeType(string value, DutchTranslation dutchTranslation)
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
        return obj is RoadNodeType type && Equals(type);
    }

    public bool Equals(RoadNodeType other)
    {
        return other != null && other._value == _value;
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public bool IsAnyOf(params RoadNodeType[] types)
    {
        if (types == null)
        {
            throw new ArgumentNullException(nameof(types));
        }

        return types.Contains(this);
    }

    public static bool operator ==(RoadNodeType left, RoadNodeType right)
    {
        return Equals(left, right);
    }

    public static implicit operator string(RoadNodeType instance)
    {
        return instance.ToString();
    }

    public static bool operator !=(RoadNodeType left, RoadNodeType right)
    {
        return !Equals(left, right);
    }

    public static RoadNodeType Parse(string value)
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

    public static bool TryParse(string value, out RoadNodeType parsed)
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
