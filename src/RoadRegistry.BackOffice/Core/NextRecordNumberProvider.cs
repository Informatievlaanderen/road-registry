namespace RoadRegistry.BackOffice.Core
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class NextRecordNumberProvider
    {
        private RecordNumber _nextValue;

        public NextRecordNumberProvider(RecordNumber initialValue)
        {
            _nextValue = initialValue;
        }

        public RecordNumber Next()
        {
            var result = _nextValue;
            _nextValue = _nextValue.Next();
            return result;
        }
    }
}
