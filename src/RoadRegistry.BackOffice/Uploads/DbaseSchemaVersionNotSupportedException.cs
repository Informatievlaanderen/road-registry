namespace RoadRegistry.BackOffice.Uploads;

public class DbaseSchemaVersionNotSupportedException : DbaseReaderException
{
    public DbaseSchemaVersionNotSupportedException(string dbaseSchemaVersion)
        : base($"No dbase reader available for schema '{dbaseSchemaVersion}'")
    {
    }
}
