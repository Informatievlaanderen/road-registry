namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

public sealed class DownloadProductNotFoundException : Exception
{
    public DownloadProductNotFoundException()
        : base(string.Empty)
    {
    }
}
