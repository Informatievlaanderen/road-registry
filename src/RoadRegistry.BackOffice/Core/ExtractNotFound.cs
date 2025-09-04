namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class ExtractNotFound : Error
{
    public ExtractNotFound(DownloadId downloadId)
        : base(ProblemCode.Extract.NotFound,
            new ProblemParameter("DownloadId", downloadId))
    {
    }
}
