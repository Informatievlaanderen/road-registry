namespace RoadRegistry.Extracts.Infrastructure.Dbase;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri.Dbf;
using NetTopologySuite.IO.Esri.Dbf.Fields;
using RoadRegistry.Extensions;

public static class DbaseExtensions
{
    private const string DateTimeFormat = "yyyyMMddTHHmmss";

    public static DbfField[] ToDbfFields(this DbaseSchema dbaseSchema)
    {
        return dbaseSchema.Fields
            .Select(field => field.ToDbfField())
            .ToArray();
    }

    public static DbaseSchema ToDbaseSchema(this DbfFieldCollection dbfFields)
    {
        return new AnonymousDbaseSchema(dbfFields
            .Select(x => x.ToDbaseField())
            .ToArray());
    }

    public static IFeature ToFeature(this DbaseRecord dbaseRecord, Geometry geometry)
    {
        return new Feature(geometry, dbaseRecord.ToAttributesTable());
    }

    public static AttributesTable ToAttributesTable(this DbaseRecord dbaseRecord)
    {
        var attributes = new AttributesTable();

        foreach (var dbaseFieldValue in dbaseRecord.Values)
        {
            var value = dbaseFieldValue.GetValue();
            attributes.Add(dbaseFieldValue.Field.Name, value);
        }

        return attributes;
    }

    private static DbfField ToDbfField(this DbaseField field)
    {
        switch (field.FieldType)
        {
            case DbaseFieldType.Character:
                return new DbfCharacterField(field.Name, field.Length.ToInt32());
            case DbaseFieldType.Number:
                return new DbfNumericDoubleField(field.Name, field.Length.ToInt32(), field.DecimalCount.ToInt32());
        }

        throw new NotImplementedException($"Unknown field type: {field.FieldType}");
    }

    private static DbaseField ToDbaseField(this DbfField field)
    {
        switch (field.FieldType)
        {
            case DbfType.Character:
                return new DbaseField(new DbaseFieldName(field.Name), DbaseFieldType.Character, ByteOffset.Initial, new DbaseFieldLength(field.Length), new DbaseDecimalCount(field.NumericScale));
            case DbfType.Numeric:
                return new DbaseField(new DbaseFieldName(field.Name), DbaseFieldType.Number, ByteOffset.Initial, new DbaseFieldLength(field.Length), new DbaseDecimalCount(field.NumericScale));
        }

        throw new NotImplementedException($"Unknown field type: {field.FieldType}");
    }

    // Fills a record with the fields it is handed. Which of the fields in a file those are is the reader's to say:
    // see DbaseFields.NotRead.
    internal static void ReadValuesFrom(this DbaseRecord record, IEnumerable<DbfField> fields)
    {
        foreach (var field in fields)
        {
            record.Values.Single(x => x.Field.Name == field.Name).SetValue(field.Value);
        }
    }

    private static object? GetValue(this DbaseFieldValue dbaseFieldValue)
    {
        if (dbaseFieldValue is DbaseInt16 shortField)
        {
            return shortField.HasValue ? shortField.Value : 0;
        }

        if (dbaseFieldValue is DbaseNullableInt16 nullableShortField)
        {
            return nullableShortField.Value;
        }

        if (dbaseFieldValue is DbaseInt32 intField)
        {
            return intField.HasValue ? intField.Value : 0;
        }

        if (dbaseFieldValue is DbaseNullableInt32 nullableIntField)
        {
            return nullableIntField.Value;
        }

        if (dbaseFieldValue is DbaseString stringField)
        {
            return stringField.HasValue ? stringField.Value : null;
        }

        if (dbaseFieldValue is DbaseDateTime dateTimeField)
        {
            return dateTimeField.HasValue ? dateTimeField.Value.ToString(DateTimeFormat) : null;
        }

        if (dbaseFieldValue is DbaseDouble doubleField)
        {
            return doubleField.HasValue ? doubleField.Value : 0.0;
        }

        if (dbaseFieldValue is DbaseNullableDouble nullableDoubleField)
        {
            return nullableDoubleField.Value;
        }

        throw new NotImplementedException($"Unknown field type: {dbaseFieldValue.Field.FieldType}");
    }

    internal static void SetValue(this DbaseFieldValue dbaseFieldValue, object? value)
    {
        if (dbaseFieldValue is DbaseInt16 shortField)
        {
            if (value is not null)
            {
                shortField.Value = Convert.ToInt16(value);
            }

            return;
        }

        if (dbaseFieldValue is DbaseNullableInt16 nullableShortField)
        {
            nullableShortField.Value = value is not null
                ? Convert.ToInt16(value)
                : null;
            return;
        }

        if (dbaseFieldValue is DbaseInt32 intField)
        {
            if (value is not null)
            {
                intField.Value = Convert.ToInt32(value);
            }

            return;
        }

        if (dbaseFieldValue is DbaseNullableInt32 nullableIntField)
        {
            nullableIntField.Value = value is not null
                ? Convert.ToInt32(value)
                : null;
            return;
        }

        if (dbaseFieldValue is DbaseString stringField)
        {
            stringField.Value = (string)value;
            return;
        }

        if (dbaseFieldValue is DbaseDateTime dateTimeField)
        {
            // CREATIE and VERSIE are the only date fields the files carry, and nothing reads them back: an upload
            // is judged on its identifiers, its geometry and its attributes. A value that is not in the format
            // they are written in therefore says nothing about the record, and it is left behind rather than
            // refused - a throw here stops the reader on that record and leaves every record behind it unread.
            if (!string.IsNullOrWhiteSpace(value?.ToString())
                && DateTime.TryParseExact(value.ToString(), DateTimeFormat, null, DateTimeStyles.None, out var dateTimeValue))
            {
                dateTimeField.Value = dateTimeValue;
            }

            return;
        }

        if (dbaseFieldValue is DbaseDouble doubleField)
        {
            if (value is not null)
            {
                doubleField.Value = Convert.ToDouble(value);
            }

            return;
        }

        if (dbaseFieldValue is DbaseNullableDouble nullableDoubleField)
        {
            nullableDoubleField.Value = value is not null
                ? Convert.ToDouble(value)
                : null;
            return;
        }

        throw new NotImplementedException($"Unknown field type: {dbaseFieldValue.Field.FieldType}");
    }

    internal static IDbaseRecordEnumerator<TDbaseRecord> CreateDbaseRecordEnumerator<TDbaseRecord>(this DbfReader reader, IReadOnlySet<string> excludedFieldNames)
        where TDbaseRecord : DbaseRecord, new()
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(excludedFieldNames);
        return new DbfRecordEnumerator<TDbaseRecord>(reader, excludedFieldNames);
    }

    private sealed class DbfRecordEnumerator<TDbaseRecord> : IDbaseRecordEnumerator<TDbaseRecord>
        where TDbaseRecord : DbaseRecord, new()
    {
        private enum State
        {
            Initial,
            Started,
            Ended
        }

        private readonly DbfReader _reader;
        private readonly DbfField[] _fieldsToRead;
        private RecordNumber _number;
        private TDbaseRecord? _current;
        private State _state;

        public DbfRecordEnumerator(DbfReader reader, IReadOnlySet<string> excludedFieldNames)
        {
            _reader = reader.ThrowIfNull();
            _fieldsToRead = _reader.Fields.Where(field => !excludedFieldNames.Contains(field.Name)).ToArray();
            _current = null;
            _state = State.Initial;
            _number = RecordNumber.Initial;
        }

        public bool MoveNext()
        {
            if (_state == State.Ended)
            {
                return false;
            }

            if (_state == State.Initial)
            {
                _state = State.Started;
            }
            else
            {
                if (_reader.RecordCount == _number.ToInt32())
                {
                    _current = null;
                    _state = State.Ended;
                    return false;
                }

                _number = _number.Next();
            }

            try
            {
                if (!_reader.Read())
                {
                    _current = null;
                    _state = State.Ended;
                    return false;
                }

                var record = new TDbaseRecord();
                record.ReadValuesFrom(_fieldsToRead);

                _current = record;
            }
            catch (Exception)
            {
                _current = null;
                _state = State.Ended;
                throw;
            }

            return _state == State.Started;
        }

        public void Reset()
        {
            throw new NotSupportedException("Reset is not supported. Enumeration can only be performed once.");
        }

        object System.Collections.IEnumerator.Current => Current;

        public TDbaseRecord Current
        {
            get
            {
                if (_state == State.Initial)
                {
                    throw new InvalidOperationException("Enumeration has not started. Call MoveNext().");
                }

                if (_state == State.Ended)
                {
                    throw new InvalidOperationException("Enumeration has already ended. Reset is not supported.");
                }

                return _current;
            }
        }

        public RecordNumber CurrentRecordNumber => _number;

        public void Dispose()
        {
            _reader.Dispose();
        }
    }

    public static short ToDbaseShortValue(this bool value)
    {
        return value ? (short)1 : (short)0;
    }

    public static bool? ToBooleanFromDbaseValue(this int? value)
    {
        return value == 1 ? true : value == 0 ? false : null;
    }

    public static bool ToBooleanFromDbaseValue(this short value)
    {
        return value == 1
            ? true : value == 0
                ? false
                : throw new ArgumentException($"Invalid value: {value}");
    }
}
