namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    [Serializable]
    public class DbaseReadRecordException : DbaseReaderException
    {
        public RecordNumber RecordNumber { get; }

        public DbaseReadRecordException(RecordNumber recordNumber, Exception innerException)
            : base($"Error reading record number {recordNumber}", innerException)
        {
            RecordNumber = recordNumber;
        }

        protected DbaseReadRecordException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
