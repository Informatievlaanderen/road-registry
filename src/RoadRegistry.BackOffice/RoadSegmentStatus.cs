namespace RoadRegistry.BackOffice;

using System;
using System.Collections.Generic;
using System.Linq;

public sealed class RoadSegmentStatus : IEquatable<RoadSegmentStatus>
{
    private RoadSegmentStatus(string value, DutchTranslation dutchTranslation)
    {
        _value = value;
        Translation = dutchTranslation;
    }

    private readonly string _value;

    public static readonly RoadSegmentStatus BuildingPermitGranted =
        new(
            nameof(BuildingPermitGranted),
            new DutchTranslation(
                2,
                "bouwvergunning verleend",
                "Geometrie komt voor op goedgekeurd, niet vervallen bouwdossier."
            )
        );

    public static readonly RoadSegmentStatus InUse =
        new(
            nameof(InUse),
            new DutchTranslation(
                4,
                "in gebruik",
                "Werken zijn opgeleverd."
            )
        );

    public static readonly RoadSegmentStatus OutOfUse =
        new(
            nameof(OutOfUse),
            new DutchTranslation(
                5,
                "buiten gebruik",
                "Fysieke weg is buiten gebruik gesteld maar niet gesloopt."
            )
        );

    public static readonly RoadSegmentStatus PermitRequested =
        new(
            nameof(PermitRequested),
            new DutchTranslation(
                1,
                "vergunning aangevraagd",
                "Geometrie komt voor op officieel document in behandeling."
            )
        );

    public static readonly RoadSegmentStatus UnderConstruction =
        new(
            nameof(UnderConstruction),
            new DutchTranslation(
                3,
                "in aanbouw",
                "Aanvang der werken is gemeld."
            )
        );

    public static readonly RoadSegmentStatus Unknown =
        new(
            nameof(Unknown),
            new DutchTranslation(
                -8,
                "niet gekend",
                "Geen informatie beschikbaar"
            )
        );

    public static readonly RoadSegmentStatus[] All =
    {
        Unknown, PermitRequested, BuildingPermitGranted, UnderConstruction, InUse, OutOfUse
    };


    public static readonly IReadOnlyDictionary<int, RoadSegmentStatus> ByIdentifier =
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

    public override bool Equals(object obj)
    {
        return obj is RoadSegmentStatus type && Equals(type);
    }

    public bool Equals(RoadSegmentStatus other)
    {
        return other != null && other._value == _value;
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }


    public static bool operator ==(RoadSegmentStatus left, RoadSegmentStatus right)
    {
        return Equals(left, right);
    }

    public static implicit operator string(RoadSegmentStatus instance)
    {
        return instance.ToString();
    }

    public static bool operator !=(RoadSegmentStatus left, RoadSegmentStatus right)
    {
        return !Equals(left, right);
    }


    public static RoadSegmentStatus Parse(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (!TryParse(value, out var parsed)) throw new FormatException($"The value {value} is not a well known road segment status.");
        return parsed;
    }


    public override string ToString()
    {
        return _value;
    }

    public DutchTranslation Translation { get; }

    public static bool TryParse(string value, out RoadSegmentStatus parsed)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        parsed = Array.Find(All, candidate => candidate._value == value);
        return parsed != null;
    }
}
