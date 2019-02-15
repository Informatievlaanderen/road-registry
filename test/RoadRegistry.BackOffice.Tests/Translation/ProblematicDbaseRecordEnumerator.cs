namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class ProblematicDbaseRecordEnumerator<TRecord> : IEnumerator<TRecord>
        where TRecord : DbaseRecord
    {
        private readonly TRecord[] _records;
        private readonly int _failAt;
        private readonly Exception _failure;
        private int _index;

        public ProblematicDbaseRecordEnumerator(TRecord[] records, int failAt, Exception failure)
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
            if (_index == _failAt)
            {
                throw _failure;
            }
            return _index != _records.Length;
        }

        public void Reset()
        {
            _index = -1;
        }

        public TRecord Current
        {
            get
            {
                if(_index == -1) throw new Exception("The enumeration has not started. Call MoveNext().");
                if(_index == _records.Length) throw new Exception("The enumeration has ended. Call Reset().");
                return _records[_index];
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
    
    public class ProblematicShapeRecordEnumerator<TRecord> : IEnumerator<TRecord>
        where TRecord : DbaseRecord
    {
        private readonly TRecord[] _records;
        private readonly int _failAt;
        private readonly Exception _failure;
        private int _index;

        public ProblematicShapeRecordEnumerator(TRecord[] records, int failAt, Exception failure)
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
            if (_index == _failAt)
            {
                throw _failure;
            }
            return _index != _records.Length;
        }

        public void Reset()
        {
            _index = -1;
        }

        public TRecord Current
        {
            get
            {
                if(_index == -1) throw new Exception("The enumeration has not started. Call MoveNext().");
                if(_index == _records.Length) throw new Exception("The enumeration has ended. Call Reset().");
                return _records[_index];
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}