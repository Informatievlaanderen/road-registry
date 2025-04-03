namespace RoadRegistry.BackOffice.ShapeFile;

using System;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Dbase;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri.Dbf;
using NetTopologySuite.IO.Esri.Dbf.Fields;
using NetTopologySuite.IO.Esri.Shapefiles.Readers;

public static class ShapefileExtensions
{
    public static IShapefileRecordEnumerator<TDbaseRecord> CreateShapefileRecordEnumerator<TDbaseRecord>(this ShapefileReader reader)
        where TDbaseRecord : DbaseRecord, new()
    {
        ArgumentNullException.ThrowIfNull(reader);
        return new ShapefileRecordEnumerator<TDbaseRecord>(reader);
    }

    private class ShapefileRecordEnumerator<TDbaseRecord> : IShapefileRecordEnumerator<TDbaseRecord>
        where TDbaseRecord : DbaseRecord, new()
        {
            private enum State { Initial, Started, Ended }

            private readonly ShapefileReader _reader;
            private RecordNumber _number;
            private (TDbaseRecord, Geometry) _current;
            private State _state;

            public ShapefileRecordEnumerator(ShapefileReader reader)
            {
                _reader = reader.ThrowIfNull();
                _current = default;
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
                        _current = default;
                        _state = State.Ended;
                        return false;
                    }

                    _number = _number.Next();
                }

                try
                {
                    if (!_reader.Read())
                    {
                        _current = default;
                        _state = State.Ended;
                        return false;
                    }

                    var record = new TDbaseRecord();
                    foreach (var field in _reader.Fields)
                    {
                        var dbaseFieldValue = record.Values.Single(x => x.Field.Name == field.Name);
                        dbaseFieldValue.SetValue(field.Value);
                    }

                    _current = (record, _reader.Geometry);
                }
                catch
                {
                    _current = default;
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

            public (TDbaseRecord, Geometry) Current
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
}
