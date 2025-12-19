namespace RoadRegistry.Extracts.Uploads
{
    using System;

    public class DbaseHeaderFormatException : DbaseReaderException
    {
        public DbaseHeaderFormatException(Exception innerException)
            : base("Error reading dbase header format", innerException)
        {
        }
    }
}
