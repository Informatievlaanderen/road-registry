namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class ExtractRequestNotFound : Error
{
    public ExtractRequestNotFound(DownloadId downloadId)
        : base(ProblemCode.Extract.NotFound,
            new ProblemParameter("DownloadId", downloadId))
    {
    }
}
