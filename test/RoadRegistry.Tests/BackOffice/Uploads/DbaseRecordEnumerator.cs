namespace RoadRegistry.Tests.BackOffice.Uploads;

using System.Collections;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class DbaseRecordEnumerator<TDbaseRecord> : IDbaseRecordEnumerator<TDbaseRecord>
    where TDbaseRecord : DbaseRecord
{
    private readonly IEnumerator<TDbaseRecord> _enumerator;
    private RecordNumber _number;
    private State _state;

    public DbaseRecordEnumerator(IEnumerator<TDbaseRecord> enumerator)
    {
        _enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
        _number = RecordNumber.Initial;
        _state = State.Initial;
    }

    public TDbaseRecord Current => _enumerator.Current;

    object IEnumerator.Current => Current;

    public RecordNumber CurrentRecordNumber => _number;

    public void Dispose()
    {
        _enumerator.Dispose();
    }

    public bool MoveNext()
    {
        if (_state == State.Ended) return false;

        if (_state == State.Initial)
        {
            if (_enumerator.MoveNext())
            {
                _state = State.Started;
                return true;
            }

            _state = State.Ended;
            return false;
        }

        if (_enumerator.MoveNext())
        {
            _number = _number.Next();
            return true;
        }

        _state = State.Ended;
        return false;
    }

    public void Reset()
    {
        _enumerator.Reset();
    }

    private enum State
    {
        Initial,
        Started,
        Ended
    }
}