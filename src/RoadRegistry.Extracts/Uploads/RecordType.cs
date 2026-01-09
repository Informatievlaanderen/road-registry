namespace RoadRegistry.Extracts.Uploads;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class RecordType : IEquatable<RecordType>
{
    public const int AddedIdentifier = 2;
    public const int IdenticalIdentifier = 1;
    public const int ModifiedIdentifier = 3;
    public const int RemovedIdentifier = 4;

    public static readonly RecordType Identical =
        new(
            nameof(Identical),
            new DutchTranslation(
                IdenticalIdentifier,
                "gelijk",
                "Het record werd niet aangepast."
            )
        );

    public static readonly RecordType Added =
        new(
            nameof(Added),
            new DutchTranslation(
                AddedIdentifier,
                "toegevoegd",
                "Het record werd toegevoegd."
            )
        );

    public static readonly RecordType Modified =
        new(
            nameof(Modified),
            new DutchTranslation(
                ModifiedIdentifier,
                "gewijzigd",
                "Het record werd aangepast."
            )
        );

    public static readonly RecordType Removed =
        new(
            nameof(Removed),
            new DutchTranslation(
                RemovedIdentifier,
                "verwijderd",
                "Het record werd verwijderd."
            )
        );

    public static readonly RecordType[] All =
    {
        Identical, Added, Modified, Removed
    };

    public static readonly IReadOnlyDictionary<int, RecordType> ByIdentifier =
        All.ToDictionary(key => key.Translation.Identifier);

    private readonly string _value;

    private RecordType(string value, DutchTranslation dutchTranslation)
    {
        _value = value;
        Translation = dutchTranslation;
    }

    public DutchTranslation Translation { get; }

    public bool Equals(RecordType? other)
    {
        return other != null && other._value == _value;
    }

    public override bool Equals(object? obj)
    {
        return obj is RecordType type && Equals(type);
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

    public bool IsAnyOf(params RecordType[] these)
    {
        return Array.Exists(these, candidate => candidate == this);
    }

    public static bool operator ==(RecordType left, RecordType right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RecordType left, RecordType right)
    {
        return !Equals(left, right);
    }

    public static implicit operator string(RecordType instance)
    {
        return instance.ToString();
    }

    public static RecordType Parse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (!TryParse(value, out var parsed)) throw new FormatException($"The value {value} is not a well known type of grade separated junction.");
        return parsed;
    }

    public override string ToString()
    {
        return _value;
    }

    public static bool TryParse(string value, out RecordType parsed)
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
