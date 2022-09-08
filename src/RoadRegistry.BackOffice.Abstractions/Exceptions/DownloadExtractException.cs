namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public abstract class DownloadExtractException : ApplicationException
{
    protected DownloadExtractException(string message) : base(message)
    {
    }

    public string Description { get; set; }
}
