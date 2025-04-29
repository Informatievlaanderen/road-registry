namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

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
    }
}
