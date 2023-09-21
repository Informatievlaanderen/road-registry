namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Serializable]
    public abstract class DbaseReaderException : DomainException
    {
        protected DbaseReaderException(string message)
            : base(message)
        {
        }

        protected DbaseReaderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DbaseReaderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
