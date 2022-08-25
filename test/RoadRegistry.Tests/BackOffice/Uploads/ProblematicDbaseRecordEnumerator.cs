namespace RoadRegistry.Tests.BackOffice.Uploads;

using System.Collections;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class ProblematicDbaseRecordEnumerator<TDbaseRecord> : IDbaseRecordEnumerator<TDbaseRecord>
    where TDbaseRecord : DbaseRecord
{
    private readonly int _failAt;
    private readonly Exception _failure;
    private readonly TDbaseRecord[] _records;
    private int _index;
    private RecordNumber _number;

    public ProblematicDbaseRecordEnumerator(TDbaseRecord[] records, int failAt, Exception failure)
    {
        _records = records ?? throw new ArgumentNullException(nameof(records));
        _failAt = failAt;
        _failure = failure;
        _index = -1;
        _number = RecordNumber.Initial;
    }

    public bool MoveNext()
    {
        if (_index == _records.Length) return false;
        _number = _index == -1
            ? RecordNumber.Initial
            : _number.Next();
        _index++;
        if (_index == _failAt) throw _failure;
        return _index != _records.Length;
    }

    public void Reset()
    {
        _index = -1;
    }

    public TDbaseRecord Current
    {
        get
        {
            if (_index == -1) throw new Exception("The enumeration has not started. Call MoveNext().");
            if (_index == _records.Length) throw new Exception("The enumeration has ended. Call Reset().");
            return _records[_index];
        }
    }

    public RecordNumber CurrentRecordNumber => _number;

    object IEnumerator.Current => Current;

    public void Dispose()
    {
    }
}
