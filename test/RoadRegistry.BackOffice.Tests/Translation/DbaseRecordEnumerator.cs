namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class DbaseRecordEnumerator<TDbaseRecord> : IDbaseRecordEnumerator<TDbaseRecord>
        where TDbaseRecord : DbaseRecord
    {
        private enum State { Initial, Started, Ended }
        private readonly IEnumerator<TDbaseRecord> _enumerator;
        private RecordNumber _number;
        private State _state;

        public DbaseRecordEnumerator(IEnumerator<TDbaseRecord> enumerator)
        {
            _enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
            _number = RecordNumber.Initial;
            _state = State.Initial;
        }

        public bool MoveNext()
        {
            if (_state == State.Ended)
            {
                return false;
            }
            
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

        public RecordNumber CurrentRecordNumber => _number;

        public void Reset()
        {
            _enumerator.Reset();
        }

        public TDbaseRecord Current => _enumerator.Current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _enumerator.Dispose();
        }
    }
}