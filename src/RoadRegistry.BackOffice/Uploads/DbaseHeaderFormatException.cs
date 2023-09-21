namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class DbaseHeaderFormatException : DbaseReaderException
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
