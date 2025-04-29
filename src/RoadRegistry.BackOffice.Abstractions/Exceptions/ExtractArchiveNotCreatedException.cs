namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public class ExtractArchiveNotCreatedException : DownloadExtractException
{
    public ExtractArchiveNotCreatedException()
        : base("Extract archive was never created")
    {
    }
}
