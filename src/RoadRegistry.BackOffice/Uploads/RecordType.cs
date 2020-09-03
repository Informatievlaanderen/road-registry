namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class RecordType : IEquatable<RecordType>
    {
        public const int IdenticalIdentifier = 1;
        public static readonly RecordType Identical =
            new RecordType(
                nameof(Identical),
                new DutchTranslation(
                    IdenticalIdentifier,
                    "gelijk",
                    "Het record werd niet aangepast."
                )
            );
        public const int AddedIdentifier = 2;
        public static readonly RecordType Added =
            new RecordType(
                nameof(Added),
                new DutchTranslation(
                    AddedIdentifier,
                    "toegevoegd",
                    "Het record werd toegevoegd."
                )
            );
        public const int ModifiedIdentifier = 3;
        public static readonly RecordType Modified =
            new RecordType(
                nameof(Modified),
                new DutchTranslation(
                    ModifiedIdentifier,
                    "gewijzigd",
                    "Het record werd aangepast."
                )
            );
        public const int RemovedIdentifier = 4;
        public static readonly RecordType Removed =
            new RecordType(
                nameof(Removed),
                new DutchTranslation(
                    RemovedIdentifier,
                    "verwijderd",
                    "Het record werd verwijderd."
                )
            );

        public static readonly RecordType[] All = {
            Identical, Added, Modified, Removed
        };

        public static readonly IReadOnlyDictionary<int, RecordType> ByIdentifier =
            All.ToDictionary(key => key.Translation.Identifier);

        private readonly string _value;
        private readonly DutchTranslation _dutchTranslation;

        private RecordType(string value, DutchTranslation dutchTranslation)
        {
            _value = value;
            _dutchTranslation = dutchTranslation;
        }

        public DutchTranslation Translation => _dutchTranslation;

        public static bool CanParse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return Array.Find(All, candidate => candidate._value == value) != null;
        }

        public static bool TryParse(string value, out RecordType parsed)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            parsed = Array.Find(All, candidate => candidate._value == value);
            return parsed != null;
        }

        public static RecordType Parse(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!TryParse(value, out var parsed))
            {
                throw new FormatException($"The value {value} is not a well known type of grade separated junction.");
            }
            return parsed;
        }

        public bool Equals(RecordType other) => other != null && other._value == _value;
        public override bool Equals(object obj) => obj is RecordType type && Equals(type);
        public override int GetHashCode() => _value.GetHashCode();
        public override string ToString() => _value;
        public static implicit operator string(RecordType instance) => instance.ToString();
        public static bool operator ==(RecordType left, RecordType right) => Equals(left, right);
        public static bool operator !=(RecordType left, RecordType right) => !Equals(left, right);

        public class DutchTranslation
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
}
