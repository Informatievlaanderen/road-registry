namespace RoadRegistry.BackOffice.Dbase;

using System;
using System.Collections;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri.Dbf;
using NetTopologySuite.IO.Esri.Dbf.Fields;

public static class DbaseExtensions
{
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
            // case DbaseFieldType.Float:
            //     return new DbfFloatField(field.Name, field.Length.ToInt32(), field.DecimalCount.ToInt32());
            // case DbaseFieldType.Date:
            //     return new DbfDateField(field.Name, field.Length.ToInt32());
            // case DbaseFieldType.Logical:
            //     return new DbfLogicalField(field.Name);
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
            // case DbfType.Float:
            // case DbfType.Date:
            // case DbfType.Logical:
        }

        throw new NotImplementedException($"Unknown field type: {field.FieldType}");
    }

    private static object GetValue(this DbaseFieldValue dbaseFieldValue)
    {
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
            return dateTimeField.HasValue ? dateTimeField.Value : null;
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

    private static void SetValue(this DbaseFieldValue dbaseFieldValue, object value)
    {
        if (dbaseFieldValue is DbaseInt32 intField)
        {
            intField.Value = Convert.ToInt32(value);
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
            dateTimeField.Value = Convert.ToDateTime(value);
            return;
        }
        if (dbaseFieldValue is DbaseDouble doubleField)
        {
            doubleField.Value = Convert.ToDouble(value);
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

    public static IDbaseRecordEnumerator<TDbaseRecord> CreateDbaseRecordEnumerator<TDbaseRecord>(this DbfReader reader)
        where TDbaseRecord : DbaseRecord, new()
    {
        ArgumentNullException.ThrowIfNull(reader);
        return new DbfDbaseRecordEnumerator<TDbaseRecord>(reader);
    }

    private class DbfDbaseRecordEnumerator<TDbaseRecord> : IDbaseRecordEnumerator<TDbaseRecord>
        where TDbaseRecord : DbaseRecord, new()
        {
            private enum State { Initial, Started, Ended }

            private readonly DbfReader _reader;
            private RecordNumber _number;
            private TDbaseRecord _current;
            private State _state;

            public DbfDbaseRecordEnumerator(DbfReader reader)
            {
                _reader = reader.ThrowIfNull();
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
                    foreach (var field in _reader.Fields)
                    {
                        var dbaseFieldValue = record.Values.Single(x => x.Field.Name == field.Name);
                        dbaseFieldValue.SetValue(field.Value);
                    }

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

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _reader.Dispose();
            }
        }
}
