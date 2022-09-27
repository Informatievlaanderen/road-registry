namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public class DownloadProductNotFoundException : ApplicationException
{
    public DownloadProductNotFoundException(string? message) : base(message)
    {
    }
}
