namespace RoadRegistry.Tests.BackOffice.Uploads;

using System.Collections;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class ProblematicShapeRecordEnumerator : IEnumerator<ShapeRecord>
{
    private readonly int _failAt;
    private readonly Exception _failure;
    private readonly ShapeRecord[] _records;
    private int _index;

    public ProblematicShapeRecordEnumerator(ShapeRecord[] records, int failAt, Exception failure)
    {
        _records = records ?? throw new ArgumentNullException(nameof(records));
        _failAt = failAt;
        _failure = failure;
        _index = -1;
    }

    public bool MoveNext()
    {
        if (_index == _records.Length) return false;
        _index++;
        if (_index == _failAt) throw _failure;
        return _index != _records.Length;
    }

    public void Reset()
    {
        _index = -1;
    }

    public ShapeRecord Current
    {
        get
        {
            if (_index == -1) throw new Exception("The enumeration has not started. Call MoveNext().");
            if (_index == _records.Length) throw new Exception("The enumeration has ended. Call Reset().");
            return _records[_index];
        }
    }

    object IEnumerator.Current => Current;

    public void Dispose()
    {
    }
}
