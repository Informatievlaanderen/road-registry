namespace RoadRegistry.Api.BackOffice.Handlers.Exceptions;

public abstract class DownloadExtractException : ApplicationException
{
    public string Description { get; init; }
}
