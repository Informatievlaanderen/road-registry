namespace RoadRegistry.BackOffice.Uploads
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class DbaseSchemaMismatchException : DomainException
    {
        public string FileName { get; }
        public DbaseSchema ExpectedSchema { get; }
        public DbaseSchema ActualSchema { get; }

        public DbaseSchemaMismatchException(string fileName, DbaseSchema expectedSchema, DbaseSchema actualSchema)
        {
            FileName = fileName;
            ExpectedSchema = expectedSchema;
            ActualSchema = actualSchema;
        }

        protected DbaseSchemaMismatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
