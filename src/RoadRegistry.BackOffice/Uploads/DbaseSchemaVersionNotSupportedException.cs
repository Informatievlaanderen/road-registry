namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class DbaseSchemaVersionNotSupportedException : DbaseReaderException
    {
        public DbaseSchemaVersionNotSupportedException(string dbaseSchemaVersion)
            : base($"No dbase reader available for schema '{dbaseSchemaVersion}'")
        {
        }

        protected DbaseSchemaVersionNotSupportedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
