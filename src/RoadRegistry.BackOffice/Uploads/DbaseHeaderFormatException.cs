namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    [Serializable]
    public class DbaseHeaderFormatException : DomainException
    {
        public DbaseHeaderFormatException(Exception innerException)
            : base("Error reading dbase header format", innerException)
        {
        }

        protected DbaseHeaderFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
